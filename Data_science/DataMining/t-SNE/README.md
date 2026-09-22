# Barnes-Hut t-SNE 降维嵌入

## 引言

t-SNE 的核心思想是：**在高维空间用高斯分布描述「相似度」，在低维空间用 t 分布描述「相似度」，然后最小化两者之间的 KL 散度。**

它之所以在可视化领域广受欢迎，是因为 t 分布的长尾特性能够缓解「拥挤问题」——高维空间里的中等距离点，在低维空间可以被推得足够远，从而形成边界清晰的簇。

但原始形式的代价是 O(n²)：每个点都要和所有其他点交互。**Barnes-Hut 近似**把这个代价降到 O(n log n)。

## 算法四步

1. **计算高维相似度**：在给定困惑度（perplexity）下把成对亲缘度转成联合概率（`SparseProbability`）；
2. **构建空间索引**：把低维点组织成四叉树（`SPTree`）；
3. **近似梯度**：近距离点逐个计算，远距离点按「质心」整体计算（`BarnesHutGradient`）；
4. **优化**：沿 KL 散度梯度下降（`CostFunction`）。

## 核心能力与关键类型

全部类型位于根命名空间 `Microsoft.VisualBasic.MachineLearning.tSNE`：

| 类型 | 职责 |
|---|---|
| `tSNE` | 主算法：困惑度标定与优化主循环 |
| `SparseProbability` | 把成对亲缘度转成稀疏联合概率 |
| `CostFunction` | KL 散度代价函数 |
| `BarnesHutGradient` | Barnes-Hut 近似的梯度计算 |
| `SPTree` | Barnes-Hut 使用的空间划分树 |
| `RandomHelper` | 低维初始化的随机辅助 |
| `Helper` / `NumericTableExtensions` | 共享辅助与数值表扩展 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MachineLearning.tSNE

Dim tsne As New tSNE(perplexity:=30, theta:=0.5)

' 1. 由数据表构建
Call tsne.LoadData(data)

' 2. 运行优化
Dim embedding = tsne.Run(iterations:=1000)

Console.WriteLine(embedding.Dimensions)
```

## 实现要点

- **困惑度（perplexity）是什么**：可以粗略理解为「每个点的有效邻居数量」。它通过二分搜索标定每个点的高斯带宽，使得条件分布的熵等于 log(perplexity)。**典型取值 5–50**，值越大越强调全局结构。
- **Barnes-Hut 的 θ 参数**：θ 控制「多远算远」——θ=0 退化为精确计算，θ 越大越快但误差越大。常用 0.5。
- **结果的解读边界**：t-SNE 的簇内结构可信，**簇间距离不可过度解读**（不同运行之间尤其如此），这是由算法本身的损失函数决定的，不是实现缺陷。

## 包信息

- Assembly：`Microsoft.VisualBasic.MachineLearning.tSNE`
- TargetFramework：`net10.0`
- Tags：`scibasic;t-sne;dimensionality-reduction;barnes-hut;sptree;kl-divergence;visualization`
- 许可：GPL-3.0-or-later
