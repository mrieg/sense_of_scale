namespace ContourLines

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open ContourLines

[<AutoOpen>]
module Mutable =

    
    
    type MModel(__initial : ContourLines.Model) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<ContourLines.Model> = Aardvark.Base.Incremental.EqModRef<ContourLines.Model>(__initial) :> Aardvark.Base.Incremental.IModRef<ContourLines.Model>
        let _inc = Aardvark.UI.Mutable.MNumericInput.Create(__initial.inc)
        let _offset = Aardvark.UI.Mutable.MNumericInput.Create(__initial.offset)
        
        member x.inc = _inc
        member x.offset = _offset
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : ContourLines.Model) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                Aardvark.UI.Mutable.MNumericInput.Update(_inc, v.inc)
                Aardvark.UI.Mutable.MNumericInput.Update(_offset, v.offset)
                
        
        static member Create(__initial : ContourLines.Model) : MModel = MModel(__initial)
        static member Update(m : MModel, v : ContourLines.Model) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<ContourLines.Model> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Model =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let inc =
                { new Lens<ContourLines.Model, Aardvark.UI.NumericInput>() with
                    override x.Get(r) = r.inc
                    override x.Set(r,v) = { r with inc = v }
                    override x.Update(r,f) = { r with inc = f r.inc }
                }
            let offset =
                { new Lens<ContourLines.Model, Aardvark.UI.NumericInput>() with
                    override x.Get(r) = r.offset
                    override x.Set(r,v) = { r with offset = v }
                    override x.Update(r,f) = { r with offset = f r.offset }
                }
