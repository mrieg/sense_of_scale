namespace PlaneExtrude

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI.Trafos

[<DomainType>]
type PlaneModel =
    {
        v0    : V3d
        v1    : V3d
        v2    : V3d
        v3    : V3d

        group : int
        order : int

        [<NonIncremental>]
        id : string
    }
    
[<DomainType>]
type Model =
    {
        addMode     : bool
        extrudeMode : bool
        pointsModel : Utils.PickPointsModel
        planeModels : plist<PlaneModel>
        selected    : option<string>
        trafo       : Transformation
        maxGroupId  : int

        [<NonIncremental>]
        id : string
    }