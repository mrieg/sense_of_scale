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
        | ToggleStepped
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
        | ToggleStepped    -> {m with stepped = not m.stepped}
        | SetPos pos       -> {m with scaleBar = {m.scaleBar with pos = pos}}
    
    let viewScene (m : MModel) (view : IMod<CameraView>) =
        adaptive {
            let! height = m.height.value
            let! stepped = m.stepped
            let! horiz = m.horizontal
            //uncomment for correct labels when stepping, bad performance
            //let! v = view
            
            let sg =
                if stepped
                    then
                    if horiz
                    then ScaleBar.drawBarHorizontalStepped m.scaleBar view
                    else ScaleBar.drawBarVerticalStepped m.scaleBar view
                else
                    if horiz
                    then ScaleBar.drawBarHorizontal m.scaleBar view
                    else ScaleBar.drawBarVertical m.scaleBar view
            
            return sg
        }
        |> Sg.dynamic
    
    let viewScene' (m : MModel) (view : IMod<CameraView>) pickSg (liftMessage : Action -> 'msg) =
        viewScene m view
        |> Sg.andAlso (
            pickSg [Sg.onDoubleClick ( fun v -> v |> SetPos |> liftMessage )]
        )
    
    let view' (m : MModel) =
        div [clazz "ui"][
            Incremental.table
                (AttributeMap.ofList[
                    style "color:#ffffff"
                ])
                    (
                        alist {
                            let! stepped = m.stepped
                            if not stepped
                            then yield Html.row "Height: " [Numeric.view m.height |> UI.map HeightMessage]
                            yield Html.row "Vert/Hori: " [Utils.Html.toggleButton m.horizontal "Horizontal" "Vertical" ToggleHorizontal]
                            yield Html.row "Stepped: " [Utils.Html.toggleButton m.stepped "Stepped" "Fixed" ToggleStepped]
                        }
                    )
        ]
    
    let initial =
        let scaleBar = ScaleBar.initial
        let h        = scaleBar.height
        {
            scaleBar   = scaleBar
            height     = {Numeric.init with min = 1.0; max = 50.0; step = 1.0; value = h}
            horizontal = false
            stepped    = false
        }
    
    let setup pos height =
        let scaleBar = ScaleBar.setup pos height
        let h        = scaleBar.height
        {
            scaleBar   = scaleBar
            height     = {Numeric.init with min = 0.0; max = 50.0; step = 0.1; value = h}
            horizontal = false
            stepped    = false
        }
