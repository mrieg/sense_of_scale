namespace PlaneExtrude

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.Rendering
open Aardvark.UI
open Aardvark.UI.Trafos
open Aardvark.UI.Primitives

module Plane =

    type Action =
        | Transform of Trafo3d
    
    let update (m : PlaneModel) (act : Action) =
        match act with
        | Transform t ->
            let f = t.Forward.TransformPos
            {m with v0 = m.v0 |> f; v1 = m.v1 |> f; v2 = m.v2 |> f; v3 = m.v3 |> f}

    let mkSg (m : MPlaneModel) events =
        adaptive {
            let! v0 = m.v0
            let! v1 = m.v1
            let! v2 = m.v2
            let! v3 = m.v3

            let tri0 = Triangle3d(v0, v1, v2)
            let tri1 = Triangle3d(v0, v2, v3)
            let triangles = [|tri0; tri1|] |> Mod.constant
            
            let kdTree = Geometry.KdTree.build Geometry.Spatial.triangle Geometry.KdBuildInfo.Default [|tri0; tri1|]
            
            return
                Sg.triangles m.color triangles
                |> Sg.noEvents
                |> Sg.pickable (kdTree |> PickShape.Triangles)
        }
        |> Sg.dynamic
        |> Sg.requirePicking
        |> Sg.noEvents
        |> Sg.withEvents events
        |> Sg.shader {
            do! DefaultSurfaces.trafo
            do! DefaultSurfaces.vertexColor
        }
    
    let center (m : PlaneModel) =
        (m.v0 + m.v1 + m.v2 + m.v3) * 0.25

    let mkTrafo (m : PlaneModel) =
        let normal = V3d.Cross(m.v1 - m.v0, m.v2 - m.v0).Normalized
        let c = m |> center
        let rot = Rot3d.FromM33d(Trafo3d.RotateInto(V3d.OOI, normal).Forward.UpperLeftM33())
        let pose = {Pose.translate c with rotation = rot}
        {TrafoController.initial with pose = pose; previewTrafo = Pose.toTrafo pose; mode = TrafoMode.Local}
    
    let setup v0 v1 v2 v3 color =
        {
            v0    = v0
            v1    = v1
            v2    = v2
            v3    = v3
            color = color
            id    = System.Guid.NewGuid().ToString()
        }
    
    let fromLineAndNormal (line : Line3d) (normal : V3d) (color : C4b) =
        let offset = V3d.Cross(normal.Normalized, line.Direction.Normalized) * 0.5
        let v0     = line.P0
        let v1     = line.P1
        let v2     = v1 + offset
        let v3     = v0 + offset
        {
            v0    = v0
            v1    = v1
            v2    = v2
            v3    = v3
            color = color
            id    = System.Guid.NewGuid().ToString()
        }
    
module App =

    type Action =
        | Select of string
        | PointsMsg of Utils.Picking.Action
        | FinishPoints
        | ToggleAddMode
        | ToggleExtrudeMode
        | AddPlane
        | RemovePlane
        | TranslateCtrlMsg of TrafoController.Action
        | OnKeyDown of Aardvark.Application.Keys
    
    let trafoGrabbed (m : Model) = m.trafo.grabbed.IsSome

    let rec update (m : Model) (a : Action) =
        match a with
        | Select id ->
            let p = m.planeModels.AsList |> List.find (fun x -> x.id = id)
            let trafo = p |> Plane.mkTrafo
            {m with selected = Some id; trafo = trafo}
        | PointsMsg msg -> {m with pointsModel = Utils.Picking.update m.pointsModel msg}
        | FinishPoints ->
            let pts = m.pointsModel.points
            if pts.Count < 2
            then m
            else
                let plane = (Plane.fromLineAndNormal (Line3d(V3d(0.0, 0.0, 0.0), V3d(1.0, 1.0, 1.0))) (V3d.IOO) (C4b.White))
                let planeModels =
                    m.planeModels
                    |> PList.append plane
                update {m with pointsModel = Utils.Picking.Action.Reset |> Utils.Picking.update m.pointsModel; addMode = false; planeModels = planeModels} (Select plane.id)
        
        | ToggleAddMode ->
            {m with addMode = not m.addMode; pointsModel = Utils.Picking.Action.Reset |> Utils.Picking.update m.pointsModel; extrudeMode = false}
        | ToggleExtrudeMode ->
            if m.addMode
            then m
            else {m with extrudeMode = not m.extrudeMode}
        
        | AddPlane ->
            match m.selected with
            | Some id ->
                let p = m.planeModels |> PList.toList |> List.find (fun x -> x.id = id)
                let newPlane = Plane.setup p.v0 p.v1 p.v2 p.v3 C4b.Red
                let trafo = m.trafo
                let planeModels = m.planeModels |> PList.append newPlane
                {m with planeModels = planeModels; selected = Some newPlane.id; trafo = trafo}
            | None ->
                m
        | RemovePlane ->
            match m.selected with
            | Some id ->
                let p = m.planeModels |> PList.toList |> List.find (fun x -> x.id = id)
                let planeModels =
                    m.planeModels
                    |> PList.toList
                    |> List.except [p]
                    |> PList.ofList
                {m with planeModels = planeModels; selected = None}
            | None ->
                m

        | TranslateCtrlMsg msg ->
            match m.selected with
            | Some id ->
                let planeModels =
                    m.planeModels
                    |> PList.map (fun x ->
                        match x.id = id with
                        | true  ->
                            let pc = x |> Plane.center
                            let tc = m.trafo.pose.position + ((m.trafo.pose |> Pose.toRotTrafo).Forward.TransformPos(m.trafo.workingPose.position))
                            let t = tc - pc
                            let a = Trafo3d.Translation(t) |> Plane.Transform
                            Plane.update x a
                        | false -> x
                    )
                {m with trafo = TranslateController.updateController m.trafo msg; planeModels = planeModels;}
            | None ->
                m
        
        | OnKeyDown key ->
            match key with
            | Aardvark.Application.Keys.Space ->
                update m AddPlane
            | Aardvark.Application.Keys.Delete ->
                update m RemovePlane
            | Aardvark.Application.Keys.Enter ->
                if m.addMode
                then FinishPoints |> update m
                else m
            | Aardvark.Application.Keys.Escape ->
                if m.addMode
                then update m ToggleAddMode
                else if m.extrudeMode
                then update m ToggleExtrudeMode
                else m
            | _ -> m
    
    let viewScene' (m : MModel) (view : IMod<CameraView>) (pickSg) (liftMessage : Action -> 'msg) =
        let sg =
            aset {
                for pm in m.planeModels |> AList.toASet do
                    let events = [Sg.onDoubleClick (fun _ -> pm.id |> Action.Select |> liftMessage)]
                    yield Plane.mkSg pm events
            }
            |> Sg.set
        
        let trafoctrl =
            m.selected
            |> Mod.map ( fun s ->
                match s with
                | None -> Sg.empty
                | Some id -> TranslateController.viewController (fun x -> x |> TranslateCtrlMsg |> liftMessage) view m.trafo
            )
            |> Sg.dynamic

        let points = Utils.Picking.mkSg m.pointsModel view (fun x -> x |> PointsMsg |> liftMessage)
        
        let events = [Sg.onDoubleClick ( fun pos -> pos |> Utils.Picking.Action.AddPoint |> PointsMsg |> liftMessage )]
        let pickSg = pickSg events
        let pointsSg =
            m.addMode
            |> Mod.map ( fun am ->
                if am
                then pickSg |> Sg.andAlso points
                else Sg.empty
            )
            |> Sg.dynamic
        
        [sg; trafoctrl; pointsSg]
        |> Sg.ofList
    
    let view' (m : MModel) =
        div [][
            Html.SemUi.stuffStack [
                Utils.Html.toggleButton m.addMode "Add" "Cancel" ToggleAddMode
            ]
        ]
    
    let initial =
        {
            addMode     = false
            extrudeMode = false
            pointsModel = Utils.Picking.initial
            planeModels = PList.empty
            selected    = None
            trafo       = TrafoController.initial
            id          = System.Guid.NewGuid().ToString()
        }