namespace BoxManip

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.UI
open Aardvark.UI.Primitives
open Aardvark.UI.Trafos

type CornerType =
    | Min
    | Max

[<DomainType>]
type Corner =
    {
        t       : CornerType
        trafo   : Transformation
    }

type EdgeType =
    | XEdge
    | YEdge
    | ZEdge

type EdgePos =
    | LFB_RFB
    | RFB_RBB
    | RBB_LBB
    | LBB_LFB
    | LFT_RFT
    | RFT_RBT
    | RBT_LBT
    | LBT_LFT
    | LFB_LFT
    | RFB_RFT
    | RBB_RBT
    | LBB_LBT

[<DomainType>]
type Edge =
    {
        t   : EdgeType
        pos : EdgePos
    }
    
[<DomainType>]
type BoundingBox =
    {
        min     : Corner
        max     : Corner
        trafo   : Transformation
        edges   : Edge hset

        [<TreatAsValue>]
        id      : string
    }

[<DomainType>]
type World =
    {
        boxes           : hmap<string, BoundingBox>
        selectedBoxes   : hset<string>
        selectedCorner  : CornerType option
        hoveredCorner   : CornerType option
        hoveredEdge     : EdgePos option
        hoveredBox      : string option
        editMode        : bool
        trafoKind       : TrafoKind
        cullMode        : CullMode
        enableBlending  : bool
        camera          : CameraControllerState
    }
