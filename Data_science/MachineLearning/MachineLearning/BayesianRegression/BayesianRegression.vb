' Copyright (c) sciBASIC.NET Foundation
' GNU GENERAL PUBLIC LICENSE (GPL3) or later

Imports System
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.DataMining.Kernel.Classifier

Namespace BayesianRegression

    ''' <summary>
    ''' 特征似然的两种计算路径。
    ''' </summary>
    Public Enum BayesFeatureMode
        ''' <summary>离散/分类型特征，使用带加性平滑的频率表似然</summary>
        Discrete
        ''' <summary>连续特征，使用逐类逐维高斯似然</summary>
        Continuous
    End Enum

    ''' <summary>
    ''' 基于朴素贝叶斯方法的回归模型。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 朴素贝叶斯是分类器，本模型通过「目标分箱 + 后验期望」把它用于回归：
    ''' 训练时把连续目标 y 离散成 K 个等宽区间，以区间编号作为类别标签拟合
    ''' 朴素贝叶斯分类器；预测时计算各区间的后验概率，再以
    ''' <c>y_hat = SUM_k P(k|x) * representative(k)</c> 还原连续预测值。
    ''' </para>
    ''' <para>
    ''' <see cref="BayesFeatureMode.Discrete"/> 使用 <see cref="Bayesian"/> 的频率表似然，
    ''' <see cref="BayesFeatureMode.Continuous"/> 使用 <see cref="GaussianBayes"/> 的高斯似然，
    ''' 两条路径共用同一套训练/预测入口与后验计算，仅似然策略不同。
    ''' </para>
    ''' </remarks>
    Public Class BayesianRegression : Inherits Global.Microsoft.VisualBasic.MachineLearning.Model

        Private _model As NaiveBayesModel
        Private _binning As TargetBinning
        Private _classes As Integer()
        Private _representatives As Double()
        Private _mode As BayesFeatureMode
        Private _bins As Integer
        Private _smoothing As Double
        Private _varianceFloor As Double

        ''' <summary>预测结果写回标签列时使用的默认列名</summary>
        Public Const DefaultPredictionColumn As String = "bayes_prediction"

        ''' <summary>模型是否已完成训练</summary>
        Public ReadOnly Property IsTrained As Boolean
            Get
                Return _model IsNot Nothing
            End Get
        End Property

        ''' <summary>本次训练使用的特征似然路径</summary>
        Public ReadOnly Property Mode As BayesFeatureMode
            Get
                Return _mode
            End Get
        End Property

        ''' <summary>本次训练请求的箱数</summary>
        Public ReadOnly Property Bins As Integer
            Get
                Return _bins
            End Get
        End Property

        ''' <summary>离散路径使用的加性平滑系数</summary>
        Public ReadOnly Property Smoothing As Double
            Get
                Return _smoothing
            End Get
        End Property

        ''' <summary>连续路径使用的方差下限</summary>
        Public ReadOnly Property VarianceFloor As Double
            Get
                Return _varianceFloor
            End Get
        End Property

        ''' <summary>目标分箱器</summary>
        Public ReadOnly Property Binning As TargetBinning
            Get
                Return _binning
            End Get
        End Property

        ''' <summary>底层的朴素贝叶斯分类器（两条路径的统一基类）</summary>
        Public ReadOnly Property PosteriorModel As NaiveBayesModel
            Get
                Return _model
            End Get
        End Property

        ''' <summary>模型期望的输入特征维度，未训练时为 0</summary>
        Public ReadOnly Property FeatureWidth As Integer
            Get
                Return If(_model Is Nothing, 0, _model.FeatureWidth)
            End Get
        End Property

        ''' <summary>
        ''' 用统一数值表训练回归模型。
        ''' </summary>
        ''' <param name="table">特征矩阵 + 连续目标列的数值表</param>
        ''' <param name="targetColumn">连续目标所在标签列的索引</param>
        ''' <param name="mode">特征似然路径</param>
        ''' <param name="bins">目标分箱数量</param>
        ''' <param name="smoothing">离散路径的加性平滑系数</param>
        ''' <param name="varianceFloor">连续路径的方差下限</param>
        Public Sub Train(table As NumericTable,
                         targetColumn As Integer,
                         Optional mode As BayesFeatureMode = BayesFeatureMode.Discrete,
                         Optional bins As Integer = TargetBinning.DefaultBins,
                         Optional smoothing As Double = Bayesian.DefaultSmoothing,
                         Optional varianceFloor As Double = GaussianBayes.DefaultVarianceFloor)

            If table Is Nothing Then
                Throw New ArgumentNullException(NameOf(table))
            End If

            Call table.Validate()

            If table.labels Is Nothing Then
                Throw New ArgumentException("the training table has no label column for the continuous target.", NameOf(table))
            End If
            If targetColumn < 0 OrElse targetColumn >= table.nlabels Then
                Throw New ArgumentOutOfRangeException(NameOf(targetColumn),
                    $"target column index {targetColumn} is out of the label range [0, {table.nlabels - 1}].")
            End If

            Call TrainCore(table.features, table.GetLabel(targetColumn), mode, bins, smoothing, varianceFloor)
        End Sub

        ''' <summary>
        ''' 用统一数值表按标签列名称训练回归模型。
        ''' </summary>
        Public Sub Train(table As NumericTable,
                         targetColumn As String,
                         Optional mode As BayesFeatureMode = BayesFeatureMode.Discrete,
                         Optional bins As Integer = TargetBinning.DefaultBins,
                         Optional smoothing As Double = Bayesian.DefaultSmoothing,
                         Optional varianceFloor As Double = GaussianBayes.DefaultVarianceFloor)

            If table Is Nothing Then
                Throw New ArgumentNullException(NameOf(table))
            End If

            Call table.Validate()
            Call TrainCore(table.features, table.GetLabel(targetColumn), mode, bins, smoothing, varianceFloor)
        End Sub

        ''' <summary>
        ''' 直接由特征矩阵与连续目标向量训练回归模型。
        ''' </summary>
        Public Sub Train(x As Double()(),
                         y As Double(),
                         Optional mode As BayesFeatureMode = BayesFeatureMode.Discrete,
                         Optional bins As Integer = TargetBinning.DefaultBins,
                         Optional smoothing As Double = Bayesian.DefaultSmoothing,
                         Optional varianceFloor As Double = GaussianBayes.DefaultVarianceFloor)

            Call TrainCore(x, y, mode, bins, smoothing, varianceFloor)
        End Sub

        Private Sub TrainCore(x As Double()(),
                              y As Double(),
                              mode As BayesFeatureMode,
                              bins As Integer,
                              smoothing As Double,
                              varianceFloor As Double)

            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If y Is Nothing Then
                Throw New ArgumentNullException(NameOf(y))
            End If
            If x.Length = 0 Then
                Throw New ArgumentException("the training set contains no samples.", NameOf(x))
            End If
            If x.Length <> y.Length Then
                Throw New ArgumentException(
                    $"feature rows ({x.Length}) and target values ({y.Length}) have different lengths.", NameOf(y))
            End If

            Dim binning As TargetBinning = TargetBinning.Fit(y, bins)
            Dim codes As Integer() = binning.Encode(y)
            Dim training As New NumericTable(x, codes)

            ' 校验每个样本的特征维度一致
            Call training.Validate()

            Dim fitted As NaiveBayesModel

            Select Case mode
                Case BayesFeatureMode.Discrete
                    fitted = Bayesian.Load(training, 0, smoothing)
                Case BayesFeatureMode.Continuous
                    fitted = GaussianBayes.Load(training, 0, varianceFloor)
                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(mode),
                        $"unsupported naive bayes feature mode: {mode}.")
            End Select

            ' 全部训练期状态在这里冻结，预测期不再枚举训练数据
            _model = fitted
            _binning = binning
            _classes = fitted.Classes
            _representatives = binning.Representatives
            _mode = mode
            _bins = bins
            _smoothing = smoothing
            _varianceFloor = varianceFloor
        End Sub

        Private Sub RequireTrained()
            If _model Is Nothing Then
                Throw New InvalidOperationException("the bayesian regression model has not been trained yet, call Train() first.")
            End If
        End Sub

        ''' <summary>
        ''' 返回各类别（目标箱）的后验概率，顺序与 <see cref="NaiveBayesModel.Classes"/> 一致。
        ''' </summary>
        Public Function PredictPosterior(x As Double()) As Double()
            Call RequireTrained()
            Return _model.Posterior(x)
        End Function

        ''' <summary>
        ''' 预测单个样本的连续目标值，即后验概率对箱代表值的加权期望。
        ''' </summary>
        Public Function Predict(x As Double()) As Double
            Dim posterior As Double() = PredictPosterior(x)
            Dim expected As Double = 0.0

            For k As Integer = 0 To posterior.Length - 1
                expected += posterior(k) * _representatives(_classes(k))
            Next

            Return expected
        End Function

        ''' <summary>批量预测特征矩阵</summary>
        Public Function Predict(X As Double()()) As Double()
            Call RequireTrained()

            If X Is Nothing Then
                Throw New ArgumentNullException(NameOf(X))
            End If

            Dim result As Double() = New Double(X.Length - 1) {}

            For i As Integer = 0 To X.Length - 1
                result(i) = Predict(X(i))
            Next

            Return result
        End Function

        ''' <summary>批量预测数值表中的全部样本</summary>
        Public Function Predict(table As NumericTable) As Double()
            Call RequireTrained()

            If table Is Nothing Then
                Throw New ArgumentNullException(NameOf(table))
            End If

            Call table.Validate()

            Dim result As Double() = New Double(table.nsamples - 1) {}

            For i As Integer = 0 To table.nsamples - 1
                result(i) = Predict(table.features(i))
            Next

            Return result
        End Function

        ''' <summary>
        ''' 批量预测并按 <see cref="NumericTable"/> 的约定把结果写回标签列。
        ''' </summary>
        ''' <param name="table">待预测的数值表</param>
        ''' <param name="columnName">写入的标签列名，列已存在时覆盖</param>
        ''' <returns>返回传入的表对象以支持链式调用</returns>
        Public Function Apply(table As NumericTable,
                              Optional columnName As String = DefaultPredictionColumn) As NumericTable

            Dim predicted As Double() = Predict(table)

            ' NumericTable.SetLabel 在追加新列时会读取已有的 labelNames，
            ' 而 New NumericTable(x, y) 构造的表不带列名，这里补齐以避免空引用。
            If table.labelNames Is Nothing AndAlso table.nlabels > 0 Then
                Dim names As String() = New String(table.nlabels - 1) {}

                For i As Integer = 0 To names.Length - 1
                    names(i) = $"label_{i + 1}"
                Next

                table.labelNames = names
            End If

            Call table.SetLabel(columnName, predicted)

            Return table
        End Function

        Public Overrides Function ToString() As String
            If _model Is Nothing Then
                Return "BayesianRegression (not trained)."
            End If

            Return $"BayesianRegression: mode={_mode}, bins={_binning.BinCount}, features={_model.FeatureWidth}, " &
                   $"samples={_model.SampleSize}, classes={_model.ClassCount}."
        End Function

    End Class
End Namespace
