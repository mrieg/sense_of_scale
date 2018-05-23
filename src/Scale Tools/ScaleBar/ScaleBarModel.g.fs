namespace ScaleBar

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open ScaleBar

[<AutoOpen>]
module Mutable =

    
    
    type MScaleBarModel(__initial : ScaleBar.ScaleBarModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<ScaleBar.ScaleBarModel> = Aardvark.Base.Incremental.EqModRef<ScaleBar.ScaleBarModel>(__initial) :> Aardvark.Base.Incremental.IModRef<ScaleBar.ScaleBarModel>
        let _pos = ResetMod.Create(__initial.pos)
        let _height = ResetMod.Create(__initial.height)
        let _color1 = ResetMod.Create(__initial.color1)
        let _color2 = ResetMod.Create(__initial.color2)
        
        member x.pos = _pos :> IMod<_>
        member x.height = _height :> IMod<_>
        member x.color1 = _color1 :> IMod<_>
        member x.color2 = _color2 :> IMod<_>
        member x.id = __current.Value.id
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : ScaleBar.ScaleBarModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                ResetMod.Update(_pos,v.pos)
                ResetMod.Update(_height,v.height)
                ResetMod.Update(_color1,v.color1)
                ResetMod.Update(_color2,v.color2)
                
        
        static member Create(__initial : ScaleBar.ScaleBarModel) : MScaleBarModel = MScaleBarModel(__initial)
        static member Update(m : MScaleBarModel, v : ScaleBar.ScaleBarModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<ScaleBar.ScaleBarModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module ScaleBarModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let pos =
                { new Lens<ScaleBar.ScaleBarModel, Aardvark.Base.V3d>() with
                    override x.Get(r) = r.pos
                    override x.Set(r,v) = { r with pos = v }
                    override x.Update(r,f) = { r with pos = f r.pos }
                }
            let height =
                { new Lens<ScaleBar.ScaleBarModel, System.Double>() with
                    override x.Get(r) = r.height
                    override x.Set(r,v) = { r with height = v }
                    override x.Update(r,f) = { r with height = f r.height }
                }
            let color1 =
                { new Lens<ScaleBar.ScaleBarModel, Aardvark.Base.V4d>() with
                    override x.Get(r) = r.color1
                    override x.Set(r,v) = { r with color1 = v }
                    override x.Update(r,f) = { r with color1 = f r.color1 }
                }
            let color2 =
                { new Lens<ScaleBar.ScaleBarModel, Aardvark.Base.V4d>() with
                    override x.Get(r) = r.color2
                    override x.Set(r,v) = { r with color2 = v }
                    override x.Update(r,f) = { r with color2 = f r.color2 }
                }
            let id =
                { new Lens<ScaleBar.ScaleBarModel, System.String>() with
                    override x.Get(r) = r.id
                    override x.Set(r,v) = { r with id = v }
                    override x.Update(r,f) = { r with id = f r.id }
                }
    
    
    type MModel(__initial : ScaleBar.Model) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<ScaleBar.Model> = Aardvark.Base.Incremental.EqModRef<ScaleBar.Model>(__initial) :> Aardvark.Base.Incremental.IModRef<ScaleBar.Model>
        let _scaleBar = MScaleBarModel.Create(__initial.scaleBar)
        let _height = Aardvark.UI.Mutable.MNumericInput.Create(__initial.height)
        let _horizontal = ResetMod.Create(__initial.horizontal)
        
        member x.scaleBar = _scaleBar
        member x.height = _height
        member x.horizontal = _horizontal :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : ScaleBar.Model) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MScaleBarModel.Update(_scaleBar, v.scaleBar)
                Aardvark.UI.Mutable.MNumericInput.Update(_height, v.height)
                ResetMod.Update(_horizontal,v.horizontal)
                
        
        static member Create(__initial : ScaleBar.Model) : MModel = MModel(__initial)
        static member Update(m : MModel, v : ScaleBar.Model) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<ScaleBar.Model> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Model =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let scaleBar =
                { new Lens<ScaleBar.Model, ScaleBar.ScaleBarModel>() with
                    override x.Get(r) = r.scaleBar
                    override x.Set(r,v) = { r with scaleBar = v }
                    override x.Update(r,f) = { r with scaleBar = f r.scaleBar }
                }
            let height =
                { new Lens<ScaleBar.Model, Aardvark.UI.NumericInput>() with
                    override x.Get(r) = r.height
                    override x.Set(r,v) = { r with height = v }
                    override x.Update(r,f) = { r with height = f r.height }
                }
            let horizontal =
                { new Lens<ScaleBar.Model, System.Boolean>() with
                    override x.Get(r) = r.horizontal
                    override x.Set(r,v) = { r with horizontal = v }
                    override x.Update(r,f) = { r with horizontal = f r.horizontal }
                }
    
    
    type MMultiModel(__initial : ScaleBar.MultiModel) =
        inherit obj()
        let mutable __current : Aardvark.Base.Incremental.IModRef<ScaleBar.MultiModel> = Aardvark.Base.Incremental.EqModRef<ScaleBar.MultiModel>(__initial) :> Aardvark.Base.Incremental.IModRef<ScaleBar.MultiModel>
        let _bars = MMap.Create(__initial.bars, (fun v -> MModel.Create(v)), (fun (m,v) -> MModel.Update(m, v)), (fun v -> v))
        let _selected = MOption.Create(__initial.selected)
        let _addMode = ResetMod.Create(__initial.addMode)
        
        member x.bars = _bars :> amap<_,_>
        member x.selected = _selected :> IMod<_>
        member x.addMode = _addMode :> IMod<_>
        
        member x.Current = __current :> IMod<_>
        member x.Update(v : ScaleBar.MultiModel) =
            if not (System.Object.ReferenceEquals(__current.Value, v)) then
                __current.Value <- v
                
                MMap.Update(_bars, v.bars)
                MOption.Update(_selected, v.selected)
                ResetMod.Update(_addMode,v.addMode)
                
        
        static member Create(__initial : ScaleBar.MultiModel) : MMultiModel = MMultiModel(__initial)
        static member Update(m : MMultiModel, v : ScaleBar.MultiModel) = m.Update(v)
        
        override x.ToString() = __current.Value.ToString()
        member x.AsString = sprintf "%A" __current.Value
        interface IUpdatable<ScaleBar.MultiModel> with
            member x.Update v = x.Update v
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module MultiModel =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let bars =
                { new Lens<ScaleBar.MultiModel, Aardvark.Base.hmap<System.String,ScaleBar.Model>>() with
                    override x.Get(r) = r.bars
                    override x.Set(r,v) = { r with bars = v }
                    override x.Update(r,f) = { r with bars = f r.bars }
                }
            let selected =
                { new Lens<ScaleBar.MultiModel, Microsoft.FSharp.Core.Option<System.String>>() with
                    override x.Get(r) = r.selected
                    override x.Set(r,v) = { r with selected = v }
                    override x.Update(r,f) = { r with selected = f r.selected }
                }
            let addMode =
                { new Lens<ScaleBar.MultiModel, System.Boolean>() with
                    override x.Get(r) = r.addMode
                    override x.Set(r,v) = { r with addMode = v }
                    override x.Update(r,f) = { r with addMode = f r.addMode }
                }
