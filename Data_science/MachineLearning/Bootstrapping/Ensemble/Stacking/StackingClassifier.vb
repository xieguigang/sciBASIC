Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualBasic.MachineLearning.Bootstrapping.Ensemble.Persistence

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
    Public NotInheritable Class StackingClassifier
        Implements IClassifier
        Implements IFeatureSchema
        Implements IPersistableModel

        Private ReadOnly learners As IClassifierLearner()
        Private ReadOnly meta As IClassifierLearner
        Private ReadOnly folds As Integer
        Private ReadOnly seed As Integer
        Private ReadOnly trained As IClassifier()
        Private classesArray As Double()
        Private metaFitted As IClassifier
        Private oofAccuracyValue As Double
        Private featureWidthValue As Integer

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

        ''' <summary>
        ''' 用已经训练好的基模型与元模型直接构造集成器（用于从持久化模型恢复，不会再次训练）。
        ''' </summary>
        ''' <param name="trained">已训练的基分类器，顺序即元特征的段顺序</param>
        ''' <param name="meta">已训练的元分类器</param>
        ''' <param name="classes">全局类别顺序</param>
        ''' <param name="folds">训练期的折数（仅用于诊断输出）</param>
        ''' <param name="seed">训练期的随机种子（仅用于诊断输出）</param>
        ''' <param name="oofAccuracy">训练期记录的 OOF 准确率</param>
        ''' <param name="featureWidth">训练期的特征维度，<c>0</c> 表示未知（跳过维度校验）</param>
        Public Sub New(trained As IClassifier(),
                       meta As IClassifier,
                       classes As Double(),
                       Optional folds As Integer = 5,
                       Optional seed As Integer = 42,
                       Optional oofAccuracy As Double = Double.NaN,
                       Optional featureWidth As Integer = 0)
            If trained Is Nothing OrElse trained.Length = 0 Then
                Throw New ArgumentException("the trained classifier list can not be empty!", NameOf(trained))
            End If
            If trained.Any(Function(m) m Is Nothing) Then
                Throw New ArgumentException("the trained classifier list contains a NULL entry!", NameOf(trained))
            End If
            If meta Is Nothing Then
                Throw New ArgumentNullException(NameOf(meta))
            End If
            If classes Is Nothing OrElse classes.Length = 0 Then
                Throw New ArgumentException("the classes array can not be empty!", NameOf(classes))
            End If

            Me.learners = New IClassifierLearner() {}
            Me.meta = Nothing
            Me.folds = folds
            Me.seed = seed
            Me.trained = CType(trained.Clone(), IClassifier())
            Me.metaFitted = meta
            Me.classesArray = CType(classes.Clone(), Double())
            Me.oofAccuracyValue = oofAccuracy
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

        ''' <summary>训练使用的随机种子（从模型包恢复后仍保留原值）</summary>
        Public ReadOnly Property RandomSeed As Integer
            Get
                Return seed
            End Get
        End Property

        ''' <summary>是否并行训练基模型（默认关闭，避免与算法库内部并行嵌套）</summary>
        Public Property Parallel As Boolean = False

        Public Function Train(x As Double()(), y As Double()) As StackingClassifier
            Call EnsembleValidation.ValidateTrainingData(x, y, "StackingClassifier.Train")

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
            Call EnsembleParallel.FitAll(trained.Length, Parallel,
                Sub(l)
                    trained(l) = learners(l).Fit(x, yIndex, encoder.Classes)
                End Sub)

            classesArray = encoder.Classes
            featureWidthValue = x(0).Length

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
            Call RequireTrained()
            Call x.ValidateFeatureVector(FeatureWidth, "StackingClassifier.PredictProbability")

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
            Call RequireTrained()
            Call x.ValidateFeatureMatrix(FeatureWidth, "StackingClassifier.PredictBatch")

            Dim result As Double() = New Double(x.Length - 1) {}

            Call EnsembleParallel.PredictAll(x.Length, Parallel,
                Sub(i)
                    result(i) = ProbabilityMath.PredictClass(Me, x(i))
                End Sub)

            Return result
        End Function

        Public ReadOnly Property ModelKind As String Implements IPersistableModel.ModelKind
            Get
                Return "stacking"
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
            manifest.SetInteger("folds", folds)
            manifest.SetInteger("seed", seed)
            manifest.SetInteger("modelCount", trained.Length)
            manifest.SetScalar("oofAccuracy", oofAccuracyValue)

            For l As Integer = 0 To trained.Length - 1
                Dim childFolder As String = ModelBundle.CombineRelative(folder, $"base/{l}")

                Call manifest.children.Add(EnsembleModelStore.SaveChild(trained(l), bundle, childFolder))
            Next

            ' 元学习器固定追加在 children 末尾，加载时按最后的条目取回
            Dim metaFolder As String = ModelBundle.CombineRelative(folder, "meta")

            Call manifest.children.Add(EnsembleModelStore.SaveChild(metaFitted, bundle, metaFolder))

            Return manifest
        End Function

        Public Function Save(directory As String) As ModelManifest Implements IPersistableModel.Save
            Return EnsembleModelStore.Save(Me, directory)
        End Function

        Private Sub RequireTrained()
            Call EnsembleValidation.RequireTrained(
                classesArray IsNot Nothing AndAlso metaFitted IsNot Nothing,
                NameOf(StackingClassifier), "classes/meta")
        End Sub

        Public Overrides Function ToString() As String
            Return $"StackingClassifier: {trained.Length} base model(s), folds={folds}, seed={seed}, " &
                   $"oofAccuracy={oofAccuracyValue:F4}."
        End Function

    End Class

End Namespace
