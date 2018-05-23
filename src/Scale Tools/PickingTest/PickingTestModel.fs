namespace PickingTest

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI.Primitives

[<DomainType>]
type Model =
    {
        pos         : V3d
        camera      : CameraControllerState
    }