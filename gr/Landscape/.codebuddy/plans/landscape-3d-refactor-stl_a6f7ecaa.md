---
name: landscape-3d-refactor-stl
overview: 重构 Landscape 三维模型读取项目的源代码，修复现有 Bug，新增 STL 格式（ASCII + Binary）解析支持，补全缺失代码（COLLADA 解析器、Extensions 模块等）。
todos:
  - id: fix-vector-scan0
    content: 修复 Vector.vb 中 Scan0 编译错误，将第83行 v(Scan0) 改为 v(0)
    status: completed
  - id: refactor-obj-parser
    content: 重构 Wavefront/TextParser.vb：修复 Scan0 错误、改用 g/o 指令分隔 ObjectPart、CSng 替代 Val、支持多顶点面解析
    status: completed
  - id: fix-xmlmodel-error-handling
    content: 修复 3DBuilder/XML/XmlModel3D.vb 中 On Error Resume Next，改用 Try-Catch 正确错误处理
    status: completed
  - id: fix-io-type-conflict
    content: 修复 3DBuilder/IO.vb 中 NotNull 方法的 [object] 类型名称冲突
    status: completed
  - id: complete-collada-parser
    content: 完成 COLLADA/COLLADA.vb 的几何数据解析器，支持从 DAE 文件读取顶点和三角面数据
    status: completed
  - id: fill-extensions-module
    content: 补充 3DBuilder/Extensions.vb 空模块体，添加必要的扩展方法
    status: completed
  - id: add-stl-parser
    content: 新增 STL/STLParser.vb，实现 ASCII 和 Binary 两种 STL 格式的解析支持
    status: completed
---

## 用户需求

对现有 VB.NET 三维模型文件读取项目进行以下工作：

### 1. 源代码重构与 Bug 修复

项目当前存在代码混乱、多处编译错误和逻辑缺陷，需要全面重构：

- 修复 `Vector.vb` 中 `Scan0` 未定义的编译错误（应改为 `0`）
- 修复 `Wavefront/TextParser.vb` 中 OBJ 解析器的多个问题：`Scan0` 编译错误、空行误触发 ObjectPart 创建、`Val` 函数精度丢失、面解析不支持多于3个顶点
- 修复 `3DBuilder/XML/XmlModel3D.vb` 中过时的 `On Error Resume Next` 错误抑制
- 修复 `3DBuilder/IO.vb` 中 `[object]` 类型名称与 XML 模型类冲突的问题

### 2. 新增 STL 格式支持

新增 STL (.stl) 三角网格三维模型文件的解析支持，同时支持 ASCII 文本格式和 Binary 二进制格式的 STL 文件读取。解析结果应输出为项目统一的 `Surface()` 数据结构。

### 3. 补充缺失代码

- COllADA/COLLADA.vb 目前仅有空的 `version` 和 `asset` 类，需要实现基本的几何数据读取（顶点坐标、法向量、三角面索引）
- 3DBuilder/Extensions.vb 模块体完全为空，需要补充必要的扩展方法

## 技术栈

- **语言**: VB.NET
- **目标框架**: net10.0
- **核心依赖**: Microsoft.VisualBasic.Imaging.Drawing3D（提供 `Point3D`、`Surface`、`Brush` 等 3D 基础类型）、Microsoft.VisualBasic.Core（提供 `Value`、`DoCall`、`JoinBy` 等语言扩展）、System.Drawing.Common
- **数据模型**: `Data.Graphics`（顶层容器） → `Data.Surface()`（三角面/网格） → `Data.Vector()`（三维点坐标）

## 实现方案

### 整体策略

采用按格式模块独立重构的方式，每个格式解析器自包含在其命名空间中，对外输出统一的 `Data.Surface()` 数组。不引入新的架构模式，在现有项目结构基础上进行修复和扩展。

### Bug 修复方案

#### Vector.vb — Scan0 编译错误

`Scan0` 是常量名但未在项目中定义。根据上下文（数组索引），应直接使用 VB.NET 的标准数组索引 `0`。将第 83 行 `v(Scan0)` 改为 `v(0)`。

#### Wavefront/TextParser.vb — OBJ 解析器重构

核心问题在于当前实现将空行作为 ObjectPart 分隔符，但 OBJ 规范中空行不具语义意义。应采用 `g`（组名）或 `o`（对象名）指令作为 ObjectPart 的正确分隔边界。同时：

- 所有 `Scan0` 替换为 `0`（第 88 行、第 107 行）
- 将 `Val()` 替换为 `CSng()` 以保证浮点精度
- 面解析支持四边形（4个顶点）及更多顶点的 polygon，使用三角扇分解
- 对 `f` 行缺少 `vn` 索引的情况增加防御性检查（如只有 `v` 没有 `v/vt/vn` 的格式）

#### XmlModel3D.vb — 错误处理现代化

移除第 102 行 `On Error Resume Next`，用 `Try...Catch` 包裹循环体内部的关键操作。当 `CInt(obj.pindex)` 转换失败或 `mesh` 为 Nothing 时，跳过当前对象并继续处理下一个，同时可通过现有项目日志机制记录警告。

#### IO.vb — 类型名称冲突

第 85 行 `NotNull` 的参数类型 `[object]` 与 `resources.vb` 中定义的 `[object]` 类冲突。应将其改为 `XML.object`（使用完全限定名）以消除歧义。

### COLLADA 解析器实现

分析 `house.dae` 文件结构，COLLADA 的基本几何数据组织为：

- `<library_geometries>` → `<geometry>` → `<mesh>` → `<source>`（含 `<float_array>` 顶点/法向量数据） + `<vertices>` + `<triangles>`（含 `<p>` 索引列表）

实现方案：使用 VB.NET 的 `XElement`/LINQ to XML 解析 DAE 文件，提取每个 `<geometry>` 中的顶点坐标和三角面索引，转换为 `Surface()` 输出。暂不实现材质（material）、纹理坐标（texcoord）的完整解析，但保留扩展点。

### STL 格式解析器实现

#### ASCII STL

文本格式以 `solid <name>` 开头，`endsolid <name>` 结尾。每个三角面由 `facet normal` → `outer loop` → 三个 `vertex` → `endloop` → `endfacet` 组成。逐行解析，提取法向量和三个顶点坐标，构建 `Surface()`。

#### Binary STL

二进制格式结构：80 字节文件头 + 4 字节 UInt32（三角面数量） + 50 字节/三角面（12 字节法向量 + 36 字节顶点 + 2 字节属性）。使用 `BinaryReader` 按字节读取，首先读取 80 字节头（跳过），然后读取三角面计数，循环读取每个三角面的数据。

自动检测策略：如果文件以 "solid "（忽略大小写）开头则为 ASCII，否则按 Binary 处理。

### 目录结构规划

```
Landscape/
├── Landscape.vbproj              # [MODIFY] 无需修改，SDK 项目自动包含新文件
├── Graphics.vb                   # [不变] 主数据模型容器
├── Surface.vb                    # [不变] 表面数据模型
├── Vector.vb                     # [MODIFY] 修复 Scan0 → 0
├── 3DBuilder/
│   ├── Extensions.vb             # [MODIFY] 补充扩展方法
│   ├── IO.vb                     # [MODIFY] 修复 [object] 类型冲突
│   ├── Project.vb                # [不变]
│   └── XML/
│       ├── models.vb             # [不变]
│       ├── resources.vb          # [不变]
│       └── XmlModel3D.vb         # [MODIFY] 替换 On Error Resume Next
├── COLLADA/
│   └── COLLADA.vb                # [MODIFY] 完成几何数据解析器
├── PLY/
│   ├── Header.vb                 # [不变]
│   ├── PointCloud.vb             # [不变]
│   └── SimplePlyWriter.vb        # [不变]
├── Wavefront/
│   ├── FileName.vb               # [不变] Triangle 类
│   ├── OBJ.vb                    # [不变] OBJ 数据模型
│   └── TextParser.vb             # [MODIFY] 重构 OBJ 解析逻辑
└── STL/
    └── STLParser.vb              # [NEW] STL 解析器（ASCII + Binary）
```

### 关键代码结构

#### STLParser 模块接口

```
Namespace STL
    Public Module STLParser
        ' 自动检测格式并解析
        Public Function ParseSTL(filePath As String) As Surface()
        ' 从 Stream 解析（自动检测 ASCII/Binary）
        Public Function ParseSTL(stream As Stream) As Surface()
        ' 显式解析 ASCII STL
        Public Function ParseAsciiSTL(reader As StreamReader) As Surface()
        ' 显式解析 Binary STL
        Public Function ParseBinarySTL(reader As BinaryReader) As Surface()
    End Module
End Namespace
```

#### COLLADA 几何解析器补充

```
Namespace COLLADA
    Public Module COLLADAParser
        ' 从 DAE 文件读取几何数据
        Public Function ReadGeometries(filePath As String) As Surface()
        ' 从 XDocument 解析
        Public Function ReadGeometries(doc As XDocument) As Surface()
    End Module
End Namespace
```

## 实现注意事项

### 性能

- STL Binary 解析使用 `BinaryReader` 直接按字节读取，单次遍历，O(n) 复杂度（n 为三角面数量）
- OBJ 解析器保持现有的逐行流式读取模式，避免全文件加载到内存
- COLLADA 解析使用 LINQ to XML 的延迟求值特性，但 `float_array` 内容可能很大，需注意一次性解析为数组

### 错误处理

- 统一使用 `Try...Catch` 替代 `On Error Resume Next`
- 对格式不符合预期的行，记录警告并跳过（而非抛异常中断整个文件读取）
- 对 Binary STL 中文件长度不匹配三角面数量声明的情况进行校验

### 兼容性

- 所有修改不改变对外公开接口的签名
- 新增 STL 和 COLLADA 解析器遵循现有项目命名规范（命名空间大写、模块级方法）
- 保持 `Data.Surface` 作为统一输出格式，确保与现有下游代码兼容