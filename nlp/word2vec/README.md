# Word2Vec 词向量训练器（Skip-Gram / CBOW）

## 引言

Word2Vec 的核心洞见只有一句话：**一个词的语义由它的上下文决定**。把这个洞见变成可计算的目标函数，有两条经典路线：

- **Skip-Gram**：给定中心词，预测上下文；
- **CBOW**：给定上下文，预测中心词。

两条路线都需要一个技巧来降低 softmax 的计算量 —— 本实现选择 **Huffman 树**：高频词分到短码，从而让每次参数更新只涉及 O(log V) 个节点。本包把整套训练过程完整实现，并提供一个流式（fluent）工厂来集中声明所有超参数。

## 设计目标

- **配置集中且链式**：向量维度、窗口、采样率、频次阈值、学习率、线程数一次性声明；
- **训练与查询分离**：训练产出 `VectorModel`，查询侧只依赖该模型（可持久化、可并行使用）；
- **多线程**：训练按线程切分，`setNumOfThread` 直接控制并发度。

## 核心特性

- 由 `Word2VecFactory` 驱动的**多线程 Word2Vec 训练**（`setNumOfThread`），可配置向量维度、窗口、采样率、频次阈值与学习率；
- 基于 Huffman 树词神经元实现 **Skip-Gram 与 CBOW** 两种目标（`TrainMethod.Skip_Gram` / `TrainMethod.CBow`）；
- 支持从句子或纯 token 集合读入语料，并用频次阈值裁剪罕见词；
- 向量模型查询：按词或按中心向量求最近邻，以及 `word a - word b + word c` 形式的类比检索。

## 命名空间与关键类型

全部类型位于 `Microsoft.VisualBasic.Data.NLP.Word2Vec`：

| 类型 | 职责 |
|---|---|
| `Word2VecFactory` | 流式构建器（`setVectorSize`、`setWindow`、`setMethod`、`setSample`、`setNumOfThread`），`build` 返回训练器 |
| `Word2Vec` | 训练器：`readTokens`、`training`、`outputVector` |
| `VectorModel` | 训练好的向量集合：`similar`、`analogy`、`getWordVector` |
| `TrainMethod` | `CBow` 或 `Skip_Gram` |
| `WordNeuron` | 一个词及其 Huffman 路径与向量 |
| `WordScore` | 一个排序后的邻居（词名 + 得分） |
| `Trainer` | 并行训练使用的后台训练任务 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.NLP.Model
Imports Microsoft.VisualBasic.Data.NLP.Word2Vec

Dim w2v As Word2Vec = New Word2VecFactory() _
    .setVectorSize(100) _
    .setWindow(5) _
    .setMethod(TrainMethod.Skip_Gram) _
    .setNumOfThread(4) _
    .build()

Call w2v.readTokens(New Sentence({"the", "quick", "brown", "fox", "jumps"}))
Call w2v.training()

Dim model As VectorModel = w2v.outputVector()

For Each hit As WordScore In model.similar("fox", 10)
    Console.WriteLine($"{hit.name}  {hit.score}")
Next
```

## 实现要点

- **Huffman 树的作用**：把词汇表上的 softmax 分解为「从根到叶这条路径上的一串二分类」，使每个训练样本的代价从 O(V) 降到 O(log V)。
- **频次阈值**：语料中的长尾词噪声大且训练代价高，通过频次阈值直接裁掉，是提升向量质量最廉价的手段之一。
- **窗口与降采样**：`setWindow` 决定上下文半径，`setSample` 控制高频词的降采样比例，两者共同决定向量偏向「语义」还是「语法」。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.NLP.Word2Vec`
- TargetFramework：`net10.0`
- Tags：`scibasic;word2vec;word-embedding;skip-gram;cbow;huffman-tree;word-similarity;machine-learning`
- 许可：GPL-3.0-or-later
