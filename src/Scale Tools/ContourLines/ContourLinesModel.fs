namespace ContourLines

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.Rendering
open Aardvark.SceneGraph
open Aardvark.UI
open Aardvark.UI.Primitives

[<DomainType>]
type Model =
    {
        inc    : NumericInput
        offset : NumericInput
    }