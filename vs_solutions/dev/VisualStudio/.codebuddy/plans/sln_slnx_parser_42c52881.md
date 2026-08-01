---
name: sln_slnx_parser
overview: 在 VisualStudio/sln 文件夹中，完善统一的解决方案数据模型（Solution/Project/TypeId），并实现针对经典 .sln（文本格式）和新 .slnx（XML 格式）两种解决方案文件的解析函数（Module 形式）。
todos:
  - id: extend-model
    content: 完善 Solution.vb 统一数据模型，扩充 Project/TypeId/SolutionConfiguration
    status: completed
  - id: fill-global
    content: 填充 Global.vb 的 [Global] 类承载全局区段
    status: completed
    dependencies:
      - extend-model
  - id: impl-sln-parser
    content: 在 Parser.vb 实现 ParseSln 文本解析与辅助函数
    status: completed
    dependencies:
      - extend-model
      - fill-global
  - id: impl-slnx-parser
    content: 在 Parser.vb 实现 ParseSlnx XML 解析函数
    status: completed
    dependencies:
      - extend-model
      - fill-global
  - id: add-dispatch
    content: 新增 Parse(path) 自动识别格式分派函数
    status: completed
    dependencies:
      - impl-sln-parser
      - impl-slnx-parser
---

## 用户需求

使用 VB.NET 在 `sln/` 文件夹中实现针对 Visual Studio 新旧两种解决方案文件（经典 `.sln` 文本格式与 VS2022 17.10+ 引入的 `.slnx` XML 格式）的解析函数。

## 产品概述

在现有 `sln` 命名空间下，先完善统一的数据模型（一套 `Solution`/`Project` 模型同时承载两种格式结果），再在现有 `Parser` Module 中分别实现 `ParseSln(path)` 与 `ParseSlnx(path)` 静态解析函数，能够完整还原解决方案的项目结构、层级关系、配置平台等信息。

## 核心功能

- 完善统一数据模型：扩充 `Solution`、`Project`，新增层级关系（父 GUID / 嵌套文件夹）、项目物理与相对路径、扩充项目类型枚举（解决方案文件夹 / C# / VB / Web 等）、解决方案全局配置与构建配置平台（Debug|AnyCPU 等）。
- 实现经典 `.sln` 文本格式解析：识别格式版本、VisualStudio 版本、项目声明、`GlobalSection` 中的解决方案配置平台、嵌套项目关系、项目配置平台。
- 实现 `.slnx` XML 格式解析：读取 XML 根元素，解析 `<Project>`/`<Folder>`/`<Configuration>` 等节点，映射为统一模型。
- 以 `Parser` Module 的 `ParseSln` / `ParseSlnx` 函数对外暴露 API，保持与现有代码风格一致。

## 技术栈选择

- 语言：VB.NET（与现有项目一致）
- 框架：.NET（沿用现有项目目标框架）
- 依赖：仅使用 BCL（`System.IO`、`System.Xml`、`System.ComponentModel`），不引入第三方库
- 命名空间：`sln`

## 实现方案

### 总体策略

基于现有 `Solution.vb` / `Global.vb` / `Parser.vb` 三个文件进行增量完善，采用单一统一数据模型，两种格式解析函数均输出同一 `Solution` 对象。解析逻辑以纯文本逐行扫描（sln）和 `System.Xml`（slnx）为主，复杂度线性 O(N)。

### 关键技术决策

1. **统一模型**：扩充 `Project` 增加 `ParentGuid`、`RelativePath`、`FullPath`、`TypeId` 已存在但扩充枚举值（如 CSharp、WebSite 等常见 GUID）。`Solution` 增加 `Configurations`（构建配置平台集合）与 `Global` 全局配置对象。
2. **层级关系**：在 `Project` 增加 `ParentGuid`，sln 由 `GlobalSection(NestedProjects)` 解析，slnx 由 `<Project>` 的父节点/Folder 嵌套解析，解析后构建树。
3. **配置平台**：新增 `SolutionConfiguration` 类型（Name 如 `Debug|AnyCPU`），从 `SolutionConfigurationPlatforms` 提取；项目级配置从 `ProjectConfigurationPlatforms` 提取。
4. **API 形态**：沿用现有空 `Module Parser`，新增 `ParseSln(path As String) As Solution` 与 `ParseSlnx(path As String) As Solution`，并提供一个 `Parse(path)` 按扩展名自动分派。
5. **Global.vb**：填充 `Global` 类承载 `GlobalSection` 通用键值（如 `SolutionGuid`），避免信息丢失。

### 性能与可靠性

- sln 解析为单趟逐行读取，避免重复遍历；slnx 使用 `XDocument`/`XmlDocument` 一次性加载（文件通常较小，内存可控）。
- 对缺失区块（如无解配置、无嵌套）提供安全降级（空集合/空字符串），避免空引用异常。
- 路径处理统一使用 `Path` 类，结合 sln 文件所在目录计算相对/绝对路径。

## 实现注意事项

- 复用现有文件版权头与 `#Region` 注释风格，保持代码一致。
- `TypeId` 枚举以 `Description` 特性保存 GUID，复用 `ComponentModel` 解析方式；slnx 解析时同样通过 GUID/Type 属性映射。
- 不在解析器中做写入/修改操作，保持纯函数式（输入路径，输出模型）。
- 避免对无关文件（如 `VBProject/`、`test/`）做任何改动，控制改动范围。

## 架构设计

现有结构（`sln` 命名空间三个文件）已能完整承载需求，无需引入新模块或新架构。数据流向如下：

```mermaid
flowchart TD
    A[.sln 文件] -->|Parser.ParseSln| C[Solution 模型]
    B[.slnx 文件] -->|Parser.ParseSlnx| C[Solution 模型]
    C --> D[Solution: 版本/配置平台]
    C --> E[Project[]: 类型/GUID/路径/父关系]
    C --> F[Global: 全局区段]
```

## 目录结构

```
sln/
├── Solution.vb   # [MODIFY] 完善统一数据模型。扩充 Solution 类（增加 Configurations、Global 属性）；扩充 Project 类（增加 ParentGuid、RelativePath、FullPath，保留并扩充 NodeType/TypeId）；扩充 TypeId 枚举（新增 CSharp、WebSite 等常见项目类型 GUID，保留 FolderGroup/VBProject/NjsProject）；新增 SolutionConfiguration 类型承载构建配置平台。
├── Global.vb     # [MODIFY] 填充 [Global] 类，承载 GlobalSection 通用键值（如 SolutionGuid、其他全局属性），作为 Solution.Global 的数据载体。
└── Parser.vb     # [MODIFY] 在 Module Parser 中实现 ParseSln(path)、ParseSlnx(path) 与 Parse(path) 自动分派函数，完成文本/XML 到统一模型的映射；内部辅助函数处理 GlobalSection、NestedProjects、配置平台解析。
```

## 关键代码结构（可选）

```
Namespace sln
    Public Class Solution
        Public Property FormatVersion As String
        Public Property VisualStudioVersion As String
        Public Property MinimumVisualStudioVersion As String
        Public Property Projects As Project()
        Public Property Configurations As SolutionConfiguration()
        Public Property Global As [Global]
    End Class

    Public Class Project
        Public Property NodeType As TypeId
        Public Property Guid As String
        Public Property Name As String
        Public Property TreePath As String
        Public Property RelativePath As String
        Public Property FullPath As String
        Public Property ParentGuid As String
    End Class

    Public Class SolutionConfiguration
        Public Property Name As String   ' e.g. "Debug|AnyCPU"
    End Class
End Namespace
```