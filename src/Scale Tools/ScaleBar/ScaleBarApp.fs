namespace ScaleBar

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.Rendering
open Aardvark.Rendering.Text
open Aardvark.UI
open Aardvark.UI.Primitives

module App =
    
    type Action =
        | HeightMessage of Numeric.Action
        | ToggleHorizontal
        | SetPos of V3d
    
    let update (m : Model) (act : Action) =
        match act with
        | HeightMessage msg ->
            match msg with
            | Numeric.Action.SetValue x ->
                let scaleBar = ScaleBar.update m.scaleBar (x |> ScaleBar.Action.SetHeight)
                {m with height = Numeric.update m.height msg; scaleBar = scaleBar}
            | _ -> m
        
        | ToggleHorizontal -> {m with horizontal = not m.horizontal}
        | SetPos pos -> {m with scaleBar = {m.scaleBar with pos = pos}}
    
    let viewScene (m : MModel) (view : IMod<CameraView>) =
        m.horizontal
        |> Mod.map ( fun h ->
            if h
            then ScaleBar.drawBarHorizontal m.scaleBar view
            else ScaleBar.drawBarVertical m.scaleBar view
        )
        |> Sg.dynamic
    
    let viewScene' (m : MModel) (view : IMod<CameraView>) pickSg (liftMessage : Action -> 'msg) =
        m.horizontal
        |> Mod.map ( fun h ->
            if h
            then ScaleBar.drawBarHorizontal m.scaleBar view
            else ScaleBar.drawBarVertical m.scaleBar view
        )
        |> Sg.dynamic
        |> Sg.andAlso (
            pickSg [Sg.onDoubleClick ( fun v -> v |> SetPos |> liftMessage )]
        )
    
    let view' (m : MModel) =
        div [clazz "ui"][
            Html.table [
                Html.row "Height : " [
                    Numeric.view m.height |> UI.map HeightMessage
                ]
                Html.row "Vert/Hori: " [
                    Html.SemUi.toggleBox m.horizontal ToggleHorizontal
                ]
            ]
        ]
    
    let initial =
        let scaleBar = ScaleBar.initial
        let h        = scaleBar.height
        {
            scaleBar     = scaleBar
            height       = {Numeric.init with min = 1.0; max = 50.0; step = 1.0; value = h}
            horizontal   = false
        }
    
    let setup pos height =
        let scaleBar = ScaleBar.setup pos height
        let h        = scaleBar.height
        {
            scaleBar     = scaleBar
            height       = {Numeric.init with min = 1.0; max = 50.0; step = 1.0; value = h}
            horizontal   = false
        }