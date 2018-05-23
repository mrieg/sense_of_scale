namespace BoxManip

open Aardvark.Base
open Aardvark.Base.IO
open Aardvark.Base.Rendering
open Aardvark.Base.Rendering.Effects

module Shaders =
    
    open FShade

    type Vertex =
        {
            [<Position>]                pos     : V4d
            [<WorldPosition>]           wp      : V4d
            [<Color>]                   col     : V4d
            [<Normal>]                  n       : V3d
            [<TexCoord>]                tc      : V2d
        }
    
    type UniformScope with
        member x.CamLoc : V3d = uniform?CamLoc
        member x.Near : float = uniform?Near
        member x.HFov : float = uniform?HFov
        member x.Size : float = uniform?Size
    
    let transformScaleInv (v : Vertex) =
        vertex {
            let wp = uniform.ModelTrafo * v.pos

            let hfov_rad = uniform?HFov * Constant.Pi / 180.0
            let wz = Fun.Tan(hfov_rad / 2.0) * uniform?Near * uniform?Size
            let dist = V3d.Distance(wp.XYZ, uniform?CamLoc)
            let scale = ( wz / uniform?Near ) * dist

            let scaleMat = M44d(scale, 0.0, 0.0, 0.0, 0.0, scale, 0.0, 0.0, 0.0, 0.0, scale, 0.0, 0.0, 0.0, 0.0, 1.0)

            let pos = uniform.ModelViewProjTrafo * scaleMat * v.pos
            let wp = uniform.ModelTrafo * scaleMat * v.pos
            let n = uniform.NormalMatrix * v.n

            return {
                v with
                    pos = pos
                    wp = wp
                    n = n
            }
        }
    
    let shade (v : Vertex) =
        fragment {
            return v
        }
