namespace SliderTest

open Aardvark.Base.Incremental
open Aardvark.UI

[<DomainType>]
type Model =
    {
        value : NumericInput
    }