# Data_science/MachineLearning/ML_SHAP/ML_SHAP.vbproj

- RootNamespace : Microsoft.VisualBasic.MachineLearning.SHAP
- AssemblyName  : Microsoft.VisualBasic.MachineLearning.SHAPAnalysis
- TargetFramework: net10.0
- Source files  : 8
- Existing Title: SHAP Model Interpretation and Feature Importance Analysis
- Existing Desc : A SHAP (SHapley Additive exPlanations) analysis facade for sciBASIC# machine learning: runs linear, random forest and XGBoost explainers over sample datasets to produce per-sample contribution matrices, global feature importance rankings and CSV report export.
- Existing Tags : scibasic;shap;shapley-value;model-interpretation;explainable-ai;feature-importance

## Namespaces
- (no explicit Namespace statement; every type lives directly under the RootNamespace Microsoft.VisualBasic.MachineLearning.SHAP)

## Public types
- Class ShapDataset (Data\ShapDataset.vb) - 用于 SHAP 分析的数值数据集。 从空格（或制表符）分隔的文本文件之中加载，文件格式为： ```
- Class ModelPrediction (Evaluation\ModelPrediction.vb) - 一个模型在一个数据集之上的预测结果与常用的评估指标。
- Class LinearShapModel (Models\LinearShapModel.vb) - 线性模型（多元线性回归）/ 广义线性模型（逻辑回归）的 SHAP 适配器。 分类任务使用逻辑回归，回归任务使用多元线性回归（MLR）。 两者的 SHAP 值都可以通过解析公式 ``phi_i = bi * (xi - E[xi])`` 精确计算。
- Class RanFogShapModel (Models\RanFogShapModel.vb) - 随机森林（RanFog）模型的 SHAP 适配器。 该适配器把 RanFog 训练之后保留下来的树结构（<see cref="rf.RanFog.Trees"/>） 转换为 TreeSHAP 所需的 <see cref="PkTree"/>，从而可以计算精确的
- Class XGBoostShapModel (Models\XGBoostShapModel.vb) - XGBoost / TGBoost 模型的 SHAP 适配器。 该适配器把 <see cref="xgb.GBM"/> 训练得到的每一棵回归树转换为 <see cref="PkTree"/>：叶值乘以学习率 ``eta``，并叠加模型的
- Class NamespaceDoc (NamespaceDoc.vb) - SHAP (SHapley Additive exPlanations) analysis for interpreting machine learning model predictions.
- Module ShapCsvWriter (Report\ShapCsvWriter.vb) - 把 SHAP 分析结果导出为 CSV 文件。
- Class ShapAnalysisResult (ShapAnalyzer.vb) - 一个模型在一个数据集之上的整体 SHAP 分析结果。
- Module ShapAnalyzer (ShapAnalyzer.vb) - SHAP 分析的门面：把任意 <see cref="IShapExplainer"/> 与数据集组合起来， 计算逐样本的 SHAP 矩阵以及全局特征重要性。

## Notable public members
- Public Property Name As String
- Public Property IsCategorical As Boolean
- Public Property IDs As String()
- Public Property Labels As Double()
- Public Property Features As Double()()
- Public Property FeatureNames As String()
- Public ReadOnly Property Size As Integer
- Public ReadOnly Property Width As Integer
- Public ReadOnly Property ClassLabels As Integer()
- Public Shared Function LoadFile(path As String, isCategorical As Boolean, Optional name As String = Nothing) As ShapDataset
- Public Shared Function Parse(lines As IEnumerable(Of String), isCategorical As Boolean, Optional name As String = Nothing) As ShapDataset
- Public Property ModelName As String
- Public Property DatasetName As String
- Public Property IsClassification As Boolean
- Public Property IDs As String()
- Public Property Labels As Double()
- Public Property Predictions As Double()
- Public ReadOnly Property Size As Integer
- Public ReadOnly Property MSE As Double
- Public ReadOnly Property RMSE As Double
- Public ReadOnly Property R2 As Double
- Public ReadOnly Property Accuracy As Double
- Public Overrides Function ToString() As String
- Public Property Fitted As IFitted
- Public Property Coefficients As Double()
- Public Property Intercept As Double
- Public Property Link As Func(Of Double, Double)
- Public Property Training As ShapDataset
- Public Property IsClassification As Boolean
- Public ReadOnly Property Name As String
- Public Shared Function Train(dataset As ShapDataset,
- Public Function Predict(features As Double()()) As Double()
- Public Function Output(x As Double()) As Double
- Public Function PredictResult() As ModelPrediction
- Public Function BuildExplainer() As IShapExplainer
- Public Property Forest As rf.RanFog
- Public Property Training As ShapDataset
- Public Property IsClassification As Boolean
- Public ReadOnly Property Name As String
- Public Shared Function Train(dataset As ShapDataset,
- Public Function Predict(features As Double()()) As Double()
- Public Function Output(x As Double()) As Double
- Public Function PredictResult() As ModelPrediction
- Public Function BuildExplainer() As IShapExplainer
- Public Property Model As xgb.GBM
- Public Property Training As ShapDataset
- Public Property IsClassification As Boolean
- Public ReadOnly Property Name As String
- Public Shared Function Train(dataset As ShapDataset,
- Public Function Predict(features As Double()()) As Double()
- Public Function Output(x As Double()) As Double
- Public Function PredictResult() As ModelPrediction
- Public Function BuildExplainer() As IShapExplainer
- Public Sub WriteExplanations(result As ShapAnalysisResult, path As String)
- Public Sub WriteGlobalImportance(result As ShapAnalysisResult, path As String)
- Public Property ModelName As String
- Public Property DatasetName As String
- Public Property IsClassification As Boolean
- Public Property FeatureNames As String()
- Public Property Baseline As Double
- ... and 5 more

## Imports
- Microsoft.VisualBasic.ComponentModel.TagData
- Microsoft.VisualBasic.Data
- Microsoft.VisualBasic.Data.Bootstrapping
- Microsoft.VisualBasic.Data.Bootstrapping.Logistic
- Microsoft.VisualBasic.Data.Bootstrapping.Multivariate
- Microsoft.VisualBasic.DataMining.Evaluation
- Microsoft.VisualBasic.MachineLearning.XGBoost.DataSet
- Microsoft.VisualBasic.Math.Statistics.ShapleyValue
- Microsoft.VisualBasic.Math.Statistics.ShapleyValue.TreeShap
- rf = Microsoft.VisualBasic.MachineLearning.RandomForests
- System.Globalization
- System.IO
- System.Linq
- System.Text
- xgb = Microsoft.VisualBasic.MachineLearning.XGBoost.train

## File tree
- Data\ShapDataset.vb
- Evaluation\ModelPrediction.vb
- Models\LinearShapModel.vb
- Models\RanFogShapModel.vb
- Models\XGBoostShapModel.vb
- NamespaceDoc.vb
- Report\ShapCsvWriter.vb
- ShapAnalyzer.vb

