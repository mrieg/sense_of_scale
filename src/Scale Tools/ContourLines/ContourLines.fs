namespace ContourLines

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.Rendering
open Aardvark.SceneGraph
open Aardvark.UI
open Aardvark.UI.Primitives

module Shader =
    
    open FShade

    type UniformScope with
        member this.SkyTrafo : M44d  = uniform?SkyTrafo
        member this.Inc      : float = uniform?Inc
        member this.Offset   : float = uniform?Offset
        member this.LineCol  : V4d   = uniform?LineCol
    
    let heightLines (v : Effects.Vertex) =
        fragment {
            let z = uniform.SkyTrafo.TransformPos(v.wp.XYZ).Z + uniform.Offset
            let m = z % uniform.Inc

            let vpDist = V3d.Distance(uniform.CameraLocation, v.wp.XYZ)
            let width = 0.002 * vpDist
            
            let c = uniform.LineCol

            let a = 0.25
            let c = (1.0 - a) * c + a * v.c

            let c =
                if m <= width
                then c
                else v.c

            return {v with c = c}
        }
    
module App =
    
    type Action =
        | IncMessage    of Numeric.Action
        | OffsetMessage of Numeric.Action
    
    let update (m : Model) (act : Action) =
        match act with
        | IncMessage msg    -> {m with inc = Numeric.update m.inc msg}
        | OffsetMessage msg -> {m with offset = Numeric.update m.offset msg}

    let mkEffects (m : MModel) (v : IMod<CameraView>) effects sg =
        let efx =
            [
                for e in effects do
                    yield e
                yield Shader.heightLines |> toEffect
            ]

        sg
        |> Sg.uniform "Inc"      m.inc.value
        |> Sg.uniform "SkyTrafo" (v |> Mod.map ( fun x -> Trafo3d.RotateInto(V3d.OOI, x.Sky).Forward ))
        |> Sg.uniform "Offset"   m.offset.value
        |> Sg.uniform "LineCol"  (V4d(1.0, 1.0, 0.4, 1.0) |> Mod.constant)
        |> Sg.effect efx
    
    let view' (m : MModel) =
        div [clazz "ui"][
            Html.table [
                Html.row "Increment: " [
                    Numeric.view m.inc |> UI.map IncMessage
                ]
                Html.row "Offset: " [
                    Numeric.view m.offset |> UI.map OffsetMessage
                ]
            ]
        ]

    let initial =
        let inc =
            {
                Numeric.init with
                    min = 0.0001
                    max = 50.0
                    step = 0.1
                    value = 1.0
            }
        
        let offset =
            {
                Numeric.init with
                    min = -50.0
                    max = 50.0
                    step = 0.01
                    value = 0.0
            }
        
        {
            inc = inc
            offset = offset
        }