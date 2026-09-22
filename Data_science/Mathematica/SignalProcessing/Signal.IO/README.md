# 信号数据 IO：netCDF 读写

## 引言

信号数据的持久化比普通表格更复杂，因为除了采样值本身，还必须保留：

- **采样率 / 时间轴**；
- **物理单位**（mV、nm、吸光度…）；
- **分段信息**（一段长信号常被切成多个 chunk，各自可能对应不同实验条件）。

如果这些元数据在保存时丢失，「读回来的数据还是不是原来的数据」就无从保证。本包的做法是：**把信号集合及其全部元数据一起写入 netCDF 文件**。

## 设计目标

- **元数据不丢失**：信号元数据、chunk 索引与测量单位都随数据一起保存；
- **与 R / Python 互通**：netCDF 是跨语言标准容器，R 侧的分析流程可以直接读取；
- **面向"信号集合"**：不只支持单条信号，而是支持一批信号的整体读写。

## 核心能力

| 能力 | 说明 |
|---|---|
| **信号集合读写** | 读 / 写一批时间序列信号到 netCDF 文件 |
| **元数据持久化** | 采样、通道、采集条件等元信息随数据保存 |
| **chunk 索引** | 描述信号如何分段，读回时可还原分段结构 |
| **测量单位** | 保留每个通道的物理单位，避免单位混淆 |

## 命名空间

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.Data.Signal`（根） | 信号集合与 netCDF 读写入口 |

底层数值数组的 netCDF 编解码由 `Microsoft.VisualBasic.DataStorage.netCDF` 提供。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.Signal

' 1. 写出信号集合（含元数据、chunk 索引与单位）
Dim signals As New SignalCollection()
Call signals.Add("channel1", data, unit:="mV")
Call signals.Add("channel2", data2, unit:="nm")

Call signals.Save("./experiment.nc")

' 2. 读回：元数据与分段结构一并恢复
Dim reloaded = SignalCollection.Load("./experiment.nc")

For Each signal In reloaded.Signals
    Console.WriteLine($"{signal.Name}  {signal.Unit}  {signal.Length} points")
Next
```

## 实现要点

- **为什么选 netCDF 而不是 CSV**：CSV 无法表达"这个数组的物理单位是 mV"，也无法自然表达多维数组与多段数据；netCDF 从设计之初就面向"带语义的多维数组"。
- **chunk 索引的必要性**：一段连续采集的信号常被拆成多段（如每次进样一段）；若只保存拼接后的数组，段边界就永久丢失了。
- **与 R 流程互通的实际价值**：R 有成熟的信号处理与统计生态；能双向交换数据意味着可以「在 .NET 侧采集与预处理，在 R 侧做统计建模」，而不必统一技术栈。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.Signal`
- TargetFramework：`net10.0`
- Tags：`scibasic;signal-io;netcdf;time-series;chunk-index;measure-units;data-exchange;file-format`
- 许可：GPL-3.0-or-later
