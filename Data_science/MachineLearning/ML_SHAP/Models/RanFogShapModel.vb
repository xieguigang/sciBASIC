#Region "Microsoft.VisualBasic::RanFogShapModel, MachineLearning\ML_SHAP\Models\RanFogShapModel.vb"

Imports System.Linq
Imports Microsoft.VisualBasic.Math.Statistics.ShapleyValue
Imports Microsoft.VisualBasic.Math.Statistics.ShapleyValue.TreeShap
Imports rf = Microsoft.VisualBasic.MachineLearning.RandomForests

''' <summary>
''' 随机森林（RanFog）模型的 SHAP 适配器。
''' 
''' 该适配器把 RanFog 训练之后保留下来的树结构（<see cref="rf.RanFog.Trees"/>）
''' 转换为 TreeSHAP 所需的 <see cref="PkTree"/>，从而可以计算精确的
''' 森林 SHAP 值。同时它也提供对新样本的预测能力。
''' </summary>
Public Class RanFogShapModel

    ''' <summary>
    ''' 训练好的随机森林模型。
    ''' </summary>
    ''' <returns></returns>
    Public Property Forest As rf.RanFog

    ''' <summary>
    ''' 训练数据集。
    ''' </summary>
    ''' <returns></returns>
    Public Property Training As ShapDataset

    Public Property IsClassification As Boolean

    Public ReadOnly Property Name As String
        Get
            Return "RandomForest"
        End Get
    End Property

    ''' <summary>
    ''' 在给定的数据集之上训练一个随机森林。
    ''' </summary>
    ''' <param name="dataset">训练数据集</param>
    ''' <param name="isClassification">是否进行分类任务</param>
    ''' <param name="maxTrees">树的数量，默认 30（原始缺省值为 500，在小样本演示场景下过慢）</param>
    ''' <param name="maxBranch">单棵树的最大分支数量</param>
    ''' <param name="mtry">
    ''' 每个节点随机选择的特征数量，小于等于 0 时使用 ``sqrt(特征维度)``
    ''' 作为缺省值（自助采样聚合的经典取值）
    ''' </param>
    ''' <returns></returns>
    Public Shared Function Train(dataset As ShapDataset,
                                 isClassification As Boolean,
                                 Optional maxTrees As Integer = 30,
                                 Optional maxBranch As Integer = 256,
                                 Optional mtry As Integer = 0) As RanFogShapModel

        Dim treeCount As Integer = If(maxTrees <= 0, 30, maxTrees)
        Dim branchLimit As Integer = If(maxBranch <= 0, 256, maxBranch)
        Dim mtrySize As Integer = If(mtry <= 0, System.Math.Max(1, CInt(System.Math.Sqrt(dataset.Width))), mtry)

        Dim forest As New rf.RanFog()
        Dim trainingData As New rf.Data()

        forest.max_tree = treeCount
        forest.max_branch = branchLimit
        forest.mtry = mtrySize
        ' 对 0/1 目标而言，方差下降等价于基尼/信息增益的划分准则
        forest.LF_c = rf.LF_c.Mean_Squared_Error

        trainingData.ID = dataset.IDs
        trainingData.phenotype = dataset.Labels
        trainingData.Genotype = dataset.Features
        trainingData.attributeNames = dataset.FeatureNames

        Call forest.Run(trainingData)

        Return New RanFogShapModel With {
            .Forest = forest,
            .Training = dataset,
            .IsClassification = isClassification
        }
    End Function

    ''' <summary>
    ''' 对新样本进行预测。分类任务返回正类概率，回归任务返回回归值。
    ''' </summary>
    ''' <param name="features"></param>
    ''' <returns></returns>
    Public Function Predict(features As Double()()) As Double()
        Return Forest.Predict(features)
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
    ''' 构建随机森林的 TreeSHAP 解释器（多棵树贡献求和）。
    ''' </summary>
    ''' <remarks>
    ''' <see cref="rf.RanFog.Predict"/> 返回的是所有树叶值的平均值，
    ''' 而 TreeSHAP 的贡献是多棵树相加，因此这里必须把每一棵树的叶值
    ''' 按照 ``1/树的数量`` 进行缩放，才能保证
    ''' ``sum(contributions) + baseline = 森林预测值`` 成立。
    ''' </remarks>
    ''' <returns></returns>
    Public Function BuildExplainer() As IShapExplainer
        Dim trees As New List(Of PkTree)()
        Dim globalMean As Double = If(Training.Labels.Length > 0, Training.Labels.Average(), 0)
        Dim scale As Double = 1.0 / System.Math.Max(1, Forest.Trees.Count)

        For Each snapshot As rf.Branch() In Forest.Trees
            If snapshot Is Nothing OrElse snapshot.Length = 0 Then
                Continue For
            End If

            Dim tree As PkNode = BuildNode(snapshot, 0, globalMean, scale, New HashSet(Of Integer)())
            trees.Add(New PkTree(tree))
        Next

        Return New TreeShapExplainer(trees, Training.FeatureNames)
    End Function

    ''' <summary>
    ''' 把 RanFog 的分支数组递归地转换为 <see cref="PkNode"/>。
    ''' </summary>
    ''' <remarks>
    ''' RanFog 在均值处进行二分，因此当某一个子节点为空样本集合的时候，
    ''' 该分裂是没有意义的（并且会导致 TreeSHAP 的路径权重出现除零），
    ''' 此时会将该节点直接坍缩为叶节点。
    ''' </remarks>
    Private Shared Function BuildNode(branches As rf.Branch(),
                                      index As Integer,
                                      fallback As Double,
                                      scale As Double,
                                      visited As HashSet(Of Integer)) As PkNode

        If index < 0 OrElse index >= branches.Length Then
            Return PkNode.leaf(1, fallback * scale)
        End If

        If Not visited.Add(index) Then
            ' 防御性处理：避免畸形结构导致的无限递归
            Return PkNode.leaf(1, fallback * scale)
        End If

        Dim node As rf.Branch = branches(index)
        Dim cover As Double = node.list.Count
        Dim ownMean As Double = If(cover > 0, node.mean, fallback)

        If node.status = "F" OrElse cover <= 0 Then
            Return PkNode.leaf(System.Math.Max(1.0, cover), ownMean * scale)
        End If

        Dim leftIndex As Integer = node.Child1
        Dim rightIndex As Integer = node.Child2

        If leftIndex < 0 OrElse leftIndex >= branches.Length OrElse
           rightIndex < 0 OrElse rightIndex >= branches.Length Then
            Return PkNode.leaf(System.Math.Max(1.0, cover), ownMean * scale)
        End If

        Dim leftNode As rf.Branch = branches(leftIndex)
        Dim rightNode As rf.Branch = branches(rightIndex)

        If leftNode.list.Count <= 0 OrElse rightNode.list.Count <= 0 Then
            Return PkNode.leaf(System.Math.Max(1.0, cover), ownMean * scale)
        End If

        Return PkNode.split(cover,
                            node.Feature,
                            node.mean_snp,
                            BuildNode(branches, leftIndex, ownMean, scale, visited),
                            BuildNode(branches, rightIndex, ownMean, scale, visited))
    End Function

End Class

#End Region
