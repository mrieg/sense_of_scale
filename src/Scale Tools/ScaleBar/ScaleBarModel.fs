namespace ScaleBar

open Aardvark.Base
open Aardvark.Base.Rendering
open Aardvark.Base.Incremental
open Aardvark.UI
open Aardvark.UI.Primitives

[<DomainType>]
type ScaleBarModel =
    {
        pos      : V3d
        height   : float
        color1   : V4d
        color2   : V4d

        [<NonIncremental>]
        id       : string
    }

[<DomainType>]
type Model =
    {
        scaleBar   : ScaleBarModel
        height     : NumericInput
        horizontal : bool
        stepped    : bool
    }

[<DomainType>]
type MultiModel =
    {
        bars     : hmap<string,Model>
        selected : option<string>
        addMode  : bool
    }