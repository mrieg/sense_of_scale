namespace DistanceShading

open Aardvark.Base
open Aardvark.Base.Rendering

module Shader =
    
    open FShade

    type UniformScope with
        member x.CenterPosition : V4d   = uniform?CenterPosition
        member x.Radius         : float = uniform?Radius
        member x.StartColor     : V4d   = uniform?StartColor
        member x.EndColor       : V4d   = uniform?EndColor
        member x.LineColor      : V4d   = uniform?LineColor
        member x.NumLines       : int   = uniform?NumLines
        member x.LineWidth      : float = uniform?LineWidth
        member x.DrawLines      : bool  = uniform?DrawLines
        member x.CelColors      : bool  = uniform?CelColors
        member x.AlphaValue     : float = uniform?AlphaValue
    
    [<ReflectedDefinition>]
    let lerp (t : float) =
        (1.0-t) * uniform.StartColor + t * uniform.EndColor
    
    [<ReflectedDefinition>]
    let lerpStep (t : float) =
        let n = float uniform.NumLines
        let s = int (n * t)
        let nt = float s / n
        lerp nt

    [<ReflectedDefinition>]
    let drawLine (t : float) =
        let n = uniform.NumLines
        let d = 1.0 / float n
        let vpDist = V3d.Distance(uniform.CameraLocation, uniform.CenterPosition.XYZ)
        let w = uniform.LineWidth * (1.0 / uniform.Radius) * 0.05 * vpDist
        let k = t % d

        if k < w
        then true
        else false

    let distShading (v : Effects.Vertex) =
        fragment {
            let d = V4d.Distance(v.wp, uniform.CenterPosition)
            let r = uniform.Radius
            let a = uniform.AlphaValue

            let c =
                if d > r
                then v.c
                else
                    let t = d / r
                    let col = if uniform.CelColors then lerpStep t else lerp t
                    let col = (1.0 - a) * v.c + a * col
                    if not uniform.DrawLines
                    then col
                    else
                        if drawLine t
                        then uniform.LineColor
                        else col
            
            return {v with c = c}
        }