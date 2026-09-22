# ML_SHAP：面向 sciBASIC# 的模型可解释性分析

## 一、为什么需要它

训练出一个精度不错的模型只是第一步。当模型交付给业务方时，被问得最多的问题往往是：**「为什么这个样本得到了这样的预测？」**

`ML_SHAP` 就是 `sciBASIC#` 机器学习栈中回答这个问题的门面层。它把底层的 **Shapley 值归因**（由 `Microsoft.VisualBasic.Math.Statistics.ShapleyValue` 提供）与数据集组合起来，产出一份可以直接进入报告的分析结果。

## 二、设计目标

- **与解释器解耦**：任何实现了 `IShapExplainer` 的解释器（包括 `ShapleyValue.TreeShap` 以及 XGBoost 集成）都可以直接接入，门面层不认识任何具体模型。
- **一次调用同时产出全局与局部**：既给出每个样本的贡献向量，也给出全局特征重要性排序。
- **可采样**：通过 `maxSamples` 限制参与解释的样本数量，便于在探索阶段快速出图。

## 三、架构与命名空间地图

| 命名空间 | 职责 |
|---|---|
| `Microsoft.VisualBasic.MachineLearning.SHAP` | 本包：分析门面 `ShapAnalyzer` 与结果模型 `ShapAnalysisResult` |
| `Microsoft.VisualBasic.Math.Statistics.ShapleyValue` | 归因算法：`IShapExplainer`、`ShapExplanation`、`ShapDataset`、`TreeShap` |
| `Microsoft.VisualBasic.MachineLearning.XGBoost` | 树模型集成，可提供 SHAP 解释器 |

本包只有两个公开类型，刻意保持「薄」：

- `ShapAnalysisResult` —— 分析结果模型；
- `Module ShapAnalyzer` —— 唯一的分析入口 `Analyze`。

## 四、关键类型与 API

### `ShapAnalysisResult`

一次「模型 × 数据集」SHAP 分析的完整结果：

| 成员 | 含义 |
|---|---|
| `ModelName` / `DatasetName` | 报告用的模型与数据集标识 |
| `IsClassification` | 回归还是分类任务（来自数据集） |
| `FeatureNames` | 特征名数组 |
| `Baseline` | 基线值 / 期望值（加性分解的起点） |
| `Explanations` | 逐样本解释结果 `List(Of ShapExplanation)` |
| `MeanAbsShap` | 每个特征的**平均绝对** SHAP 值 → 全局重要性 |
| `MeanShap` | 每个特征的**平均有符号** SHAP 值 → 影响方向 |
| `GlobalImportance()` | 按全局重要性降序返回 `(特征名, 重要性)` 序列 |

### `ShapAnalyzer.Analyze`

```vbnet
Public Function Analyze(explainer As IShapExplainer,
                        dataset As ShapDataset,
                        modelName As String,
                        Optional maxSamples As Integer = 0,
                        Optional output As Func(Of Double(), Double) = Nothing) As ShapAnalysisResult
```

参数说明：

- `explainer` —— 已经针对某个模型构建好的 SHAP 解释器；
- `dataset` —— 参与计算的样本集合（提供 `Size`、`Width`、`Features`、`IDs`、`FeatureNames`、`IsCategorical`、`Name`）；
- `modelName` —— 报告用的模型名；
- `maxSamples` —— 最多解释的样本数量，小于等于 `0` 表示解释全部样本；
- `output` —— 可选的模型原始输出函数（例如逻辑回归的类别概率）。为空时报告加性值 `baseline + Σ contribution`。

内部逻辑：逐样本调用 `explainer.Explain(x)` 取得贡献向量，累加得到绝对和与有符号和；循环结束后按实际解释的样本数取均值，从而同时得到 `MeanAbsShap` 与 `MeanShap`；每个样本的 `AdditiveValue` 由 `Baseline` 逐项累加贡献得到。

## 五、快速上手

```vbnet
Imports Microsoft.VisualBasic.MachineLearning.SHAP
Imports Microsoft.VisualBasic.Math.Statistics.ShapleyValue

' explainer 已经针对你的模型构建完毕
Dim result As ShapAnalysisResult =
    ShapAnalyzer.Analyze(explainer, dataset, "xgboost-model", maxSamples:=500)

' 全局特征重要性（降序）
For Each item In result.GlobalImportance()
    Console.WriteLine($"{item.name} : {item.importance}")
Next

' 单个样本的贡献
Dim one As ShapExplanation = result.Explanations(0)
Console.WriteLine($"baseline={one.Baseline}, additive={one.AdditiveValue}")
```

## 六、与 sciBASIC# 生态的关系

`ML_SHAP` 位于机器学习栈的「解释层」：向下依赖统计与归因算法包（`Math.Statistics`、`ShapleyValue`），并可与 `xgboost`、`MachineLearning` 中的模型配合；向上服务于报告与可视化——`MeanAbsShap` 可以直接交给 `DataPlot` / `Plots` 绘制特征重要性条形图。

## 七、实现要点

- **加性一致性**：`AdditiveValue` 恒等于 `Baseline + Σ(Contributions)`，便于与 `Output`（真实模型输出）对照，用于校验解释质量。
- **特征名兜底**：当 `FeatureNames` 为空或长度不足时，`GlobalImportance()` 自动使用 `F1`、`F2`… 作为占位名。
- **性能**：SHAP 的计算代价与「样本数 × 特征数 × 子集枚举」相关，`maxSamples` 是控制分析耗时的第一手段。

## 八、包信息

- Assembly：`Microsoft.VisualBasic.MachineLearning.SHAPAnalysis`
- TargetFramework：`net10.0`
- 依赖：`Microsoft.VisualBasic.Core`、`binarydata`、`DataMining`、`Math.Statistics`、`Math`、`DataFittings`、`MachineLearning`、`XGBoostDataSet`、`xgboost`
- 许可：GPL-3.0-or-later
