namespace Boxes

open Aardvark.Base.Rendering
open Aardvark.UI

module RenderOptions =
    
    type Action =
    | ChangeCullMode    of CullMode
    | ChangeFillMode    of BoxFillMode
    | ChangeBlending

    let update (m : RenderOptionsModel) (act : Action) =
        match act with
        | ChangeCullMode x -> {m with cullMode = x}
        | ChangeFillMode x -> {m with fillMode = x}
        | ChangeBlending ->
            printfn "Blending: %b" (not m.blending)
            {m with blending = not m.blending}
    
    let view' (m : MRenderOptionsModel) (liftMessage : Action -> 'msg) =
        Html.table [
            Html.row "FillMode: " [
                Html.SemUi.dropDown m.fillMode ChangeFillMode |> UI.map liftMessage
            ]
            Html.row "CullMode: " [
                Html.SemUi.dropDown m.cullMode ChangeCullMode |> UI.map liftMessage
            ]
            Html.row "Blending: " [
                Html.SemUi.toggleBox m.blending ChangeBlending |> UI.map liftMessage
            ]
        ]
    
    let initial =
        {
            cullMode = CullMode.None
            fillMode = BoxFillMode.Solid
            blending = false
        }
