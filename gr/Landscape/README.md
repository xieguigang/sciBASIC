# 3D 网格模型解析与体素化工具箱

## 引言

3D 模型的格式之多，几乎每个软件生态都有自己的方言：STL（3D 打印）、OBJ（建模）、glTF/GLB（Web 3D 的事实标准）、COLLADA、3DS（老牌）、3MF（现代 3D 打印）……而 CFD（计算流体力学）之类下游任务并不关心来源格式，它只关心：**有一个统一的面片模型，并能从中得到体素 / 有符号距离场（SDF）**。

本包提供的正是这条链路：

```text
各种格式文件 ──► 统一的 SceneModel ──► 体素 / SDF 体 ──► CFD 网格
```

## 设计目标

- **一格式一解析器**：每种格式各自独立实现，互不干扰；
- **统一中间模型**：所有解析器产出同一个 `SceneModel`，下游只写一遍；
- **面向仿真**：内置体素化与 SDF 生成，直接服务于 CFD。

## 核心特性

- **格式读取器**：
  - STL —— ASCII 与二进制两种编码（`StlAsciiReader`、`StlBinaryReader`、`STLParser`）；
  - Wavefront OBJ —— `ObjTextParser`、`ObjModel`、`Triangle`；
  - glTF / GLB —— `GltfReader`、`GlbReader`、`GltfTypes`、`ModelBuilder`；
  - COLLADA —— `ColladaParser`、`ColladaTypes`；
  - Autodesk 3DS —— `Max3DSParser`、`Max3DSTypes`；
  - 3MF —— `Project`、`ModelIO`、`Extensions` 与 XML 模型（`Mesh`、`Model3D`、`Resources`）；
  - PLY —— 作为**点云**读取（`Header`、`PlyReader`、`PlyWriter`、`PointCloud`）。
- **统一模型**：`Data` 命名空间提供共享场景表示（`SceneModel`、`Surface`、`Vertex`）与按文件类型分派的 `ModelLoader`。
- **体素化**：`Voxelization` 命名空间把面片转换为体 —— `Voxelizer` / `VoxelModel`（表面体素化）、`BVH`（包围体层次，加速求交）、`SDFVoxelizer` / `SDFVolume`（有符号距离场）。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.Imaging.Landscape`（根） | 工具箱入口 |
| `....Landscape.Data` | 统一场景模型与模型加载器 |
| `....Landscape.STL` / `.Wavefront` / `.glTF` / `.COLLADA` / `.Max3DS` / `.ThreeMF` / `.PLY` | 各格式解析器 |
| `....Landscape.Voxelization` | 体素化与 SDF 生成 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Imaging.Landscape
Imports Microsoft.VisualBasic.Imaging.Landscape.Data
Imports Microsoft.VisualBasic.Imaging.Landscape.Voxelization

' 1. 载入任意支持的格式，得到统一场景模型
Dim scene As SceneModel = ModelLoader.Load("./model.stl")

' 2. 表面体素化
Dim voxel As VoxelModel = Voxelizer.Voxelize(scene, resolution:=256)

' 3. 生成有符号距离场（可直接作为 CFD 网格输入）
Dim sdf As SDFVolume = SDFVoxelizer.Compute(scene, resolution:=256)
```

## 实现要点

- **为什么需要 BVH**：SDF 计算要对每个体素求「到最近表面的距离」，朴素实现是「体素数 × 三角形数」；`BVH` 把三角形按空间组织成层次结构，把查询复杂度降到对数级。
- **STL 的两种编码**：ASCII 版可读但体积大，二进制版紧凑但需按固定记录长度解析；两者都必须支持，因为不同上游工具默认输出不同。
- **PLY 的特殊地位**：PLY 常见于扫描点云而非面片，因此本包把它作为点云读取，而不是强行纳入 `SceneModel`。

## 包信息

- Assembly：`Microsoft.VisualBasic.Imaging.Landscape`
- TargetFramework：`net10.0`
- Tags：`scibasic;3d-model;mesh-parser;point-cloud;voxelization;sdf;stl;obj;gltf;collada;3mf`
- 许可：GPL-3.0-or-later
