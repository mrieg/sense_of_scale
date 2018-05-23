namespace OPCTest

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.Rendering
open Aardvark.UI
open Aardvark.UI.Trafos
open Aardvark.UI.Primitives

open Aardvark.SceneGraph.Opc

module Mars =
    
    //scene descriptor
    let mars () =
        let patchHierarchies =
            System.IO.Directory.GetDirectories(@"..\..\data\mars")
            |> Seq.collect System.IO.Directory.GetDirectories

        { 
            useCompressedTextures = true
            preTransform     = Trafo3d.Identity
            patchHierarchies = patchHierarchies
            
            //bounding box fuer root-patch aus Patch.xml
            boundingBox      = Box3d.Parse("[[3376372.058677169, -325173.566694686, -121309.194857123], [3376385.170513898, -325152.282144333, -121288.943956908]]")
            near             = 0.1
            far              = 10000.0
            speed            = 3.0
            lodDecider       = DefaultMetrics.mars
        }
    
    let scene = mars()

    let preTransform =
        let bb = scene.boundingBox
        Trafo3d.Translation(-bb.Center) * scene.preTransform
    
    let up = scene.boundingBox.Center.Normalized

module App =
    
    type Action =
        | CameraMessage of CameraController.Message
        | PickingAction of V3d
    
    let update (m : Model) (act : Action) =
        match act with
        | CameraMessage msg -> {m with camera = CameraController.update m.camera msg}
        | PickingAction v -> printfn "%A" v; m
    
    let viewScene (m : MModel) =
        Sg2.createFlatISg Mars.scene
        |> Sg.noEvents
        |> Sg.transform Mars.preTransform
        |> Sg.trafo (Mod.constant (Trafo3d.RotateInto(Mars.up, V3d.OOI)))
        |> Sg.effect [
                  DefaultSurfaces.trafo |> toEffect
                  DefaultSurfaces.constantColor C4f.White |> toEffect
                  DefaultSurfaces.diffuseTexture |> toEffect
        ]

    let view (m : MModel) =
        let frustum = Mod.constant (Frustum.perspective 60.0 0.1 1000.0 1.0)
        require Html.semui (
            div [clazz "ui"][
                CameraController.controlledControl m.camera CameraMessage frustum
                    (AttributeMap.ofList[
                        attribute "style" "background:#121212; width:100%; height:100%"
                    ]) (viewScene m)
            ]
        )
    
    let initial =
        let camera = CameraController.initial
        {camera = camera}

    //let initial' =
    //    let camera = CameraController.initial
    //    let up = Mars.scene.boundingBox.Center.Normalized
    //    {
    //        camera = {camera with view = CameraView.look V3d.OOO -V3d.OIO up}
    //    }

    let threads (m : Model) =
        CameraController.threads m.camera |> ThreadPool.map CameraMessage
    
    let app =
        {
            unpersist = Unpersist.instance
            threads   = threads
            initial   = initial
            update    = update
            view      = view
        }
    
    let start() = App.start app