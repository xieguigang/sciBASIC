# 动态规划：序列比对与背包问题

## 引言

动态规划的精髓在于**把指数级的枚举换成表填充**。本包提供两组经典问题的实现：

- **序列比对**：全局（Needleman-Wunsch）与局部（Smith-Waterman）比对，以及多序列比对的中心星法；
- **背包问题**：0-1 背包求解。

这两类问题贯穿生物信息学（基因 / 蛋白序列比对）与资源分配（预算与容量约束），实现上都基于同一套「表 + 回溯」范式。

## 设计目标

- **通用内核**：算法作用于泛型元素序列，因此比对对象可以是核苷酸、氨基酸、文本 token 或任意可比较元素；
- **可扩展检索**：局部比对支持 HSP（高分片段对）连接，把零散的高分片段串成完整对齐；
- **可加速**：提供 k-banded 搜索，在「预期相似度高」的场景下把搜索空间从 O(n·m) 降到 O(k·n)。

## 核心能力

| 命名空间 | 算法 | 说明 |
|---|---|---|
| `...DynamicProgramming.NeedlemanWunsch` | 全局比对 | 两端都要求对齐，适合同源全长比较 |
| `...DynamicProgramming.SmithWaterman` | 局部比对 | 只保留得分最高的局部片段，适合包含无关区段的比较；含 HSP 连接、k-banded 搜索与中心星多序列比对 |
| `...DynamicProgramming.Knapsack` | 0-1 背包 | 容量约束下的最优价值选择 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.DataMining.DynamicProgramming.SmithWaterman

' 1. 局部比对，得到高分片段对
Dim hsps = SmithWaterman.Align(query, subject)

' 2. 把 HSP 串接成完整对齐（含 k-banded 加速）
Dim alignment = SmithWaterman.Chain(hsps, bandWidth:=16)

Console.WriteLine(alignment.Score)
```

## 实现要点

- **全局 vs 局部的本质差别**：全局比对从 (0,0) 走到 (n,m)，惩罚「悬空」的序列两端；局部比对允许从任意位置起始与结束，且分数不会降到负值（触底归零）。一字之差决定了「是同源吗」与「哪一段同源」这两种不同问题。
- **HSP 连接的必要性**：局部比对一次只给出一个高分片段，但两个长序列的同源区往往被插入 / 缺失打断成多个片段；把这些片段按顺序连接起来才能得到生物学意义上的对齐。
- **k-banded 的适用条件**：当两条序列高度相似时，最优路径必然靠近对角线；限制搜索带宽 k 带来的加速非常可观，但相似度低时会漏掉最优解。

## 包信息

- Assembly：`Microsoft.VisualBasic.DataMining.DynamicProgramming`
- TargetFramework：`net10.0`
- Tags：`scibasic;dynamic-programming;sequence-alignment;smith-waterman;needleman-wunsch;knapsack;hsp-chaining;multiple-alignment`
- 许可：GPL-3.0-or-later
