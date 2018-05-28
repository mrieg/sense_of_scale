namespace EllipseShading

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.Rendering.Text
open Aardvark.UI
open Aardvark.UI.Trafos

module Shader =
    
    open FShade
    
    type UniformScope with
        member x.CenterPos : V3d  = uniform?CenterPos
        member x.Base0     : V3d  = uniform?Base0
        member x.Base1     : V3d  = uniform?Base1
        member x.Base2     : V3d  = uniform?Base2
        member x.InvSkyRot : M33d = uniform?InvSkyRot
        member x.EllColor  : V4d  = uniform?EllColor
    
    [<ReflectedDefinition>]
    let det33 (m : M33d) =
        m.M00*(m.M11*m.M22-m.M12*m.M21) - m.M01*(m.M10*m.M22-m.M12*m.M20) + m.M02*(m.M10*m.M21-m.M11*m.M20)
    
    let ellipsoidShading (v : Effects.Vertex) =
        fragment {
            let ctr = uniform.CenterPos
            let f0  = uniform.Base0
            let f1  = uniform.Base1
            let f2  = uniform.Base2
            let pos = uniform.InvSkyRot * (v.wp.XYZ - ctr)
            
            let m0 = M33d(pos.X, f1.X, f2.X, pos.Y, f1.Y, f2.Y, pos.Z, f1.Z, f2.Z)
            let d0 = m0 |> det33

            let m1 = M33d(f0.X, pos.X, f2.X, f0.Y, pos.Y, f2.Y, f0.Z, pos.Z, f2.Z)
            let d1 = m1 |> det33

            let m2 = M33d(f0.X, f1.X, pos.X, f0.Y, f1.Y, pos.Y, f0.Z, f1.Z, pos.Z)
            let d2 = m2 |> det33

            let m3 = M33d(f0.X, f1.X, f2.X, f0.Y, f1.Y, f2.Y, f0.Z, f1.Z, f2.Z)
            let d3 = m3 |> det33
            
            let value = d0*d0 + d1*d1 + d2*d2 - d3*d3
            let c =
                if value < 0.0
                then uniform.EllColor
                else v.c
            
            return {v with c = c}
        }
    
module Ellipse =
    
    let initial =
        {
            a        = 1.0
            b        = 1.0
            c        = 1.0
            center   = V3d.OOO
            rotation = Trafo3d.Identity
            color    = V4d(0.0, 0.0, 1.0, 1.0)
        }
    
    let setup (values : V3d) (center : V3d) (rotation : Trafo3d) (color : C4b) =
        {
            a        = values.X
            b        = values.Y
            c        = values.Z
            center   = center
            rotation = rotation
            color    = color.ToC4d().ToV4d()
        }
    
(*
    
    TODO: correct controls (trafotrls, inputs, setcenter)
    TODO: precision

*)
module Controls =
    
    type Action =
        | ValuesAction     of Vector3d.Action
        | CenterAction     of Vector3d.Action
        | TrafoAction      of TrafoController.Action
        | ChangeKind       of TrafoKind
        | ColPickerMessage of ColorPicker.Action
        | ToggleShowTrafo
        | ToggleDebug
    
    let update (m : ControlsModel) (act : Action) =
        match act with
        | ValuesAction msg ->
            {m with values = Vector3d.update m.values msg}
        
        | CenterAction msg ->
            //TODO: update trafocntrl
            let center = Vector3d.update m.center msg
            {m with center = center}
        
        | TrafoAction  msg ->
            match m.kind with
            | TrafoKind.Rotate ->
                let trafo = RotationController.updateController m.trafo msg
                {m with trafo = trafo}
            
            | _ ->
                let trafo = TranslateController.updateController m.trafo msg
                let c     = trafo.pose.position + trafo.workingPose.position
                {m with trafo = trafo; center = {m.center with value = c; x = {m.center.x with value = c.X}; y = {m.center.y with value = c.Y}; z = {m.center.z with value = c.Z}}}
        
        | ChangeKind       kind -> {m with kind = kind}
        | ColPickerMessage msg  -> {m with colPicker = ColorPicker.update m.colPicker msg}
        | ToggleShowTrafo       -> {m with showTraf = not m.showTraf}
        | ToggleDebug           -> {m with showDebug = not m.showDebug}
    
    let viewScene' (m : MControlsModel) (view : IMod<CameraView>) (liftMessage : Action -> 'msg) =
        Mod.map2 ( fun k s ->
            if s
            then
                match k with
                | TrafoKind.Rotate ->
                    RotationController.viewController (fun x -> x |> TrafoAction |> liftMessage) view m.trafo
                | _ ->
                    TranslateController.viewController (fun x -> x |> TrafoAction |> liftMessage) view m.trafo
            else Sg.empty
        ) m.kind m.showTraf
        |> Sg.dynamic
    
    let view' (m : MControlsModel) =
        div [clazz "ui"][
            Html.table [
                Html.row "Center: " [
                    Vector3d.view m.center |> UI.map CenterAction
                ]
                Html.row "Values: " [
                    Vector3d.view m.values |> UI.map ValuesAction
                ]
                Html.row "Color: " [
                    ColorPicker.view m.colPicker |> UI.map ColPickerMessage
                ]
                Html.row "Show Trafocontrols: " [
                    Utils.Html.toggleButton m.showTraf "Show" "Hide" ToggleShowTrafo
                ]
                Html.row "Debug Ellipsoid: " [
                    Utils.Html.toggleButton m.showDebug "Show" "Hide" ToggleDebug
                ]
                Html.row "Kind: " [
                    Html.SemUi.dropDown m.kind ChangeKind
                ]
            ]
        ]

    let initial (ell : EllipseModel) (sky : V3d) =
        let values = V3d(ell.a, ell.b, ell.c) |> Vector3d.initV3d
        let v =
            {
                values with
                    x = {values.x with min = 0.0; max = 50000.0; step = 0.1}
                    y = {values.y with min = 0.0; max = 50000.0; step = 0.1}
                    z = {values.z with min = 0.0; max = 50000.0; step = 0.1}
            }
        
        let center = ell.center |> Vector3d.initV3d
        let c =
            {
                center with
                    x = {center.x with min = -50000.0; max = 50000.0; step = 0.1}
                    y = {center.y with min = -50000.0; max = 50000.0; step = 0.1}
                    z = {center.z with min = -50000.0; max = 50000.0; step = 0.1}
            }
        
        let skyRot = Trafo3d.RotateInto(V3d.OOI, sky)
        let pose   = Pose.translate ell.center
        let prevTrafo = pose |> Pose.toTrafo
        
        let prevTrafo = skyRot * prevTrafo
        
        let trafo = {TrafoController.initial with pose = pose; previewTrafo = prevTrafo; mode = TrafoMode.Local}
        
        {
            values    = v
            center    = c
            trafo     = trafo
            kind      = TrafoKind.Rotate
            showTraf  = false
            showDebug = false
            colPicker = {ColorPicker.init with c = ell.color.ToC4d().ToC4b()}
        }
    
module App =
    
    type Action =
        | PointsMsg   of Utils.Picking.Action * CameraView
        | ControlsMsg of Controls.Action
        | SetCenter   of V3d * CameraView
        | ToggleAddMode
    
    let trafoGrabbed (m : Model) = m.controls.trafo.grabbed.IsSome
    
    let update (m : Model) (act : Action) =
        match act with
        | PointsMsg (msg, view) ->
            match msg with
            | Utils.Picking.Action.AddPoint pos ->
                if m.pickPoints.points.Count < 3
                then
                    let pickPoints = Utils.Picking.update m.pickPoints msg
                    if pickPoints.points.Count = 3
                    then
                        let p0 = pickPoints.points.[0].pos
                        let p1 = pickPoints.points.[1].pos
                        let p2 = pickPoints.points.[2].pos

                        let ctr = (p1 + p0) * 0.5
                        
                        let sky = view.Sky
                        let invSkyRot = Trafo3d.RotateInto(V3d.OOI, sky).Inverse.Forward.TransformPos
                        
                        let p0 = p0 |> invSkyRot
                        let p1 = p1 |> invSkyRot
                        let p2 = p2 |> invSkyRot
                        
                        let p0 = V3d(p0.X, p0.Y, 0.0)
                        let p1 = V3d(p1.X, p1.Y, 0.0)
                        let p2 = V3d(p2.X, p2.Y, 0.0)
                        
                        let a = V3d.Distance(p0, p1)
                        let l = Line3d(p0, p1)
                        let b = l.GetMinimalDistanceTo(p2) * 2.0
                        let c = b
                        
                        let r = Trafo3d.RotateInto(V3d.IOO, (p1 - p0).Normalized)
                        
                        let ell = {a = a; b = b; c = c; center = ctr; rotation = r; color = m.ellipse.color}
                        let controls = {Controls.initial ell view.Sky with colPicker = m.controls.colPicker}
                        {m with pickPoints = pickPoints; ellipse = ell; controls = controls; addMode = false}
                    else
                        {m with pickPoints = pickPoints}
                else m
            | _ ->
                {m with pickPoints = Utils.Picking.update m.pickPoints msg}
        
        | ControlsMsg msg ->
            let controls = Controls.update m.controls msg

            let ell =
                match msg with
                | Controls.Action.TrafoAction ta ->
                    //TODO
                    m.ellipse
                | Controls.Action.ToggleShowTrafo -> m.ellipse
                | Controls.Action.ChangeKind kind -> m.ellipse
                | _ ->
                    Ellipse.setup controls.values.value controls.center.value m.ellipse.rotation controls.colPicker.c

            {m with controls = controls; ellipse = ell}
        
        | SetCenter (ctr, view) ->
            let ell = {m.ellipse with center = ctr; color = m.ellipse.color}
            let controls = {Controls.initial ell view.Sky with showDebug = m.controls.showDebug; showTraf = m.controls.showTraf; colPicker = m.controls.colPicker}
            {m with ellipse = ell; controls = controls}
        
        | ToggleAddMode -> {m with addMode = not m.addMode; pickPoints = Utils.Picking.initial}
    
    let mkEffects (m : MModel) (view : IMod<CameraView>) effects isg =
        
        let efx =
            [
                for e in effects do
                    yield e
                yield Shader.ellipsoidShading |> toEffect
            ]
        
        let invSkyRot = view  |> Mod.map2 (fun r v -> (r * Trafo3d.RotateInto(V3d.OOI, v.Sky)).Forward.UpperLeftM33().Inverse) m.ellipse.rotation
        let base0     = m.ellipse.a |> Mod.map (fun a -> V3d.IOO * a * 0.5)
        let base1     = m.ellipse.b |> Mod.map (fun b -> V3d.OIO * b * 0.5)
        let base2     = m.ellipse.c |> Mod.map (fun c -> V3d.OOI * c * 0.5)
        
        isg
        |> Sg.uniform "CenterPos" m.ellipse.center
        |> Sg.uniform "Base0"     base0
        |> Sg.uniform "Base1"     base1
        |> Sg.uniform "Base2"     base2
        |> Sg.uniform "InvSkyRot" invSkyRot
        |> Sg.uniform "EllColor"  m.ellipse.color
        |> Sg.effect efx
    
    let viewScene' (m : MModel) (view : IMod<CameraView>) (pickSg) (liftMessage : Action -> 'msg) =
        let points =
            Utils.Picking.mkSg m.pickPoints view (fun x -> (x, view |> Mod.force) |> PointsMsg |> liftMessage)
        
        let eventsAdd =
            [
                Sg.onDoubleClick (fun pos -> (pos |> Utils.Picking.Action.AddPoint, view |> Mod.force) |> PointsMsg |> liftMessage)
            ]
        
        let eventsNormal = [Sg.onDoubleClick (fun pos -> (pos, view |> Mod.force) |> SetCenter |> liftMessage)]
        
        let debugEllipsoid =
            Sg.unitSphere' 7 C4b.DarkRed
            |> Sg.noEvents
            |> Sg.trafo (
                let s =
                    adaptive {
                        let! a = m.ellipse.a
                        let! b = m.ellipse.b
                        let! c = m.ellipse.c
                        return V3d(a*0.5, b*0.5, c*0.5)
                    }
                
                s |> Mod.map Trafo3d.Scale
            )
            |> Sg.trafo (
                view |> Mod.map2 (fun r v -> r * Trafo3d.RotateInto(V3d.OOI, v.Sky)) m.ellipse.rotation
            )
            |> Sg.trafo (
                m.ellipse.center |> Mod.map Trafo3d.Translation
            )
            |> Sg.fillMode (FillMode.Line |> Mod.constant)
            |> Sg.shader {
                do! DefaultSurfaces.trafo
                do! DefaultSurfaces.vertexColor
                do! DefaultSurfaces.simpleLighting
            }
        
        let debugEllipsoid =
            m.controls.showDebug
            |> Mod.map ( fun x ->
                if x then debugEllipsoid else Sg.empty
            )
            |> Sg.dynamic
        
        let trafoControls = Controls.viewScene' m.controls view (fun x -> x |> ControlsMsg |> liftMessage)
        
        let axesLines =
            let lines =
                adaptive {
                    let! a = m.ellipse.a
                    let! b = m.ellipse.b

                    let p00 = -V3d.IOO * 0.5 * a + V3d(0.0, 0.0, 0.01)
                    let p01 =  V3d.IOO * 0.5 * a + V3d(0.0, 0.0, 0.01)
                    let l0  =  Line3d(p00, p01)

                    let p10 = -V3d.OIO * 0.5 * b + V3d(0.0, 0.0, 0.01)
                    let p11 =  V3d.OIO * 0.5 * b + V3d(0.0, 0.0, 0.01)
                    let l1  =  Line3d(p10, p11)
                    
                    return [|l0;l1|]
                }
            
            let col = C4b.White |> Mod.constant
            Sg.lines col lines
            |> Sg.noEvents
            |> Sg.trafo (
                m.ellipse.rotation
            )
            |> Sg.trafo (
                view
                |> Mod.map ( fun v ->
                    Trafo3d.RotateInto(V3d.OOI, v.Sky)
                )
            )
            |> Sg.trafo (
                m.ellipse.center
                |> Mod.map ( fun c ->
                    Trafo3d.Translation(c)
                )
            )
            |> Sg.shader {
                do! DefaultSurfaces.trafo
                do! DefaultSurfaces.vertexColor
            }
        
        let axesLabels =
            let a = m.ellipse.a |> Mod.map (fun x -> sprintf "%.2f" x)
            let b = m.ellipse.b |> Mod.map (fun x -> sprintf "%.2f" x)
            
            let transA =
                adaptive {
                    let! a = m.ellipse.a
                    let! c = m.ellipse.center
                    let! r = m.ellipse.rotation
                    let! v = view

                    let p = V3d.IOO * a * 0.5
                    let t = r * Trafo3d.RotateInto(V3d.OOI, v.Sky) * Trafo3d.Translation(c)
                    let p = p |> t.Forward.TransformPos

                    let d = V3d.Distance(p, v.Location)
                    let s = Trafo3d.Scale(0.025 * d)
                    let trafo = s * Trafo3d.Translation(p)
                    
                    return trafo
                }
            
            let labA =
                Sg.text (Font.create "arial" FontStyle.Regular) C4b.White a
                |> Sg.billboard
                |> Sg.noEvents
                |> Sg.trafo transA
            
            let transB =
                adaptive {
                    let! b = m.ellipse.b
                    let! c = m.ellipse.center
                    let! r = m.ellipse.rotation
                    let! v = view

                    let p = V3d.OIO * b * 0.5
                    let t = r * Trafo3d.RotateInto(V3d.OOI, v.Sky) * Trafo3d.Translation(c)
                    let p = p |> t.Forward.TransformPos
                    
                    let d = V3d.Distance(p, v.Location)
                    let s = Trafo3d.Scale(0.025 * d)
                    let trafo = s * Trafo3d.Translation(p)
                    
                    return trafo
                }
            
            let labB =
                Sg.text (Font.create "arial" FontStyle.Regular) C4b.White b
                |> Sg.billboard
                |> Sg.noEvents
                |> Sg.trafo transB
            
            [labA; labB]
            |> Sg.ofList
        
        m.addMode
        |> Mod.map ( fun x ->
            if x
            then [points; pickSg eventsAdd] |> Sg.ofList
            else [pickSg eventsNormal; trafoControls] |> Sg.ofList
        )
        |> Sg.dynamic
        |> Sg.andAlso debugEllipsoid
        |> Sg.andAlso axesLines
        |> Sg.andAlso axesLabels
    
    let view' (m : MModel) =
        Html.SemUi.stuffStack [
            Utils.Html.toggleButton m.addMode "Adjust" "Cancel" ToggleAddMode
            Controls.view' m.controls |> UI.map ControlsMsg
        ]
    
    let initial (sky : V3d) =
        let ell  = Ellipse.initial
        {
            ellipse    = ell
            pickPoints = Utils.Picking.initial
            controls   = Controls.initial ell sky
            addMode    = false
        }