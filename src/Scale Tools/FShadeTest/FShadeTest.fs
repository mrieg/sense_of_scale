namespace FShadeTest

open Aardvark.Base
open Aardvark.Base.Rendering
open Aardvark.Base.Rendering.Effects
open Aardvark.Base.Vec
open Aardvark.Base.Incremental
open Aardvark.SceneGraph
open Aardvark.UI
open Aardvark.UI.Primitives

open FShapeTestModel

module Shader =
    open FShade
    open FShade.InstrinsicAttributes
    open FShade.UniformExtensions
    open Aardvark.SceneGraph.Semantics
    open Aardvark.Application

    type Vertex =
        {
            [<Position>]    pos     : V4d
            [<Color>]       col     : V4d
            [<Normal>]      n       : V3d
        }
    
    type UniformScope with
        member x.MyUniform : V4d = uniform?MyUniform

    let transform (v : Vertex) =
        vertex {
            return {
                pos = uniform.ModelViewProjTrafo * v.pos
                col = v.col
                n = uniform.NormalMatrix * v.n
            }
        }
    
    let shade (v : Vertex) =
        let lightPos = V3d(10.0, 10.0, 10.0)
        fragment {
            let lDir = lightPos - V3d(v.pos.X, v.pos.Y, v.pos.Z)
            let dotNL = max 0.0 (V3d.Dot(lDir.Normalized, v.n.Normalized))
            let col = V3d(uniform.MyUniform) * dotNL
            let col = col + V3d(0.05, 0.05, 0.05)
            return {v with col = V4d(col, 1.0)}
        }

module FShapeTest =

    let initial =
        {
            camera = CameraController.initial
        }
    
    type FshadeTestAction =
        | CameraMessage of CameraController.Message
        
    let update (scene : Scene) (a : FshadeTestAction) =
        match a with
        | CameraMessage msg -> {scene with camera = CameraController.update scene.camera msg}
    
    let viewScene (scene : MScene) =
        Sg.sphere 5 (Mod.constant C4b.Red) (Mod.constant 1.0)
        |> Sg.noEvents
        |> Sg.uniform "MyUniform" (Mod.constant (V4d(0.0, 0.0, 1.0, 1.0)))
        |> Sg.effect [
            Shader.transform |> toEffect
            Shader.shade |> toEffect
        ]

    let view (scene : MScene) =
        let frustum = Frustum.perspective 60.0 0.1 100.0 1.0 |> Mod.constant
        require Html.semui (
            CameraController.controlledControl scene.camera CameraMessage frustum
                (AttributeMap.ofList[
                    attribute "style" "width:100%; height:100%"
                ]) (viewScene scene)
        )
    
    let app =
        {
            unpersist = Unpersist.instance
            threads = fun (scene : Scene) -> CameraController.threads scene.camera |> ThreadPool.map CameraMessage
            initial = initial
            update = update
            view = view
        }

    let start() = App.start app
