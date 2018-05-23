namespace GeometryShaderTest

open Aardvark.Base.Incremental
open Aardvark.UI.Primitives

[<DomainType>]
type Scene =
    {
        camera : CameraControllerState
    }