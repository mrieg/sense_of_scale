namespace TestApp

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.UI
open Aardvark.UI.Primitives

module App =

    type Action =
        | CameraMessage of CameraController.Message
    
    let update (m : Model) (act : Action) =
        match act with
        | CameraMessage msg -> {m with camera = CameraController.update m.camera msg}
    
    let viewScene (m : MModel) =
        Sg.unitSphere' 7 C4b.White
        |> Sg.noEvents
        |> Sg.shader {
            do! DefaultSurfaces.trafo
            do! DefaultSurfaces.vertexColor
            do! DefaultSurfaces.simpleLighting
        }
    
    let view (m : MModel) =
        let frustum = Frustum.perspective 60.0 0.1 100.0 1.0 |> Mod.constant
        require (Html.semui) (
            div [clazz "ui"][
                CameraController.controlledControl m.camera CameraMessage frustum
                    (AttributeMap.ofList[
                        style "width:100%; height:80%;"
                    ]) (viewScene m)
                
                h1 [][text "TestApp"]
            ]
        )
    
    let initial =
        {
            camera = CameraController.initial
        }

    let threads (m : Model) =
        CameraController.threads m.camera |> ThreadPool.map CameraMessage
    
    let app =
        {
            unpersist = Unpersist.instance
            threads   = threads
            initial   = initial
            update    = update
            view      = view
        }
    
    let start() = App.start app