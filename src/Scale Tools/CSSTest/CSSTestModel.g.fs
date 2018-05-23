namespace CSSTest

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open CSSTest

[<AutoOpen>]
module Mutable =

    
    
    type MModel(__initial : CSSTest.Model) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<CSSTest.Model> = Aardvark.Base.Incremental.EqModRef<CSSTest.Model>(__initial) :> Aardvark.Base.Incremental.IModRef<CSSTest.Model>
        let _str = ResetMod.Create(__initial.str)
        
        member x.str = _str :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : CSSTest.Model) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_str,v.str)
                
        
        static member Create(__initial : CSSTest.Model) : MModel = MModel(__initial)
        static member Update(m : MModel, v : CSSTest.Model) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<CSSTest.Model> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Model =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let str =
                { new Lens<CSSTest.Model, System.String>() with
                    override x.Get(r) = r.str
                    override x.Set(r,v) = { r with str = v }
                    override x.Update(r,f) = { r with str = f r.str }
                }
