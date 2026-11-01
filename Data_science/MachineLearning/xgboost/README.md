# XGBoost 模型预测器与梯度提升树训练器

## 引言

XGBoost 是表格数据上最常用的强模型之一。本包提供**两个方向的完整能力**：

| 方向 | 能力 | 价值 |
|---|---|---|
| **推理** | 读取并评分 XGBoost 模型文件 | 在 .NET 进程内使用别处训练好的模型，**无需原生依赖** |
| **训练** | 训练三元梯度提升树 | 在 .NET 内完成建模范式，含类别特征与缺失值处理 |

## 支持的 booster

| Booster | 特点 |
|---|---|
| **GBTree** | 经典梯度提升树（默认） |
| **GBLinear** | 线性 booster（特征维度高、样本少时） |
| **Dart** | 引入 dropout 的树提升，抗过拟合 |

推理侧会解码模型的完整结构：**树结构、分裂条件与叶子值**——因此不依赖任何 XGBoost 原生库。

## 命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.MachineLearning.XGBoost`（根） | 模型读取与预测入口 |
| `....XGBoost.gbm` | 梯度提升机核心 |
| `....XGBoost.learner` | learner 抽象 |
| `....XGBoost.tree` | 决策树基学习器 |
| `....XGBoost.train` | TGBoost 训练实现（三元树） |
| `....XGBoost.train.kernels` | 训练内核：列块 `ColumnBlock`、行分区 `RowPartitioner`、分裂查找 `Exact/Histogram`、`FlatTree` |
| `....XGBoost.config` | 参数模型（学习率、子采样、正则化） |
| `....XGBoost.util`（+ `FVecArray`） | 特征向量与工具方法 |
| `....XGBoost.spark` | Spark 集成类型 |

## 快速上手

```vbnet
Imports Microsoft.VisualBasic.MachineLearning.XGBoost
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.train

' 1. 读取别处训练好的 XGBoost 二进制模型并预测
Using stream = IO.File.OpenRead("./model.bin")
    Dim predictor As New Predictor(stream)
    Dim score As Double() = predictor.predict(featureVector)          ' 变换后的预测值
    Dim margin As Double() = predictor.predict(featureVector, True)   ' 原始 margin
    Dim leaves As Integer() = predictor.predictLeaf(featureVector)    ' 每棵树的叶子下标
End Using

' 2. 在 .NET 内训练（TGBoost，三元树 + 类别特征 + 缺失值）
Dim gbm As New GBM()

Call gbm.fit(trainset:=trainData,
             valset:=validData,
             early_stopping_rounds:=10,
             maximize:=True,
             eval_metric:=Metrics.auc,
             loss:="logloss",
             eta:=0.3,
             num_boost_round:=100,
             max_depth:=6,
             rowsample:=0.8,
             colsample:=0.8,
             min_child_weight:=1,
             min_sample_split:=5,
             lambda:=1.0,
             gamma:=0.0,
             num_thread:=-1,
             tree_method:=TreeMethod.Exact,
             max_bin:=256,
             random_seed:=42)

Dim prediction As Double() = gbm.predict(testData)
' 早停选出的最优轮次（模型已自动截断到该轮次）
Console.WriteLine($"best iteration = {gbm.bestIteration}, trees = {gbm.trees.Count}")
```

## 训练参数

| 参数 | 默认 | 说明 |
|---|---|---|
| `loss` | `logloss` | `logloss` / `squareloss` / `qlinearloss`（也接受 `binary:logistic`、`reg:squarederror` 等别名）。未知名称会抛 `ArgumentException` |
| `eta` | `0.3` | 学习率（收缩系数） |
| `num_boost_round` | `20` | 最大提升轮数 |
| `max_depth` | `7` | 最大深度；`0` 表示不限制（内部硬上限 30） |
| `rowsample` | `0.8` | **每轮重新**伯努利抽样的行比例 |
| `colsample` | `0.8` | 每轮重新抽样的列比例（至少保留 1 列） |
| `min_child_weight` | `1` | 子节点海森和下限（按未抽样前的全量海森计算） |
| `min_sample_split` | `5` | 子节点最小样本量 |
| `lambda` | `1` | L2 正则；仅当传入 ≤ 0 时内部兜底为 `1e-5` |
| `gamma` | `0` | 分裂最小增益 |
| `num_thread` | `-1` | 并行度；`-1` 取 CPU 核心数，`1` 表示串行 |
| `tree_method` | `Exact` | `Exact`（精确前缀扫描）/ `Hist`（`max_bin` 分桶直方图） |
| `max_bin` | `256` | `Hist` 模式的最大分桶数 |
| `random_seed` | `Nothing` | 随机种子；固定后可完全复现训练结果 |

> `tree_method=Hist` 通过分桶压缩分裂搜索空间，在高基数（连续型、取值很多）特征上更快，
> 但会引入量化误差。默认仍是 `Exact`，以保证与既有模型/基线的数值一致。

## SIMD 加速

热路径默认走 `Microsoft.VisualBasic.Core` 的 SIMD 算子（`SimdIntrinsics` FMA /
`SimdMath` 向量化 `exp` / `SimdReduce` / `SimdParallel`）。可观测性与开关：

```vbnet
' 训练日志会打印一行能力信息，例如：
' SIMD=on,fma=on,avx2=on,vecWidth=4,fastExp=on,minVec=8,minParallel=4096,cpus=24
Console.WriteLine(SimdOptions.Describe())

' 关闭 SIMD，退回纯标量实现（用于数值对照或排查硬件问题）
SimdOptions.UseSimd = False
```

环境变量（无需改代码）：

| 变量 | 作用 |
|---|---|
| `XGB_SIMD=0` | 全局关闭 SIMD 与并行算子，退回标量实现 |
| `XGB_SIMD_FASTEXP=0` | 关闭多项式近似 `exp`（改用 `System.Math.Exp`，精度更高但更慢） |

精度口径（测试 `SimdPrecision` 实测）：

- 近似 `exp` 相对误差 `1.53E-15`（z ∈ [-40, 0]），因此 `FastExp` 默认开启；
- 多累加器求和的重结合误差 `3.36E-14`，会让**极接近**的两个候选切分点交换胜负，
  端到端 loss 因此有 1e-4 量级的差异——属离散分裂选择的固有放大，不是数值错误；
- 需要与历史结果逐位一致的场景，请设置 `XGB_SIMD=0`。

并行度提示：训练是流式访存内核，加速比会先撞内存带宽墙。开发机（24 逻辑核）上
`num_thread=8` 即饱和，`32/64` 反而略慢；请在自己机器上用 `SimdBenchmark` 的
线程扩展性输出确认拐点，再决定线上 `num_thread`。

## 回归与基准测试

```bash
dotnet run --project test/test.vbproj -c Debug
```

- 退出码 = 失败断言数（`0` 表示全部通过）；
- 报告写入 `test/bin/Debug/net10.0/xgboost-test-report.json`（由 `DebugAudit` 生成）；
- 覆盖：推理回归（真实 `.model` + 期望值逐值比对）、训练回归（损失单调性、叶值最优性
  `leaf = -G/(H+λ)`、扁平树与 `TreeNode` 路由一致性、早停回滚、序列化往返、类别编码保真、
  相同种子可复现、`hist`/`exact` 一致性）、性能基准。

缺陷与优化记录见 [REVIEW.md](./REVIEW.md)。

## 实现要点

- **为什么要在 .NET 内实现推理**：生产环境部署原生 XGBoost 往往受限于平台二进制、版本匹配与许可合规；纯托管推理只需一个 DLL，且行为完全可预测。
- **Booster 的选择依据**：树模型（GBTree / Dart）适合非线性与特征交互；线性 booster 在高维稀疏特征（如文本 TF-IDF）上更合适，且更易解释。
- **Dart 的 dropout 机制**：随机丢弃一部分已建成的树参与预测，从而抑制后续树对前序树的过度依赖，提升泛化。
- **类别特征与缺失值**：XGBoost 的一大优势是**内置处理**这两类问题——类别列无需 one-hot（避免维度爆炸），缺失值有默认分支（无需插补）。
- **与 `XGBoostDataSet` 的分工**：本包负责「模型」，训练数据的准备（CSV 读取、类别发现、缺失值索引）由 `XGBoostDataSet` 包完成。

## 包信息

- Assembly：`Microsoft.VisualBasic.MachineLearning.XGBoost`
- TargetFramework：`net10.0`
- Tags：`scibasic;xgboost;gradient-boosting;gbtree;gblinear;dart;decision-tree;predictor;classifier`
- 许可：GPL-3.0-or-later
