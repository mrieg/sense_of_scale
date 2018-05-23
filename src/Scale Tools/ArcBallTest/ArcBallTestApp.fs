namespace ArcBallTest

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.Rendering
open Aardvark.UI
open Aardvark.UI.Trafos
open Aardvark.UI.Primitives

module App =
    
    type Action =
        | CameraMessage  of CameraController.Message
        | ArcBallMessage of ArcBallController.Message
        | Entered        of V3d
    
    let update (m : Model) (act : Action) =
        match act with
        | CameraMessage msg  -> {m with camera = CameraController.update m.camera msg}
        | ArcBallMessage msg ->
            match msg with
            | ArcBallController.Message.Pick p -> printfn "p"; {m with camera = ArcBallController.update m.camera msg}
            | _ -> {m with camera = ArcBallController.update m.camera msg}
        | Entered v          -> printfn "DoubleClicked: %A" v; m
    
    let viewScene (m : MModel) =
        Sg.box' C4b.White (Box3d(-V3d.III, V3d.III))
        |> Sg.requirePicking
        |> Sg.noEvents
        |> Sg.withEvents [
            Sg.onEnter ( fun v -> Entered v )
        ]
        |> Sg.andAlso (
            Sg.box' C4b.Red (Box3d(-V3d.III, V3d.III))
            |> Sg.requirePicking
            |> Sg.noEvents
            |> Sg.translate -3.0 -4.0 2.0
            |> Sg.withEvents [
                Sg.onEnter ( fun v -> Entered v )
            ]
        )
        |> Sg.shader {
            do! DefaultSurfaces.trafo
            do! DefaultSurfaces.vertexColor
            do! DefaultSurfaces.simpleLighting
        }

    let view (m : MModel) =

        let frustum = Mod.constant (Frustum.perspective 60.0 0.1 100.0 1.0)
        let renderControlAttributes = ArcBallController.extractAttributes m.camera ArcBallMessage frustum |> AttributeMap.ofAMap
        
        require Html.semui (
            div [clazz "ui"][
                yield
                    Incremental.renderControl (Mod.map2 Camera.create m.camera.view frustum)
                        (AttributeMap.unionMany [
                            renderControlAttributes
                            AttributeMap.ofList [
                                attribute "style" "width:70%; height: 100%; float: left;"
                            ]
                        ]) (viewScene m)
            ]
        )
    
    let initial =
        {
            camera =
                {
                    CameraController.initial with
                        orbitCenter = Some V3d.III
                        view = CameraView.Look(V3d(12.0, 12.0, 4.0), V3d(0.0, -1.0, 0.0), V3d(0.0, 0.5, 0.5))
                }
        }
    
    let threads (m : Model) =
        CameraController.threads m.camera |> ThreadPool.map CameraMessage
    
    let app =
        {
            unpersist = Unpersist.instance
            threads = threads
            initial = initial
            update = update
            view = view
        }
    
    let start() = App.start app