namespace PlaneExtrude

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.Rendering
open Aardvark.UI
open Aardvark.UI.Trafos
open Aardvark.UI.Primitives

module Plane =
    
    let mkSg (m : MPlaneModel) =
        adaptive {
            let! v0 = m.v0
            let! v1 = m.v1
            let! v2 = m.v2
            let! v3 = m.v3

            let tri0 = Triangle3d(v0, v1, v2)
            let tri1 = Triangle3d(v0, v2, v3)
            let triangles = [|tri0; tri1|] |> Mod.constant
            
            return
                Sg.triangles m.color triangles
                |> Sg.noEvents
        }
        |> Sg.dynamic
        |> Sg.shader {
            do! DefaultSurfaces.trafo
            do! DefaultSurfaces.vertexColor
        }
    
    let setup v0 v1 v2 v3 color =
        {
            v0    = v0
            v1    = v1
            v2    = v2
            v3    = v3
            color = color
            id    = System.Guid.NewGuid().ToString()
        }
    
    let fromLineAndNormal (line : Line3d) (normal : V3d) (color : C4b) =
        let offset = V3d.Cross(normal.Normalized, line.Direction.Normalized) * 0.5
        let v0     = line.P0
        let v1     = line.P1
        let v2     = v1 + offset
        let v3     = v0 + offset
        {
            v0    = v0
            v1    = v1
            v2    = v2
            v3    = v3
            color = color
            id    = System.Guid.NewGuid().ToString()
        }
    
module App =
    
    type Action =
        | PointsMsg of Utils.Picking.Action
        | ToggleAddMode
    
    let update (m : Model) (a : Action) =
        match a with
        | PointsMsg msg -> {m with pointsModel = Utils.Picking.update m.pointsModel msg}
        | ToggleAddMode -> {m with addMode = not m.addMode; pointsModel = Utils.Picking.update m.pointsModel Utils.Picking.Action.Reset}
    
    let viewScene' (m : MModel) (view : IMod<CameraView>) (pickSg) (liftMessage : Action -> 'msg) =
        let sg = Plane.mkSg m.planeModel
        
        let points = Utils.Picking.mkSg m.pointsModel view (fun x -> x |> PointsMsg |> liftMessage)
        
        let events = [Sg.onDoubleClick ( fun pos -> pos |> Utils.Picking.Action.AddPoint |> PointsMsg |> liftMessage )]
        let pickSg = pickSg events
        let pointsSg =
            m.addMode
            |> Mod.map ( fun am ->
                if am
                then pickSg |> Sg.andAlso points
                else Sg.empty
            )
            |> Sg.dynamic
        
        [sg; pointsSg]
        |> Sg.ofList
    
    let view' (m : MModel) =
        div [][
            Html.SemUi.stuffStack [
                Utils.Html.toggleButton m.addMode "Add" "Cancel" ToggleAddMode
            ]
        ]
    
    let initial =
        let planeModel =
            let line   = Line3d(V3d(0.0, 0.0, 0.0), V3d(0.0, 2.0, 0.0))
            let normal = V3d.IOO
            Plane.fromLineAndNormal line normal C4b.Red

        {
            addMode     = false
            pointsModel = Utils.Picking.initial
            planeModel  = planeModel
            id          = System.Guid.NewGuid().ToString()
        }