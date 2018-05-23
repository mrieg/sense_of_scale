namespace Boxes

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.UI

module AddBoxFromPoints =
    
    type Action =
        | AddPoint    of V3d * string
        | RemovePoint of string
    
    let minSize = 0.025
    let defaultSize = 0.01

    let createBox (m : AddBoxFromPointsModel) =
        let pts = m.ptList |> PList.map ( fun x -> x.pos ) |> PList.toList
        let (c, s, r) = PCA.fitBox pts
        let size =
            if s.X < minSize
            then V3d(defaultSize, s.Y, s.Z)
            else s
        
        let size =
            if size.Y < minSize
            then V3d(size.X, defaultSize, size.Z)
            else size
        
        let size =
            if size.Z < minSize
            then V3d(size.X, size.Y, defaultSize)
            else size
        
        Box.setup c size r "Box"
    
    let createBox3pts (m : AddBoxFromPointsModel) =
        let pts = m.ptList |> PList.map ( fun s -> s.pos ) |> PList.toList
        let p0 = pts.[0]
        let p1 = pts.[1]
        let p2 = pts.[2]
        
        let v0 = (p1 - p0).Normalized
        let v1 = (p2 - p1).Normalized
        let n  = (V3d.Cross(v0,v1)).Normalized
        let r  = (V3d.Cross(n,v0)).Normalized
        
        let rot = (Rot3d.FromFrame(v0, r, n)).GetEulerAngles()
        let rotation = Trafo3d.Rotation(rot)
        let invRotPts = pts |> List.map ( fun v -> rotation.Inverse.Forward.TransformPos(v) )

        let box = (Box3d.FromPoints(invRotPts.[0], invRotPts.[1])).ExtendedBy(invRotPts.[2])
        let s = box.Size
        
        let size =
            if s.X < minSize
            then V3d(defaultSize, s.Y, s.Z)
            else s
        
        let size =
            if size.Y < minSize
            then V3d(size.X, defaultSize, size.Z)
            else size
        
        let size =
            if size.Z < minSize
            then V3d(size.X, size.Y, defaultSize)
            else size
        
        let center = rotation.Forward.TransformPos(box.Center)
        Box.setup center size rot "Box"
    
    let createBoxAA (m : AddBoxFromPointsModel) =
        let ptList = m.ptList |> PList.map ( fun x -> x.pos ) |> PList.toArray
        let b = Box3d(ptList)
        let center = b.Center
        let size = b.Size
        let rotation = V3d.OOO
        Box.setup center size rotation "Box"

    let setValues (m : AddBoxFromPointsModel) =
        let box =
            match m.ptList.Count with
            | 0 | 1 | 2 ->
                let center = -V3d.III * 100000.0
                {Box.initial center V3d.OOI with center = center}
            | 3 -> {createBox3pts m with color = C4b(92, 92, 92, 120); options = {RenderOptions.initial with blending = true}}
            | _ -> {createBox m with color = C4b(92, 92, 92, 120); options = {RenderOptions.initial with blending = true}}
            
        {m with box = box}
    
    let update (m : AddBoxFromPointsModel) (act : Action) =
        match act with
        | AddPoint (v,id) ->
            let ptList = PList.append {pos = v; id = id} m.ptList
            setValues {m with ptList = ptList}
        
        | RemovePoint id  ->
            let removeIdx = m.ptList |> PList.toList |> List.findIndex ( fun x -> x.id = id )
            let ptList = m.ptList |> PList.removeAt removeIdx
            setValues {m with ptList = ptList}
    
    let drawPoints (m : MAddBoxFromPointsModel) (view : IMod<CameraView>) (liftMessage : Action -> 'msg) =
        aset {
            for s in m.ptList |> AList.toASet do
                let trans = s.pos |> Mod.map Trafo3d.Translation

                let scale =
                    Mod.map2 ( fun (v : CameraView) (p : V3d) ->
                        let d = V3d.Distance(v.Location, p)
                        let s = 0.008 * d
                        Trafo3d.Scale(s)
                    ) view s.pos
                
                let sg =
                    Sg.sphere' 4 C4b.Green 1.0
                    |> Sg.requirePicking
                    |> Sg.noEvents
                    |> Sg.trafo scale
                    |> Sg.withEvents [
                        Sg.onDoubleClick (fun _ -> RemovePoint s.id |> liftMessage)
                    ]
                    |> Sg.trafo trans
                
                yield sg
        }
        |> Sg.set
        |> Sg.andAlso (Box.mkISg m.box [])
        |> Sg.shader {
            do! DefaultSurfaces.trafo
            do! DefaultSurfaces.vertexColor
        }
    
    let initial =
        let center = -V3d.III * 100000.0
        {
            ptList = PList.empty
            box    = {Box.initial center V3d.OOI with center = center}
        }