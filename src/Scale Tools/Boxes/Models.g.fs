namespace Boxes

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open Boxes

[<AutoOpen>]
module Mutable =

    
    
    type MRenderOptionsModel(__initial : Boxes.RenderOptionsModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Boxes.RenderOptionsModel> = Aardvark.Base.Incremental.EqModRef<Boxes.RenderOptionsModel>(__initial) :> Aardvark.Base.Incremental.IModRef<Boxes.RenderOptionsModel>
        let _cullMode = ResetMod.Create(__initial.cullMode)
        let _fillMode = ResetMod.Create(__initial.fillMode)
        let _blending = ResetMod.Create(__initial.blending)
        
        member x.cullMode = _cullMode :> IMod<_>
        member x.fillMode = _fillMode :> IMod<_>
        member x.blending = _blending :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Boxes.RenderOptionsModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_cullMode,v.cullMode)
                ResetMod.Update(_fillMode,v.fillMode)
                ResetMod.Update(_blending,v.blending)
                
        
        static member Create(__initial : Boxes.RenderOptionsModel) : MRenderOptionsModel = MRenderOptionsModel(__initial)
        static member Update(m : MRenderOptionsModel, v : Boxes.RenderOptionsModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Boxes.RenderOptionsModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module RenderOptionsModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let cullMode =
                { new Lens<Boxes.RenderOptionsModel, Aardvark.Base.Rendering.CullMode>() with
                    override x.Get(r) = r.cullMode
                    override x.Set(r,v) = { r with cullMode = v }
                    override x.Update(r,f) = { r with cullMode = f r.cullMode }
                }
            let fillMode =
                { new Lens<Boxes.RenderOptionsModel, Boxes.BoxFillMode>() with
                    override x.Get(r) = r.fillMode
                    override x.Set(r,v) = { r with fillMode = v }
                    override x.Update(r,f) = { r with fillMode = f r.fillMode }
                }
            let blending =
                { new Lens<Boxes.RenderOptionsModel, System.Boolean>() with
                    override x.Get(r) = r.blending
                    override x.Set(r,v) = { r with blending = v }
                    override x.Update(r,f) = { r with blending = f r.blending }
                }
    
    
    type MBoxValuesModel(__initial : Boxes.BoxValuesModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Boxes.BoxValuesModel> = Aardvark.Base.Incremental.EqModRef<Boxes.BoxValuesModel>(__initial) :> Aardvark.Base.Incremental.IModRef<Boxes.BoxValuesModel>
        let _center = Aardvark.UI.Mutable.MV3dInput.Create(__initial.center)
        let _size = Aardvark.UI.Mutable.MV3dInput.Create(__initial.size)
        let _name = ResetMod.Create(__initial.name)
        
        member x.center = _center
        member x.size = _size
        member x.name = _name :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Boxes.BoxValuesModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                Aardvark.UI.Mutable.MV3dInput.Update(_center, v.center)
                Aardvark.UI.Mutable.MV3dInput.Update(_size, v.size)
                ResetMod.Update(_name,v.name)
                
        
        static member Create(__initial : Boxes.BoxValuesModel) : MBoxValuesModel = MBoxValuesModel(__initial)
        static member Update(m : MBoxValuesModel, v : Boxes.BoxValuesModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Boxes.BoxValuesModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module BoxValuesModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let center =
                { new Lens<Boxes.BoxValuesModel, Aardvark.UI.V3dInput>() with
                    override x.Get(r) = r.center
                    override x.Set(r,v) = { r with center = v }
                    override x.Update(r,f) = { r with center = f r.center }
                }
            let size =
                { new Lens<Boxes.BoxValuesModel, Aardvark.UI.V3dInput>() with
                    override x.Get(r) = r.size
                    override x.Set(r,v) = { r with size = v }
                    override x.Update(r,f) = { r with size = f r.size }
                }
            let name =
                { new Lens<Boxes.BoxValuesModel, System.String>() with
                    override x.Get(r) = r.name
                    override x.Set(r,v) = { r with name = v }
                    override x.Update(r,f) = { r with name = f r.name }
                }
    
    
    type MBoxModel(__initial : Boxes.BoxModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Boxes.BoxModel> = Aardvark.Base.Incremental.EqModRef<Boxes.BoxModel>(__initial) :> Aardvark.Base.Incremental.IModRef<Boxes.BoxModel>
        let _geom = ResetMod.Create(__initial.geom)
        let _center = ResetMod.Create(__initial.center)
        let _size = ResetMod.Create(__initial.size)
        let _color = ResetMod.Create(__initial.color)
        let _trafo = ResetMod.Create(__initial.trafo)
        let _trafoMode = ResetMod.Create(__initial.trafoMode)
        let _centering = ResetMod.Create(__initial.centering)
        let _options = MRenderOptionsModel.Create(__initial.options)
        let _values = MBoxValuesModel.Create(__initial.values)
        let _display = ResetMod.Create(__initial.display)
        let _name = ResetMod.Create(__initial.name)
        
        member x.geom = _geom :> IMod<_>
        member x.center = _center :> IMod<_>
        member x.size = _size :> IMod<_>
        member x.color = _color :> IMod<_>
        member x.trafo = _trafo :> IMod<_>
        member x.trafoMode = _trafoMode :> IMod<_>
        member x.centering = _centering :> IMod<_>
        member x.options = _options
        member x.values = _values
        member x.display = _display :> IMod<_>
        member x.name = _name :> IMod<_>
        member x.id = __current.Value.id
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Boxes.BoxModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_geom,v.geom)
                ResetMod.Update(_center,v.center)
                ResetMod.Update(_size,v.size)
                ResetMod.Update(_color,v.color)
                ResetMod.Update(_trafo,v.trafo)
                ResetMod.Update(_trafoMode,v.trafoMode)
                ResetMod.Update(_centering,v.centering)
                MRenderOptionsModel.Update(_options, v.options)
                MBoxValuesModel.Update(_values, v.values)
                ResetMod.Update(_display,v.display)
                _name.Update(v.name)
                
        
        static member Create(__initial : Boxes.BoxModel) : MBoxModel = MBoxModel(__initial)
        static member Update(m : MBoxModel, v : Boxes.BoxModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Boxes.BoxModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module BoxModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let geom =
                { new Lens<Boxes.BoxModel, Aardvark.Base.Box3d>() with
                    override x.Get(r) = r.geom
                    override x.Set(r,v) = { r with geom = v }
                    override x.Update(r,f) = { r with geom = f r.geom }
                }
            let center =
                { new Lens<Boxes.BoxModel, Aardvark.Base.V3d>() with
                    override x.Get(r) = r.center
                    override x.Set(r,v) = { r with center = v }
                    override x.Update(r,f) = { r with center = f r.center }
                }
            let size =
                { new Lens<Boxes.BoxModel, Aardvark.Base.V3d>() with
                    override x.Get(r) = r.size
                    override x.Set(r,v) = { r with size = v }
                    override x.Update(r,f) = { r with size = f r.size }
                }
            let color =
                { new Lens<Boxes.BoxModel, Aardvark.Base.C4b>() with
                    override x.Get(r) = r.color
                    override x.Set(r,v) = { r with color = v }
                    override x.Update(r,f) = { r with color = f r.color }
                }
            let trafo =
                { new Lens<Boxes.BoxModel, Aardvark.Base.Trafo3d>() with
                    override x.Get(r) = r.trafo
                    override x.Set(r,v) = { r with trafo = v }
                    override x.Update(r,f) = { r with trafo = f r.trafo }
                }
            let trafoMode =
                { new Lens<Boxes.BoxModel, Aardvark.UI.Trafos.TrafoMode>() with
                    override x.Get(r) = r.trafoMode
                    override x.Set(r,v) = { r with trafoMode = v }
                    override x.Update(r,f) = { r with trafoMode = f r.trafoMode }
                }
            let centering =
                { new Lens<Boxes.BoxModel, System.Boolean>() with
                    override x.Get(r) = r.centering
                    override x.Set(r,v) = { r with centering = v }
                    override x.Update(r,f) = { r with centering = f r.centering }
                }
            let options =
                { new Lens<Boxes.BoxModel, Boxes.RenderOptionsModel>() with
                    override x.Get(r) = r.options
                    override x.Set(r,v) = { r with options = v }
                    override x.Update(r,f) = { r with options = f r.options }
                }
            let values =
                { new Lens<Boxes.BoxModel, Boxes.BoxValuesModel>() with
                    override x.Get(r) = r.values
                    override x.Set(r,v) = { r with values = v }
                    override x.Update(r,f) = { r with values = f r.values }
                }
            let display =
                { new Lens<Boxes.BoxModel, System.Boolean>() with
                    override x.Get(r) = r.display
                    override x.Set(r,v) = { r with display = v }
                    override x.Update(r,f) = { r with display = f r.display }
                }
            let name =
                { new Lens<Boxes.BoxModel, System.String>() with
                    override x.Get(r) = r.name
                    override x.Set(r,v) = { r with name = v }
                    override x.Update(r,f) = { r with name = f r.name }
                }
            let id =
                { new Lens<Boxes.BoxModel, System.String>() with
                    override x.Get(r) = r.id
                    override x.Set(r,v) = { r with id = v }
                    override x.Update(r,f) = { r with id = f r.id }
                }
    
    
    type MBoxSelectionState(__initial : Boxes.BoxSelectionState) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Boxes.BoxSelectionState> = Aardvark.Base.Incremental.EqModRef<Boxes.BoxSelectionState>(__initial) :> Aardvark.Base.Incremental.IModRef<Boxes.BoxSelectionState>
        let _selected = ResetMod.Create(__initial.selected)
        let _trafoBoxMode = ResetMod.Create(__initial.trafoBoxMode)
        let _editMode = ResetMod.Create(__initial.editMode)
        
        member x.selected = _selected :> IMod<_>
        member x.trafoBoxMode = _trafoBoxMode :> IMod<_>
        member x.editMode = _editMode :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Boxes.BoxSelectionState) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_selected,v.selected)
                ResetMod.Update(_trafoBoxMode,v.trafoBoxMode)
                ResetMod.Update(_editMode,v.editMode)
                
        
        static member Create(__initial : Boxes.BoxSelectionState) : MBoxSelectionState = MBoxSelectionState(__initial)
        static member Update(m : MBoxSelectionState, v : Boxes.BoxSelectionState) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Boxes.BoxSelectionState> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module BoxSelectionState =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let selected =
                { new Lens<Boxes.BoxSelectionState, System.Boolean>() with
                    override x.Get(r) = r.selected
                    override x.Set(r,v) = { r with selected = v }
                    override x.Update(r,f) = { r with selected = f r.selected }
                }
            let trafoBoxMode =
                { new Lens<Boxes.BoxSelectionState, System.Boolean>() with
                    override x.Get(r) = r.trafoBoxMode
                    override x.Set(r,v) = { r with trafoBoxMode = v }
                    override x.Update(r,f) = { r with trafoBoxMode = f r.trafoBoxMode }
                }
            let editMode =
                { new Lens<Boxes.BoxSelectionState, System.Boolean>() with
                    override x.Get(r) = r.editMode
                    override x.Set(r,v) = { r with editMode = v }
                    override x.Update(r,f) = { r with editMode = f r.editMode }
                }
    
    
    type MTransformBoxModel(__initial : Boxes.TransformBoxModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Boxes.TransformBoxModel> = Aardvark.Base.Incremental.EqModRef<Boxes.TransformBoxModel>(__initial) :> Aardvark.Base.Incremental.IModRef<Boxes.TransformBoxModel>
        let _box = MBoxModel.Create(__initial.box)
        let _trafo = Aardvark.UI.Trafos.Mutable.MTransformation.Create(__initial.trafo)
        let _trafoKind = ResetMod.Create(__initial.trafoKind)
        
        member x.box = _box
        member x.trafo = _trafo
        member x.trafoKind = _trafoKind :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Boxes.TransformBoxModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MBoxModel.Update(_box, v.box)
                Aardvark.UI.Trafos.Mutable.MTransformation.Update(_trafo, v.trafo)
                ResetMod.Update(_trafoKind,v.trafoKind)
                
        
        static member Create(__initial : Boxes.TransformBoxModel) : MTransformBoxModel = MTransformBoxModel(__initial)
        static member Update(m : MTransformBoxModel, v : Boxes.TransformBoxModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Boxes.TransformBoxModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module TransformBoxModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let box =
                { new Lens<Boxes.TransformBoxModel, Boxes.BoxModel>() with
                    override x.Get(r) = r.box
                    override x.Set(r,v) = { r with box = v }
                    override x.Update(r,f) = { r with box = f r.box }
                }
            let trafo =
                { new Lens<Boxes.TransformBoxModel, Aardvark.UI.Trafos.Transformation>() with
                    override x.Get(r) = r.trafo
                    override x.Set(r,v) = { r with trafo = v }
                    override x.Update(r,f) = { r with trafo = f r.trafo }
                }
            let trafoKind =
                { new Lens<Boxes.TransformBoxModel, Aardvark.UI.Trafos.TrafoKind>() with
                    override x.Get(r) = r.trafoKind
                    override x.Set(r,v) = { r with trafoKind = v }
                    override x.Update(r,f) = { r with trafoKind = f r.trafoKind }
                }
    
    
    type MFace(__initial : Boxes.Face) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Boxes.Face> = Aardvark.Base.Incremental.EqModRef<Boxes.Face>(__initial) :> Aardvark.Base.Incremental.IModRef<Boxes.Face>
        let _center = ResetMod.Create(__initial.center)
        let _normal = ResetMod.Create(__initial.normal)
        let _faceType = ResetMod.Create(__initial.faceType)
        
        member x.center = _center :> IMod<_>
        member x.normal = _normal :> IMod<_>
        member x.faceType = _faceType :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Boxes.Face) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_center,v.center)
                ResetMod.Update(_normal,v.normal)
                ResetMod.Update(_faceType,v.faceType)
                
        
        static member Create(__initial : Boxes.Face) : MFace = MFace(__initial)
        static member Update(m : MFace, v : Boxes.Face) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Boxes.Face> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Face =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let center =
                { new Lens<Boxes.Face, Aardvark.Base.V3d>() with
                    override x.Get(r) = r.center
                    override x.Set(r,v) = { r with center = v }
                    override x.Update(r,f) = { r with center = f r.center }
                }
            let normal =
                { new Lens<Boxes.Face, Aardvark.Base.V3d>() with
                    override x.Get(r) = r.normal
                    override x.Set(r,v) = { r with normal = v }
                    override x.Update(r,f) = { r with normal = f r.normal }
                }
            let faceType =
                { new Lens<Boxes.Face, Boxes.FaceType>() with
                    override x.Get(r) = r.faceType
                    override x.Set(r,v) = { r with faceType = v }
                    override x.Update(r,f) = { r with faceType = f r.faceType }
                }
    
    
    type MTransformFaceModel(__initial : Boxes.TransformFaceModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Boxes.TransformFaceModel> = Aardvark.Base.Incremental.EqModRef<Boxes.TransformFaceModel>(__initial) :> Aardvark.Base.Incremental.IModRef<Boxes.TransformFaceModel>
        let _box = MBoxModel.Create(__initial.box)
        let _selectedFace = MOption.Create(__initial.selectedFace)
        let _trafo = Aardvark.UI.Trafos.Mutable.MTransformation.Create(__initial.trafo)
        
        member x.box = _box
        member x.selectedFace = _selectedFace :> IMod<_>
        member x.trafo = _trafo
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Boxes.TransformFaceModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MBoxModel.Update(_box, v.box)
                MOption.Update(_selectedFace, v.selectedFace)
                Aardvark.UI.Trafos.Mutable.MTransformation.Update(_trafo, v.trafo)
                
        
        static member Create(__initial : Boxes.TransformFaceModel) : MTransformFaceModel = MTransformFaceModel(__initial)
        static member Update(m : MTransformFaceModel, v : Boxes.TransformFaceModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Boxes.TransformFaceModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module TransformFaceModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let box =
                { new Lens<Boxes.TransformFaceModel, Boxes.BoxModel>() with
                    override x.Get(r) = r.box
                    override x.Set(r,v) = { r with box = v }
                    override x.Update(r,f) = { r with box = f r.box }
                }
            let selectedFace =
                { new Lens<Boxes.TransformFaceModel, Microsoft.FSharp.Core.Option<Boxes.FaceType>>() with
                    override x.Get(r) = r.selectedFace
                    override x.Set(r,v) = { r with selectedFace = v }
                    override x.Update(r,f) = { r with selectedFace = f r.selectedFace }
                }
            let trafo =
                { new Lens<Boxes.TransformFaceModel, Aardvark.UI.Trafos.Transformation>() with
                    override x.Get(r) = r.trafo
                    override x.Set(r,v) = { r with trafo = v }
                    override x.Update(r,f) = { r with trafo = f r.trafo }
                }
    
    
    type MLabelsModel(__initial : Boxes.LabelsModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Boxes.LabelsModel> = Aardvark.Base.Incremental.EqModRef<Boxes.LabelsModel>(__initial) :> Aardvark.Base.Incremental.IModRef<Boxes.LabelsModel>
        let _box = MBoxModel.Create(__initial.box)
        let _view = ResetMod.Create(__initial.view)
        let _xEdge = MOption.Create(__initial.xEdge)
        let _yEdge = MOption.Create(__initial.yEdge)
        let _zEdge = MOption.Create(__initial.zEdge)
        
        member x.box = _box
        member x.view = _view :> IMod<_>
        member x.xEdge = _xEdge :> IMod<_>
        member x.yEdge = _yEdge :> IMod<_>
        member x.zEdge = _zEdge :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Boxes.LabelsModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MBoxModel.Update(_box, v.box)
                ResetMod.Update(_view,v.view)
                MOption.Update(_xEdge, v.xEdge)
                MOption.Update(_yEdge, v.yEdge)
                MOption.Update(_zEdge, v.zEdge)
                
        
        static member Create(__initial : Boxes.LabelsModel) : MLabelsModel = MLabelsModel(__initial)
        static member Update(m : MLabelsModel, v : Boxes.LabelsModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Boxes.LabelsModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module LabelsModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let box =
                { new Lens<Boxes.LabelsModel, Boxes.BoxModel>() with
                    override x.Get(r) = r.box
                    override x.Set(r,v) = { r with box = v }
                    override x.Update(r,f) = { r with box = f r.box }
                }
            let view =
                { new Lens<Boxes.LabelsModel, Aardvark.Base.CameraView>() with
                    override x.Get(r) = r.view
                    override x.Set(r,v) = { r with view = v }
                    override x.Update(r,f) = { r with view = f r.view }
                }
            let xEdge =
                { new Lens<Boxes.LabelsModel, Microsoft.FSharp.Core.Option<Aardvark.Base.Line3d>>() with
                    override x.Get(r) = r.xEdge
                    override x.Set(r,v) = { r with xEdge = v }
                    override x.Update(r,f) = { r with xEdge = f r.xEdge }
                }
            let yEdge =
                { new Lens<Boxes.LabelsModel, Microsoft.FSharp.Core.Option<Aardvark.Base.Line3d>>() with
                    override x.Get(r) = r.yEdge
                    override x.Set(r,v) = { r with yEdge = v }
                    override x.Update(r,f) = { r with yEdge = f r.yEdge }
                }
            let zEdge =
                { new Lens<Boxes.LabelsModel, Microsoft.FSharp.Core.Option<Aardvark.Base.Line3d>>() with
                    override x.Get(r) = r.zEdge
                    override x.Set(r,v) = { r with zEdge = v }
                    override x.Update(r,f) = { r with zEdge = f r.zEdge }
                }
    
    
    type MAddBoxPointSphere(__initial : Boxes.AddBoxPointSphere) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Boxes.AddBoxPointSphere> = Aardvark.Base.Incremental.EqModRef<Boxes.AddBoxPointSphere>(__initial) :> Aardvark.Base.Incremental.IModRef<Boxes.AddBoxPointSphere>
        let _pos = ResetMod.Create(__initial.pos)
        
        member x.pos = _pos :> IMod<_>
        member x.id = __current.Value.id
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Boxes.AddBoxPointSphere) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_pos,v.pos)
                
        
        static member Create(__initial : Boxes.AddBoxPointSphere) : MAddBoxPointSphere = MAddBoxPointSphere(__initial)
        static member Update(m : MAddBoxPointSphere, v : Boxes.AddBoxPointSphere) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Boxes.AddBoxPointSphere> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module AddBoxPointSphere =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let pos =
                { new Lens<Boxes.AddBoxPointSphere, Aardvark.Base.V3d>() with
                    override x.Get(r) = r.pos
                    override x.Set(r,v) = { r with pos = v }
                    override x.Update(r,f) = { r with pos = f r.pos }
                }
            let id =
                { new Lens<Boxes.AddBoxPointSphere, System.String>() with
                    override x.Get(r) = r.id
                    override x.Set(r,v) = { r with id = v }
                    override x.Update(r,f) = { r with id = f r.id }
                }
    
    
    type MAddBoxFromPointsModel(__initial : Boxes.AddBoxFromPointsModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Boxes.AddBoxFromPointsModel> = Aardvark.Base.Incremental.EqModRef<Boxes.AddBoxFromPointsModel>(__initial) :> Aardvark.Base.Incremental.IModRef<Boxes.AddBoxFromPointsModel>
        let _ptList = MList.Create(__initial.ptList, (fun v -> MAddBoxPointSphere.Create(v)), (fun (m,v) -> MAddBoxPointSphere.Update(m, v)), (fun v -> v))
        let _box = MBoxModel.Create(__initial.box)
        
        member x.ptList = _ptList :> alist<_>
        member x.box = _box
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Boxes.AddBoxFromPointsModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MList.Update(_ptList, v.ptList)
                MBoxModel.Update(_box, v.box)
                
        
        static member Create(__initial : Boxes.AddBoxFromPointsModel) : MAddBoxFromPointsModel = MAddBoxFromPointsModel(__initial)
        static member Update(m : MAddBoxFromPointsModel, v : Boxes.AddBoxFromPointsModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Boxes.AddBoxFromPointsModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module AddBoxFromPointsModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let ptList =
                { new Lens<Boxes.AddBoxFromPointsModel, Aardvark.Base.plist<Boxes.AddBoxPointSphere>>() with
                    override x.Get(r) = r.ptList
                    override x.Set(r,v) = { r with ptList = v }
                    override x.Update(r,f) = { r with ptList = f r.ptList }
                }
            let box =
                { new Lens<Boxes.AddBoxFromPointsModel, Boxes.BoxModel>() with
                    override x.Get(r) = r.box
                    override x.Set(r,v) = { r with box = v }
                    override x.Update(r,f) = { r with box = f r.box }
                }
    
    
    type MMultiBoxAppModel(__initial : Boxes.MultiBoxAppModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Boxes.MultiBoxAppModel> = Aardvark.Base.Incremental.EqModRef<Boxes.MultiBoxAppModel>(__initial) :> Aardvark.Base.Incremental.IModRef<Boxes.MultiBoxAppModel>
        let _boxes = MMap.Create(__initial.boxes, (fun v -> MBoxModel.Create(v)), (fun (m,v) -> MBoxModel.Update(m, v)), (fun v -> v))
        let _selected = MOption.Create(__initial.selected)
        let _trafoBoxMode = ResetMod.Create(__initial.trafoBoxMode)
        let _editMode = ResetMod.Create(__initial.editMode)
        let _addBoxMode = ResetMod.Create(__initial.addBoxMode)
        let _boxTrafo = MTransformBoxModel.Create(__initial.boxTrafo)
        let _faceTrafo = MTransformFaceModel.Create(__initial.faceTrafo)
        let _trafoMode = ResetMod.Create(__initial.trafoMode)
        let _trafoKind = ResetMod.Create(__initial.trafoKind)
        let _addBoxPoints = MAddBoxFromPointsModel.Create(__initial.addBoxPoints)
        let _labels = MLabelsModel.Create(__initial.labels)
        let _moveBoxMode = ResetMod.Create(__initial.moveBoxMode)
        let _sceneName = ResetMod.Create(__initial.sceneName)
        let _hideTrafoCtrls = ResetMod.Create(__initial.hideTrafoCtrls)
        
        member x.boxes = _boxes :> amap<_,_>
        member x.selected = _selected :> IMod<_>
        member x.trafoBoxMode = _trafoBoxMode :> IMod<_>
        member x.editMode = _editMode :> IMod<_>
        member x.addBoxMode = _addBoxMode :> IMod<_>
        member x.boxTrafo = _boxTrafo
        member x.faceTrafo = _faceTrafo
        member x.trafoMode = _trafoMode :> IMod<_>
        member x.trafoKind = _trafoKind :> IMod<_>
        member x.addBoxPoints = _addBoxPoints
        member x.labels = _labels
        member x.moveBoxMode = _moveBoxMode :> IMod<_>
        member x.sceneName = _sceneName :> IMod<_>
        member x.hideTrafoCtrls = _hideTrafoCtrls :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Boxes.MultiBoxAppModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MMap.Update(_boxes, v.boxes)
                MOption.Update(_selected, v.selected)
                ResetMod.Update(_trafoBoxMode,v.trafoBoxMode)
                ResetMod.Update(_editMode,v.editMode)
                ResetMod.Update(_addBoxMode,v.addBoxMode)
                MTransformBoxModel.Update(_boxTrafo, v.boxTrafo)
                MTransformFaceModel.Update(_faceTrafo, v.faceTrafo)
                ResetMod.Update(_trafoMode,v.trafoMode)
                ResetMod.Update(_trafoKind,v.trafoKind)
                MAddBoxFromPointsModel.Update(_addBoxPoints, v.addBoxPoints)
                MLabelsModel.Update(_labels, v.labels)
                ResetMod.Update(_moveBoxMode,v.moveBoxMode)
                ResetMod.Update(_sceneName,v.sceneName)
                ResetMod.Update(_hideTrafoCtrls,v.hideTrafoCtrls)
                
        
        static member Create(__initial : Boxes.MultiBoxAppModel) : MMultiBoxAppModel = MMultiBoxAppModel(__initial)
        static member Update(m : MMultiBoxAppModel, v : Boxes.MultiBoxAppModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Boxes.MultiBoxAppModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module MultiBoxAppModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let boxes =
                { new Lens<Boxes.MultiBoxAppModel, Aardvark.Base.hmap<System.String,Boxes.BoxModel>>() with
                    override x.Get(r) = r.boxes
                    override x.Set(r,v) = { r with boxes = v }
                    override x.Update(r,f) = { r with boxes = f r.boxes }
                }
            let selected =
                { new Lens<Boxes.MultiBoxAppModel, Microsoft.FSharp.Core.Option<System.String>>() with
                    override x.Get(r) = r.selected
                    override x.Set(r,v) = { r with selected = v }
                    override x.Update(r,f) = { r with selected = f r.selected }
                }
            let trafoBoxMode =
                { new Lens<Boxes.MultiBoxAppModel, System.Boolean>() with
                    override x.Get(r) = r.trafoBoxMode
                    override x.Set(r,v) = { r with trafoBoxMode = v }
                    override x.Update(r,f) = { r with trafoBoxMode = f r.trafoBoxMode }
                }
            let editMode =
                { new Lens<Boxes.MultiBoxAppModel, System.Boolean>() with
                    override x.Get(r) = r.editMode
                    override x.Set(r,v) = { r with editMode = v }
                    override x.Update(r,f) = { r with editMode = f r.editMode }
                }
            let addBoxMode =
                { new Lens<Boxes.MultiBoxAppModel, Boxes.AddBoxMode>() with
                    override x.Get(r) = r.addBoxMode
                    override x.Set(r,v) = { r with addBoxMode = v }
                    override x.Update(r,f) = { r with addBoxMode = f r.addBoxMode }
                }
            let boxTrafo =
                { new Lens<Boxes.MultiBoxAppModel, Boxes.TransformBoxModel>() with
                    override x.Get(r) = r.boxTrafo
                    override x.Set(r,v) = { r with boxTrafo = v }
                    override x.Update(r,f) = { r with boxTrafo = f r.boxTrafo }
                }
            let faceTrafo =
                { new Lens<Boxes.MultiBoxAppModel, Boxes.TransformFaceModel>() with
                    override x.Get(r) = r.faceTrafo
                    override x.Set(r,v) = { r with faceTrafo = v }
                    override x.Update(r,f) = { r with faceTrafo = f r.faceTrafo }
                }
            let trafoMode =
                { new Lens<Boxes.MultiBoxAppModel, Aardvark.UI.Trafos.TrafoMode>() with
                    override x.Get(r) = r.trafoMode
                    override x.Set(r,v) = { r with trafoMode = v }
                    override x.Update(r,f) = { r with trafoMode = f r.trafoMode }
                }
            let trafoKind =
                { new Lens<Boxes.MultiBoxAppModel, Aardvark.UI.Trafos.TrafoKind>() with
                    override x.Get(r) = r.trafoKind
                    override x.Set(r,v) = { r with trafoKind = v }
                    override x.Update(r,f) = { r with trafoKind = f r.trafoKind }
                }
            let addBoxPoints =
                { new Lens<Boxes.MultiBoxAppModel, Boxes.AddBoxFromPointsModel>() with
                    override x.Get(r) = r.addBoxPoints
                    override x.Set(r,v) = { r with addBoxPoints = v }
                    override x.Update(r,f) = { r with addBoxPoints = f r.addBoxPoints }
                }
            let labels =
                { new Lens<Boxes.MultiBoxAppModel, Boxes.LabelsModel>() with
                    override x.Get(r) = r.labels
                    override x.Set(r,v) = { r with labels = v }
                    override x.Update(r,f) = { r with labels = f r.labels }
                }
            let moveBoxMode =
                { new Lens<Boxes.MultiBoxAppModel, System.Boolean>() with
                    override x.Get(r) = r.moveBoxMode
                    override x.Set(r,v) = { r with moveBoxMode = v }
                    override x.Update(r,f) = { r with moveBoxMode = f r.moveBoxMode }
                }
            let sceneName =
                { new Lens<Boxes.MultiBoxAppModel, System.String>() with
                    override x.Get(r) = r.sceneName
                    override x.Set(r,v) = { r with sceneName = v }
                    override x.Update(r,f) = { r with sceneName = f r.sceneName }
                }
            let hideTrafoCtrls =
                { new Lens<Boxes.MultiBoxAppModel, System.Boolean>() with
                    override x.Get(r) = r.hideTrafoCtrls
                    override x.Set(r,v) = { r with hideTrafoCtrls = v }
                    override x.Update(r,f) = { r with hideTrafoCtrls = f r.hideTrafoCtrls }
                }
    
    
    type MMultiComposedAppModel(__initial : Boxes.MultiComposedAppModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<Boxes.MultiComposedAppModel> = Aardvark.Base.Incremental.EqModRef<Boxes.MultiComposedAppModel>(__initial) :> Aardvark.Base.Incremental.IModRef<Boxes.MultiComposedAppModel>
        let _app = MMultiBoxAppModel.Create(__initial.app)
        let _patchBB = MBoxModel.Create(__initial.patchBB)
        let _patchBBLabels = MLabelsModel.Create(__initial.patchBBLabels)
        let _drawPatchBB = ResetMod.Create(__initial.drawPatchBB)
        let _camera = Aardvark.UI.Primitives.Mutable.MCameraControllerState.Create(__initial.camera)
        let _navMode = ResetMod.Create(__initial.navMode)
        
        member x.app = _app
        member x.patchBB = _patchBB
        member x.patchBBLabels = _patchBBLabels
        member x.drawPatchBB = _drawPatchBB :> IMod<_>
        member x.camera = _camera
        member x.navMode = _navMode :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : Boxes.MultiComposedAppModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MMultiBoxAppModel.Update(_app, v.app)
                MBoxModel.Update(_patchBB, v.patchBB)
                MLabelsModel.Update(_patchBBLabels, v.patchBBLabels)
                ResetMod.Update(_drawPatchBB,v.drawPatchBB)
                Aardvark.UI.Primitives.Mutable.MCameraControllerState.Update(_camera, v.camera)
                ResetMod.Update(_navMode,v.navMode)
                
        
        static member Create(__initial : Boxes.MultiComposedAppModel) : MMultiComposedAppModel = MMultiComposedAppModel(__initial)
        static member Update(m : MMultiComposedAppModel, v : Boxes.MultiComposedAppModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<Boxes.MultiComposedAppModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module MultiComposedAppModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let app =
                { new Lens<Boxes.MultiComposedAppModel, Boxes.MultiBoxAppModel>() with
                    override x.Get(r) = r.app
                    override x.Set(r,v) = { r with app = v }
                    override x.Update(r,f) = { r with app = f r.app }
                }
            let patchBB =
                { new Lens<Boxes.MultiComposedAppModel, Boxes.BoxModel>() with
                    override x.Get(r) = r.patchBB
                    override x.Set(r,v) = { r with patchBB = v }
                    override x.Update(r,f) = { r with patchBB = f r.patchBB }
                }
            let patchBBLabels =
                { new Lens<Boxes.MultiComposedAppModel, Boxes.LabelsModel>() with
                    override x.Get(r) = r.patchBBLabels
                    override x.Set(r,v) = { r with patchBBLabels = v }
                    override x.Update(r,f) = { r with patchBBLabels = f r.patchBBLabels }
                }
            let drawPatchBB =
                { new Lens<Boxes.MultiComposedAppModel, System.Boolean>() with
                    override x.Get(r) = r.drawPatchBB
                    override x.Set(r,v) = { r with drawPatchBB = v }
                    override x.Update(r,f) = { r with drawPatchBB = f r.drawPatchBB }
                }
            let camera =
                { new Lens<Boxes.MultiComposedAppModel, Aardvark.UI.Primitives.CameraControllerState>() with
                    override x.Get(r) = r.camera
                    override x.Set(r,v) = { r with camera = v }
                    override x.Update(r,f) = { r with camera = f r.camera }
                }
            let navMode =
                { new Lens<Boxes.MultiComposedAppModel, Boxes.NavMode>() with
                    override x.Get(r) = r.navMode
                    override x.Set(r,v) = { r with navMode = v }
                    override x.Update(r,f) = { r with navMode = f r.navMode }
                }
