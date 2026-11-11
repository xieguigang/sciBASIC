' Copyright (c) sciBASIC.NET Foundation
' GNU GENERAL PUBLIC LICENSE (GPL3) or later

Imports System
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.DataMining.Kernel.Classifier

Namespace BayesianRegression

    ''' <summary>
    ''' 连续特征的高斯朴素贝叶斯分类器。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 与 <see cref="Bayesian"/> 的离散频率表路径相对，本类对每个类别、每个维度
    ''' 独立估计均值与方差，使用高斯密度计算 log P(x|Y)。两条路径共享
    ''' <see cref="NaiveBayesModel"/> 的类别先验、log 域累加和 log-sum-exp 归一化，
    ''' 因而对外暴露完全一致的 <c>Posterior</c> / <c>Classify</c> 接口。
    ''' </para>
    ''' <para>
    ''' 采用闭式 log 似然
    ''' <c>-0.5 * (log(2*pi*sigma^2) + (x-mu)^2/sigma^2)</c>，
    ''' 与 <c>Microsoft.VisualBasic.Math.Distributions.pnorm.ProbabilityDensity</c>
    ''' 的密度语义一致，但不会因多特征密度连乘而下溢。
    ''' 方差带有下限保护，避免零方差导致除零。
    ''' </para>
    ''' </remarks>
    Public Class GaussianBayes : Inherits NaiveBayesModel

        ''' <summary>_means(classIndex)(featureIndex)</summary>
        Private ReadOnly _means As Double()()
        ''' <summary>_variances(classIndex)(featureIndex)</summary>
        Private ReadOnly _variances As Double()()
        Private ReadOnly _varianceFloor As Double

        ''' <summary>方差下限的默认值</summary>
        Public Const DefaultVarianceFloor As Double = 0.000000001

        Private Sub New(table As NumericTable, classColumn As Integer, varianceFloor As Double)
            MyBase.New(table, classColumn)

            If Double.IsNaN(varianceFloor) OrElse Double.IsInfinity(varianceFloor) OrElse varianceFloor <= 0.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(varianceFloor),
                    "the variance floor must be a positive finite number.")
            End If

            _varianceFloor = varianceFloor

            Dim slots As Integer = _classes.Length * _width
            Dim counts As Integer() = New Integer(slots - 1) {}
            Dim means As Double() = New Double(slots - 1) {}
            Dim m2 As Double() = New Double(slots - 1) {}

            ' Welford 在线算法：单次遍历同时得到均值与总体方差，避免两遍扫描
            For i As Integer = 0 To _sampleSize - 1
                Dim classOffset As Integer = _classIndex(i) * _width
                Dim row As Double() = _features(i)

                For j As Integer = 0 To _width - 1
                    Dim value As Double = row(j)

                    If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
                        Throw New ArgumentException(
                            $"the feature value at sample[{i}][{j}] is not a finite number.", NameOf(table))
                    End If

                    Dim slot As Integer = classOffset + j
                    counts(slot) += 1

                    Dim delta As Double = value - means(slot)
                    means(slot) += delta / counts(slot)
                    m2(slot) += delta * (value - means(slot))
                Next
            Next

            _means = New Double(_classes.Length - 1)() {}
            _variances = New Double(_classes.Length - 1)() {}

            For k As Integer = 0 To _classes.Length - 1
                _means(k) = New Double(_width - 1) {}
                _variances(k) = New Double(_width - 1) {}

                For j As Integer = 0 To _width - 1
                    Dim slot As Integer = k * _width + j
                    Dim variance As Double = m2(slot) / counts(slot)

                    If Double.IsNaN(variance) OrElse Double.IsInfinity(variance) OrElse variance < _varianceFloor Then
                        variance = _varianceFloor
                    End If

                    _means(k)(j) = means(slot)
                    _variances(k)(j) = variance
                Next
            Next
        End Sub

        ''' <summary>
        ''' 从已持久化的均值/方差矩阵直接恢复高斯分类器，不做任何训练数据扫描。
        ''' </summary>
        Private Sub New(classes As Integer(), logPriors As Double(), classCounts As Integer(),
                        width As Integer, sampleSize As Integer,
                        means As Double()(), variances As Double()(),
                        varianceFloor As Double)
            MyBase.New(classes, logPriors, classCounts, width, sampleSize)

            If Double.IsNaN(varianceFloor) OrElse Double.IsInfinity(varianceFloor) OrElse varianceFloor <= 0.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(varianceFloor),
                    "the variance floor must be a positive finite number.")
            End If
            If means Is Nothing OrElse means.Length <> classes.Length Then
                Throw New ArgumentException(
                    $"the restored mean matrix ({If(means Is Nothing, 0, means.Length)}) " &
                    $"does not match the class count ({classes.Length})!", NameOf(means))
            End If
            If variances Is Nothing OrElse variances.Length <> classes.Length Then
                Throw New ArgumentException(
                    $"the restored variance matrix ({If(variances Is Nothing, 0, variances.Length)}) " &
                    $"does not match the class count ({classes.Length})!", NameOf(variances))
            End If

            For k As Integer = 0 To classes.Length - 1
                If means(k) Is Nothing OrElse means(k).Length <> width Then
                    Throw New ArgumentException(
                        $"the restored mean row [{k}] has {If(means(k) Is Nothing, 0, means(k).Length)} " &
                        $"value(s), expected {width}!", NameOf(means))
                End If
                If variances(k) Is Nothing OrElse variances(k).Length <> width Then
                    Throw New ArgumentException(
                        $"the restored variance row [{k}] has {If(variances(k) Is Nothing, 0, variances(k).Length)} " &
                        $"value(s), expected {width}!", NameOf(variances))
                End If
            Next

            ' 深拷贝：调用方随后可以安全释放原数组
            _means = New Double(classes.Length - 1)() {}
            _variances = New Double(classes.Length - 1)() {}

            For k As Integer = 0 To classes.Length - 1
                _means(k) = CType(means(k).Clone(), Double())
                _variances(k) = CType(variances(k).Clone(), Double())
            Next

            _varianceFloor = varianceFloor
        End Sub

        ''' <summary>
        ''' 从已保存的模型状态恢复高斯朴素贝叶斯分类器（用于模型持久化加载）。
        ''' </summary>
        Public Shared Function Restore(classes As Integer(),
                                       logPriors As Double(),
                                       classCounts As Integer(),
                                       width As Integer,
                                       sampleSize As Integer,
                                       means As Double()(),
                                       variances As Double()(),
                                       Optional varianceFloor As Double = DefaultVarianceFloor) As GaussianBayes
            Return New GaussianBayes(classes, logPriors, classCounts, width, sampleSize,
                                     means, variances, varianceFloor)
        End Function

        ''' <summary>方差下限</summary>
        Public ReadOnly Property VarianceFloor As Double
            Get
                Return _varianceFloor
            End Get
        End Property

        ''' <summary>按类别、维度索引的均值矩阵，请勿修改返回的数组</summary>
        Public ReadOnly Property Means As Double()()
            Get
                Return _means
            End Get
        End Property

        ''' <summary>按类别、维度索引的方差矩阵，请勿修改返回的数组</summary>
        Public ReadOnly Property Variances As Double()()
            Get
                Return _variances
            End Get
        End Property

        ''' <summary>
        ''' 高斯 log 似然 log P(x|Y=classIndex)。
        ''' </summary>
        Protected Overrides Function LogLikelihood(x As Double(), classIndex As Integer) As Double
            Dim sum As Double = 0.0
            Dim classMeans As Double() = _means(classIndex)
            Dim classVariances As Double() = _variances(classIndex)

            For j As Integer = 0 To _width - 1
                Dim value As Double = x(j)

                If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
                    Throw New ArgumentException($"the input feature[{j}] is not a finite number.", NameOf(x))
                End If

                Dim variance As Double = classVariances(j)
                Dim delta As Double = value - classMeans(j)

                sum += -0.5 * (System.Math.Log(2.0 * System.Math.PI * variance) + delta * delta / variance)
            Next

            Return sum
        End Function

        ''' <summary>从统一数值表构建高斯分类器</summary>
        Public Shared Function Load(table As NumericTable,
                                    Optional classColumn As Integer = 0,
                                    Optional varianceFloor As Double = DefaultVarianceFloor) As GaussianBayes

            Return New GaussianBayes(table, classColumn, varianceFloor)
        End Function

        ''' <summary>从统一数值表按标签列名称构建高斯分类器</summary>
        Public Shared Function Load(table As NumericTable,
                                    classColumn As String,
                                    Optional varianceFloor As Double = DefaultVarianceFloor) As GaussianBayes

            Return New GaussianBayes(table, FindLabelColumn(table, classColumn), varianceFloor)
        End Function

        ''' <summary>直接由特征矩阵与整数类别向量构建高斯分类器</summary>
        Public Shared Function Load(x As Double()(),
                                    y As Integer(),
                                    Optional varianceFloor As Double = DefaultVarianceFloor) As GaussianBayes

            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If y Is Nothing Then
                Throw New ArgumentNullException(NameOf(y))
            End If
            If x.Length <> y.Length Then
                Throw New ArgumentException(
                    $"feature rows ({x.Length}) and class labels ({y.Length}) have different lengths.", NameOf(y))
            End If

            Return New GaussianBayes(New NumericTable(x, y), 0, varianceFloor)
        End Function

        Public Overrides Function ToString() As String
            Return $"Gaussian Naive Bayes, {SampleSize} samples, {FeatureWidth} features, {ClassCount} classes (variance floor={_varianceFloor})."
        End Function

    End Class
End Namespace
