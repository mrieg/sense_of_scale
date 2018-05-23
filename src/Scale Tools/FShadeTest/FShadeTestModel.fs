namespace FShapeTestModel

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.SceneGraph
open Aardvark.UI
open Aardvark.UI.Primitives

[<DomainType>]
type Scene =
    {
        camera : CameraControllerState
    }
