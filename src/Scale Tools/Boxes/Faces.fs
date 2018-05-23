namespace Boxes

open Aardvark.Base

module Faces =
    let faceIndex (f : FaceType) =
        match f with
        | FaceFront -> 0
        | FaceBack -> 1
        | FaceTop -> 2
        | FaceBottom -> 3
        | FaceLeft -> 4
        | FaceRight -> 5
        
    let boxFaces (center : V3d) (size : V3d) =
        let c = center
        let s = size * 0.5
        let front =
            {
                center = c + V3d(0.0, s.Y, 0.0)
                normal = V3d.OIO
                faceType = FaceType.FaceFront
            }
    
        let back =
            {
                center = c - V3d(0.0, s.Y, 0.0)
                normal = -V3d.OIO
                faceType = FaceType.FaceBack
            }
            
        let top =
            {
                center = c + V3d(0.0, 0.0, s.Z)
                normal = V3d.OOI
                faceType = FaceType.FaceTop
            }
            
        let bottom =
            {
                center = c - V3d(0.0, 0.0, s.Z)
                normal = -V3d.OOI
                faceType = FaceType.FaceBottom
            }
    
        let left =
            {
                center = c + V3d(s.X, 0.0, 0.0)
                normal = -V3d.IOO
                faceType = FaceType.FaceLeft
            }
    
        let right = 
            {
                center = c - V3d(s.X, 0.0, 0.0)
                normal = V3d.IOO
                faceType = FaceType.FaceRight
            }
            
        [front; back; top; bottom; left; right]