# 二部知识图谱：同义词消歧与本体关系推断

## 引言

构建领域知识库时，最棘手的问题往往不是「如何存」，而是「如何清理」：同一个概念会在不同来源里以不同名字出现（`water` / `H2O` / 水），而这些别名到底是不是同一个东西、彼此之间又是什么关系，需要**统计上站得住脚的判据**，而不是拍脑袋的字符串匹配。

本包用一张**二部图**（bipartite graph）承载这个问题：图中只有两种节点 —— **实体（entity）** 与 **属性（attribute）**，且边只能从实体连向属性。这个简单约束带来两个好处：

1. 「共享属性越多」天然等价于「语义越接近」，于是可以用图上的相似度度量；
2. 可以为每个相似度做**度保持的随机重连置换检验**，得到 z 值与 p 值，从而把「看起来像同义词」升级为「统计上显著是同义词」。

## 设计目标

- **可解释的判据**：每一个同义词合并决定都由具体的相似度指标 + p 值阈值支撑；
- **零分布来自图自身**：置换检验保持每个实体的度不变，因此对照的是同一张图的随机版本，避免度分布差异带来的假阳性；
- **本体推断带类型**：不只是「相关」，而是区分 `IsA` / `SiblingOf` / `HasFunction` / `RelatedTo` 并给出置信度。

## 核心特性

- **二部图存储**：实体节点只与属性节点相连；提供邻接表、度查询、邻居查询与类别统计。
- **二部结构上的相似度**：Jaccard、Adamic-Adar、余弦、共同邻居数与包含率。
- **度保持置换检验**：输出 z 值、p 值与 Bonferroni 校正结果，用于判定两个实体是否真的互为同义词。
- **本体关系推断**：把实体对分类为 `IsA`、`SiblingOf`、`HasFunction` 或 `RelatedTo`，并给出置信度。

## 命名空间与关键类型

| 类型 | 职责 |
|---|---|
| `KnowledgeGraph` | 二部图容器：增删实体 / 属性 / 边，查询度、邻居与共享属性 |
| `SimilarityMetrics` | 计算单对或全对的 Jaccard、Adamic-Adar、余弦与包含率 |
| `StatisticalTest` | 度保持随机重连置换检验，产出 p 值与 z 值 |
| `EntityDisambiguator` | 将通过 Jaccard / p 值阈值的实体合并为 `SynonymGroup` |
| `OntologyInferer` | 结合图与同义词组推断带类型的本体关系 |
| `EntityNode` / `AttributeNode` | 两种节点：知识术语 vs 知识属性 |
| `OntologyRelation` | 一条推断出的关系：类型、得分、共享属性与置信度 |

以上类型均位于根命名空间 `Microsoft.VisualBasic.Data.NLP.Knowledge`。

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.Data.NLP.Knowledge

Dim graph As New KnowledgeGraph()
Dim water As Integer = graph.AddEntity("water", "en", "compound")
Dim h2o As Integer = graph.AddEntity("H2O", "en", "compound")

Call graph.AddEntityAttribute(water, "formula", "chemical")
Call graph.AddEntityAttribute(h2o, "formula", "chemical")
Call graph.AddEntityAttribute(water, "boiling-point", "physical")

Dim synonyms = New EntityDisambiguator(graph).Disambiguate()
Dim relations = New OntologyInferer(graph).InferRelations(synonyms)
```

## 实现要点

- **为什么必须用置换检验**：两个实体共享 5 个属性听起来很像同义词，但如果它们本来就各有一百个属性，这 5 个重叠可能完全出于偶然。度保持置换检验正是为了剔除这种「因为热门所以重合」的假信号。
- **Bonferroni 校正**：当需要对全对实体做检验时，多重比较会抬高假阳性率；实现中已给出校正结果，供调用方直接使用。

## 与 sciBASIC# 生态的关系

本包属于 `nlp` 家族的「知识层」：其上可以叠加 `word2vec` 的词向量相似度、`NLP` 的分词与检索能力，共同构成知识库构建流水线。

## 包信息

- Assembly：`Microsoft.VisualBasic.Data.NLP.Knowledge`
- TargetFramework：`net10.0`
- Tags：`scibasic;knowledge-graph;bipartite-graph;entity-disambiguation;synonym-merge;ontology;similarity;nlp`
- 许可：GPL-3.0-or-later
