# sciBASIC# 运行时核心库

## 引言

如果把 `sciBASIC#` 的上百个包画成一张依赖图，本包就位于图的底部：**其它所有包都建立在它之上**。

它的定位不是「某个功能」，而是**把 .NET 里用得最多、却总缺一点的东西补齐**：

- LINQ 之外还需要的集合与序列操作；
- 反射调用到原生库的一整条通路；
- 文本 / XML / JSON 的统一处理方式；
- 命令行程序的完整框架；
- 并行、终端、日志等应用服务。

## 设计目标

- **广泛复用**：这里的每个模块都被多个上层包依赖，因此接口必须稳定、通用；
- **零第三方依赖**：只用 .NET 基类库，保证任何环境都能加载；
- **面向科学计算**：数学、文本、表格、序列化等能力都按科研数据处理的实际需求设计。

## 核心能力概览

| 领域 | 主要内容 |
|---|---|
| 集合与数据 | LINQ 风格序列扩展、集合辅助（双端队列 / 优先队列）、组件模型（具名值、数据源与仓储、模式映射、设置与 INI 配置）、文件与路径扩展 |
| 文本与序列化 | 文本处理与编码、文本模式与校验、文本解析框架（含 HTML 解析）、字符串相似度（Levenshtein）、文本检索索引、XML 工具箱（文档树 / 元素模型 / 序列化 / LINQ 查询 / Open XML）、JSON 序列化器（含模式提供者与格式化器） |
| 反射与互操作 | 反射辅助、基于 IL emit 的委托工厂、Marshal 辅助、动态原生库加载（Windows / Unix） |
| 命令行 | 特性驱动的反射式 CLI 绑定、POSIX 风格参数、解析器、互操作服务与共享 ORM、解释器与手册页查看器 |
| 并行与应用服务 | 数据管道与流缓冲、内存映射文件协议、任务与线程池、应用与时钟辅助、调试与断言、日志、本地化、插件加载、zip 归档 |
| 终端 | Shell 与控制台抽象、行编辑（历史与补全）、Markdown 与 ANSI 渲染、控制台表格构建器、进度条与 tqdm 风格进度条 |
| 数学与其它 | 数学核心（统计、相关性、信息论、SIMD 向量化）、脚本服务（元数据与分词）、网络客户端（HTTP / TCP / FTP / MIME）、打印与 GDI+ 绘图驱动 |

## 关键类型与 API

- `Microsoft.VisualBasic.ApplicationServices.App` —— 进程 / 程序集 / 主目录上下文、stdout-stderr 重定向与 CPU / 内存信息；
- `...ApplicationServices.Debugging.Logging.LogFile` —— 带缓冲与级别的文本日志，提供 `WriteLine`、`log`、`LogException` 与 `Save`；
- `...CommandLine.CommandLine` —— 分词后的命令行模型与 CLI 解析入口；
- `...ComponentModel.DataSourceModel.NamedValue(Of T)` —— 框架内广泛使用的泛型键值数据模型；
- `...ApplicationServices.DynamicInterop.UnmanagedDll` —— 加载与调用非托管动态库的代理；
- `...ApplicationServices.Terminal.TablePrinter.ConsoleTableBuilder` —— 流式控制台表格构建器；
- `...Text.ASCII` —— 文本与字节缓冲的 ASCII / 编码辅助；
- `...Math.RandomExtensions` —— 随机数生成器与采样辅助。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging

Dim log As New LogFile("./scibasic.log", append:=True)

Call log.WriteLine($"app={App.AssemblyName}, home={App.HOME}, cores={App.CPUCoreNumbers}")
Call log.Save()
```

## 目录与模块地图

| 目录 / 命名空间前缀 | 说明 |
|---|---|
| `ApplicationServices` | 应用服务、调试与断言、日志、动态互操作、本地化、插件、终端与 zip |
| `CommandLine` | CLI 框架、解析器、POSIX、反射绑定与互操作服务 |
| `ComponentModel` | 集合、算法、数据结构、数据源与设置 |
| `Data` / `Extensions` | 数据仓储、反射 / 集合 / IO / 安全 / 值类型扩展 |
| `Drawing` / `Math` / `Net` / `Text` | 绘图、数学、网络与文本处理 |
| `Scripting` / `Serialization` | 脚本服务与 XML / JSON / Bencode / 二进制转储 |
| `Language` | 语言服务（VB / C / Java / Perl / Python / Unix Shell 扩展与向量化） |

## 实现要点

- **为什么运行时核心要自带终端与命令行框架**：`sciBASIC#` 的工具链以命令行程序为主，日志、进度条、表格输出与参数解析是每个工具都要用的能力；放在核心库里可以避免每个工具重复实现。
- **动态互操作而非固定绑定**：`DynamicInterop` 在运行时加载平台相关的原生库，因此同一个程序集可以在 Windows / Linux / macOS 上以不同后端工作，而不需要在编译期区分。
- **序列化能力为何集中在此**：XML 与 JSON 是所有上层包共用的交换格式；把它们放在核心库可以保证行为一致，也避免版本分裂。

## 包信息

- Assembly：`Microsoft.VisualBasic.Runtime`
- RootNamespace：`Microsoft.VisualBasic`
- TargetFramework：`net10.0`
- Tags：`scibasic;runtime;linq;reflection;application-services;commandline;serialization;interop;terminal`
- 许可：GPL-3.0-or-later
