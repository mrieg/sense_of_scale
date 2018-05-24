namespace Boxes

//open Aardvark.Base
//open Aardvark.Base.Incremental
//open Aardvark.Base.Rendering
//open Aardvark.SceneGraph
//open Aardvark.UI
//
//module TestScene =
//    
//    let terrainScene =
//        let terrain = IO.Loader.Assimp.load "../../data/models/mountain/MountainTerrain.obj"
//        let terrainTrafo = Mod.constant (Trafo3d.RotationXInDegrees(90.0) * Trafo3d.Scale(0.1) * Trafo3d.Translation(V3d(0.0, 0.0, -10.0)))
//        let rockScene = IO.Loader.Assimp.load "../../data/models/Rock1/Rock1_clean.obj"
//        let rockTrafo1 = Mod.constant (Trafo3d.Scale(2.0) * Trafo3d.Translation(-6.0, -3.0, 6.0))
//        let rockTrafo2 = Mod.constant (Trafo3d.Scale(3.0) * Trafo3d.Translation(9.0, -7.0, 7.5))
//        let rockTrafo3 = Mod.constant (Trafo3d.Scale(V3d(1.0, 0.5, 2.2)) * Trafo3d.RotationZInDegrees(33.0) * Trafo3d.Translation(10.0, 6.0, 5.5))
//        
//        [
//            (terrain, terrainTrafo)
//            (rockScene, rockTrafo1)
//            (rockScene, rockTrafo2)
//            (rockScene, rockTrafo3)
//        ]
//    
//    let testScene events =
//        let effects = [DefaultSurfaces.trafo |> toEffect; DefaultSurfaces.diffuseTexture |> toEffect; DefaultSurfaces.simpleLighting |> toEffect]
//        PickMesh.modelSg terrainScene effects
//        |> Sg.noEvents
//        |> Sg.withEvents events