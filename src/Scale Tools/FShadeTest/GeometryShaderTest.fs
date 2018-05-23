namespace GeometryShaderTest

open Aardvark.Base
open Aardvark.Base.Rendering
open Aardvark.Base.Rendering.Effects
open Aardvark.Base.Vec
open Aardvark.Base.Incremental
open Aardvark.SceneGraph
open Aardvark.UI
open Aardvark.UI.Primitives

module Shader =
    open FShade
    open FShade.InstrinsicAttributes
    open FShade.UniformExtensions
    open Aardvark.SceneGraph.Semantics
    open Aardvark.Application
    
    let transform (v : Effects.Vertex) =
        vertex {
            let wp = uniform.ModelTrafo * v.pos
            let p = uniform.ViewProjTrafo * wp
            let n = uniform.NormalMatrix * v.n
            let c = v.c
            let tc = v.tc
            
            return
                {
                    v with
                        pos = p
                        wp  = wp
                        n   = n
                        c   = c
                        tc  = tc
                }
        }

    let shade (v : Effects.Vertex) =
        fragment {
            return {v with c = V4d(v.tc.X, v.tc.Y, 0.0, 1.0)}
        }

module App =
    
    type GeomAction =
        | CameraMessage of CameraController.Message
    
    let update (scene : Scene) (act : GeomAction) =
        match act with
        | CameraMessage msg -> {scene with camera = CameraController.update scene.camera msg}
    
    let viewScene (scene : MScene) =
        Sg.sphere' 7 C4b.Red 0.5
        |> Sg.noEvents
        |> Sg.shader {
            //do! DefaultSurfaces.trafo
            //do! DefaultSurfaces.vertexColor
            //do! DefaultSurfaces.simpleLighting
            do! Shader.transform
            do! DefaultSurfaces.simpleLighting
            //do! Shader.shade
        }

    let view (scene : MScene) =
        let frustum = Frustum.perspective 60.0 0.1 100.0 1.0
        require Html.semui (
            div [clazz "ui"][
                CameraController.controlledControl scene.camera CameraMessage (Mod.constant frustum)
                    (AttributeMap.ofList[
                        attribute "style" "width:100%; height:100%; float:left"
                    ]) (viewScene scene)
            ]
        )
    
    let app =
        {
            unpersist   = Unpersist.instance
            threads     = fun (scene : Scene) -> CameraController.threads scene.camera |> ThreadPool.map CameraMessage
            initial     = {camera = CameraController.initial}
            update      = update
            view        = view
        }
    
    let start() = App.start app