namespace ScaleTools

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open ScaleTools

[<AutoOpen>]
module Mutable =

    
    
    type MModel(__initial : ScaleTools.Model) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<ScaleTools.Model> = Aardvark.Base.Incremental.EqModRef<ScaleTools.Model>(__initial) :> Aardvark.Base.Incremental.IModRef<ScaleTools.Model>
        let _appType = ResetMod.Create(__initial.appType)
        let _boxesApp = Boxes.Mutable.MMultiComposedAppModel.Create(__initial.boxesApp)
        let _shadingApp = DistanceShading.Mutable.MModel.Create(__initial.shadingApp)
        let _barsApp = ScaleBar.Mutable.MMultiModel.Create(__initial.barsApp)
        let _contourApp = ContourLines.Mutable.MModel.Create(__initial.contourApp)
        let _objectsApp = KnownObject.Mutable.MMultiModel.Create(__initial.objectsApp)
        let _vertApp = VerticalExaggeration.Mutable.MModel.Create(__initial.vertApp)
        let _planeExApp = PlaneExtrude.Mutable.MModel.Create(__initial.planeExApp)
        let _ellipseApp = EllipseShading.Mutable.MModel.Create(__initial.ellipseApp)
        let _camera = Aardvark.UI.Primitives.Mutable.MCameraControllerState.Create(__initial.camera)
        let _shading = ResetMod.Create(__initial.shading)
        let _drawOrientationCube = ResetMod.Create(__initial.drawOrientationCube)
        
        member x.appType = _appType :> IMod<_>
        member x.boxesApp = _boxesApp
        member x.shadingApp = _shadingApp
        member x.barsApp = _barsApp
        member x.contourApp = _contourApp
        member x.objectsApp = _objectsApp
        member x.vertApp = _vertApp
        member x.planeExApp = _planeExApp
        member x.ellipseApp = _ellipseApp
        member x.camera = _camera
        member x.shading = _shading :> IMod<_>
        member x.drawOrientationCube = _drawOrientationCube :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : ScaleTools.Model) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_appType,v.appType)
                Boxes.Mutable.MMultiComposedAppModel.Update(_boxesApp, v.boxesApp)
                DistanceShading.Mutable.MModel.Update(_shadingApp, v.shadingApp)
                ScaleBar.Mutable.MMultiModel.Update(_barsApp, v.barsApp)
                ContourLines.Mutable.MModel.Update(_contourApp, v.contourApp)
                KnownObject.Mutable.MMultiModel.Update(_objectsApp, v.objectsApp)
                VerticalExaggeration.Mutable.MModel.Update(_vertApp, v.vertApp)
                PlaneExtrude.Mutable.MModel.Update(_planeExApp, v.planeExApp)
                EllipseShading.Mutable.MModel.Update(_ellipseApp, v.ellipseApp)
                Aardvark.UI.Primitives.Mutable.MCameraControllerState.Update(_camera, v.camera)
                ResetMod.Update(_shading,v.shading)
                ResetMod.Update(_drawOrientationCube,v.drawOrientationCube)
                
        
        static member Create(__initial : ScaleTools.Model) : MModel = MModel(__initial)
        static member Update(m : MModel, v : ScaleTools.Model) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<ScaleTools.Model> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Model =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let appType =
                { new Lens<ScaleTools.Model, ScaleTools.AppType>() with
                    override x.Get(r) = r.appType
                    override x.Set(r,v) = { r with appType = v }
                    override x.Update(r,f) = { r with appType = f r.appType }
                }
            let boxesApp =
                { new Lens<ScaleTools.Model, Boxes.MultiComposedAppModel>() with
                    override x.Get(r) = r.boxesApp
                    override x.Set(r,v) = { r with boxesApp = v }
                    override x.Update(r,f) = { r with boxesApp = f r.boxesApp }
                }
            let shadingApp =
                { new Lens<ScaleTools.Model, DistanceShading.Model>() with
                    override x.Get(r) = r.shadingApp
                    override x.Set(r,v) = { r with shadingApp = v }
                    override x.Update(r,f) = { r with shadingApp = f r.shadingApp }
                }
            let barsApp =
                { new Lens<ScaleTools.Model, ScaleBar.MultiModel>() with
                    override x.Get(r) = r.barsApp
                    override x.Set(r,v) = { r with barsApp = v }
                    override x.Update(r,f) = { r with barsApp = f r.barsApp }
                }
            let contourApp =
                { new Lens<ScaleTools.Model, ContourLines.Model>() with
                    override x.Get(r) = r.contourApp
                    override x.Set(r,v) = { r with contourApp = v }
                    override x.Update(r,f) = { r with contourApp = f r.contourApp }
                }
            let objectsApp =
                { new Lens<ScaleTools.Model, KnownObject.MultiModel>() with
                    override x.Get(r) = r.objectsApp
                    override x.Set(r,v) = { r with objectsApp = v }
                    override x.Update(r,f) = { r with objectsApp = f r.objectsApp }
                }
            let vertApp =
                { new Lens<ScaleTools.Model, VerticalExaggeration.Model>() with
                    override x.Get(r) = r.vertApp
                    override x.Set(r,v) = { r with vertApp = v }
                    override x.Update(r,f) = { r with vertApp = f r.vertApp }
                }
            let planeExApp =
                { new Lens<ScaleTools.Model, PlaneExtrude.Model>() with
                    override x.Get(r) = r.planeExApp
                    override x.Set(r,v) = { r with planeExApp = v }
                    override x.Update(r,f) = { r with planeExApp = f r.planeExApp }
                }
            let ellipseApp =
                { new Lens<ScaleTools.Model, EllipseShading.Model>() with
                    override x.Get(r) = r.ellipseApp
                    override x.Set(r,v) = { r with ellipseApp = v }
                    override x.Update(r,f) = { r with ellipseApp = f r.ellipseApp }
                }
            let camera =
                { new Lens<ScaleTools.Model, Aardvark.UI.Primitives.CameraControllerState>() with
                    override x.Get(r) = r.camera
                    override x.Set(r,v) = { r with camera = v }
                    override x.Update(r,f) = { r with camera = f r.camera }
                }
            let shading =
                { new Lens<ScaleTools.Model, System.Boolean>() with
                    override x.Get(r) = r.shading
                    override x.Set(r,v) = { r with shading = v }
                    override x.Update(r,f) = { r with shading = f r.shading }
                }
            let drawOrientationCube =
                { new Lens<ScaleTools.Model, System.Boolean>() with
                    override x.Get(r) = r.drawOrientationCube
                    override x.Set(r,v) = { r with drawOrientationCube = v }
                    override x.Update(r,f) = { r with drawOrientationCube = f r.drawOrientationCube }
                }
