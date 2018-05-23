namespace KnownObject

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open KnownObject

[<AutoOpen>]
module Mutable =

    
    
    type MModel(__initial : KnownObject.Model) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<KnownObject.Model> = Aardvark.Base.Incremental.EqModRef<KnownObject.Model>(__initial) :> Aardvark.Base.Incremental.IModRef<KnownObject.Model>
        let _trafo = Aardvark.UI.Trafos.Mutable.MTransformation.Create(__initial.trafo)
        let _trafoKind = ResetMod.Create(__initial.trafoKind)
        let _showTrafo = ResetMod.Create(__initial.showTrafo)
        let _typ = ResetMod.Create(__initial.typ)
        let _ctrOffset = ResetMod.Create(__initial.ctrOffset)
        let _selected = ResetMod.Create(__initial.selected)
        
        member x.trafo = _trafo
        member x.trafoKind = _trafoKind :> IMod<_>
        member x.showTrafo = _showTrafo :> IMod<_>
        member x.typ = _typ :> IMod<_>
        member x.ctrOffset = _ctrOffset :> IMod<_>
        member x.selected = _selected :> IMod<_>
        member x.id = __current.Value.id
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : KnownObject.Model) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                Aardvark.UI.Trafos.Mutable.MTransformation.Update(_trafo, v.trafo)
                ResetMod.Update(_trafoKind,v.trafoKind)
                ResetMod.Update(_showTrafo,v.showTrafo)
                ResetMod.Update(_typ,v.typ)
                ResetMod.Update(_ctrOffset,v.ctrOffset)
                ResetMod.Update(_selected,v.selected)
                
        
        static member Create(__initial : KnownObject.Model) : MModel = MModel(__initial)
        static member Update(m : MModel, v : KnownObject.Model) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<KnownObject.Model> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Model =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let trafo =
                { new Lens<KnownObject.Model, Aardvark.UI.Trafos.Transformation>() with
                    override x.Get(r) = r.trafo
                    override x.Set(r,v) = { r with trafo = v }
                    override x.Update(r,f) = { r with trafo = f r.trafo }
                }
            let trafoKind =
                { new Lens<KnownObject.Model, Aardvark.UI.Trafos.TrafoKind>() with
                    override x.Get(r) = r.trafoKind
                    override x.Set(r,v) = { r with trafoKind = v }
                    override x.Update(r,f) = { r with trafoKind = f r.trafoKind }
                }
            let showTrafo =
                { new Lens<KnownObject.Model, System.Boolean>() with
                    override x.Get(r) = r.showTrafo
                    override x.Set(r,v) = { r with showTrafo = v }
                    override x.Update(r,f) = { r with showTrafo = f r.showTrafo }
                }
            let typ =
                { new Lens<KnownObject.Model, KnownObject.KnownObjectType>() with
                    override x.Get(r) = r.typ
                    override x.Set(r,v) = { r with typ = v }
                    override x.Update(r,f) = { r with typ = f r.typ }
                }
            let ctrOffset =
                { new Lens<KnownObject.Model, System.Double>() with
                    override x.Get(r) = r.ctrOffset
                    override x.Set(r,v) = { r with ctrOffset = v }
                    override x.Update(r,f) = { r with ctrOffset = f r.ctrOffset }
                }
            let selected =
                { new Lens<KnownObject.Model, System.Boolean>() with
                    override x.Get(r) = r.selected
                    override x.Set(r,v) = { r with selected = v }
                    override x.Update(r,f) = { r with selected = f r.selected }
                }
            let id =
                { new Lens<KnownObject.Model, System.String>() with
                    override x.Get(r) = r.id
                    override x.Set(r,v) = { r with id = v }
                    override x.Update(r,f) = { r with id = f r.id }
                }
    
    
    type MMultiModel(__initial : KnownObject.MultiModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<KnownObject.MultiModel> = Aardvark.Base.Incremental.EqModRef<KnownObject.MultiModel>(__initial) :> Aardvark.Base.Incremental.IModRef<KnownObject.MultiModel>
        let _selected = MOption.Create(__initial.selected)
        let _models = MMap.Create(__initial.models, (fun v -> MModel.Create(v)), (fun (m,v) -> MModel.Update(m, v)), (fun v -> v))
        let _sky = ResetMod.Create(__initial.sky)
        
        member x.selected = _selected :> IMod<_>
        member x.models = _models :> amap<_,_>
        member x.sky = _sky :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : KnownObject.MultiModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MOption.Update(_selected, v.selected)
                MMap.Update(_models, v.models)
                ResetMod.Update(_sky,v.sky)
                
        
        static member Create(__initial : KnownObject.MultiModel) : MMultiModel = MMultiModel(__initial)
        static member Update(m : MMultiModel, v : KnownObject.MultiModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<KnownObject.MultiModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module MultiModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let selected =
                { new Lens<KnownObject.MultiModel, Microsoft.FSharp.Core.Option<System.String>>() with
                    override x.Get(r) = r.selected
                    override x.Set(r,v) = { r with selected = v }
                    override x.Update(r,f) = { r with selected = f r.selected }
                }
            let models =
                { new Lens<KnownObject.MultiModel, Aardvark.Base.hmap<System.String,KnownObject.Model>>() with
                    override x.Get(r) = r.models
                    override x.Set(r,v) = { r with models = v }
                    override x.Update(r,f) = { r with models = f r.models }
                }
            let sky =
                { new Lens<KnownObject.MultiModel, Aardvark.Base.V3d>() with
                    override x.Get(r) = r.sky
                    override x.Set(r,v) = { r with sky = v }
                    override x.Update(r,f) = { r with sky = f r.sky }
                }
    
    
    type MAppModel(__initial : KnownObject.AppModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<KnownObject.AppModel> = Aardvark.Base.Incremental.EqModRef<KnownObject.AppModel>(__initial) :> Aardvark.Base.Incremental.IModRef<KnownObject.AppModel>
        let _model = MModel.Create(__initial.model)
        let _camera = Aardvark.UI.Primitives.Mutable.MCameraControllerState.Create(__initial.camera)
        
        member x.model = _model
        member x.camera = _camera
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : KnownObject.AppModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MModel.Update(_model, v.model)
                Aardvark.UI.Primitives.Mutable.MCameraControllerState.Update(_camera, v.camera)
                
        
        static member Create(__initial : KnownObject.AppModel) : MAppModel = MAppModel(__initial)
        static member Update(m : MAppModel, v : KnownObject.AppModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<KnownObject.AppModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module AppModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let model =
                { new Lens<KnownObject.AppModel, KnownObject.Model>() with
                    override x.Get(r) = r.model
                    override x.Set(r,v) = { r with model = v }
                    override x.Update(r,f) = { r with model = f r.model }
                }
            let camera =
                { new Lens<KnownObject.AppModel, Aardvark.UI.Primitives.CameraControllerState>() with
                    override x.Get(r) = r.camera
                    override x.Set(r,v) = { r with camera = v }
                    override x.Update(r,f) = { r with camera = f r.camera }
                }
