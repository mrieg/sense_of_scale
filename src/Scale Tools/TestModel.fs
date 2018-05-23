namespace TestApp

open Aardvark.Base.Incremental
open Aardvark.UI.Primitives

[<DomainType>]
type Model =
    {
        camera : CameraControllerState
    }