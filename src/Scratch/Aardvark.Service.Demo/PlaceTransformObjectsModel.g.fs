namespace PlaceTransformObjects

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open PlaceTransformObjects

[<AutoOpen>]
module Mutable =

    
    
    type MObject(__initial : PlaceTransformObjects.Object) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<PlaceTransformObjects.Object> = Aardvark.Base.Incremental.EqModRef<PlaceTransformObjects.Object>(__initial) :> Aardvark.Base.Incremental.IModRef<PlaceTransformObjects.Object>
        let _name = ResetMod.Create(__initial.name)
        let _objectType = ResetMod.Create(__initial.objectType)
        let _transformation = ResetMod.Create(__initial.transformation)
        
        member x.name = _name :> IMod<_>
        member x.objectType = _objectType :> IMod<_>
        member x.transformation = _transformation :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : PlaceTransformObjects.Object) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_name,v.name)
                ResetMod.Update(_objectType,v.objectType)
                ResetMod.Update(_transformation,v.transformation)
                
        
        static member Create(__initial : PlaceTransformObjects.Object) : MObject = MObject(__initial)
        static member Update(m : MObject, v : PlaceTransformObjects.Object) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<PlaceTransformObjects.Object> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Object =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let name =
                { new Lens<PlaceTransformObjects.Object, System.String>() with
                    override x.Get(r) = r.name
                    override x.Set(r,v) = { r with name = v }
                    override x.Update(r,f) = { r with name = f r.name }
                }
            let objectType =
                { new Lens<PlaceTransformObjects.Object, PlaceTransformObjects.ObjectType>() with
                    override x.Get(r) = r.objectType
                    override x.Set(r,v) = { r with objectType = v }
                    override x.Update(r,f) = { r with objectType = f r.objectType }
                }
            let transformation =
                { new Lens<PlaceTransformObjects.Object, System.Object>() with
                    override x.Get(r) = r.transformation
                    override x.Set(r,v) = { r with transformation = v }
                    override x.Update(r,f) = { r with transformation = f r.transformation }
                }
    
    
    type MWorld(__initial : PlaceTransformObjects.World) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<PlaceTransformObjects.World> = Aardvark.Base.Incremental.EqModRef<PlaceTransformObjects.World>(__initial) :> Aardvark.Base.Incremental.IModRef<PlaceTransformObjects.World>
        let _objects = MMap.Create(__initial.objects, (fun v -> MObject.Create(v)), (fun (m,v) -> MObject.Update(m, v)), (fun v -> v))
        let _selectedObjects = MSet.Create(__initial.selectedObjects)
        
        member x.objects = _objects :> amap<_,_>
        member x.selectedObjects = _selectedObjects :> aset<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : PlaceTransformObjects.World) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MMap.Update(_objects, v.objects)
                MSet.Update(_selectedObjects, v.selectedObjects)
                
        
        static member Create(__initial : PlaceTransformObjects.World) : MWorld = MWorld(__initial)
        static member Update(m : MWorld, v : PlaceTransformObjects.World) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<PlaceTransformObjects.World> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module World =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let objects =
                { new Lens<PlaceTransformObjects.World, Aardvark.Base.hmap<System.String,PlaceTransformObjects.Object>>() with
                    override x.Get(r) = r.objects
                    override x.Set(r,v) = { r with objects = v }
                    override x.Update(r,f) = { r with objects = f r.objects }
                }
            let selectedObjects =
                { new Lens<PlaceTransformObjects.World, Aardvark.Base.hset<System.String>>() with
                    override x.Get(r) = r.selectedObjects
                    override x.Set(r,v) = { r with selectedObjects = v }
                    override x.Update(r,f) = { r with selectedObjects = f r.selectedObjects }
                }
    
    
    type MScene(__initial : PlaceTransformObjects.Scene) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<PlaceTransformObjects.Scene> = Aardvark.Base.Incremental.EqModRef<PlaceTransformObjects.Scene>(__initial) :> Aardvark.Base.Incremental.IModRef<PlaceTransformObjects.Scene>
        let _world = MWorld.Create(__initial.world)
        let _kind = ResetMod.Create(__initial.kind)
        let _mode = ResetMod.Create(__initial.mode)
        let _camera = ResetMod.Create(__initial.camera)
        
        member x.world = _world
        member x.kind = _kind :> IMod<_>
        member x.mode = _mode :> IMod<_>
        member x.camera = _camera :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : PlaceTransformObjects.Scene) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MWorld.Update(_world, v.world)
                ResetMod.Update(_kind,v.kind)
                ResetMod.Update(_mode,v.mode)
                ResetMod.Update(_camera,v.camera)
                
        
        static member Create(__initial : PlaceTransformObjects.Scene) : MScene = MScene(__initial)
        static member Update(m : MScene, v : PlaceTransformObjects.Scene) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<PlaceTransformObjects.Scene> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Scene =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let world =
                { new Lens<PlaceTransformObjects.Scene, PlaceTransformObjects.World>() with
                    override x.Get(r) = r.world
                    override x.Set(r,v) = { r with world = v }
                    override x.Update(r,f) = { r with world = f r.world }
                }
            let kind =
                { new Lens<PlaceTransformObjects.Scene, System.Object>() with
                    override x.Get(r) = r.kind
                    override x.Set(r,v) = { r with kind = v }
                    override x.Update(r,f) = { r with kind = f r.kind }
                }
            let mode =
                { new Lens<PlaceTransformObjects.Scene, System.Object>() with
                    override x.Get(r) = r.mode
                    override x.Set(r,v) = { r with mode = v }
                    override x.Update(r,f) = { r with mode = f r.mode }
                }
            let camera =
                { new Lens<PlaceTransformObjects.Scene, System.Object>() with
                    override x.Get(r) = r.camera
                    override x.Set(r,v) = { r with camera = v }
                    override x.Update(r,f) = { r with camera = f r.camera }
                }
