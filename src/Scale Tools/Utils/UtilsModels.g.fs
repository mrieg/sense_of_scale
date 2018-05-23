namespace Utils

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open Utils

[<AutoOpen>]
module Mutable =

    
    
    type MPickPointModel(__initial : Utils.PickPointModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Utils.PickPointModel> = Aardvark.Base.Incremental.EqModRef<Utils.PickPointModel>(__initial) :> Aardvark.Base.Incremental.IModRef<Utils.PickPointModel>
        let _pos = ResetMod.Create(__initial.pos)
        
        member x.pos = _pos :> IMod<_>
        member x.id = __current.Value.id
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Utils.PickPointModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_pos,v.pos)
                
        
        static member Create(__initial : Utils.PickPointModel) : MPickPointModel = MPickPointModel(__initial)
        static member Update(m : MPickPointModel, v : Utils.PickPointModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Utils.PickPointModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module PickPointModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let pos =
                { new Lens<Utils.PickPointModel, Aardvark.Base.V3d>() with
                    override x.Get(r) = r.pos
                    override x.Set(r,v) = { r with pos = v }
                    override x.Update(r,f) = { r with pos = f r.pos }
                }
            let id =
                { new Lens<Utils.PickPointModel, System.String>() with
                    override x.Get(r) = r.id
                    override x.Set(r,v) = { r with id = v }
                    override x.Update(r,f) = { r with id = f r.id }
                }
    
    
    type MPickPointsModel(__initial : Utils.PickPointsModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Utils.PickPointsModel> = Aardvark.Base.Incremental.EqModRef<Utils.PickPointsModel>(__initial) :> Aardvark.Base.Incremental.IModRef<Utils.PickPointsModel>
        let _points = MList.Create(__initial.points, (fun v -> MPickPointModel.Create(v)), (fun (m,v) -> MPickPointModel.Update(m, v)), (fun v -> v))
        
        member x.points = _points :> alist<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Utils.PickPointsModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MList.Update(_points, v.points)
                
        
        static member Create(__initial : Utils.PickPointsModel) : MPickPointsModel = MPickPointsModel(__initial)
        static member Update(m : MPickPointsModel, v : Utils.PickPointsModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Utils.PickPointsModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module PickPointsModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let points =
                { new Lens<Utils.PickPointsModel, Aardvark.Base.plist<Utils.PickPointModel>>() with
                    override x.Get(r) = r.points
                    override x.Set(r,v) = { r with points = v }
                    override x.Update(r,f) = { r with points = f r.points }
                }
