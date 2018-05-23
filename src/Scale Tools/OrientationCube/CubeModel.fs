namespace OrientationCube

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI.Primitives

[<DomainType>]
type Model =
  {
    camera  : CameraControllerState
  }
