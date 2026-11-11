Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace Ensemble

    ''' <summary>
    ''' Stacking 分类器：先用分层 K 折生成基模型的**外推（OOF）**概率，
    ''' 把各基模型的概率向量拼接成元特征，再训练元分类器。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 必须使用 OOF 而不是训练集内的预测：后者会让元学习器学到基模型的
    ''' 过拟合噪声，导致集成后的泛化能力反而下降。
    ''' </para>
    ''' <para>
    ''' 元特征宽度 = <c>基模型数 × 类别数</c>，每一段都按统一的 <c>Classes</c>
    ''' 顺序对齐，因此基模型的类别编码差异不会串位。
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class StackingClassifier : Implements IClassifier

        Private ReadOnly learners As IClassifierLearner()
        Private ReadOnly meta As IClassifierLearner
        Private ReadOnly folds As Integer
        Private ReadOnly seed As Integer
        Private ReadOnly trained As IClassifier()
        Private classesArray As Double()
        Private metaFitted As IClassifier
        Private oofAccuracyValue As Double

        ''' <param name="learners">基学习器（异质）</param>
        ''' <param name="meta">元学习器，同样走 <see cref="IClassifierLearner"/> 契约</param>
        ''' <param name="folds">分层 K 折的折数</param>
        ''' <param name="seed">折划分的随机种子</param>
        Public Sub New(learners As IEnumerable(Of IClassifierLearner),
                       meta As IClassifierLearner,
                       Optional folds As Integer = 5,
                       Optional seed As Integer = 42)
            If learners Is Nothing Then
                Throw New ArgumentNullException(NameOf(learners))
            End If
            If meta Is Nothing Then
                Throw New ArgumentNullException(NameOf(meta))
            End If

            Dim items As IClassifierLearner() = learners.ToArray()

            If items.Length = 0 Then
                Throw New ArgumentException("the stacking classifier requires at least one base learner!", NameOf(learners))
            End If
            If items.Any(Function(m) m Is Nothing) Then
                Throw New ArgumentException("the base learner list contains a NULL entry!", NameOf(learners))
            End If
            If folds < 2 Then
                Throw New ArgumentOutOfRangeException(NameOf(folds), $"stacking requires at least 2 folds, but got {folds}!")
            End If

            Me.learners = items
            Me.meta = meta
            Me.folds = folds
            Me.seed = seed
            Me.trained = New IClassifier(items.Length - 1) {}
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

        ''' <summary>训练后的元分类器</summary>
        Public ReadOnly Property MetaModel As IClassifier
            Get
                Return metaFitted
            End Get
        End Property

        ''' <summary>元分类器在 OOF 元特征上的准确率</summary>
        Public ReadOnly Property OofAccuracy As Double
            Get
                Return oofAccuracyValue
            End Get
        End Property

        Public ReadOnly Property FoldCount As Integer
            Get
                Return folds
            End Get
        End Property

        Public Function Train(x As Double()(), y As Double()) As StackingClassifier
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
            Dim classCount As Integer = encoder.ClassCount

            Call CrossValidation.Validate(n, folds)

            Dim foldIndex As Integer()() = CrossValidation.StratifiedKFold(yIndex, folds, seed)
            Dim metaWidth As Integer = learners.Length * classCount
            Dim oof As Double()() = New Double(n - 1)() {}

            For i As Integer = 0 To n - 1
                oof(i) = New Double(metaWidth - 1) {}
            Next

            ' 每个基模型都在 K 折上训练，只为「测试折」写 OOF 预测
            For l As Integer = 0 To learners.Length - 1
                For Each testIndex As Integer() In foldIndex
                    Dim trainIndex As Integer() = CrossValidation.Complement(n, testIndex)
                    Dim xTrain As Double()() = BootstrapSampler.Resample(x, trainIndex)
                    Dim yTrain As Integer() = BootstrapSampler.Resample(yIndex, trainIndex)
                    Dim model As IClassifier = learners(l).Fit(xTrain, yTrain, encoder.Classes)

                    For Each i As Integer In testIndex
                        Dim aligned As Double() = ProbabilityMath.AlignTo(
                            model.PredictProbability(x(i)), model.Classes, encoder.Classes)
                        Dim offset As Integer = l * classCount

                        For k As Integer = 0 To classCount - 1
                            oof(i)(offset + k) = aligned(k)
                        Next
                    Next
                Next
            Next

            metaFitted = meta.Fit(oof, yIndex, encoder.Classes)

            ' 最终基模型使用全量训练数据
            For l As Integer = 0 To learners.Length - 1
                trained(l) = learners(l).Fit(x, yIndex, encoder.Classes)
            Next

            classesArray = encoder.Classes

            Dim hit As Integer = 0

            For i As Integer = 0 To n - 1
                If ProbabilityMath.ArgMax(metaFitted.PredictProbability(oof(i))) = yIndex(i) Then
                    hit += 1
                End If
            Next

            oofAccuracyValue = hit / CDbl(n)

            Return Me
        End Function

        Public Function PredictProbability(x As Double()) As Double() Implements IClassifier.PredictProbability
            RequireTrained()

            Dim classCount As Integer = classesArray.Length
            Dim metaWidth As Integer = trained.Length * classCount
            Dim metaX As Double() = New Double(metaWidth - 1) {}

            For l As Integer = 0 To trained.Length - 1
                Dim model As IClassifier = trained(l)

                If model Is Nothing Then
                    Throw New InvalidOperationException($"the stacking base model at index {l} is missing!")
                End If

                Dim aligned As Double() = ProbabilityMath.AlignTo(
                    model.PredictProbability(x), model.Classes, classesArray)
                Dim offset As Integer = l * classCount

                For k As Integer = 0 To classCount - 1
                    metaX(offset + k) = aligned(k)
                Next
            Next

            Return ProbabilityMath.Normalize(metaFitted.PredictProbability(metaX))
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
            If classesArray Is Nothing OrElse metaFitted Is Nothing Then
                Throw New InvalidOperationException(
                    "the stacking classifier has not been trained yet; call Train() before predicting!")
            End If
        End Sub

    End Class

End Namespace
