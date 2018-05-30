namespace ScaleTools

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.Rendering
open Aardvark.UI
open Aardvark.UI.Primitives

module App =
    
    type Action =
        | CameraMessage      of CameraController.Message
        | BoxesMessage       of Boxes.App.Action
        | ShadingMessage     of DistanceShading.App.Action
        | BarsMessage        of ScaleBar.MultiApp.Action
        | ContourMessage     of ContourLines.App.Action
        | KnownObjectMessage of KnownObject.Multi.Action
        | VertMessage        of VerticalExaggeration.App.Action
        | PlaneExMessage     of PlaneExtrude.App.Action
        | EllipseMessage     of EllipseShading.App.Action
        | ChangeAppType      of AppType
        | OnKeyDownMsg       of Aardvark.Application.Keys
        | ToggleOrientationCube
        | ToggleShading
    
    let cameraShouldUpdate (m : Model) =
        (Boxes.App.cameraShouldUpdate m.boxesApp) && not (KnownObject.Multi.trafoGrabbed m.objectsApp) && not (EllipseShading.App.trafoGrabbed m.ellipseApp)
    
    let update (m : Model) (act : Action) =
        match act with
        | CameraMessage  msg ->
            match cameraShouldUpdate m with
            | false -> m
            | true  ->
                let boxesApp = Boxes.App.update m.boxesApp (Boxes.App.CameraMessage msg)
                {m with camera = CameraController.update m.camera msg; boxesApp = boxesApp}
        
        | BoxesMessage       msg -> {m with boxesApp   = Boxes.App.update m.boxesApp msg}
        | ShadingMessage     msg -> {m with shadingApp = DistanceShading.App.update m.shadingApp msg}
        | BarsMessage        msg -> {m with barsApp    = ScaleBar.MultiApp.update m.barsApp msg}
        | ContourMessage     msg -> {m with contourApp = ContourLines.App.update m.contourApp msg}
        | KnownObjectMessage msg -> {m with objectsApp = KnownObject.Multi.update m.objectsApp msg}
        | VertMessage        msg -> {m with vertApp    = VerticalExaggeration.App.update m.vertApp msg}
        | PlaneExMessage     msg -> {m with planeExApp = PlaneExtrude.App.update m.planeExApp msg}
        | EllipseMessage     msg -> {m with ellipseApp = EllipseShading.App.update m.ellipseApp msg}
        | ChangeAppType      t   -> {m with appType = t}
        | ToggleOrientationCube  -> {m with drawOrientationCube = not m.drawOrientationCube}
        | ToggleShading          -> {m with shading = not m.shading}
        | OnKeyDownMsg key ->
            match key with
            | Aardvark.Application.Keys.D0 -> {m with shading = not m.shading}
            | _ ->
                match m.appType with
                | AppType.Boxes ->
                    let msg = key |> Boxes.App.Action.OnKeyDown
                    {m with boxesApp = Boxes.App.update m.boxesApp msg}
                | AppType.ScaleBars ->
                    let msg = key |> ScaleBar.MultiApp.Action.OnKeyDown
                    {m with barsApp = ScaleBar.MultiApp.update m.barsApp msg}
                | _ -> m
    
    let terrainSg() = Mars.Terrain.mkISg()
    let defaultEffects = Mars.Terrain.defaultEffects
    let shadingEffects = Mars.Terrain.simpleLightingEffects
    let pickSg = Mars.Terrain.pickSg
    let sky = Mars.Terrain.up
    let patchBB = Mars.Terrain.patchBB()
    
    let viewScene (m : MModel) =
        let currentScene =
            m.appType
            |> Mod.map ( fun t ->
                match t with
                | AppType.Boxes          -> Boxes.App.viewScene' m.boxesApp pickSg m.camera.view BoxesMessage
                | AppType.ScaleBars      -> ScaleBar.MultiApp.viewScene' m.barsApp m.camera.view pickSg BarsMessage
                | AppType.Shading        -> DistanceShading.App.viewScene' m.shadingApp m.camera.view pickSg ShadingMessage
                | AppType.KnownObjects   -> KnownObject.Multi.viewScene m.objectsApp m.camera.view pickSg KnownObjectMessage
                | AppType.PlaneExtrude   -> PlaneExtrude.App.viewScene' m.planeExApp m.camera.view pickSg PlaneExMessage
                | AppType.EllipseShading -> EllipseShading.App.viewScene' m.ellipseApp m.camera.view pickSg EllipseMessage
                | _ -> Sg.empty
            )
            |> Sg.dynamic
        
        let terrain =
            Mod.map2 ( fun t s ->
                
                let effects = if s then shadingEffects else defaultEffects
                
                match t with
                | AppType.Shading ->
                    terrainSg()
                    |> DistanceShading.App.mkEffects m.shadingApp effects
                | AppType.ContourLines ->
                    terrainSg()
                    |> ContourLines.App.mkEffects m.contourApp m.camera.view effects
                | AppType.VertExagg ->
                    terrainSg()
                    |> VerticalExaggeration.App.mkEffects m.vertApp m.camera.view effects
                | AppType.EllipseShading ->
                    terrainSg()
                    |> EllipseShading.App.mkEffects m.ellipseApp m.camera.view effects
                | _ ->
                    terrainSg()
                    |> Sg.effect effects
            ) m.appType m.shading
            |> Sg.dynamic

        currentScene
        |> Sg.andAlso terrain
        |> Sg.andAlso (
            m.drawOrientationCube
            |> Mod.map ( fun x ->
                if x
                then
                    OrientationCube.Sg.loadModel "../../data/models/rotationcube/rotationcube.dae"
                    |> OrientationCube.Sg.orthoOrientation m.camera.view
                else Sg.empty
            )
            |> Sg.dynamic
        )
    
    let view (m : MModel) =
        let frustum = Mod.constant ( Frustum.perspective 60.0 0.1 1000.0 1.0 )
        require Html.semui (
            let selectView =
                div [clazz "ui"][
                    Html.table [
                        Html.row "App: " [
                            Html.SemUi.dropDown m.appType ChangeAppType
                        ]
                        Html.row "Orientation Cube: " [
                            Utils.Html.toggleButton m.drawOrientationCube "Show" "Hide" ToggleOrientationCube
                        ]
                    ]
                ]
            
            let currentView =
                Incremental.div
                    (AttributeMap.ofList[])
                        (
                            alist {
                                let! t = m.appType
                                match t with
                                | AppType.Boxes          -> yield Boxes.App.view' m.boxesApp               |> UI.map BoxesMessage
                                | AppType.ScaleBars      -> yield ScaleBar.MultiApp.view' m.barsApp        |> UI.map BarsMessage
                                | AppType.Shading        -> yield DistanceShading.App.view' m.shadingApp   |> UI.map ShadingMessage
                                | AppType.ContourLines   -> yield ContourLines.App.view' m.contourApp      |> UI.map ContourMessage
                                | AppType.KnownObjects   -> yield KnownObject.Multi.view' m.objectsApp     |> UI.map KnownObjectMessage
                                | AppType.VertExagg      -> yield VerticalExaggeration.App.view' m.vertApp |> UI.map VertMessage
                                | AppType.PlaneExtrude   -> yield PlaneExtrude.App.view' m.planeExApp      |> UI.map PlaneExMessage
                                | AppType.EllipseShading -> yield EllipseShading.App.view' m.ellipseApp    |> UI.map EllipseMessage
                                | _ -> yield div[][h5 [][text "No Scale-App loaded."]]
                            }
                        )
            
            body [] [
                div [clazz "ui"][
                    CameraController.controlledControl m.camera CameraMessage frustum
                        (AttributeMap.ofList[
                            //8x Antialiasing
                            attribute "data-samples" "8"
                            onKeyDown OnKeyDownMsg
                            style "background:#121212; width:100%; height:100%;"
                        ]) (viewScene m)
                    
                    div [clazz "ui"; style "background:#121212; position:fixed; top:0; left:0; right:0; bottom:0; width:35%; max-width: 400px; height:100%; z-index:2;"][
                        Html.SemUi.tabbed [] [
                            ("Select App", selectView)
                            ("App Controls", currentView)
                        ] "Select App"
                    ]
                ]
            ]
        )
    
    let initial (sky : V3d) =
        let r       = Trafo3d.RotateInto(V3d.OOI, sky)
        let camPos  = r.Forward.TransformPos( V3d(8.0, 8.0, 6.0) )
        let camView = CameraView.lookAt camPos V3d.OOO sky
        {
            appType    = AppType.None
            boxesApp   = Boxes.App.initial camView patchBB sky
            shadingApp = DistanceShading.App.initial sky
            barsApp    = ScaleBar.MultiApp.initial
            contourApp = ContourLines.App.initial
            objectsApp = KnownObject.Multi.initial sky
            vertApp    = VerticalExaggeration.App.initial
            planeExApp = PlaneExtrude.App.initial
            ellipseApp = EllipseShading.App.initial sky
            camera     = {CameraController.initial with view = camView}
            shading    = false
            drawOrientationCube = true
        }
    
    let threads (m : Model) =
        CameraController.threads m.camera |> ThreadPool.map CameraMessage
    
    let app =
        {
            unpersist = Unpersist.instance
            threads   = threads
            initial   = initial sky
            update    = update
            view      = view
        }
    
    let start() = App.start app
