namespace Mars

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph
open Aardvark.UI
open Aardvark.SceneGraph.Opc
open MBrace.FsPickler

module Shader =
    
    open FShade
    
    let mkNormals (tri : Triangle<Effects.Vertex>) =
        triangle {
            let a = tri.P1.wp.XYZ - tri.P0.wp.XYZ
            let b = tri.P2.wp.XYZ - tri.P0.wp.XYZ
            let n = V3d.Cross(a,b)
            yield {tri.P0 with n = n}
            yield {tri.P1 with n = n}
            yield {tri.P2 with n = n}
        }

module Terrain =
    
    let pickler = FsPickler.CreateBinarySerializer()
    let pickle = pickler.Pickle<QTree<Patch>>
    let unpickle = pickler.UnPickle<QTree<Patch>>
    
    let patchHierarchies =
        System.IO.Directory.GetDirectories(@"..\..\data\mars")
        |> Seq.collect System.IO.Directory.GetDirectories
    
    let pHs =
        [ 
            for h in patchHierarchies do
                yield PatchHierarchy.load pickle unpickle h
        ]
    
    type OPCScene =
        {
            useCompressedTextures : bool
            preTransform          : Trafo3d
            patchHierarchies      : seq<string>
            boundingBox           : Box3d
            near                  : float
            far                   : float
            speed                 : float
        }
    
    let mars() =
        { 
            useCompressedTextures = true
            preTransform     = Trafo3d.Identity
            patchHierarchies = patchHierarchies
            boundingBox      = Box3d.Parse("[[3376372.058677169, -325173.566694686, -121309.194857123], [3376385.170513898, -325152.282144333, -121288.943956908]]")
            near             = 0.1
            far              = 10000.0
            speed            = 3.0
        }
    
    let scene = mars()
    
    let preTransform =
        let bb = scene.boundingBox
        Trafo3d.Translation(-bb.Center) * scene.preTransform
    
    let up = scene.boundingBox.Center.Normalized

    let mkISg() =
        Sg2.createFlatISg pickle unpickle patchHierarchies
        |> Sg.noEvents
        |> Sg.transform preTransform
    
    let defaultEffects =
        [
            DefaultSurfaces.trafo                   |> toEffect
            DefaultSurfaces.constantColor C4f.White |> toEffect
            DefaultSurfaces.diffuseTexture          |> toEffect
        ]
    
    let simpleLightingEffects =
        let col = C4f(V4d(0.8, 0.5, 0.5, 1.0))
        [
            DefaultSurfaces.trafo             |> toEffect
            Shader.mkNormals                  |> toEffect
            DefaultSurfaces.constantColor col |> toEffect
            DefaultSurfaces.simpleLighting    |> toEffect
        ]
    
    let mutable min     = V3d.III * 50000000.0
    let mutable max     = -V3d.III * 50000000.0

    let mutable totalBB = Box3d.Unit.Translated(scene.boundingBox.Center)

    let patchBB() = totalBB.Translated(-scene.boundingBox.Center)
    
    let buildKDTree (g : IndexedGeometry) (local2global : Trafo3d) =
        let pos = g.IndexedAttributes.[DefaultSemantic.Positions] |> unbox<V3f[]>
        let index = g.IndexArray |> unbox<int[]>
    
        let triangles =
            [| 0 .. 3 .. index.Length - 2 |] 
                |> Array.choose (fun bi -> 
                    let p0 = pos.[index.[bi]]
                    let p1 = pos.[index.[bi + 1]]
                    let p2 = pos.[index.[bi + 2]]
                    if isNan p0 || isNan p1 || isNan p2 then
                        None
                    else
                        let a = V3d(float p0.X, float p0.Y, float p0.Z) |> local2global.Forward.TransformPos
                        let b = V3d(float p1.X, float p1.Y, float p1.Z) |> local2global.Forward.TransformPos
                        let c = V3d(float p2.X, float p2.Y, float p2.Z) |> local2global.Forward.TransformPos
                        
                        if a.X < min.X then min.X <- a.X
                        if a.Y < min.Y then min.Y <- a.Y
                        if a.Z < min.Z then min.Z <- a.Z
                        if a.X > max.X then max.X <- a.X
                        if a.Y > max.Y then max.Y <- a.Y
                        if a.Z > max.Z then max.Z <- a.Z
    
                        if b.X < min.X then min.X <- b.X
                        if b.Y < min.Y then min.Y <- b.Y
                        if b.Z < min.Z then min.Z <- b.Z
                        if b.X > max.X then max.X <- b.X
                        if b.Y > max.Y then max.Y <- b.Y
                        if b.Z > max.Z then max.Z <- b.Z
    
                        if c.X < min.X then min.X <- c.X
                        if c.Y < min.Y then min.Y <- c.Y
                        if c.Z < min.Z then min.Z <- c.Z
                        if c.X > max.X then max.X <- c.X
                        if c.Y > max.Y then max.Y <- c.Y
                        if c.Z > max.Z then max.Z <- c.Z
                        
                        totalBB <- Box3d(min, max)
                        Triangle3d(V3d p0, V3d p1, V3d p2) |> Some
                )
        
        let tree = Geometry.KdTree.build Geometry.Spatial.triangle (Geometry.KdBuildInfo(100, 5)) triangles
        tree
    
    let leaves =
        pHs
        |> List.collect(fun x ->
            x.tree |> QTree.getLeaves |> Seq.toList |> List.map(fun y -> (x.baseDir, y)))
    
    let kdTrees =
        leaves
        |> List.map(fun (dir,patch) -> (Patch.load dir patch.info, dir, patch.info))
        |> List.map(fun ((a,_),c,d) -> (a,c,d))
        |> List.map ( fun (g,dir,info) ->
            buildKDTree g info.Local2Global
        )
    
    let pickSg events =
        leaves
        |> List.map(fun (dir,patch) -> (Patch.load dir patch.info, dir, patch.info))
        |> List.map(fun ((a,_),c,d) -> (a,c,d))
        |> List.map2 ( fun t (g,dir,info) ->
            let pckShp = t |> PickShape.Triangles
            Sg.ofIndexedGeometry g
            |> Sg.pickable pckShp
            |> Sg.trafo (Mod.constant info.Local2Global)
        ) kdTrees
        |> Sg.ofList
        |> Sg.requirePicking
        |> Sg.noEvents
        |> Sg.withEvents events
        |> Sg.transform preTransform
        |> Sg.shader {
            do! DefaultSurfaces.trafo
            do! DefaultSurfaces.constantColor C4f.DarkRed
        }
        |> Sg.depthTest (Mod.constant DepthTestMode.Never)

module SimpleTerrain =
    let boundingBox = Box3d.Parse("[[3376372.058677169, -325173.566694686, -121309.194857123], [3376385.170513898, -325152.282144333, -121288.943956908]]")
    let up = boundingBox.Center.Normalized

    let mkISg() =
        Box3d(V3d(-100.0, -100.0, -0.5), V3d(100.0, 100.0, 0.0))
        |> Sg.box' C4b.DarkBlue
        |> Sg.noEvents
        |> Sg.trafo (Trafo3d.RotateInto(V3d.OOI, up) |> Mod.constant)
    
    let defaultEffects =
        [
            DefaultSurfaces.trafo                   |> toEffect
            DefaultSurfaces.constantColor C4f.White |> toEffect
            DefaultSurfaces.simpleLighting          |> toEffect
        ]
    
    let mutable min     = V3d.III * 50000000.0
    let mutable max     = -V3d.III * 50000000.0
    let mutable totalBB = boundingBox
    
    let buildKDTree (g : IndexedGeometry) (local2global : Trafo3d) =
        let pos = g.IndexedAttributes.[DefaultSemantic.Positions] |> unbox<V3f[]>
        let index = g.IndexArray |> unbox<int[]>
    
        let triangles =
            [| 0 .. 3 .. index.Length - 2 |] 
                |> Array.choose (fun bi -> 
                    let p0 = pos.[index.[bi]]
                    let p1 = pos.[index.[bi + 1]]
                    let p2 = pos.[index.[bi + 2]]
                    if isNan p0 || isNan p1 || isNan p2 then
                        None
                    else
                        let a = V3d(float p0.X, float p0.Y, float p0.Z) |> local2global.Forward.TransformPos
                        let b = V3d(float p1.X, float p1.Y, float p1.Z) |> local2global.Forward.TransformPos
                        let c = V3d(float p2.X, float p2.Y, float p2.Z) |> local2global.Forward.TransformPos
                        
                        if a.X < min.X then min.X <- a.X
                        if a.Y < min.Y then min.Y <- a.Y
                        if a.Z < min.Z then min.Z <- a.Z
                        if a.X > max.X then max.X <- a.X
                        if a.Y > max.Y then max.Y <- a.Y
                        if a.Z > max.Z then max.Z <- a.Z
    
                        if b.X < min.X then min.X <- b.X
                        if b.Y < min.Y then min.Y <- b.Y
                        if b.Z < min.Z then min.Z <- b.Z
                        if b.X > max.X then max.X <- b.X
                        if b.Y > max.Y then max.Y <- b.Y
                        if b.Z > max.Z then max.Z <- b.Z
    
                        if c.X < min.X then min.X <- c.X
                        if c.Y < min.Y then min.Y <- c.Y
                        if c.Z < min.Z then min.Z <- c.Z
                        if c.X > max.X then max.X <- c.X
                        if c.Y > max.Y then max.Y <- c.Y
                        if c.Z > max.Z then max.Z <- c.Z
                        
                        totalBB <- Box3d(min, max)
                        Triangle3d(V3d p0, V3d p1, V3d p2) |> Some
                )
        
        let tree = Geometry.KdTree.build Geometry.Spatial.triangle (Geometry.KdBuildInfo(100, 5)) triangles
        tree
    
    let leaves =
        []
        |> List.collect(fun x ->
            x.tree |> QTree.getLeaves |> Seq.toList |> List.map(fun y -> (x.baseDir, y)))
    
    let kdTrees =
        leaves
        |> List.map(fun (dir,patch) -> (Patch.load dir patch.info, dir, patch.info))
        |> List.map(fun ((a,_),c,d) -> (a,c,d))
        |> List.map ( fun (g,dir,info) ->
            buildKDTree g info.Local2Global
        )
    
    let pickSg events =
        mkISg()
        |> Sg.requirePicking
        |> Sg.noEvents
        |> Sg.withEvents events
        |> Sg.shader {
            do! DefaultSurfaces.trafo
            do! DefaultSurfaces.constantColor C4f.DarkRed
        }
        |> Sg.depthTest (Mod.constant DepthTestMode.Never)