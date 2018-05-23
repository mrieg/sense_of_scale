namespace Performance

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open Performance

[<AutoOpen>]
module Mutable =

    
    
    type MModel(__initial : Performance.Model) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Performance.Model> = Aardvark.Base.Incremental.EqModRef<Performance.Model>(__initial) :> Aardvark.Base.Incremental.IModRef<Performance.Model>
        let _visible = MList.Create(__initial.visible)
        let _objects = MList.Create(__initial.objects)
        let _cameraState = ResetMod.Create(__initial.cameraState)
        
        member x.visible = _visible :> alist<_>
        member x.objects = _objects :> alist<_>
        member x.cameraState = _cameraState :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Performance.Model) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MList.Update(_visible, v.visible)
                MList.Update(_objects, v.objects)
                ResetMod.Update(_cameraState,v.cameraState)
                
        
        static member Create(__initial : Performance.Model) : MModel = MModel(__initial)
        static member Update(m : MModel, v : Performance.Model) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Performance.Model> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Model =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let visible =
                { new Lens<Performance.Model, Aardvark.Base.plist<Aardvark.Base.Trafo3d>>() with
                    override x.Get(r) = r.visible
                    override x.Set(r,v) = { r with visible = v }
                    override x.Update(r,f) = { r with visible = f r.visible }
                }
            let objects =
                { new Lens<Performance.Model, Aardvark.Base.plist<Aardvark.Base.Trafo3d>>() with
                    override x.Get(r) = r.objects
                    override x.Set(r,v) = { r with objects = v }
                    override x.Update(r,f) = { r with objects = f r.objects }
                }
            let cameraState =
                { new Lens<Performance.Model, System.Object>() with
                    override x.Get(r) = r.cameraState
                    override x.Set(r,v) = { r with cameraState = v }
                    override x.Update(r,f) = { r with cameraState = f r.cameraState }
                }
