namespace PlaneExtrude

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.Rendering
open Aardvark.Rendering.Text
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
                Sg.triangles (C4b(100, 100, 160, 90) |> Mod.constant) triangles
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
    
    let normal (m : PlaneModel) =
        V3d.Cross(m.v1 - m.v0, m.v2 - m.v0)

    let mkTrafo (m : PlaneModel) =
        let normal = V3d.Cross(m.v1 - m.v0, m.v2 - m.v0).Normalized
        let c = m |> center
        let rot = Rot3d.FromM33d(Trafo3d.RotateInto(V3d.OOI, normal).Forward.UpperLeftM33())
        let pose = {Pose.translate c with rotation = rot}
        {TrafoController.initial with pose = pose; previewTrafo = Pose.toTrafo pose; mode = TrafoMode.Local}
    
    let setup v0 v1 v2 v3 group above below =
        {
            v0    = v0
            v1    = v1
            v2    = v2
            v3    = v3
            group = group
            above = above
            below = below
            id    = System.Guid.NewGuid().ToString()
        }

module PELine =

    let setup (startPlane : PlaneModel) (endPlane : PlaneModel) (group : int) (side : LineSide) =
        {
            startPlane  = startPlane
            endPlane    = endPlane
            group       = group
            side        = side
        }

    let mkSg (m : MLineModel) (view : IMod<CameraView>) =
        adaptive {
            let! v = view
            let camPos = v.Location

            let! side = m.side
            let! sv1 =
                match side with
                | LEFT  -> m.startPlane.v0
                | RIGHT -> m.startPlane.v1
            
            let! sv2 =
                match side with
                | LEFT  -> m.startPlane.v3
                | RIGHT -> m.startPlane.v2
                
            let! ev1 =
                match side with
                | LEFT  -> m.endPlane.v0
                | RIGHT -> m.endPlane.v1
            
            let! ev2 =
                match side with
                | LEFT  -> m.endPlane.v3
                | RIGHT -> m.endPlane.v2

            let s = (sv1 + sv2) * 0.5
            let e = (ev1 + ev2) * 0.5
            let c = (s + e) * 0.5
            let l = Line3d(s,e)
            let lines = [|l|] |> Mod.constant

            let color =
                match side with
                | LEFT  -> C4b.VRVisGreen
                | RIGHT -> C4b.WHITE

            let d = (e - s).Length
            let font = Font.create "arial" FontStyle.Regular
            let content = d |> sprintf "%.2fm" |> Mod.constant

            let camDist = (c - camPos).Length
            let scale = Fun.Max(0.05, camDist / 52.0)
            let labelTrafo = Trafo3d.Scale(scale) * Trafo3d.Translation(c) |> Mod.constant

            let labelSg =
                Sg.text font color content
                |> Sg.billboard
                |> Sg.noEvents
                |> Sg.trafo labelTrafo
            
            return
                Sg.lines (color |> Mod.constant) lines
                |> Sg.noEvents
                |> Sg.shader {
                    do! DefaultSurfaces.trafo
                    do! DefaultSurfaces.vertexColor
                }
                |> Sg.andAlso labelSg
        }
        |> Sg.dynamic
        |> Sg.depthTest (DepthTestMode.Always |> Mod.constant)
    
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

    let orderPlanes (group : list<PlaneModel>) =
        group
        |> List.mapi ( fun i x ->
            let mutable above = 0
            let mutable below = 0

            for item in group do
                if not (item.id = x.id)
                then
                    let n = x |> Plane.normal
                    let c = x |> Plane.center
                    let a = item |> Plane.center
                    let d = V3d.Dot(n, a-c)
                    if d > 0.0
                    then
                        above <- above + 1
                    else if d < 0.0 then
                        below <- below + 1

            {x with above = above; below = below}
        )
        |> List.sortBy ( fun x ->
            x.above
        )
    
    let setupLines (order : list<PlaneModel>) =
        [
            for i in 0..order.Length-2 do
                let sp = order.[i]
                let ep = order.[i+1]
                let line = PELine.setup sp ep sp.group LineSide.RIGHT
                yield line
            
            if order.Length >= 2
            then
                yield PELine.setup order.[0] order.[order.Length-1] order.[0].group LineSide.LEFT
        ]

    let rec update (m : Model) (a : Action) =
        match a with
        | Select id ->
            let p = m.planeModels.AsList |> List.find (fun x -> x.id = id)
            let trafo = p |> Plane.mkTrafo
            match m.selected with
            | None -> {m with selected = Some id; trafo = trafo}
            | Some sel ->
                match id = sel with
                | true  -> {m with selected = None; trafo = trafo} //Unselect
                | false -> {m with selected = Some id; trafo = trafo}
            
        | PointsMsg msg ->
            {m with pointsModel = Utils.Picking.update m.pointsModel msg}
        | FinishPoints -> //adds a new group with the first plane
            let pts = m.pointsModel.points
            if pts.Count < 3
            then m
            else
                let vecs = pts |> PList.toList |> List.map( fun x -> x.pos ) |> Boxes.PCA.pca
                let eig0 = vecs.[0]
                let eig1 = vecs.[1]
                let eig2 = vecs.[2]

                let p0 = pts.[0].pos
                let p1 = pts.[1].pos

                let v0 = p0 + (eig1*0.25)
                let v1 = p1 + (eig1*0.25)
                let v2 = p1 - (eig1*0.25)
                let v3 = p0 - (eig1*0.25)
                
                let plane = Plane.setup v0 v1 v2 v3 (m.maxGroupId+1) 0 0
                
                let planeModels =
                    m.planeModels
                    |> PList.append plane
                
                update {m with pointsModel = Utils.Picking.Action.Reset |> Utils.Picking.update m.pointsModel; addMode = false; planeModels = planeModels; maxGroupId = m.maxGroupId+1} (Select plane.id)
        
        | ToggleAddMode ->
            {m with addMode = not m.addMode; pointsModel = Utils.Picking.Action.Reset |> Utils.Picking.update m.pointsModel; extrudeMode = false}
        | ToggleExtrudeMode ->
            if m.addMode
            then m
            else {m with extrudeMode = not m.extrudeMode}
        
        | AddPlane -> //adds a new plane to the selected group
            match m.selected with
            | Some id ->
                let planes = m.planeModels |> PList.toList
                let p = planes |> List.find (fun x -> x.id = id)
                let offset = (p |> Plane.normal).Normalized * 0.001
                let newPlane = Plane.setup (p.v0 + offset) (p.v1 + offset) (p.v2 + offset) (p.v3 + offset) p.group 0 0
                let planeModels = m.planeModels |> PList.append newPlane
                let trafo = m.trafo

                let group = planeModels |> PList.toList |> List.filter ( fun x -> p.group = x.group )
                let order = group |> orderPlanes
                
                let planeModels =
                    planeModels
                    |> PList.toList
                    |> List.except group
                    |> List.append order
                    |> PList.ofList
                
                let lineGroup =
                    m.lineModels
                    |> PList.toList
                    |> List.filter ( fun x ->
                        x.group = p.group
                    )

                let lineModels =
                    m.lineModels
                    |> PList.toList
                    |> List.except lineGroup
                    |> List.append (
                        order |> setupLines
                    )
                    |> PList.ofList
                
                {m with planeModels = planeModels; lineModels = lineModels; selected = Some newPlane.id; trafo = trafo}  
            | None -> m
        
        | RemovePlane ->
            //TODO: new ordering and setup of lines
            match m.selected with
            | Some id ->
                let planes = m.planeModels |> PList.toList
                let p = planes |> List.find (fun x -> x.id = id)
                let planeModels =
                    m.planeModels
                    |> PList.toList
                    |> List.except [p]
                    |> PList.ofList
                
                let group = planeModels |> PList.toList |> List.filter ( fun x -> p.group = x.group )
                let order = group |> orderPlanes
                
                let planeModels =
                    planeModels
                    |> PList.toList
                    |> List.except group
                    |> List.append order
                    |> PList.ofList
                
                let lineGroup =
                    m.lineModels
                    |> PList.toList
                    |> List.filter ( fun x ->
                        x.group = p.group
                    )

                let lineModels =
                    m.lineModels
                    |> PList.toList
                    |> List.except lineGroup
                    |> List.append (
                        order |> setupLines
                    )
                    |> PList.ofList
                
                {m with planeModels = planeModels; lineModels = lineModels; selected = None}
            | None -> m

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
                
                let planes = planeModels |> PList.toList
                let p = planes |> List.find (fun x -> x.id = id)
                let group = planeModels |> PList.toList |> List.filter ( fun x -> p.group = x.group )
                let order = group |> orderPlanes
                
                let planeModels =
                    planeModels
                    |> PList.toList
                    |> List.except group
                    |> List.append order
                    |> PList.ofList
                
                let lineGroup =
                    m.lineModels
                    |> PList.toList
                    |> List.filter ( fun x ->
                        x.group = p.group
                    )

                let lineModels =
                    m.lineModels
                    |> PList.toList
                    |> List.except lineGroup
                    |> List.append (
                        order |> setupLines
                    )
                    |> PList.ofList

                {m with trafo = TranslateController.updateController m.trafo msg; planeModels = planeModels; lineModels = lineModels}
            | None -> m
        
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
        
        let linesSg =
            aset {
                for line in m.lineModels |> AList.toASet do
                    yield PELine.mkSg line view
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
            |> Sg.depthTest (DepthTestMode.Always |> Mod.constant)

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
        
        [sg; linesSg; trafoctrl; pointsSg]
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
            lineModels  = PList.empty
            selected    = None
            trafo       = TrafoController.initial
            maxGroupId  = 0
            id          = System.Guid.NewGuid().ToString()
        }
