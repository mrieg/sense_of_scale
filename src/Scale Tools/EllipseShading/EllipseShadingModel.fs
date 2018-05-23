namespace EllipseShading

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI
open Aardvark.UI.Trafos

[<DomainType>]
type EllipseModel =
    {
        a        : float
        b        : float
        c        : float
        center   : V3d
        rotation : Trafo3d
    }

[<DomainType>]
type ControlsModel =
    {
        values    : V3dInput
        center    : V3dInput
        trafo     : Transformation
        kind      : TrafoKind
        showTraf  : bool
        showDebug : bool
    }

[<DomainType>]
type Model =
    {
        ellipse    : EllipseModel
        pickPoints : Utils.PickPointsModel
        controls   : ControlsModel
        addMode    : bool
    }