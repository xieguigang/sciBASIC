# Myers O(ND) 差异算法与文本比较

## 引言

「两个版本之间改了什么」看起来是个简单问题，但当文本长达数十万行时，朴素的逐行比较会退化成 O(N²)。

**Myers 算法**（Eugene Myers, 1986）给出了一个优雅的解法：把「编辑距离」问题转化为**在编辑图上寻找最短路径**，用贪心的最远到达（furthest-reaching）搜索配合回溯，在 O((N+M)·D) 时间内找到**最短编辑脚本**（D 为实际编辑次数）。

本包实现了该算法，并围绕它提供完整的差异结果模型与输出格式化。

## 设计目标

- **粒度可选**：既支持文件 / 字符串数组的**行级**比较，也支持纯文本的**字符级**比较；
- **结果可编程**：不只是打印差异，而是给出结构化的 `DiffItem` / `DiffBlock`，便于二次处理；
- **输出开箱可用**：unified diff、并排视图与带相似度的一行摘要。

## 核心特性

- 以经典 Myers 贪心最远到达搜索配合回溯，在 O((N+M)D) 时间内计算最短编辑脚本；
- 文件或字符串数组的**行级**比较，以及纯文本的**字符级**比较；
- **结构化结果模型**：逐条编辑类型与旧 / 新索引，并聚合成连续同类型的 `DiffBlock`；
- **可直接打印的输出**：带上下文行的 unified diff、并排（side-by-side）视图，以及带相似度评分的一行摘要。

## 关键类型与 API

- `Microsoft.VisualBasic.Data.MyersDiff.MyersDiff` —— 算法引擎：`Compare`、`CompareLines`、`CompareFiles`、`CompareChars`；
- `...MyersDiff.DiffResult` —— 完整结果：条目、计数、`Similarity`、`ToUnifiedDiff`、`ToSideBySide`、`ToSummary`；
- `...MyersDiff.DiffItem` —— 单条编辑（保留 / 插入 / 删除），含旧、新索引与文本值；
- `...MyersDiff.DiffBlock` —— 连续同类型差异条目的聚合；
- `...MyersDiff.EditType` —— 编辑操作类型（equal / insert / delete）；
- `...MyersDiff.DiffUtils` —— 直接返回差异文本或结果的静态便捷封装。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.MyersDiff

Dim differ As New MyersDiff()
Dim result As DiffResult = differ.CompareFiles("old.txt", "new.txt")

Console.WriteLine(result.ToUnifiedDiff("old.txt", "new.txt"))
Console.WriteLine(result.Similarity)
```

## 实现要点

- **为什么要「回溯」**：贪心搜索只记录「能走到多远」，最短编辑脚本本身需要在搜索结束后沿记录的最远到达路径回溯重建。
- **D 决定了实际开销**：复杂度中的 D 是编辑次数而非序列长度；这正是 Myers 算法擅长「相似文本对比」而弱于「完全不同文本对比」的原因。
- **块聚合的价值**：逐行编辑对人有噪声，把连续同类型编辑聚合成块之后，差异视图（unified / side-by-side）才能直接可读。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.MyersDiff`
- TargetFramework：`net10.0`
- Tags：`scibasic;diff;myers-algorithm;text-comparison;edit-script;unified-diff;similarity`
- 许可：GPL-3.0-or-later
