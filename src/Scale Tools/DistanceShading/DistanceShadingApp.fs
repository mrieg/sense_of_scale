namespace DistanceShading

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.Rendering
open Aardvark.SceneGraph
open Aardvark.UI
open Aardvark.UI.Primitives

module App =
    
    type Action =
        | CameraMessage   of CameraController.Message
        | ControlsMessage of Controls.Action
        | SetCenterPos    of V3d
        | ScaleBarMessage of ScaleBar.App.Action
    
    let update (m : Model) (act : Action) =
        match act with
        | CameraMessage msg   -> {m with camera = CameraController.update m.camera msg}
        | ControlsMessage msg -> {m with controls = Controls.update m.controls msg}
        | SetCenterPos v ->
            let controls =
                {
                    m.controls with
                        center =
                            {
                                m.controls.center with
                                    value = v
                                    x = {m.controls.center.x with value = v.X}
                                    y = {m.controls.center.y with value = v.Y}
                                    z = {m.controls.center.z with value = v.Z}
                            }
                }
            
            let scaleBar = ScaleBar.App.update m.scaleBar (v |> ScaleBar.App.Action.SetPos)
            {m with controls = controls; scaleBar = scaleBar}
        
        | ScaleBarMessage msg -> {m with scaleBar = ScaleBar.App.update m.scaleBar msg}
    
    let mkGroundSg (m : MModel) sg pickSg =
        let events = [Sg.onDoubleClick ( fun v -> SetCenterPos v )]
        
        let lw = Mod.constant 0.1
        let startColor = V4d(0.9, 0.32, 0.0, 1.0) |> Mod.constant
        let endColor   = V4d(0.0, 0.22, 0.8, 1.0) |> Mod.constant
        let lineColor  = V4d(0.8, 0.8,  0.8, 1.0) |> Mod.constant

        sg
        |> Sg.effect [
            DefaultSurfaces.trafo |> toEffect
            DefaultSurfaces.constantColor C4f.White |> toEffect
            DefaultSurfaces.diffuseTexture |> toEffect
            Shader.distShading |> toEffect
        ]
        |> Sg.uniform "CenterPosition" (m.controls.center.value |> Mod.map ( fun v -> V4d(v.X, v.Y, v.Z, 1.0) ))
        |> Sg.uniform "Radius"         m.controls.radius.value
        |> Sg.uniform "NumLines"       (m.controls.levels.value |> Mod.map ( fun x -> int x ))
        |> Sg.uniform "DrawLines"      m.controls.lines
        |> Sg.uniform "CelColors"      m.controls.discrete
        |> Sg.uniform "AlphaValue"     m.controls.alpha.value
        |> Sg.uniform "LineWidth"      lw
        |> Sg.uniform "StartColor"     startColor
        |> Sg.uniform "EndColor"       endColor
        |> Sg.uniform "LineColor"      lineColor
        |> Sg.andAlso (
            pickSg events
        )
    
    let mkEffects (m : MModel) effects isg =
        let lw = Mod.constant 0.1
        let startColor = V4d(0.9, 0.32, 0.0, 1.0) |> Mod.constant
        let endColor   = V4d(0.0, 0.22, 0.8, 1.0) |> Mod.constant
        let lineColor  = V4d(0.8, 0.8,  0.8, 1.0) |> Mod.constant

        let efx =
            [
                for e in effects do
                    yield e
                yield Shader.distShading |> toEffect
            ]
        
        isg
        |> Sg.uniform "CenterPosition" (m.controls.center.value |> Mod.map ( fun v -> V4d(v.X, v.Y, v.Z, 1.0) ))
        |> Sg.uniform "Radius"         m.controls.radius.value
        |> Sg.uniform "NumLines"       (m.controls.levels.value |> Mod.map ( fun x -> int x ))
        |> Sg.uniform "DrawLines"      m.controls.lines
        |> Sg.uniform "CelColors"      m.controls.discrete
        |> Sg.uniform "AlphaValue"     m.controls.alpha.value
        |> Sg.uniform "LineWidth"      lw
        |> Sg.uniform "StartColor"     startColor
        |> Sg.uniform "EndColor"       endColor
        |> Sg.uniform "LineColor"      lineColor
        |> Sg.effect efx

    let mkBarSg (m : MModel) view =
        m.controls.drawBar
        |> Mod.map ( fun x ->
            if x then ScaleBar.App.viewScene m.scaleBar view else Sg.empty
        )
        |> Sg.dynamic

    let viewScene (m : MModel) view =
        [mkGroundSg m (Mars.Terrain.mkISg()) Mars.Terrain.pickSg; mkBarSg m view]
        |> Sg.ofList
    
    let viewScene' (m : MModel) view pickSg (liftMessage : Action -> 'msg) =
        let events = [Sg.onDoubleClick ( fun v -> SetCenterPos v |> liftMessage )]
        [pickSg events; mkBarSg m view]
        |> Sg.ofList
    
    let view' (m : MModel) =
        div [clazz "ui"; style "background:#121212"][
            Html.table [
                Html.row "Options: " [
                    Html.SemUi.stuffStack [
                        Html.table [
                            Html.row "Discrete: " [
                                Html.SemUi.toggleBox m.controls.discrete Controls.Action.ChangeDiscrete |> UI.map ControlsMessage
                            ]
                            Html.row "Lines: "[
                                Html.SemUi.toggleBox m.controls.lines Controls.Action.ChangeLines |> UI.map ControlsMessage
                            ]
                        ]
                    ]
                ]
                Html.row "Radius: " [
                    Numeric.view m.controls.radius |> UI.map Controls.Action.UpdateRadius |> UI.map ControlsMessage
                ]
                Html.row "Levels: " [
                    Numeric.view m.controls.levels |> UI.map Controls.Action.UpdateLevels |> UI.map ControlsMessage
                ]
                Html.row "Alpha: " [
                    Numeric.view m.controls.alpha |> UI.map Controls.Action.UpdateAlpha |> UI.map ControlsMessage
                ]
                Html.row "ScaleBar: " [
                    Html.SemUi.stuffStack [
                        Html.table [
                            Html.row "Draw Scale-Bar: " [
                                Html.SemUi.toggleBox m.controls.drawBar Controls.Action.ChangeDrawBar |> UI.map ControlsMessage
                            ]
                        ]
                        ScaleBar.App.view' m.scaleBar |> UI.map ScaleBarMessage
                    ]
                ]
            ]
        ]

    let view (m : MModel) =
        let frustum = Mod.constant (Frustum.perspective 60.0 0.1 1000.0 1.0)
        require Html.semui (
            div [clazz "ui"][
                CameraController.controlledControl m.camera CameraMessage frustum
                    (AttributeMap.ofList[
                        attribute "style" "background:#121212; width:70%; height:100%; float:left"
                    ]) (viewScene m m.camera.view)
                
                div [clazz "ui"; style "background:#121212; width:30%; height:100%; float:right"][
                    view' m
                ]
            ]
        )
    
    let initial (up : V3d) =
        let camView = CameraView.look V3d.OOO -V3d.OIO up
        {
            scaleBar   = ScaleBar.App.initial
            controls   = Controls.initial
            camera     = {CameraController.initial with view = camView}
        }

    let threads (m : Model) = CameraController.threads m.camera |> ThreadPool.map CameraMessage

    let app =
        {
            unpersist = Unpersist.instance
            threads   = threads
            initial   = initial Mars.Terrain.up
            update    = update
            view      = view
        }
    
    let start() = App.start app