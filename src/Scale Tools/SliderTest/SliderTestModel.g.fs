namespace SliderTest

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open SliderTest

[<AutoOpen>]
module Mutable =

    
    
    type MModel(__initial : SliderTest.Model) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<SliderTest.Model> = Aardvark.Base.Incremental.EqModRef<SliderTest.Model>(__initial) :> Aardvark.Base.Incremental.IModRef<SliderTest.Model>
        let _value = Aardvark.UI.Mutable.MNumericInput.Create(__initial.value)
        
        member x.value = _value
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : SliderTest.Model) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                Aardvark.UI.Mutable.MNumericInput.Update(_value, v.value)
                
        
        static member Create(__initial : SliderTest.Model) : MModel = MModel(__initial)
        static member Update(m : MModel, v : SliderTest.Model) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<SliderTest.Model> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Model =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let value =
                { new Lens<SliderTest.Model, Aardvark.UI.NumericInput>() with
                    override x.Get(r) = r.value
                    override x.Set(r,v) = { r with value = v }
                    override x.Update(r,f) = { r with value = f r.value }
                }
