namespace Boxes

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.Rendering
open Aardvark.UI
open Aardvark.UI.Trafos
open Aardvark.UI.Primitives

module Box =
    
    module Config =
        let unselectColor = C4b(212, 212, 212, 92)
        let selectColor = C4b(232, 232, 22, 92)
        let editColor = C4b(22, 252, 252, 92)
        let trafoColor = C4b.VRVisGreen
    
    let initial (c : V3d) (up : V3d) =
        {
            geom      = Box3d.FromCenterAndSize(V3d.OOO, V3d.III)
            center    = c
            size      = V3d.III
            color     = Config.unselectColor
            trafo     = Trafo3d.RotateInto(V3d.OOI, up.Normalized)
            trafoMode = TrafoMode.Local
            centering = true
            options   = RenderOptions.initial
            values    = BoxValues.setup' c V3d.III "Box"
            display   = true
            name      = "Box"
            id        = System.Guid.NewGuid().ToString()
        }
    
    let setup (center : V3d) (size : V3d) (rotation : V3d) (name : string) =
        {
            geom      = Box3d.FromCenterAndSize(V3d.OOO, size)
            center    = center
            size      = size
            color     = Config.unselectColor
            trafo     = Trafo3d.Rotation(rotation)
            trafoMode = TrafoMode.Local
            centering = false
            options   = RenderOptions.initial
            values    = BoxValues.setup' center size name
            display   = true
            name      = name
            id        = System.Guid.NewGuid().ToString()
        }
    
    let setup' (center : V3d) (size : V3d) (sky : V3d) (name : string) =
        {
            geom      = Box3d.FromCenterAndSize(V3d.OOO, size)
            center    = center
            size      = size
            color     = Config.unselectColor
            trafo     = Trafo3d.RotateInto(V3d.OOI, sky.Normalized)
            trafoMode = TrafoMode.Local
            centering = false
            options   = RenderOptions.initial
            values    = BoxValues.setup' center size name
            display   = true
            name      = name
            id        = System.Guid.NewGuid().ToString()
        }

    type Action =
        | UpdateCenter        of V3d
        | UpdateSize          of V3d
        | EnlargeBox          of V3d * FaceType
        | Rotate              of Axis * float
        | RotateWithTrafo     of Trafo3d
        | ChangeTrafoMode     of TrafoMode
        | ChangeRenderOptions of RenderOptions.Action
        | ToggleCentering
        | ValuesMessage       of BoxValues.Action
        | ToggleDisplay
    
    let update (m : BoxModel) (act : Action) =
        match act with
        | UpdateCenter v -> {m with center = v; values = BoxValues.setup' v m.size m.name}

        | UpdateSize s ->
            match m.centering with
            | true -> {m with size = s; geom = Box3d.FromCenterAndSize(V3d.OOO, s); values = BoxValues.setup' m.center s m.name}
            | false ->
                let euler = Rot3d.FromM33d( m.trafo.Forward.UpperLeftM33() ).GetEulerAngles()
                let newTrafo = Trafo3d.Rotation(euler)
            
                let newCenter =
                    let value = m.center + (m.size - s) * 0.5
                    let centerTrafo = M44d.Translation(m.center) * newTrafo.Forward * M44d.Translation(-m.center)
                    centerTrafo.TransformPos(value)
                
                {m with size = s; center = newCenter; trafo = newTrafo; geom = Box3d.FromCenterAndSize(V3d.OOO, s); values = BoxValues.setup' newCenter s m.name}
        
        | EnlargeBox (v, ft) ->
            let inside (v : V3d) (b : Box3d) =
                if v.X > b.Min.X && v.X < b.Max.X && v.Y > b.Min.Y && v.Y < b.Max.Y && v.Z > b.Min.Z && v.Z < b.Max.Z
                then true
                else false
            
            let shrinkBox (v : V3d) (ft : FaceType) (b : Box3d) =
                let min = b.Min
                let max = b.Max
                match ft with
                | FaceFront | FaceTop | FaceLeft ->
                    let newMax =
                        match ft with
                        | FaceFront -> V3d(max.X, v.Y, max.Z)
                        | FaceTop   -> V3d(max.X, max.Y, v.Z)
                        | FaceLeft  -> V3d(v.X, max.Y, max.Z)
                        | _ -> max
                    
                    Box3d(min, newMax)
                | FaceBack | FaceBottom | FaceRight ->
                    let newMin =
                        match ft with
                        | FaceBack -> V3d(min.X, v.Y, min.Z)
                        | FaceBottom -> V3d(min.X, min.Y, v.Z)
                        | FaceRight -> V3d(v.X, min.Y, min.Z)
                        | _ -> min

                    Box3d(newMin, max)

            let geom =
                if inside v m.geom
                then shrinkBox v ft m.geom
                else m.geom.ExtendedBy(v)
            
            let size = geom.Size
            let euler = Rot3d.FromM33d( m.trafo.Forward.UpperLeftM33() ).GetEulerAngles()
            let newTrafo = Trafo3d.Rotation(euler)

            let newCenter =
                let value = m.center + geom.Center
                let centerTrafo = M44d.Translation(m.center) * newTrafo.Forward * M44d.Translation(-m.center)
                centerTrafo.TransformPos(value)

            let geom = Box3d.FromCenterAndSize(V3d.OOO, size)
            {m with size = size; center = newCenter; trafo = newTrafo; geom = geom; values = BoxValues.setup' newCenter size m.name}
        
        | Rotate (axis, angle) ->
            match axis with
            | Axis.X ->
                match m.trafoMode with
                | TrafoMode.Local -> {m with trafo = Trafo3d.RotationX(angle) * m.trafo }
                | TrafoMode.Global -> {m with trafo = m.trafo * Trafo3d.RotationX(angle) }
                | _ -> m
            | Axis.Y ->
                match m.trafoMode with
                | TrafoMode.Local -> {m with trafo = Trafo3d.RotationY(angle) * m.trafo }
                | TrafoMode.Global -> {m with trafo = m.trafo * Trafo3d.RotationY(angle) }
                | _ -> m
            | Axis.Z ->
                match m.trafoMode with
                | TrafoMode.Local -> {m with trafo = Trafo3d.RotationZ(angle) * m.trafo }
                | TrafoMode.Global -> {m with trafo = m.trafo * Trafo3d.RotationZ(angle) }
                | _ -> m
        
        | RotateWithTrafo trafo -> {m with trafo = trafo}
        | ChangeTrafoMode trafoMode -> {m with trafoMode = trafoMode}
        | ChangeRenderOptions msg -> {m with options = RenderOptions.update m.options msg}
        | ToggleCentering -> {m with centering = not m.centering}

        | ValuesMessage msg ->
            let values = BoxValues.update m.values msg
            let center = values.center.value
            let size = values.size.value
            let rotation = Rot3d.FromM33d( m.trafo.Forward.UpperLeftM33() ).GetEulerAngles()
            {setup center size rotation values.name with values = values; color = m.color; options = m.options; trafoMode = m.trafoMode; centering = m.centering; display = m.display}
        
        | ToggleDisplay -> {m with display = not m.display}
    
    let mkISg (m : MBoxModel) events =
        let translation = m.center |> Mod.map (fun c -> Trafo3d.Translation(c))
        
        let box =
            m.options.fillMode
            |> Mod.map ( fun x ->
                match x with
                | BoxFillMode.Solid ->
                    Sg.box m.color m.geom
                    |> Sg.shader {
                        do! DefaultSurfaces.trafo
                        do! DefaultSurfaces.vertexColor
                        do! DefaultSurfaces.simpleLighting
                    }
                
                | BoxFillMode.Line ->
                    Sg.wireBox m.color m.geom
                    |> Sg.noEvents
                    |> Sg.pickable (
                        m.geom
                        |> Mod.map ( fun b ->
                            b |> PickShape.Box
                        )
                        |> Mod.force
                    )
                    |> Sg.shader {
                        do! DefaultSurfaces.trafo
                        do! DefaultSurfaces.vertexColor
                    }
                | _ -> Sg.empty
            )
            |> Sg.dynamic
        
        Mod.map2 ( fun blend fill ->
            match blend with
                | false -> box
                | true ->
                    match fill with
                    | BoxFillMode.Solid ->
                        box
                        |> Sg.pass ( RenderPass.after "transparent" RenderPassOrder.BackToFront RenderPass.main )
                        |> Sg.blendMode (Mod.constant BlendMode.Blend)
                    | _ -> box
        ) m.options.blending m.options.fillMode
        |> Mod.map2 ( fun d sg ->
            if d then sg else Sg.empty
        ) m.display
        |> Sg.dynamic
        |> Sg.requirePicking
        |> Sg.noEvents
        |> Sg.withEvents events
        |> Sg.trafo m.trafo
        |> Sg.trafo translation
        |> Sg.cullMode m.options.cullMode
    
    let mkColor (state : BoxSelectionState) =
        let color =
            if state.selected
            then Config.selectColor
            else Config.unselectColor
        
        let color =
            if state.trafoBoxMode && state.selected
            then Config.trafoColor
            else color
        
        let color =
            if state.editMode && state.selected
            then Config.editColor
            else color
        
        color
    
    module BoxSelectionState =
        let initial =
            {
                selected        = false
                trafoBoxMode    = false
                editMode        = false
            }
