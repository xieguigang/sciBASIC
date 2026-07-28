---
name: sciBASIC-readme-rewrite
overview: "Rewrite the project's README.md (title: \"sciBASIC#: Microsoft VisualBasic for Scientific Computing\") in English as a modern, source-grounded documentation, based on reading the VB.NET source across the framework's main libraries. Cover features, install/build, quick start with short VB.NET examples, and a module/namespace overview for math, machine learning, visualization, data I/O, NLP/GraphQuery/TextRank, ODEs scripting, CLI framework, language extensions, and LLMs."
todos:
  - id: explore-core-runtime
    content: Use [subagent:code-explorer] to map Microsoft.VisualBasic.Core namespaces, language extensions, CLI, app services, LLMs
    status: completed
  - id: explore-data-libs
    content: Use [subagent:code-explorer] to survey Data (DataFrame/IO/GraphQuery/TextRank/netCDF) and mime parsers
    status: completed
  - id: explore-science-graphics
    content: Use [subagent:code-explorer] to survey Data_science (math/ML/mining/viz) and gr Imaging/network/SVG plus www
    status: completed
  - id: verify-build-info
    content: Verify build/install facts from Core.vbproj, nuget.slnx, runtimeconfig; update prerequisites
    status: completed
  - id: rewrite-readme
    content: Rewrite README.md in English with modern structure, namespace table, and short VB.NET examples
    status: completed
    dependencies:
      - explore-core-runtime
      - explore-data-libs
      - explore-science-graphics
      - verify-build-info
  - id: validate-readme
    content: Validate internal links, image paths, and code snippets in the new README
    status: completed
    dependencies:
      - rewrite-readme
---

## 需求概述

围绕标题《sciBASIC#: Microsoft VisualBasic for Scientific Computing》，结合框架实际的 VB.NET 源代码，对项目的 `README.md` 进行整体重写（保持英文）。目标是生成一份现代、结构清晰、内容基于源码、并可运行的开发文档，替换现有约 486 行的旧文档。

## 文档需覆盖的核心内容

- 项目定位：基于 VB.NET 的科学计算运行时，面向 Windows / Linux / macOS（.NET / .NET Framework / mono / 超算平台）的数据科学 CLI 应用开发
- 数学计算、统计与机器学习（Microsoft.VisualBasic.Math / MachineLearning / DataMining / Darwinism 进化算法）
- 科学数据可视化与绘图（Microsoft.VisualBasic.Imaging、ChartPlots、网络图布局、2D/3D 与等距引擎、SVG / d3js）
- 科学数据读写（DataFrame、CSV 与 NetCDF 等 BinaryData、MIME 文本/XML 解析、Excel openxml）
- 自然语言处理（TextRank 关键词抽取、GraphQuery 查询语言与执行引擎）
- 扩展的 VB.NET 语言特性（inline assign、List(Of T) +=、int 区间类型、UnixBash ls、With/ref、类型字符）
- ODEs 动力学系统脚本语言与 VisualBasic CLI 应用框架
- 新增能力（如 LLMs 代理 HookOllama / LLMsTalk 等）
- 安装/构建说明、快速开始、命名空间概览、常见问题与示例

## 技术栈与文档方法

- 文档语言：英文（与现有 README 一致，不翻译、不对译为双语）
- 目标文件：`g:\GCModeller\src\runtime\sciBASIC#\README.md`（整体重写，非增量修补）

## 实现策略

采用「先探索源码、再据实撰写」的方式：先使用 `code-explorer` 子代理对各主库做大规模检索，枚举真实公共命名空间/模块与关键 API，核对构建与安装事实；随后以现代 README 结构重写文档，确保所有描述均可在源码或既有子 README 中得到印证，避免凭空编造。

### 关键决策

- **以源码为准的命名空间表**：现有文档仅列出约 9 个命名空间且部分已过时。计划从 `Microsoft.VisualBasic.Core/src` 各子目录、`Data`、`Data_science`、`gr`、`mime` 的 .vbproj `RootNamespace`、公共 Module/Class 与 `Imports` 声明中重建准确、更完整的命名空间/模块清单，并区分「通用运行时」与「数据科学运行时（带星标）」。
- **核对构建/安装事实**：现有文档提及 VisualStudio 2017、.NET Framework 4.6、mono 与 nuget 包，需核对 `Microsoft.VisualBasic.Core/src/Core.vbproj`、`runtimeconfig.template.json` 及 `nuget.slnx`，更新为准确的 TargetFramework（.NET / .NET Framework）、构建工具链与安装方式。
- **复用既有权威子文档**：`Data/GraphQuery/README.md`、`Data/TextRank/README.md`、`Data/BinaryData/netCDF/README.md`、`gr/network-visualization/README.md`、`docs/guides/*`、`Microsoft.VisualBasic.Core/src/CommandLine/POSIX/README.md` 作为内容来源，避免重复探索并保证一致性。
- **嵌入可运行短示例**：为数学计算、DataFrame 读写、绘图、ML/聚类、GraphQuery、CLI 框架、扩展语言语法等提供简短 VB.NET 片段，示例需与源码中的真实 API/命名空间一致（如 `Imports Microsoft.VisualBasic.Data.ChartPlots`、`Microsoft.VisualBasic.Language`、`Microsoft.VisualBasic.CommandLine`）。
- **保持定位**：保留「面向科学论文的可打印高质量绘图、CLI 而非交互式控件」的定位（来自现有 FAQ）。

## 实现要点（防回归）

- 仅修改 `README.md`；不改动任何源码、配置或子文档。
- 文档中所有相对链接与图片路径需逐一核验存在性，移除/修正失效链接。
- 代码示例须可在对应命名空间下编译，避免使用源码中不存在的方法名。
- 徽章/外链保留原有风格，缺失资源不臆造。

## 推荐文档结构

1. 标题 + 徽章 + 一句话定位
2. Introduction（运行时组成：DataFrame / 数据分析 / 图形 / 通用核心运行时）
3. Features（分点能力概览）
4. Installation & Build（依赖、.NET 目标、nuget、源码构建）
5. Quick Start（最短可运行 VB.NET 示例：CLI 程序骨架 + DataFrame + 绘图）
6. Module & Namespace Overview（基于源码的真实命名空间/模块表）
7. Language Extensions（扩展 VB.NET 语法与示例）
8. Examples by domain（Math / ML / Visualization / Data I/O / NLP / ODEs / LLMs）
9. FAQ
10. Documentation links & Contacts

## Agent Extensions

### SubAgent

- **code-explorer**
- Purpose: 大规模检索各主库的 VB.NET 源文件，枚举真实公共命名空间/模块/关键 API 与扩展语言特性，并核对 `.vbproj`/解决方案中的构建与安装事实
- Expected outcome: 输出各库的能力清单、公共 API 概览、准确的命名空间表，以及 TargetFramework/依赖/构建方式等事实，作为撰写准确 README 的唯一权威依据