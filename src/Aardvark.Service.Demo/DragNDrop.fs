﻿namespace DragNDrop

module App =

    open Aardvark.Base
    open Aardvark.Base.Incremental
    
    open Aardvark.SceneGraph
    open Aardvark.Base.Rendering
    open Aardvark.UI
    open Aardvark.UI.Primitives
    open Aardvark.Base.Geometry

    type Action = 
        | StartDrag of SceneHit
        | StopDrag
        | MoveRay      of RayPart 
        | CameraAction of CameraController.Message

    let update (m : Model) (a : Action) =

        match a with
            | CameraAction a when Option.isNone m.dragging -> 
                // disable cam on dragging
                { m with camera = CameraController.update m.camera a }
            | MoveRay p ->
                match m.dragging with
                    | Some { PickPoint = worldSpaceStart; Offset = centerOffset } -> 
                        let i = p.Ray.Ray.Intersect (Plane3d(V3d.OOI,worldSpaceStart))
                        { m with trafo = Trafo3d.Translation (i - centerOffset) }
                    | None -> m
            | StartDrag p -> 
                { m with dragging = Some { PickPoint = p.globalPosition; Offset =  p.globalPosition - m.trafo.Forward.TransformPos(V3d.OOO) }}
            | StopDrag    -> { m with dragging = None   }
            | _ -> m


    let scene (m : MModel) =

        let box =
            Sg.box (Mod.constant C4b.Green) (Mod.constant (Box3d.FromCenterAndSize(V3d.OOO,V3d.III)))
            |> Sg.requirePicking
            |> Sg.noEvents    
            |> Sg.withEvents [
                    Sg.onMouseDownEvt (fun e -> StartDrag e )
                    Sg.onMouseUp      (fun p -> StopDrag    )
               ]
            |> Sg.trafo m.trafo

        let plane = 
            Sg.box' C4b.White (Box3d.FromCenterAndSize(V3d.OOO,V3d(10.0,10.0,0.1)))
            |> Sg.noEvents

        let scene = 
            Sg.ofSeq [box; plane]
            |> Sg.effect [
                    toEffect <| DefaultSurfaces.trafo
                    toEffect <| DefaultSurfaces.vertexColor
                    toEffect <| DefaultSurfaces.simpleLighting
               ]

            

        CameraController.controlledControl m.camera CameraAction (Frustum.perspective 60.0 0.1 100.0 1.0 |> Mod.constant) 
            (AttributeMap.ofList [ 
                attribute "style" "width:100%; height: 100%"
             ]) scene

    let view (m : MModel) =
        div [] [
            scene m
        ]

    let app =
        {
            unpersist = Unpersist.instance
            threads = fun (model : Model) -> CameraController.threads model.camera |> ThreadPool.map CameraAction
            initial = { trafo = Trafo3d.Identity; dragging = None; camera = CameraController.initial }
            update = update
            view = view
        }

    let start() = App.start app



module TranslateController =

    open Aardvark.Base
    open Aardvark.Base.Incremental
    
    open Aardvark.SceneGraph
    open Aardvark.Base.Rendering
    open Aardvark.UI
    open Aardvark.UI.Primitives

    open Aardvark.Base.Geometry

    open DragNDrop

    type RayPart with
        member x.Transformed(t : Trafo3d) =
            RayPart(FastRay3d(x.Ray.Ray.Transformed(t.Forward)), x.TMin, x.TMax)

    module Shader =
    
        open FShade
        open Aardvark.Base.Rendering.Effects

        let hoverColor (v : Vertex) =
            vertex {
                let c : V4d = uniform?HoverColor
                return { v with c = c }
            }


    [<AutoOpen>]
    module Config =
        let coneHeight = 0.1
        let coneRadius = 0.03
        let cylinderRadius = 0.015
        let tessellation = 8

    type ControllerAction = 
        | Hover of Axis
        | Unhover 
        | MoveRay of RayPart
        | Grab of RayPart * Axis
        | Release
    
    type SceneAction =
        | ControllerAction of ControllerAction
        | CameraAction     of CameraController.Message


    let closestT (r : RayPart) (axis : Axis) =
        let other =
            match axis with
            | X -> Ray3d(V3d.OOO, V3d.IOO)
            | Y -> Ray3d(V3d.OOO, V3d.OIO)
            | Z -> Ray3d(V3d.OOO, V3d.OOI)

        let mutable unused = 0.0
        let mutable t = 0.0
        r.Ray.Ray.GetMinimalDistanceTo(other,&unused,&t) |> ignore
        t

    let updateController (m : Transformation) (a : ControllerAction) =
        match a with
            | Hover axis -> 
                { m with hovered = Some axis }
            | Unhover -> 
                { m with hovered = None }
            | Grab (rp, axis) ->
                let offset = closestT rp axis
                { m with grabbed = Some { offset = offset; axis = axis; hit = V3d.NaN } } 
            | Release ->
                { m with grabbed = None }
            | MoveRay rp ->
                match m.grabbed with
                | Some { offset = offset; axis = axis } ->

                     let other =
                        match axis with
                        | X -> V3d.IOO
                        | Y -> V3d.OIO
                        | Z -> V3d.OOI

                     // implement mode switch here (t2)
                     let trafo = m.trafo //Trafo3d.Translation m.trafo.Forward.C3.XYZ

                     let closestPoint = closestT rp axis
                     let trafo = Trafo3d.Translation ((closestPoint - offset) * other) * trafo

                     { m with trafo = trafo; }
                | None -> m

    let viewController (liftMessage : ControllerAction -> 'msg) (m : MTransformation) =
        
        let arrow rot axis =
            let col =
                m.hovered |> Mod.map2 ( fun g h ->
                 match h, g, axis with
                 | _,      Some g, p when g = p -> C4b.Yellow
                 | Some h, None,   p when h = p -> C4b.White
                 | _,      _,      X -> C4b.Red
                 | _,      _,      Y -> C4b.Green
                 | _,      _,      Z -> C4b.Blue
                ) (m.grabbed |> Mod.map (Option.map ( fun p -> p.axis )))
            Sg.cylinder tessellation col (Mod.constant cylinderRadius) (Mod.constant 1.0) 
            |> Sg.noEvents
            |> Sg.andAlso (                
                IndexedGeometryPrimitives.solidCone V3d.OOI V3d.OOI coneHeight coneRadius tessellation C4b.Red 
                 |> Sg.ofIndexedGeometry 
                 |> Sg.noEvents
                )
            |> Sg.pickable (Cylinder3d(V3d.OOO,V3d.OOI + V3d(0.0,0.0,0.1),cylinderRadius + 0.1) |> PickShape.Cylinder)
            |> Sg.transform rot
            |> Sg.uniform "HoverColor" col
            |> Sg.withEvents [ 
                    Sg.onEnter        (fun _ ->   Hover axis)
                    Sg.onMouseDownEvt (fun evt -> Grab (evt.localRay, axis))
                    Sg.onLeave        (fun _ ->   Unhover) 
               ]
            |> Sg.Incremental.withGlobalEvents ( 
                    amap {
                        let! grabbed = m.grabbed
                        if grabbed.IsSome then
                            yield Global.onMouseMove (fun e -> MoveRay e.localRay)
                            yield Global.onMouseUp   (fun _ -> Release)
                    }
                )

        let arrowX = arrow (Trafo3d.RotationY Constant.PiHalf) X
        let arrowY = arrow (Trafo3d.RotationX -Constant.PiHalf) Y
        let arrowZ = arrow (Trafo3d.RotationY 0.0) Z

        let t2 =
            adaptive {
                let! t = m.trafo
                let pos = t.Forward.C3
                printfn "%A" pos
                return Trafo3d.Translation pos.XYZ
            }
        
        Sg.ofList [arrowX; arrowY; arrowZ ]
        |> Sg.effect [ DefaultSurfaces.trafo |> toEffect; Shader.hoverColor |> toEffect; DefaultSurfaces.simpleLighting |> toEffect]
        |> Sg.trafo m.trafo // implement mode switch here (t2)
        |> Sg.map liftMessage
        

    let updateScene (m : Scene) (a : SceneAction) =
        match a with
            | CameraAction a when m.transformation.grabbed.IsNone -> 
                { m with camera = CameraController.update m.camera a }
            | ControllerAction a -> { m with transformation = updateController m.transformation a }
            | _ -> m

    let viewScene' (m : MScene) =
        let cross =
             IndexedGeometryPrimitives.coordinateCross (V3d(10.0, 10.0, 10.0))
                |> Sg.ofIndexedGeometry
                |> Sg.effect [
                    toEffect DefaultSurfaces.trafo
                    toEffect DefaultSurfaces.vertexColor
                    ]
                |> Sg.noEvents

        Sg.ofList [viewController ControllerAction m.transformation; cross]

    let viewScene (m : MScene) =
        div [] [
            CameraController.controlledControl m.camera CameraAction (Frustum.perspective 60.0 0.1 100.0 1.0 |> Mod.constant) 
                (AttributeMap.ofList [ 
                    yield  attribute "style" "width:100%; height: 100%"; 
                 ]) (viewScene' m)
        ]

    let initial =  { hovered = None; grabbed = None; trafo = Trafo3d.Identity; workingTrafo = Trafo3d.Identity }

    let app =
        {
            unpersist = Unpersist.instance
            threads = fun (model : Scene) -> CameraController.threads model.camera |> ThreadPool.map CameraAction
            initial = {  camera = CameraController.initial
                         transformation = initial
                      }
            update = updateScene
            view = viewScene
        }

    let start() = App.start app

module RotationController =
    
    // old version ...https://github.com/aardvark-platform/aardvark.media/blob/base2/src/Aardvark.ImmutableSceneGraph/TrafoApps.fs 


    open Aardvark.Base
    open Aardvark.Base.Incremental
    
    open Aardvark.Base.Rendering
    open Aardvark.Base.Geometry
   
    open Aardvark.UI
    open Aardvark.UI.Primitives

    open Aardvark.SceneGraph

    type RayPart with
        member x.Transformed(t : Trafo3d) =
            RayPart(FastRay3d(x.Ray.Ray.Transformed(t.Forward)), x.TMin, x.TMax)

    [<AutoOpen>]
    module Config =
        let cylinderRadius = 0.015
        let radius    = 1.0
        let tesselation = 32.0

    module Shader =
        open FShade
        open Aardvark.Base.Rendering.Effects

        let hoverColor (v : Vertex) =
            vertex {
                let c : V4d = uniform?HoverColor
                return { v with c = c }
            }

    type ControllerAction = 
      | Hover of Axis
      | Unhover 
      | RotateRay of RayPart
      | Grab of RayPart * Axis
      | Release
      | Nop
    
    type SceneAction =
      | ControllerAction of ControllerAction
      | CameraAction     of CameraController.Message

    
    module RotationHandle = 
        
        let circlePoint (c:V3d) r ang axis =
            let x = c.X + r * cos ang
            let y = c.Y + r * sin ang
            match axis with
              | X -> V3d(0.0, x, y)
              | Y -> V3d(x, 0.0, y)
              | Z -> V3d(x, y, 0.0)
        
        let PI2 = System.Math.PI * 2.0

        let circle c r tess axis =
            let step = PI2 / tess
            //[0.0 .. 1.0 .. 1.0] |> List.map (fun x -> circlePoint c r x axis) |> List.toSeq
            [0.0 .. step .. (PI2)] |> List.map (fun x -> circlePoint c r x axis) |> List.toSeq

        let sg axis =
            [
                let poly = (circle V3d.Zero radius tesselation axis) |> Polygon3d
                
                let segments = 
                    poly.EdgeLines 
                      |> Seq.map(fun edge -> Line3d(edge.P0, edge.P1)) 
                      |> Seq.toArray                      
                
                yield Sg.lines (Mod.constant(C4b.Red)) (Mod.constant(segments))
            ] |> Sg.group
     
    let axisToV3d axis = 
        match axis with 
          | X -> V3d.XAxis
          | Y -> V3d.YAxis
          | Z -> V3d.ZAxis

    let toCircle axis = 
        let v = axis |> axisToV3d        
        Circle3d(V3d.Zero, v, radius)
        
          
    let intersect (ray : RayPart) (circle:Circle3d) =
        let mutable p = V3d.NaN
        let mutable t = 0.0  
            
        ray.Ray.Ray.Intersects(circle.Plane, &t, &p), p

    let closestT (r : RayPart) (axis : Axis) =
        let other =
            match axis with
            | X -> Ray3d(-V3d.IOO, V3d.IOO)
            | Y -> Ray3d(-V3d.OIO, V3d.OIO)
            | Z -> Ray3d(-V3d.OOI, V3d.OOI)

        let mutable unused = 0.0
        let mutable t = 0.0
        r.Ray.Ray.GetMinimalDistanceTo(other,&unused,&t) |> ignore
        t

    let updateController (m : Transformation) (a : ControllerAction) =
        match a with
            | Hover axis -> 
                { m with hovered = Some axis }
            | Unhover -> 
                { m with hovered = None }
            | Grab (rp, axis) ->
                Log.warn "grabbing %A" axis

                let h, p = intersect rp (axis |> toCircle)

                //let offset = closestT rp axis
                { m with grabbed = Some { offset = 0.0; axis = axis; hit = p } }
            | Release ->
                match m.grabbed with
                    | Some _ -> { m with grabbed = None; trafo = m.workingTrafo * m.trafo; workingTrafo = Trafo3d.Identity}
                    | _ -> m
            | RotateRay rp ->
                match m.grabbed with
                | Some { offset = offset; axis = axis; hit = hit } ->
                     let h, p = intersect rp (axis |> toCircle)
                     if h && (not hit.IsNaN) then
                         let trafo = Trafo3d.RotateInto(hit.Normalized, p.Normalized)
                         { m with workingTrafo = trafo  } 
                     else 
                        m
                | None -> m
            | Nop -> m

    let viewController (liftMessage : ControllerAction -> 'msg) (m : MTransformation) : ISg<'msg> =
            
        let circle dir axis =
            let col =
                m.hovered |> Mod.map2 ( fun g h ->
                 
                 match h, g, axis with
                 | _,      Some g, p when g = p -> C4b.Yellow
                 | Some h, None,   p when h = p -> C4b.White
                 | _,      _,      X -> C4b.Red
                 | _,      _,      Y -> C4b.Green
                 | _,      _,      Z -> C4b.Blue
                ) (m.grabbed |> Mod.map (Option.map ( fun p -> p.axis )))                                                
            
            let circle = Circle3d(V3d.Zero, dir, radius)
               
            RotationHandle.sg axis            
            |> Sg.trafo m.workingTrafo
            |> Sg.uniform "HoverColor" col
            |> Sg.uniform "LineWidth" (Mod.constant(cylinderRadius * 20.0))
            |> Sg.noEvents            
            |> Sg.Incremental.withGlobalEvents ( 
                    amap {                        
                        let! grabbed = m.grabbed
                        if grabbed.IsSome then
                            yield Global.onMouseMove (fun e -> RotateRay e.localRay)
                            yield Global.onMouseUp   (fun _ -> Release)
                        else
                            yield Global.onMouseMove (
                                fun e -> 
                                    let hit, p = intersect e.localRay circle
                                    if hit then
                                        let dist = (V3d.Distance(V3d.Zero, p))                                        
                                        if (dist > radius - 0.1 && dist < radius + 0.1) then                                            
                                            Hover axis
                                        else                                            
                                            m.hovered |> Mod.map(fun x ->
                                                match x with
                                                    | Some h when h = axis -> 
                                                        Unhover
                                                    | _ -> Nop) |> Mod.force
                                    else
                                        Nop)
                            yield Global.onMouseDown (
                                fun e ->
                                    m.hovered |> Mod.map(fun x ->
                                        match x with
                                          | Some h when h = axis -> 
                                                Grab (e.localRay, axis)
                                          | _ -> Nop
                                    ) |> Mod.force

                                    //torus.GetMinimalDistance
                                    //check enter -> hover or unhover
                                    //on click -> grab
                                    )
                    }
                )

        let circleX = circle V3d.XAxis Axis.X
        let circleY = circle V3d.YAxis Axis.Y
        let circleZ = circle V3d.ZAxis Axis.Z
        
        Sg.ofList [circleX; circleY; circleZ ]
        |> Sg.effect [ DefaultSurfaces.trafo |> toEffect; Shader.hoverColor |> toEffect] //; DefaultSurfaces.simpleLighting |> toEffect        
        |> Sg.trafo m.trafo
        |> Sg.noEvents
        |> Aardvark.UI.``F# Sg``.Sg.map liftMessage
        
    let updateScene (m : Scene) (a : SceneAction) =
        match a with
            | CameraAction a when m.transformation.grabbed.IsNone -> 
                { m with camera = CameraController.update m.camera a }
            | ControllerAction a -> { m with transformation = updateController m.transformation a }
            | _ -> m

    let viewScene' (m : MScene) : ISg<SceneAction> =
        let cross =
             IndexedGeometryPrimitives.coordinateCross (V3d(10.0, 10.0, 10.0))
                |> Sg.ofIndexedGeometry
                |> Sg.effect [
                    toEffect DefaultSurfaces.trafo
                    toEffect DefaultSurfaces.vertexColor
                    ]
                |> Sg.noEvents

        let first = viewController ControllerAction m.transformation |> Sg.noEvents

        Aardvark.UI.``F# Sg``.Sg.ofList [first; cross]

    let viewScene (m : MScene) : DomNode<SceneAction> =
        div [] [
            CameraController.controlledControl m.camera CameraAction (Frustum.perspective 60.0 0.1 100.0 1.0 |> Mod.constant) 
                (AttributeMap.ofList [ 
                    yield  attribute "style" "width:100%; height: 100%"; 
                 ]) (viewScene' m)
        ]

    let initial =  { hovered = None; grabbed = None; trafo = Trafo3d.Identity; workingTrafo = Trafo3d.Identity }

    let app =
        {
            unpersist = Unpersist.instance
            threads = fun (model : Scene) -> CameraController.threads model.camera |> ThreadPool.map CameraAction
            initial = {  camera = CameraController.initial
                         transformation = initial
                      }
            update = updateScene
            view = viewScene
        }

    let start() = App.start app