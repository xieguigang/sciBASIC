---
name: nuget-release-output-path-fixer
overview: 为 PkgVersionUpgrade 新增 --fix-output-path 功能：扫描框架内所有 RootNamespace 以 Microsoft.VisualBasic 起始的 SDK 风格 vbproj，将其 nuget_release|x64 编译配置（含带 TargetFramework 的变体）的 OutputPath 统一设为指向 G:\pixelArtist\src\framework\.nuget 的正确相对路径，缺失该配置组的项目自动补建，并补齐 Configurations/Platforms 声明。
todos:
  - id: add-xml-editor
    content: 新增 XmlEditor.vb 共享 XML 编辑原语，并改造 VersionUpgrader、ConfigCleaner 复用它
    status: completed
  - id: add-output-path-fixer
    content: 新增 OutputPathFixer.vb：相对路径计算、条件下标匹配、OutputPath 写入与配置组补建
    status: completed
    dependencies:
      - add-xml-editor
  - id: wire-cli-flag
    content: Program.vb 接入 --fix-output-path 开关、编排调用与汇总统计
    status: completed
    dependencies:
      - add-output-path-fixer
  - id: verify-dry-run
    content: 编译后用 dry-run 校验新功能，并回归确认既有清理功能仍幂等
    status: completed
    dependencies:
      - wire-cli-flag
  - id: apply-and-review
    content: 正式执行 --fix-output-path 并用 git diff 抽样复核写入结果
    status: completed
    dependencies:
      - verify-dry-run
---

## 产品概述

为已有的 `PkgVersionUpgrade` 命令行工具新增一项可选功能：扫描框架源代码文件夹内所有 `RootNamespace` 以 `Microsoft.VisualBasic` 起始的 SDK 风格 vbproj，将其 `nuget_release|x64` 编译配置（含带 `$(TargetFramework)` 的变体形式）的 `<OutputPath>` 统一设置为指向 `G:\pixelArtist\src\framework\.nuget` 的正确相对路径；缺失该配置组的项目自动补建，并补齐 `<Configurations>` / `<Platforms>` 声明，使新配置真正可被 MSBuild 求值。

## 核心功能

### 1. 目标筛选

- 递归扫描框架根目录（排除 `obj`、`bin`、`.git`、`.vs` 等目录）下全部 `*.vbproj`
- 仅处理同时满足两个条件的项目：`Microsoft.NET.Sdk` 风格，且 `<RootNamespace>` 以 `Microsoft.VisualBasic` 起始（大小写不敏感）
- 共命中 91 个，其中 90 个为 SDK 风格、1 个 legacy（`mime\application%rtf\RTF.vbproj`）跳过

### 2. OutputPath 修正

- 正确值 = 从 vbproj 所在目录到 `.nuget` 目录的相对路径，分隔符用 `/`，末尾带 `/`（如 `../../.nuget/`、`../../../.nuget/`）
- 命中范围：所有 `Configuration=nuget_release` 且 `Platform=x64` 的条件属性组，**包含** `'$(Configuration)|$(TargetFramework)|$(Platform)'=='nuget_release|net10.0|x64'` 这类变体形式，共 75 个组（66 纯形式 + 8 个 net10.0 变体 + 1 个 net10.0-windows 变体）
- 组内已有 `<OutputPath>` 则仅在不同时改写（现有 64 个值全部正确，不产生改动）；组内无该元素则补写（1 个：`ODESolver.Extensions.NET5.vbproj`）

### 3. 配置组补建

- 25 个项目完全没有 `nuget_release|x64` 配置组，新建 `<PropertyGroup Condition="'$(Configuration)|$(Platform)'=='nuget_release|x64'">`
- 新建组内容严格为两个元素：`<PlatformTarget>x64</PlatformTarget>` 与 `<OutputPath>`，不触碰 `DebugSymbols` / `DebugType` / `RemoveIntegerChecks` 等调试符号行为
- 新组插入到文件中最后一个 `<PropertyGroup>` 之后

### 4. 配置声明补齐

- 确保主属性组的 `<Configurations>` 含 `nuget_release`（21 个需追加，其中 12 个连元素本身都没有，需新建）
- 确保 `<Platforms>` 含 `x64`（实测 90 个 SDK 目标已全部含 x64，实际 0 处改动，但仍需校验以保证结果自洽）
- 追加时使用 `;` 分隔，保持原有取值顺序

### 5. 命令行

- 新增独立开关 `--fix-output-path`，**默认关闭**；不加该参数时工具行为与之前完全一致
- 支持与 `--dry-run` 组合预览；汇总输出新增一行统计
- `--help` 中补充该开关说明

## 效果预期

- 预览（dry-run）显示：25 个组待创建、约 75 个 OutputPath 待写入、21 个 Configurations 声明待追加
- 正式执行后，全部 `Microsoft.VisualBasic*` SDK 项目在执行 `nuget_release|x64` 构建时均输出到统一的 `.nuget` 目录；已正确的 64 个既有值保持不变
- 重复运行完全幂等（`updated=0, created=0, declarations=0`）

## 技术栈

- 语言/框架：VB.NET，.NET 10（`net10.0`），`Microsoft.NET.Sdk` 风格控制台工程
- XML 处理：`System.Xml.Linq`（`XDocument` / `XElement` / `XAttribute` / `XText`）
- 路径计算：`System.IO.Path.GetRelativePath`（.NET Core 2.1+ / .NET 10 原生可用）
- 复用既有模块：`VBProject.LoadProjectXml`（读取工程模型）、`ApplicationInfoUtils.CalculateVersion`（版本计算，本轮不涉及）
- 工程无需新增引用，SDK 风格自动 glob 收录新增 `*.vb`

## 实施思路

**策略**：新增一个职责单一的 `OutputPathFixer` 模块，沿用上一轮确立的「模型只读、原始 XML 原地外科手术式修改」范式；同时把分散在 `VersionUpgrader` 与 `ConfigCleaner` 中的 XML 缩进/空白处理辅助抽到共享的 `XmlEditor` 模块，避免第三处重复实现。

**核心决策与理由**：

1. **条件解析必须按模板分段动态定位下标**
实测条件模板中 `$(Configuration)` / `$(Platform)` / `$(TargetFramework)` 的**顺序不固定**，存在 `'$(Configuration)|$(Platform)|$(TargetFramework)'=='mzkit_win32|x64|net6.0'` 这类颠倒写法。固定取第 0/1 段会把 `x64` 误判为 Configuration。做法：以 `==` 切分模板与比较值，各自按 `|` 分段，先找到 `$(Configuration)`、`$(Platform)` 在模板中的下标，再取比较值同下标的分段。这样纯形式与 TF 变体形式被同一套逻辑天然覆盖，无需分支。

2. **沿用保真读写，不使用 `VBProject.Generate()`**
`Generate()` 只重建 5 类节点，会静默丢弃 `EmbeddedResource` / `None` / `Content` 等节点与 XML 注释。写回一律用自定义 `XmlWriter`：沿用原文件 BOM 设置（框架内 vbproj 全部为带 BOM 的 UTF-8、无 XML 声明）、`Indent=False`、禁用自动重排，保证 diff 只落在实际改动行。

3. **抽取 `XmlEditor` 而非复制辅助方法**
`VersionUpgrader` 已有 `MainPropertyGroup` / `AddProperty` / `InferChildIndent`，`ConfigCleaner` 已有 `Unquote` / `RemoveWithTrailingWhitespace`。新功能同样需要「推断缩进后插入元素」「改值或新建」「连带空白节点移除」三种能力。复制到第三个文件会造成三份重复的缩进推断逻辑；抽成共享模块后三个模块统一。风险可控——被改的两个文件都是本轮会话中新建并已验证的，改动后用 dry-run 回归确认 `cleaned: 0` 幂等即可。

4. **跳过 legacy 项目**
唯一命中的 legacy 项目 `mime\application%rtf\RTF.vbproj` 不含 `<Platforms>` 元素（该属性是 SDK 专有），补声明无意义。与工具既有行为（只处理 `IsDotNetCoreSDK`）保持一致，并在汇总中体现跳过数。

5. **默认关闭**
该功能会新增 25 个配置组并改写 21 处声明，属于结构性变更，与版本号刷新性质不同。按用户选择设为显式开关，避免默认流程对所有使用者产生意外改动。

## 实施要点（执行细节）

### 条件匹配

```
模板段 = Condition 左侧去引号后按 "|" 拆分
取值段 = Condition 右侧去引号后按 "|" 拆分
配置 = 取值段[IndexOf(模板段, "$(Configuration)")]
平台 = 取值段[IndexOf(模板段, "$(Platform)")]
命中 = 配置 == "nuget_release" 且 平台 == "x64"（OrdinalIgnoreCase）
```

含 `!=` 否定比较或无法定位下标的条件一律判定为不命中（保守跳过）。

### 路径计算

```
ComputeOutputPath(projPath, nugetDir):
    dir = Path.GetDirectoryName(Path.GetFullPath(projPath))
    rel = Path.GetRelativePath(dir, nugetDir)
    return rel.Replace("\"c, "/"c) & "/"
```

注意：Windows PowerShell 5.1 无 `Path.GetRelativePath`，但 VB/.NET 10 有，直接使用。

### 新组插入位置

插入到文档中**最后一个 `<PropertyGroup>` 元素之后**（跟随既有配置组，而不是插到主属性组内部）；采用 `XmlEditor` 的缩进推断，沿用文件既有 Tab/Space 风格，并在新组前后补齐空白文本节点。

### 声明追加

- 定位主属性组（第一个无 `Condition` 的 `<PropertyGroup>`）
- `<Configurations>` 存在但不含 `nuget_release` → 值末尾追加 `;nuget_release`；元素不存在 → 新建
- `<Platforms>` 同理追加 `x64`

### 兼容性与影响面

- 全部改动限于 `PkgVersionUpgrade` 目录内；不修改 `VBProject.vb`、`ApplicationInfoUtils.vb` 或任何被扫描项目的逻辑代码
- 不加 `--fix-output-path` 时，工具行为与上一轮完全一致（版本升级 + 条件清理）
- 单文件处理包在 `Try/Catch` 内，失败只记 error 并继续，不中断整批
- 日志仅输出到控制台，不写入框架日志目录

## 架构设计

```mermaid
flowchart TD
    A[Program.Main 解析 CLI] --> B{--fix-output-path ?}
    B -- 否 --> C[仅版本号升级 + 条件清理]
    B -- 是 --> D[额外执行输出路径修正]
    C --> E[扫描 vbproj]
    D --> E
    E --> F[VBProject.LoadProjectXml 读模型]
    F --> G{IsDotNetCoreSDK 且 RootNamespace 以 Microsoft.VisualBasic 起始 ?}
    G -- 否 --> X[跳过]
    G -- 是 --> H[OutputPathFixer.Apply]
    H --> I[计算相对路径]
    I --> J[遍历条件组写入 OutputPath]
    J --> K{命中数为 0 ?}
    K -- 是 --> L[新建 nuget_release|x64 PropertyGroup]
    K -- 否 --> M[补齐声明]
    L --> M
    M --> N[Configurations 追加 nuget_release / Platforms 追加 x64]
    N --> O{changed ?}
    O -- 是 --> P[XDocument 保真写回]
    O -- 否 --> Q[统计]
    P --> Q
    Q --> R[汇总输出]
```

模块职责（单一职责）：

- `OutputPathFixer` —— 纯输出路径逻辑，不关心版本号与条件清理
- `XmlEditor` —— 纯 XML 原地编辑原语，不含任何业务语义
- `VersionUpgrader` / `ConfigCleaner` —— 既有职责不变，仅把 XML 原语改为调用 `XmlEditor`
- `Program` —— CLI、编排、汇总

## 目录结构

```
g:\pixelArtist\src\framework\vs_solutions\PkgVersionUpgrade\
├── PkgVersionUpgrade.vbproj   # [不改] SDK 风格自动 glob 收录 *.vb，引用已足够
├── XmlEditor.vb               # [新增] 共享 XML 原地编辑原语：
│                              #        MainPropertyGroup(doc, ns) 取/建主属性组
│                              #        InferChildIndent(parent) 推断子元素缩进，兼容 Tab/Space 混用
│                              #        AddElement(parent, ns, name, value) 保持缩进追加元素
│                              #        SetOrCreateElement(parent, ns, name, value) 改值或新建，返回是否变更
│                              #        RemoveNode(element) 连带后续空白文本节点一起移除
│                              #        Unquote(text) 去掉 Condition 两侧引号
├── OutputPathFixer.vb         # [新增] 输出路径修正：
│                              #        IsTarget(model) 判定 SDK + RootNamespace 前缀
│                              #        ComputeOutputPath(projPath, nugetDir) 计算 ../../.nuget/ 形式相对路径
│                              #        IsNugetReleaseX64(condition) 按模板下标匹配 Configuration/Platform
│                              #        Apply(doc, ns, projPath, nugetDir) 写入/补建/补齐声明，返回统计
├── VersionUpgrader.vb         # [改] 删除 MainPropertyGroup / AddProperty / InferChildIndent 私有副本，改为调用 XmlEditor
├── ConfigCleaner.vb           # [改] 删除 Unquote / RemoveWithTrailingWhitespace 私有副本，改为调用 XmlEditor
└── Program.vb                 # [改] CliOptions 增加 FixOutputPath 字段；ParseCommandLine 识别 --fix-output-path；
                               #        ProcessProject 中按开关调用 OutputPathFixer.Apply；
                               #        ProjectResult 增加输出路径统计字段；PrintSummary 增加统计行；
                               #        PrintUsage 补充开关说明
```

## 关键代码结构

新模块的核心契约（接口级，实现时按此签名）：

```
Namespace PkgVersionUpgrade

    ''' <summary>单个项目的输出路径修正结果</summary>
    Public Class OutputPathResult
        ''' <summary>被改写或补写 OutputPath 的条件组数量</summary>
        Public Property Updated As Integer
        ''' <summary>新建的 nuget_release|x64 属性组数量（0 或 1）</summary>
        Public Property Created As Integer
        ''' <summary>补齐的 Configurations / Platforms 声明条数</summary>
        Public Property DeclarationsAdded As Integer

        Public ReadOnly Property Changed As Boolean
    End Class

    ''' <summary>nuget_release|x64 输出路径修正器</summary>
    Public Module OutputPathFixer

        ''' <summary>SDK 风格且 RootNamespace 以 Microsoft.VisualBasic 起始</summary>
        Public Function IsTarget(model As VBProject) As Boolean

        ''' <summary>计算从项目目录到 .nuget 目录的相对路径（正斜杠 + 结尾斜杠）</summary>
        Public Function ComputeOutputPath(projectPath As String, nugetDir As String) As String

        ''' <summary>
        ''' 判定条件是否为 Configuration=nuget_release 且 Platform=x64。
        ''' 按模板分段动态定位下标，同时覆盖纯形式与带 $(TargetFramework) 的变体形式。
        ''' 无法解析时返回 False（调用方保守跳过）。
        ''' </summary>
        Public Function IsNugetReleaseX64(condition As String) As Boolean

        ''' <summary>就地写入 OutputPath；无命中组时补建；最后补齐 Configurations/Platforms 声明</summary>
        Public Function Apply(doc As XDocument,
                              ns As XNamespace,
                              projectPath As String,
                              nugetDir As String) As OutputPathResult
    End Module
End Namespace
```

注意：工程 `RootNamespace` 已是 `PkgVersionUpgrade`，源码中**不要再声明** `Namespace PkgVersionUpgrade`，否则完整名会变成 `PkgVersionUpgrade.PkgVersionUpgrade`。