namespace OPCTest

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI
open Aardvark.UI.Primitives

[<DomainType>]
type Model =
    {
        camera : CameraControllerState
    }