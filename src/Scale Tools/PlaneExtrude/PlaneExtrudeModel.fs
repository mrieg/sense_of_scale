namespace PlaneExtrude

open Aardvark.Base
open Aardvark.Base.Incremental

[<DomainType>]
type PlaneModel =
    {
        v0    : V3d
        v1    : V3d
        v2    : V3d
        v3    : V3d
        color : C4b
        
        [<NonIncremental>]
        id : string
    }

[<DomainType>]
type Model =
    {
        addMode     : bool
        pointsModel : Utils.PickPointsModel
        planeModel  : PlaneModel
        
        [<NonIncremental>]
        id : string
    }
