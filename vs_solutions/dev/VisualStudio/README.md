# Visual Studio 解决方案、VB 项目与源码分析工具箱

## 引言

「读懂一个 .NET 仓库」这件事，在不同深度上有完全不同的难度：

1. **读解决方案** —— `.sln` / `.slnx` 里的项目、文件夹与构建配置；
2. **读项目文件** —— `.vbproj` 里的引用、配置与 NuGet 元数据；
3. **读源码** —— 把 VB.NET 文本解析成命名空间 / 类型 / 成员 / 变量的符号树；
4. **读已编译产物** —— 没有源码时用元数据反射还原符号，甚至把 IL 反编译回语法树；
5. **读调试映射** —— 用 source map 把生成代码的行列位置映射回源文件。

`Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio` 把这五层能力全部实现，是 `sciBASIC#` 代码生成、文档生成与静态分析流水线的地基。

## 设计目标

- **统一符号模型**：源码解析（`VBParser`）与元数据反射（`AssemblySymbolLoader`）产出**同一套**符号树，因此下游分析代码不需要区分「源码来自文件还是程序集」；
- **格式双支持**：解决方案同时支持经典文本 `.sln` 与现代 XML `.slnx`，并按格式分派专用读写器；
- **可回写**：不仅能读，`SlnxWriter` 等还能把模型写回磁盘，支撑代码生成场景。

## 核心特性

- **解决方案解析 / 写出**：经典 `.sln` 与新式 XML `.slnx` 两种格式，含项目树、配置与解决方案工作区模型；
- **VB.NET 源码分析**：扫描器 + 递归下降解析器，产出命名空间 / 类 / 模块 / 成员 / 变量的符号树；`.vbproj` 模型暴露引用、构建配置与 NuGet 元数据；
- **编译产物分析**：只读元数据反射把程序集映射进同一符号模型；IL 读取与反编译器（控制流图、SSA、结构恢复、语法写出）；source map（base64 VLQ）解码；
- **项目工具链**：代码统计、许可横幅插入 / 移除、resx / xsd 模型，以及 git / svn 日志与 diff 解析。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `...VisualStudio`（根） | `WCFTraceFile` 等共享类型 |
| `....sln` / `....sln.File` | 解决方案模型与 `.sln` / `.slnx` 读写器 |
| `....VBProj.Project` / `.ProjectXml` | `.vbproj` 文档模型与其 XML 对象模型 |
| `....VBProj.CodeDOM` / `.CodeDOM.Syntax` | VB 符号树与递归下降解析器 |
| `....VBProj.Reflection` | 程序集符号加载器（元数据反射） |
| `....VBProj.NuGet` | 包、框架与版本模型及解析器 |
| `....IL` / `.IL.Decompiler` / `.IL.Syntax` | IL 读取、反编译与恢复出的语法树 |
| `....SourceMap` | source map 编解码（base64 VLQ） |
| `....Resource` / `.Resource.xml` | `.resx` / `.xsd` 资源模型 |
| `....CodeSign` | 代码统计、许可信息与代码签名 |
| `....VersionControl` / `.VersionControl.Git` | 版本控制集成与 git 日志 / diff |

## 关键类型与 API

- `...sln.Solution` —— 统一解决方案模型，含 `Load`、`Save` 与项目 / 配置查询；
- `...sln.File.Parser` —— 入口解析器，分派给经典 `.sln` 或 `.slnx` 读取器；
- `...sln.File.SlnxWriter` —— 把解决方案模型写回 `.slnx`；
- `...VBProj.Project.VBProject` —— `.vbproj` 文件模型（文档、引用、配置）；
- `...VBProj.CodeDOM.Syntax.VBParser` —— 把 VB.NET 源码文本转换为符号树的递归下降解析器；
- `...VBProj.Reflection.AssemblySymbolLoader` —— 通过元数据反射把编译产物映射进 VB 符号树；
- `...IL.Decompiler.MethodDecompiler` —— 把方法的 IL 体解码为表达式 / 语句语法树；
- `...SourceMap.sourceMap` —— source map 编解码（base64 VLQ 映射）。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.sln
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VBProj.CodeDOM.Syntax

Dim sln As Solution = Solution.Load("D:\repo\MyApp.sln")

For Each p In sln.Projects
    Console.WriteLine(p.Name)
Next
```

## 实现要点

- **源码与反射的符号树对齐**：这是本包最有价值的设计 —— 当需要分析「尚未编译的新代码」与「已发布的旧程序集」之间的差异时，两者可以直接在同一模型上比较。
- **IL 反编译的分层恢复**：从不依赖栈的指令序列出发，依次重建控制流图、SSA 形式、结构化语法，最终写出可读表达式，每一步都是独立可测的模块。
- **source map 的现实意义**：在 SourceLink / 编译期代码生成场景中，source map 是唯一能把运行时行号还原回原始源文件的凭据。

## 与 sciBASIC# 生态的关系

本包为 `Microsoft.VisualBasic.Core` 中的 XML 文档模型（`ApplicationServices.Development.XmlDoc`）提供输入：由源码 / 程序集符号树生成命名空间与成员的文档；同时被仓库自身的代码统计与文档流水线使用。

## 包信息

- Assembly：`Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio`
- TargetFramework：`net10.0`
- Tags：`scibasic;visual-studio;solution-parser;slnx;vbproj;code-analysis;vbnet;il-decompiler;source-map;reflection`
- 许可：GPL-3.0-or-later
