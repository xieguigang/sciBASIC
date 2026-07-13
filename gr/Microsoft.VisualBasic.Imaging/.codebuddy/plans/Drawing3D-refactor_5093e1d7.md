---
name: Drawing3D-refactor
overview: 重构 Microsoft.VisualBasic.Imaging 项目 Drawing3D 文件夹的 3D 渲染流程，优化数学与渲染性能、改进光照模型、优化 Camera，合并冗余对象，并按 .NET 规范重命名类型/成员/文件。
todos:
  - id: math-core-unify
    content: 统一旋转/投影/PointXY 到 Point3D 并预缓存三角函数，去除 Vector3D 重复实现
    status: completed
  - id: point3d-rename
    content: 将 Point3D 小写方法改 PascalCase，修复 _X 残留与 Project 深度处理，同步文件夹内调用方
    status: completed
    dependencies:
      - math-core-unify
  - id: light-camera-optimize
    content: 改进 Light 光照模型并修正命名空间，封装 Camera 字段、合并 Rotate 重载、修复空 Catch
    status: completed
    dependencies:
      - math-core-unify
  - id: render-pipeline-perf
    content: Painter 排序改 O(n log n)，优化 Isometric 排序并合并 IsPointInPoly、清理死代码
    status: completed
    dependencies:
      - light-camera-optimize
  - id: redundant-cleanup
    content: 清理 DistanceFieldSampler Unity 残留与 Path3D 可疑逻辑等冗余代码
    status: completed
    dependencies:
      - render-pipeline-perf
  - id: naming-namespace-sync
    content: 修正 Path3D 命名空间、重命名文件名、字段改属性，并同步更新 imaging 项目调用方
    status: completed
    dependencies:
      - redundant-cleanup
  - id: cross-platform-build
    content: 编译 imaging.NET5.vbproj 双平台分支并修复测试，验证重构结果
    status: completed
    dependencies:
      - naming-namespace-sync
---

## 用户需求

对 `Microsoft.VisualBasic.Imaging` 项目中 `Drawing3D` 文件夹（含 `Math3D`、`Models`、`Models/Shapes`、`Models/Paths` 全部子目录）的 VB.NET 3D 绘图代码进行整体重构，解决三类已知问题并达成以下目标。

## 重构范围

- 覆盖整个 `Drawing3D` 文件夹（用户已确认“整个文件夹”）。
- 允许破坏性修改（公开 API、类型、文件名均可重命名/合并）。
- 允许渲染结果（颜色、明暗、排序顺序）因改进而发生变化。
- 命名遵循 .NET 标准（PascalCase 类型/方法，camelCase 局部变量，公共字段改属性）。

## 核心目标

1. **提升渲染流程性能**：重构 `PainterAlgorithm`、`Light`、`IsometricEngine` 三模块的数学算法，消除 O(n²) 选择排序与 O(n²) 两两相交拓扑排序，旋转/投影批处理并预计算三角函数，减少中间数组与 LINQ 分配。
2. **合并冗余对象**：将分散在 `Point3D`、`Vector3D`、`Camera`、`Transformation` 中的旋转/投影实现统一到单一权威实现；合并重复的 `IsPointInPoly` 重载；删除已废弃的 `Illumination` 与 `DistanceFieldSampler` 的 Unity 移植残留。
3. **命名规范化**：将 `Point3D` 等类型的小写/camelCase 公开方法改为 PascalCase；公共字段改为属性；修正 `Light.vb`（文件在根目录但命名空间为 `Drawing3D.Device`）及 `Path3D`（命名空间 `Drawing3D.Models.Isometric`）的命名空间与文件位置不一致问题，并对不规范的类型/文件名重命名。
4. **改进光照算法**：以 Lambert 漫反射 + 环境光项替代现有近似模型，增加法线朝向（背面）校正与退化法线（magnitude=0）防护，集中到统一的 `Light` 模块。
5. **优化 Camera 对象**：封装公开字段为属性，去除冗余的 `Rotate`/`RotateX/Y/Z` 重载组，修复 `Lighting` 中静默吞异常（空 Catch）的问题，批量化旋转/投影。
6. **保持编译与可测**：确保 Linux（`IGraphics` 分支）与 Windows（`System.Drawing.Graphics` 分支，`#If WINDOWS`）双分支均可编译，并同步更新 imaging 项目内所有调用方后通过构建与测试。

## 技术栈选择

- 语言/框架：VB.NET / .NET 5（`imaging.NET5.vbproj`），不引入新依赖或第三方库。
- 跨平台抽象：复用现有 `IGraphics` 接口与 `#If WINDOWS` 条件编译，保证 Linux 与 Windows 双分支均保持可编译。
- 数学原语：以 `Point3D`（值类型结构）作为唯一几何原语，承载旋转/投影/光照法线计算。

## 实现方案

**总体策略**：以 `Point3D` 为数学核心建立单一旋转/投影/向量运算路径；`Camera` 与 `IsometricEngine` 两条渲染管线共享该核心与统一的 `Light` 模块；用更优算法替换低效排序并清理冗余实现。

**关键技术决策与权衡**：

1. **旋转/投影统一（去重）**：`Point3D.RotateX/Y/Z` 作为权威实现；`Camera.Rotate*` 在单次调用内预计算 `Cos/Sin(angleX/Y/Z)` 后组合三个轴旋转，消除 `Camera`、`Vector3D`、`Transformation` 中重复且各自未缓存三角函数的实现。`Vector3D` 仅保留其批处理投影语义，旋转/点集转换委托给 `Point3D`；`Projection.PointXY` 与 `Vector3D.PointXY` 的边界钳制逻辑合并为 `Point3D` 上的单一扩展方法。
2. **排序性能**：`PainterAlgorithm.OrderProvider` 的选择排序（O(n²)）改为对预计算的“平均 Z”数组使用 `Array.Sort` 并保留索引映射（O(n log n)）。`IsometricEngine.sortPaths` 的 O(n²) 两两相交检测改为“宽相包围盒快速拒绝 + 仅对重叠对做精确相交 + 拓扑排序”，复杂度由 O(n²·E) 降至近似 O(n·log n + k·E)（k 为相交候选数）。输出顺序可能变化（已允许）。
3. **光照模型**：实现 `Lambert 漫反射 + 环境光`：`normal = (v1-v0)×(v2-v0)`，若 `normal.Z<0` 翻转向观察者，归一化后与单位光向点乘取 `max(0, N·L)`，`factor = ambient + (1-ambient)*diffuse`，最终颜色按 `factor` 与 `lightColor` 着色；`magnitude=0` 或顶点不足时返回基色。删除旧 `Illumination`，合并 `Lighting` 两个重载。
4. **Camera 优化**：公开字段（`viewDistance!`、`angleX!`、`fov!`、`screen`、`offset`、`lightAngle` 等）封装为属性（保留名称）；`Rotate` 系列合并为“单点 + 点集”两组并复用缓存三角值；`Lighting` 空 Catch 改为：几何不足时返回基色，异常不再静默吞没（保留安全回退并记录）。
5. **命名与结构**：`Point3D.add/subtract/multiply/divide/length/normalize/translate/distance/lerp/rotateYP` → PascalCase；修复 `add/subtract` 中已不存在的 `_X/_Y/_Z` 残留；`Transformation.OffSets`→`Offsets`、`Centra`→`Centroid`；`Surface.vertices/brush`、`Model2D` 字段改为属性；`Light` 命名空间修正为与文件位置一致；`Path3D` 命名空间改为 `Drawing3D.Models`；源文件名与主要类型对齐（`Painter.vb`→`PainterAlgorithm.vb`、`Isometric.vb`→`IsometricEngine.vb`）。

**性能与可靠性**：热路径（逐顶点投影、排序、相交检测）避免重复数组分配与 LINQ `ToArray`；三角函数每帧仅计算一次；退化输入（零法线、空集合、投影深度≤0）均有安全回退，避免 `NaN`/`Infinity` 进入绘制。

## 实现要点（执行须知）

- **双分支编译**：所有改动须同时保证 `#If WINDOWS` 的 `Graphics`/`Graphics2D` 分支与 Linux 的 `IGraphics` 分支可编译，不得删除任一分支逻辑。
- **死代码清理**：移除 `BufferPainting` 内被注释的 `Illumination` 调用、`FindItemForPosition` 中永不执行的分支（`If point.Y = top.Y … If point.Y <> top.Y`）、`DistanceFieldSampler` 的 `#Region "Unity Junk"` 与从未赋值的 `_lastSampleSize`/`NeedsUpdate` 死逻辑、`Point3D` 的 `_X/_Y/_Z` 残留。
- **爆炸半径控制**：因允许破坏性修改，需全仓检索并更新对 `Point3D` 重命名方法、`Light`/`Camera`/`Surface`/`Path3D` 重命名成员与命名空间的引用，范围含 `Drawing3D` 内部及 imaging 项目的 `SVG/`、`Drawing2D/`、`test/` 调用方。

## 架构设计

重构后两条渲染管线共享 `Point3D` 数学核心与统一 `Light` 模块，消除重复旋转/投影实现：

```mermaid
flowchart TD
    A[Surface / Path3D 模型] --> B[Point3D 数学核心: Rotate / Project / PointXY]
    A --> C[统一 Light 模块: Lambert + 环境光]
    B --> D1[Camera + PainterAlgorithm 管线]
    B --> D2[IsometricEngine 管线]
    C --> D1
    C --> D2
    D1 --> E[PainterBuffer: O(n log n) Z 序排序]
    D2 --> F[Isometric: 宽相拒绝 + 拓扑排序]
    E --> G[IGraphics / GDI+ 画布]
    F --> G
```

## 目录结构（受影响文件）

```
Drawing3D/
├── Painter.vb                 # [MODIFY→RENAME PainterAlgorithm.vb] OrderProvider 改为 O(n log n) 索引排序；移除死代码；Polygon 字段改属性；减少 PainterBuffer 分配
├── Light.vb                   # [MODIFY] 命名空间修正为 Drawing3D；实现新光照模型；删除 Illumination；合并 Lighting 重载
├── Isometric.vb               # [MODIFY→RENAME IsometricEngine.vb] 优化 sortPaths（宽相+拓扑排序）；合并 IsPointInPoly 重载；清除 FindItemForPosition 死分支
├── Camera.vb                  # [MODIFY] 字段封装为属性；合并 Rotate/RotateX-Y-Z 重载并预缓存三角函数；修复 Lighting 空 Catch；投影批处理
├── Point3D.vb                 # [MODIFY] 方法改 PascalCase；修复 _X/_Y/_Z 残留；改进 Project 深度处理；统一 PointXY 边界钳制
├── Surface.vb                 # [MODIFY] vertices/brush 改为属性；保持 I3DModel/Draw/Copy 行为
├── Model2D.vb                 # [MODIFY] Friend 字段改属性/一致命名；isLine/isDot 健壮化
├── Math3D/
│   ├── Vector3D.vb            # [MODIFY] 删除重复的 RotateX/Y/Z、Project、PointXY，委托 Point3D
│   ├── Transformation.vb      # [MODIFY] OffSets→Offsets；Centra→Centroid；绕原点旋转复用缓存
│   ├── Projection.vb          # [MODIFY] 合并 PointXY 至 Point3D；清理冗余薄包装
│   └── DistanceFieldSampler.vb# [MODIFY] 移除 Unity 残留与 _lastSampleSize/NeedsUpdate 死逻辑
├── Models/
│   ├── Path3D.vb              # [MODIFY] 命名空间改 Drawing3D.Models；修正 CountCloserThan 可疑逻辑
│   ├── Shape3D.vb / Mesh.vb / Cube.vb / Line.vb / Extensions.vb / I3DModel.vb / IPointCloud.vb / DrawGraphics.vb
│   │                          # [MODIFY] 同步引用重命名后的成员/命名空间
│   ├── Shapes/*.vb            # [MODIFY] 同步重命名成员引用
│   └── Paths/*.vb             # [MODIFY] 同步重命名成员引用
└── （imaging 项目内）SVG/ Drawing2D/ test/
                               # [MODIFY] 更新对重命名类型/方法/命名空间的所有引用，保证编译与测试通过
```

## 关键代码结构（示意）

仅给出改进后光照与旋转核心的接口级定义，供实现对齐：

```
' 统一光照模型（位于 Drawing3D.Light 模块）
<Extension>
Public Function ComputeLighting(
    vertices As Point3D(),        ' 构成表面的顶点
    lightDirection As Point3D,    ' 单位向量，指向光源
    baseColor As Color,
    ambientStrength As Double,    ' 环境光强度 0~1
    lightColor As Color) As Color
    ' 1) 取 (v1-v0)×(v2-v0) 计算面法线
    ' 2) 若 normal.Z < 0 翻转向观察者
    ' 3) magnitude=0 或顶点<3 时返回 baseColor
    ' 4) diffuse = Max(0, normal·lightDirection)
    ' 5) factor = ambientStrength + (1-ambientStrength)*diffuse
    ' 6) 按 factor 与 lightColor 着色 baseColor 后返回
End Function

' Camera 旋转核心（单次预缓存三角函数）
Private Function RotatePoints(points As IEnumerable(Of Point3D)) As IEnumerable(Of Point3D)
    Dim (cosX, sinX) = Trig(angleX)
    Dim (cosY, sinY) = Trig(angleY)
    Dim (cosZ, sinZ) = Trig(angleZ)
    ' 对每个点依次 RotateX→RotateY→RotateZ，复用上述三角值
End Function
```