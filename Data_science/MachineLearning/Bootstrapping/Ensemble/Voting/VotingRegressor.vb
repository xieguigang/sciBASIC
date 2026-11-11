Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualBasic.MachineLearning.Bootstrapping.Ensemble.Persistence

Namespace Ensemble

    ''' <summary>
    ''' Voting 回归器：对各基回归器的预测值做加权平均。
    ''' </summary>
    ''' <remarks>
    ''' 回归没有「概率」概念，因此这里只有加权均值；若需要抗异常值的聚合，
    ''' 请使用 <see cref="BaggingRegressor"/> 的中位数聚合。
    ''' </remarks>
    Public NotInheritable Class VotingRegressor
        Implements IRegressor
        Implements IFeatureSchema
        Implements IPersistableModel

        Private ReadOnly learners As IRegressorLearner()
        Private ReadOnly modelWeights As Double()
        ''' <summary>调用方提供的原始权重（归一化之前）；<c>Nothing</c> 表示等权</summary>
        Private ReadOnly sourceWeights As Double()
        Private ReadOnly trained As IRegressor()
        Private featureWidthValue As Integer

        Public Sub New(learners As IEnumerable(Of IRegressorLearner),
                       Optional weights As Double() = Nothing)
            If learners Is Nothing Then
                Throw New ArgumentNullException(NameOf(learners))
            End If

            Dim items As IRegressorLearner() = learners.ToArray()

            If items.Length = 0 Then
                Throw New ArgumentException("the voting regressor requires at least one base learner!", NameOf(learners))
            End If
            If items.Any(Function(m) m Is Nothing) Then
                Throw New ArgumentException("the base learner list contains a NULL entry!", NameOf(learners))
            End If

            Me.learners = items
            Me.modelWeights = ProbabilityMath.NormalizeWeights(weights, items.Length)
            Me.sourceWeights = If(weights Is Nothing OrElse weights.Length = 0, Nothing, CType(weights.Clone(), Double()))
            Me.trained = New IRegressor(items.Length - 1) {}
        End Sub

        ''' <summary>
        ''' 用已经训练好的基模型直接构造集成器（用于从持久化模型恢复，不会再次训练）。
        ''' </summary>
        ''' <param name="trained">已训练的回归器，顺序与 <paramref name="weights"/> 对齐</param>
        ''' <param name="weights">非负权重，为空时等权；内部会归一化</param>
        ''' <param name="featureWidth">训练期的特征维度，<c>0</c> 表示未知（跳过维度校验）</param>
        Public Sub New(trained As IRegressor(),
                       Optional weights As Double() = Nothing,
                       Optional featureWidth As Integer = 0)
            If trained Is Nothing OrElse trained.Length = 0 Then
                Throw New ArgumentException("the trained regressor list can not be empty!", NameOf(trained))
            End If
            If trained.Any(Function(m) m Is Nothing) Then
                Throw New ArgumentException("the trained regressor list contains a NULL entry!", NameOf(trained))
            End If

            Me.learners = New IRegressorLearner() {}
            Me.modelWeights = ProbabilityMath.NormalizeWeights(weights, trained.Length)
            Me.sourceWeights = If(weights Is Nothing OrElse weights.Length = 0, Nothing, CType(weights.Clone(), Double()))
            Me.trained = CType(trained.Clone(), IRegressor())
            Me.featureWidthValue = System.Math.Max(0, featureWidth)
        End Sub

        ''' <summary>基模型数量</summary>
        Public ReadOnly Property BaseModelCount As Integer
            Get
                Return trained.Length
            End Get
        End Property

        ''' <summary>训练后的基模型</summary>
        Public ReadOnly Property BaseModels As IRegressor()
            Get
                Return trained
            End Get
        End Property

        ''' <summary>归一化后的聚合权重</summary>
        Public ReadOnly Property Weights As Double()
            Get
                Return modelWeights
            End Get
        End Property

        ''' <summary>
        ''' 调用方提供的原始权重（归一化之前）；<c>Nothing</c> 表示构造时未指定、按等权处理。
        ''' 模型持久化保存的正是这份原始值。
        ''' </summary>
        Public ReadOnly Property OriginalWeights As Double()
            Get
                Return sourceWeights
            End Get
        End Property

        ''' <summary>训练期的特征维度；<c>0</c> 表示未知</summary>
        Public ReadOnly Property FeatureWidth As Integer Implements IFeatureSchema.FeatureWidth
            Get
                If featureWidthValue > 0 Then
                    Return featureWidthValue
                End If

                For i As Integer = 0 To trained.Length - 1
                    Dim schema As IFeatureSchema = TryCast(trained(i), IFeatureSchema)

                    If schema IsNot Nothing AndAlso schema.FeatureWidth > 0 Then
                        Return schema.FeatureWidth
                    End If
                Next

                Return 0
            End Get
        End Property

        ''' <summary>是否并行训练基模型（默认关闭，避免与算法库内部并行嵌套）</summary>
        Public Property Parallel As Boolean = False

        ''' <summary>训练全部基模型</summary>
        Public Function Train(x As Double()(), y As Double()) As VotingRegressor
            Call EnsembleValidation.ValidateTrainingData(x, y, "VotingRegressor.Train")

            Call EnsembleParallel.FitAll(trained.Length, Parallel,
                Sub(i)
                    trained(i) = learners(i).Fit(x, y)
                End Sub)

            featureWidthValue = x(0).Length

            Return Me
        End Function

        Public Function Predict(x As Double()) As Double Implements IRegressor.Predict
            Call RequireTrained()
            Call x.ValidateFeatureVector(FeatureWidth, "VotingRegressor.Predict")

            Dim value As Double = 0.0R

            For i As Integer = 0 To trained.Length - 1
                Dim part As Double = EnsembleValidation.EnsureFinite(
                    trained(i).Predict(x), DescribeBase(i))

                value += modelWeights(i) * part
            Next

            Return value
        End Function

        ''' <summary>批量预测（<see cref="Parallel"/> 打开时按样本维并行）</summary>
        Public Function PredictBatch(x As Double()()) As Double()
            Call RequireTrained()
            Call x.ValidateFeatureMatrix(FeatureWidth, "VotingRegressor.PredictBatch")

            Dim result As Double() = New Double(x.Length - 1) {}

            Call EnsembleParallel.PredictAll(x.Length, Parallel,
                Sub(i)
                    result(i) = Predict(x(i))
                End Sub)

            Return result
        End Function

        Public ReadOnly Property ModelKind As String Implements IPersistableModel.ModelKind
            Get
                Return "voting"
            End Get
        End Property

        Public ReadOnly Property ModelCategory As String Implements IPersistableModel.ModelCategory
            Get
                Return "regressor"
            End Get
        End Property

        Public Function SaveModel(bundle As ModelBundle, folder As String) As ModelManifest Implements IPersistableModel.SaveModel
            Call RequireTrained()

            Dim manifest As ModelManifest = ModelManifest.Create(ModelCategory, ModelKind, "")

            manifest.featureWidth = FeatureWidth
            manifest.SetInteger("modelCount", trained.Length)

            ' 与 VotingClassifier 一致：保存归一化之前的原始权重
            If sourceWeights IsNot Nothing AndAlso sourceWeights.Length > 0 Then
                manifest.SetVector("weights", sourceWeights)
            End If

            For i As Integer = 0 To trained.Length - 1
                Dim childFolder As String = ModelBundle.CombineRelative(folder, $"base/{i}")

                Call manifest.children.Add(EnsembleModelStore.SaveChild(trained(i), bundle, childFolder))
            Next

            Return manifest
        End Function

        Public Function Save(directory As String) As ModelManifest Implements IPersistableModel.Save
            Return EnsembleModelStore.Save(Me, directory)
        End Function

        Private Function DescribeBase(index As Integer) As String
            If learners IsNot Nothing AndAlso learners.Length > index AndAlso learners(index) IsNot Nothing Then
                Return $"the base regressor '{learners(index).Name}' (index {index})"
            End If

            Return $"the base regressor at index {index}"
        End Function

        Private Sub RequireTrained()
            Call EnsembleValidation.RequireTrained(
                trained.Length > 0 AndAlso trained(0) IsNot Nothing, NameOf(VotingRegressor), "trained(base[0])")
        End Sub

        Public Overrides Function ToString() As String
            Return $"VotingRegressor: {If(trained, New IRegressor() {}).Length} base model(s)."
        End Function

    End Class

End Namespace
