#Region "Microsoft.VisualBasic::XGBoostShapModel, MachineLearning\ML_SHAP\Models\XGBoostShapModel.vb"

Imports System.Linq
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.DataMining.Evaluation
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.DataSet
Imports Microsoft.VisualBasic.Math.Statistics.ShapleyValue
Imports Microsoft.VisualBasic.Math.Statistics.ShapleyValue.TreeShap
Imports xgb = Microsoft.VisualBasic.MachineLearning.XGBoost.train

''' <summary>
''' XGBoost / TGBoost 模型的 SHAP 适配器。
''' 
''' 该适配器把 <see cref="xgb.GBM"/> 训练得到的每一棵回归树转换为
''' <see cref="PkTree"/>：叶值乘以学习率 ``eta``，并叠加模型的
''' ``first_round_pred`` 作为基线偏置项，从而计算精确的 TreeSHAP 贡献。
''' </summary>
Public Class XGBoostShapModel

    Public Property Model As xgb.GBM
    Public Property Training As ShapDataset
    Public Property IsClassification As Boolean

    Public ReadOnly Property Name As String
        Get
            Return "XGBoost"
        End Get
    End Property

    ''' <summary>
    ''' 在给定的数据集之上训练一个梯度提升树模型。
    ''' </summary>
    ''' <param name="dataset">训练数据集</param>
    ''' <param name="isClassification">是否进行分类任务</param>
    ''' <param name="rounds">boosting 轮数</param>
    ''' <param name="maxDepth">单棵树的最大深度</param>
    ''' <param name="eta">学习率</param>
    ''' <returns></returns>
    Public Shared Function Train(dataset As ShapDataset,
                                 isClassification As Boolean,
                                 Optional rounds As Integer = 20,
                                 Optional maxDepth As Integer = 4,
                                 Optional eta As Double = 0.3) As XGBoostShapModel

        Dim matrix As DoubleTagged(Of Single())() = dataset.Features _
            .Select(Function(f, i) New DoubleTagged(Of Single())(dataset.Labels(i), f.Select(Function(d) CSng(d)).ToArray)) _
            .ToArray

        ' 所有特征均按数值特征处理（该数据集不包含类别型特征，也不含缺失值）
        Dim trainData As xgb.TrainData = matrix.ToTrainingSet(dataset.FeatureNames, New String() {})
        Dim gbm As New xgb.GBM()

        If isClassification Then
            Call gbm.fit(trainset:=trainData,
                         valset:=Nothing,
                         maximize:=True,
                         eval_metric:=Metrics.auc,
                         loss:="logloss",
                         eta:=eta,
                         num_boost_round:=rounds,
                         max_depth:=maxDepth)
        Else
            Call gbm.fit(trainset:=trainData,
                         valset:=Nothing,
                         maximize:=False,
                         eval_metric:=Metrics.mse,
                         loss:="squareloss",
                         eta:=eta,
                         num_boost_round:=rounds,
                         max_depth:=maxDepth)
        End If

        Return New XGBoostShapModel With {
            .Model = gbm,
            .Training = dataset,
            .IsClassification = isClassification
        }
    End Function

    ''' <summary>
    ''' 对新样本进行预测。分类任务返回类别概率，回归任务返回回归值。
    ''' </summary>
    ''' <param name="features"></param>
    ''' <returns></returns>
    Public Function Predict(features As Double()()) As Double()
        Dim singleFeatures As Single()() = features _
            .Select(Function(row) row.Select(Function(d) CSng(d)).ToArray()) _
            .ToArray

        Return Model.predict(singleFeatures)
    End Function

    ''' <summary>
    ''' 模型在单个样本上的原始输出。
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    Public Function Output(x As Double()) As Double
        Return Predict(New Double()() {x})(0)
    End Function

    Public Function PredictResult() As ModelPrediction
        Return New ModelPrediction With {
            .ModelName = Name,
            .DatasetName = Training.Name,
            .IsClassification = IsClassification,
            .IDs = Training.IDs,
            .Labels = Training.Labels,
            .Predictions = Predict(Training.Features)
        }
    End Function

    ''' <summary>
    ''' 构建梯度提升树的 TreeSHAP 解释器。
    ''' </summary>
    ''' <returns></returns>
    Public Function BuildExplainer() As IShapExplainer
        Dim eta As Double = Model.eta
        Dim trees As New List(Of PkTree)()

        For Each tree As xgb.Tree In Model.trees
            If tree Is Nothing OrElse tree.root Is Nothing Then
                Continue For
            End If

            trees.Add(New PkTree(BuildNode(tree.root, eta)))
        Next

        Return New TreeShapExplainer(trees, Training.FeatureNames, Nothing, Model.first_round_pred)
    End Function

    ''' <summary>
    ''' 计算节点的 cover（落入该子树的样本数量）。
    ''' </summary>
    ''' <remarks>
    ''' TGBoost 只会为非叶节点累计 ``num_sample``；对于根节点以及直接成为叶节点的
    ''' 情况这里使用子节点 cover 之和作为回退，从而保证 TreeSHAP 的分数权重定义良好。
    ''' </remarks>
    Private Shared Function Cover(node As xgb.TreeNode) As Double
        If node Is Nothing Then
            Return 0
        End If

        If node.is_leaf Then
            Return System.Math.Max(1.0, node.num_sample)
        End If

        Dim count As Double = node.num_sample

        If count <= 0 Then
            count = Cover(node.left_child) + Cover(node.right_child)
        End If

        If count > 0 Then
            Return count
        Else
            Return 1.0
        End If
    End Function

    Private Shared Function BuildNode(node As xgb.TreeNode, eta As Double) As PkNode
        If node Is Nothing Then
            Return PkNode.leaf(1, 0)
        End If

        Dim nodeCover As Double = Cover(node)

        If node.is_leaf Then
            Return PkNode.leaf(nodeCover, node.leafValue * eta)
        End If

        Dim left As xgb.TreeNode = node.left_child
        Dim right As xgb.TreeNode = node.right_child

        If left Is Nothing OrElse right Is Nothing Then
            ' 仅含缺失值分支或者结构异常的节点，回退为叶节点
            Return PkNode.leaf(nodeCover, 0)
        End If

        Return PkNode.split(nodeCover,
                            node.split_feature,
                            node.split_threshold,
                            BuildNode(left, eta),
                            BuildNode(right, eta))
    End Function

End Class

#End Region
