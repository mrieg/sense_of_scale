namespace ScaleTools

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI.Primitives

type AppType =
    | None           = 0
    | Boxes          = 1
    | ScaleBars      = 2
    | Shading        = 3
    | ContourLines   = 4
    | KnownObjects   = 5
    | VertExagg      = 6
    | PlaneExtrude   = 7
    | EllipseShading = 8

[<DomainType>]
type Model =
    {
        appType    : AppType
        boxesApp   : Boxes.MultiComposedAppModel
        shadingApp : DistanceShading.Model
        barsApp    : ScaleBar.MultiModel
        contourApp : ContourLines.Model
        objectsApp : KnownObject.MultiModel
        vertApp    : VerticalExaggeration.Model
        planeExApp : PlaneExtrude.Model
        ellipseApp : EllipseShading.Model
        camera     : CameraControllerState
        shading    : bool
        drawOrientationCube : bool
    }
