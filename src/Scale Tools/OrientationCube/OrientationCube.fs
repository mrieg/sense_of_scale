namespace OrientationCube

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.Rendering
open Aardvark.UI
open Aardvark.UI.Trafos
open Aardvark.UI.Primitives

module Shader =
    
    open FShade
    
    let aspectTrafo (v : Effects.Vertex) =
        vertex {
            let vps = uniform.ViewportSize
            let aspect = (float vps.X) / (float vps.Y)
            let tx = 0.75
            let ty = 0.75
            return {v with pos = V4d(v.pos.X / aspect + tx, v.pos.Y + ty, v.pos.Z, v.pos.W)}
        }
    
    let samplerAniso =
        sampler2d {
            texture uniform.DiffuseColorTexture
            filter Filter.Anisotropic
        }

    let anisoTexShader (v : Effects.Vertex) =
        fragment {
            let c = samplerAniso.Sample v.tc
            return {v with c = c}
        }
        
module Sg =
    
    let loadModel (filename : string) =
        Aardvark.SceneGraph.IO.Loader.Assimp.load filename
        |> Sg.adapter
        |> Sg.noEvents
    
    let orthoOrientation (camView : IMod<CameraView>) (model : ISg<'msg>) =
        let viewTrafo =
            camView
            |> Mod.map ( fun cv ->
                let view = CameraView.look V3d.OOO cv.Forward cv.Sky
                view.ViewTrafo
            )
        
        let orthoTrafo =
            let d = 5.0
            let t = V3d((-d+1.0), -d+1.0, 0.0)
            let min = V3d(-d, -d, -d*2.0)
            let max = V3d(d, d, d*2.0)
            let fr = Frustum.ortho (Box3d(min, max))
            Mod.constant (Frustum.orthoTrafo fr)
        
        model
        |> Sg.trafo (Mod.constant (Trafo3d.Scale(1.0,1.0,-1.0)))
        |> Sg.trafo (Mod.constant (Trafo3d.RotationXInDegrees(90.0)))
        |> Sg.trafo ( camView |> Mod.map ( fun v ->  Trafo3d.RotateInto(V3d.OOI, v.Sky) ) )
        |> Sg.viewTrafo viewTrafo
        |> Sg.projTrafo orthoTrafo
        |> Sg.shader {
            do! DefaultSurfaces.trafo
            do! Shader.aspectTrafo
            do! Shader.anisoTexShader
        }
        |> Sg.pass (RenderPass.after "cube" RenderPassOrder.Arbitrary RenderPass.main)
    
    let insideOrientation (camView : IMod<CameraView>) (frustum : IMod<Frustum>) (model : ISg<'msg>) =
        let viewTrafo =
            camView
            |> Mod.map ( fun cv ->
                let view = CameraView.look V3d.OOO cv.Forward V3d.OOI
                view.ViewTrafo
            )
        
        let perspTrafo = frustum |> Mod.map ( fun f -> Frustum.projTrafo f)
        
        model
        |> Sg.trafo (Mod.constant (Trafo3d.Scale(100.0, 100.0, 100.0)))
        |> Sg.viewTrafo viewTrafo
        |> Sg.projTrafo perspTrafo
        |> Sg.shader {
            do! DefaultSurfaces.trafo
            do! DefaultSurfaces.diffuseTexture
        }
        |> Sg.pass (RenderPass.after "cube" RenderPassOrder.Arbitrary RenderPass.main)
    
module App =
    
    type Action =
        | CameraMessage of CameraController.Message

    let update (m : Model) (act : Action) =
        match act with
        | CameraMessage msg -> {m with camera = CameraController.update m.camera msg}
    
    let testSg (m : MModel) =
        Sg.box (Mod.constant C4b.Red) (Mod.constant (Box3d.FromCenterAndSize(V3d.OOO, V3d.III)))
        |> Sg.shader {
            do! DefaultSurfaces.trafo
            do! DefaultSurfaces.vertexColor
            do! DefaultSurfaces.simpleLighting
        }

    let viewScene (m : MModel) (frustum : IMod<Frustum>) =
        let filename = "../../data/models/rotationcube/rotationcube.dae"
        let model = Sg.loadModel filename
        [Sg.orthoOrientation m.camera.view model; Sg.insideOrientation m.camera.view frustum model; testSg m]
        |> Sg.ofList

    let view (m : MModel) =
        let frustum = Mod.constant (Frustum.perspective 60.0 0.1 100.0 1.0)
        require Html.semui (
            div [clazz "ui"][
                CameraController.controlledControl m.camera CameraMessage frustum
                    (AttributeMap.ofList[
                        attribute "style" "background:#121212; width:100%; height:100%"
                    ]) (viewScene m frustum)
            ]
        )
    
    let initial =
        {
            camera  = CameraController.initial
        }
    
    let threads (m : Model) =
        CameraController.threads m.camera |> ThreadPool.map CameraMessage

    let app =
        {
            unpersist = Unpersist.instance
            threads = threads
            initial = initial
            update = update
            view = view
        }
    
    let start() = App.start app