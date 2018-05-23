namespace DistanceShading

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open DistanceShading

[<AutoOpen>]
module Mutable =

    
    
    type MControlsModel(__initial : DistanceShading.ControlsModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<DistanceShading.ControlsModel> = Aardvark.Base.Incremental.EqModRef<DistanceShading.ControlsModel>(__initial) :> Aardvark.Base.Incremental.IModRef<DistanceShading.ControlsModel>
        let _center = Aardvark.UI.Mutable.MV3dInput.Create(__initial.center)
        let _radius = Aardvark.UI.Mutable.MNumericInput.Create(__initial.radius)
        let _alpha = Aardvark.UI.Mutable.MNumericInput.Create(__initial.alpha)
        let _levels = Aardvark.UI.Mutable.MNumericInput.Create(__initial.levels)
        let _discrete = ResetMod.Create(__initial.discrete)
        let _lines = ResetMod.Create(__initial.lines)
        let _drawBar = ResetMod.Create(__initial.drawBar)
        
        member x.center = _center
        member x.radius = _radius
        member x.alpha = _alpha
        member x.levels = _levels
        member x.discrete = _discrete :> IMod<_>
        member x.lines = _lines :> IMod<_>
        member x.drawBar = _drawBar :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : DistanceShading.ControlsModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                Aardvark.UI.Mutable.MV3dInput.Update(_center, v.center)
                Aardvark.UI.Mutable.MNumericInput.Update(_radius, v.radius)
                Aardvark.UI.Mutable.MNumericInput.Update(_alpha, v.alpha)
                Aardvark.UI.Mutable.MNumericInput.Update(_levels, v.levels)
                ResetMod.Update(_discrete,v.discrete)
                ResetMod.Update(_lines,v.lines)
                ResetMod.Update(_drawBar,v.drawBar)
                
        
        static member Create(__initial : DistanceShading.ControlsModel) : MControlsModel = MControlsModel(__initial)
        static member Update(m : MControlsModel, v : DistanceShading.ControlsModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<DistanceShading.ControlsModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module ControlsModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let center =
                { new Lens<DistanceShading.ControlsModel, Aardvark.UI.V3dInput>() with
                    override x.Get(r) = r.center
                    override x.Set(r,v) = { r with center = v }
                    override x.Update(r,f) = { r with center = f r.center }
                }
            let radius =
                { new Lens<DistanceShading.ControlsModel, Aardvark.UI.NumericInput>() with
                    override x.Get(r) = r.radius
                    override x.Set(r,v) = { r with radius = v }
                    override x.Update(r,f) = { r with radius = f r.radius }
                }
            let alpha =
                { new Lens<DistanceShading.ControlsModel, Aardvark.UI.NumericInput>() with
                    override x.Get(r) = r.alpha
                    override x.Set(r,v) = { r with alpha = v }
                    override x.Update(r,f) = { r with alpha = f r.alpha }
                }
            let levels =
                { new Lens<DistanceShading.ControlsModel, Aardvark.UI.NumericInput>() with
                    override x.Get(r) = r.levels
                    override x.Set(r,v) = { r with levels = v }
                    override x.Update(r,f) = { r with levels = f r.levels }
                }
            let discrete =
                { new Lens<DistanceShading.ControlsModel, System.Boolean>() with
                    override x.Get(r) = r.discrete
                    override x.Set(r,v) = { r with discrete = v }
                    override x.Update(r,f) = { r with discrete = f r.discrete }
                }
            let lines =
                { new Lens<DistanceShading.ControlsModel, System.Boolean>() with
                    override x.Get(r) = r.lines
                    override x.Set(r,v) = { r with lines = v }
                    override x.Update(r,f) = { r with lines = f r.lines }
                }
            let drawBar =
                { new Lens<DistanceShading.ControlsModel, System.Boolean>() with
                    override x.Get(r) = r.drawBar
                    override x.Set(r,v) = { r with drawBar = v }
                    override x.Update(r,f) = { r with drawBar = f r.drawBar }
                }
    
    
    type MModel(__initial : DistanceShading.Model) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<DistanceShading.Model> = Aardvark.Base.Incremental.EqModRef<DistanceShading.Model>(__initial) :> Aardvark.Base.Incremental.IModRef<DistanceShading.Model>
        let _scaleBar = ScaleBar.Mutable.MModel.Create(__initial.scaleBar)
        let _controls = MControlsModel.Create(__initial.controls)
        let _camera = Aardvark.UI.Primitives.Mutable.MCameraControllerState.Create(__initial.camera)
        
        member x.scaleBar = _scaleBar
        member x.controls = _controls
        member x.camera = _camera
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : DistanceShading.Model) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ScaleBar.Mutable.MModel.Update(_scaleBar, v.scaleBar)
                MControlsModel.Update(_controls, v.controls)
                Aardvark.UI.Primitives.Mutable.MCameraControllerState.Update(_camera, v.camera)
                
        
        static member Create(__initial : DistanceShading.Model) : MModel = MModel(__initial)
        static member Update(m : MModel, v : DistanceShading.Model) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<DistanceShading.Model> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Model =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let scaleBar =
                { new Lens<DistanceShading.Model, ScaleBar.Model>() with
                    override x.Get(r) = r.scaleBar
                    override x.Set(r,v) = { r with scaleBar = v }
                    override x.Update(r,f) = { r with scaleBar = f r.scaleBar }
                }
            let controls =
                { new Lens<DistanceShading.Model, DistanceShading.ControlsModel>() with
                    override x.Get(r) = r.controls
                    override x.Set(r,v) = { r with controls = v }
                    override x.Update(r,f) = { r with controls = f r.controls }
                }
            let camera =
                { new Lens<DistanceShading.Model, Aardvark.UI.Primitives.CameraControllerState>() with
                    override x.Get(r) = r.camera
                    override x.Set(r,v) = { r with camera = v }
                    override x.Update(r,f) = { r with camera = f r.camera }
                }
