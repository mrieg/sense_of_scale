namespace Boxes

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.SceneGraph
open Aardvark.Base.Rendering
open Aardvark.Base.Geometry
open Aardvark.UI
open Aardvark.UI.Trafos
open Aardvark.UI.Trafos.TrafoController
open Aardvark.UI.Trafos.TranslateController

module TranslateFaceTrafoCtrl =
    let viewController (liftMessage : TrafoController.Action -> 'msg) (v : IMod<CameraView>) (m : MTransformation) (f : FaceType) =
        let arrow rot axis =
            let col =
                m.hovered |> Mod.map2 (colorMatch axis) (m.grabbed |> Mod.map (Option.map ( fun p -> p.axis )))

            Sg.cylinder tessellation col (Mod.constant cylinderRadius) (Mod.constant 1.0) 
            |> Sg.noEvents
            |> Sg.andAlso (                
                IndexedGeometryPrimitives.solidCone V3d.OOI V3d.OOI coneHeight coneRadius tessellation C4b.Red 
                 |> Sg.ofIndexedGeometry 
                 |> Sg.noEvents
                )
            |> Sg.pickable (Cylinder3d(V3d.OOO,V3d.OOI + V3d(0.0,0.0,0.1),cylinderRadius + 0.1) |> PickShape.Cylinder)
            |> Sg.transform rot       
            |> Sg.uniform "HoverColor" col           
            |> Sg.withEvents [ 
                    Sg.onEnter        (fun _ ->   Hover axis)
                    Sg.onMouseDownEvt (fun evt -> Grab (evt.localRay, axis))
                    Sg.onLeave        (fun _ ->   Unhover) 
               ]
               
        let scaleTrafo (pos : IMod<V3d>) =
            Sg.computeInvariantScale' v (Mod.constant 0.1) pos (Mod.constant 0.3) (Mod.constant 60.0) |> Mod.map Trafo3d.Scale

        let pickGraph =
            Sg.empty 
                |> Sg.Incremental.withGlobalEvents ( 
                        amap {
                            let! grabbed = m.grabbed
                            if grabbed.IsSome then
                                yield Global.onMouseMove (fun e -> MoveRay e.localRay)
                                yield Global.onMouseUp   (fun _ -> Release)
                        }
                    )
                //|> Sg.trafo (m.pose |> Pose.toTrafo' |> TrafoController.getTranslation |> scaleTrafo)
                |> Sg.trafo (TrafoController.pickingTrafo m)
                |> Sg.map liftMessage
        
        let arrowX = arrow (Trafo3d.RotationY Constant.PiHalf)  Axis.X
        let arrowY = arrow (Trafo3d.RotationX -Constant.PiHalf) Axis.Y
        let arrowZ = arrow (Trafo3d.RotationY 0.0)              Axis.Z

        let arrowX' = arrow (Trafo3d.RotationY -Constant.PiHalf) Axis.X
        let arrowY' = arrow (Trafo3d.RotationX Constant.PiHalf)  Axis.Y
        let arrowZ' = arrow (Trafo3d.Scale -1.0)                 Axis.Z
          
        let currentTrafo : IMod<Trafo3d> =
            adaptive {
                let! mode = m.mode
                match mode with
                    | TrafoMode.Local -> 
                        return! m.previewTrafo
                    | TrafoMode.Global -> 
                        let! a = m.previewTrafo
                        return Trafo3d.Translation(a.Forward.TransformPos(V3d.Zero))
                    | _ -> 
                        return failwith ""
            }
        
        let sgList =
            match f with
            | FaceType.FaceFront -> [arrowY]
            | FaceType.FaceBack -> [arrowY']
            | FaceType.FaceTop -> [arrowZ]
            | FaceType.FaceBottom -> [arrowZ']
            | FaceType.FaceLeft -> [arrowX]
            | FaceType.FaceRight -> [arrowX']

        let scene =
            Sg.ofList sgList
            |> Sg.effect [ Shader.stableTrafo |> toEffect; Shader.hoverColor |> toEffect]
            //|> Sg.trafo (currentTrafo |> TrafoController.getTranslation |> scaleTrafo)
            //|> Sg.trafo (m.pose |> Pose.toTrafo' |> TrafoController.getTranslation |> scaleTrafo)
            //|> Sg.trafo currentTrafo
            |> Sg.map liftMessage   
        
        Sg.ofList [pickGraph; scene]