namespace Boxes

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.Rendering
open Aardvark.Rendering.Text
open Aardvark.UI
open Aardvark.UI.Trafos
open Aardvark.UI.Primitives

module Labels =

    let initial =
        {
            box   = Box.initial V3d.OOO V3d.OOI
            view  = CameraView.lookAt (V3d(5.0, 5.0, 5.0)) V3d.OOO V3d.OOI
            xEdge = None
            yEdge = None
            zEdge = None
        }
    
    let setup (box : BoxModel) (view : CameraView) =
        let trafo = box.trafo * Trafo3d.Translation(box.center)
        let (x,y,z) = LabelEdges.calcLabelEdges box.geom trafo view
        {
            box   = box
            view  = view
            xEdge = x
            yEdge = y
            zEdge = z
        }
    
    let labelsSg (m : MLabelsModel) (view : IMod<CameraView>) =
        let mkOneLabel (str : string) (e : Line3d) =
            let pos  = (e.P0 + e.P1) * 0.5
            let trafo =
                Mod.map2 ( fun (v : CameraView) (c : V3d) ->
                    let vp   = v.Location
                    let tPos = M44d.Translation(c).TransformPos(pos)
                    let d    = V3d.Distance(vp, tPos)
                    let s    = 0.018 * d
                    let sMin = 0.01
                    let s    = if s < sMin then sMin else s
                    Trafo3d.Scale(s) * Trafo3d.Translation(pos)
                ) view m.box.center
            
            Sg.textWithBackground (Font.create "arial" FontStyle.Regular) C4b.White C4b.DarkBlue Border2d.None (Mod.constant str)
            |> Sg.billboard
            |> Sg.noEvents
            |> Sg.trafo ( m.box.trafo |> Mod.map (fun x -> x.Inverse) )
            |> Sg.trafo trafo
        
        let xEdge =
            m.xEdge
            |> Mod.map ( fun x ->
                match x with
                | None -> Sg.empty
                | Some e ->
                    let str = sprintf "%.2f m" (V3d.Distance(e.P0, e.P1))
                    mkOneLabel str e
            )
            |> Sg.dynamic
        
        let yEdge =
            m.yEdge
            |> Mod.map ( fun y ->
                match y with
                | None -> Sg.empty
                | Some e ->
                    let str = sprintf "%.2f m" (V3d.Distance(e.P0, e.P1))
                    mkOneLabel str e
            )
            |> Sg.dynamic
        
        let zEdge =
            m.zEdge
            |> Mod.map ( fun z ->
                match z with
                | None -> Sg.empty
                | Some e ->
                    let str = sprintf "%.2f m" (V3d.Distance(e.P0, e.P1))
                    mkOneLabel str e
            )
            |> Sg.dynamic
        
        [xEdge; yEdge; zEdge]
        |> Sg.ofList
        |> Sg.trafo m.box.trafo
        |> Sg.trafo ( m.box.center |> Mod.map ( fun c -> Trafo3d.Translation(c) ) )
        |> Sg.depthTest (Mod.constant DepthTestMode.Always)