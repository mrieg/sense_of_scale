namespace ScaleBar

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.Rendering
open Aardvark.Rendering.Text
open Aardvark.UI
open Aardvark.UI.Primitives

module ScaleBar =
    
    type Action =
        | SetHeight        of float
        | SetPosition      of V3d
        | SetColors        of V4d * V4d
    
    let update (m : ScaleBarModel) (act : Action) =
        match act with
        | SetHeight h       -> {m with height = h}
        | SetPosition p     -> {m with pos = p}
        | SetColors (c1,c2) -> {m with color1 = c1; color2 = c2}
    
    let radius = 0.032
    let alignToView (view : IMod<CameraView>) =
            view
            |> Mod.map ( fun v ->
                Trafo3d.RotateInto(V3d.IOO, v.Right)
            )
    
    let scaleFactor = 0.019
    let sMin = 0.005
    let drawLabelsHorizontal (m : MScaleBarModel) (view : IMod<CameraView>) =
        let font = Font.create "arial" FontStyle.Regular
        let spaceToBar =
            Mod.map2 ( fun (v : CameraView) (p : V3d) ->
                let vp = v.Location
                let d  = V3d.Distance(vp, p)
                let s  = scaleFactor * d
                let s  = if s < sMin then sMin else s
                let r  = v.Up * radius * 1.25
                Trafo3d.Scale(s) * Trafo3d.Translation(r)
            ) view m.pos

        aset {
            let n = 2
            for i in 0 .. n do
                let height  = float i / float n
                let content = m.height |> Mod.map ( fun h -> height * h |> sprintf "%.2f m" )
                let heightTrafo =
                    view
                    |> Mod.map( fun v ->
                        Trafo3d.Translation(height * v.Right.Normalized)
                    )

                yield
                    Sg.textWithBackground font C4b.White C4b.DarkRed Border2d.None content
                    |> Sg.billboard
                    |> Sg.noEvents
                    |> Sg.trafo ( m.height |> Mod.map ( fun h -> Trafo3d.Scale(1.0/h) ) )
                    |> Sg.trafo spaceToBar
                    |> Sg.trafo heightTrafo
                    |> Sg.trafo ( m.height |> Mod.map ( fun h -> Trafo3d.Scale(h) ) )
                    |> Sg.trafo ( m.pos |> Mod.map ( fun p -> Trafo3d.Translation(p) ) )
        }
        |> Sg.set

    let drawLabelsVertical (m : MScaleBarModel) (view : IMod<CameraView>) =
        let font  = Font.create "arial" FontStyle.Regular
        let spaceToBar =
            Mod.map2 ( fun (v : CameraView) (p : V3d) ->
                let vp   = v.Location
                let d    = V3d.Distance(vp, p)
                let s    = scaleFactor * d
                let s    = if s < sMin then sMin else s
                let r    = v.Right * radius * 1.25
                Trafo3d.Scale(s) * Trafo3d.Translation(r)
            ) view m.pos
        
        aset {
            let n = 2
            for i in 0 .. n do
                let height      = float i / float n
                let content     = m.height |> Mod.map ( fun h -> height * h |> sprintf "%.2f m" )
                let heightTrafo = Mod.constant ( Trafo3d.Translation(V3d(0.0, 0.0, height)) )
                yield
                    Sg.textWithBackground font C4b.White C4b.DarkRed Border2d.None content
                    |> Sg.billboard
                    |> Sg.noEvents
                    |> Sg.trafo ( m.height |> Mod.map ( fun h -> Trafo3d.Scale(1.0/h) ) )
                    |> Sg.trafo spaceToBar
                    |> Sg.trafo ( view |> Mod.map ( fun v -> Trafo3d.RotateInto(V3d.OOI, v.Sky).Inverse ) )
                    |> Sg.trafo heightTrafo
                    |> Sg.trafo ( m.height |> Mod.map ( fun h -> Trafo3d.Scale(h) ) )
                    |> Sg.trafo ( view |> Mod.map ( fun v -> Trafo3d.RotateInto(V3d.OOI, v.Sky) ) )
                    |> Sg.trafo ( m.pos |> Mod.map ( fun p -> Trafo3d.Translation(p) ) )
        }
        |> Sg.set

    let drawBarHorizontal (m : MScaleBarModel) (view : IMod<CameraView>) =
        let scale = m.height |> Mod.map ( fun h -> Trafo3d.Scale(h) )
        let rotationY = Mod.constant (Trafo3d.RotationYInDegrees(90.0))

        Sg.cylinder 12 (Mod.constant C4b.White) (Mod.constant radius) (Mod.constant 1.0)
        |> Sg.noEvents
        |> Sg.trafo scale
        |> Sg.trafo rotationY
        |> Sg.trafo (alignToView view)
        |> Sg.translate' m.pos
        |> Sg.uniform "Height"   m.height
        |> Sg.uniform "Color1"   m.color1
        |> Sg.uniform "Color2"   m.color2
        |> Sg.shader {
            do! Shader.setTC
            do! DefaultSurfaces.trafo
            do! Shader.shadeScaleBar
        }
        |> Sg.andAlso (drawLabelsHorizontal m view)

    let drawBarVertical (m : MScaleBarModel) (view : IMod<CameraView>) =
        let scale = m.height |> Mod.map ( fun h -> Trafo3d.Scale(h) )
        Sg.cylinder 12 (Mod.constant C4b.White) (Mod.constant radius) (Mod.constant 1.0)
        |> Sg.noEvents
        |> Sg.trafo scale
        |> Sg.trafo ( view |> Mod.map ( fun v -> Trafo3d.RotateInto(V3d.OOI, v.Sky) ) )
        |> Sg.translate' m.pos
        |> Sg.uniform "Height"   m.height
        |> Sg.uniform "Color1"   m.color1
        |> Sg.uniform "Color2"   m.color2
        |> Sg.shader {
            do! Shader.setTC
            do! DefaultSurfaces.trafo
            do! Shader.shadeScaleBar
        }
        |> Sg.andAlso (drawLabelsVertical m view)
    
    //--------------------------------------------------------------------------------------------------------------------------------
    //Stepped drawing

    let calcHeightStep (pos : IMod<V3d>) (view : IMod<CameraView>) =
        Mod.map2 ( fun (p : V3d) (v : CameraView) ->
            let l = v.Location
            let d = V3d.Distance(p, l)
            let h = d/5.0
            let powBase = 10.0
            let pow = ((Fun.Log h) / (Fun.Log powBase)) |> Fun.Floor
            let v = Fun.Pow(powBase, pow)
            (h / v |> Fun.Floor) * v
            
        ) pos view
    
    let drawLabelsHorizontalStepped (m : MScaleBarModel) (view : IMod<CameraView>) =
        let font = Font.create "arial" FontStyle.Regular
        let spaceToBar =
            Mod.map2 ( fun (v : CameraView) (p : V3d) ->
                let vp = v.Location
                let d  = V3d.Distance(vp, p)
                let s  = scaleFactor * d
                let s  = if s < sMin then sMin else s
                let r  = v.Up * radius * 1.25
                Trafo3d.Scale(s) * Trafo3d.Translation(r)
            ) view m.pos

        aset {
            let n = 2
            for i in 0 .. n do
                let height  = float i / float n
                let hs = calcHeightStep m.pos view
                let content = hs |> Mod.map ( fun h -> height * h |> sprintf "%.2f m" )
                let heightTrafo =
                    view
                    |> Mod.map( fun v ->
                        Trafo3d.Translation(height * v.Right.Normalized)
                    )

                yield
                    Sg.textWithBackground font C4b.White C4b.DarkRed Border2d.None content
                    |> Sg.billboard
                    |> Sg.noEvents
                    |> Sg.trafo ( hs |> Mod.map ( fun h -> Trafo3d.Scale(1.0/h) ) )
                    |> Sg.trafo spaceToBar
                    |> Sg.trafo heightTrafo
                    |> Sg.trafo ( hs |> Mod.map ( fun h -> Trafo3d.Scale(h) ) )
                    |> Sg.trafo ( m.pos |> Mod.map ( fun p -> Trafo3d.Translation(p) ) )
        }
        |> Sg.set

    let drawLabelsVerticalStepped (m : MScaleBarModel) (view : IMod<CameraView>) =
        let font  = Font.create "arial" FontStyle.Regular
        let spaceToBar =
            Mod.map2 ( fun (v : CameraView) (p : V3d) ->
                let vp   = v.Location
                let d    = V3d.Distance(vp, p)
                let s    = scaleFactor * d
                let s    = if s < sMin then sMin else s
                let r    = v.Right * radius * 1.25
                Trafo3d.Scale(s) * Trafo3d.Translation(r)
            ) view m.pos
        
        aset {
            let n = 2
            for i in 0 .. n do
                let height      = float i / float n
                let hs = calcHeightStep m.pos view
                let content     = hs |> Mod.map ( fun h -> height * h |> sprintf "%.2f m" )
                let heightTrafo = Mod.constant ( Trafo3d.Translation(V3d(0.0, 0.0, height)) )
                yield
                    Sg.textWithBackground font C4b.White C4b.DarkRed Border2d.None content
                    |> Sg.billboard
                    |> Sg.noEvents
                    |> Sg.trafo ( hs |> Mod.map ( fun h -> Trafo3d.Scale(1.0/h) ) )
                    |> Sg.trafo spaceToBar
                    |> Sg.trafo ( view |> Mod.map ( fun v -> Trafo3d.RotateInto(V3d.OOI, v.Sky).Inverse ) )
                    |> Sg.trafo heightTrafo
                    |> Sg.trafo ( hs |> Mod.map ( fun h -> Trafo3d.Scale(h) ) )
                    |> Sg.trafo ( view |> Mod.map ( fun v -> Trafo3d.RotateInto(V3d.OOI, v.Sky) ) )
                    |> Sg.trafo ( m.pos |> Mod.map ( fun p -> Trafo3d.Translation(p) ) )
        }
        |> Sg.set
    
    let drawBarHorizontalStepped (m : MScaleBarModel) (view : IMod<CameraView>) =
        let hs = calcHeightStep m.pos view
        let scale = hs |> Mod.map ( fun h -> Trafo3d.Scale(h) )
        let rotationY = Mod.constant (Trafo3d.RotationYInDegrees(90.0))

        Sg.cylinder 12 (Mod.constant C4b.White) (Mod.constant radius) (Mod.constant 1.0)
        |> Sg.noEvents
        |> Sg.trafo scale
        |> Sg.trafo rotationY
        |> Sg.trafo (alignToView view)
        |> Sg.translate' m.pos
        |> Sg.uniform "Height"   hs
        |> Sg.uniform "Color1"   m.color1
        |> Sg.uniform "Color2"   m.color2
        |> Sg.shader {
            do! Shader.setTC
            do! DefaultSurfaces.trafo
            do! Shader.shadeScaleBar
        }
        |> Sg.andAlso (drawLabelsHorizontalStepped m view)
    
    let drawBarVerticalStepped (m : MScaleBarModel) (view : IMod<CameraView>) =
        let hs = calcHeightStep m.pos view
        let scale = hs |> Mod.map ( fun h -> Trafo3d.Scale(h) )
        Sg.cylinder 12 (Mod.constant C4b.White) (Mod.constant radius) (Mod.constant 1.0)
        |> Sg.noEvents
        |> Sg.trafo scale
        |> Sg.trafo ( view |> Mod.map ( fun v -> Trafo3d.RotateInto(V3d.OOI, v.Sky) ) )
        |> Sg.translate' m.pos
        |> Sg.uniform "Height"   hs
        |> Sg.uniform "Color1"   m.color1
        |> Sg.uniform "Color2"   m.color2
        |> Sg.shader {
            do! Shader.setTC
            do! DefaultSurfaces.trafo
            do! Shader.shadeScaleBar
        }
        |> Sg.andAlso (drawLabelsVerticalStepped m view)
    
    //END Stepped Drawing
    //--------------------------------------------------------------------------------------------------------------------------------

    let initial =
        {
            pos      = V3d.OOO
            height   = 1.0
            color1   = V4d(0.2, 0.2, 0.2, 1.0)
            color2   = V4d(1.0, 1.0, 1.0, 1.0)
            id       = System.Guid.NewGuid().ToString()
        }
    
    let setup pos height =
        {
            pos      = pos
            height   = height
            color1   = V4d(0.2, 0.2, 0.2, 1.0)
            color2   = V4d(1.0, 1.0, 1.0, 1.0)
            id       = System.Guid.NewGuid().ToString()
        }