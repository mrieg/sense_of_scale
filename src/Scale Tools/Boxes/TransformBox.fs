namespace Boxes

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.Rendering
open Aardvark.UI
open Aardvark.UI.Trafos
open Aardvark.UI.Primitives

module TransformBox =
    
    type Action =
        | TranslateBox  of TrafoController.Action
        | RotateBox     of TrafoController.Action
    
    let grabbed (m : TransformBoxModel) =
        m.trafo.grabbed.IsSome
    
    let update (m : TransformBoxModel) (act : Action) =
        match act with
        | TranslateBox msg ->
            match msg with
            | TrafoController.Action.MoveRay rp ->
                let trafo = TranslateController.updateController m.trafo msg
                let pos = trafo.pose.position + trafo.workingPose.position
                let pos =
                    match m.box.trafoMode with
                    | TrafoMode.Local ->
                        let tm = M44d.Translation(trafo.pose.position) * (Pose.toRotTrafo trafo.pose).Forward * M44d.Translation(-trafo.pose.position)
                        tm.TransformPos(pos)
                    | _ -> pos
                
                let box = Box.update m.box (Box.Action.UpdateCenter pos)
                {m with trafo = trafo; box = box}
            | TrafoController.Action.Release ->
                {m with trafo = TranslateController.updateController m.trafo msg}
            | _ ->
                {m with trafo = TranslateController.updateController m.trafo msg}
        
        | RotateBox msg ->
            match msg with
            | TrafoController.Action.MoveRay _ | TrafoController.Action.Release ->
                let trafo = RotationController.updateController m.trafo msg
                let rotTrafo =
                    match m.box.trafoMode with
                    | TrafoMode.Local -> (Pose.toRotTrafo trafo.workingPose) * (Pose.toRotTrafo trafo.pose)
                    | _ -> (Pose.toRotTrafo trafo.pose) * (Pose.toRotTrafo trafo.workingPose)
                let box = Box.update m.box (Box.Action.RotateWithTrafo rotTrafo)
                {m with trafo = trafo; box = box}
            | _ ->
                {m with trafo = RotationController.updateController m.trafo msg}
    
    let mkControllerISg (m : MTransformBoxModel) (camView : IMod<CameraView>) (liftMessage : Action -> 'msg) =
        m.trafoKind
        |> Mod.map ( fun x ->
            match x with
            | TrafoKind.Rotate -> RotationController.viewController ( fun x -> RotateBox x |> liftMessage ) camView m.trafo
            | _ -> TranslateController.viewController ( fun x -> TranslateBox x |> liftMessage ) camView m.trafo
        )
        |> Sg.dynamic
        |> Sg.depthTest (Mod.constant DepthTestMode.Always)
        |> Sg.pass (RenderPass.after "controls" RenderPassOrder.Arbitrary RenderPass.main)
    
    let initial (trafoKind : TrafoKind) =
        {
            box         = Box.initial V3d.OOO V3d.OOI
            trafo       = TrafoController.initial
            trafoKind   = trafoKind
        }
    
    let setup (box : BoxModel) (trafoKind : TrafoKind) (trafoMode : TrafoMode) =
        let trafo =
            let rotation = Rot3d.FromM33d( box.trafo.Forward.UpperLeftM33() )
            let pose =
                {
                    Pose.translate box.center with
                        rotation = rotation
                }
            {
                TrafoController.initial with
                    pose            = pose
                    previewTrafo    = Pose.trafo pose
                    mode            = box.trafoMode
            }
        
        {
            box         = box
            trafo       = trafo
            trafoKind   = trafoKind
        }