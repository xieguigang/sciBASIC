Imports System
Imports System.IO
Imports System.Linq
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.MachineLearning.XGBoost
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.train
Imports Microsoft.VisualBasic.MachineLearning.XGBoost.util
Imports XGBoostDataSet = Microsoft.VisualBasic.MachineLearning.XGBoost.DataSet

Namespace Ensemble

    ''' <summary>
    ''' XGBoost 训练结果的薄包装。
    ''' </summary>
    ''' <remarks>
    ''' 仓库里存在两套 XGBoost 实现：旧的 xgboost4j 移植只有推理
    ''' （<c>Predictor</c>），训练在 TGBoost 的 <c>GBM</c> 里。本类统一走
    ''' <c>GBM.fit</c> + <c>GBM.predict</c>，因此集成框架可以直接训练 XGBoost 基模型；
    ''' 若要使用外部训练好的 xgboost4j 二进制模型，请改用
    ''' <c>XGBoostPredictorClassifier</c> / <c>XGBoostPredictorRegressor</c>。
    ''' </remarks>
    Public NotInheritable Class XGBoostModel

        Private ReadOnly fitted As GBM
        Private ReadOnly width As Integer

        Public Sub New(fitted As GBM, width As Integer)
            Me.fitted = fitted
            Me.width = width
        End Sub

        ''' <summary>底层已训练的 TGBoost GBM 模型（供持久化/诊断）</summary>
        Public ReadOnly Property FittedModel As GBM
            Get
                Return fitted
            End Get
        End Property

        ''' <summary>特征维数</summary>
        Public ReadOnly Property FeatureWidth As Integer
            Get
                Return width
            End Get
        End Property

        ''' <summary>
        ''' 训练一个单输出 XGBoost 模型。
        ''' </summary>
        ''' <param name="loss">
        ''' <c>logloss</c>（二分类）或 <c>squareloss</c>（回归）
        ''' </param>
        Public Shared Function Fit(x As Double()(), y As Double(),
                                   loss As String,
                                   rounds As Integer,
                                   maxDepth As Integer,
                                   seed As Integer) As XGBoostModel
            If x Is Nothing OrElse x.Length = 0 Then
                Throw New ArgumentException("the feature matrix can not be empty!", NameOf(x))
            End If
            If y Is Nothing OrElse y.Length <> x.Length Then
                Throw New ArgumentException(
                    $"the feature rows ({x.Length}) and labels ({If(y Is Nothing, 0, y.Length)}) do not agree!",
                    NameOf(y))
            End If

            Dim rows As DoubleTagged(Of Single())() = New DoubleTagged(Of Single())(x.Length - 1) {}

            For i As Integer = 0 To x.Length - 1
                rows(i) = New DoubleTagged(Of Single()) With {
                    .Tag = y(i),
                    .Value = ToSingle(x(i))
                }
            Next

            Dim columns As String() = Enumerable.Range(1, x(0).Length).Select(Function(i) $"F{i}").ToArray()
            Dim train As TrainData = XGBoostDataSet.Conversion.ToTrainingSet(
                rows, columns, Array.Empty(Of String)())
            Dim gbm As New GBM()

            Call gbm.fit(train, Nothing,
                         loss:=loss,
                         num_boost_round:=rounds,
                         max_depth:=maxDepth,
                         random_seed:=CType(seed, Integer?))

            Return New XGBoostModel(gbm, x(0).Length)
        End Function

        ''' <summary>单输出预测（二分类时为 <c>P(y=1)</c>，回归时为原始尺度值）</summary>
        Public Function Predict(x As Double()) As Double
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If x.Length <> width Then
                Throw New ArgumentException($"feature size not agree: {x.Length} vs {width}!", NameOf(x))
            End If

            ' GBM.predict 返回按样本的数组（单输出模型长度为 1）
            Return fitted.predict(New Single()() {ToSingle(x)})(0)
        End Function

        ''' <summary>二分类概率，裁剪到 <c>[1e-5, 1-1e-5]</c> 以避免下游取对数时下溢</summary>
        Public Function PredictProbability(x As Double()) As Double
            Dim p As Double = Predict(x)

            Return System.Math.Max(0.00001R, System.Math.Min(1.0R - 0.00001R, p))
        End Function

        Friend Shared Function ToSingle(x As Double()) As Single()
            Dim values As Single() = New Single(x.Length - 1) {}

            For j As Integer = 0 To x.Length - 1
                values(j) = CSng(x(j))
            Next

            Return values
        End Function

    End Class

    ''' <summary>
    ''' 包装外部训练好的 xgboost4j 二进制模型（推理侧）。
    ''' </summary>
    ''' <remarks>
    ''' <c>Predictor</c> 不提供训练入口，只能从模型流加载；二分类
    ''' <c>binary:logistic</c> 的输出长度为 1（<c>P(y=1)</c>），多分类为类别数。
    ''' 稠密特征必须使用 <c>treatsZeroAsNA := False</c>，否则真实的 0 会被当成缺失值。
    ''' </remarks>
    Public NotInheritable Class XGBoostPredictor

        Private ReadOnly fitted As Predictor
        Private ReadOnly classCount As Integer

        Private Sub New(fitted As Predictor, classCount As Integer)
            Me.fitted = fitted
            Me.classCount = classCount
        End Sub

        ''' <summary>从模型字节加载</summary>
        Public Shared Function Load(modelBytes As Byte(), classCount As Integer) As XGBoostPredictor
            If modelBytes Is Nothing OrElse modelBytes.Length = 0 Then
                Throw New ArgumentException("the xgboost model bytes can not be empty!", NameOf(modelBytes))
            End If

            Return New XGBoostPredictor(New Predictor(New MemoryStream(modelBytes)), classCount)
        End Function

        ''' <summary>原始输出向量：二分类长度为 1，多分类为类别数</summary>
        Public Function PredictRaw(x As Double()) As Double()
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If

            Return fitted.predict(FVecTransformer.fromArray(x, treatsZeroAsNA:=False))
        End Function

        ''' <summary>把原始输出展开成与类别数对齐的概率向量</summary>
        Public Function PredictProbability(x As Double()) As Double()
            Dim raw As Double() = PredictRaw(x)

            If classCount <= 2 Then
                If raw.Length <> 1 Then
                    Throw New InvalidOperationException(
                        $"the binary xgboost model returned {raw.Length} value(s), expected 1!")
                End If

                Return New Double() {1.0R - raw(0), raw(0)}
            End If

            If raw.Length <> classCount Then
                Throw New InvalidOperationException(
                    $"the multi-class xgboost model returned {raw.Length} value(s), expected {classCount}!")
            End If

            Return ProbabilityMath.Normalize(raw)
        End Function

    End Class

    ''' <summary>包装外部 XGBoost 模型的分类器</summary>
    Public NotInheritable Class XGBoostPredictorClassifier : Implements IClassifier

        Private ReadOnly fitted As XGBoostPredictor
        Private ReadOnly classesArray As Double()

        Public Sub New(fitted As XGBoostPredictor, classes As Double())
            If fitted Is Nothing Then
                Throw New ArgumentNullException(NameOf(fitted))
            End If

            Me.fitted = fitted
            Me.classesArray = classes
        End Sub

        Public ReadOnly Property Classes As Double() Implements IClassifier.Classes
            Get
                Return classesArray
            End Get
        End Property

        Public Function PredictProbability(x As Double()) As Double() Implements IClassifier.PredictProbability
            Return fitted.PredictProbability(x)
        End Function

    End Class

    ''' <summary>包装外部 XGBoost 模型的回归器</summary>
    Public NotInheritable Class XGBoostPredictorRegressor : Implements IRegressor

        Private ReadOnly fitted As Predictor

        Public Sub New(fitted As Predictor)
            If fitted Is Nothing Then
                Throw New ArgumentNullException(NameOf(fitted))
            End If

            Me.fitted = fitted
        End Sub

        Public Function Predict(x As Double()) As Double Implements IRegressor.Predict
            Return fitted.predict(FVecTransformer.fromArray(x, treatsZeroAsNA:=False))(0)
        End Function

    End Class

End Namespace
