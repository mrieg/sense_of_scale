namespace BoxManip

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open BoxManip

[<AutoOpen>]
module Mutable =

    
    
    type MCorner(__initial : BoxManip.Corner) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<BoxManip.Corner> = Aardvark.Base.Incremental.EqModRef<BoxManip.Corner>(__initial) :> Aardvark.Base.Incremental.IModRef<BoxManip.Corner>
        let _t = ResetMod.Create(__initial.t)
        let _trafo = Aardvark.UI.Trafos.Mutable.MTransformation.Create(__initial.trafo)
        
        member x.t = _t :> IMod<_>
        member x.trafo = _trafo
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : BoxManip.Corner) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_t,v.t)
                Aardvark.UI.Trafos.Mutable.MTransformation.Update(_trafo, v.trafo)
                
        
        static member Create(__initial : BoxManip.Corner) : MCorner = MCorner(__initial)
        static member Update(m : MCorner, v : BoxManip.Corner) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<BoxManip.Corner> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Corner =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let t =
                { new Lens<BoxManip.Corner, BoxManip.CornerType>() with
                    override x.Get(r) = r.t
                    override x.Set(r,v) = { r with t = v }
                    override x.Update(r,f) = { r with t = f r.t }
                }
            let trafo =
                { new Lens<BoxManip.Corner, Aardvark.UI.Trafos.Transformation>() with
                    override x.Get(r) = r.trafo
                    override x.Set(r,v) = { r with trafo = v }
                    override x.Update(r,f) = { r with trafo = f r.trafo }
                }
    
    
    type MEdge(__initial : BoxManip.Edge) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<BoxManip.Edge> = Aardvark.Base.Incremental.EqModRef<BoxManip.Edge>(__initial) :> Aardvark.Base.Incremental.IModRef<BoxManip.Edge>
        let _t = ResetMod.Create(__initial.t)
        let _pos = ResetMod.Create(__initial.pos)
        
        member x.t = _t :> IMod<_>
        member x.pos = _pos :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : BoxManip.Edge) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_t,v.t)
                ResetMod.Update(_pos,v.pos)
                
        
        static member Create(__initial : BoxManip.Edge) : MEdge = MEdge(__initial)
        static member Update(m : MEdge, v : BoxManip.Edge) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<BoxManip.Edge> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Edge =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let t =
                { new Lens<BoxManip.Edge, BoxManip.EdgeType>() with
                    override x.Get(r) = r.t
                    override x.Set(r,v) = { r with t = v }
                    override x.Update(r,f) = { r with t = f r.t }
                }
            let pos =
                { new Lens<BoxManip.Edge, BoxManip.EdgePos>() with
                    override x.Get(r) = r.pos
                    override x.Set(r,v) = { r with pos = v }
                    override x.Update(r,f) = { r with pos = f r.pos }
                }
    
    
    type MBoundingBox(__initial : BoxManip.BoundingBox) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<BoxManip.BoundingBox> = Aardvark.Base.Incremental.EqModRef<BoxManip.BoundingBox>(__initial) :> Aardvark.Base.Incremental.IModRef<BoxManip.BoundingBox>
        let _min = MCorner.Create(__initial.min)
        let _max = MCorner.Create(__initial.max)
        let _trafo = Aardvark.UI.Trafos.Mutable.MTransformation.Create(__initial.trafo)
        let _edges = MSet.Create(unbox, __initial.edges, (fun v -> MEdge.Create(v)), (fun (m,v) -> MEdge.Update(m, v)), (fun v -> v))
        let _id = ResetMod.Create(__initial.id)
        
        member x.min = _min
        member x.max = _max
        member x.trafo = _trafo
        member x.edges = _edges :> aset<_>
        member x.id = _id :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : BoxManip.BoundingBox) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MCorner.Update(_min, v.min)
                MCorner.Update(_max, v.max)
                Aardvark.UI.Trafos.Mutable.MTransformation.Update(_trafo, v.trafo)
                MSet.Update(_edges, v.edges)
                _id.Update(v.id)
                
        
        static member Create(__initial : BoxManip.BoundingBox) : MBoundingBox = MBoundingBox(__initial)
        static member Update(m : MBoundingBox, v : BoxManip.BoundingBox) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<BoxManip.BoundingBox> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module BoundingBox =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let min =
                { new Lens<BoxManip.BoundingBox, BoxManip.Corner>() with
                    override x.Get(r) = r.min
                    override x.Set(r,v) = { r with min = v }
                    override x.Update(r,f) = { r with min = f r.min }
                }
            let max =
                { new Lens<BoxManip.BoundingBox, BoxManip.Corner>() with
                    override x.Get(r) = r.max
                    override x.Set(r,v) = { r with max = v }
                    override x.Update(r,f) = { r with max = f r.max }
                }
            let trafo =
                { new Lens<BoxManip.BoundingBox, Aardvark.UI.Trafos.Transformation>() with
                    override x.Get(r) = r.trafo
                    override x.Set(r,v) = { r with trafo = v }
                    override x.Update(r,f) = { r with trafo = f r.trafo }
                }
            let edges =
                { new Lens<BoxManip.BoundingBox, Aardvark.Base.hset<BoxManip.Edge>>() with
                    override x.Get(r) = r.edges
                    override x.Set(r,v) = { r with edges = v }
                    override x.Update(r,f) = { r with edges = f r.edges }
                }
            let id =
                { new Lens<BoxManip.BoundingBox, System.String>() with
                    override x.Get(r) = r.id
                    override x.Set(r,v) = { r with id = v }
                    override x.Update(r,f) = { r with id = f r.id }
                }
    
    
    type MWorld(__initial : BoxManip.World) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<BoxManip.World> = Aardvark.Base.Incremental.EqModRef<BoxManip.World>(__initial) :> Aardvark.Base.Incremental.IModRef<BoxManip.World>
        let _boxes = MMap.Create(__initial.boxes, (fun v -> MBoundingBox.Create(v)), (fun (m,v) -> MBoundingBox.Update(m, v)), (fun v -> v))
        let _selectedBoxes = MSet.Create(__initial.selectedBoxes)
        let _selectedCorner = MOption.Create(__initial.selectedCorner)
        let _hoveredCorner = MOption.Create(__initial.hoveredCorner)
        let _hoveredEdge = MOption.Create(__initial.hoveredEdge)
        let _hoveredBox = MOption.Create(__initial.hoveredBox)
        let _editMode = ResetMod.Create(__initial.editMode)
        let _trafoKind = ResetMod.Create(__initial.trafoKind)
        let _cullMode = ResetMod.Create(__initial.cullMode)
        let _enableBlending = ResetMod.Create(__initial.enableBlending)
        let _camera = Aardvark.UI.Primitives.Mutable.MCameraControllerState.Create(__initial.camera)
        
        member x.boxes = _boxes :> amap<_,_>
        member x.selectedBoxes = _selectedBoxes :> aset<_>
        member x.selectedCorner = _selectedCorner :> IMod<_>
        member x.hoveredCorner = _hoveredCorner :> IMod<_>
        member x.hoveredEdge = _hoveredEdge :> IMod<_>
        member x.hoveredBox = _hoveredBox :> IMod<_>
        member x.editMode = _editMode :> IMod<_>
        member x.trafoKind = _trafoKind :> IMod<_>
        member x.cullMode = _cullMode :> IMod<_>
        member x.enableBlending = _enableBlending :> IMod<_>
        member x.camera = _camera
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : BoxManip.World) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MMap.Update(_boxes, v.boxes)
                MSet.Update(_selectedBoxes, v.selectedBoxes)
                MOption.Update(_selectedCorner, v.selectedCorner)
                MOption.Update(_hoveredCorner, v.hoveredCorner)
                MOption.Update(_hoveredEdge, v.hoveredEdge)
                MOption.Update(_hoveredBox, v.hoveredBox)
                ResetMod.Update(_editMode,v.editMode)
                ResetMod.Update(_trafoKind,v.trafoKind)
                ResetMod.Update(_cullMode,v.cullMode)
                ResetMod.Update(_enableBlending,v.enableBlending)
                Aardvark.UI.Primitives.Mutable.MCameraControllerState.Update(_camera, v.camera)
                
        
        static member Create(__initial : BoxManip.World) : MWorld = MWorld(__initial)
        static member Update(m : MWorld, v : BoxManip.World) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<BoxManip.World> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module World =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let boxes =
                { new Lens<BoxManip.World, Aardvark.Base.hmap<System.String,BoxManip.BoundingBox>>() with
                    override x.Get(r) = r.boxes
                    override x.Set(r,v) = { r with boxes = v }
                    override x.Update(r,f) = { r with boxes = f r.boxes }
                }
            let selectedBoxes =
                { new Lens<BoxManip.World, Aardvark.Base.hset<System.String>>() with
                    override x.Get(r) = r.selectedBoxes
                    override x.Set(r,v) = { r with selectedBoxes = v }
                    override x.Update(r,f) = { r with selectedBoxes = f r.selectedBoxes }
                }
            let selectedCorner =
                { new Lens<BoxManip.World, Microsoft.FSharp.Core.Option<BoxManip.CornerType>>() with
                    override x.Get(r) = r.selectedCorner
                    override x.Set(r,v) = { r with selectedCorner = v }
                    override x.Update(r,f) = { r with selectedCorner = f r.selectedCorner }
                }
            let hoveredCorner =
                { new Lens<BoxManip.World, Microsoft.FSharp.Core.Option<BoxManip.CornerType>>() with
                    override x.Get(r) = r.hoveredCorner
                    override x.Set(r,v) = { r with hoveredCorner = v }
                    override x.Update(r,f) = { r with hoveredCorner = f r.hoveredCorner }
                }
            let hoveredEdge =
                { new Lens<BoxManip.World, Microsoft.FSharp.Core.Option<BoxManip.EdgePos>>() with
                    override x.Get(r) = r.hoveredEdge
                    override x.Set(r,v) = { r with hoveredEdge = v }
                    override x.Update(r,f) = { r with hoveredEdge = f r.hoveredEdge }
                }
            let hoveredBox =
                { new Lens<BoxManip.World, Microsoft.FSharp.Core.Option<System.String>>() with
                    override x.Get(r) = r.hoveredBox
                    override x.Set(r,v) = { r with hoveredBox = v }
                    override x.Update(r,f) = { r with hoveredBox = f r.hoveredBox }
                }
            let editMode =
                { new Lens<BoxManip.World, System.Boolean>() with
                    override x.Get(r) = r.editMode
                    override x.Set(r,v) = { r with editMode = v }
                    override x.Update(r,f) = { r with editMode = f r.editMode }
                }
            let trafoKind =
                { new Lens<BoxManip.World, Aardvark.UI.Trafos.TrafoKind>() with
                    override x.Get(r) = r.trafoKind
                    override x.Set(r,v) = { r with trafoKind = v }
                    override x.Update(r,f) = { r with trafoKind = f r.trafoKind }
                }
            let cullMode =
                { new Lens<BoxManip.World, Aardvark.Base.Rendering.CullMode>() with
                    override x.Get(r) = r.cullMode
                    override x.Set(r,v) = { r with cullMode = v }
                    override x.Update(r,f) = { r with cullMode = f r.cullMode }
                }
            let enableBlending =
                { new Lens<BoxManip.World, System.Boolean>() with
                    override x.Get(r) = r.enableBlending
                    override x.Set(r,v) = { r with enableBlending = v }
                    override x.Update(r,f) = { r with enableBlending = f r.enableBlending }
                }
            let camera =
                { new Lens<BoxManip.World, Aardvark.UI.Primitives.CameraControllerState>() with
                    override x.Get(r) = r.camera
                    override x.Set(r,v) = { r with camera = v }
                    override x.Update(r,f) = { r with camera = f r.camera }
                }
