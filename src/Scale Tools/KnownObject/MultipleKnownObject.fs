namespace KnownObject

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.UI
open Aardvark.UI.Trafos
open Aardvark.UI.Primitives

module Multi =
    
    type Action =
        | SelectModel  of string
        | AddModel
        | RemoveModel
        | ModelMessage of KnownObject.Action
    
    let trafoGrabbed (m : MultiModel) =
        match m.selected with
        | None -> false
        | Some id ->
            let model = m.models |> HMap.find id
            model.trafo.grabbed.IsSome
    
    let alterSelected (id : string) (value : bool) (modelMap : hmap<string, Model>) =
        modelMap
        |> HMap.alter id ( fun x ->
            match x with
            | None -> x
            | Some model -> Some {model with selected = value}
        )

    let update (m : MultiModel) (act : Action) =
        match act with
        | SelectModel id ->
            match m.selected with
            | None ->
                let models = m.models |> alterSelected id true
                {m with selected = Some id; models = models}
            | Some str ->
                if str = id
                then
                    let models = m.models |> alterSelected id false
                    {m with selected = None; models = models}
                else
                    let models = m.models |> alterSelected id  true
                    let models = models   |> alterSelected str false
                    {m with selected = Some id; models = models}
        
        | AddModel ->
            let model = KnownObject.initial m.sky
            let models = m.models |> HMap.add model.id model
            let models =
                match m.selected with
                | None -> models
                | Some id -> models |> alterSelected id false
            
            {m with models = models; selected = Some model.id}
        
        | RemoveModel ->
            match m.selected with
            | None -> m
            | Some str ->
                let models = m.models |> HMap.remove str
                {m with models = models; selected = None}
        
        | ModelMessage msg ->
            match m.selected with
            | None -> m
            | Some id ->
                let model = m.models |> HMap.find id
                let newModel = KnownObject.update model msg
                let models =
                    m.models
                    |> HMap.alter id ( fun x ->
                        match x with
                        | None -> x
                        | Some _ -> Some newModel
                    )
                
                {m with models = models}
    
    let viewScene (m : MMultiModel) (view : IMod<CameraView>) pickSg (liftMessage : Action -> 'msg) =
        aset {
            let! sel = m.selected
            for (id,model) in m.models |> AMap.toASet do
                let trafoSg =
                    match sel with
                    | None -> Sg.empty
                    | Some str ->
                        if str = id
                        then KnownObject.mkTrafoSg model view (fun x -> x |> ModelMessage |> liftMessage)
                        else Sg.empty

                yield
                    KnownObject.viewScene' model view pickSg [Sg.onClick ( fun _ -> SelectModel id |> liftMessage )] (fun x -> x |> ModelMessage |> liftMessage)
                    |> Sg.andAlso (trafoSg)
        }
        |> Sg.set
    
    let view' (m : MMultiModel) =
        div [clazz "ui"] [
            Html.table [
                Html.row "Add Model: " [
                    button [clazz "ui button blue"; onClick ( fun _ -> AddModel )][text "Add"]
                ]
            ]
            Incremental.div
                (AttributeMap.ofList[])
                    (
                        alist {
                            let! sel = m.selected
                            match sel with
                            | Some id ->
                                let! model = AMap.find id m.models
                                yield
                                    div [clazz "ui"][
                                        Html.table [
                                            Html.row "Remove Model: " [
                                                button [clazz "ui button red"; onClick ( fun _ -> RemoveModel )][text "Remove"]
                                            ]
                                        ]
                                        KnownObject.view' model |> UI.map ModelMessage
                                    ]
                            | None -> yield h5 [][text "nothing selected."]
                        }
                    )
        ]
    
    let initial sky =
        {
            selected = None
            models   = HMap.empty
            sky      = sky
        }