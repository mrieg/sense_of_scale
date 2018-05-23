namespace Boxes
open Aardvark.Base

module LabelEdges =

    type EdgeDir =
        | X
        | Y
        | Z
    
    let calcSilhouette (box : Box3d) (trafo : Trafo3d) (view : CameraView) : Line3d list =
        let vp  = view.Location
        let min = box.Min
        let max = box.Max
        let corners = box.ComputeCorners()
        
        let faceFront  = Quad3d( corners.[0b011], corners.[0b010], corners.[0b110], corners.[0b111] )
        let faceBack   = Quad3d( corners.[0b000], corners.[0b001], corners.[0b101], corners.[0b100] )
        let faceTop    = Quad3d( corners.[0b111], corners.[0b110], corners.[0b100], corners.[0b101] )
        let faceBottom = Quad3d( corners.[0b000], corners.[0b010], corners.[0b011], corners.[0b001] )
        let faceLeft   = Quad3d( corners.[0b001], corners.[0b011], corners.[0b111], corners.[0b101] )
        let faceRight  = Quad3d( corners.[0b010], corners.[0b000], corners.[0b100], corners.[0b110] )

        let trans = trafo.Forward.TransformPos
        let nm = trafo.Forward.Inverse.Transposed
        let tNorm = nm.TransformPos

        let dotFront  = V3d.Dot( tNorm faceFront.Normal, (vp - trans (faceFront.ComputeCentroid()).Normalized) )
        let dotBack   = V3d.Dot( tNorm faceBack.Normal, (vp - trans (faceBack.ComputeCentroid()).Normalized) )
        let dotTop    = V3d.Dot( tNorm faceTop.Normal, (vp - trans (faceTop.ComputeCentroid()).Normalized) )
        let dotBottom = V3d.Dot( tNorm faceBottom.Normal, (vp - trans (faceBottom.ComputeCentroid()).Normalized) )
        let dotLeft   = V3d.Dot( tNorm faceLeft.Normal, (vp - trans (faceLeft.ComputeCentroid()).Normalized) )
        let dotRight  = V3d.Dot( tNorm faceRight.Normal, (vp - trans (faceRight.ComputeCentroid()).Normalized) )   

        let front =
            match dotFront > 0.0 with
            | false -> []
            | true ->
                let e0 =
                    match dotBottom <= 0.0 with
                    | true -> [faceFront.GetEdgeLine(0)]
                    | false -> []
                    
                let e1 =
                    match dotRight <= 0.0 with
                    | true -> [faceFront.GetEdgeLine(1)]
                    | false -> []
                    
                let e2 =
                    match dotTop <= 0.0 with
                    | true -> [faceFront.GetEdgeLine(2)]
                    | false -> []
                    
                let e3 =
                    match dotLeft <= 0.0 with
                    | true -> [faceFront.GetEdgeLine(3)]
                    | false -> []
                    
                List.concat [e0; e1; e2; e3]
        
        let back =
            match dotBack > 0.0 with
            | false -> []
            | true ->
                let e0 =
                    match dotBottom <= 0.0 with
                    | true -> [faceBack.GetEdgeLine(0)]
                    | false -> []
                    
                let e1 =
                    match dotLeft <= 0.0 with
                    | true -> [faceBack.GetEdgeLine(1)]
                    | false -> []
                    
                let e2 =
                    match dotTop <= 0.0 with
                    | true -> [faceBack.GetEdgeLine(2)]
                    | false -> []
                    
                let e3 =
                    match dotRight <= 0.0 with
                    | true -> [faceBack.GetEdgeLine(3)]
                    | false -> []
                    
                List.concat [e0; e1; e2; e3]
            
        let top =
            match dotTop > 0.0 with
            | false -> []
            | true ->
                let e0 =
                    match dotFront <= 0.0 with
                    | true -> [faceTop.GetEdgeLine(0)]
                    | false -> []
                    
                let e1 =
                    match dotRight <= 0.0 with
                    | true -> [faceTop.GetEdgeLine(1)]
                    | false -> []
                    
                let e2 =
                    match dotBack <= 0.0 with
                    | true -> [faceTop.GetEdgeLine(2)]
                    | false -> []
                    
                let e3 =
                    match dotLeft <= 0.0 with
                    | true -> [faceTop.GetEdgeLine(3)]
                    | false -> []
                    
                List.concat [e0; e1; e2; e3]
            
        let bottom =
            match dotBottom > 0.0 with
            | false -> []
            | true ->
                let e0 =
                    match dotRight <= 0.0 with
                    | true -> [faceBottom.GetEdgeLine(0)]
                    | false -> []
                    
                let e1 =
                    match dotFront <= 0.0 with
                    | true -> [faceBottom.GetEdgeLine(1)]
                    | false -> []
                    
                let e2 =
                    match dotLeft <= 0.0 with
                    | true -> [faceBottom.GetEdgeLine(2)]
                    | false -> []
                    
                let e3 =
                    match dotBack <= 0.0 with
                    | true -> [faceBottom.GetEdgeLine(3)]
                    | false -> []
                    
                List.concat [e0; e1; e2; e3]
            
        let left =
            match dotLeft > 0.0 with
            | false -> []
            | true ->
                let e0 =
                    match dotBottom <= 0.0 with
                    | true -> [faceLeft.GetEdgeLine(0)]
                    | false -> []
                    
                let e1 =
                    match dotFront <= 0.0 with
                    | true -> [faceLeft.GetEdgeLine(1)]
                    | false -> []
                    
                let e2 =
                    match dotTop <= 0.0 with
                    | true -> [faceLeft.GetEdgeLine(2)]
                    | false -> []
                    
                let e3 =
                    match dotBack <= 0.0 with
                    | true -> [faceLeft.GetEdgeLine(3)]
                    | false -> []
                    
                List.concat [e0; e1; e2; e3]
            
        let right =
            match dotRight > 0.0 with
            | false -> []
            | true ->
                let e0 =
                    match dotBottom <= 0.0 with
                    | true -> [faceRight.GetEdgeLine(0)]
                    | false -> []
                    
                let e1 =
                    match dotBack <= 0.0 with
                    | true -> [faceRight.GetEdgeLine(1)]
                    | false -> []
                    
                let e2 =
                    match dotTop <= 0.0 with
                    | true -> [faceRight.GetEdgeLine(2)]
                    | false -> []
                    
                let e3 =
                    match dotFront <= 0.0 with
                    | true -> [faceRight.GetEdgeLine(3)]
                    | false -> []
                    
                List.concat [e0; e1; e2; e3]
            
        List.concat [front; back; top; bottom; left; right]

    let calcLabelEdges (box : Box3d) (trafo : Trafo3d) (view : CameraView) =
        
        let findEdgeDirection (e : Line3d) =
            let dir = e.Direction
            if dir.X <> 0.0 then EdgeDir.X
            elif dir.Y <> 0.0 then EdgeDir.Y
            else EdgeDir.Z
        
        let labelEdges =
            let sil = calcSilhouette box trafo view
            let xEdges =
                sil
                |> List.filter ( fun e ->
                    match findEdgeDirection e with
                    | EdgeDir.X -> true
                    | _ -> false
                )
            
            let yEdges =
                sil
                |> List.filter ( fun e ->
                    match findEdgeDirection e with
                    | EdgeDir.Y -> true
                    | _ -> false
                )
            
            let zEdges =
                sil
                |> List.filter ( fun e ->
                    match findEdgeDirection e with
                    | EdgeDir.Z -> true
                    | _ -> false
                )
            
            let vdir  = view.Forward.Normalized
            let up    = view.Up.Normalized
            let right = view.Right.Normalized
            
            let x =
                match xEdges.Length with
                | 2 -> Some xEdges.[0]
                | _ -> None
            
            let y =
                match yEdges.Length with
                | 2 -> Some yEdges.[0]
                | _ -> None
            
            let z =
                match zEdges.Length with
                | 2 -> Some zEdges.[0]
                | _ -> None
            
            (x, y, z)
        
        labelEdges