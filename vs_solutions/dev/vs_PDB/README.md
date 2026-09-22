# PDB 调试符号读取器（经典 MSF 与 Portable 格式）

## 引言

当线上程序崩溃、需要从堆栈里还原出错位置，或者想从已编译的程序集反推源码结构时，**调试符号文件（PDB）** 是唯一的凭据。麻烦之处在于它存在两代截然不同的格式：

- **经典 MSF（Multi-Stream Format）** —— 使用 SuperBlock 魔数、分页索引与多条命名流（DBI / TPI / 符号流），Windows 原生工具链产物；
- **Portable PDB** —— 基于 PE / ECMA-335 元数据（`BSJB` 根），跨平台、体积更小。

`Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.ProgramDatabase` 把两者**统一解码成同一个模型**，调用方无需关心符号来自哪一代格式。

## 设计目标

- **格式自动识别**：`PDB.Open` 依据文件头自动判断容器类型；
- **统一结果模型**：源文档、行号映射、符号与类型记录共用一套 `Models`；
- **路径可复现**：内置 GitHub URL 重定位，把本地构建路径转换成仓库永久链接。

## 核心特性

- `PDB.Open` 中的**格式自动检测**：经典 MSF 容器（SuperBlock 魔数）与 Portable PDB（PE `MZ` 头或独立 `BSJB` 元数据根）；
- **经典 PDB 流**：DBI 流（模块、源文档、行号）、TPI 流（CodeView 类型记录）与公共符号流（`S_PUB32` 等）；
- **Portable PDB**：PE / COR20 / 元数据根遍历、`#Pdb` 自定义流、文档表与序列点；
- **统一结果模型**：带校验和 / 语言 GUID 的源文档、行号映射、符号与类型记录，外加 GitHub URL 重定位辅助。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `...ProgramDatabase`（根） | `PDB` 入口、`MSF` 容器、`PortablePdb`、DBI / TPI / 公共符号读取器、`CodeView`、`Stream` |
| `....Models` | 统一符号模型：`SourceDocument`、`LineInfo`、`Symbol`、`TypeRecord`、`Flags` |

## 关键类型与 API

- `...ProgramDatabase.PDB` —— 统一入口；`Open(path)` 返回文档、行号、符号与类型记录；
- `...ProgramDatabase.MSFReader` —— 经典 MSF 容器及其流的分页读取器；
- `...ProgramDatabase.PortablePdbReader` —— Portable PDB 的 PE / 元数据解码器；
- `...ProgramDatabase.DbiReader` —— DBI 流解析（模块、源文档、行号）；
- `...ProgramDatabase.TpiReader` —— 解码 TPI 流中的 CodeView `LF_*` 类型记录；
- `...ProgramDatabase.PublicSymbolReader` —— 解析经典公共符号流；
- `...ProgramDatabase.Models.SourceDocument` —— 一个被引用的源文件：校验和、语言与可选 GitHub URL。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.ProgramDatabase

Dim pdb As PDB = PDB.Open("D:\build\MyApp.pdb")

For Each doc In pdb.SourceDocuments
    Console.WriteLine(doc.FilePath)
Next

For Each sym In pdb.Symbols
    Console.WriteLine($"{sym.Name} @ {sym.Section}:{sym.Offset}")
Next
```

## 实现要点

- **两代格式共享一套模型**：经典路径需要手工解析分页流与 CodeView 类型表，Portable 路径需要遍历元数据表；把二者的输出归一到 `Models` 之后，上层（堆栈还原、行号映射、文档生成）代码只需写一次。
- **URL 重定位**：CI 构建路径在开发者机器上无效，通过把本地路径改写为仓库永久链接，堆栈信息可以直接贴进 issue 或文档。

## 与 sciBASIC# 生态的关系

本包与 `Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio`（解决方案 / 源码 / IL 分析）配对使用：前者提供「已编译产物 + 调试符号」，后者提供「源码与符号树」，二者共同支撑 `sciBASIC#` 的文档生成与代码分析流水线。

## 包信息

- Assembly：`Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.ProgramDatabase`
- TargetFramework：`net10.0`
- Tags：`scibasic;pdb;debug-symbols;portable-pdb;msf;codeview;source-mapping;stack-trace`
- 许可：GPL-3.0-or-later
