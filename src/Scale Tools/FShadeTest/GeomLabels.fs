namespace GeomLabels

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.Rendering
open Aardvark.Rendering.Text
open Aardvark.UI
open Aardvark.UI.Trafos
open Aardvark.UI.Primitives

module Shader =
    
    open FShade

    let simpleTransform (v : Effects.Vertex) =
        vertex {
            return
                {
                    v with
                        wp  = uniform.ModelTrafo * v.pos
                        pos = uniform.ModelViewProjTrafo * v.pos
                        n   = uniform.NormalMatrix * v.n
                }
        }
    
    let shrink (v : Triangle<Effects.Vertex>) =
        triangle {
            let cp = (v.P0.wp + v.P1.wp + v.P2.wp) / 3.0
            let r = 0.9

            let a = V4d(cp.XYZ * (1.0 - r) + v.P0.wp.XYZ * r, v.P0.wp.W)
            let b = V4d(cp.XYZ * (1.0 - r) + v.P1.wp.XYZ * r, v.P1.wp.W)
            let c = V4d(cp.XYZ * (1.0 - r) + v.P2.wp.XYZ * r, v.P2.wp.W)

            yield { v.P0 with pos = uniform.ViewProjTrafo * a; wp = a }
            yield { v.P1 with pos = uniform.ViewProjTrafo * b; wp = b }
            yield { v.P2 with pos = uniform.ViewProjTrafo * c; wp = c }
        }

    let explode (tri : Triangle<Effects.Vertex>) =
        triangle {
            let offset0 = tri.P0.n * 0.8
            let offset1 = tri.P1.n * 0.8
            let offset2 = tri.P2.n * 0.8

            let a = V4d(tri.P0.wp.XYZ + offset0, 1.0)
            let b = V4d(tri.P1.wp.XYZ + offset1, 1.0)
            let c = V4d(tri.P2.wp.XYZ + offset2, 1.0)
            
            yield {tri.P0 with pos = uniform.ViewProjTrafo * a; wp = a}
            yield {tri.P1 with pos = uniform.ViewProjTrafo * b; wp = b}
            yield {tri.P2 with pos = uniform.ViewProjTrafo * c; wp = c}
            endPrimitive()

            let a1 = V4d(tri.P0.wp.XYZ + offset2, 1.0)
            let b1 = V4d(tri.P1.wp.XYZ + offset0, 1.0)
            let c1 = V4d(tri.P2.wp.XYZ + offset1, 1.0)

            yield {tri.P0 with pos = uniform.ViewProjTrafo * a1; wp = a1}
            yield {tri.P1 with pos = uniform.ViewProjTrafo * b1; wp = b1}
            yield {tri.P2 with pos = uniform.ViewProjTrafo * c1; wp = c1}
            endPrimitive()
        }
    
    let tester (v : Triangle<Effects.Vertex>) =
        triangle {
            yield v.P0
            yield v.P1
            yield v.P2
        }

    let simpleColor (v : Effects.Vertex) =
        fragment {
            return {v with c = V4d(1.0, 1.0, 1.0, 1.0)}
        }

module App =
    
    type Action =
        | CameraMessage of CameraController.Message
    
    let update (m : Model) (act : Action) =
        match act with
        | CameraMessage msg -> {m with camera = CameraController.update m.camera msg}
    
    let viewScene (m : MModel) =

        let ig = IndexedGeometryPrimitives.Box.solidBox (Box3d(-V3d.OOO, V3d.OOO)) C4b.Red
        
        Sg.box' C4b.Red (Box3d(-V3d.III, V3d.III))
        |> Sg.noEvents
        |> Sg.shader {
            do! DefaultSurfaces.trafo
            do! Shader.shrink
            do! Shader.explode
            do! DefaultSurfaces.vertexColor
            do! DefaultSurfaces.simpleLighting
        }
    
    let view (m : MModel) =
        let frustum = Mod.constant (Frustum.perspective 60.0 0.1 100.0 1.0)
        require Html.semui (
            div [clazz "ui"][
                CameraController.controlledControl m.camera CameraMessage frustum
                    (AttributeMap.ofList[
                        attribute "style" "background:#121212; width:100%; height:100%"
                    ]) (viewScene m)
            ]
        )
    
    let threads (m : Model) =
        CameraController.threads m.camera |> ThreadPool.map CameraMessage
    
    let initial =
        {
            camera = CameraController.initial
        }

    let app =
        {
            unpersist   = Unpersist.instance
            threads     = threads
            initial     = initial
            update      = update
            view        = view
        }
    
    let start() = App.start app