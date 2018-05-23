namespace TestApp

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open TestApp

[<AutoOpen>]
module Mutable =

    
    
    type MModel(__initial : TestApp.Model) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<TestApp.Model> = Aardvark.Base.Incremental.EqModRef<TestApp.Model>(__initial) :> Aardvark.Base.Incremental.IModRef<TestApp.Model>
        let _camera = Aardvark.UI.Primitives.Mutable.MCameraControllerState.Create(__initial.camera)
        
        member x.camera = _camera
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : TestApp.Model) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                Aardvark.UI.Primitives.Mutable.MCameraControllerState.Update(_camera, v.camera)
                
        
        static member Create(__initial : TestApp.Model) : MModel = MModel(__initial)
        static member Update(m : MModel, v : TestApp.Model) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<TestApp.Model> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Model =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let camera =
                { new Lens<TestApp.Model, Aardvark.UI.Primitives.CameraControllerState>() with
                    override x.Get(r) = r.camera
                    override x.Set(r,v) = { r with camera = v }
                    override x.Update(r,f) = { r with camera = f r.camera }
                }
