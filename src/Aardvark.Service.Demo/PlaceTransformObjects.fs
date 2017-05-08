﻿namespace PlaceTransformObjects


module App =

    open Aardvark.Base
    open Aardvark.Base.Incremental
    
    open DragNDrop
    open Aardvark.SceneGraph
    open Aardvark.Base.Rendering
    open Aardvark.UI
    open Aardvark.Base.Geometry
    open PlaceTransformObjects

    type Action = 
        | MovePlane of V3d   
        | PlaceBox 
        | Select of string
        | Translate of string * TranslateController.ControllerAction
        | CameraMessage of CameraController.Message
        | Unselect
        | Nop


    let update (m : Scene) (a : Action) =
        match a with
            | CameraMessage a -> 
                //let isGrabbed =
                //    m.world.selectedObjects |> HSet.exists (fun name -> 
                //        match HMap.tryFind name m.world.objects with
                //            | Some o -> o.transformation.grabbed.IsSome
                //            | None -> false
                //    )

                //if isGrabbed then m 
                //else
                { m with camera = CameraController.update m.camera a }
            | MovePlane t -> m
            | PlaceBox -> 
                let name = System.Guid.NewGuid() |> string
                let newObject = { name = name; objectType = ObjectType.Box; transformation = TranslateController.initial }
                let world = { m.world with objects = HMap.add name newObject m.world.objects }
                { m with world = world }
            | Select n -> 
                let world = { m.world with selectedObjects = HSet.add n m.world.selectedObjects }
                { m with world = world }
            | Translate(name,a) -> 
                let lens = Scene.Lens.world |. World.Lens.objects |. HMap.Lens.item name |? Unchecked.defaultof<_> |. Object.Lens.transformation
                lens.Update(m, fun t -> TranslateController.updateController t a)
            | Unselect -> 
                { m with world = { m.world with selectedObjects = HSet.empty } } 
            | Nop -> m


    let viewScene (m : MScene) =


        let plane = 
            Sg.box' C4b.White (Box3d.FromCenterAndSize(V3d.OOO,V3d(10.0,10.0,-0.1)))
            |> Sg.requirePicking
            |> Sg.noEvents
            //|> Sg.withEvents [
            //        Sg.onMouseMove MovePlane
            //   ]

        let objects =
            aset {
                for (name,obj) in m.world.objects |> AMap.toASet do
                    let selected = ASet.contains name m.world.selectedObjects
                    let color = selected |> Mod.map (function | true -> C4b.Red | false -> C4b.Gray)
                    let controller =
                        TranslateController.viewController (fun t -> Translate(obj.name |> Mod.force, t)) obj.transformation 
                        |> Sg.onOff selected 
                    yield 
                        Sg.box color (Box3d.FromCenterAndSize(V3d.OOO,V3d.III*0.5) |> Mod.constant) 
                        |> Sg.requirePicking 
                        |> Sg.noEvents
                        |> Sg.Incremental.withEvents (
                            amap {
                                let! selected = selected
                                if not selected then
                                    yield Sg.onMouseDown (fun _ _ -> Select name)
                            } )
                        |> Sg.trafo obj.transformation.trafo 
                        |> Sg.andAlso controller
            } |> Sg.set

        Sg.ofSeq [plane; objects; ]//selectedObj]
        |> Sg.effect [
                toEffect <| DefaultSurfaces.trafo
                toEffect <| DefaultSurfaces.vertexColor
                toEffect <| DefaultSurfaces.simpleLighting
            ]


    let view (m : MScene) =
        require (Html.semui) (
            div [clazz "ui"; style "background: #1B1C1E"] [
                CameraController.controlledControl m.camera CameraMessage (Frustum.perspective 60.0 0.1 100.0 1.0 |> Mod.constant) 
                    (AttributeMap.ofList [
                        yield attribute "style" "width:85%; height: 100%; float: left;"
                        //yield! TranslateController.controlSubscriptions (fun a -> 
                        //    match  m.world.selectedObjects |> ASet.toList with
                        //        | [] -> Nop
                        //        | n::_ -> Translate(n,a)
                        //    )
                    ]) (viewScene m)

                div [style "width:15%; height: 100%; float:right"] [
                    Html.SemUi.stuffStack [
                        button [clazz "ui button"; onClick (fun _ ->  PlaceBox )] [text "Add Box"]
                        button [clazz "ui button"; onClick (fun _ ->  Unselect )] [text "Unselect"]
                    ]
                ]
            ]
        )

    let app =
        {
            unpersist = Unpersist.instance
            threads = fun (model : Scene) -> CameraController.threads model.camera |> ThreadPool.map CameraMessage
            initial = { world = { objects = HMap.empty; selectedObjects = HSet.empty }; camera = CameraController.initial; }
            update = update
            view = view
        }

    let start() = App.start app