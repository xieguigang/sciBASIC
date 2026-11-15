' Copyright (c) sciBASIC.NET Foundation
' GNU GENERAL PUBLIC LICENSE (GPL3) or later
'
' Original authors: asuka (amethyst.asuka@gcmodeller.org), xie (genetics@smrucc.org),
'                   xieguigang (xie.guigang@live.com)

Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualBasic.Data
' 项目内存在 Microsoft.VisualBasic.Math 命名空间，会遮蔽 System.Math，
' 因此数值函数一律使用完全限定名 System.Math.*。

Namespace Kernel.Classifier

    ''' <summary>
    ''' 朴素贝叶斯分类器（离散/分类型特征）。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 相对于旧实现的修正：
    ''' </para>
    ''' <list type="bullet">
    ''' <item>条件概率按「该类别的样本数」归一，而不是全部样本数；</item>
    ''' <item>加入可配置的 Laplace / 加性平滑，未见过的特征值不再使整个似然归零；</item>
    ''' <item>训练期一次性构建「类别 × 维度 × 取值」频率表并缓存先验，
    '''       预测期只做 O(K·D) 的字典查表，消除原来 O(K·D·N) 的嵌套全表扫描；</item>
    ''' <item>推断在 log 域完成并经 log-sum-exp 归一化，避免多特征连乘下溢；</item>
    ''' <item>输入改用框架统一的 <see cref="NumericTable"/> 数值表，
    '''       不再依赖已弃用的 <c>IntegerEntity</c>。</item>
    ''' </list>
    ''' <para>
    ''' 离散路径假定每个特征值都是整数类别编码，非整数值会按
    ''' <see cref="MidpointRounding.AwayFromZero"/> 四舍五入为整数键。
    ''' 连续特征请改用 Gaussian 朴素贝叶斯实现。
    ''' </para>
    ''' </remarks>
    Public Class Bayesian : Inherits NaiveBayesModel

        ''' <summary>
        ''' 频率表数组，下标 = classIndex * width + featureIndex，
        ''' 字典的键为离散化的特征值，值为该类别该维度上出现的次数。
        ''' </summary>
        Private ReadOnly _tables As Dictionary(Of Long, Integer)()
        ''' <summary>每个维度在全部训练数据中出现的不同取值个数</summary>
        Private ReadOnly _distinct As Integer()
        ''' <summary>加性平滑系数 alpha，必须为正的有限值</summary>
        Private ReadOnly _smoothing As Double

        ''' <summary>平滑系数的默认值</summary>
        Public Const DefaultSmoothing As Double = 1.0

        Private Sub New(table As NumericTable, classColumn As Integer, smoothing As Double)
            MyBase.New(table, classColumn)

            If Double.IsNaN(smoothing) OrElse Double.IsInfinity(smoothing) OrElse smoothing <= 0.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(smoothing),
                    "the additive smoothing factor must be a positive finite number.")
            End If

            _smoothing = smoothing

            Dim slots As Integer = _classes.Length * _width
            _tables = New Dictionary(Of Long, Integer)(slots - 1) {}

            For i As Integer = 0 To slots - 1
                _tables(i) = New Dictionary(Of Long, Integer)()
            Next

            Dim distinct As HashSet(Of Long)() = New HashSet(Of Long)(_width - 1) {}

            For j As Integer = 0 To _width - 1
                distinct(j) = New HashSet(Of Long)()
            Next

            ' 单次遍历完成全部计数，不在预测期重新扫描训练数据
            For i As Integer = 0 To _sampleSize - 1
                Dim classOffset As Integer = _classIndex(i) * _width
                Dim row As Double() = _features(i)

                For j As Integer = 0 To _width - 1
                    Dim key As Long = DiscreteKey(row(j))
                    Dim counts As Dictionary(Of Long, Integer) = _tables(classOffset + j)
                    Dim n As Integer = 0

                    If counts.TryGetValue(key, n) Then
                        counts(key) = n + 1
                    Else
                        counts.Add(key, 1)
                    End If

                    Call distinct(j).Add(key)
                Next
            Next

            _distinct = New Integer(_width - 1) {}

            For j As Integer = 0 To _width - 1
                _distinct(j) = distinct(j).Count
            Next
        End Sub

        ''' <summary>
        ''' 从已持久化的频率表直接恢复分类器，不做任何训练数据扫描。
        ''' </summary>
        ''' <param name="classes">升序排列的类别标签</param>
        ''' <param name="logPriors">各类别的 log 先验</param>
        ''' <param name="classCounts">各类别的训练样本数</param>
        ''' <param name="width">特征维度</param>
        ''' <param name="sampleSize">训练样本总数（仅用于诊断输出）</param>
        ''' <param name="tables">
        ''' 频率表，长度必须等于 <c>classes.Length * width</c>，
        ''' 下标 = classIndex * width + featureIndex
        ''' </param>
        ''' <param name="distinct">每个维度在训练数据中出现过的不同取值个数，长度必须等于 <paramref name="width"/></param>
        ''' <param name="smoothing">加性平滑系数</param>
        Private Sub New(classes As Integer(), logPriors As Double(), classCounts As Integer(),
                        width As Integer, sampleSize As Integer,
                        tables As Dictionary(Of Long, Integer)(),
                        distinct As Integer(),
                        smoothing As Double)
            MyBase.New(classes, logPriors, classCounts, width, sampleSize)

            If Double.IsNaN(smoothing) OrElse Double.IsInfinity(smoothing) OrElse smoothing <= 0.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(smoothing),
                    "the additive smoothing factor must be a positive finite number.")
            End If
            If tables Is Nothing Then
                Throw New ArgumentNullException(NameOf(tables))
            End If
            If tables.Length <> classes.Length * width Then
                Throw New ArgumentException(
                    $"the frequency table count ({tables.Length}) does not match classes * width ({classes.Length} * {width})!",
                    NameOf(tables))
            End If
            If distinct Is Nothing OrElse distinct.Length <> width Then
                Throw New ArgumentException(
                    $"the distinct value count ({If(distinct Is Nothing, 0, distinct.Length)}) " &
                    $"does not match the feature width ({width})!", NameOf(distinct))
            End If

            For i As Integer = 0 To tables.Length - 1
                If tables(i) Is Nothing Then
                    Throw New ArgumentException($"the frequency table at index {i} is NULL!", NameOf(tables))
                End If
            Next

            Me._tables = CType(tables.Clone(), Dictionary(Of Long, Integer)())
            Me._distinct = CType(distinct.Clone(), Integer())
            Me._smoothing = smoothing
        End Sub

        ''' <summary>
        ''' 从已保存的模型状态恢复朴素贝叶斯分类器（用于模型持久化加载）。
        ''' </summary>
        ''' <remarks>
        ''' 恢复后的分类器与保存前的预测结果逐位等价；但不再持有训练集，
        ''' 因此无法重新训练或继续做增量统计。
        ''' </remarks>
        Public Shared Function Restore(classes As Integer(),
                                       logPriors As Double(),
                                       classCounts As Integer(),
                                       width As Integer,
                                       sampleSize As Integer,
                                       tables As Dictionary(Of Long, Integer)(),
                                       distinct As Integer(),
                                       Optional smoothing As Double = DefaultSmoothing) As Bayesian
            Return New Bayesian(classes, logPriors, classCounts, width, sampleSize,
                                tables, distinct, smoothing)
        End Function

        ''' <summary>
        ''' 条件频率表（下标 = classIndex * FeatureWidth + featureIndex）。
        ''' 仅供模型持久化使用，请勿修改返回的数组或者其中的字典。
        ''' </summary>
        Public ReadOnly Property Tables As Dictionary(Of Long, Integer)()
            Get
                Return _tables
            End Get
        End Property

        ''' <summary>
        ''' 每个特征维度在训练数据中出现过的不同取值个数。
        ''' 仅供模型持久化使用，请勿修改返回的数组。
        ''' </summary>
        Public ReadOnly Property DistinctCounts As Integer()
            Get
                Return _distinct
            End Get
        End Property

        ''' <summary>加性平滑系数</summary>
        Public ReadOnly Property Smoothing As Double
            Get
                Return _smoothing
            End Get
        End Property

        ''' <summary>
        ''' 把离散特征值规范化为整数类别键。
        ''' </summary>
        Private Shared Function DiscreteKey(value As Double) As Long
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
                Throw New ArgumentException(
                    $"feature value '{value}' is not a finite number, the discrete naive bayes path requires numeric codes.")
            End If

            Return CLng(System.Math.Round(value, MidpointRounding.AwayFromZero))
        End Function

        ''' <summary>
        ''' log P(x|Y=classIndex)，条件概率按类别样本数归一并做加性平滑：
        ''' (count + alpha) / (N_y + alpha * (V_j + 1))。
        ''' </summary>
        Protected Overrides Function LogLikelihood(x As Double(), classIndex As Integer) As Double
            Dim sum As Double = 0.0
            Dim classSize As Double = _classCounts(classIndex)
            Dim classOffset As Integer = classIndex * _width

            For j As Integer = 0 To _width - 1
                Dim key As Long = DiscreteKey(x(j))
                Dim count As Integer = 0

                Call _tables(classOffset + j).TryGetValue(key, count)

                ' V_j + 1：额外预留一个平滑槽位给训练时未见过的特征值
                Dim denominator As Double = classSize + _smoothing * (_distinct(j) + 1)
                Dim probability As Double = (count + _smoothing) / denominator

                sum += System.Math.Log(probability)
            Next

            Return sum
        End Function

        ''' <summary>
        ''' 从统一数值表构建分类器。
        ''' </summary>
        ''' <param name="table">特征矩阵 + 整数类别标签列的数值表</param>
        ''' <param name="classColumn">类别标签列的索引，默认第 0 列</param>
        ''' <param name="smoothing">加性平滑系数，默认 1.0（Laplace 平滑）</param>
        Public Shared Function Load(table As NumericTable,
                                    Optional classColumn As Integer = 0,
                                    Optional smoothing As Double = DefaultSmoothing) As Bayesian

            Return New Bayesian(table, classColumn, smoothing)
        End Function

        ''' <summary>
        ''' 从统一数值表构建分类器，按标签列名称定位类别列。
        ''' </summary>
        Public Shared Function Load(table As NumericTable,
                                    classColumn As String,
                                    Optional smoothing As Double = DefaultSmoothing) As Bayesian

            Return New Bayesian(table, FindLabelColumn(table, classColumn), smoothing)
        End Function

        ''' <summary>
        ''' 直接由特征矩阵与整数类别向量构建分类器。
        ''' </summary>
        Public Shared Function Load(x As Double()(),
                                    y As Integer(),
                                    Optional smoothing As Double = DefaultSmoothing) As Bayesian

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

            Return New Bayesian(New NumericTable(x, y), 0, smoothing)
        End Function

        ''' <summary>
        ''' P(X|Y=y)，即条件似然。特征维度较多时该值可能下溢为 0，
        ''' 需要稳定计算时请使用 <see cref="LogP"/> 或者 <see cref="NaiveBayesModel.Posterior"/>。
        ''' </summary>
        ''' <param name="x">Subject condition</param>
        ''' <param name="y">Target class</param>
        Public Function P(x As Double(), y As Integer) As Double
            Return System.Math.Exp(LogP(x, y))
        End Function

        ''' <summary>
        ''' log P(X|Y=y)，在 log 域返回条件似然，不会发生连乘下溢。
        ''' 类别标签不存在时返回负无穷。
        ''' </summary>
        Public Function LogP(x As Double(), y As Integer) As Double
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If x.Length <> FeatureWidth Then
                Throw New ArgumentException(
                    $"the input vector has {x.Length} features, but the model was trained with {FeatureWidth}.", NameOf(x))
            End If

            Dim classIndex As Integer = ClassIndexOf(y)

            If classIndex < 0 Then
                Return Double.NegativeInfinity
            End If

            Return LogLikelihood(x, classIndex)
        End Function

        Public Overrides Function ToString() As String
            Return $"Bayesian Classifier, {SampleSize} Entities, {FeatureWidth} properties and {ClassCount} classes (alpha={_smoothing})."
        End Function

    End Class
End Namespace
