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
        let _color = ResetMod.Create(__initial.color)
        
        member x.v0 = _v0 :> IMod<_>
        member x.v1 = _v1 :> IMod<_>
        member x.v2 = _v2 :> IMod<_>
        member x.v3 = _v3 :> IMod<_>
        member x.color = _color :> IMod<_>
        member x.id = __current.Value.id
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : PlaneExtrude.PlaneModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_v0,v.v0)
                ResetMod.Update(_v1,v.v1)
                ResetMod.Update(_v2,v.v2)
                ResetMod.Update(_v3,v.v3)
                ResetMod.Update(_color,v.color)
                
        
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
            let color =
                { new Lens<PlaneExtrude.PlaneModel, Aardvark.Base.C4b>() with
                    override x.Get(r) = r.color
                    override x.Set(r,v) = { r with color = v }
                    override x.Update(r,f) = { r with color = f r.color }
                }
            let id =
                { new Lens<PlaneExtrude.PlaneModel, System.String>() with
                    override x.Get(r) = r.id
                    override x.Set(r,v) = { r with id = v }
                    override x.Update(r,f) = { r with id = f r.id }
                }
    
    
    type MModel(__initial : PlaneExtrude.Model) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<PlaneExtrude.Model> = Aardvark.Base.Incremental.EqModRef<PlaneExtrude.Model>(__initial) :> Aardvark.Base.Incremental.IModRef<PlaneExtrude.Model>
        let _addMode = ResetMod.Create(__initial.addMode)
        let _pointsModel = Utils.Mutable.MPickPointsModel.Create(__initial.pointsModel)
        let _planeModel = MPlaneModel.Create(__initial.planeModel)
        
        member x.addMode = _addMode :> IMod<_>
        member x.pointsModel = _pointsModel
        member x.planeModel = _planeModel
        member x.id = __current.Value.id
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : PlaneExtrude.Model) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_addMode,v.addMode)
                Utils.Mutable.MPickPointsModel.Update(_pointsModel, v.pointsModel)
                MPlaneModel.Update(_planeModel, v.planeModel)
                
        
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
            let addMode =
                { new Lens<PlaneExtrude.Model, System.Boolean>() with
                    override x.Get(r) = r.addMode
                    override x.Set(r,v) = { r with addMode = v }
                    override x.Update(r,f) = { r with addMode = f r.addMode }
                }
            let pointsModel =
                { new Lens<PlaneExtrude.Model, Utils.PickPointsModel>() with
                    override x.Get(r) = r.pointsModel
                    override x.Set(r,v) = { r with pointsModel = v }
                    override x.Update(r,f) = { r with pointsModel = f r.pointsModel }
                }
            let planeModel =
                { new Lens<PlaneExtrude.Model, PlaneExtrude.PlaneModel>() with
                    override x.Get(r) = r.planeModel
                    override x.Set(r,v) = { r with planeModel = v }
                    override x.Update(r,f) = { r with planeModel = f r.planeModel }
                }
            let id =
                { new Lens<PlaneExtrude.Model, System.String>() with
                    override x.Get(r) = r.id
                    override x.Set(r,v) = { r with id = v }
                    override x.Update(r,f) = { r with id = f r.id }
                }
