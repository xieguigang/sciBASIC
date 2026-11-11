Imports System
Imports Microsoft.VisualBasic.MachineLearning.Bootstrapping.Ensemble.Persistence

Namespace Ensemble

    ''' <summary>
    ''' Bagging 回归器：对训练集做有放回采样训练多个同质基学习器，
    ''' 再用均值或中位数聚合预测值。
    ''' </summary>
    ''' <remarks>
    ''' 中位数聚合对长尾的基模型误差更稳健，适合存在明显异常样本的场景；
    ''' 均值聚合方差更小，是默认选择。
    ''' </remarks>
    Public NotInheritable Class BaggingRegressor
        Implements IRegressor
        Implements IFeatureSchema
        Implements IPersistableModel

        Private ReadOnly learner As IRegressorLearner
        Private ReadOnly modelCount As Integer
        Private ReadOnly ratio As Double
        Private ReadOnly seed As Integer
        Private ReadOnly trained As IRegressor()
        Private featureWidthValue As Integer
        Private aggregationMode As BaggingAggregation = BaggingAggregation.Mean

        Public Sub New(learner As IRegressorLearner,
                       Optional modelCount As Integer = 10,
                       Optional ratio As Double = 1.0R,
                       Optional seed As Integer = 42)
            If learner Is Nothing Then
                Throw New ArgumentNullException(NameOf(learner))
            End If
            If modelCount < 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(modelCount), $"the model count must be positive, but got {modelCount}!")
            End If
            If ratio <= 0.0R OrElse ratio > 2.0R Then
                Throw New ArgumentOutOfRangeException(
                    NameOf(ratio), $"the sample ratio must be within (0, 2], but got {ratio}!")
            End If

            Me.learner = learner
            Me.modelCount = modelCount
            Me.ratio = ratio
            Me.seed = seed
            Me.trained = New IRegressor(modelCount - 1) {}
        End Sub

        ''' <summary>
        ''' 用已经训练好的基模型直接构造集成器（用于从持久化模型恢复，不会再次训练）。
        ''' </summary>
        ''' <param name="trained">已训练的回归器，顺序即基模型下标</param>
        ''' <param name="aggregation">聚合方式</param>
        ''' <param name="ratio">训练期的采样比例（仅用于诊断输出）</param>
        ''' <param name="seed">训练期的随机种子（仅用于诊断输出）</param>
        ''' <param name="featureWidth">训练期的特征维度，<c>0</c> 表示未知（跳过维度校验）</param>
        Public Sub New(trained As IRegressor(),
                       Optional aggregation As BaggingAggregation = BaggingAggregation.Mean,
                       Optional ratio As Double = 1.0R,
                       Optional seed As Integer = 42,
                       Optional featureWidth As Integer = 0)
            If trained Is Nothing OrElse trained.Length = 0 Then
                Throw New ArgumentException("the trained regressor list can not be empty!", NameOf(trained))
            End If

            For i As Integer = 0 To trained.Length - 1
                If trained(i) Is Nothing Then
                    Throw New ArgumentException("the trained regressor list contains a NULL entry!", NameOf(trained))
                End If
            Next

            Me.learner = Nothing
            Me.modelCount = trained.Length
            Me.ratio = ratio
            Me.seed = seed
            Me.trained = CType(trained.Clone(), IRegressor())
            Me.aggregationMode = aggregation
            Me.featureWidthValue = System.Math.Max(0, featureWidth)
        End Sub

        Public ReadOnly Property BaseModelCount As Integer
            Get
                Return trained.Length
            End Get
        End Property

        Public ReadOnly Property BaseModels As IRegressor()
            Get
                Return trained
            End Get
        End Property

        Public ReadOnly Property SampleRatio As Double
            Get
                Return ratio
            End Get
        End Property

        ''' <summary>训练使用的随机种子（从模型包恢复后仍保留原值）</summary>
        Public ReadOnly Property RandomSeed As Integer
            Get
                Return seed
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

        ''' <summary>聚合方式</summary>
        Public Property Aggregation As BaggingAggregation
            Get
                Return aggregationMode
            End Get
            Set(value As BaggingAggregation)
                aggregationMode = value
            End Set
        End Property

        ''' <summary>是否并行训练基模型（默认关闭，避免与算法库内部并行嵌套）</summary>
        Public Property Parallel As Boolean = False

        Public Function Train(x As Double()(), y As Double()) As BaggingRegressor
            Call EnsembleValidation.ValidateTrainingData(x, y, "BaggingRegressor.Train")

            Dim n As Integer = x.Length
            Dim size As Integer = System.Math.Max(1, CInt(System.Math.Round(n * ratio)))

            Call EnsembleParallel.FitAll(trained.Length, Parallel,
                Sub(m)
                    trained(m) = FitOne(x, y, size, m)
                End Sub)

            featureWidthValue = x(0).Length

            Return Me
        End Function

        Private Function FitOne(x As Double()(), y As Double(),
                                size As Integer, modelIndex As Integer) As IRegressor
            Dim sampler As New BootstrapSampler(seed + modelIndex)
            Dim index As Integer() = sampler.Sample(size)
            Dim xSub As Double()() = BootstrapSampler.Resample(x, index)
            Dim ySub As Double() = BootstrapSampler.Resample(y, index)

            Return learner.Fit(xSub, ySub)
        End Function

        Public Function Predict(x As Double()) As Double Implements IRegressor.Predict
            Call RequireTrained()
            Call x.ValidateFeatureVector(FeatureWidth, "BaggingRegressor.Predict")

            Dim values As Double() = New Double(trained.Length - 1) {}

            For m As Integer = 0 To trained.Length - 1
                Dim model As IRegressor = trained(m)

                If model Is Nothing Then
                    Throw New InvalidOperationException($"the bagging base model at index {m} is missing!")
                End If

                values(m) = EnsembleValidation.EnsureFinite(
                    model.Predict(x), $"the bagging base model at index {m}")
            Next

            If aggregationMode = BaggingAggregation.Median Then
                Return EnsembleValidation.Median(values)
            End If

            Dim total As Double = 0.0R

            For m As Integer = 0 To values.Length - 1
                total += values(m)
            Next

            Return total / values.Length
        End Function

        ''' <summary>批量预测</summary>
        Public Function PredictBatch(x As Double()()) As Double()
            Call RequireTrained()
            Call x.ValidateFeatureMatrix(FeatureWidth, "BaggingRegressor.PredictBatch")

            Dim result As Double() = New Double(x.Length - 1) {}

            Call EnsembleParallel.PredictAll(x.Length, Parallel,
                Sub(i)
                    result(i) = Predict(x(i))
                End Sub)

            Return result
        End Function

        Public ReadOnly Property ModelKind As String Implements IPersistableModel.ModelKind
            Get
                Return "bagging"
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
            manifest.SetScalar("sampleRatio", ratio)
            manifest.SetInteger("seed", seed)
            manifest.SetInteger("modelCount", trained.Length)
            manifest.SetParameter("aggregation", aggregationMode.ToString())

            For m As Integer = 0 To trained.Length - 1
                Dim childFolder As String = ModelBundle.CombineRelative(folder, $"base/{m}")

                Call manifest.children.Add(EnsembleModelStore.SaveChild(trained(m), bundle, childFolder))
            Next

            Return manifest
        End Function

        Public Function Save(directory As String) As ModelManifest Implements IPersistableModel.Save
            Return EnsembleModelStore.Save(Me, directory)
        End Function

        Private Sub RequireTrained()
            Call EnsembleValidation.RequireTrained(
                trained.Length > 0 AndAlso trained(0) IsNot Nothing, NameOf(BaggingRegressor), "trained(base[0])")
        End Sub

        Public Overrides Function ToString() As String
            Return $"BaggingRegressor({aggregationMode}): {trained.Length} base model(s), ratio={ratio}, seed={seed}."
        End Function

    End Class

End Namespace
