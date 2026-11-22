# Microsoft.VisualBasic.Runtime

**sciBASIC# 运行时核心库**（本文件是随 NuGet 包发布的说明文档）。
如果把 sciBASIC# 的上百个包画成一张依赖图，本包位于图的最底部：
所有上层包（GCModeller、mzkit、R# 等）都直接或间接建立于其上。

| 项目 | 值 |
|---|---|
| 程序集 | `Microsoft.VisualBasic.Runtime` |
| 根命名空间 | `Microsoft.VisualBasic` |
| 目标框架 | `net10.0` |
| 源码规模 | 约 1085 个 VB.NET 源文件 |
| 运行时标识 | `win-x64` / `linux-x64` / `linux-arm64` / `osx-x64` |
| 版本 | `10.8.9818.14504`（AssemblyVersion `5.33.9818.14504`） |
| 许可 | GPL-3.0-or-later |

## 这个库解决什么问题

它不是一个「功能包」，而是把 .NET 基线库里**用得最多、却总缺一点**的部分补齐，
并且保证这些补齐在各个平台上行为一致：

- LINQ 之外还需要的集合、序列、区间与数据结构；
- 从反射调用一路打通到原生库（P/Invoke 动态加载）的整条通路；
- JSON / XML / 二进制 / Bencode 的统一读写方式；
- 命令行程序的完整框架（参数绑定、帮助页、管道、互操作服务）；
- 终端交互、进度条、表格与 Markdown 渲染；
- 并行、进程间通信、日志与调试审计等应用服务；
- 面向科研数据的统计、SIMD 向量化数学与带行列名的数值表。

## 设计原则

- **广泛复用，接口稳定**：这里的每个模块都被多个上层包依赖，破坏性改动的成本极高。
- **贴近 BCL**：除 `System.Composition` 与 `System.Resources.Extensions` 之外不引入任何第三方依赖。
- **跨平台一致**：Windows / Linux / macOS 共用同一套 API，平台差异在模块内部消化
  （路径映射、动态库加载器、POSIX 终端、托管 GDI+ 兼容层）。
- **面向数据处理**：`NumericTable`、`TextIndexing`、`JsonContract` 等能力都按科研数据的实际形态设计，
  而不是照搬通用库的抽象。

## 能力总览

| 领域 | 主要内容 |
|---|---|
| 命令行 | 特性（`<ExportAPI>`）驱动的反射式绑定、POSIX 风格参数、参数对象映射、帮助/手册页、子命令管道、跨语言绑定生成 |
| 终端 | `IConsole`/`Shell` 抽象、行编辑与补全、历史栈、ANSI 颜色、Markdown 渲染、控制台表格、进度条（含 tqdm 风格） |
| 集合与数据结构 | 双端队列、优先队列/配对堆、二叉堆、多重值字典、位集、环形缓冲、八叉树、树结构、区间模型 |
| 文本 | 编码探测与 GB2312 码表、分隔文本（CSV/TSV）解析、词法/格式化解析框架、HTML 标签与表格提取、正则与通配符、Levenshtein 相似度、全文检索索引 |
| 序列化 | JSON（含 pretty-print 与 schema provider）、XML 工具箱（构建/迭代/LINQ/OpenXML）、Bencode、网络字节序缓冲、结构体与对象二进制转储 |
| 数学 | 统计量与样本、相关系数与距离、信息论指标、矩阵、SIMD 内核与向量化数学、随机采样、数值表示与区间归一化 |
| 数据 | `NumericTable` 数值表与二进制存储、TSV 映射、WAL + JSONL 文本仓储、哈希指纹、类型转换器 |
| 反射与互操作 | 反射辅助、IL emit 委托工厂、指针/Span 封装、非托管动态库加载（Windows / Unix 双实现） |
| 并发 | 线程池与任务队列、Actor 模型、数据管道（含 zip 管道）、并行 LINQ 调度、内存映射文件通信与进程锁 |
| 网络 | HTTP 客户端与请求构建、RPC/JSON-RPC、TCP/IPv4 工具、FTP 客户端、MIME、下载器 |
| 图像 | 位图/GIF/JPEG/PNG 编解码、颜色模型、几何与栅格化、GDI+ 抽象驱动、纯托管 GDI+ 兼容层 |
| 脚本与语言兼容 | 代码生成（VB/Shell）、词法与表达式模型、脚本元数据、模拟 C/C++/Java/Perl/Python/Unix Bash 的 API 语义 |
| 应用服务 | 日志与异常日志、断言与调试审计、插件加载、zip/tar.gz 归档、本地化资源、工程与 XML 文档模型 |

## 模块地图

源码按目录组织，**目录名与命名空间并不总是一致**（下表以实际命名空间为准；
源码中省略 `Microsoft.VisualBasic.` 前缀，由 `RootNamespace` 自动补全）。

| 目录 | 命名空间 | 内容 |
|---|---|---|
| `ApplicationServices/` | `...ApplicationServices` | `Application`、`Clock`、调试与日志、动态互操作、文件系统抽象、终端、报告渲染、插件、zip、工程模型（`App` 模块位于根命名空间） |
| `ApplicationServices/Parallel/` | `...Parallel(.Tasks/.Threading/.MMFProtocol)` | 线程池、任务队列、Actor、数据管道、内存映射通信、并行 LINQ 调度 |
| `CommandLine/` | `...CommandLine(.Parsers/.POSIX/.Reflection/.InteropService/.ManView)` | CLI 框架、解析器、反射入口、帮助页生成、互操作服务与共享 ORM |
| `ComponentModel/` | `...ComponentModel(.Collection/.DataStructures/.DataSourceModel/...)` | 具名值、集合与数据结构、算法、区间、数据源与仓储、ORM 映射、配置与 INI |
| `Data/` | `...Data`、`...ComponentModel.DataSourceModel` | `NumericTable`、TSV 映射、文本/JSONL 仓储与 WAL、哈希、类型转换、文本分析 |
| `Extensions/` | `...`、`...Linq`、`...FileIO`、`...SecurityString`、`...ValueTypes` | 全库扩展方法：LINQ、集合、字符串、IO/路径、反射与 emit、加密、HTTP |
| `Language/` | `...Language(.C/.Java/.Perl/.Python/.UnixBash/.Vectorization)` | 语言兼容与语法糖：`Value(Of T)`、`LinqAPI`、R 风格向量、各语言 API 仿真 |
| `Math/` | `...Math(.Correlations/.Information/.SIMD/.Statistics/.Parallel)` | 统计、相关性、信息论、矩阵、SIMD 引擎、随机与三角 |
| `Text/` | `...Text(.Parser/.Patterns/.Similarity/.Xml/.Search)` | 编码、解析器、HTML、XML 工具箱、相似度、检索索引 |
| `Serialization/` | `...Serialization(.JSON/.Bencoding/.BinaryDumping)` | JSON、Bencode、二进制转储、LZW 流 |
| `Net/` | `...Net(.Http/.Tcp/.FTP/.WebClient/.Protocols)` | HTTP、JSON-RPC、TCP、FTP 客户端、MIME、下载器 |
| `Drawing/` | `...Imaging` | 颜色、位图编解码、GDI+ 抽象、几何、纯托管 GDI+ 兼容层 |
| `Scripting/` | `...Scripting` | 代码生成器、分词、表达式、脚本元数据、脚本运行时 |
| `Printing/` | `...Printing` | 打印机选择与打印文档抽象、R 风格格式化 |
| `My/` | `...My.*` | 框架内部服务：句柄表、控制台日志、线程队列、单例、平台路径映射、框架配置、JS 互操作 |
| 根文件 | `Microsoft.VisualBasic` | `App`、`Globals`、`LICENSE`、`LLMs`、`NamespaceDoc` |

## 快速上手

### 1. 一个命令行程序

`<ExportAPI>` 标注的公共方法会被反射注册为子命令，参数按名称自动绑定；
XML 文档注释会直接变成 `--help` 的帮助文本。

```vbnet
Imports Microsoft.VisualBasic.CommandLine.Reflection

Module Program

    ''' <summary>
    ''' the sum of two numbers
    ''' </summary>
    ''' <param name="a">the first number</param>
    ''' <param name="b">the second number</param>
    <ExportAPI("--sum")>
    Public Function SumOver(a As Double, b As Double) As Integer
        Call Console.WriteLine(a + b)
        Return 0
    End Function

    Public Function Main(args As String()) As Integer
        Return GetType(Program).RunCLI(App.CommandLine, AddressOf onEmptyArgs)
    End Function

    Private Function onEmptyArgs() As Integer
        Call Console.WriteLine("try: app --sum <a> <b>")
        Return 0
    End Function
End Module
```

```bash
app --sum 1 2
app --help
```

### 2. 日志与调试审计

```vbnet
Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging

Using log As New LogFile("./app.log", append:=True)
    Call log.WriteLine($"pid={App.PID}, cores={App.CPUCoreNumbers}, home={App.HOME}")
    Call log.info("pipeline started")
    Call log.Save()
End Using

' 断言失败时只记录事件（可切换为 fail-fast），成功路径上零分配
Call DebugAudit.Check(App.CPUCoreNumbers > 0, "cpu cores detected")
Call DebugAudit.Capture(App.MemoryLoad)
Call DebugAudit.SaveReport("./audit-report.txt")
```

### 3. 控制台表格

```vbnet
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.TablePrinter.Flags

Dim table = ConsoleTableBuilder _
    .From(rows) _
    .WithFormat(ConsoleTableBuilderFormat.MarkDown)

Call Console.WriteLine(table.Export.ToString)
```

### 4. JSON 与数值表

```vbnet
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Serialization.JSON

Dim json$ = JsonContract.GetJson(result, indent:=True)
Dim model = JsonContract.LoadJSON(Of AnalysisResult)(json)

' 行主序特征矩阵 + 行名 + 列名，可直接做切片、分组与标签管理
Dim table As New NumericTable(features, rowNames, featureNames)
Dim subset = table.Slice(rows)
```

## 关键类型速查

| 类型 | 命名空间 | 说明 |
|---|---|---|
| `App` | `Microsoft.VisualBasic` | 进程/程序集/目录上下文、标准流、CPU 与内存信息、`RunCLI` 入口 |
| `CommandLine` | `...CommandLine` | 分词后的参数容器（`-name value` / `--name=value` / `/name`） |
| `APIDelegate` | `...CommandLine.Reflection.EntryPoints` | 由 `<ExportAPI>` 生成的反射调用入口 |
| `InteropService` | `...CommandLine.InteropService` | 把 .NET API 包装为 CLI 子命令并可导出多语言绑定 |
| `LogFile` | `...ApplicationServices.Debugging.Logging` | 带缓冲与级别的文本日志，可回读（`LogReader`） |
| `DebugAudit` | `...ApplicationServices.Debugging` | 断言、变量捕获、快照、计时、标记与报告导出 |
| `UnmanagedDll` | `...ApplicationServices.DynamicInterop` | 非托管动态库加载与委托调用 |
| `ConsoleTableBuilder` | `...ApplicationServices.Terminal.TablePrinter` | 流式控制台表格构建器 |
| `MarkdownRender` | `...ApplicationServices.Terminal` | 终端 Markdown 渲染（含主题与表格） |
| `NamedValue(Of T)` | `...ComponentModel` | 全库通用的泛型具名值模型 |
| `PriorityQueue` / `Deque` | `...ComponentModel.Collection(.Deque)` | 优先队列、配对堆、双端队列 |
| `Repository(Of T)` | `...ComponentModel.DataSourceModel.Repository` | 通用数据仓储与全文检索索引 |
| `JsonContract` | `...Serialization.JSON` | JSON 序列化/反序列化入口 |
| `BencodeDecoder` | `...Serialization.Bencoding` | Bencode 解码与元素模型 |
| `NumericTable` | `...Data` | 带行列名的数值表（实现 `INumericMatrix`/`ILabeledMatrix`） |
| `TextIndexing` | `...Text.Search` | 文本检索索引与分段 |
| `XmlBuilder` / `XmlDoc` | `...Text.Xml(.Models)` | XML/HTML 构建与文档模型 |
| `SimdEngine` | `...Math.SIMD` | SIMD 内核（算术、归约、直方图、距离、比较） |
| `FtpClient` | `...Net.FTP` | 完整 FTP 客户端（含断点与进度） |
| `BitmapBuffer` | `...Imaging.BitmapImage` | 位图缓冲区与 GIF/JPEG/PNG 编解码 |
| `PipelineExtensions` | `...Linq` | `DoCall` / `PipeOf` 链式管道 |

## 约定与注意事项

- **命名空间前缀**：源码中的 `Namespace Data` / `Namespace Serialization.JSON` 等省略了
  `Microsoft.VisualBasic.`，实际类型全名需要补全前缀；`Drawing` 目录的命名空间是 `Imaging`（不是 `Drawing`）。
- **XML 工具箱在 `Text.Xml`，不在 `Serialization`**；`Serialization` 负责 JSON、Bencode 与二进制。
- **没有通用的 `DataFrame` 类型**：表格载体是 `Data.NumericTable`；`ComponentModel` 里的
  `DataFrameIO`/`DataFrameColumnAttribute` 只提供列特性映射的基类。
- **`Scripting` 不是 R 服务**：它是代码生成/分词/元数据的工具集；R 相关能力分散在
  `My.RlangInterop`（输出转义处理）、`Printing.RFormat`（R 风格格式化）与 `CommandLine.InteropService.SharedORM.Languages.R`。
- **构建与自检**：`dotnet build src/Core.vbproj`；`FRAMEWORD_CORE` 标记核心运行时构建，`UNIX` 控制平台条件编译；
  `test/` 与 `_smoketest/` 是控制台自检工程（`dotnet run --project test/test.vbproj -- --simd` 等），
  断言与报告由 `DebugAudit` 记录。

## 源码与反馈

- 仓库：<https://github.com/xieguigang/sciBASIC>
- 项目主页：<http://scibasic.net/>
- 本包的 GitHub 首页说明与本文内容一致，仓库根目录 `README.md` 另含构建/打包与测试的完整说明。

## 许可

GPL-3.0-or-later，见包内 `LICENSE` 或 `Microsoft.VisualBasic.LICENSE.GPL3`。
