namespace PRo3DModels

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI

[<DomainType>]
type FalseColorsModel = {
    useFalseColors  : bool
    lowerBound      : NumericInput
    upperBound      : NumericInput
    interval        : NumericInput
    invertMapping   : bool
    lowerColor      : ColorInput //C4b
    upperColor      : ColorInput //C4b
}