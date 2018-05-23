namespace ScaleBar

open Aardvark.Base
open Aardvark.Base.Rendering

module Shader =
    
    open FShade

    type UniformScope with
        member x.Height   : float = uniform?Height
        member x.Color1   : V4d   = uniform?Color1
        member x.Color2   : V4d   = uniform?Color2
    
    let setTC (v : Effects.Vertex) =
        vertex {
            let tc = if v.pos.Z = 0.0 then V2d(0.0, 0.0) else V2d(0.0, 1.0)
            return {v with tc = tc}
        }

    let shadeScaleBar (v : Effects.Vertex) =
        fragment {
            let s1 = 0.125
            let s2 = 0.25
            let s3 = 0.5
            let tc = v.tc.Y
            let c1 = uniform.Color1
            let c2 = uniform.Color2
            
            let col =
                if tc < s1 
                then c1 
                else if tc >= s1 && tc < s2
                then c2
                else if tc >= s2 && tc < s3
                then c1
                else if tc >= s3
                then c2
                else c1

            return {v with c = col}
        }