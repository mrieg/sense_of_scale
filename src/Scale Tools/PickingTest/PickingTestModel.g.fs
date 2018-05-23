namespace PickingTest

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open PickingTest

[<AutoOpen>]
module Mutable =

    
    
    type MModel(__initial : PickingTest.Model) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<PickingTest.Model> = Aardvark.Base.Incremental.EqModRef<PickingTest.Model>(__initial) :> Aardvark.Base.Incremental.IModRef<PickingTest.Model>
        let _pos = ResetMod.Create(__initial.pos)
        let _camera = Aardvark.UI.Primitives.Mutable.MCameraControllerState.Create(__initial.camera)
        
        member x.pos = _pos :> IMod<_>
        member x.camera = _camera
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : PickingTest.Model) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_pos,v.pos)
                Aardvark.UI.Primitives.Mutable.MCameraControllerState.Update(_camera, v.camera)
                
        
        static member Create(__initial : PickingTest.Model) : MModel = MModel(__initial)
        static member Update(m : MModel, v : PickingTest.Model) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<PickingTest.Model> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Model =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let pos =
                { new Lens<PickingTest.Model, Aardvark.Base.V3d>() with
                    override x.Get(r) = r.pos
                    override x.Set(r,v) = { r with pos = v }
                    override x.Update(r,f) = { r with pos = f r.pos }
                }
            let camera =
                { new Lens<PickingTest.Model, Aardvark.UI.Primitives.CameraControllerState>() with
                    override x.Get(r) = r.camera
                    override x.Set(r,v) = { r with camera = v }
                    override x.Update(r,f) = { r with camera = f r.camera }
                }
