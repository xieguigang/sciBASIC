' Copyright (c) sciBASIC.NET Foundation
' GNU GENERAL PUBLIC LICENSE (GPL3) or later

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.VisualBasic.Data
' 项目内存在 Microsoft.VisualBasic.Math 命名空间，会遮蔽 System.Math，
' 因此数值函数一律使用完全限定名 System.Math.*。

Namespace Kernel.Classifier

    ''' <summary>
    ''' 朴素贝叶斯模型的共享推断内核。
    ''' </summary>
    ''' <remarks>
    ''' 本类负责训练数据的统一校验、类别先验的统计以及 log 域的后验推断；
    ''' 具体的似然估计 P(x|Y) 由派生类实现（例如离散特征频率表或者连续特征高斯分布）。
    ''' 全部推断都在 log 域中完成，并通过 log-sum-exp 归一化，
    ''' 从而避免多特征概率连乘造成的浮点下溢。
    ''' </remarks>
    Public MustInherit Class NaiveBayesModel

        ''' <summary>升序排列的类别标签</summary>
        Protected ReadOnly _classes As Integer()
        ''' <summary>各类别的 log 先验，log(N_y / N)</summary>
        Protected ReadOnly _logPriors As Double()
        ''' <summary>各类别的样本数量</summary>
        Protected ReadOnly _classCounts As Integer()
        ''' <summary>每个训练样本对应的类别下标</summary>
        Protected ReadOnly _classIndex As Integer()
        ''' <summary>特征维度</summary>
        Protected ReadOnly _width As Integer
        ''' <summary>训练样本数量</summary>
        Protected ReadOnly _sampleSize As Integer
        ''' <summary>训练特征矩阵，行主序</summary>
        Protected ReadOnly _features As Double()()
        ''' <summary>训练类别标签</summary>
        Protected ReadOnly _labels As Integer()

        ''' <summary>
        ''' 从统一数值表中读取训练数据并完成类别统计。
        ''' </summary>
        ''' <param name="table">带类别标签列的数值表</param>
        ''' <param name="classColumn">类别标签所在列的索引（0 基）</param>
        Protected Sub New(table As NumericTable, classColumn As Integer)
            If table Is Nothing Then
                Throw New ArgumentNullException(NameOf(table))
            End If

            ' 由数据载体自身校验特征/标签矩阵的维度一致性
            Call table.Validate()

            If table.nsamples = 0 Then
                Throw New ArgumentException("the training table contains no samples.", NameOf(table))
            End If
            If table.nfeatures = 0 Then
                Throw New ArgumentException("the training table contains no feature columns.", NameOf(table))
            End If
            If table.labels Is Nothing Then
                Throw New ArgumentException("the training table has no label column for the class codes.", NameOf(table))
            End If
            If classColumn < 0 OrElse classColumn >= table.nlabels Then
                Throw New ArgumentOutOfRangeException(NameOf(classColumn),
                    $"class column index {classColumn} is out of the label range [0, {table.nlabels - 1}].")
            End If

            _features = table.features
            _width = table.nfeatures
            _sampleSize = table.nsamples
            _labels = New Integer(_sampleSize - 1) {}
            _classIndex = New Integer(_sampleSize - 1) {}

            For i As Integer = 0 To _sampleSize - 1
                Dim value As Double = table.labels(i)(classColumn)

                If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
                    Throw New ArgumentException($"the class label at row[{i}] is not a finite number.", NameOf(table))
                End If
                If System.Math.Abs(value - System.Math.Round(value)) > 0.0000001 Then
                    Throw New ArgumentException(
                        $"the class label at row[{i}] = {value} is not an integer class code. " &
                        "discretize continuous targets before fitting a naive bayes classifier.", NameOf(table))
                End If

                _labels(i) = CInt(System.Math.Round(value))
            Next

            _classes = _labels.Distinct().OrderBy(Function(c) c).ToArray()
            _classCounts = New Integer(_classes.Length - 1) {}
            _logPriors = New Double(_classes.Length - 1) {}

            Dim lookup As New Dictionary(Of Integer, Integer)()

            For k As Integer = 0 To _classes.Length - 1
                lookup.Add(_classes(k), k)
            Next

            For i As Integer = 0 To _sampleSize - 1
                Dim ci As Integer = lookup(_labels(i))
                _classIndex(i) = ci
                _classCounts(ci) += 1
            Next

            For k As Integer = 0 To _classes.Length - 1
                _logPriors(k) = System.Math.Log(_classCounts(k) / _sampleSize)
            Next
        End Sub

        ''' <summary>升序排列的类别标签数组，请勿修改返回的数组。</summary>
        Public ReadOnly Property Classes As Integer()
            Get
                Return _classes
            End Get
        End Property

        ''' <summary>各类别的 log 先验，请勿修改返回的数组。</summary>
        Public ReadOnly Property LogPriors As Double()
            Get
                Return _logPriors
            End Get
        End Property

        ''' <summary>各类别的训练样本数，请勿修改返回的数组。</summary>
        Public ReadOnly Property ClassCounts As Integer()
            Get
                Return _classCounts
            End Get
        End Property

        ''' <summary>特征维度</summary>
        Public ReadOnly Property FeatureWidth As Integer
            Get
                Return _width
            End Get
        End Property

        ''' <summary>训练样本数量</summary>
        Public ReadOnly Property SampleSize As Integer
            Get
                Return _sampleSize
            End Get
        End Property

        ''' <summary>类别的数量</summary>
        Public ReadOnly Property ClassCount As Integer
            Get
                Return _classes.Length
            End Get
        End Property

        ''' <summary>读取指定类别下标对应的类别标签</summary>
        Public ReadOnly Property ClassAt(index As Integer) As Integer
            Get
                Return _classes(index)
            End Get
        End Property

        ''' <summary>
        ''' 查找类别标签对应的下标，标签不存在时返回 -1。
        ''' </summary>
        Public Function ClassIndexOf(y As Integer) As Integer
            Return Array.IndexOf(_classes, y)
        End Function

        ''' <summary>
        ''' 按名称（大小写不敏感）定位标签列索引，供派生类的 Load 重载复用。
        ''' </summary>
        Protected Shared Function FindLabelColumn(table As NumericTable, name As String) As Integer
            If String.IsNullOrEmpty(name) Then
                Throw New ArgumentNullException(NameOf(name))
            End If
            If table IsNot Nothing AndAlso table.labelNames IsNot Nothing Then
                For i As Integer = 0 To table.labelNames.Length - 1
                    If String.Equals(table.labelNames(i), name, StringComparison.OrdinalIgnoreCase) Then
                        Return i
                    End If
                Next
            End If

            Throw New KeyNotFoundException($"missing label column '{name}' in the training table.")
        End Function

        ''' <summary>
        ''' log P(x|Y=classIndex)，由派生类实现。
        ''' </summary>
        Protected MustOverride Function LogLikelihood(x As Double(), classIndex As Integer) As Double

        ''' <summary>
        ''' 计算各类别的未归一化 log 后验：log P(Y) + log P(x|Y)。
        ''' </summary>
        Private Function LogScores(x As Double()) As Double()
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If x.Length <> _width Then
                Throw New ArgumentException(
                    $"the input vector has {x.Length} features, but the model was trained with {_width}.", NameOf(x))
            End If

            Dim scores As Double() = New Double(_classes.Length - 1) {}

            For k As Integer = 0 To _classes.Length - 1
                scores(k) = _logPriors(k) + LogLikelihood(x, k)
            Next

            Return scores
        End Function

        ''' <summary>
        ''' 计算后验概率 P(Y|x)，返回顺序与 <see cref="Classes"/> 一致，各元素之和为 1。
        ''' </summary>
        ''' <param name="x">长度等于 <see cref="FeatureWidth"/> 的特征向量</param>
        Public Function Posterior(x As Double()) As Double()
            Dim scores As Double() = LogScores(x)
            Dim n As Integer = scores.Length
            Dim maxScore As Double = Double.NegativeInfinity

            For k As Integer = 0 To n - 1
                If Double.IsNaN(scores(k)) Then
                    scores(k) = Double.NegativeInfinity
                End If
                If scores(k) > maxScore Then
                    maxScore = scores(k)
                End If
            Next

            If Double.IsNaN(maxScore) OrElse Double.IsInfinity(maxScore) Then
                ' 所有类别的似然都退化时，退化为均匀分布，保证返回值始终可用
                Dim uniform As Double = 1.0 / n
                Dim fallback As Double() = New Double(n - 1) {}

                For k As Integer = 0 To n - 1
                    fallback(k) = uniform
                Next

                Return fallback
            End If

            ' log-sum-exp: exp(s - max) / sum(exp(s - max))
            Dim total As Double = 0.0

            For k As Integer = 0 To n - 1
                scores(k) = System.Math.Exp(scores(k) - maxScore)
                total += scores(k)
            Next
            For k As Integer = 0 To n - 1
                scores(k) /= total
            Next

            Return scores
        End Function

        ''' <summary>
        ''' 返回后验概率最大的类别标签。
        ''' </summary>
        Public Function Classify(x As Double()) As Integer
            Dim scores As Double() = LogScores(x)
            Dim best As Integer = 0

            For k As Integer = 1 To scores.Length - 1
                If scores(k) > scores(best) Then
                    best = k
                End If
            Next

            Return _classes(best)
        End Function

        Public Overrides Function ToString() As String
            Return $"{Me.GetType().Name}: {_sampleSize} samples, {_width} features, {_classes.Length} classes."
        End Function

    End Class
End Namespace
