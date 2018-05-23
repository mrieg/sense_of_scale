namespace GeometryShaderTest

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open GeometryShaderTest

[<AutoOpen>]
module Mutable =

    
    
    type MScene(__initial : GeometryShaderTest.Scene) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<GeometryShaderTest.Scene> = Aardvark.Base.Incremental.EqModRef<GeometryShaderTest.Scene>(__initial) :> Aardvark.Base.Incremental.IModRef<GeometryShaderTest.Scene>
        let _camera = Aardvark.UI.Primitives.Mutable.MCameraControllerState.Create(__initial.camera)
        
        member x.camera = _camera
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : GeometryShaderTest.Scene) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                Aardvark.UI.Primitives.Mutable.MCameraControllerState.Update(_camera, v.camera)
                
        
        static member Create(__initial : GeometryShaderTest.Scene) : MScene = MScene(__initial)
        static member Update(m : MScene, v : GeometryShaderTest.Scene) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<GeometryShaderTest.Scene> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Scene =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let camera =
                { new Lens<GeometryShaderTest.Scene, Aardvark.UI.Primitives.CameraControllerState>() with
                    override x.Get(r) = r.camera
                    override x.Set(r,v) = { r with camera = v }
                    override x.Update(r,f) = { r with camera = f r.camera }
                }
