namespace Boxes

//open Aardvark.Base
//open Aardvark.Base.Incremental
//open Aardvark.Base.Rendering
//open Aardvark.SceneGraph
//open Aardvark.UI
//
//module PickMesh =
//    
//    let pickSg (scene : (Aardvark.SceneGraph.IO.Loader.Scene * IMod<Trafo3d>) list) events =
//        [
//            for (s,t) in scene do
//                for mesh in s.meshes do
//                    yield
//                        mesh.geometry.Sg
//                        |> Sg.noEvents
//                        |> Sg.trafo t
//        ]
//        |> Sg.ofList
//        |> Sg.requirePicking
//        |> Sg.noEvents
//        |> Sg.withEvents events
//        |> Sg.shader {
//            do! DefaultSurfaces.trafo
//            do! DefaultSurfaces.constantColor C4f.White
//        }
//        |> Sg.depthTest (Mod.constant DepthTestMode.Never)
//    
//    let modelSg (scene : (Aardvark.SceneGraph.IO.Loader.Scene * IMod<Trafo3d>) list) effects =
//        [
//            for (s,t) in scene do
//                yield
//                    s
//                    |> Sg.adapter
//                    |> Sg.trafo t
//        ]
//        |> Sg.ofList
//        |> Sg.noEvents
//        |> Sg.effect effects
//
//    let toSg scene events effects =
//        [pickSg scene events; modelSg scene effects]
//        |> Sg.ofList