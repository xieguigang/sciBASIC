# 3D Mesh Model Parsers And Voxelization Toolkit

Reads common 3D mesh and point-cloud file formats into one unified scene model, then converts that geometry into voxel and signed-distance volumes for CFD simulation.

## Overview
- A single entry point (`ModelLoader.LoadModel`) auto-detects and parses STL (ASCII and binary), glTF 2.0 (`.gltf` and `.glb`), Wavefront OBJ, COLLADA `.dae`, 3D-Studio `.3ds` and 3MF (`.3mf`, ZIP container + XML) into one `SceneModel`.
- PLY files (ASCII, binary little-endian and big-endian) are read into `PointCloud` objects and written back with `PlyWriter`.
- Voxelization: a column-based ray-casting voxelizer for solid CFD volumes, an anti-aliased occupancy variant, and a BVH-accelerated SDF voxelizer producing signed-distance volumes.
- glTF support covers buffers, buffer views, accessors, meshes/primitives, materials, nodes and scene graphs, with shared model-building logic for both the JSON and the GLB container forms.
- 3MF support covers the XML model/resources/mesh object graph plus the ZIP package IO.

## Key Types
- `Microsoft.VisualBasic.Imaging.Landscape.Data.ModelLoader` — format detection and unified loading, plus `VoxelizeForCFD` and the `ModelFormat` enum.
- `Microsoft.VisualBasic.Imaging.Landscape.Data.SceneModel` / `.Surface` / `.Vertex` — the unified triangle-surface scene representation.
- `Microsoft.VisualBasic.Imaging.Landscape.Gltf.GltfReader` / `.GlbReader` / `.GltfRoot` — glTF JSON and GLB binary parsing plus the parsed glTF object model.
- `Microsoft.VisualBasic.Imaging.Landscape.Wavefront.ObjTextParser` / `.ObjModel` / `.ObjGroup` — Wavefront OBJ text parsing and model objects.
- `Microsoft.VisualBasic.Imaging.Landscape.ThreeMF.ModelIO` / `.Project` / `.Xml.Mesh` — 3MF package IO and XML model handling.
- `Microsoft.VisualBasic.Imaging.Landscape.Voxelization.Voxelizer` / `.VoxelModel` — column ray-casting solid voxelization for CFD.
- `Microsoft.VisualBasic.Imaging.Landscape.Voxelization.SDFVoxelizer` / `.SDFVolume` / `.BVH` — signed-distance-field volumes with BVH nearest-triangle acceleration.
- `Microsoft.VisualBasic.Imaging.Landscape.Ply.PlyReader` / `.PlyWriter` / `.PointCloud` — PLY point-cloud IO.

## Quick Start
```vbnet
Imports Microsoft.VisualBasic.Imaging.Landscape.Data
Imports Microsoft.VisualBasic.Imaging.Landscape.Voxelization

Dim model As SceneModel = ModelLoader.LoadModel("scene.obj")
Dim voxels As VoxelModel = ModelLoader.VoxelizeForCFD("scene.obj", resolution:=128)

Console.WriteLine(model.Surfaces.Length)
```

## Package
- Assembly: `Microsoft.VisualBasic.Imaging.Landscape`
- TargetFramework: `net10.0`
- Tags: `scibasic;3d-model;mesh-parser;point-cloud;voxelization;geometry`

## License
GPL-3.0-or-later
