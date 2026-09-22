# PaCMAP：成对控制流形近似与投影

## 引言

降维方法的「哲学分歧」在于：**到底要保留局部结构，还是全局结构？**

- t-SNE 极擅长局部结构（谁和谁是邻居），但簇与簇之间的距离通常没有意义；
- UMAP 试图兼顾，但全局结构的稳定性依赖参数；
- **PaCMAP** 的思路是把这件事**显式建模**：在损失函数里同时定义三类点对，并分阶段调整它们的权重。

## 三类点对（这是 PaCMAP 的核心）

| 点对类型 | 含义 | 作用 |
|---|---|---|
| **Nearest pairs（近邻对）** | k 近邻 | 保持**局部**邻域关系 |
| **Mid-near pairs（中近对）** | 从较近但不最近的样本中采样 | 保持**中程**结构，防止局部簇漂移 |
| **Further pairs（远距对）** | 随机远距离样本 | 保持**全局**分离 |

**中近对**是 PaCMAP 区别于其他方法的关键：它像「中间尺度锚点」一样约束簇之间的相对位置，使嵌入结果中的簇间距离具备可解释性。

## 三阶段优化

损失权重随优化推进而**分阶段变化**：

1. **早期**：以近邻对为主 → 先把局部簇凝聚成形；
2. **中期**：引入中近对 → 调整簇的相对位置；
3. **后期**：强化远距对 → 拉开全局分离。

这种「先局部后全局」的调度，避免了 UMAP 早期容易出现的「簇间挤压」问题。

## 核心能力与关键类型

全部类型位于根命名空间 `Microsoft.VisualBasic.DataMining.PaCMAP`：

| 类型 | 职责 |
|---|---|
| `PaCMAP` | 主算法：三类点对采样、三阶段损失与优化循环 |
| `Euclidean` | 欧氏距离度量 |
| `Normalized` | 归一化距离度量 |
| `NumericTableExtensions` / `TensorExtensions` | 数值表与张量扩展辅助 |

优化器为 **Adagrad**（对每个维度自适应调整学习率），因此对初始学习率不敏感。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.DataMining.PaCMAP

' 1. 用数值表构建嵌入器
Dim pacmap As New PaCMAP(nComponents:=2)

' 2. 直接在张量上优化（自动完成三阶段调度）
Dim embedding = pacmap.FitTransform(data)

' 3. 嵌入结果可交给可视化层绘制
Console.WriteLine(embedding.Dimensions)
```

## 实现要点

- **为什么需要中近对**：只用近邻对 + 远距对时，簇可以「任意摆放」而不违反损失（因为簇内关系已满足）；中近对提供了额外的中程约束，使簇的相对位置被数据决定，而非随机初始化决定。
- **Adagrad 的适配性**：不同维度的梯度尺度差异很大（尤其在高维数据上），逐维自适应学习率能显著减少调参负担。
- **与 UMAP 的选择依据**：若下游只做「聚类后查看」，局部结构足够；若要做「簇间比较 / 轨迹推断」，PaCMAP 的全局结构更可靠。

## 包信息

- Assembly：`Microsoft.VisualBasic.DataMining.PaCMAP`
- TargetFramework：`net10.0`
- Tags：`scibasic;pacmap;dimensionality-reduction;manifold-learning;pairwise-controlled;visualization`
- 许可：GPL-3.0-or-later
