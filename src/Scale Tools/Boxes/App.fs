namespace Boxes

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
        | SetNavMode     of NavMode
        | AppMessage     of MultiBox.Action
        | OnKeyDown      of Aardvark.Application.Keys
        | ToggleDrawPatchBB
    
    let cameraShouldUpdate (m : MultiComposedAppModel) =
        not (TransformBox.grabbed m.app.boxTrafo || TransformFace.grabbed m.app.faceTrafo)
    
    let update (m : MultiComposedAppModel) (act : Action) =
        match act with
        | CameraMessage msg ->
            match cameraShouldUpdate m with
            | false -> m
            | true  ->
                let camera = CameraController.update m.camera msg
                let patchBBLabels = Labels.setup m.patchBB camera.view
                let app = MultiBox.update m.app (camera.view |> MultiBox.Action.SetLabelsView)
                {m with camera = camera; app = app; patchBBLabels = patchBBLabels}
        
        | ArcBallMessage msg ->
            match TransformBox.grabbed m.app.boxTrafo || TransformFace.grabbed m.app.faceTrafo with
            | true -> m
            | false ->
                let camera = ArcBallController.update m.camera msg
                {m with camera = camera}
        
        | SetNavMode mode -> {m with navMode = mode}

        | AppMessage msg ->
            match msg with
            | MultiBox.Action.SelectBox id ->
                let box = m.app.boxes |> HMap.find id
                let center =
                    match m.app.selected with
                    | None -> Some box.center
                    | Some b -> if b = id then Some V3d.OOO else Some box.center
                    
                {m with app = MultiBox.update m.app msg; camera = {m.camera with orbitCenter = center}}
            | _ -> {m with app = MultiBox.update m.app msg}
        
        | OnKeyDown key ->
            match key with
            | Aardvark.Application.Keys.E  -> {m with app = MultiBox.update m.app MultiBox.Action.ChangeEditMode}

            | Aardvark.Application.Keys.L  ->
                match m.app.addBoxMode with
                | AddBoxMode.Off           -> {m with app = MultiBox.update m.app (MultiBox.Action.ChangeAddBoxMode AddBoxMode.AddWithMouse)}
                | AddBoxMode.AddWithMouse  -> {m with app = MultiBox.update m.app (MultiBox.Action.ChangeAddBoxMode AddBoxMode.Off)}
                | AddBoxMode.AddFromPoints -> m
            
            | Aardvark.Application.Keys.N  ->
                match m.app.addBoxMode with
                | AddBoxMode.Off           -> {m with app = MultiBox.update m.app (MultiBox.Action.ChangeAddBoxMode AddBoxMode.AddFromPoints)}
                | AddBoxMode.AddWithMouse  -> m
                | AddBoxMode.AddFromPoints ->
                    let app = MultiBox.update m.app (MultiBox.Action.ChangeAddBoxMode AddBoxMode.Off)
                    {m with app = {app with addBoxPoints = AddBoxFromPoints.initial}}
            
            | Aardvark.Application.Keys.Enter ->
                match m.app.addBoxMode with
                | AddBoxMode.AddFromPoints ->
                    let app = MultiBox.update m.app MultiBox.Action.FinishBoxFromPoints
                    {m with app = app}
                | _ -> m
            
            | Aardvark.Application.Keys.Escape ->
                let app = MultiBox.update m.app MultiBox.Action.Escape
                {m with app = app}
            
            | Aardvark.Application.Keys.Delete -> {m with app = MultiBox.update m.app MultiBox.Action.RemoveBox}
            | Aardvark.Application.Keys.M  -> {m with app = MultiBox.update m.app MultiBox.Action.ChangeMoveMode}
            | Aardvark.Application.Keys.O  -> {m with app = MultiBox.update m.app (BoxFillMode.Solid |> MultiBox.Action.ChangeBoxFillMode)}
            | Aardvark.Application.Keys.P  -> {m with app = MultiBox.update m.app (BoxFillMode.Line |> MultiBox.Action.ChangeBoxFillMode)}
            | Aardvark.Application.Keys.B  -> {m with app = MultiBox.update m.app MultiBox.Action.ChangeBoxBlending}
            | Aardvark.Application.Keys.D1 -> {m with app = MultiBox.update m.app (FaceType.FaceTop |> TransformFace.Action.SelectFace |> MultiBox.Action.FaceTrafoMessage)}
            | Aardvark.Application.Keys.D2 -> {m with app = MultiBox.update m.app (FaceType.FaceFront |> TransformFace.Action.SelectFace |> MultiBox.Action.FaceTrafoMessage)}
            | Aardvark.Application.Keys.D3 -> {m with app = MultiBox.update m.app (FaceType.FaceRight |> TransformFace.Action.SelectFace |> MultiBox.Action.FaceTrafoMessage)}
            | Aardvark.Application.Keys.D4 -> {m with app = MultiBox.update m.app (FaceType.FaceBack |> TransformFace.Action.SelectFace |> MultiBox.Action.FaceTrafoMessage)}
            | Aardvark.Application.Keys.D5 -> {m with app = MultiBox.update m.app (FaceType.FaceLeft |> TransformFace.Action.SelectFace |> MultiBox.Action.FaceTrafoMessage)}
            | Aardvark.Application.Keys.D6 -> {m with app = MultiBox.update m.app (FaceType.FaceBottom |> TransformFace.Action.SelectFace |> MultiBox.Action.FaceTrafoMessage)}
            | Aardvark.Application.Keys.H  -> {m with app = MultiBox.update m.app MultiBox.Action.ToggleHideTrafoCtrls}
            | Aardvark.Application.Keys.U  -> {m with app = MultiBox.update m.app MultiBox.Action.Unselect}
            | _ -> m
        
        | ToggleDrawPatchBB -> {m with drawPatchBB = not m.drawPatchBB}
    
    let viewScene (m : MMultiComposedAppModel) (liftMessage : Action -> 'msg) =
        MultiBox.viewScene m.app m.camera.view ( fun x -> AppMessage x |> liftMessage ) Mars.Terrain.pickSg
        |> Sg.andAlso (
            Mars.Terrain.mkISg()
            |> Sg.effect Mars.Terrain.defaultEffects
        )
    
    let patchBBSg (m : MMultiComposedAppModel) (view : IMod<CameraView>) =
        m.drawPatchBB
        |> Mod.map ( fun x ->
            if x
            then
                let labels = Labels.labelsSg m.patchBBLabels view
                Box.mkISg m.patchBB []
                |> Sg.andAlso labels
            else
                Sg.empty
        )
        |> Sg.dynamic

    let viewScene' (m : MMultiComposedAppModel) pickSg view (liftMessage : Action -> 'msg) =
        MultiBox.viewScene m.app view ( fun x -> AppMessage x |> liftMessage ) pickSg
        |> Sg.andAlso (patchBBSg m view)
    
    let view' (m : MMultiComposedAppModel) =
        div [clazz "ui"][
            yield
                div [clazz "ui"; style "background:#121212"][
                    MultiBox.view' m.app ( fun x -> AppMessage x )
                    div [clazz "ui"][
                        Html.table [
                            Html.row "Global Bounding-Box:" [
                                Utils.Html.toggleButton m.drawPatchBB "Show" "Hide" ToggleDrawPatchBB
                            ]
                            Html.row "Size: " [
                                Incremental.div
                                    (AttributeMap.ofList[])
                                        (
                                            alist {
                                                let! s = m.patchBB.size
                                                yield
                                                    Html.table [
                                                        Html.row "X: " [
                                                            h5 [][text (sprintf "%.2f m" s.X)]
                                                        ]
                                                        Html.row "Y: " [
                                                            h5 [][text (sprintf "%.2f m" s.Y)]
                                                        ]
                                                        Html.row "Z: " [
                                                            h5 [][text (sprintf "%.2f m" s.Z)]
                                                        ]
                                                    ]
                                            }
                                        )
                            ]
                        ]
                    ]
                ]
        ]
    
    let view (m : MMultiComposedAppModel) =
        
        let frustum = Mod.constant ( Frustum.perspective 60.0 0.1 100.0 1.0 )

        let renderControlAttributes =
            amap {
                let! state = m.navMode
                match state with
                    | NavMode.FreeFly -> yield! CameraController.extractAttributes  m.camera CameraMessage  frustum
                    | NavMode.ArcBall -> yield! ArcBallController.extractAttributes m.camera ArcBallMessage frustum
                    | _               -> failwith "Invalid NavigationMode"
            } |> AttributeMap.ofAMap
        
        require Html.semui (
            div [clazz "ui"][
                yield
                    Incremental.renderControl (Mod.map2 Camera.create m.camera.view frustum)
                        (AttributeMap.unionMany [
                            renderControlAttributes
                            AttributeMap.ofList [
                                onKeyDown OnKeyDown
                                attribute "style" "width:70%; height: 100%; float: left;"
                            ]
                        ]) (viewScene m ( fun x -> x ))
                
                yield
                    div [clazz "ui"; style "background:#121212; width:30%; height:100%; float:right"][
                        MultiBox.view' m.app ( fun x -> AppMessage x )
                    ]
            ]
        )
    
    let initial view (patchBB : Box3d) (sky : V3d) =
        let patchBB = Box.setup' patchBB.Center patchBB.Size V3d.OOI "BB"
        let patchBB = {patchBB with options = {patchBB.options with fillMode = BoxFillMode.Line}}
        let patchBBLabels = Labels.setup patchBB view
        {
            app     = MultiBox.initial
            patchBB = patchBB
            patchBBLabels = patchBBLabels
            drawPatchBB = true
            camera  = {CameraController.initial with view = view}
            navMode = NavMode.FreeFly
        }
    
    let threads (m : MultiComposedAppModel) =
        CameraController.threads m.camera |> ThreadPool.map CameraMessage

    let app =
        {
            unpersist = Unpersist.instance
            threads   = threads
            initial   = initial (CameraView.look V3d.OOO -V3d.OIO Mars.Terrain.up) (Mars.Terrain.patchBB()) V3d.OOI
            update    = update
            view      = view
        }
    
    let start() = App.start app