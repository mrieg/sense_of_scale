namespace KnownObject

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.UI
open Aardvark.UI.Trafos

module Shader =
    
    open FShade
    
    type UniformScope with
        member x.Selected : bool = uniform?Selected
    
    let selectedShader (v : Effects.Vertex) =
        fragment {
            
            let c =
                if uniform.Selected
                then 0.6 * v.c + 0.4 * V4d.OIOI
                else v.c
            
            return {v with c = c}
        }

module Loader =
    
    let load (typ : KnownObjectType) =
        
        let modelsPath = "../../data/models/knownobjects/"
        let path =
            match typ with
            | KnownObjectType.Hammer      -> modelsPath + "hammer/Hammer.obj"
            | KnownObjectType.Person      -> modelsPath + "aardvark/aardvark.obj"
            | KnownObjectType.Chair       -> modelsPath + "chair/chair.dae"
            | KnownObjectType.Car         -> modelsPath + "aardvark/aardvark.obj"
            | KnownObjectType.Coin        -> modelsPath + "coinmodel/coin.obj"
            | KnownObjectType.SoccerField -> modelsPath + "soccerfield/soccerfield.obj"
            | _                           -> modelsPath + "aardvark/aardvark.obj"
        
        let scale =
            match typ with
            | KnownObjectType.Hammer -> 1.0 / 50.0
            | KnownObjectType.Coin   -> 1.0
            | KnownObjectType.Chair  -> 1.0 / 7.5
            | _                      -> 1.0
        
        let scaleTrafo = Trafo3d.Scale(scale) |> Mod.constant

        let rotation =
            match typ with
            | KnownObjectType.Hammer      -> Trafo3d.RotationX(Constant.Pi) * Trafo3d.RotationZ(Constant.Pi * 0.5) * Trafo3d.Scale(1.0,1.0,-1.0)
            | KnownObjectType.Chair       -> Trafo3d.RotationX(Constant.Pi * 0.5)
            | KnownObjectType.Coin        -> Trafo3d.Scale(-1.0)
            | KnownObjectType.SoccerField -> Trafo3d.Scale(0.5)
            | _                           -> Trafo3d.Scale(1.0,1.0,-1.0)

        let geom = IO.Loader.Assimp.load path
        let pickMeshes =
            [
                for mesh in geom.meshes do
                    yield
                        mesh.geometry.Sg
                        |> Sg.noEvents
            ]
            |> Sg.ofList
            |> Sg.trafo (geom.rootTrafo |> Mod.constant)
        
        let pickSg =
            pickMeshes
            |> Sg.requirePicking
            |> Sg.noEvents
            |> Sg.shader {
                do! DefaultSurfaces.trafo
                do! DefaultSurfaces.constantColor C4f.White
            }
            |> Sg.depthTest (Mod.constant DepthTestMode.Never)
        
        geom
        |> Sg.adapter
        |> Sg.effect [
            DefaultSurfaces.trafo          |> toEffect
            DefaultSurfaces.diffuseTexture |> toEffect
            DefaultSurfaces.simpleLighting |> toEffect
            Shader.selectedShader          |> toEffect
        ]
        |> Sg.andAlso pickSg
        |> Sg.trafo scaleTrafo
        |> Sg.trafo (rotation |> Mod.constant)
    
    let getOffset (typ : KnownObjectType) =
        match typ with
        | KnownObjectType.UnitCube    -> 0.5
        | KnownObjectType.UnitSphere  -> 1.0
        | KnownObjectType.Hammer      -> 0.16
        | KnownObjectType.Chair       -> 0.62
        | KnownObjectType.Coin        -> 0.025
        | KnownObjectType.SoccerField -> 0.0
        | _ -> 0.2
    
module Sg =
    
    let mkKnownObjectSg (m : MModel) (view : IMod<CameraView>) =
        m.typ
        |> Mod.map ( fun t ->
            match t with
            | KnownObjectType.UnitCube ->
                Sg.box' C4b.White (Box3d.FromCenterAndSize(V3d.OOO, V3d.III))
                |> Sg.requirePicking
                |> Sg.noEvents
                |> Sg.effect [
                    DefaultSurfaces.trafo          |> toEffect
                    DefaultSurfaces.vertexColor    |> toEffect
                    DefaultSurfaces.simpleLighting |> toEffect
                    Shader.selectedShader          |> toEffect
                ]
            | KnownObjectType.UnitSphere ->
                Sg.sphere' 7 C4b.White 1.0
                |> Sg.requirePicking
                |> Sg.noEvents
                |> Sg.effect [
                    DefaultSurfaces.trafo          |> toEffect
                    DefaultSurfaces.vertexColor    |> toEffect
                    DefaultSurfaces.simpleLighting |> toEffect
                    Shader.selectedShader          |> toEffect
                ]
            | _ ->
                Loader.load t
        )
        |> Sg.dynamic
        |> Sg.trafo m.trafo.previewTrafo
        |> Sg.uniform "Selected" m.selected