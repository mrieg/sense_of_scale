namespace Boxes

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.Rendering
open Aardvark.UI
open Aardvark.UI.Trafos
open Aardvark.UI.Primitives

module TransformFace =
    
    let initial =
        {
            box             = Box.initial V3d.OOO V3d.OOI
            selectedFace    = None
            trafo           = TrafoController.initial
        }
    
    let setup (box : BoxModel) =
        {
            box             = box
            selectedFace    = None
            trafo           = TrafoController.initial
        }

    type Action =
        | SelectFace    of FaceType
        | TranslateFace of TrafoController.Action * FaceType
    
    let grabbed (m : TransformFaceModel) =
        m.trafo.grabbed.IsSome
    
    let update (m : TransformFaceModel) (act : Action) =
        match act with
        | SelectFace ft -> {setup m.box with selectedFace = Some ft}
        | TranslateFace (msg, ft) ->
            let trafo = TranslateController.updateController m.trafo msg
            match msg with
            | TrafoController.Action.MoveRay rp ->
                let faces = Faces.boxFaces m.box.center m.box.size
                let face = faces.[Faces.faceIndex ft]
                let diff = face.center - m.box.center
                let faceTrafo = Trafo3d.Translation(diff)
                let pos = faceTrafo.Forward.TransformPos(trafo.pose.position + trafo.workingPose.position)
                let box = Box.update m.box (Box.Action.EnlargeBox (pos, ft))
                {m with trafo = trafo; box = box}
            | TrafoController.Action.Release ->
                let newM = setup m.box
                {newM with selectedFace = m.selectedFace}
            | _ -> {m with trafo = trafo}

    let mkControllerISg (m : MTransformFaceModel) (camView : IMod<CameraView>) (liftMessage : Action -> 'msg) =
        let centerTrafo = m.box.center |> Mod.map ( fun c -> Trafo3d.Translation(c) )
        let invCenterTrafo = m.box.center |> Mod.map ( fun c -> Trafo3d.Translation(-c) )

        let pickGraph =
            m.trafo.grabbed
            |> Mod.bind ( fun x ->
                match x with
                | Some _ -> Mod.constant Sg.empty
                | None ->
                    Mod.map2 ( fun c s ->
                        let l = Faces.boxFaces c s
                        let sg =
                            [
                                for i in 0..5 do
                                    let f = l.[i]
                                    let pick =
                                        let pickBoxSize = s * ( V3d( Fun.Abs(f.normal.X), Fun.Abs(f.normal.Y), Fun.Abs(f.normal.Z) ) - V3d.III) * -1.0 - s * 0.1
                                        let pickBoxSize = V3d(Fun.Abs(pickBoxSize.X), Fun.Abs(pickBoxSize.Y), Fun.Abs(pickBoxSize.Z))
                                        Sg.pickable (PickShape.Box ( Box3d.FromCenterAndSize(f.center, pickBoxSize) )) Sg.empty
                                        |> Sg.requirePicking
                                        |> Sg.noEvents
                                        |> Sg.withEvents [
                                            Sg.onDoubleClick ( fun _ -> SelectFace f.faceType |> liftMessage )
                                        ]
                                    yield pick
                            ]
                
                        sg |> Sg.ofList
                     ) m.box.center m.box.size
            )
            |> Sg.dynamic
            |> Sg.trafo invCenterTrafo
        
        let faceTrafo =
            Mod.bind2 ( fun c s ->
                m.selectedFace
                |> Mod.bind ( fun sel ->
                    match sel with
                    | None -> Mod.constant Trafo3d.Identity
                    | Some ft ->
                        let faces = Faces.boxFaces c s
                        let face = faces.[Faces.faceIndex ft]
                        let diff = face.center - c
                        Mod.constant (Trafo3d.Translation(diff))
                )
            ) m.box.center m.box.size

        let faceCtrls =
            m.selectedFace
            |> Mod.map ( fun sel ->
                match sel with
                | None -> Sg.empty
                | Some ft ->
                    TranslateFaceTrafoCtrl.viewController ( fun x -> TranslateFace (x, ft) |> liftMessage ) camView m.trafo ft
            )
            |> Sg.dynamic
            |> Sg.trafo faceTrafo

        [faceCtrls; pickGraph]
        |> Sg.ofList
        |> Sg.trafo m.box.trafo
        |> Sg.trafo centerTrafo
        |> Sg.depthTest (Mod.constant DepthTestMode.Always)
        |> Sg.pass (RenderPass.after "controls" RenderPassOrder.Arbitrary RenderPass.main)
    
    let view' (m : MTransformFaceModel) =
        Incremental.div
            (AttributeMap.ofList[]) (
                alist {
                    yield h1 [][text "Faces"]
                }
            )