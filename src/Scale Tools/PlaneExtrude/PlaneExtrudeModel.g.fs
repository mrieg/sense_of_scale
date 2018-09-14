namespace PlaneExtrude

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open PlaneExtrude

[<AutoOpen>]
module Mutable =

    
    
    type MPlaneModel(__initial : PlaneExtrude.PlaneModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<PlaneExtrude.PlaneModel> = Aardvark.Base.Incremental.EqModRef<PlaneExtrude.PlaneModel>(__initial) :> Aardvark.Base.Incremental.IModRef<PlaneExtrude.PlaneModel>
        let _v0 = ResetMod.Create(__initial.v0)
        let _v1 = ResetMod.Create(__initial.v1)
        let _v2 = ResetMod.Create(__initial.v2)
        let _v3 = ResetMod.Create(__initial.v3)
        let _group = ResetMod.Create(__initial.group)
        let _above = ResetMod.Create(__initial.above)
        let _below = ResetMod.Create(__initial.below)
        
        member x.v0 = _v0 :> IMod<_>
        member x.v1 = _v1 :> IMod<_>
        member x.v2 = _v2 :> IMod<_>
        member x.v3 = _v3 :> IMod<_>
        member x.group = _group :> IMod<_>
        member x.above = _above :> IMod<_>
        member x.below = _below :> IMod<_>
        member x.id = __current.Value.id
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : PlaneExtrude.PlaneModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_v0,v.v0)
                ResetMod.Update(_v1,v.v1)
                ResetMod.Update(_v2,v.v2)
                ResetMod.Update(_v3,v.v3)
                ResetMod.Update(_group,v.group)
                ResetMod.Update(_above,v.above)
                ResetMod.Update(_below,v.below)
                
        
        static member Create(__initial : PlaneExtrude.PlaneModel) : MPlaneModel = MPlaneModel(__initial)
        static member Update(m : MPlaneModel, v : PlaneExtrude.PlaneModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<PlaneExtrude.PlaneModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module PlaneModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let v0 =
                { new Lens<PlaneExtrude.PlaneModel, Aardvark.Base.V3d>() with
                    override x.Get(r) = r.v0
                    override x.Set(r,v) = { r with v0 = v }
                    override x.Update(r,f) = { r with v0 = f r.v0 }
                }
            let v1 =
                { new Lens<PlaneExtrude.PlaneModel, Aardvark.Base.V3d>() with
                    override x.Get(r) = r.v1
                    override x.Set(r,v) = { r with v1 = v }
                    override x.Update(r,f) = { r with v1 = f r.v1 }
                }
            let v2 =
                { new Lens<PlaneExtrude.PlaneModel, Aardvark.Base.V3d>() with
                    override x.Get(r) = r.v2
                    override x.Set(r,v) = { r with v2 = v }
                    override x.Update(r,f) = { r with v2 = f r.v2 }
                }
            let v3 =
                { new Lens<PlaneExtrude.PlaneModel, Aardvark.Base.V3d>() with
                    override x.Get(r) = r.v3
                    override x.Set(r,v) = { r with v3 = v }
                    override x.Update(r,f) = { r with v3 = f r.v3 }
                }
            let group =
                { new Lens<PlaneExtrude.PlaneModel, System.Int32>() with
                    override x.Get(r) = r.group
                    override x.Set(r,v) = { r with group = v }
                    override x.Update(r,f) = { r with group = f r.group }
                }
            let above =
                { new Lens<PlaneExtrude.PlaneModel, System.Int32>() with
                    override x.Get(r) = r.above
                    override x.Set(r,v) = { r with above = v }
                    override x.Update(r,f) = { r with above = f r.above }
                }
            let below =
                { new Lens<PlaneExtrude.PlaneModel, System.Int32>() with
                    override x.Get(r) = r.below
                    override x.Set(r,v) = { r with below = v }
                    override x.Update(r,f) = { r with below = f r.below }
                }
            let id =
                { new Lens<PlaneExtrude.PlaneModel, System.String>() with
                    override x.Get(r) = r.id
                    override x.Set(r,v) = { r with id = v }
                    override x.Update(r,f) = { r with id = f r.id }
                }
    
    
    type MLineModel(__initial : PlaneExtrude.LineModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<PlaneExtrude.LineModel> = Aardvark.Base.Incremental.EqModRef<PlaneExtrude.LineModel>(__initial) :> Aardvark.Base.Incremental.IModRef<PlaneExtrude.LineModel>
        let _startPlane = MPlaneModel.Create(__initial.startPlane)
        let _endPlane = MPlaneModel.Create(__initial.endPlane)
        let _group = ResetMod.Create(__initial.group)
        
        member x.startPlane = _startPlane
        member x.endPlane = _endPlane
        member x.group = _group :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : PlaneExtrude.LineModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MPlaneModel.Update(_startPlane, v.startPlane)
                MPlaneModel.Update(_endPlane, v.endPlane)
                ResetMod.Update(_group,v.group)
                
        
        static member Create(__initial : PlaneExtrude.LineModel) : MLineModel = MLineModel(__initial)
        static member Update(m : MLineModel, v : PlaneExtrude.LineModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<PlaneExtrude.LineModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module LineModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let startPlane =
                { new Lens<PlaneExtrude.LineModel, PlaneExtrude.PlaneModel>() with
                    override x.Get(r) = r.startPlane
                    override x.Set(r,v) = { r with startPlane = v }
                    override x.Update(r,f) = { r with startPlane = f r.startPlane }
                }
            let endPlane =
                { new Lens<PlaneExtrude.LineModel, PlaneExtrude.PlaneModel>() with
                    override x.Get(r) = r.endPlane
                    override x.Set(r,v) = { r with endPlane = v }
                    override x.Update(r,f) = { r with endPlane = f r.endPlane }
                }
            let group =
                { new Lens<PlaneExtrude.LineModel, System.Int32>() with
                    override x.Get(r) = r.group
                    override x.Set(r,v) = { r with group = v }
                    override x.Update(r,f) = { r with group = f r.group }
                }
    
    
    type MModel(__initial : PlaneExtrude.Model) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<PlaneExtrude.Model> = Aardvark.Base.Incremental.EqModRef<PlaneExtrude.Model>(__initial) :> Aardvark.Base.Incremental.IModRef<PlaneExtrude.Model>
        let _pointsModel = Utils.Mutable.MPickPointsModel.Create(__initial.pointsModel)
        let _planeModels = MList.Create(__initial.planeModels, (fun v -> MPlaneModel.Create(v)), (fun (m,v) -> MPlaneModel.Update(m, v)), (fun v -> v))
        let _lineModels = MList.Create(__initial.lineModels, (fun v -> MLineModel.Create(v)), (fun (m,v) -> MLineModel.Update(m, v)), (fun v -> v))
        let _addMode = ResetMod.Create(__initial.addMode)
        let _extrudeMode = ResetMod.Create(__initial.extrudeMode)
        let _selected = MOption.Create(__initial.selected)
        let _trafo = Aardvark.UI.Trafos.Mutable.MTransformation.Create(__initial.trafo)
        let _maxGroupId = ResetMod.Create(__initial.maxGroupId)
        
        member x.pointsModel = _pointsModel
        member x.planeModels = _planeModels :> alist<_>
        member x.lineModels = _lineModels :> alist<_>
        member x.addMode = _addMode :> IMod<_>
        member x.extrudeMode = _extrudeMode :> IMod<_>
        member x.selected = _selected :> IMod<_>
        member x.trafo = _trafo
        member x.maxGroupId = _maxGroupId :> IMod<_>
        member x.id = __current.Value.id
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : PlaneExtrude.Model) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                Utils.Mutable.MPickPointsModel.Update(_pointsModel, v.pointsModel)
                MList.Update(_planeModels, v.planeModels)
                MList.Update(_lineModels, v.lineModels)
                ResetMod.Update(_addMode,v.addMode)
                ResetMod.Update(_extrudeMode,v.extrudeMode)
                MOption.Update(_selected, v.selected)
                Aardvark.UI.Trafos.Mutable.MTransformation.Update(_trafo, v.trafo)
                ResetMod.Update(_maxGroupId,v.maxGroupId)
                
        
        static member Create(__initial : PlaneExtrude.Model) : MModel = MModel(__initial)
        static member Update(m : MModel, v : PlaneExtrude.Model) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<PlaneExtrude.Model> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Model =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let pointsModel =
                { new Lens<PlaneExtrude.Model, Utils.PickPointsModel>() with
                    override x.Get(r) = r.pointsModel
                    override x.Set(r,v) = { r with pointsModel = v }
                    override x.Update(r,f) = { r with pointsModel = f r.pointsModel }
                }
            let planeModels =
                { new Lens<PlaneExtrude.Model, Aardvark.Base.plist<PlaneExtrude.PlaneModel>>() with
                    override x.Get(r) = r.planeModels
                    override x.Set(r,v) = { r with planeModels = v }
                    override x.Update(r,f) = { r with planeModels = f r.planeModels }
                }
            let lineModels =
                { new Lens<PlaneExtrude.Model, Aardvark.Base.plist<PlaneExtrude.LineModel>>() with
                    override x.Get(r) = r.lineModels
                    override x.Set(r,v) = { r with lineModels = v }
                    override x.Update(r,f) = { r with lineModels = f r.lineModels }
                }
            let addMode =
                { new Lens<PlaneExtrude.Model, System.Boolean>() with
                    override x.Get(r) = r.addMode
                    override x.Set(r,v) = { r with addMode = v }
                    override x.Update(r,f) = { r with addMode = f r.addMode }
                }
            let extrudeMode =
                { new Lens<PlaneExtrude.Model, System.Boolean>() with
                    override x.Get(r) = r.extrudeMode
                    override x.Set(r,v) = { r with extrudeMode = v }
                    override x.Update(r,f) = { r with extrudeMode = f r.extrudeMode }
                }
            let selected =
                { new Lens<PlaneExtrude.Model, Microsoft.FSharp.Core.Option<System.String>>() with
                    override x.Get(r) = r.selected
                    override x.Set(r,v) = { r with selected = v }
                    override x.Update(r,f) = { r with selected = f r.selected }
                }
            let trafo =
                { new Lens<PlaneExtrude.Model, Aardvark.UI.Trafos.Transformation>() with
                    override x.Get(r) = r.trafo
                    override x.Set(r,v) = { r with trafo = v }
                    override x.Update(r,f) = { r with trafo = f r.trafo }
                }
            let maxGroupId =
                { new Lens<PlaneExtrude.Model, System.Int32>() with
                    override x.Get(r) = r.maxGroupId
                    override x.Set(r,v) = { r with maxGroupId = v }
                    override x.Update(r,f) = { r with maxGroupId = f r.maxGroupId }
                }
            let id =
                { new Lens<PlaneExtrude.Model, System.String>() with
                    override x.Get(r) = r.id
                    override x.Set(r,v) = { r with id = v }
                    override x.Update(r,f) = { r with id = f r.id }
                }
