Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualBasic.MachineLearning.Bootstrapping.Ensemble.Persistence

Namespace Ensemble

    ''' <summary>
    ''' Stacking 回归器：K 折 OOF 预测拼成元特征，再训练元回归器。
    ''' </summary>
    ''' <remarks>
    ''' 回归没有类别，OOF 元特征宽度等于基模型数；元特征与最终预测都使用
    ''' 同一组基模型定义，训练完成后基模型会用全量数据重训一次。
    ''' </remarks>
    Public NotInheritable Class StackingRegressor
        Implements IRegressor
        Implements IFeatureSchema
        Implements IPersistableModel

        Private ReadOnly learners As IRegressorLearner()
        Private ReadOnly meta As IRegressorLearner
        Private ReadOnly folds As Integer
        Private ReadOnly seed As Integer
        Private ReadOnly trained As IRegressor()
        Private metaFitted As IRegressor
        Private oofRMSEValue As Double
        Private featureWidthValue As Integer

        Public Sub New(learners As IEnumerable(Of IRegressorLearner),
                       meta As IRegressorLearner,
                       Optional folds As Integer = 5,
                       Optional seed As Integer = 42)
            If learners Is Nothing Then
                Throw New ArgumentNullException(NameOf(learners))
            End If
            If meta Is Nothing Then
                Throw New ArgumentNullException(NameOf(meta))
            End If

            Dim items As IRegressorLearner() = learners.ToArray()

            If items.Length = 0 Then
                Throw New ArgumentException("the stacking regressor requires at least one base learner!", NameOf(learners))
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
            Me.trained = New IRegressor(items.Length - 1) {}
        End Sub

        ''' <summary>
        ''' 用已经训练好的基模型与元模型直接构造集成器（用于从持久化模型恢复，不会再次训练）。
        ''' </summary>
        ''' <param name="trained">已训练的基回归器，顺序即元特征的列顺序</param>
        ''' <param name="meta">已训练的元回归器</param>
        ''' <param name="folds">训练期的折数（仅用于诊断输出）</param>
        ''' <param name="seed">训练期的随机种子（仅用于诊断输出）</param>
        ''' <param name="oofRMSE">训练期记录的 OOF 均方根误差</param>
        ''' <param name="featureWidth">训练期的特征维度，<c>0</c> 表示未知（跳过维度校验）</param>
        Public Sub New(trained As IRegressor(),
                       meta As IRegressor,
                       Optional folds As Integer = 5,
                       Optional seed As Integer = 42,
                       Optional oofRMSE As Double = Double.NaN,
                       Optional featureWidth As Integer = 0)
            If trained Is Nothing OrElse trained.Length = 0 Then
                Throw New ArgumentException("the trained regressor list can not be empty!", NameOf(trained))
            End If

            For i As Integer = 0 To trained.Length - 1
                If trained(i) Is Nothing Then
                    Throw New ArgumentException("the trained regressor list contains a NULL entry!", NameOf(trained))
                End If
            Next

            If meta Is Nothing Then
                Throw New ArgumentNullException(NameOf(meta))
            End If

            Me.learners = New IRegressorLearner() {}
            Me.meta = Nothing
            Me.folds = folds
            Me.seed = seed
            Me.trained = CType(trained.Clone(), IRegressor())
            Me.metaFitted = meta
            Me.oofRMSEValue = oofRMSE
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

        ''' <summary>训练后的元回归器</summary>
        Public ReadOnly Property MetaModel As IRegressor
            Get
                Return metaFitted
            End Get
        End Property

        ''' <summary>元回归器在 OOF 元特征上的均方根误差</summary>
        Public ReadOnly Property OofRMSE As Double
            Get
                Return oofRMSEValue
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

        Public Function Train(x As Double()(), y As Double()) As StackingRegressor
            Call EnsembleValidation.ValidateTrainingData(x, y, "StackingRegressor.Train")

            Dim n As Integer = x.Length

            Call CrossValidation.Validate(n, folds)

            Dim foldIndex As Integer()() = CrossValidation.KFold(n, folds, seed)
            Dim oof As Double()() = New Double(n - 1)() {}

            For i As Integer = 0 To n - 1
                oof(i) = New Double(learners.Length - 1) {}
            Next

            For l As Integer = 0 To learners.Length - 1
                For Each testIndex As Integer() In foldIndex
                    Dim trainIndex As Integer() = CrossValidation.Complement(n, testIndex)
                    Dim xTrain As Double()() = BootstrapSampler.Resample(x, trainIndex)
                    Dim yTrain As Double() = BootstrapSampler.Resample(y, trainIndex)
                    Dim model As IRegressor = learners(l).Fit(xTrain, yTrain)

                    For Each i As Integer In testIndex
                        oof(i)(l) = model.Predict(x(i))
                    Next
                Next
            Next

            metaFitted = meta.Fit(oof, y)

            Call EnsembleParallel.FitAll(trained.Length, Parallel,
                Sub(l)
                    trained(l) = learners(l).Fit(x, y)
                End Sub)

            Dim predicted As Double() = New Double(n - 1) {}

            For i As Integer = 0 To n - 1
                predicted(i) = metaFitted.Predict(oof(i))
            Next

            oofRMSEValue = EnsembleMetrics.RMSE(predicted, y)
            featureWidthValue = x(0).Length

            Return Me
        End Function

        Public Function Predict(x As Double()) As Double Implements IRegressor.Predict
            Call RequireTrained()
            Call x.ValidateFeatureVector(FeatureWidth, "StackingRegressor.Predict")

            Dim metaX As Double() = New Double(trained.Length - 1) {}

            For l As Integer = 0 To trained.Length - 1
                Dim model As IRegressor = trained(l)

                If model Is Nothing Then
                    Throw New InvalidOperationException($"the stacking base model at index {l} is missing!")
                End If

                metaX(l) = model.Predict(x)
            Next

            Return EnsembleValidation.EnsureFinite(
                metaFitted.Predict(metaX), "the stacking meta regressor")
        End Function

        ''' <summary>批量预测</summary>
        Public Function PredictBatch(x As Double()()) As Double()
            Call RequireTrained()
            Call x.ValidateFeatureMatrix(FeatureWidth, "StackingRegressor.PredictBatch")

            Dim result As Double() = New Double(x.Length - 1) {}

            Call EnsembleParallel.PredictAll(x.Length, Parallel,
                Sub(i)
                    result(i) = Predict(x(i))
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
                Return "regressor"
            End Get
        End Property

        Public Function SaveModel(bundle As ModelBundle, folder As String) As ModelManifest Implements IPersistableModel.SaveModel
            Call RequireTrained()

            Dim manifest As ModelManifest = ModelManifest.Create(ModelCategory, ModelKind, "")

            manifest.featureWidth = FeatureWidth
            manifest.SetInteger("folds", folds)
            manifest.SetInteger("seed", seed)
            manifest.SetInteger("modelCount", trained.Length)
            manifest.SetScalar("oofRmse", oofRMSEValue)

            For l As Integer = 0 To trained.Length - 1
                Dim childFolder As String = ModelBundle.CombineRelative(folder, $"base/{l}")

                Call manifest.children.Add(EnsembleModelStore.SaveChild(trained(l), bundle, childFolder))
            Next

            Dim metaFolder As String = ModelBundle.CombineRelative(folder, "meta")

            Call manifest.children.Add(EnsembleModelStore.SaveChild(metaFitted, bundle, metaFolder))

            Return manifest
        End Function

        Public Function Save(directory As String) As ModelManifest Implements IPersistableModel.Save
            Return EnsembleModelStore.Save(Me, directory)
        End Function

        Private Sub RequireTrained()
            Call EnsembleValidation.RequireTrained(
                metaFitted IsNot Nothing, NameOf(StackingRegressor), "meta")
        End Sub

        Public Overrides Function ToString() As String
            Return $"StackingRegressor: {trained.Length} base model(s), folds={folds}, seed={seed}, " &
                   $"oofRmse={oofRMSEValue:F4}."
        End Function

    End Class

End Namespace
