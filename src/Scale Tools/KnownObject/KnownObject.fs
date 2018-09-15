namespace KnownObject

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.UI
open Aardvark.UI.Trafos
open Aardvark.UI.Primitives

module KnownObject =
    
    type Action =
        | SetPos of V3d
        | SetTyp of KnownObjectType
        | ToggleShowTrafoCtrl
        | ChangeTrafoKind of TrafoKind
        | TrafoMessage of TrafoController.Action
    
    let trafoGrabbed (m : Model) = m.trafo.grabbed.IsSome

    let rec update (m : Model) (act : Action) =
        match act with
        | SetPos pos ->
            let rotTrafo = Pose.toRotTrafo m.trafo.pose
            let rotation = Rot3d.FromM33d(rotTrafo.Forward.UpperLeftM33())
            let pose = {Pose.translate pos with rotation = rotation}
            let prevTraf = Pose.toTrafo pose
            let trafo = {m.trafo with pose = pose; previewTrafo = prevTraf}
            {m with trafo = trafo}
        
        | SetTyp typ ->
            let ctrOffset = typ |> Loader.getOffset
            {m with typ = typ; ctrOffset = ctrOffset}
        
        | ToggleShowTrafoCtrl -> {m with showTrafo = not m.showTrafo}

        | ChangeTrafoKind kind -> {m with trafoKind = kind}

        | TrafoMessage msg ->
            let trafo =
                match m.trafoKind with
                | TrafoKind.Rotate ->
                    RotationController.updateController m.trafo msg
                | TrafoKind.Translate ->
                    TranslateController.updateController m.trafo msg
                | _ -> m.trafo
            {m with trafo = trafo}
    
    let mkTrafoSg (m : MModel) (view : IMod<CameraView>) (liftMessage : Action -> 'msg) =
        m.showTrafo
        |> Mod.map2 ( fun k x ->
            if x
            then
                match k with
                | TrafoKind.Rotate ->
                    RotationController.viewController (fun x -> TrafoMessage x |> liftMessage) view m.trafo
                | _ -> TranslateController.viewController (fun x -> TrafoMessage x |> liftMessage) view m.trafo
            else Sg.empty
        ) m.trafoKind
        |> Sg.dynamic
        |> Sg.depthTest (Mod.constant DepthTestMode.Always)
        |> Sg.pass (RenderPass.after "controls" RenderPassOrder.Arbitrary RenderPass.main)
        |> Sg.trafo (
                Mod.map2 ( fun (offs : float) (v : CameraView) ->
                    Trafo3d.Translation(v.Sky.Normalized * offs)
                ) m.ctrOffset view
            )
    
    let viewScene' (m : MModel) (view : IMod<CameraView>) pickSg modelEvents (liftMessage : Action -> 'msg) =
        let sg =
            Sg.mkKnownObjectSg m view
            |> Sg.trafo (
                Mod.map2 ( fun (offs : float) (v : CameraView) ->
                    Trafo3d.Translation(v.Sky.Normalized * offs)
                ) m.ctrOffset view
            )
            |> Sg.withEvents modelEvents
        
        let events = [Sg.onDoubleClick ( fun v -> SetPos v |> liftMessage )]
        [sg; pickSg events]
        |> Sg.ofList
    
    let view' (m : MModel) =
        Html.table [
            Html.row "Known-Object-Type: " [
                Html.SemUi.dropDown m.typ SetTyp
            ]
            Html.row "Trafocontrols: " [
                div [clazz "ui"][
                    Utils.Html.toggleButton m.showTrafo "Show" "Hide" ToggleShowTrafoCtrl
                    Html.SemUi.dropDown m.trafoKind ChangeTrafoKind
                ]
            ]
        ]
    
    let initial sky =
        let pos = V3d(-20000.0, -20000.0, -20000.0)
        let rotation = Rot3d.FromM33d(Trafo3d.RotateInto(V3d.OOI, sky).Forward.UpperLeftM33())
        let pose = {Pose.translate pos with rotation = rotation}
        let previewTrafo = Pose.toTrafo pose
        {
            trafo     = {TrafoController.initial with mode = TrafoMode.Local; pose = pose; previewTrafo = previewTrafo}
            trafoKind = TrafoKind.Rotate
            showTrafo = false
            typ       = KnownObjectType.UnitCube
            ctrOffset = 0.5
            selected  = true
            id        = System.Guid.NewGuid().ToString()
        }