---
name: namespace-doc-generation
overview: 为 sciBASIC# 仓库中 95 个 RootNamespace 以 "Microsoft.VisualBasic" 为前缀的 vbproj 项目，按命名空间补充或更新 NamespaceDoc.vb 文件，并在其中的 NamespaceDoc 类型上添加英文 XML summary 注释；按顶层目录分批交付。
todos:
  - id: build-namespace-index
    content: 用 [subagent:code-explorer] 扫描 95 个匹配项目，建立命名空间到文件夹及现有文档的映射清单
    status: completed
  - id: batch-small-dirs
    content: 完成 nlp、www、cuda 批次（6 个项目），生成并校验 NamespaceDoc，确定模板风格
    status: completed
    dependencies:
      - build-namespace-index
  - id: batch-vs-solutions
    content: 完成 vs_solutions 批次（2 个项目）的 NamespaceDoc 生成与更新
    status: completed
    dependencies:
      - batch-small-dirs
  - id: batch-mime
    content: 完成 mime 批次（10 个项目）的 NamespaceDoc 生成与更新
    status: completed
    dependencies:
      - batch-vs-solutions
  - id: batch-data
    content: 完成 Data 批次（13 个项目）的 NamespaceDoc 生成与更新
    status: completed
    dependencies:
      - batch-mime
  - id: batch-gr
    content: 完成 gr 批次（10 个项目）的 NamespaceDoc 生成与更新
    status: completed
    dependencies:
      - batch-data
  - id: batch-core
    content: 用 [skill:lsp-code-analysis] 完成 Core 批次（约 141 个命名空间）的 NamespaceDoc 生成与更新
    status: completed
    dependencies:
      - batch-gr
  - id: batch-data-science
    content: 完成 Data_science 批次（53 个项目）的 NamespaceDoc 生成与更新并整体校验
    status: completed
    dependencies:
      - batch-core
---

## 需求概述

对上一轮已筛选出的 **95 个** `RootNamespace` 以 `Microsoft.VisualBasic` 开头的 `.vbproj` 项目，依据其 VB 源码内容，更新并完善 NuGet 程序包描述性元数据：`Title`、`Description`、`PackageTags`、`PackageReleaseNotes`、`PackageReadmeFile`（及其配套 README 文件与打包项）。既有元数据一并评估、补全或改写。

## 核心功能

- **Title（英文）**：约 40–70 字符的项目短标题，概括项目核心用途；已存在的评估后保留或改写。
- **Description（英文）**：约 **250 字符**摘要性描述，说明该代码库的定位与关键技术点。
- **PackageTags（英文）**：分号分隔小写标签，以 `scibasic` 开头，补足 5–8 个技术关键词。
- **PackageReleaseNotes（英文，内联）**：**500–1200 字符**的功能详述，逐项说明项目提供的核心能力、覆盖的算法/文件格式/模块，以及与 sciBASIC# 生态中其他包的关系。
- **PackageReadmeFile 指向的 README.md（简体中文博客体）**：以博客文章形式详述项目代码内容，保留原有技术细节（关键类型、快速上手代码示例等），并补充背景、设计目标、架构与命名空间地图、用例、生态关系、性能要点、版本与许可证等章节。

## 补齐与修正项

- **新增 Title**：`ILCudaTensor`、`ML_SHAP`、`mHG`。
- **新增 Description**：上述 3 个 + `SNN`。
- **新增 PackageTags**：`ILCudaTensor`、`ML_SHAP`、`mHG`、`RTF`。
- **新增 PackageReadmeFile**：`ILCudaTensor`、`ML_SHAP`、`SNN`、`mHG`、`RTF`。
- **新增 README 打包项**（缺失则 README 不会进包）：`ILCudaTensor`、`ML_SHAP`、`SNN`、`mHG`、`RTF`、`Sundials.CVODE`。
- **新建 README.md 文件**：`ILCudaTensor`、`ML_SHAP`、`Sundials.CVODE`（后者当前已声明 `PackageReadmeFile` 但文件不存在，打包会失败）。
- **扩写已有 PackageReleaseNotes**：`FeatherFormat`、`dataframework-netcore5`、`GraphQuery.NET5`。

## 视觉与文本效果

产出为 XML 元数据文本与中文 Markdown 长文，无界面。README 以 Markdown 标题层级、要点列表、`vbnet` 代码块、表格组织，呈现为可读性强的技术博客文章。

## 技术栈

- **目标载体**：VB.NET SDK 风格 `.vbproj`（MSBuild / NuGet 打包元数据），项目内已启用 `GeneratePackageOnBuild`。
- **编辑方式**：对 `.vbproj` 采用**定向文本编辑**（就地替换/插入元数据元素），不使用 XML 反序列化后整体重写，避免破坏既有缩进、元素顺序、`<!-- 中文注释 -->` 与转义写法。
- **分析手段**：PowerShell + `Select-String` 正则做项目/元数据盘点与校验（`Get-Content` 必须显式 `-Encoding`）；语义结构分析用 LSP 能力，失败回退正则。
- **文本产出**：README.md 均以 **UTF-8** 写入（中文内容，不引入 GBK）；`vbproj` 内新增/改写文本中的 `&`、`<`、`>` 必须 XML 转义。
- 不新增依赖、不改动构建配置（仅允许补齐 `PackageReadmeFile` 元素与 README 打包项）。

## 实现方案

1. **素材采集（每项目）**：读取 `<RootNamespace>`、`<AssemblyName>`、现有 `<Title>/<Description>/<PackageTags>`、`<TargetFrameworks>`、`<ProjectReference>`；复用上一轮产出的 **614 个 `NamespaceDoc.vb`**（命名空间→英文功能摘要）作为权威功能素材，并用 `[subagent:code-explorer]` 与 `[skill:lsp-code-analysis]` 补充关键类型/公共 API 结构，用于 README 的“关键类型”与“快速上手”章节。
2. **文案撰写**：按统一模板产出 Title（40–70 字符）、Description（约 250 字符，建议 200–300）、PackageTags（`scibasic` 起头，5–8 个）、PackageReleaseNotes（500–1200 字符，纯文本、无 Markdown 语法、必要时转义）。四者信息层级递进且不重复：Title 定位 → Description 摘要 → ReleaseNotes 功能详述 → README 全貌长文。
3. **元数据落位**：在首个 `<PropertyGroup>` 内按现有顺序就地替换或插入元素；缺失的 `<PackageReadmeFile>README.md</PackageReadmeFile>` 插入到 `<PackageLicenseExpression>` 之后；`<PackageReleaseNotes>` 紧随 `<Description>` 之后插入。
4. **README 改造（保守扩写）**：保留原文全部技术内容（Key Types、Quick Start 代码块、Package/License 等），按中文博客体重排为：标题 → 引言（背景与要解决的问题）→ 设计目标 → 核心特性 → 架构与命名空间地图 → 关键类型与 API → 快速上手（`vbnet` 代码）→ 与 sciBASIC# 生态的关系（依赖/被依赖）→ 性能与实现要点 → 版本与许可证。对 5 个大文件（`netCDF` 35KB、`SNN` 32KB、`ILCuda` 19KB、`Gibbs` 14.6KB、`HMM` 8KB）**严禁删减原有内容**，仅重排结构、翻译/补充中文叙述章节。
5. **打包配置修正**：确保 `<PackageReadmeFile>README.md</PackageReadmeFile>` 存在，且末尾 `<ItemGroup>` 中存在 `<None Include="README.md"><Pack>True</Pack><PackagePath>\</PackagePath></None>`，且 `README.md` 文件真实存在。
6. **校验**：用 `[xml]` 解析每个 `vbproj` 确认合法；校验 Description 与 ReleaseNotes 长度区间；校验 `PackageReadmeFile` 指向的文件存在；确认 README 打包项覆盖 95/95。

### 关键技术决策

- **定向文本编辑而非 XML 重写**：`vbproj` 含大量手写注释、非常规缩进与 `<Configurations>` / `<Compile Remove>` / `<EmbeddedResource>` 等关键配置，序列化重写会引入大面积 diff 且可能清空注释。文本替换可把 blast radius 限制在 4–5 行内。
- **保留 + 重排 + 中文扩写**：README 是包内唯一长文档，全量重写对大文件会丢信息；保留原有 API/示例、外层补博客式叙述章节，兼顾信息保全与风格要求。
- **文案与上一轮 NamespaceDoc 复用**：避免重复解析源码，保证 Description/ReleaseNotes/README 与代码实际命名空间功能一致，降低幻觉风险。
- **转义与编码前置约束**：`<Authors>xieguigang &lt;I@xieguigang.me&gt;</Authors>` 证明文件内含转义；中文 README 以 UTF-8 写入，避免 NuGet 打包后乱码。

### 性能与可靠性

- 盘点与校验为 O(项目数 × 文件数) 的单次遍历，全仓库约 5380 个 `.vb` 文件，可接受。
- 按目录分批，批次间无共享状态，任一项目异常可局部重做，不影响其他批次。

## 实施注意事项

- **长度度量口径**：Description 与 PackageReleaseNotes 长度按元素内纯文本字符数（不含标签与转义实体展开差异）估算，Description 落在 200–300，ReleaseNotes 落在 500–1200。
- **禁止改动项**：`<RootNamespace>`、`<TargetFrameworks>`、`<Configurations>`、`<Compile Remove>`、`<EmbeddedResource>`、`<ProjectReference>`、`<Version>` / `<AssemblyVersion>`、`<PackageIcon>` 等一律不动。
- **ReleaseNotes 文本形态**：纯文本单行（与既有 3 个项目写法一致），不使用 Markdown 语法，避免在 NuGet 门户显示错乱。
- **README 语言一致性**：正文中文，但代码块、类型全名、命令行、标签保持英文原样。
- **编码安全**：写入前对目标文件确认 UTF-8；禁止用未指定 `-Encoding` 的 `Get-Content` 参与文本替换。
- **范围界定**：仅修改本 95 个项目，`vs_solutions/dev/LicenseMgr` 等无 `RootNamespace` 前缀的项目不纳入。
- **不新增依赖、不改动构建配置**（仅 README 打包项与 `PackageReadmeFile` 的补齐属例外）。

## 目录结构（影响范围）

改动分布在工作区 95 个项目目录内，每个项目预计修改 1 个 `.vbproj` + 1 个 `README.md`，另新建 3 个 `README.md`（约 193 个文件）。按批次汇总如下：

sciBASIC#/
├── cuda/
│   ├── ILCuda/README.md                                  # [MODIFY] 中文博客化扩写；ILCuda.vbproj 元数据完善
│   ├── ILCuda/ILCuda.vbproj                              # [MODIFY] Title/Description/Tags/ReleaseNotes 完善
│   └── ILCudaTensor/                                     # [MODIFY] 缺口项目：新增 Title/Description/Tags/ReleaseNotes/PackageReadmeFile + README 打包项
│       ├── ILCudaTensor.vbproj                           # [MODIFY] 补齐全部包元数据与打包项
│       └── README.md                                     # [NEW] 首建中文博客体 README（项目原本无 README）
├── Data/
│   ├── BinaryData/{BinaryData,DataStorage