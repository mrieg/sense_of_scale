namespace BoxManip

open System

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.Rendering
open Aardvark.Rendering.Text
open Aardvark.UI
open Aardvark.UI.Primitives
open Aardvark.UI.Trafos

(*
    KEYS:
	C: Unselect Corner
	B: Unselect Box
	E: EditMode on/off
	1: TranslationControl
	2: RotationControl
*)

module BoxManip =

    let createBox (min : V3d) (max : V3d) (rot : Rot3d) =
        let minPose = Pose.translate min
        let maxPose = Pose.translate max
            
        let minTrafo =
            {
                TrafoController.initial with
                    pose            = minPose
                    previewTrafo    = Pose.toTrafo minPose
                    mode            = TrafoMode.Local
            }

        let maxTrafo =
            {
                TrafoController.initial with
                    pose            = maxPose
                    previewTrafo    = Pose.toTrafo maxPose
                    mode            = TrafoMode.Local
            }
            
        let center = (min + max) * 0.5
        let boxPose = {(Pose.translate center) with rotation = rot}
        let boxTrafo =
            {
                TrafoController.initial with
                    pose            = boxPose
                    previewTrafo    = Pose.toTrafo boxPose
                    mode            = TrafoMode.Global
            }
            
        {
            id      = System.Guid.NewGuid().ToString()
            min     = {t = Min; trafo = minTrafo}
            max     = {t = Max; trafo = maxTrafo}
            trafo   = boxTrafo
            edges   =
                HSet.ofList [
                    {t = XEdge; pos = LFB_RFB}
                    {t = YEdge; pos = RFB_RBB}
                    {t = XEdge; pos = RBB_LBB}
                    {t = YEdge; pos = LBB_LFB}
                    {t = XEdge; pos = LFT_RFT}
                    {t = YEdge; pos = RFT_RBT}
                    {t = XEdge; pos = RBT_LBT}
                    {t = YEdge; pos = LBT_LFT}
                    {t = ZEdge; pos = LFB_LFT}
                    {t = ZEdge; pos = RFB_RFT}
                    {t = ZEdge; pos = RBB_RBT}
                    {t = ZEdge; pos = LBB_LBT}
                ]
        }

    let initial =
        let boxInitial (minVec : V3d) (maxVec : V3d) =
            let b = createBox minVec maxVec (Rot3d.FromAngleAxis( V3d(0.0, 0.0, 0.0) ))
            HMap.add b.id b HMap.empty

        let rec initNBoxes total n =
            let trans = V3d(float n - (float total / 2.0), 0.0, 0.0) * 2.0
            let minVec = -V3d.IIO * 0.5 + trans
            let maxVec = V3d.III * 0.5 + trans
            match n with
            | 0 -> HMap.empty
            | x when x < 0 -> HMap.empty
            | _ -> HMap.union (boxInitial minVec maxVec) (initNBoxes total (n-1))

        {
            boxes           = initNBoxes 1 1
            selectedBoxes   = HSet.empty
            selectedCorner  = None
            hoveredCorner   = None
            hoveredEdge     = None
            hoveredBox      = None
            editMode        = false
            cullMode        = CullMode.None
            enableBlending  = false
            trafoKind       = TrafoKind.Translate
            camera          = CameraController.initial
        }
    
    type BoxManipAction =
        | CameraMessage     of CameraController.Message
        | SelectBox         of string
        | SelectCorner      of CornerType
        | EnterCorner       of CornerType
        | ExitCorner
        | TranslateCorner   of CornerType * string * TrafoController.Action
        | TranslateBox      of string * TrafoController.Action
        | KeyDown           of key : Aardvark.Application.Keys
        | UnselectBox
        | UnselectCorner
        | RotateBox         of string * TrafoController.Action
        | EnterEdge         of EdgePos
        | ExitEdge
        | SetKind           of TrafoKind
        | ChangeCullMode    of CullMode
        | ToggleEditMode
        | EnterBox          of string
        | ExitBox
        | EnableBlending
    
    let isGrabbed (world : World) =
        world.selectedBoxes |> HSet.exists (fun name -> 
            match HMap.tryFind name world.boxes with
                | Some b ->
                    match b.trafo.grabbed.IsSome with
                    | true -> true
                    | false ->
                        match world.selectedCorner with
                        | Some corner ->
                            match corner with
                            | Min -> b.min.trafo.grabbed.IsSome
                            | Max -> b.max.trafo.grabbed.IsSome
                        | None -> false
                | None -> false
        )
    
    let updateBoxCorner (world : World) (c : CornerType) (id : string) (act : TrafoController.Action) =
        let updateCorner (box : BoundingBox) (corner : Corner) =
            let updateTrafo = TranslateController.updateController corner.trafo act
            let newPos = updateTrafo.pose.position + updateTrafo.workingPose.position
            let newPose = {corner.trafo.pose with position = newPos}
            let newTrafo =
                {
                    corner.trafo with
                        pose            = newPose
                        previewTrafo    = Pose.trafo newPose
                        hovered         = updateTrafo.hovered
                        grabbed         = updateTrafo.grabbed
                }
            
            let newCorner = {corner with trafo = newTrafo}

            match act with
            | TrafoController.Action.MoveRay _ ->
                match corner.t with
                | Min -> Some {box with min = newCorner}
                | Max -> Some {box with max = newCorner}

            | TrafoController.Action.Release ->
                match corner.t with
                | Min -> Some {box with min = newCorner}
                | Max -> Some {box with max = newCorner}

            | _ ->
                match corner.t with
                | Min -> Some {box with min = {box.min with trafo = {box.min.trafo with hovered = updateTrafo.hovered; grabbed = updateTrafo.grabbed}}}
                | Max -> Some {box with max = {box.max with trafo = {box.max.trafo with hovered = updateTrafo.hovered; grabbed = updateTrafo.grabbed}}}
                
        world.boxes
            |> HMap.alter id (fun b ->
                match b with
                | Some box ->
                    match c with
                    | Min ->
                        let corner = box.min
                        updateCorner box corner
                    | Max ->
                        let corner = box.max
                        updateCorner box corner
                | None -> b
            )

    let rec update (world : World) (a : BoxManipAction) =
        match a with
        | CameraMessage msg ->
            match isGrabbed world with
            | true -> world
            | false -> {world with camera = CameraController.update world.camera msg}
        
        | SelectBox id ->
            match HSet.contains id world.selectedBoxes with
            | true -> {world with selectedBoxes = HSet.empty; selectedCorner = None; editMode = false}
            | false -> {world with selectedBoxes = HSet.add id HSet.empty; selectedCorner = None; editMode = false}
        
        | SelectCorner c ->
            match world.selectedCorner with
            | Some sc ->
                match sc = c with
                | true -> {world with selectedCorner = None}
                | false -> {world with selectedCorner = Some c}
            | None -> {world with selectedCorner = Some c}
        
        | EnterCorner c -> {world with hoveredCorner = Some c}
        | ExitCorner -> {world with hoveredCorner = None}

        | TranslateCorner (c, id, act) ->
            match HSet.contains id world.selectedBoxes with
            | true ->
                let newMap = updateBoxCorner world c id act
                {world with boxes = newMap}
            | false -> world
        
        | TranslateBox (id, act) ->
            match world.selectedBoxes |> HSet.contains id with
            | true ->
                let newMap =
                    world.boxes
                    |> HMap.alter id ( fun b ->
                        match b with
                        | Some box -> Some {box with trafo = TranslateController.updateController box.trafo act}
                        | None -> None
                    )
                {world with boxes = newMap}
            | false -> world
        
        | RotateBox (id, act) ->
            match world.selectedBoxes |> HSet.contains id with
            | true ->
                let newMap =
                    world.boxes
                    |> HMap.alter id ( fun b ->
                        match b with
                        | Some box -> Some {box with trafo = RotationController.updateController box.trafo act}
                        | None -> None
                    )
                {world with boxes = newMap}
            | false -> world

        | KeyDown key ->
            match key with
            | Aardvark.Application.Keys.C -> {world with selectedCorner = None}
            | Aardvark.Application.Keys.B -> update world UnselectBox
            | Aardvark.Application.Keys.E -> update world ToggleEditMode
            | Aardvark.Application.Keys.D1 -> {world with trafoKind = TrafoKind.Translate}
            | Aardvark.Application.Keys.D2 -> {world with trafoKind = TrafoKind.Rotate}
            | _ -> world
        
        | UnselectBox -> {world with selectedBoxes = HSet.empty; selectedCorner = None; editMode = false}
        | UnselectCorner -> {world with selectedCorner = None}
        
        | EnterEdge edgePos ->
            match isGrabbed world with
            | true -> {world with hoveredEdge = None}
            | false ->
                match world.selectedCorner with
                | Some _ -> {world with hoveredEdge = None}
                | None -> {world with hoveredEdge = Some edgePos}
        
        | ExitEdge -> {world with hoveredEdge = None}

        | SetKind k -> {world with trafoKind = k}
        | ChangeCullMode cm -> {world with cullMode = cm}
        | ToggleEditMode ->
            match world.selectedBoxes.IsEmpty with
                | true -> world
                | false -> {world with editMode = not world.editMode; selectedCorner = None}
        
        | EnterBox id ->
            match world.selectedBoxes.Contains id with
            | true -> {world with hoveredBox = None}
            | false -> {world with hoveredBox = Some id}
        | ExitBox -> {world with hoveredBox = None}
        | EnableBlending -> {world with enableBlending = not world.enableBlending}
    
    let mkCornerController (world : MWorld) (b : MBoundingBox) (c : MCorner) =
        adaptive {
            let! selCorner = world.selectedCorner
            let! cornerType = c.t
            let! boxId = b.id

            let ctrl = TranslateController.viewController ( fun t -> TranslateCorner (cornerType, boxId, t) ) world.camera.view c.trafo

            let! boxIsSelected = ASet.contains (boxId) world.selectedBoxes
            let sg =
                match boxIsSelected with
                | true ->
                    match selCorner with
                    | Some selCornerType ->
                        match selCornerType = cornerType with
                        | true -> ctrl
                        | false -> Sg.empty
                    | None -> Sg.empty
                | false -> Sg.empty

            return sg
        } |> Sg.dynamic
    
    let mkCornerSphere (world : MWorld) (b : MBoundingBox) (c : MCorner) =
        let col =
            world.hoveredCorner |> Mod.bind ( fun x ->
                match x with
                | None -> Mod.constant C4b.Gray
                | Some ct ->
                    match (c.t |> Mod.force) = ct with
                    | true -> Mod.constant C4b.Green
                    | false -> Mod.constant C4b.Gray
            )
        
        let radius =
            world.hoveredCorner |> Mod.bind ( fun x ->
                match x with
                | None -> Mod.constant 0.1
                | Some ct ->
                    match (c.t |> Mod.force) = ct with
                    | true -> Mod.constant 0.2
                    | false -> Mod.constant 0.1
            )

        let sphereSg =
            Sg.sphere 4 col radius
            |> Sg.requirePicking
            |> Sg.noEvents
            |> Sg.withEvents [
                Sg.onEnter ( fun _ -> EnterCorner (c.t |> Mod.force) )
                Sg.onLeave ( fun () -> ExitCorner )
                Sg.onDoubleClick ( fun _ -> SelectCorner (c.t |> Mod.force) )
            ]
            |> Sg.effect [
                DefaultSurfaces.trafo |> toEffect
                DefaultSurfaces.vertexColor |> toEffect
            ]
        
        adaptive {
            let! crn = world.selectedCorner
            let! boxId = b.id
            let! boxIsSel = ASet.contains boxId world.selectedBoxes

            let sg =
                match boxIsSel with
                | false -> Sg.empty
                | true ->
                    match crn with
                    | None -> sphereSg
                    | Some cornerType ->
                        match cornerType = (c.t |> Mod.force) with
                        | false -> sphereSg
                        | true -> Sg.empty

            return
                sg
                |> Sg.trafo c.trafo.previewTrafo

        } |> Sg.dynamic
    
    let mkBoxController (world : MWorld) (b : MBoundingBox) =
        adaptive {
            let! kind = world.trafoKind
            let! boxId = b.id
            let! cornerSelected = world.selectedCorner

            let transCtrl = TranslateController.viewController ( fun a -> TranslateBox (boxId, a) ) world.camera.view b.trafo
            let rotCtrl = RotationController.viewController ( fun a -> RotateBox (boxId, a) ) world.camera.view b.trafo

            let! boxIsSel = ASet.contains boxId world.selectedBoxes
            let outSg =
                match boxIsSel with
                | true ->
                    match kind with
                    | TrafoKind.Translate -> transCtrl
                    | TrafoKind.Rotate -> rotCtrl
                    | _ -> Sg.empty
                | false -> Sg.empty
            
            let outSg =
                match cornerSelected with
                | Some c -> Sg.empty
                | None -> outSg
            
            return outSg
        } |> Sg.dynamic
    
    let mkHoverLabel (world : MWorld) (b : MBoundingBox) (startPos : IMod<V3d>) (endPos : IMod<V3d>) =
        adaptive {
            let! sp = startPos
            let! ep = endPos
            let dist = V3d.Distance(sp, ep)
            let str = Mod.constant (sprintf "%.2f" dist)

            let trans = Mod.constant ( Trafo3d.Translation( (sp + ep) * 0.5 ) )
            let scaleTrafo = Mod.constant ( Trafo3d.Scale(0.22) )

            let invRot =
                b.trafo.pose
                |> Mod.map ( fun p ->
                    (Pose.toRotTrafo p).Inverse
                )
            
            return
                Sg.text (Font.create "helvetica" FontStyle.Regular) C4b.White str
                |> Aardvark.Rendering.Text.Sg.billboard
                |> Sg.noEvents
                |> Sg.trafo invRot
                |> Sg.trafo scaleTrafo
                |> Sg.trafo trans
                |> Sg.depthTest ( Mod.constant DepthTestMode.Always )
        } |> Sg.dynamic

    let mkHoverEdge (world : MWorld) (b : MBoundingBox) (e : MEdge) =
        let startPos =
            Mod.map2 ( fun x y ->
                let min = x.position
                let max = y.position
                match e.pos |> Mod.force with
                | LFB_RFB -> V3d(min.X, max.Y, min.Z)
                | RFB_RBB -> V3d(min.X, max.Y, min.Z)
                | RBB_LBB -> V3d(min.X, min.Y, min.Z)
                | LBB_LFB -> V3d(max.X, min.Y, min.Z)
                | LFT_RFT -> V3d(min.X, max.Y, max.Z)
                | RFT_RBT -> V3d(min.X, max.Y, max.Z)
                | RBT_LBT -> V3d(min.X, min.Y, max.Z)
                | LBT_LFT -> V3d(max.X, min.Y, max.Z)
                | LFB_LFT -> V3d(max.X, max.Y, min.Z)
                | RFB_RFT -> V3d(min.X, max.Y, min.Z)
                | RBB_RBT -> V3d(min.X, min.Y, min.Z)
                | LBB_LBT -> V3d(max.X, min.Y, min.Z)
            ) b.min.trafo.pose b.max.trafo.pose
        
        let endPos =
            Mod.map2 ( fun x y ->
                let min = x.position
                let max = y.position
                match e.pos |> Mod.force with
                | LFB_RFB -> V3d(max.X, max.Y, min.Z)
                | RFB_RBB -> V3d(min.X, min.Y, min.Z)
                | RBB_LBB -> V3d(max.X, min.Y, min.Z)
                | LBB_LFB -> V3d(max.X, max.Y, min.Z)
                | LFT_RFT -> V3d(max.X, max.Y, max.Z)
                | RFT_RBT -> V3d(min.X, min.Y, max.Z)
                | RBT_LBT -> V3d(max.X, min.Y, max.Z)
                | LBT_LFT -> V3d(max.X, max.Y, max.Z)
                | LFB_LFT -> V3d(max.X, max.Y, max.Z)
                | RFB_RFT -> V3d(min.X, max.Y, max.Z)
                | RBB_RBT -> V3d(min.X, min.Y, max.Z)
                | LBB_LBT -> V3d(max.X, min.Y, max.Z)
            ) b.min.trafo.pose b.max.trafo.pose

        let cylHeight =
            Mod.map2 (fun (x : V3d) (y : V3d) ->
                V3d.Distance(x, y)
            ) startPos endPos
        
        let cylinderRadius = Mod.constant 0.01
        let pickingRadius = 0.08
        let cylinder = Sg.cylinder 7 (Mod.constant C4b.Yellow) cylinderRadius cylHeight

        let rotation =
            Mod.map2 (fun (sPos : V3d) (ePos : V3d) ->
                match e.t |> Mod.force with
                | XEdge ->
                    match sPos.X <= ePos.X with
                    | true -> Trafo3d.RotationYInDegrees(90.0)
                    | false -> Trafo3d.RotationYInDegrees(-90.0)
                | YEdge ->
                    match sPos.Y <= ePos.Y with
                    | true -> Trafo3d.RotationXInDegrees(-90.0)
                    | false -> Trafo3d.RotationXInDegrees(90.0)
                | ZEdge ->
                    match sPos.Z <= ePos.Z with
                    | true -> Trafo3d.Identity
                    | false -> Trafo3d.RotationXInDegrees(-180.0)
            ) startPos endPos
        
        let pickGraph =
            Sg.empty
            |> Sg.pickable (PickShape.Cylinder (Cylinder3d(startPos |> Mod.force, endPos |> Mod.force, pickingRadius)))
            |> Sg.requirePicking
            |> Sg.noEvents
            |> Sg.withEvents [
                Sg.onEnter ( fun _ -> EnterEdge (e.pos |> Mod.force))
                Sg.onLeave ( fun () -> ExitEdge)
            ]

        let sg =
            cylinder
            |> Sg.noEvents
            |> Sg.trafo rotation
            |> Sg.trafo ( startPos |> Mod.map ( fun (x : V3d) -> Trafo3d.Translation(x) ) )
            |> Sg.effect [
                DefaultSurfaces.trafo |> toEffect
                DefaultSurfaces.vertexColor |> toEffect
            ]
        
        let label = mkHoverLabel world b startPos endPos

        adaptive {
            let! boxId = b.id
            let! isSel = ASet.contains boxId world.selectedBoxes
            let! edgeHovered = world.hoveredEdge
            let! cornerSel = world.selectedCorner

            let outSg =
                match isSel with
                | false -> Sg.empty
                | true ->
                    match cornerSel with
                    | Some _ -> Sg.empty
                    | None ->
                        match edgeHovered with
                        | None -> pickGraph
                        | Some edge ->
                            match edge = (e.pos |> Mod.force) with
                            | false -> Sg.empty
                            | true ->
                                pickGraph
                                |> Sg.andAlso sg
                                |> Sg.andAlso label
            
            return outSg
        } |> Sg.dynamic

    let mkHoverEdges (world : MWorld) (b : MBoundingBox) =
        aset {
            for e in b.edges do
                yield mkHoverEdge world b e
        } |> Sg.set
    
    let mkFixedLabels (box : Box3d) (vp : V3d) (invRot : IMod<Trafo3d>) (prev : Trafo3d) =
        let vpTrafo = Trafo3d.Translation(-box.Center) * prev
        let vpTrans = vpTrafo.Inverse.Forward.TransformPos(vp)
        let (x, y, z) = Silhouette.calcLabelEdges box vpTrans
        
        let mkOneLabel (str : string) (e : Line3d) =
            let pos = (e.P0 + e.P1) * 0.5
            let scale = Mod.constant ( Trafo3d.Scale(0.1) )
            let trafo = Mod.constant ( Trafo3d.Translation(pos) )

            Sg.text (Font.create "courier" FontStyle.Regular) C4b.White (Mod.constant str)
            |> Sg.billboard
            |> Sg.noEvents
            |> Sg.trafo invRot
            |> Sg.trafo scale
            |> Sg.trafo trafo
            |> Sg.trafo ( Mod.constant (Trafo3d.Translation(-box.Center)) )
            |> Sg.trafo ( Mod.constant prev )
        
        let xLabel =
            match x with
            | None -> Sg.empty
            | Some e ->
                let str = sprintf "X: %.2f" box.X.Size
                mkOneLabel str e
            
        let yLabel =
            match y with
            | None -> Sg.empty
            | Some e ->
                let str = sprintf "Y: %.2f" box.Y.Size
                mkOneLabel str e
        
        let zLabel =
            match z with
            | None -> Sg.empty
            | Some e ->
                let str = sprintf "Z: %.2f" box.Z.Size
                mkOneLabel str e
        
        let edgeSg =
            let lineX =
                match x with
                | Some e -> [e]
                | None -> []
            
            let lineY =
                match y with
                | Some e -> [e]
                | None -> []
            
            let lineZ =
                match z with
                | Some e -> [e]
                | None -> []
            
            let lines =
                [lineX; lineY; lineZ]
                |> List.concat
                |> Array.ofList
                |> Mod.constant
            
            Sg.lines (Mod.constant C4b.Yellow) lines
            |> Sg.noEvents
            |> Sg.trafo ( Mod.constant (Trafo3d.Translation(-box.Center)) )
            |> Sg.trafo ( Mod.constant prev )
            |> Sg.shader {
                do! DefaultSurfaces.trafo
                do! DefaultSurfaces.vertexColor
            }
        
        [xLabel; yLabel; zLabel; edgeSg]
        |> Sg.ofList
        |> Sg.depthTest ( Mod.constant DepthTestMode.Always )
    
    let mkBox (world : MWorld) (b : MBoundingBox) =
        let selectedColor = Mod.constant (C4b(155, 0, 0, 100))
        let editModeColor = Mod.constant (C4b(155, 135, 105, 100))
        let cornerSelectedColor = Mod.constant (C4b(0, 128, 0, 100))
        let hoverColor = C4b(222, 222, 200, 200)
        let unHoverColor = C4b(50, 100, 255, 100)

        let unselectedColor =
            adaptive {
                let! boxId = b.id
                let! sel = ASet.contains boxId world.selectedBoxes
                let! hoveredBox = world.hoveredBox

                return
                    match hoveredBox with
                    | Some id ->
                        match sel with
                        | true -> unHoverColor
                        | false ->
                            match boxId = id with
                            | true -> hoverColor
                            | false -> unHoverColor
                    | None -> unHoverColor
            }
        
        let adaptiveBox =
            adaptive {
                let! minPose = b.min.trafo.pose
                let! maxPose = b.max.trafo.pose

                let min = minPose.position
                let max = maxPose.position

                let x = min.X <= max.X
                let y = min.Y <= max.Y
                let z = min.Z <= max.Z
                let justTwo = ( not x && not y && z ) || ( not x && y && not z ) || ( x && not y && not z )

                let box =
                    match x && y && z with
                    | true -> Box3d(min, max)
                    | false ->
                        match justTwo with
                        | true -> Box3d(min, max)
                        | false -> Box3d(max, min)

                return box
            }
        
        let fixedLabelSg =
            ASet.contains (b.id |> Mod.force) world.selectedBoxes
            |> Mod.map ( fun x ->
                match x with
                | false -> Sg.empty
                | true ->
                    adaptive {
                        let! v = world.camera.view
                        let vp = v.Location

                        let! box = adaptiveBox
                        let! prev = b.trafo.previewTrafo

                        let invRot =
                            b.trafo.pose
                            |> Mod.map ( fun p ->
                                (Pose.toRotTrafo p).Inverse
                            )

                        return mkFixedLabels box vp invRot prev
                    } |> Sg.dynamic
            ) |> Sg.dynamic
        
        adaptive {
            let! boxId = b.id
            let! isSel = (ASet.contains boxId world.selectedBoxes)
            let! selCorner = world.selectedCorner
            let! editMode = world.editMode
            let! blending = world.enableBlending

            let color =
                match isSel with
                | true ->
                    match editMode with
                    | true ->
                        match selCorner with
                        | Some sc -> cornerSelectedColor
                        | None -> editModeColor
                    | false -> selectedColor
                | false -> unselectedColor
            
            let box =
                match isSel with
                | false ->
                    Sg.box color adaptiveBox
                    |> Sg.requirePicking
                    |> Sg.noEvents
                    |> Sg.withEvents [
                        Sg.onDoubleClick (fun _ -> SelectBox boxId)
                        Sg.onEnter (fun _ -> EnterBox boxId)
                        Sg.onLeave (fun () -> ExitBox)
                    ]
                    |> Sg.cullMode world.cullMode
                | true ->
                    match editMode with
                    | true ->
                        Sg.box color adaptiveBox
                        |> Sg.noEvents
                        |> Sg.cullMode world.cullMode
                    | false ->
                        Sg.box color adaptiveBox
                        |> Sg.requirePicking
                        |> Sg.noEvents
                        |> Sg.withEvents [
                            Sg.onDoubleClick (fun _ -> SelectBox boxId)
                        ]
                        |> Sg.cullMode world.cullMode

            let box =
                match blending with
                | false -> box
                | true ->
                    box
                    |> Sg.pass ( RenderPass.after "transparent" RenderPassOrder.BackToFront RenderPass.main )
                    |> Sg.blendMode (Mod.constant BlendMode.Blend)
            
            let boxCtrl =
                match editMode with
                | true -> mkBoxController world b
                | false -> Sg.empty
            
            let minCornerCtrl =
                match editMode with
                | true -> mkCornerController world b b.min
                | false -> Sg.empty

            let maxCornerCtrl =
                match editMode with
                | true -> mkCornerController world b b.max
                | false -> Sg.empty

            let minCornerSphere =
                match editMode with
                | true -> mkCornerSphere world b b.min
                | false -> Sg.empty

            let maxCornerSphere =
                match editMode with
                | true -> mkCornerSphere world b b.max
                | false -> Sg.empty
            
            //let hoverEdges =
            //    match isSel with
            //    | true -> mkHoverEdges world b
            //    | false -> Sg.empty

            let boxCenter = adaptiveBox |> Mod.map ( fun x -> x.Center)
            let toCenter = boxCenter |> Mod.map ( fun c -> Trafo3d.Translation(-c) )

            return
                [box; minCornerSphere; maxCornerSphere; minCornerCtrl; maxCornerCtrl]//; hoverEdges]
                |> Sg.ofList
                |> Sg.trafo toCenter
                |> Sg.trafo b.trafo.previewTrafo
                |> Sg.andAlso boxCtrl
        }
        |> Sg.dynamic
        |> Sg.andAlso fixedLabelSg
    
    let mkBoxesSg (world : MWorld) =
        aset {
            for (id,b) in world.boxes |> AMap.toASet do
                yield mkBox world b
        } |> Sg.set

    let viewScene (world : MWorld) =
        let boxes = mkBoxesSg world
        let object = Sg.sphere' 5 C4b.White 1.0 |> Sg.noEvents
        
        //[object; boxes]
        [boxes]
        |> Sg.ofList
        |> Sg.effect [
            DefaultSurfaces.trafo |> toEffect
            DefaultSurfaces.vertexColor |> toEffect
            DefaultSurfaces.simpleLighting |> toEffect
        ]

    let mkBoxEntry (world : MWorld) (b : MBoundingBox) =
        Incremental.div
            (AttributeMap.ofList [])(
                alist {
                    let! boxId = b.id
                    let! selBox = ASet.contains boxId world.selectedBoxes
                    let! hovBox = world.hoveredBox

                    let selectedColor = C4b(155, 0, 0, 100)
                    let hoverColor = C4b(222, 222, 200, 200)
                    let unHoverColor = C4b(50, 100, 255, 100)
                    
                    let c =
                        match selBox with
                        | true -> selectedColor
                        | false ->
                            match hovBox with
                            | Some id ->
                                match id = boxId with
                                | true -> hoverColor
                                | false -> unHoverColor
                            | None -> unHoverColor

                    let bgc = sprintf "background: %s" (Html.ofC4b c)
                    yield
                        div [
                            clazz "item"
                            style bgc
                            onMouseDoubleClick ( fun _ -> SelectBox boxId )
                            onMouseEnter ( fun _ -> EnterBox boxId )
                            onMouseLeave ( fun _ -> ExitBox )
                        ] [text "Box"]
                }
        )

    let mkBoxInfoDiv (b : MBoundingBox) =
        Incremental.div
            (AttributeMap.ofList []) (
                alist {
                    let! boxId = b.id
                    let! minPose = b.min.trafo.pose
                    let! maxPose = b.max.trafo.pose
                    let min = minPose.position
                    let max = maxPose.position

                    let xAxisStr = sprintf "W (X): %.2f" ( V3d.Distance( V3d(max.X, max.Y, min.Z), V3d(min.X, max.Y, min.Z) ) )
                    let yAxisStr = sprintf "H (Y): %.2f" ( V3d.Distance( V3d(min.X, min.Y, min.Z), V3d(min.X, max.Y, min.Z) ) )
                    let zAxisStr = sprintf "L (Z): %.2f" ( V3d.Distance( V3d(max.X, min.Y, max.Z), V3d(max.X, min.Y, min.Z) ) )
                    
                    yield
                        div [][
                            text xAxisStr
                            br []
                            text yAxisStr
                            br []
                            text zAxisStr
                        ]
                }
        )
    
    let labelTestScene (world : MWorld) =
        adaptive {
            let! v = world.camera.view
            let vp = v.Location

            let sg =
                Sg.text (Font.create "Arial" FontStyle.Bold) C4b.White (Mod.constant "Label 1")
                |> Sg.noEvents
                |> Sg.andAlso (
                    Sg.text (Font.create "Arial" FontStyle.Bold) C4b.White (Mod.constant "Label 2")
                    |> Sg.noEvents
                )
                |> Sg.andAlso (
                    Sg.text (Font.create "Arial" FontStyle.Bold) C4b.White (Mod.constant "Label 3")
                    |> Sg.noEvents
                )
                |> Sg.andAlso (
                    Sg.text (Font.create "Arial" FontStyle.Bold) C4b.White (Mod.constant "Label 4")
                    |> Sg.noEvents
                )
                |> Sg.andAlso (
                    Sg.text (Font.create "Arial" FontStyle.Bold) C4b.White (Mod.constant "Label 5")
                    |> Sg.noEvents
                )
                |> Sg.andAlso (
                    Sg.text (Font.create "Arial" FontStyle.Bold) C4b.White (Mod.constant "Label 6")
                    |> Sg.noEvents
                )
            
            return sg
        } |> Sg.dynamic

    let view (world : MWorld) =
        let frustum = Mod.constant (Frustum.perspective 60.0 0.1 100.0 1.0)
        require Html.semui (
            div [clazz "ui"; style "background-color: #1B1C1E"][
                CameraController.controlledControl world.camera CameraMessage frustum
                    (AttributeMap.ofList[
                        onKeyDown KeyDown
                        attribute "style" "width:80%; height:100%; float:left"
                    ]) (viewScene world) //(labelTestScene world)
                
                div [clazz "ui"; attribute "style" "background-color: #1B1C1E; width:20%; height:100%; float:right"][
                    Html.SemUi.stuffStack [
                        Incremental.div
                            (AttributeMap.ofList []) (
                                alist {
                                    let! x = world.editMode
                                    match x with
                                    | true -> yield button [clazz "ui button"; attribute "style" "background-color: #11AA11"; onClick ( fun _ -> ToggleEditMode )] [text "Edit-Mode"]
                                    | false -> yield button [clazz "ui button"; onClick ( fun _ -> ToggleEditMode )] [text "Edit-Mode"]
                                }
                        )

                        button [clazz "ui button"; onClick ( fun _ -> UnselectBox )][text "Unselect"]
                        button [clazz "ui button"; onClick ( fun _ -> EnableBlending )][text "Blending"]
                        
                        Html.SemUi.dropDown world.trafoKind SetKind
                        Html.SemUi.dropDown world.cullMode ChangeCullMode

                        Incremental.div
                            (AttributeMap.ofList [clazz "ui divided list"]) (
                                alist {
                                    for (id, b) in ASet.toAList (AMap.toASet world.boxes) do
                                        yield mkBoxEntry world b
                                }
                        )

                        Incremental.div
                            (AttributeMap.ofList [clazz "ui divided list"]) (
                                alist {
                                    for id in (ASet.toAList world.selectedBoxes) do
                                        let! box = AMap.find id world.boxes
                                        yield mkBoxInfoDiv box
                                }
                        )
                    ]
                ]
            ]
        )
    
    let app =
        {
            unpersist = Unpersist.instance
            threads = fun (world : World) -> CameraController.threads world.camera |> ThreadPool.map CameraMessage
            initial = initial
            update = update
            view = view
        }
    let start() = App.start app