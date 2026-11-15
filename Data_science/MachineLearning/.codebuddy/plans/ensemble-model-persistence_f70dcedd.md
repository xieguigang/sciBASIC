---
name: ensemble-model-persistence
overview: 为 Bootstrapping 项目的 Ensemble 集成学习框架新增以 JSON+CSV bundle 为载体的模型持久化能力（Save/Load、零重训严格数值等价恢复），并同步改造底层算法库补齐反序列化入口，同时优化 Ensemble 层的重复代码、特征维度校验、持久化契约与文档。
todos:
  - id: explore-verify
    content: 用 [subagent:code-explorer] 复核底层模型字段可见性与恢复签名
    status: completed
  - id: underlying-restore
    content: 为 Bayesian、GaussianBayes、TargetBinning、BayesianRegression、LassoFit 新增 Restore 工厂与只读访问器
    status: completed
    dependencies:
      - explore-verify
  - id: ensemble-shared
    content: 新增 EnsembleValidation 与 EnsembleParallel，替换 6 个集成器的重复校验与并行循环
    status: completed
    dependencies:
      - explore-verify
  - id: ensemble-feature-schema
    content: 为 6 个集成器与全部适配器加 FeatureWidth 校验及恢复构造
    status: completed
    dependencies:
      - ensemble-shared
  - id: storage-infra
    content: 实现 CsvTable、ModelManifest、ModelBundle 与 IModelCodec 注册表
    status: completed
    dependencies:
      - ensemble-shared
  - id: codecs
    content: 实现 9 个算法族的 codec 与 EnsembleModelStore 加载分发
    status: completed
    dependencies:
      - underlying-restore
      - storage-infra
      - ensemble-feature-schema
  - id: wire-and-doc
    content: 集成器实现 IPersistableModel，补充 NamespaceDoc、README 与包版本说明
    status: completed
    dependencies:
      - codecs
  - id: audit-roundtrip
    content: 用 [skill:lsp-code-analysis] 核查调用点后，在 test/Program.vb 增加全族 round-trip 审计
    status: completed
    dependencies:
      - wire-and-doc
---

## 产品概述

为现有集成学习框架补齐模型持久化能力：训练完成的集成模型（投票 / 装袋 / 堆叠）及其任意基学习器，可导出为「JSON 清单 + CSV 数据附表」的模型包；之后无需重新训练即可从模型包恢复模型对象并继续预测，预测结果与保存前完全一致。

## 核心功能

- **模型保存**：任一集成器与全部基学习器算法均可导出为模型包；目录内 `model.json` 描述模型类型、超参数、类别顺序、特征维度，同目录 CSV 附表承载系数矩阵、条件概率表、树结构、支持向量等大块数值。
- **模型加载**：从模型包恢复得到可直接预测的模型对象，走零重训路径；加载时校验格式版本、模型类型、类别顺序、CSV 行列形状，异常给出明确上下文。
- **格式可读**：JSON 与 CSV 均为纯文本、与语言无关，可被外部工具直接读取与人工检查；浮点数往返不失精度。
- **输入维度守卫**：模型记录训练时的特征维度，预测时校验输入向量长度，维度不符立即抛出带具体数值的错误，避免维度错位导致的静默错误。
- **统一接口**：所有可持久化模型提供一致的保存与加载入口、一致的模型类型标识与错误提示。
- **代码收敛**：消除集成器之间重复的训练参数校验、基模型并行/串行训练循环、批量预测与未训练守卫逻辑。
- **审计验证**：对全部算法族与三类集成器执行「训练 → 保存 → 加载 → 预测逐位比对」，并生成测试报告。

## 技术栈选择

- 语言与框架：VB.NET / `net10.0`，沿用 `Bootstrapping.vbproj` 现有工程配置（`OptionInfer On`、`ImplicitUsings enable`、`Nullable enable`）。
- JSON：复用 `Microsoft.VisualBasic.Serialization.JSON`（`Microsoft.VisualBasic.Core/src/Serialization/JSON/JsonSerialization.vb`，已通过 `Core.vbproj` 引用），API 为 `GetJson(Of T)` / `LoadJSON(Of T)` / `WriteLargeJson(Of T)`。
- CSV：在 Bootstrapping 项目内自带最小 RFC4180 读写器（`Data/DataFrame` 属于未被引用的独立项目，不新增跨项目引用）。
- 审计测试：`Microsoft.VisualBasic.ApplicationServices.Debugging.DebugAudit`（用户规则要求的 console 测试报告方式）。
- 不引入新第三方依赖，不新增项目引用。

## 实施方案

### 1. 模型包（bundle）布局

```
<model-bundle-dir>/
├── model.json                 # 清单：格式版本、模型类型、超参数、类别、特征维度、子模型条目
├── base/0_naive-bayes.csv     # 基模型大块数值（文件名由清单引用，禁止绝对路径）
├── base/0_naive-bayes.json    # 可选：单个基模型的小型标量状态（便于人工查看）
├── base/1_svm_vectors.csv
└── meta/logistic.csv          # 堆叠的元学习器数据
```

`Save(directory)` 自动创建目录；已存在的同名目录不静默覆盖已有清单，而是整体重写并先写入临时清单再替换，避免半成品包。

### 2. 清单数据结构（强类型、无多态）

清单只使用具体类型（`Dictionary(Of String, ...)`、数组、嵌套类），**不使用接口/抽象类型字段**，从而避免 System.Text.Json 的 `knownTypes` 与多态反序列化问题。每个子模型条目包含：

- `algorithm`：算法标识字符串（如 `naive-bayes` / `svm` / `random-forest` / `xgboost` / `ridge`）；
- `classes`：该基模型自身的类别顺序（用于 `ProbabilityMath.AlignTo` 对齐）；
- `state`：小型标量与枚举名（`scalars As Dictionary(Of String, Double)`、`integers As Dictionary(Of String, Integer())`、`vectors As Dictionary(Of String, Double())`、`text As Dictionary(Of String, String)`）；
- `data`：逻辑名 → 相对 CSV 路径（`Dictionary(Of String, String)`）。

### 3. 编解码器注册表

`IModelCodec` 以 `ModelKind` 为键注册到静态表，提供 `CanHandle(model)`、`SaveClassifier/SaveRegressor`、`LoadClassifier/LoadRegressor`。保存时按 `CanHandle` 匹配具体适配器类型；加载时按清单的 `algorithm` 精确查表，未注册则抛出列出全部支持标识的异常（拒绝静默降级）。9 个算法族按语义合并为 5 个 codec 文件，避免文件碎片化。

### 4. 严格恢复：底层库补齐点（仅新增，不修改现有公开行为）

| 类型 | 缺口 | 补齐方式 |
| --- | --- | --- |
| `NaiveBayesModel` | 字段全为 `Protected ReadOnly` | 新增 `Protected Sub New(classes, logPriors, classCounts, width)` 恢复构造（`_features/_labels/_classIndex` 置空数组，预测路径不使用） |
| `Bayesian` | 私有构造 + 私有 `_tables/_distinct` | 新增 `Public Shared Function Restore(...)` 工厂 + `Tables` / `DistinctCounts` 只读访问器 |
| `GaussianBayes` | 私有构造 | 新增 `Public Shared Function Restore(classes, logPriors, classCounts, width, means, variances, varianceFloor)`；`Means/Variances` 已有只读访问器 |
| `TargetBinning` | 私有构造 | 新增 `Public Shared Function Restore(minimum, maximum, interval, binCount, representatives)` |
| `BayesianRegression` | 全私有字段、`_representatives` 无访问器 | 新增 `Restore(mode, bins, smoothing, varianceFloor, binning, classes, representatives, posterior)` + `Classes` / `Representatives` 只读属性 |
| `LassoFit` | 无无参构造 | 新增 `Public Shared Function Restore(numberOfLambdas, numFeatures, intercepts, compressedWeights, indices, numberOfWeights, nonZeroWeights, lambdas, rsquared, featureNames)` 工厂（私有构造直接赋值，保留原压缩布局） |


无需修改的库：`SVM.Model`（已有 `SvmModelJSON` DTO）、`LogisticFit`、`RidgeFit`、`MLRFit`、`RanFog`（公开可写属性即可重建）、`xgboost`（`train.ModelSerializer.save_model/load_model` 已是 public 且用 `"R"` + `InvariantCulture` 保证往返精度）。

### 5. Ensemble 层重构（不改变既有公开行为）

- `Ensemble/Core/EnsembleValidation.vb`：`ValidateTrainingData(x, y, context)`、`ValidateFeatureVector(x, expectedWidth, context)`、`EnsureFinite(value, context)`、`Median(values)`，替换 6 处重复校验、4 处近似批量预测中的零散检查与 1 处 `Median` 副本。
- `Ensemble/Core/EnsembleParallel.vb`：`FitAll(count, parallel, body)`，替换 6 处 `If Parallel ... Parallel.For ... Else For ...` 成对循环。
- **FeatureWidth**：新增 `IFeatureSchema` 接口（`ReadOnly Property FeatureWidth As Integer`），由 6 个集成器与全部适配器实现；`Train` 时记录 `x(0).Length`，构造恢复时由清单/底层模型推导，预测入口统一校验。**这是有意的行为收紧**，需在文档中说明。
- **持久化契约**：新增 `IPersistableModel`（`ModelKind` + `Save(directory) As ModelManifest`），6 个集成器实现之，`Save` 内部委托 `EnsembleModelStore`。
- **恢复构造**：`BaggingClassifier` / `BaggingRegressor` / `StackingClassifier` / `StackingRegressor` 新增「用已训练基模型直接构造」的重载（`VotingClassifier` / `VotingRegressor` 已有，追加可选 `featureWidth`）。
- `RequireTrained` 保留在各类中（判定条件不同），但异常消息统一经 `EnsembleValidation` 生成，包含模型类型与缺失对象名。

### 6. 性能与可靠性

- **CSV 写**：`StreamWriter`（缓冲）+ 每行 `StringBuilder`，单次遍历，时间 O(行 × 列)，额外内存 O(1 行)。随机森林 100 棵 × 2000 节点约 20 万行，一次顺序写即可完成。
- **CSV 读**：`StreamReader` 单遍解析（含引号状态机），先按行数预分配目标数组，避免 LIST 追加与二次拷贝。
- **大数组落 CSV，小数组留 JSON**：SVM 支持向量按稀疏三元组 `(supportVector, position, index, value)` 落 CSV（O(nnz)），避免整体 JSON 膨胀；GLM 系数矩阵按「行=特征/步、列=列号」落 CSV，加载时直接填满 `Double()()`。
- **浮点往返**：统一 `InvariantCulture` 与 `ToString("R")`；加载端 `Double.Parse(..., InvariantCulture)`，保证逐位等价（审计用 `BitConverter.DoubleToInt64Bits` 比对）。
- **错误处理与安全**：所有 CSV 路径必须是清单内的相对路径，加载时规范化后校验仍位于包目录内，防止路径穿越；文件缺失、行列数不符、版本不支持、类别非严格递增、概率和为 0 等全部抛出带文件名与期望/实际值的 `InvalidOperationException`/`ArgumentException`。
- **日志**：库内不引入日志依赖，全部通过异常上下文表达；审计程序用 `DebugAudit.Mark` 记录包体积与耗时。

## Agent Extensions

### SubAgent

- **code-explorer**
- Purpose: 在实现前复核 6 个待改底层模型的字段可见性、构造函数签名与调用点，确认新增工厂不会与既有 `Load`/`Train` 语义冲突。
- Expected outcome: 每处改动点都有准确的行号证据与可编译的签名草案，避免凭空编造 API。

### Skill

- **lsp-code-analysis**
- Purpose: 对 `Bayesian` / `BayesianRegression` / `LassoFit` / `GBM` 等做引用与实现查找，确认新增只读访问器与恢复构造的影响面为零（无同名成员、无接口实现冲突）。
- Expected outcome: 输出受影响的引用清单，保证底层改动为纯增量、不破坏现有调用方。