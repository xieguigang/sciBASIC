# 图嵌入与集成学习工具箱（node2vec / graph2vec / ComplEx / Voting·Bagging·Stacking）

## 引言

机器学习算法吃的是**向量**，但很多真实数据是**图**：社交网络、分子结构、知识图谱、代谢网络。于是产生了「图嵌入」这一任务：

> 把图结构映射到向量空间，使「结构相近」的节点 / 图在向量空间中也相近。

本包提供三类主流方法：

| 方法 | 嵌入对象 | 核心思想 |
|---|---|---|
| **node2vec** | 节点 | 有偏随机游走 + word2vec |
| **graph2vec** | 整张图 | 把整图表示为向量 |
| **ComplEx 系列** | 知识图谱实体 / 关系 | 复数空间中的双线性打分 |

## node2vec：为什么"有偏"随机游走很重要

普通随机游走生成的是「邻居序列」，但邻居有两种截然不同的角色：

- **同质性（homophily）**：结构等价的节点（同一社区）→ 需要**深度优先**的游走（往回走的概率低）；
- **结构等价（structural equivalence）**：角色等价的节点（都是"枢纽"）→ 需要**广度优先**的游走（倾向回到起点附近）。

node2vec 通过两个参数（`p` 返回概率、`q` 前进概率）**连续调节**这两种偏向，因此同一张图可以产出面向不同任务的嵌入。

## graph2vec

把整张图（而不是单个节点）表示为向量，适合**图分类**任务，例如：

- 判断一个分子是否具有某种活性；
- 判断一个程序的调用图是否属于某类恶意软件。

## ComplEx 系列

| 命名空间 | 变体 | 说明 |
|---|---|---|
| `GraphEmbedding.complex` | ComplEx | 复数空间双线性打分，能表达**非对称**关系 |
| `GraphEmbedding.complex_NNE` | + 神经网络编码器 | 用神经网络学习实体 / 关系表示 |
| `GraphEmbedding.complex_NNE_AER` | + 自编码器正则 | 用自编码器约束嵌入空间结构 |
| `GraphEmbedding.complex_R` | 关系类型扩展 | 支持带类型的关系 |
| `GraphEmbedding.struct` | 结构嵌入 | 基于图结构的嵌入 |

为什么用**复数**：在实数向量空间中，双线性模型难以同时表达「对称」与「反对称」关系；引入复数虚部后，`Re(<e_s, w_r, ē_o>)` 这一打分可以自然地表达**方向性**（A 是 B 的老师 ≠ B 是 A 的老师）。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.MachineLearning.Bootstrapping`（根） | graph2vec 风格整图向量 |
| `....Bootstrapping.GraphEmbedding`（+ `.complex` / `.complex_NNE` / `.complex_NNE_AER` / `.complex_R` / `.struct` / `.util`） | 图与知识图谱嵌入 |
| `....Bootstrapping.node2vec` | node2vec 有偏随机游走 + word2vec |
| `....Bootstrapping.Ensemble`（`.Abstractions` / `.Core` / `.Adapters` / `.Voting` / `.Bagging` / `.Stacking`） | 集成学习：统一分类/回归契约与 Voting / Bagging / Stacking |
| `....Bootstrapping.Ensemble.Persistence`（`.Codecs`） | 模型持久化：JSON 清单 + CSV 数值附表，零重训加载 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MachineLearning.Bootstrapping
Imports Microsoft.VisualBasic.MachineLearning.Bootstrapping.Node2Vec

' 1. node2vec：有偏随机游走 + word2vec 训练
Dim n2v As New node2vec(embeddingDim:=128, walkLength:=80, walksPerNode:=10)
Dim nodeVectors = n2v.Fit(graph, p:=1.0, q:=0.5)

' 2. 整图向量（graph2vec 风格）
Dim graphVector = Graph2Vec.Embed(graph)

' 3. 知识图谱嵌入（ComplEx）
Dim kg = ComplEx.Train(triples, dim:=100, iterations:=1000)
Dim score = kg.Score("virus", "infects", "human")
```

## 实现要点

- **p / q 参数的直觉**：`q > 1` 偏向广度优先（结构等价），`q < 1` 偏向深度优先（同质性）；`p` 控制"立刻原路返回"的概率。
- **ComplEx 的非对称性**：把关系视为复数变换，使 `Score(s, r, o) ≠ Score(o, r, s)`，从而正确建模有向关系。
- **嵌入的实际用途**：得到节点向量后，链接预测、节点分类、聚类都可以直接用标准机器学习包处理——这正是「把图问题转化为向量问题」的价值。

## 集成学习框架（Stacking / Voting / Bagging）

`Ensemble` 子命名空间把本仓库里**异质**的算法族统一到同一套分类 / 回归契约下，并实现三类集成器。集成器只依赖 `IClassifier` / `IRegressor` 与对应的 `I*Learner`，新增算法只需要补一个适配器。

### 算法族与适配器

| 算法族 | 分类 | 回归 | 说明 |
|---|---|---|---|
| 朴素贝叶斯（DataMining） | ✅ | — | 离散特征；支持多分类 |
| 逻辑回归（DataFittings） | ✅（二分类） | — | 输出 `P(y=1\|x)` |
| SVM（MachineLearning） | ✅ | — | `C_SVC`，概率需 `probability=True` |
| SVR（MachineLearning） | — | ✅ | `EPSILON_SVR`，取 `SVMPrediction.score` |
| 随机森林（MachineLearning） | — | ✅ | `RanFog` 只输出叶均值，没有概率输出 |
| 高斯贝叶斯回归（MachineLearning） | — | ✅ | 默认走连续特征（Gaussian）模式 |
| 岭回归 / MLR / LASSO（DataFittings） | — | ✅ | LASSO 按 R² 选择正则化路径上的最优步 |
| XGBoost（TGBoost） | ✅（二分类） | ✅ | 通过 `GBM.fit` 训练；多分类未支持 |
| 外部 XGBoost 模型 | ✅ | ✅ | `XGBoostPredictor` 包装已训练的 xgboost4j 模型 |

### 用法

```vbnet
Imports Microsoft.VisualBasic.MachineLearning.Bootstrapping.Ensemble

' Voting：逻辑回归 + 朴素贝叶斯
Dim vote As New VotingClassifier(
    {New LogisticClassifierLearner(), New NaiveBayesClassifierLearner()}).Train(x, y)
Dim p = vote.PredictProbability(x(0))

' Bagging：同一个学习器 + 有放回采样
Dim bag As New BaggingClassifier(
    New NaiveBayesClassifierLearner(), modelCount:=10, seed:=42).Train(x, y)

' Stacking：K 折 OOF 元特征 + 元学习器
Dim stack As New StackingClassifier(
    {New NaiveBayesClassifierLearner(), New LogisticClassifierLearner()},
    New LogisticClassifierLearner(),
    folds:=5, seed:=42).Train(x, y)
Dim oof = stack.OofAccuracy

' 回归同理：VotingRegressor / BaggingRegressor / StackingRegressor
```

### 关键约定

- **概率语义**：`IClassifier.PredictProbability` 的顺序严格等于 `Classes`；集成器内部用 `ProbabilityMath.AlignTo` 对齐，基模型的类别编码差异不会串位。
- **标签编码只做一次**：`LabelEncoder` 在入口处把标签统一成升序类别数组与整数索引，适配器不再重复编码。
- **OOF 而非训练集内预测**：Stacking 用分层 K 折生成外推预测，避免元学习器学到基模型的过拟合噪声。
- **可复现**：Bagging 采样与 K 折划分都使用独立的 `Random(seed)`；相同种子下结果一致。
- **并行默认关闭**：`VotingRegressor.Parallel` 与 `Bagging*.Parallel` 默认 `False`，避免与 `RanFog`、`GBM` 内部的批量并行嵌套。
- **不静默降级**：单类别、权重为负或和为零、特征行长度不齐、未训练预测都会抛出带上下文的异常。
- **维度守卫**：集成器与适配器都实现 `IFeatureSchema`，训练期记录 `FeatureWidth`，预测期逐样本校验输入维度。
- **已知限制**：`RanFog.Predict` 不输出概率，因此随机森林只作为回归基学习器；`LogisticFit` 只支持二分类。

### 模型持久化

训练完成的集成模型（以及任意单个基学习器）可以导出为**模型包**，之后无需重训即可恢复预测能力，预测值与保存前逐位一致。

```vbnet
Imports Microsoft.VisualBasic.MachineLearning.Bootstrapping.Ensemble.Persistence

' 1. 保存：集成器自带 Save()，也可走统一入口
Dim manifest = stack.Save("D:\models\stack-01")
Dim report = EnsembleModelStore.Save(bag, "D:\models\bag-01")

' 2. 加载：重建后可继续预测
Dim restored = EnsembleModelStore.LoadClassifier("D:\models\stack-01")
Console.WriteLine(restored.PredictProbability(x(0)).Sum())

' 3. 也可以单独持久化一个基学习器
Dim ridge As IRegressor = New RidgeRegressorLearner().Fit(x, y)
Call EnsembleModelStore.Save(ridge, "D:\models\ridge")
Dim again = EnsembleModelStore.LoadRegressor("D:\models\ridge")
```

模型包目录布局（JSON 清单 + CSV 附表，全部为可读文本）：

```
<model-bundle>/
├── model.json                       # 类型、超参数、类别顺序、特征维度、子模型树
├── base/0-naive-bayes/tables.csv    # 条件频率表（class, feature, key, count）
├── base/1-svm/support-vectors.csv   # 支持向量的稀疏三元组
├── base/1-svm/coefficients.csv
└── meta/logistic/…                  # 堆叠的元学习器
```

设计要点：

- **严格恢复**：底层库补齐了 `Restore` 工厂（`Bayesian` / `GaussianBayes` / `TargetBinning` / `BayesianRegression` / `LassoFit`），加载路径不做任何重训；XGBoost 复用 TGBoost 的 `ModelSerializer` 文本格式，整包统一为 CSV。
- **小数点位往返**：数字一律以 `"R"` + `InvariantCulture` 写出，`Parse(Format(x)) = x` 逐位成立。
- **大小数值分治**：标量与超参数内联在 `model.json`（便于人工阅读），条件概率表、树结构、支持向量、系数矩阵落 CSV（避免 JSON 膨胀到不可读）。
- **安全**：清单里的 CSV 路径必须是包内相对路径；加载时规范化后校验仍在包目录内，拒绝绝对路径与 `..` 穿越。
- **格式版本**：清单带 `formatVersion`，不匹配时直接失败，不做「尽力而为」的猜测。
- **算法白名单**：加载按 `algorithm` 精确查表，未知算法抛出列有全部已注册标识的异常。

### 审计

`test/EnsembleAudit.vbproj` 是 console 审计程序，用 `DebugAudit` 覆盖适配器、K 折 OOF 唯一性、三类集成器、基线对比、边界异常，以及**全族模型的保存 → 加载 → 逐位比对**：

```powershell
dotnet run --project test/EnsembleAudit.vbproj
```

退出码等于本程序自身失败断言的数量（被集成算法库内部的全局断言不计入），报告写入 `bin/<Config>/net10.0/ensemble-test-report.json`。

## 包信息

- Assembly：`Microsoft.VisualBasic.MachineLearning.Bootstrapping`
- TargetFramework：`net10.0`
- Tags：`scibasic;graph-embedding;node2vec;graph2vec;knowledge-graph;complex;random-walk;representation-learning`
- 许可：GPL-3.0-or-later
