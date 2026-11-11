Imports System
Imports System.Linq
Imports Microsoft.VisualBasic.MachineLearning.Bootstrapping.Ensemble.Persistence

Namespace Ensemble

    ''' <summary>
    ''' Bagging 分类器：对训练集做有放回采样，训练多个同质基学习器，
    ''' 再把它们的概率向量取平均。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 每个基模型使用独立的 <c>BootstrapSampler(seed + index)</c>，因此
    ''' 增加模型数量不会改变已有模型的采样结果，训练完全可复现。
    ''' </para>
    ''' <para>
    ''' 某个 bootstrap 子集可能缺失部分类别，基模型的 <c>Classes</c> 会变短；
    ''' 聚合时统一通过 <c>ProbabilityMath.AlignTo</c> 对齐到全局类别顺序。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class BaggingClassifier
        Implements IClassifier
        Implements IFeatureSchema
        Implements IPersistableModel

        Private ReadOnly learner As IClassifierLearner
        Private ReadOnly modelCount As Integer
        Private ReadOnly ratio As Double
        Private ReadOnly seed As Integer
        Private ReadOnly trained As IClassifier()
        Private classesArray As Double()
        Private featureWidthValue As Integer

        ''' <param name="learner">同质基学习器</param>
        ''' <param name="modelCount">基模型数量</param>
        ''' <param name="ratio">每个子集的采样比例（相对样本数）</param>
        ''' <param name="seed">随机种子</param>
        Public Sub New(learner As IClassifierLearner,
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
            Me.trained = New IClassifier(modelCount - 1) {}
        End Sub

        ''' <summary>
        ''' 用已经训练好的基模型直接构造集成器（用于从持久化模型恢复，不会再次训练）。
        ''' </summary>
        ''' <param name="trained">已训练的分类器，顺序即基模型下标</param>
        ''' <param name="classes">全局类别顺序</param>
        ''' <param name="ratio">训练期的采样比例（仅用于诊断输出）</param>
        ''' <param name="seed">训练期的随机种子（仅用于诊断输出）</param>
        ''' <param name="featureWidth">训练期的特征维度，<c>0</c> 表示未知（跳过维度校验）</param>
        Public Sub New(trained As IClassifier(),
                       classes As Double(),
                       Optional ratio As Double = 1.0R,
                       Optional seed As Integer = 42,
                       Optional featureWidth As Integer = 0)
            If trained Is Nothing OrElse trained.Length = 0 Then
                Throw New ArgumentException("the trained classifier list can not be empty!", NameOf(trained))
            End If
            If trained.Any(Function(m) m Is Nothing) Then
                Throw New ArgumentException("the trained classifier list contains a NULL entry!", NameOf(trained))
            End If
            If classes Is Nothing OrElse classes.Length = 0 Then
                Throw New ArgumentException("the classes array can not be empty!", NameOf(classes))
            End If

            Me.learner = Nothing
            Me.modelCount = trained.Length
            Me.ratio = ratio
            Me.seed = seed
            Me.trained = CType(trained.Clone(), IClassifier())
            Me.classesArray = CType(classes.Clone(), Double())
            Me.featureWidthValue = System.Math.Max(0, featureWidth)
        End Sub

        Public ReadOnly Property Classes As Double() Implements IClassifier.Classes
            Get
                Return classesArray
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

        Public ReadOnly Property BaseModelCount As Integer
            Get
                Return trained.Length
            End Get
        End Property

        Public ReadOnly Property BaseModels As IClassifier()
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

        ''' <summary>是否并行训练基模型（默认关闭，避免与算法库内部并行嵌套）</summary>
        Public Property Parallel As Boolean = False

        Public Function Train(x As Double()(), y As Double()) As BaggingClassifier
            Call EnsembleValidation.ValidateTrainingData(x, y, "BaggingClassifier.Train")

            Dim encoder As LabelEncoder = LabelEncoder.Fit(y)
            Dim yIndex As Integer() = encoder.Encode(y)
            Dim n As Integer = x.Length
            Dim size As Integer = System.Math.Max(1, CInt(System.Math.Round(n * ratio)))

            Call EnsembleParallel.FitAll(trained.Length, Parallel,
                Sub(m)
                    trained(m) = FitOne(x, yIndex, encoder.Classes, size, m)
                End Sub)

            classesArray = encoder.Classes
            featureWidthValue = x(0).Length

            Return Me
        End Function

        Private Function FitOne(x As Double()(), yIndex As Integer(), classes As Double(),
                                size As Integer, modelIndex As Integer) As IClassifier
            Dim sampler As New BootstrapSampler(seed + modelIndex)
            Dim index As Integer() = sampler.Sample(size)
            Dim xSub As Double()() = BootstrapSampler.Resample(x, index)
            Dim ySub As Integer() = BootstrapSampler.Resample(yIndex, index)

            Return learner.Fit(xSub, ySub, classes)
        End Function

        Public Function PredictProbability(x As Double()) As Double() Implements IClassifier.PredictProbability
            Call RequireTrained()
            Call x.ValidateFeatureVector(FeatureWidth, "BaggingClassifier.PredictProbability")

            Dim size As Integer = classesArray.Length
            Dim aggregate As Double() = New Double(size - 1) {}

            For m As Integer = 0 To trained.Length - 1
                Dim model As IClassifier = trained(m)

                If model Is Nothing Then
                    Throw New InvalidOperationException($"the bagging base model at index {m} is missing!")
                End If

                Dim aligned As Double() = ProbabilityMath.AlignTo(
                    model.PredictProbability(x), model.Classes, classesArray)

                For k As Integer = 0 To size - 1
                    aggregate(k) += aligned(k)
                Next
            Next

            Return ProbabilityMath.Normalize(aggregate)
        End Function

        ''' <summary>批量预测类别标签</summary>
        Public Function PredictBatch(x As Double()()) As Double()
            Call RequireTrained()
            Call x.ValidateFeatureMatrix(FeatureWidth, "BaggingClassifier.PredictBatch")

            Dim result As Double() = New Double(x.Length - 1) {}

            Call EnsembleParallel.PredictAll(x.Length, Parallel,
                Sub(i)
                    result(i) = ProbabilityMath.PredictClass(Me, x(i))
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
                Return "classifier"
            End Get
        End Property

        Public Function SaveModel(bundle As ModelBundle, folder As String) As ModelManifest Implements IPersistableModel.SaveModel
            Call RequireTrained()

            Dim manifest As ModelManifest = ModelManifest.Create(ModelCategory, ModelKind, "")

            manifest.featureWidth = FeatureWidth
            manifest.classes = CType(classesArray.Clone(), Double())
            manifest.SetScalar("sampleRatio", ratio)
            manifest.SetInteger("seed", seed)
            manifest.SetInteger("modelCount", trained.Length)

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
            Call EnsembleValidation.RequireTrained(classesArray IsNot Nothing, NameOf(BaggingClassifier), "classes")
        End Sub

        Public Overrides Function ToString() As String
            Return $"BaggingClassifier: {If(trained, New IClassifier() {}).Length} base model(s), " &
                   $"ratio={ratio}, seed={seed}, {If(classesArray, New Double() {}).Length} class(es)."
        End Function

    End Class

End Namespace
