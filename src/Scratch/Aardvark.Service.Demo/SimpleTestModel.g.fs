namespace SimpleTest

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open SimpleTest

[<AutoOpen>]
module Mutable =

    
    
    type MModel(__initial : SimpleTest.Model) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<SimpleTest.Model> = Aardvark.Base.Incremental.EqModRef<SimpleTest.Model>(__initial) :> Aardvark.Base.Incremental.IModRef<SimpleTest.Model>
        let _sphereFirst = ResetMod.Create(__initial.sphereFirst)
        let _value = ResetMod.Create(__initial.value)
        let _cameraModel = ResetMod.Create(__initial.cameraModel)
        
        member x.sphereFirst = _sphereFirst :> IMod<_>
        member x.value = _value :> IMod<_>
        member x.cameraModel = _cameraModel :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : SimpleTest.Model) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_sphereFirst,v.sphereFirst)
                ResetMod.Update(_value,v.value)
                ResetMod.Update(_cameraModel,v.cameraModel)
                
        
        static member Create(__initial : SimpleTest.Model) : MModel = MModel(__initial)
        static member Update(m : MModel, v : SimpleTest.Model) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<SimpleTest.Model> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Model =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let sphereFirst =
                { new Lens<SimpleTest.Model, System.Boolean>() with
                    override x.Get(r) = r.sphereFirst
                    override x.Set(r,v) = { r with sphereFirst = v }
                    override x.Update(r,f) = { r with sphereFirst = f r.sphereFirst }
                }
            let value =
                { new Lens<SimpleTest.Model, System.Double>() with
                    override x.Get(r) = r.value
                    override x.Set(r,v) = { r with value = v }
                    override x.Update(r,f) = { r with value = f r.value }
                }
            let cameraModel =
                { new Lens<SimpleTest.Model, System.Object>() with
                    override x.Get(r) = r.cameraModel
                    override x.Set(r,v) = { r with cameraModel = v }
                    override x.Update(r,f) = { r with cameraModel = f r.cameraModel }
                }
