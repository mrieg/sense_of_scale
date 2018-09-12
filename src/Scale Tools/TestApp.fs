namespace TestApp

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.Rendering.Text
open Aardvark.UI
open Aardvark.UI.Primitives

module App =

    type Action =
        | CameraMessage of CameraController.Message
    
    let update (m : Model) (act : Action) =
        match act with
        | CameraMessage msg ->
            let camera = CameraController.update m.camera msg
            //printfn "LOC Z: %A" camera.view.Location.Z
            {m with camera = camera}
    
    let viewScene (m : MModel) =
        Sg.unitSphere' 7 C4b.White
        |> Sg.noEvents
        |> Sg.shader {
            do! DefaultSurfaces.trafo
            do! DefaultSurfaces.vertexColor
            do! DefaultSurfaces.simpleLighting
        }
    
    let viewScene' (m : MModel) =
        //HACK for buggy Sg.text (breaks when number of chars increases, i.e. when value goes from 9.99 to 10.00)
        let content = m.camera.view |> Mod.map ( fun v ->
            let str = sprintf "Z: %.2f" v.Location.Z
            let len = 15
            let dif = len - str.Length
            let pad =
                let mutable p = ""
                for i in 1..dif do
                    p <- p + "P" //problem: space not workin in Sg.text
                p
            let str = str + pad
            printfn "-------- %A" str
            str
        )

        let font = Font.create "arial" FontStyle.Regular
        let col = C4b.White

        Sg.text font col content
        |> Sg.noEvents
        |> Sg.andAlso (viewScene m)
    
    let view (m : MModel) =
        let frustum = Frustum.perspective 60.0 0.1 10000.0 1.0 |> Mod.constant
        require (Html.semui) (
            div [clazz "ui"][
                CameraController.controlledControl m.camera CameraMessage frustum
                    (AttributeMap.ofList[
                        style "width:100%; height:80%;"
                    ]) (viewScene' m)
                
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