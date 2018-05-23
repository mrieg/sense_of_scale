namespace PickingTest

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.Rendering
open Aardvark.UI
open Aardvark.UI.Trafos
open Aardvark.UI.Primitives

module App =
    
    type Action =
        | CameraMessage of CameraController.Message
        | Clicked       of V3d
        | Enter         of V3d
    
    let update (m : Model) (act : Action) =
        match act with
        | CameraMessage msg -> {m with camera = CameraController.update m.camera msg}
        | Clicked v -> {m with pos = v}
        | Enter v -> {m with pos = v}
    
    let viewScene (m : MModel) =
        //let geom = Aardvark.SceneGraph.IO.Loader.Assimp.load @"../../data/models/Rock1/Rock1_clean.obj"
        //let geom = Aardvark.SceneGraph.IO.Loader.Assimp.load @"../../data/models/house/hOUSE.obj"
        let geom = Aardvark.SceneGraph.IO.Loader.Assimp.load @"../../data/models/mountain/MountainTerrain.obj"

        let scaleTrafo = Mod.constant (Trafo3d.Scale(0.032))
        
        let pickMeshes =
            [
                for mesh in geom.meshes do
                    yield mesh.geometry.Sg |> Sg.noEvents
            ]
            |> Sg.ofList
        
        let pickSg =
            pickMeshes
            |> Sg.requirePicking
            |> Sg.noEvents
            |> Sg.trafo scaleTrafo
            |> Sg.withEvents [
                Sg.onEnter ( fun v -> Enter v )
                Sg.onMouseDown ( fun b v -> Clicked v )
            ]
            |> Sg.shader {
                do! DefaultSurfaces.trafo
                do! DefaultSurfaces.constantColor C4f.White
            }
            |> Sg.depthTest (Mod.constant DepthTestMode.Never)
        
        let modelSg =
            geom
            |> Sg.adapter
            |> Sg.noEvents
            |> Sg.trafo scaleTrafo
            |> Sg.shader {
                do! DefaultSurfaces.trafo
                do! DefaultSurfaces.diffuseTexture
                do! DefaultSurfaces.simpleLighting
            }

        let sphereSg =
            Sg.sphere' 5 C4b.White 0.25
            |> Sg.noEvents
            |> Sg.translate' m.pos
            |> Sg.shader {
                do! DefaultSurfaces.trafo
                do! DefaultSurfaces.vertexColor
                do! DefaultSurfaces.simpleLighting
            }
        
        [pickSg; modelSg; sphereSg]
        |> Sg.ofList

    
    let view (m : MModel) =
        let frustum = Mod.constant (Frustum.perspective 60.0 0.1 100.0 1.0)
        require Html.semui (
            CameraController.controlledControl m.camera CameraMessage frustum
                (AttributeMap.ofList[
                    attribute "style" "background:#121212; width:100%; height:100%"
                ]) (viewScene m)
        )
    
    let threads (m : Model) =
        CameraController.threads m.camera |> ThreadPool.map CameraMessage
    
    let initial = {camera = CameraController.initial; pos = V3d.OOO}

    let app =
        {
            unpersist = Unpersist.instance
            threads = threads
            initial = initial
            update = update
            view = view
        }
    
    let start() = App.start app






    //pickshape + kdtree
    //let s = Geometry.Spatial.triangle
    //let i = Geometry.KdBuildInfo.Default
    //let p0 = V3d(1.0, 1.0, -1.0)
    //let p1 = V3d(-1.0, 1.0, -1.0)
    //let p2 = V3d(-1.0, -1.0, -1.0)
    //let p3 = V3d(1.0, -1.0, -1.0)
    //let p4 = V3d(0.0, 0.0, 1.0)
    //let t0 = Triangle3d(p0, p1, p2)
    //let t1 = Triangle3d(p0, p2, p3)
    //let t2 = Triangle3d(p0, p1, p4)
    //let t3 = Triangle3d(p1, p2, p4)
    //let t4 = Triangle3d(p2, p3, p4)
    //let t5 = Triangle3d(p3, p0, p4)
    //let d = [|t0; t1; t2; t3; t4; t5|]
    //let kdTree = Geometry.KdTree.build s i d
    //let ps = PickShape.Triangles kdTree
    //Sg.ofIndexedGeometry ( IndexedGeometryPrimitives.Triangle.solidTrianglesWithColor d C4b.Red )
    //|> Sg.pickable ps