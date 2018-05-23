namespace VerticalExaggeration

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.UI

module Shader =
    
    open FShade
    
    type UniformScope with
        member x.Exaggeration : float = uniform?Exaggeration
        member x.SkyRotation  : M44d  = uniform?SkyRotation
    
    let vertExagg (v : Effects.Vertex) =
        vertex {
            let wp = uniform.SkyRotation.Inverse * v.wp
            let wp = V4d(wp.X, wp.Y, wp.Z * uniform.Exaggeration, wp.W)
            let wp = uniform.SkyRotation * wp
            let pos = uniform.ViewProjTrafo * wp
            return {v with pos = pos; wp = wp}
        }

module App =
    
    type Action =
        | ValueMessage of Numeric.Action
    
    let update (m : Model) (act : Action) =
        match act with
        | ValueMessage msg -> {m with value = Numeric.update m.value msg}
    
    let mkEffects (m : MModel) (view : IMod<CameraView>) effects sg =
        let efx =
            [
                for e in effects do
                    let x = DefaultSurfaces.trafo |> toEffect
                    if e = x
                    then
                        yield e
                        yield Shader.vertExagg |> toEffect
                    else yield e
            ]
        
        sg
        |> Sg.uniform "Exaggeration" m.value.value
        |> Sg.uniform "SkyRotation"  ( view |> Mod.map (fun v -> Trafo3d.RotateInto(V3d.OOI, v.Sky.Normalized).Forward) )
        |> Sg.effect efx
    
    let view' (m : MModel) =
        Html.SemUi.stuffStack [
            Numeric.view' [NumericInputType.Slider]   m.value |> UI.map ValueMessage
            Numeric.view' [NumericInputType.InputBox] m.value |> UI.map ValueMessage
        ]
    
    let initial = {value = {Numeric.init with value = 1.0; step = 0.01; min = 0.0; max = 3.0}}

module Debug =
    
    let scene (view : IMod<CameraView>) =
        let box = Box3d(V3d(-0.1, -0.1, -500.0), V3d(0.1, 0.1, 500.0))
        Sg.box' C4b.White box
        |> Sg.noEvents
        |> Sg.trafo ( Trafo3d.Translation(V3d(2.0, 6.0, 0.0)) |> Mod.constant )
        |> Sg.andAlso (
            Sg.box' C4b.White box
            |> Sg.noEvents
            |> Sg.trafo ( Trafo3d.Translation(V3d(4.0, 9.0, 0.0)) |> Mod.constant )
        )
        |> Sg.andAlso (
            Sg.box' C4b.White box
            |> Sg.noEvents
            |> Sg.trafo ( Trafo3d.Translation(V3d(-4.0, 9.0, 0.0)) |> Mod.constant )
        )
        |> Sg.trafo ( view |> Mod.map ( fun v -> Trafo3d.RotateInto(V3d.OOI, v.Sky.Normalized) ) )
        |> Sg.effect [
            DefaultSurfaces.trafo          |> toEffect
            DefaultSurfaces.vertexColor    |> toEffect
            DefaultSurfaces.simpleLighting |> toEffect
        ]