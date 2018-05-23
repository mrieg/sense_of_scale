namespace DistanceShading

open Aardvark.Base
open Aardvark.UI

module Controls =
    
    type Action =
        | UpdateCenter of Vector3d.Action
        | UpdateRadius of Numeric.Action
        | UpdateAlpha  of Numeric.Action
        | UpdateLevels of Numeric.Action
        | ChangeDiscrete
        | ChangeLines
        | ChangeDrawBar

    let initial =
        {
            center   = Vector3d.initV3d V3d.OOO
            radius   = {Numeric.init with value = 6.0; min = 0.1; max = 100.0; step = 0.1}
            alpha    = {Numeric.init with value = 0.6; min = 0.0; max = 1.0; step = 0.1}
            levels   = {Numeric.init with value = 4.0; min = 1.0; max = 50.0; step = 1.0}
            discrete = false
            lines    = false
            drawBar  = false
        }
    
    let update (m : ControlsModel) (act : Action) =
        match act with
        | UpdateCenter msg -> {m with center = Vector3d.update m.center msg}
        | UpdateRadius msg -> {m with radius = Numeric.update m.radius msg}
        | UpdateAlpha  msg -> {m with alpha  = Numeric.update m.alpha msg}
        | UpdateLevels msg -> {m with levels = Numeric.update m.levels msg}
        | ChangeDiscrete   -> {m with discrete = not m.discrete}
        | ChangeLines      -> {m with lines = not m.lines}
        | ChangeDrawBar    -> {m with drawBar = not m.drawBar}