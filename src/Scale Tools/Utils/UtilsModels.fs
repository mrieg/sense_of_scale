namespace Utils

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI

[<DomainType>]
type PickPointModel =
    {
        pos : V3d

        [<NonIncremental>]
        id  : string
    }

[<DomainType>]
type PickPointsModel =
    {
        points : plist<PickPointModel>
    }