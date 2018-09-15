namespace KnownObject

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI.Trafos
open Aardvark.UI.Primitives

type KnownObjectType =
    | UnitCube    = 0
    | UnitSphere  = 1
    | Hammer      = 3
    | Person      = 5
    | Chair       = 6
    | Bus         = 7
    | Coin        = 8
    | SoccerField = 9

[<DomainType>]
type Model =
    {
        trafo     : Transformation
        trafoKind : TrafoKind
        showTrafo : bool
        typ       : KnownObjectType
        ctrOffset : float
        selected  : bool

        [<NonIncremental>]
        id : string
    }

[<DomainType>]
type MultiModel =
    {
        selected : Option<string>
        models   : hmap<string, Model>
        sky      : V3d
    }

[<DomainType>]
type AppModel =
    {
        model  : Model
        camera : CameraControllerState
    }
