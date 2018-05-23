namespace Boxes

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI
open Aardvark.UI.Primitives

module BoxValues =
    
    type Action =
        | UpdateCenter of Vector3d.Action
        | UpdateSize   of Vector3d.Action
        | SetName      of string

    let update (m : BoxValuesModel) (act : Action) =
        match act with
        | UpdateCenter msg -> {m with center = Vector3d.update m.center msg}
        | UpdateSize msg   -> {m with size = Vector3d.update m.size msg}
        | SetName name     -> {m with name = name}
    
    let view' (m : MBoxValuesModel) (liftMessage : Action -> 'msg) =
        div [clazz "ui"][
            Html.table [
                Html.row "Name : " [
                    Utils.Html.textInputColorable m.name C4b.Black SetName |> UI.map liftMessage
                ]
                Html.row "Size: " [
                    Vector3d.view m.size |> UI.map ( fun x -> liftMessage (UpdateSize x) )
                ]
            ]
        ]
    
    let initV3dInput (v : V3d) (min : float) (max : float) (step : float) =
        let init = Vector3d.initV3d v
        {
            init with
                x = {init.x with min = min; max = max; step = step}
                y = {init.y with min = min; max = max; step = step}
                z = {init.z with min = min; max = max; step = step}
        }

    let initial =
        let center = initV3dInput V3d.OOO -100.0 100.0 0.1
        let size = initV3dInput V3d.III 0.01 100.0 0.1
        {center = center; size = size; name = ""}
    
    let setup (box : BoxModel) =
        let center = initV3dInput box.center -100.0 100.0 0.1
        let size = initV3dInput box.size 0.01 100.0 0.1
        {center = center; size = size; name = box.name}
    
    let setup' (c : V3d) (s : V3d) (name : string) =
        let center = initV3dInput c -100.0 100.0 0.1
        let size = initV3dInput s 0.01 100.0 0.1
        {center = center; size = size; name = name}
    