namespace DragNDrop

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open DragNDrop

[<AutoOpen>]
module Mutable =

    
    
    type MModel(__initial : DragNDrop.Model) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<DragNDrop.Model> = Aardvark.Base.Incremental.EqModRef<DragNDrop.Model>(__initial) :> Aardvark.Base.Incremental.IModRef<DragNDrop.Model>
        let _trafo = ResetMod.Create(__initial.trafo)
        let _dragging = MOption.Create(__initial.dragging)
        let _camera = ResetMod.Create(__initial.camera)
        
        member x.trafo = _trafo :> IMod<_>
        member x.dragging = _dragging :> IMod<_>
        member x.camera = _camera :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : DragNDrop.Model) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_trafo,v.trafo)
                MOption.Update(_dragging, v.dragging)
                ResetMod.Update(_camera,v.camera)
                
        
        static member Create(__initial : DragNDrop.Model) : MModel = MModel(__initial)
        static member Update(m : MModel, v : DragNDrop.Model) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<DragNDrop.Model> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Model =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let trafo =
                { new Lens<DragNDrop.Model, Aardvark.Base.Trafo3d>() with
                    override x.Get(r) = r.trafo
                    override x.Set(r,v) = { r with trafo = v }
                    override x.Update(r,f) = { r with trafo = f r.trafo }
                }
            let dragging =
                { new Lens<DragNDrop.Model, Microsoft.FSharp.Core.Option<DragNDrop.Drag>>() with
                    override x.Get(r) = r.dragging
                    override x.Set(r,v) = { r with dragging = v }
                    override x.Update(r,f) = { r with dragging = f r.dragging }
                }
            let camera =
                { new Lens<DragNDrop.Model, System.Object>() with
                    override x.Get(r) = r.camera
                    override x.Set(r,v) = { r with camera = v }
                    override x.Update(r,f) = { r with camera = f r.camera }
                }
    
    
    type MScene(__initial : DragNDrop.Scene) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<DragNDrop.Scene> = Aardvark.Base.Incremental.EqModRef<DragNDrop.Scene>(__initial) :> Aardvark.Base.Incremental.IModRef<DragNDrop.Scene>
        let _transformation = ResetMod.Create(__initial.transformation)
        let _camera = ResetMod.Create(__initial.camera)
        
        member x.transformation = _transformation :> IMod<_>
        member x.camera = _camera :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : DragNDrop.Scene) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_transformation,v.transformation)
                ResetMod.Update(_camera,v.camera)
                
        
        static member Create(__initial : DragNDrop.Scene) : MScene = MScene(__initial)
        static member Update(m : MScene, v : DragNDrop.Scene) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<DragNDrop.Scene> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Scene =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let transformation =
                { new Lens<DragNDrop.Scene, System.Object>() with
                    override x.Get(r) = r.transformation
                    override x.Set(r,v) = { r with transformation = v }
                    override x.Update(r,f) = { r with transformation = f r.transformation }
                }
            let camera =
                { new Lens<DragNDrop.Scene, System.Object>() with
                    override x.Get(r) = r.camera
                    override x.Set(r,v) = { r with camera = v }
                    override x.Update(r,f) = { r with camera = f r.camera }
                }
