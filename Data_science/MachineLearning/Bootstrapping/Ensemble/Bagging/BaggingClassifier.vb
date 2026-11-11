Imports System
Imports System.Linq

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
    Public NotInheritable Class BaggingClassifier : Implements IClassifier

        Private ReadOnly learner As IClassifierLearner
        Private ReadOnly modelCount As Integer
        Private ReadOnly ratio As Double
        Private ReadOnly seed As Integer
        Private ReadOnly trained As IClassifier()
        Private classesArray As Double()

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

        Public ReadOnly Property Classes As Double() Implements IClassifier.Classes
            Get
                Return classesArray
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

        ''' <summary>是否并行训练基模型（默认关闭，避免与算法库内部并行嵌套）</summary>
        Public Property Parallel As Boolean = False

        Public Function Train(x As Double()(), y As Double()) As BaggingClassifier
            If x Is Nothing OrElse x.Length = 0 Then
                Throw New ArgumentException("the feature matrix can not be empty!", NameOf(x))
            End If
            If y Is Nothing OrElse y.Length <> x.Length Then
                Throw New ArgumentException(
                    $"the feature rows ({x.Length}) and labels ({If(y Is Nothing, 0, y.Length)}) do not agree!",
                    NameOf(y))
            End If

            Dim encoder As LabelEncoder = LabelEncoder.Fit(y)
            Dim yIndex As Integer() = encoder.Encode(y)
            Dim n As Integer = x.Length
            Dim size As Integer = System.Math.Max(1, CInt(System.Math.Round(n * ratio)))

            If Parallel AndAlso trained.Length > 1 Then
                Call System.Threading.Tasks.Parallel.For(0, trained.Length,
                    Sub(m)
                        trained(m) = FitOne(x, yIndex, encoder.Classes, n, size, m)
                    End Sub)
            Else
                For m As Integer = 0 To trained.Length - 1
                    trained(m) = FitOne(x, yIndex, encoder.Classes, n, size, m)
                Next
            End If

            classesArray = encoder.Classes

            Return Me
        End Function

        Private Function FitOne(x As Double()(), yIndex As Integer(), classes As Double(),
                                n As Integer, size As Integer, modelIndex As Integer) As IClassifier
            Dim sampler As New BootstrapSampler(seed + modelIndex)
            Dim index As Integer() = sampler.Sample(size)
            Dim xSub As Double()() = BootstrapSampler.Resample(x, index)
            Dim ySub As Integer() = BootstrapSampler.Resample(yIndex, index)

            Return learner.Fit(xSub, ySub, classes)
        End Function

        Public Function PredictProbability(x As Double()) As Double() Implements IClassifier.PredictProbability
            RequireTrained()

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
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If

            Dim result As Double() = New Double(x.Length - 1) {}

            For i As Integer = 0 To x.Length - 1
                result(i) = ProbabilityMath.PredictClass(Me, x(i))
            Next

            Return result
        End Function

        Private Sub RequireTrained()
            If classesArray Is Nothing Then
                Throw New InvalidOperationException(
                    "the bagging classifier has not been trained yet; call Train() before predicting!")
            End If
        End Sub

    End Class

End Namespace
