namespace ScaleBar

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.Rendering
open Aardvark.Rendering.Text
open Aardvark.UI
open Aardvark.UI.Primitives

module MultiApp =
    
    type Action =
        | SelectBar  of string
        | ChangeAddMode
        | AddBar     of V3d
        | RemoveBar
        | MoveBar    of V3d
        | AppMessage of App.Action
        | Unselect
        | OnKeyDown  of Aardvark.Application.Keys
    
    let update (m : MultiModel) (act : Action) =
        match act with
        | SelectBar id ->
            match m.selected with
            | None -> {m with selected = Some id}
            | Some bar ->
                if id = bar
                then {m with selected = None}
                else {m with selected = Some id}
        | ChangeAddMode -> {m with addMode = not m.addMode}
        | AddBar v ->
            let sb = App.setup v 1.0
            let bars = m.bars |> HMap.add sb.scaleBar.id sb
            {m with bars = bars; selected = Some sb.scaleBar.id; addMode = false}
        | RemoveBar ->
            match m.selected with
            | Some id ->
                let bars = m.bars |> HMap.remove id
                {m with bars = bars; selected = None}
            | None -> m
        | MoveBar v ->
            match m.selected with
            | None -> m
            | Some id ->
                let bars = m.bars |> HMap.alter id ( fun x ->
                    match x with
                    | None -> x
                    | Some sb ->
                        let msg = v |> App.Action.SetPos
                        Some (App.update sb msg)
                )
                {m with bars = bars}
        | AppMessage msg ->
            match m.selected with
            | None -> m
            | Some id ->
                let bars =
                    m.bars
                    |> HMap.alter id ( fun x ->
                        match x with
                        | None -> x
                        | Some sb -> Some (App.update sb msg)
                    )
                {m with bars = bars}
        | Unselect -> {m with selected = None}
        | OnKeyDown key ->
            match key with
            | Aardvark.Application.Keys.L -> {m with addMode = not m.addMode}
            | Aardvark.Application.Keys.Delete ->
                match m.selected with
                | None -> m
                | Some id ->
                    let bars = m.bars |> HMap.remove id
                    {m with bars = bars; selected = None}
            | _ -> m
    
    let viewScene' (m : MMultiModel) (view : IMod<CameraView>) pickSg (liftMessage : Action -> 'msg) =
        let pickGraph =
            Mod.map2 ( fun am sel ->
                if am
                then pickSg [Sg.onDoubleClick ( fun x -> x |> AddBar |> liftMessage )]
                else
                    match sel with
                    | Some _ -> pickSg [Sg.onDoubleClick ( fun x -> x |> MoveBar |> liftMessage )]
                    | None -> Sg.empty
            ) m.addMode m.selected
            |> Sg.dynamic
        
        aset {
            for (id,sb) in m.bars |> AMap.toASet do
                let barEvents =
                    [
                        Sg.onDoubleClick ( fun _ ->
                            id |> SelectBar |> liftMessage
                        )
                    ]
                
                yield
                    App.viewScene sb view
                    |> Sg.requirePicking
                    |> Sg.noEvents
                    |> Sg.withEvents barEvents
        }
        |> Sg.set
        |> Sg.andAlso (
            pickGraph
        )

    let view' (m : MMultiModel) =
        div [clazz "ui"] [
            Html.table [
                Html.row "Add: " [
                    Utils.Html.toggleButton m.addMode "Add" "Cancel" ChangeAddMode
                ]
                Html.row "Controls: " [
                    Incremental.div
                        (AttributeMap.ofList[])
                            (
                                alist {
                                    let! sel = m.selected
                                    match sel with
                                    | None -> yield h5 [][text "No Scale-Bar selected."]
                                    | Some id ->
                                        let! sb = m.bars |> AMap.find id
                                        yield
                                            Html.SemUi.stuffStack [
                                                button [clazz "ui button blue"; onClick ( fun _ -> Unselect )][text "Unselect"]
                                                button [clazz "ui button red"; onClick ( fun _ -> RemoveBar )][text "Remove"]
                                                App.view' sb |> UI.map AppMessage
                                            ]
                                }
                            )
                ]
            ]
        ]
    
    let initial =
        {
            bars     = HMap.empty
            selected = None
            addMode  = false
        }
