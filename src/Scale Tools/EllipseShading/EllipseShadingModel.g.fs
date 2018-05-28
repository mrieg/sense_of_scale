namespace EllipseShading

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open EllipseShading

[<AutoOpen>]
module Mutable =

    
    
    type MEllipseModel(__initial : EllipseShading.EllipseModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<EllipseShading.EllipseModel> = Aardvark.Base.Incremental.EqModRef<EllipseShading.EllipseModel>(__initial) :> Aardvark.Base.Incremental.IModRef<EllipseShading.EllipseModel>
        let _a = ResetMod.Create(__initial.a)
        let _b = ResetMod.Create(__initial.b)
        let _c = ResetMod.Create(__initial.c)
        let _center = ResetMod.Create(__initial.center)
        let _rotation = ResetMod.Create(__initial.rotation)
        let _color = ResetMod.Create(__initial.color)
        
        member x.a = _a :> IMod<_>
        member x.b = _b :> IMod<_>
        member x.c = _c :> IMod<_>
        member x.center = _center :> IMod<_>
        member x.rotation = _rotation :> IMod<_>
        member x.color = _color :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : EllipseShading.EllipseModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_a,v.a)
                ResetMod.Update(_b,v.b)
                ResetMod.Update(_c,v.c)
                ResetMod.Update(_center,v.center)
                ResetMod.Update(_rotation,v.rotation)
                ResetMod.Update(_color,v.color)
                
        
        static member Create(__initial : EllipseShading.EllipseModel) : MEllipseModel = MEllipseModel(__initial)
        static member Update(m : MEllipseModel, v : EllipseShading.EllipseModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<EllipseShading.EllipseModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module EllipseModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let a =
                { new Lens<EllipseShading.EllipseModel, System.Double>() with
                    override x.Get(r) = r.a
                    override x.Set(r,v) = { r with a = v }
                    override x.Update(r,f) = { r with a = f r.a }
                }
            let b =
                { new Lens<EllipseShading.EllipseModel, System.Double>() with
                    override x.Get(r) = r.b
                    override x.Set(r,v) = { r with b = v }
                    override x.Update(r,f) = { r with b = f r.b }
                }
            let c =
                { new Lens<EllipseShading.EllipseModel, System.Double>() with
                    override x.Get(r) = r.c
                    override x.Set(r,v) = { r with c = v }
                    override x.Update(r,f) = { r with c = f r.c }
                }
            let center =
                { new Lens<EllipseShading.EllipseModel, Aardvark.Base.V3d>() with
                    override x.Get(r) = r.center
                    override x.Set(r,v) = { r with center = v }
                    override x.Update(r,f) = { r with center = f r.center }
                }
            let rotation =
                { new Lens<EllipseShading.EllipseModel, Aardvark.Base.Trafo3d>() with
                    override x.Get(r) = r.rotation
                    override x.Set(r,v) = { r with rotation = v }
                    override x.Update(r,f) = { r with rotation = f r.rotation }
                }
            let color =
                { new Lens<EllipseShading.EllipseModel, Aardvark.Base.V4d>() with
                    override x.Get(r) = r.color
                    override x.Set(r,v) = { r with color = v }
                    override x.Update(r,f) = { r with color = f r.color }
                }
    
    
    type MControlsModel(__initial : EllipseShading.ControlsModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<EllipseShading.ControlsModel> = Aardvark.Base.Incremental.EqModRef<EllipseShading.ControlsModel>(__initial) :> Aardvark.Base.Incremental.IModRef<EllipseShading.ControlsModel>
        let _values = Aardvark.UI.Mutable.MV3dInput.Create(__initial.values)
        let _center = Aardvark.UI.Mutable.MV3dInput.Create(__initial.center)
        let _trafo = Aardvark.UI.Trafos.Mutable.MTransformation.Create(__initial.trafo)
        let _kind = ResetMod.Create(__initial.kind)
        let _showTraf = ResetMod.Create(__initial.showTraf)
        let _showDebug = ResetMod.Create(__initial.showDebug)
        let _colPicker = Aardvark.UI.Mutable.MColorInput.Create(__initial.colPicker)
        
        member x.values = _values
        member x.center = _center
        member x.trafo = _trafo
        member x.kind = _kind :> IMod<_>
        member x.showTraf = _showTraf :> IMod<_>
        member x.showDebug = _showDebug :> IMod<_>
        member x.colPicker = _colPicker
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : EllipseShading.ControlsModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                Aardvark.UI.Mutable.MV3dInput.Update(_values, v.values)
                Aardvark.UI.Mutable.MV3dInput.Update(_center, v.center)
                Aardvark.UI.Trafos.Mutable.MTransformation.Update(_trafo, v.trafo)
                ResetMod.Update(_kind,v.kind)
                ResetMod.Update(_showTraf,v.showTraf)
                ResetMod.Update(_showDebug,v.showDebug)
                Aardvark.UI.Mutable.MColorInput.Update(_colPicker, v.colPicker)
                
        
        static member Create(__initial : EllipseShading.ControlsModel) : MControlsModel = MControlsModel(__initial)
        static member Update(m : MControlsModel, v : EllipseShading.ControlsModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<EllipseShading.ControlsModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module ControlsModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let values =
                { new Lens<EllipseShading.ControlsModel, Aardvark.UI.V3dInput>() with
                    override x.Get(r) = r.values
                    override x.Set(r,v) = { r with values = v }
                    override x.Update(r,f) = { r with values = f r.values }
                }
            let center =
                { new Lens<EllipseShading.ControlsModel, Aardvark.UI.V3dInput>() with
                    override x.Get(r) = r.center
                    override x.Set(r,v) = { r with center = v }
                    override x.Update(r,f) = { r with center = f r.center }
                }
            let trafo =
                { new Lens<EllipseShading.ControlsModel, Aardvark.UI.Trafos.Transformation>() with
                    override x.Get(r) = r.trafo
                    override x.Set(r,v) = { r with trafo = v }
                    override x.Update(r,f) = { r with trafo = f r.trafo }
                }
            let kind =
                { new Lens<EllipseShading.ControlsModel, Aardvark.UI.Trafos.TrafoKind>() with
                    override x.Get(r) = r.kind
                    override x.Set(r,v) = { r with kind = v }
                    override x.Update(r,f) = { r with kind = f r.kind }
                }
            let showTraf =
                { new Lens<EllipseShading.ControlsModel, System.Boolean>() with
                    override x.Get(r) = r.showTraf
                    override x.Set(r,v) = { r with showTraf = v }
                    override x.Update(r,f) = { r with showTraf = f r.showTraf }
                }
            let showDebug =
                { new Lens<EllipseShading.ControlsModel, System.Boolean>() with
                    override x.Get(r) = r.showDebug
                    override x.Set(r,v) = { r with showDebug = v }
                    override x.Update(r,f) = { r with showDebug = f r.showDebug }
                }
            let colPicker =
                { new Lens<EllipseShading.ControlsModel, Aardvark.UI.ColorInput>() with
                    override x.Get(r) = r.colPicker
                    override x.Set(r,v) = { r with colPicker = v }
                    override x.Update(r,f) = { r with colPicker = f r.colPicker }
                }
    
    
    type MModel(__initial : EllipseShading.Model) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<EllipseShading.Model> = Aardvark.Base.Incremental.EqModRef<EllipseShading.Model>(__initial) :> Aardvark.Base.Incremental.IModRef<EllipseShading.Model>
        let _ellipse = MEllipseModel.Create(__initial.ellipse)
        let _pickPoints = Utils.Mutable.MPickPointsModel.Create(__initial.pickPoints)
        let _controls = MControlsModel.Create(__initial.controls)
        let _addMode = ResetMod.Create(__initial.addMode)
        
        member x.ellipse = _ellipse
        member x.pickPoints = _pickPoints
        member x.controls = _controls
        member x.addMode = _addMode :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : EllipseShading.Model) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MEllipseModel.Update(_ellipse, v.ellipse)
                Utils.Mutable.MPickPointsModel.Update(_pickPoints, v.pickPoints)
                MControlsModel.Update(_controls, v.controls)
                ResetMod.Update(_addMode,v.addMode)
                
        
        static member Create(__initial : EllipseShading.Model) : MModel = MModel(__initial)
        static member Update(m : MModel, v : EllipseShading.Model) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<EllipseShading.Model> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Model =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let ellipse =
                { new Lens<EllipseShading.Model, EllipseShading.EllipseModel>() with
                    override x.Get(r) = r.ellipse
                    override x.Set(r,v) = { r with ellipse = v }
                    override x.Update(r,f) = { r with ellipse = f r.ellipse }
                }
            let pickPoints =
                { new Lens<EllipseShading.Model, Utils.PickPointsModel>() with
                    override x.Get(r) = r.pickPoints
                    override x.Set(r,v) = { r with pickPoints = v }
                    override x.Update(r,f) = { r with pickPoints = f r.pickPoints }
                }
            let controls =
                { new Lens<EllipseShading.Model, EllipseShading.ControlsModel>() with
                    override x.Get(r) = r.controls
                    override x.Set(r,v) = { r with controls = v }
                    override x.Update(r,f) = { r with controls = f r.controls }
                }
            let addMode =
                { new Lens<EllipseShading.Model, System.Boolean>() with
                    override x.Get(r) = r.addMode
                    override x.Set(r,v) = { r with addMode = v }
                    override x.Update(r,f) = { r with addMode = f r.addMode }
                }
