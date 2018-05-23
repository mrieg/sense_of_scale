namespace Boxes

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.Rendering
open Aardvark.UI
open Aardvark.UI.Trafos
open Aardvark.UI.Primitives

type BoxFillMode =
    | Solid = 0
    | Line  = 1

[<DomainType>]
type RenderOptionsModel =
    {
        cullMode : CullMode
        fillMode : BoxFillMode
        blending : bool
    }

[<DomainType>]
type BoxValuesModel =
    {
        center : V3dInput
        size   : V3dInput
        name   : string
    }

[<DomainType>]
type BoxModel =
    {
        geom      : Box3d
        center    : V3d
        size      : V3d
        color     : C4b
        trafo     : Trafo3d
        trafoMode : TrafoMode
        centering : bool
        options   : RenderOptionsModel
        values    : BoxValuesModel
        display   : bool
        
        [<TreatAsValue>]
        name : string

        [<NonIncremental;PrimaryKey>]
        id : string
    }

[<DomainType>]
type BoxSelectionState =
    {
        selected     : bool
        trafoBoxMode : bool
        editMode     : bool
    }

[<DomainType>]
type TransformBoxModel =
    {
        box       : BoxModel
        trafo     : Transformation
        trafoKind : TrafoKind
    }

type FaceType =
    | FaceFront
    | FaceBack
    | FaceTop
    | FaceBottom
    | FaceLeft
    | FaceRight

[<DomainType>]
type Face =
    {
        center      : V3d
        normal      : V3d
        faceType    : FaceType
    }

[<DomainType>]
type TransformFaceModel =
    {
        box             : BoxModel
        selectedFace    : option<FaceType>
        trafo           : Transformation
    }

[<DomainType>]
type LabelsModel =
    {
        box         : BoxModel
        view        : CameraView
        xEdge       : option<Line3d>
        yEdge       : option<Line3d>
        zEdge       : option<Line3d>
    }

[<DomainType>]
type AddBoxPointSphere =
    {
        pos : V3d

        [<NonIncremental>]
        id : string
    }

[<DomainType>]
type AddBoxFromPointsModel =
    {
        ptList : plist<AddBoxPointSphere>
        box    : BoxModel
    }

type AddBoxMode =
    | Off
    | AddWithMouse
    | AddFromPoints

[<DomainType>]
type MultiBoxAppModel =
    {
        boxes          : hmap<string, BoxModel>
        selected       : option<string>
        trafoBoxMode   : bool
        editMode       : bool
        addBoxMode     : AddBoxMode
        boxTrafo       : TransformBoxModel
        faceTrafo      : TransformFaceModel
        trafoMode      : TrafoMode
        trafoKind      : TrafoKind
        addBoxPoints   : AddBoxFromPointsModel
        labels         : LabelsModel
        moveBoxMode    : bool
        sceneName      : string
        hideTrafoCtrls : bool
    }

type NavMode =
    | FreeFly = 0
    | ArcBall = 1

[<DomainType>]
type MultiComposedAppModel =
    {
        app     : MultiBoxAppModel
        patchBB : BoxModel
        patchBBLabels : LabelsModel
        drawPatchBB : bool
        camera  : CameraControllerState
        navMode : NavMode
    }