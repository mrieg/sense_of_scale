namespace DistanceShading

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI
open Aardvark.UI.Primitives

[<DomainType>]
type ControlsModel =
    {
        center   : V3dInput
        radius   : NumericInput
        alpha    : NumericInput
        levels   : NumericInput
        discrete : bool
        lines    : bool
        drawBar  : bool
    }

[<DomainType>]
type Model =
    {
        scaleBar   : ScaleBar.Model
        controls   : ControlsModel
        camera     : CameraControllerState
    }