---
name: nuget-slnx-vbproj-sync-script
overview: 编写 vs_solutions 下的 PowerShell 脚本：扫描全仓库 SDK 风格 vbproj，将 RootNamespace 以 Microsoft.VisualBasic 开头的项目补入 nuget.slnx（新建 /packages/new/ 文件夹），并补齐/校正各项目的 nuget_release|x64 配置与指向根目录 .nuget 的 OutputPath。
todos:
  - id: script-skeleton
    content: 创建 vs_solutions/Sync-NuGetSolution.ps1：help、param、XML 保存与缩进辅助函数
    status: completed
  - id: scan-filter
    content: 实现 vbproj 扫描筛选：SDK 风格、RootNamespace 前缀、排除 bin/obj/缓存目录
    status: completed
    dependencies:
      - script-skeleton
  - id: vbproj-fix
    content: 实现工程修复：补 Configurations/Platforms、nuget_release|x64 条件组与 OutputPath 相对路径
    status: completed
    dependencies:
      - scan-filter
  - id: slnx-sync
    content: 实现 nuget.slnx 同步：绝对路径判重、/packages/new/ 文件夹、Project 节点插入并单次保存
    status: completed
    dependencies:
      - scan-filter
  - id: report-verify
    content: 补齐 WhatIf 预览与汇总报告，冒烟验证幂等性与 slnx 可解析，输出运行说明
    status: completed
    dependencies:
      - vbproj-fix
      - slnx-sync
---

## 用户需求

编写一个 PowerShell 脚本（存放于 `vs_solutions` 文件夹），对 sciBASIC# 仓库做一次可重复执行的"NuGet 打包配置体检与修复"，并说明运行方式。

## 功能内容

1. **扫描**：递归扫描仓库内所有 `*.vbproj`（共约 197 个），仅处理 SDK 风格（`<Project Sdk="Microsoft.NET.Sdk">`）且 `RootNamespace` 以 `Microsoft.VisualBasic` 起始的项目；自动跳过 `bin`/`obj`/`packages`/`package-install-cache`/`package-install-SetupFiles`/`.git` 等缓存与输出目录下的工程文件。
2. **解决方案同步**：检查目标项目是否已存在于 `G:\GCModeller\src\runtime\sciBASIC#\nuget.slnx`；不存在则添加，统一放入新建的 `/packages/new/` 解决方案文件夹（便于事后人工归类）。路径判重需按"解析成绝对路径后比较"，避免 `../` 与斜杠差异造成重复添加。
3. **配置组补齐**：检查工程内是否存在 `<PropertyGroup Condition="'$(Configuration)|$(Platform)'=='nuget_release|x64'">`，不存在则新增（含 `PlatformTarget = x64`）。
4. **配置声明补齐**：顶层 `PropertyGroup` 的 `<Configurations>` 若不含 `nuget_release` 则追加，`<Platforms>` 若不含 `x64` 则追加；元素缺失时创建，保证该配置在 VS/MSBuild 中可选。
5. **OutputPath 归正**：将 `nuget_release|x64` 配置组内的 `OutputPath` 强制设为"指向仓库根 `.nuget` 目录的相对路径"（缺失则插入、与计算值不一致则改写），例如 `gr/avi/AVI.NET5.vbproj` → `../../.nuget/`，深度 4 的工程 → `../../../../.nuget/`。

## 安全性与输出效果

- 支持 `-WhatIf` 预览：只逐项打印将要发生的变更，不写任何文件。
- 幂等：已符合要求的项目不产生任何写入；文件内容无实质变化时不落盘。
- 保留原文件的编码（UTF-8 BOM 与否）、换行风格（CRLF/LF）与缩进风格，不扰动无关行。
- 控制台输出彩色分级信息：每个被改动项目一行路径 + 一行操作明细（如 `slnx:added, cfgGroup:added, outputPath:updated`），结尾输出汇总报告：扫描数 / 命中数 / 新增进解决方案数 / 修改工程数 / 各类操作计数 / 跳过与失败数。
- 不生成 `.bak` 备份文件（依赖 git 作为回滚手段）。

## 技术栈

- **Windows PowerShell 5.1**（实测 `$PSVersionTable.PSVersion = 5.1.20348.4163`）：脚本必须兼容 .NET Framework 版 PS，**不可使用** `[System.IO.Path]::GetRelativePath`（.NET Core 2.1+ 才有）、`??`、`-notmatch` 之外的新语法糖。
- **System.Xml.XmlDocument**（`PreserveWhitespace = $true`）做结构化读写，而非正则文本替换——保证 XML 合法性与幂等性。
- 复用仓库已有脚本的成熟实践：`vs_solutions/dev/NuGetMetadata/Apply-NuGetMetadata.ps1` 与 `Export-ProjectInventory.ps1`。

## 实现方案

### 核心策略

单文件、自包含脚本（与仓库现有 ps1 各自内联辅助函数的风格一致，避免引入模块依赖）。流程为"一次扫描 → 逐工程修复（多次小写入）→ slnx 一次性写入"：

1. 加载 `nuget.slnx`，用 `//Project[@Path]` 建立**绝对路径索引**（`[System.IO.Path]::GetFullPath(Join-Path $slnxDir ($path -replace '/','\'))`，键统一小写）。这样可正确识别现有的 `cuda/ILCuda/ILCuda.vbproj`、根级 `Microsoft.VisualBasic.Core/src/Core.vbproj` 以及跳出仓库的 `../../../../Microsoft.VisualBasic.Drawing/...` 三种写法，O(1) 判重，避免 197×N 的字符串比对。
2. `Get-ChildItem -LiteralPath $Root -Filter '*.vbproj' -Recurse -File`，按排除正则过滤（沿用 `Export-ProjectInventory.ps1` 中已验证的 `'(^|[\\/])(obj|bin|\.git|packages)([\\/]|$)'`，再补 `package-install-cache|package-install-SetupFiles`）。注意仓库存在 `mime/text%html`、`mime/application%json` 等含 `%` 的目录名，所有路径操作使用 `-LiteralPath` / `.FullName`，禁止把路径当通配符传入 `-Path`。
3. 每个工程：`DocumentElement.GetAttribute('Sdk') -eq 'Microsoft.NET.Sdk'` 才处理；遍历直接子 `PropertyGroup` 取首个 `RootNamespace`，`StartsWith('Microsoft.VisualBasic')` 才处理（与 `Export-ProjectInventory.ps1` 判定逻辑完全一致）。
4. 修复顺序：`Configurations`/`Platforms` → 条件 `PropertyGroup` → `PlatformTarget`/`OutputPath`；累计操作标记，仅当存在非 `unchanged` 操作时调用保存。
5. slnx 侧：命中且未收录的项目 → 确保 `<Folder Name="/packages/new/">` 存在（不存在则在**最后一个 `<Folder>` 之后**插入，保持现有"Folders 在前、根级 Projects 在后"的排布），在其中追加 `<Project Path="相对正斜杠路径"><Platform Solution="*|x64" Project="x64" /></Project>`。所有变更累积后**只写一次** slnx（176 KB 文件，避免 N 次全文重写）。

### 关键技术决策

- **只补 `<Platform Solution="*|x64" Project="x64" />`，不生成成堆的 `<BuildType>` 映射**：slnx 中现有条目的 `BuildType` 映射是 VS 保存时按各工程实际配置生成的历史产物；新条目只要工程声明了 `nuget_release` 与 `x64`，VS/MSBuild 会自动做同名映射。手写 20 条 BuildType 既冗余又容易写错（如把 `nuget_release` 误映射到 `Release`）。`Platform` 映射保留是因为它与"AnyCPU;x64"双平台工程强相关，且是现有全部条目的一致约定。
- **OutputPath 用"深度 × ../ + .nuget/"计算**：仓库已验证该规律（深度 2 → `../../.nuget/`，深度 5 → `../../../../../.nuget/`），且与 `Export-ProjectInventory.ps1` 里 `('..\' * $depth) + 'vs_solutions\logo-knot.png'` 的既有算法同源；同时规避 PS 5.1 无 `GetRelativePath` 的限制。统一带结尾斜杠，可顺带纠正 `Data_science/Visualization/DataPlot/DataPlot.vbproj:12` 这类 `../../../.nuget`（缺尾斜杠）的不规范值。
- **条件字符串容错匹配**：把 `Condition` 属性做 `-replace '\s',''` 归一化后与 `'$(Configuration)|$(Platform)'=='nuget_release|x64'` 比较，兼容 `== ` 带空格等写法；若同一工程存在多个匹配组（重复定义），全部统一修正，保证行为确定。
- **保留无关配置**：只碰目标条件组，不动 `Rsharp_app_release|x64` 等其他条件组（如 `nlp/KnowledgeGraph/KnowledgeGraph.vbproj:37`），也不删除主 `PropertyGroup` 里已有的无条件 `OutputPath`（条件组会覆盖它），把影响面压到最小。
- **格式保真**：直接复用 `Apply-NuGetMetadata.ps1` 已验证的 `Save-XmlPreserving`（探测 BOM、探测 CRLF/LF、抑制 XmlWriter 自造的 `encoding="utf-16"` 声明并回填原声明、内容一致则返回 `$false` 不写盘）与 `Get-Indents` / `Append-Element`（按前后空白节点推断缩进，兼容 Tab 缩进的 `Core.vbproj`、`gr/Landscape/Landscape.vbproj`）。`nuget.slnx` 首行即 `<Solution>`（无 XML 声明），该函数同样适用。

### 性能与可靠性

- 复杂度 O(N)（N≈197 工程），每个工程一次 XML 解析；slnx 解析 1 次、写 1 次。整体秒级，无热点。
- `$ErrorActionPreference = 'Stop'` + 单工程 `try/catch` 包裹解析与保存：单个坏文件只 `Write-Warning` 并计入 `failed`，不中断全量处理。
- 幂等性由"内容比较后再写"保证，可安全重复运行；`-WhatIf` 走完全部判定逻辑但跳过所有写盘调用。

## 目录结构

```
g:\GCModeller\src\runtime\sciBASIC#\
├── vs_solutions/
│   └── Sync-NuGetSolution.ps1   # [NEW] 唯一新增文件。自包含 PowerShell 脚本：
│                                #   1) 顶部 comment-based help（.SYNOPSIS/.DESCRIPTION/.PARAMETER/.EXAMPLE）
│                                #   2) param 块：$Root=(Split-Path -Parent $PSScriptRoot)、$Solution=(Join-Path $Root 'nuget.slnx')、
│                                #      $SolutionFolder='/packages/new/'、$NuGetDirName='.nuget'、$ProjectFilter='*'、
│                                #      $ExcludePattern=(bin|obj|.git|packages|package-install-cache|package-install-SetupFiles)、
│                                #      [string]$ReportFile（可选 CSV）、[switch]$WhatIf
│                                #   3) 辅助函数（移植自 Apply-NuGetMetadata.ps1）：Test-WhitespaceNode / Get-Indents /
│                                #      Append-Element / Find-ChildElement / Set-Property / Save-XmlPreserving
│                                #   4) 新增函数：Get-ProjectAbsolutePath(slnx 路径归一化)、Get-NuGetOutputPath(深度→../ 序列)、
│                                #      Test-NugetReleaseCondition(Condition 归一化匹配)、Add-SemicolonToken(Configurations/Platforms 追加)、
│                                #      Ensure-NugetReleaseGroup、Ensure-SolutionFolder、Add-SolutionProject
│                                #   5) 主流程 + 每项目明细输出 + SUMMARY 汇总（scanned/matched/slnxAdded/projChanged/
│                                #      cfgGroupAdded/outputPathFixed/configurationsPatched/platformsPatched/skipped/failed）
├── nuget.slnx                   # [MODIFY-运行时] 由脚本写入：新增 <Folder Name="/packages/new/"> 及其下缺失的 <Project Path=...> 条目
└── **/*.vbproj                  # [MODIFY-运行时] 命中项目按需补 <Configurations>/<Platforms>、
                                 #   nuget_release|x64 条件 PropertyGroup、PlatformTarget、OutputPath
```

## 关键数据结构

脚本产出/校验的两种目标形态（作为实现契约）：

vbproj 中的目标条件组（与 `gr/avi/AVI.NET5.vbproj:37-40` 现状一致）：

```xml
<PropertyGroup Condition="'$(Configuration)|$(Platform)'=='nuget_release|x64'">
  <PlatformTarget>x64</PlatformTarget>
  <OutputPath>../../.nuget/</OutputPath>
</PropertyGroup>
```

nuget.slnx 中新增条目的目标形态：

```xml
<Folder Name="/packages/new/">
  <Project Path="gr/avi/AVI.NET5.vbproj">
    <Platform Solution="*|x64" Project="x64" />
  </Project>
</Folder>
```

## 运行方式（脚本交付后写入 help 与回复）

```
# 预览（不写任何文件）
powershell -NoProfile -ExecutionPolicy Bypass -File "G:\GCModeller\src\runtime\sciBASIC#\vs_solutions\Sync-NuGetSolution.ps1" -WhatIf

# 实际执行
powershell -NoProfile -ExecutionPolicy Bypass -File "G:\GCModeller\src\runtime\sciBASIC#\vs_solutions\Sync-NuGetSolution.ps1"

# 仅处理部分工程 / 导出 CSV 明细 / 查看详细日志
... -ProjectFilter '*Imaging*' -ReportFile .\nuget-sync-report.csv -Verbose
```