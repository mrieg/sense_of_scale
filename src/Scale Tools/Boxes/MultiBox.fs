namespace Boxes

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.Rendering
open Aardvark.UI
open Aardvark.UI.Trafos
open Aardvark.UI.Primitives
open MBrace.FsPickler

module MultiBox =
    
    let setup (numBoxes : int) =
        let boxes =
            [
                for i in 0..numBoxes-1 do
                    let center = V3d.OOO - V3d.IOO * 2.0 * (float i)
                    let size = V3d.III
                    let rotation = V3d.OOO
                    let box = Box.setup center size rotation "Box"
                    yield (box.id, box)
            ] |> HMap.ofList

        {
            boxes          = boxes
            selected       = None
            trafoBoxMode   = false
            editMode       = false
            addBoxMode     = AddBoxMode.Off
            boxTrafo       = TransformBox.initial TrafoKind.Translate
            faceTrafo      = TransformFace.initial
            trafoMode      = TrafoMode.Local
            trafoKind      = TrafoKind.Translate
            addBoxPoints   = AddBoxFromPoints.initial
            moveBoxMode    = false
            labels         = Labels.initial
            sceneName      = ""
            hideTrafoCtrls = false
        }
    
    let initial = setup 0

    type Action =
        | SetLabelsView           of CameraView
        | SelectBox               of string
        | Unselect
        | BoxTrafoMessage         of TransformBox.Action
        | FaceTrafoMessage        of TransformFace.Action
        | ChangeTrafoMode         of TrafoMode
        | ChangeTrafoKind         of TrafoKind
        | ChangeEditMode
        | AddBox                  of V3d
        | RemoveBox
        | ChangeAddBoxMode        of AddBoxMode
        | AddBoxWithMouse         of V3d * V3d
        | AddBoxPoint             of V3d
        | AddBoxFromPointsMessage of AddBoxFromPoints.Action
        | FinishBoxFromPoints
        | ChangeBoxFillMode       of BoxFillMode
        | ChangeBoxBlending
        | BoxModelAction          of Box.Action
        | ChangeMoveMode
        | MoveBoxWithMouse        of V3d
        | Escape
        | Save                    of string
        | Load                    of string
        | SetSceneName            of string
        | NewScene
        | ToggleHideTrafoCtrls
    
    let boxesColors (boxMap : hmap<string,BoxModel>) (id : string) (state : BoxSelectionState) =
        boxMap
        |> HMap.alter id ( fun x ->
            match x with
            | None -> None
            | Some b ->
                Some {b with color = Box.mkColor state}
        )

    let update (m : MultiBoxAppModel) (act : Action) =
        match act with
        | SetLabelsView view -> {m with labels = Labels.setup m.labels.box view}

        | SelectBox id ->
            let labels = Labels.setup (m.boxes |> HMap.find id) m.labels.view
            let state = {selected = true; trafoBoxMode = false; editMode = false}
            match m.selected with
            | None ->
                let newBoxes = boxesColors m.boxes id state
                let boxTrafo = TransformBox.setup (HMap.find id newBoxes) m.trafoKind m.trafoMode
                {m with boxes = newBoxes; selected = Some id; boxTrafo = boxTrafo; editMode = false; moveBoxMode = false; labels = labels; hideTrafoCtrls = false}
            | Some bid ->
                match id = bid with
                | true ->
                    let state = {state with selected = false}
                    let newBoxes = boxesColors m.boxes id state
                    let boxTrafo = TransformBox.setup (HMap.find id newBoxes) m.trafoKind m.trafoMode
                    {m with boxes = newBoxes; selected = None; boxTrafo = boxTrafo; editMode = false; moveBoxMode = false; labels = labels; hideTrafoCtrls = false}
                | false ->
                    let oldState = {state with selected = false}
                    let b1 = boxesColors m.boxes bid oldState
                    let b2 = boxesColors b1 id state
                    let boxTrafo = TransformBox.setup (HMap.find id b2) m.trafoKind m.trafoMode
                    {m with boxes = b2; selected = Some id; boxTrafo = boxTrafo; editMode = false; moveBoxMode = false; labels = labels; hideTrafoCtrls = false}
        
        | Unselect ->
            let state = {selected = false; trafoBoxMode = false; editMode = false}
            match m.selected with
            | None -> m
            | Some id ->
                let newBoxes = boxesColors m.boxes id state
                let boxTrafo = TransformBox.setup (HMap.find id newBoxes) m.trafoKind m.trafoMode
                {m with boxes = newBoxes; selected = None; boxTrafo = boxTrafo; editMode = false; moveBoxMode = false;}

        | BoxTrafoMessage msg ->
            match m.selected with
            | None -> m
            | Some id ->
                match m.editMode with
                | true -> m
                | false ->
                    let boxTrafo = TransformBox.update m.boxTrafo msg
                    let box = {boxTrafo.box with options = (HMap.find id m.boxes).options}
                    let boxes =
                        m.boxes
                        |> HMap.alter id ( fun x ->
                            match x with
                            | None -> None
                            | Some b -> Some box
                        )
                    
                    let labels = Labels.setup box m.labels.view
                    {m with boxTrafo = boxTrafo; boxes = boxes; moveBoxMode = false; labels = labels}
        
        | FaceTrafoMessage msg ->
            match m.selected with
            | None -> m
            | Some id ->
                match m.editMode with
                | false -> m
                | true ->
                    let faceTrafo = TransformFace.update m.faceTrafo msg
                    let box = {faceTrafo.box with options = (HMap.find id m.boxes).options}
                    let boxes =
                        m.boxes
                        |> HMap.alter id ( fun x ->
                            match x with
                            | None -> None
                            | Some b -> Some box
                            )
                    
                    let labels = Labels.setup box m.labels.view
                    {m with faceTrafo = faceTrafo; boxes = boxes; moveBoxMode = false; labels = labels}
        
        | ChangeTrafoMode trafoMode ->
            let boxes =
                m.boxes
                |> HMap.map ( fun id b ->
                    {b with trafoMode = trafoMode}
                )
            
            let boxTrafo = TransformBox.setup {m.boxTrafo.box with trafoMode = trafoMode} m.trafoKind trafoMode
            {m with trafoMode = trafoMode; boxes = boxes; boxTrafo = boxTrafo; moveBoxMode = false}
        
        | ChangeTrafoKind trafoKind -> {m with boxTrafo = {m.boxTrafo with trafoKind = trafoKind}; trafoKind = trafoKind}

        | ChangeEditMode ->
            match m.selected with
            | None ->
                {m with editMode = false; moveBoxMode = false}
            | Some id ->
                let state = {selected = true; editMode = not m.editMode; trafoBoxMode = false}
                let boxes =
                    m.boxes
                    |> HMap.alter id ( fun x ->
                        match x with
                        | None -> Some m.boxTrafo.box
                        | Some b -> Some m.faceTrafo.box
                    )
                
                let boxes = boxesColors m.boxes id state
                let box = HMap.find id boxes
                let faceTrafo = TransformFace.setup box
                let boxTrafo = TransformBox.setup box m.trafoKind m.trafoMode
                {m with editMode = not m.editMode; boxes = boxes; faceTrafo = faceTrafo; boxTrafo = boxTrafo; moveBoxMode = false}
        
        | AddBox sky ->
            let box = Box.initial V3d.OOO sky.Normalized
            let boxes = HMap.add box.id box m.boxes
            {m with boxes = boxes; moveBoxMode = false}
        
        | RemoveBox ->
            match m.selected with
            | None -> m
            | Some id ->
                match m.editMode with
                | true -> m
                | false ->
                    let boxes = HMap.remove id m.boxes
                    {m with boxes = boxes; selected = None; editMode = false; moveBoxMode = false}
        
        | ChangeAddBoxMode mode -> {m with addBoxMode = mode; moveBoxMode = false}

        | AddBoxWithMouse (v,sky) ->
            let box = Box.initial v sky.Normalized
            let boxes = HMap.add box.id box m.boxes
            {m with boxes = boxes; addBoxMode = AddBoxMode.Off}

        | AddBoxPoint v ->
            let id = System.Guid.NewGuid().ToString()
            let msg = AddBoxFromPoints.Action.AddPoint (v,id)
            {m with addBoxPoints = AddBoxFromPoints.update m.addBoxPoints msg}
        
        | AddBoxFromPointsMessage msg -> {m with addBoxPoints = AddBoxFromPoints.update m.addBoxPoints msg}

        | FinishBoxFromPoints ->
            match m.addBoxPoints.ptList.Count with
            | 0 | 1 | 2 -> {m with addBoxPoints = AddBoxFromPoints.initial; addBoxMode = AddBoxMode.Off}
            | _ ->
                let box = {m.addBoxPoints.box with color = Box.Config.unselectColor; options = RenderOptions.initial}
                let boxes = HMap.add box.id box m.boxes
                {m with boxes = boxes; addBoxPoints = AddBoxFromPoints.initial; addBoxMode = AddBoxMode.Off}
        
        | ChangeBoxFillMode mode ->
            match m.selected with
            | None -> m
            | Some id ->
                let box = HMap.find id m.boxes
                let box = {box with options = RenderOptions.update box.options (mode |> RenderOptions.Action.ChangeFillMode)}
                let boxes =
                    m.boxes
                    |> HMap.alter id ( fun x ->
                        match x with
                        | None -> x
                        | Some b -> Some box
                    )
            
                {m with boxes = boxes}
        
        | ChangeBoxBlending ->
            match m.selected with
            | None -> m
            | Some id ->
                let box = HMap.find id m.boxes
                let box = {box with options = RenderOptions.update box.options RenderOptions.Action.ChangeBlending}
                let boxes =
                    m.boxes
                    |> HMap.alter id ( fun x ->
                        match x with
                        | None -> x
                        | Some b -> Some box
                    )
            
                {m with boxes = boxes}
        
        | BoxModelAction msg ->
            match m.selected with
            | None -> m
            | Some id ->
                let box = Box.update (HMap.find id m.boxes) msg
                let boxes =
                    m.boxes
                    |> HMap.alter id ( fun x ->
                        match x with
                        | None -> x
                        | Some _ -> Some box
                    )
                
                let boxTrafo = TransformBox.setup box m.boxTrafo.trafoKind m.trafoMode
                let faceTrafo = TransformFace.setup box
                let labels = Labels.setup box m.labels.view
                {m with boxes = boxes; boxTrafo = boxTrafo; faceTrafo = faceTrafo; labels = labels}
        
        | ChangeMoveMode ->
            match m.selected with
            | None -> m
            | Some id ->
                match m.editMode with
                | true -> m
                | false ->
                    match m.addBoxMode with
                    | AddBoxMode.Off ->
                        {m with moveBoxMode = not m.moveBoxMode}
                    | _ -> m
        
        | MoveBoxWithMouse v ->
            match m.selected with
            | None -> m
            | Some id ->
                let boxes =
                    m.boxes
                    |> HMap.alter id ( fun x ->
                        match x with
                        | None -> None
                        | Some b -> Some ( Box.update b (Box.Action.UpdateCenter v) )
                    )
                
                let box = HMap.find id boxes
                let boxTrafo = TransformBox.setup box m.trafoKind m.trafoMode
                let faceTrafo = TransformFace.setup box
                let labels = Labels.setup box m.labels.view
                {m with boxes = boxes; boxTrafo = boxTrafo; faceTrafo = faceTrafo; moveBoxMode = false; labels = labels}
            
        | Escape -> {m with addBoxMode = AddBoxMode.Off; addBoxPoints = AddBoxFromPoints.initial; moveBoxMode = false}

        | SetSceneName name -> {m with sceneName = name}
        | Save filename ->
            if m.sceneName = ""
            then
                printfn "Scene has no name! Did not save File."
            else
                let path = filename + "\\" + m.sceneName + ".bxs"

                let pickler = FsPickler.CreateBinarySerializer()
                let bytes = pickler.Pickle m
                do System.IO.File.WriteAllBytes(path, bytes)
                
                printfn "Saved Scene to: %s" path
            
            m
        
        | Load filename ->
            printfn "Loading %s" filename
            let pickler = FsPickler.CreateBinarySerializer()
            let bytes = System.IO.File.ReadAllBytes(filename)
            pickler.UnPickle<MultiBoxAppModel> bytes
        
        | NewScene -> initial

        | ToggleHideTrafoCtrls -> {m with hideTrafoCtrls = not m.hideTrafoCtrls}
    
    let viewScene (m : MMultiBoxAppModel) (camView : IMod<CameraView>) (liftMessage : Action -> 'msg) pickSg =
        let scene =
            aset {
                let! editMode = m.editMode
                for (id,b) in m.boxes |> AMap.toASet do
                    yield
                        if editMode
                        then Box.mkISg b [Sg.onDoubleClick ( fun _ -> SelectBox id |> liftMessage )]
                        else Box.mkISg b [Sg.onDoubleClick ( fun _ -> SelectBox id |> liftMessage )]
            }
            |> Sg.set
            |> Sg.shader {
                do! DefaultSurfaces.trafo
                do! DefaultSurfaces.vertexColor
                do! DefaultSurfaces.simpleLighting
            }
        
        let boxTrafoSg =
            TransformBox.mkControllerISg m.boxTrafo camView ( fun x -> BoxTrafoMessage x |> liftMessage )
        
        let faceTrafoSg =
            TransformFace.mkControllerISg m.faceTrafo camView ( fun x -> FaceTrafoMessage x |> liftMessage )
        
        let trafoSg =
            m.hideTrafoCtrls
            |> Mod.bind ( fun h ->
                match h with
                | true -> Mod.constant Sg.empty
                | false ->
                    Mod.map2 ( fun x y ->
                        match x with
                        | Some id ->
                            if y
                            then faceTrafoSg
                            else boxTrafoSg
                        | None -> Sg.empty
                    ) m.selected m.editMode
            )
            |> Sg.dynamic

        let addBoxPoints = AddBoxFromPoints.drawPoints m.addBoxPoints camView
        
        let labelsSg =
            m.selected
            |> Mod.map ( fun sel ->
                match sel with
                | None -> Sg.empty
                | Some _ -> Labels.labelsSg m.labels camView
            )
            |> Sg.dynamic
        
        let sky = (camView |> Mod.force).Sky

        Mod.map2 ( fun mode move ->
            match mode with
            | AddBoxMode.Off ->
                match move with
                | false ->
                    [labelsSg; scene; trafoSg]
                    |> Sg.ofList
                | true  ->
                    [labelsSg; scene; pickSg [Sg.onDoubleClick ( fun v -> MoveBoxWithMouse v |> liftMessage )]]
                    |> Sg.ofList
            
            | AddBoxMode.AddWithMouse  ->
                [scene; pickSg [Sg.onDoubleClick ( fun v -> AddBoxWithMouse (v,sky) |> liftMessage )]]
                |> Sg.ofList
            
            | AddBoxMode.AddFromPoints ->
                [scene; addBoxPoints (fun msg -> AddBoxFromPointsMessage msg |> liftMessage); pickSg [Sg.onDoubleClick ( fun v -> AddBoxPoint v |> liftMessage)]]
                |> Sg.ofList
            
        ) m.addBoxMode m.moveBoxMode
        |> Sg.dynamic
    
    let view' (m : MMultiBoxAppModel) (liftMessage : Action -> 'msg) =
        div [clazz "ui"][
            //selected box ui
            let selBoxUI =
                Incremental.div
                    (AttributeMap.ofList[]) (
                        alist {
                            let! sel = m.selected
                            match sel with
                            | None -> yield div [][]
                            | Some id ->
                                let! box = AMap.find id m.boxes
                            
                                let editDom =
                                    Incremental.div
                                        (AttributeMap.ofList []) (
                                            alist {
                                                let! e = m.editMode
                                                if e
                                                then
                                                    yield
                                                        div [clazz "ui"] [
                                                            button [clazz "ui button orange"; onClick ( fun _ -> ChangeEditMode )] [text "Exit Edit Mode"]
                                                            Html.table [
                                                                Html.row "Faces: " [
                                                                    Incremental.div
                                                                        (AttributeMap.ofList []) (
                                                                            alist {
                                                                                let! selFace = m.faceTrafo.selectedFace
                                                                                match selFace with
                                                                                | Some FaceTop -> yield button [clazz "ui button green"; onClick ( fun _ -> TransformFace.SelectFace FaceTop |> FaceTrafoMessage )] [text "1"]
                                                                                | _ -> yield button [clazz "ui button"; onClick ( fun _ -> TransformFace.SelectFace FaceTop |> FaceTrafoMessage )] [text "1"]

                                                                                match selFace with
                                                                                | Some FaceFront -> yield button [clazz "ui button green"; onClick ( fun _ -> TransformFace.SelectFace FaceFront |> FaceTrafoMessage )] [text "2"]
                                                                                | _ -> yield button [clazz "ui button"; onClick ( fun _ -> TransformFace.SelectFace FaceFront |> FaceTrafoMessage )] [text "2"]

                                                                                match selFace with
                                                                                | Some FaceRight -> yield button [clazz "ui button green"; onClick ( fun _ -> TransformFace.SelectFace FaceRight |> FaceTrafoMessage )] [text "3"]
                                                                                | _ -> yield button [clazz "ui button"; onClick ( fun _ -> TransformFace.SelectFace FaceRight |> FaceTrafoMessage )] [text "3"]

                                                                                match selFace with
                                                                                | Some FaceBack -> yield button [clazz "ui button green"; onClick ( fun _ -> TransformFace.SelectFace FaceBack |> FaceTrafoMessage )] [text "4"]
                                                                                | _ -> yield button [clazz "ui button"; onClick ( fun _ -> TransformFace.SelectFace FaceBack |> FaceTrafoMessage )] [text "4"]
                                                                            
                                                                                match selFace with
                                                                                | Some FaceLeft -> yield button [clazz "ui button green"; onClick ( fun _ -> TransformFace.SelectFace FaceLeft |> FaceTrafoMessage )] [text "5"]
                                                                                | _ -> yield button [clazz "ui button"; onClick ( fun _ -> TransformFace.SelectFace FaceLeft |> FaceTrafoMessage )] [text "5"]

                                                                                match selFace with
                                                                                | Some FaceBottom -> yield button [clazz "ui button green"; onClick ( fun _ -> TransformFace.SelectFace FaceBottom |> FaceTrafoMessage )] [text "6"]
                                                                                | _ -> yield button [clazz "ui button"; onClick ( fun _ -> TransformFace.SelectFace FaceBottom |> FaceTrafoMessage )] [text "6"]
                                                                            }
                                                                        )
                                                                ]
                                                            ]
                                                        ]
                                                else
                                                    yield
                                                        div [clazz "ui"] [
                                                            button [clazz "ui button blue"; onClick ( fun _ -> ChangeEditMode )] [text "Edit Faces"]
                                                            Utils.Html.toggleButton m.moveBoxMode "Move Box" "Cancel" ChangeMoveMode
                                                        ]
                                            }
                                        )

                                yield
                                    div [clazz "ui"][
                                        Html.SemUi.stuffStack [
                                            Utils.Html.toggleButton m.hideTrafoCtrls "Hide Trafocontrols" "Show Trafocontrols" ToggleHideTrafoCtrls |> UI.map liftMessage
                                            div [clazz "ui"][
                                                button [clazz "ui button blue"; onClick ( fun _ -> Unselect )] [text "Unselect"] |> UI.map liftMessage
                                                button [clazz "ui button red"; onClick ( fun _ -> RemoveBox )] [text "Delete"] |> UI.map liftMessage
                                            ]
                                        ]
                                        
                                        Html.SemUi.tabbed
                                            ([])
                                                ([
                                                    ("Values", BoxValues.view' box.values ( fun x -> Box.Action.ValuesMessage x |> BoxModelAction |> liftMessage ))
                                                    ("Edit", editDom |> UI.map liftMessage)
                                                    ("Rendering", RenderOptions.view' box.options ( fun x -> Box.Action.ChangeRenderOptions x |> BoxModelAction |> liftMessage ))
                                                ]) "Values"

                                        Html.table [
                                            Html.row "TrafoKind" [
                                                Html.SemUi.dropDown m.boxTrafo.trafoKind ChangeTrafoKind |> UI.map ( fun x -> liftMessage x )
                                            ]
                                            Html.row "TrafoMode" [
                                                Html.SemUi.dropDown m.trafoMode ChangeTrafoMode |> UI.map ( fun x -> liftMessage x )
                                            ]
                                        ]
                                    ]
                        })
            
            //general UI
            let generalUI =
                div [clazz "ui"] [
                    Html.table [
                        Html.row "Scene Name: " [
                            Utils.Html.textInputColorable m.sceneName C4b.Black SetSceneName
                        ]
                        Html.row "File: " [
                            openDialogButton
                                { OpenDialogConfig.file with allowMultiple = false; title = "Load Boxes Scene" }
                                [ clazz "ui button green"; onChooseFile Load ]
                                [ text "Load" ]
                        
                            openDialogButton
                                { OpenDialogConfig.folder with allowMultiple = false; title = "Save Boxes Scene"; mode = OpenDialogMode.Folder }
                                [ clazz "ui button green"; onChooseFile Save ]
                                [ text "Save" ]
                        ]
                        Html.row "New Scene: " [
                            button [clazz "ui button yellow"; onClick ( fun _ -> NewScene )][text "New"]
                        ]
                    ]
                    Html.table [
                        Html.row "Add Box: " [
                            Incremental.div
                                (AttributeMap.ofList[]) (
                                    alist {
                                        let! mode = m.addBoxMode
                                        if mode = AddBoxMode.Off
                                        then
                                            yield
                                                div [clazz "ui"] [
                                                    button [clazz "ui button blue"; onClick ( fun _ -> AddBoxMode.AddWithMouse |> ChangeAddBoxMode )][text "Mouse"]
                                                    button [clazz "ui button blue"; onClick ( fun _ -> AddBoxMode.AddFromPoints |> ChangeAddBoxMode )][text "PCA"]
                                                ]
                                    }
                                )
                            
                            Incremental.div
                                (AttributeMap.ofList[]) (
                                    alist {
                                        let! mode = m.addBoxMode
                                        if mode = AddBoxMode.AddWithMouse then yield button [clazz "ui button orange"; onClick ( fun _ -> Escape )][text "Cancel"]
                                        if mode = AddBoxMode.AddFromPoints
                                        then
                                            yield
                                                div [clazz "ui"][
                                                    button [clazz "ui button green"; onClick ( fun _ -> FinishBoxFromPoints )][text "Finish"]
                                                    button [clazz "ui button orange"; onClick ( fun _ -> Escape )][text "Cancel"]
                                                ]
                                    }
                                )
                        ]
                    ]
                ]
            
            let boxList =
                Html.table [
                    Incremental.div
                        (AttributeMap.ofList [clazz "ui divided list"]) (
                            alist {
                                for (id,box) in m.boxes |> AMap.toASet |> AList.ofASet do
                                    let! name = box.name
                                    let! c = box.color

                                    let bgc = sprintf "background: %s; color:#000000" (Html.ofC4b c)
                                    yield
                                        div [
                                            clazz "item"; style bgc
                                            onClick(fun _ -> SelectBox id)
                                        ] [
                                            text name
                                        ]
                            }
                        )
                ]
            
            yield
                div [clazz "ui"][
                    Html.SemUi.tabbed
                        ([])
                            ([
                                ("General", generalUI |> UI.map liftMessage)
                                ("Box", selBoxUI)
                                ("List", boxList |> UI.map liftMessage)
                            ]) "General"
                ]
        ]
