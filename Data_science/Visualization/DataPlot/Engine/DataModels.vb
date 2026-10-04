' ---------------------------------------------------------------------------
'  DataPlot / Engine / DataModels.vb
'  Copyright (c) 2018-2026 sciBASIC.NET Foundation, GPL3 Licensed
'
'  This program is free software: you can redistribute it and/or modify
'  it under the terms of the GNU General Public License as published by
'  the Free Software Foundation, either version 3 of the License, or
'  (at your option) any later version.
' ---------------------------------------------------------------------------

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports std = System.Math

' ===========================================================================
'  本文件存放「解耦数据契约」：
'
'  DataPlot 只做绘制，不实现任何统计算法。聚类树、回归拟合、PCA 得分、相关性矩阵
'  这些由上层算法包（DataMining / ANOVA / DataFittings / ODE / stats 等）算好的结果，
'  通过这里定义的数据模型进入绘图管线。这样 DataPlot 的项目引用可以永远保持
'  imaging / Core / html / Math 四项，不会因为要画一张图而拖进整套算法依赖。
'
'  每个模型都只描述「结果是什么」，不涉及算法来源。
' ===========================================================================

''' <summary>
''' 层次聚类树节点。聚类算法由调用方完成，DataPlot 只消费树形结果用于绘制树状图 /
''' 聚类热图的行、列装饰。
''' </summary>
Public Class ClusterTreeNode

    ''' <summary>合并高度（绘制时决定横 / 竖向的分支长度）</summary>
    Public Property Height As Double = 0

    ''' <summary>叶节点名称（非叶节点可为 Nothing）</summary>
    Public Property Name As String = Nothing

    ''' <summary>左子树</summary>
    Public Property Left As ClusterTreeNode = Nothing

    ''' <summary>右子树</summary>
    Public Property Right As ClusterTreeNode = Nothing

    ''' <summary>该子树下所有叶子的名称（按绘制顺序）</summary>
    Public Property LeafNames As String() = Nothing

    ''' <summary>是否为叶节点</summary>
    Public ReadOnly Property IsLeaf As Boolean
        Get
            Return Left Is Nothing AndAlso Right Is Nothing
        End Get
    End Property

    ''' <summary>构造一个叶节点</summary>
    Public Shared Function Leaf(name As String) As ClusterTreeNode
        Return New ClusterTreeNode With {
            .Name = name,
            .Height = 0,
            .LeafNames = {name}
        }
    End Function

    ''' <summary>把两个子树合并成一个新节点</summary>
    Public Shared Function Join(left As ClusterTreeNode, right As ClusterTreeNode, height As Double) As ClusterTreeNode
        Return New ClusterTreeNode With {
            .Left = left,
            .Right = right,
            .Height = height,
            .LeafNames = left.Leaves.Concat(right.Leaves).ToArray()
        }
    End Function

    ''' <summary>收集子树下的所有叶子（递归）</summary>
    Public Function Leaves() As String()
        If LeafNames IsNot Nothing AndAlso LeafNames.Length > 0 Then Return LeafNames
        If IsLeaf Then Return If(Name Is Nothing, New String() {}, {Name})

        Dim list As New List(Of String)()
        If Left IsNot Nothing Then list.AddRange(Left.Leaves())
        If Right IsNot Nothing Then list.AddRange(Right.Leaves())

        LeafNames = list.ToArray()
        Return LeafNames
    End Function

    ''' <summary>子树的最大叶深，用于估算画布上的高度占用</summary>
    Public Function Depth() As Integer
        If IsLeaf Then Return 1
        Return 1 + std.Max(If(Left Is Nothing, 0, Left.Depth()), If(Right Is Nothing, 0, Right.Depth()))
    End Function
End Class

''' <summary>
''' 回归拟合结果。调用方把已经拟合好的结果包进来即可绘制拟合图，
''' DataPlot 不会自己去算回归。
''' </summary>
Public Interface IRegressionFit
    ''' <summary>自变量观测值</summary>
    ReadOnly Property X As Double()
    ''' <summary>因变量观测值</summary>
    ReadOnly Property Y As Double()
    ''' <summary>拟合值，与 <see cref="X"/> 等长</summary>
    ReadOnly Property Yfit As Double()
    ''' <summary>可选的拟合方程文本，用于图上标注（可为 Nothing）</summary>
    ReadOnly Property EquationText As String
    ''' <summary>可选的置信带下界（可为 Nothing）</summary>
    ReadOnly Property BandLower As Double()
    ''' <summary>可选的置信带上界（可为 Nothing）</summary>
    ReadOnly Property BandUpper As Double()
End Interface

''' <summary><see cref="IRegressionFit"/> 的即开即用实现，方便手工构造拟合结果</summary>
Public Class RegressionFit : Implements IRegressionFit

    Public Property X As Double() = {}
    Public Property Y As Double() = {}
    Public Property Yfit As Double() = {}
    Public Property EquationText As String = Nothing
    Public Property BandLower As Double() = Nothing
    Public Property BandUpper As Double() = Nothing
    ''' <summary>拟合曲线的颜色（留空时取主题调色板）</summary>
    Public Property Color As Color? = Nothing
    ''' <summary>图例标题</summary>
    Public Property Name As String = "fit"

    Private ReadOnly Property Fit_X As Double() Implements IRegressionFit.X
        Get
            Return X
        End Get
    End Property
    Private ReadOnly Property Fit_Y As Double() Implements IRegressionFit.Y
        Get
            Return Y
        End Get
    End Property
    Private ReadOnly Property Fit_Yfit As Double() Implements IRegressionFit.Yfit
        Get
            Return Yfit
        End Get
    End Property
    Private ReadOnly Property Fit_EquationText As String Implements IRegressionFit.EquationText
        Get
            Return EquationText
        End Get
    End Property
    Private ReadOnly Property Fit_BandLower As Double() Implements IRegressionFit.BandLower
        Get
            Return BandLower
        End Get
    End Property
    Private ReadOnly Property Fit_BandUpper As Double() Implements IRegressionFit.BandUpper
        Get
            Return BandUpper
        End Get
    End Property
End Class

''' <summary>
''' PCA 分析结果。 <see cref="Contributions"/> 同时用于碎石图，因此得分图与碎石图共用该模型。
''' </summary>
Public Interface IPCAScore
    ''' <summary>样本名</summary>
    ReadOnly Property SampleNames As String()
    ''' <summary>每个样本的分组标签（可与 <see cref="SampleNames"/> 等长，也可为空）</summary>
    ReadOnly Property Groups As String()
    ''' <summary>第一主成分得分</summary>
    ReadOnly Property PC1 As Double()
    ''' <summary>第二主成分得分</summary>
    ReadOnly Property PC2 As Double()
    ''' <summary>各主成分贡献率（0~1），供碎石图使用</summary>
    ReadOnly Property Contributions As Double()
End Interface

''' <summary><see cref="IPCAScore"/> 的即开即用实现</summary>
Public Class PCAScoreSet : Implements IPCAScore

    Public Property SampleNames As String() = {}
    Public Property Groups As String() = {}
    Public Property PC1 As Double() = {}
    Public Property PC2 As Double() = {}
    Public Property Contributions As Double() = {}

    Private ReadOnly Property Score_Names As String() Implements IPCAScore.SampleNames
        Get
            Return SampleNames
        End Get
    End Property
    Private ReadOnly Property Score_Groups As String() Implements IPCAScore.Groups
        Get
            Return Groups
        End Get
    End Property
    Private ReadOnly Property Score_PC1 As Double() Implements IPCAScore.PC1
        Get
            Return PC1
        End Get
    End Property
    Private ReadOnly Property Score_PC2 As Double() Implements IPCAScore.PC2
        Get
            Return PC2
        End Get
    End Property
    Private ReadOnly Property Score_Contributions As Double() Implements IPCAScore.Contributions
        Get
            Return Contributions
        End Get
    End Property
End Class

''' <summary>
''' 相关性矩阵。数据由调用方算好（Pearson / Spearman 等任意度量），
''' 这里只承载矩阵本身与行列名。
''' </summary>
Public Class CorrelationMatrix

    ''' <summary>变量名（行列共用同一组名称）</summary>
    Public Property Names As String() = {}

    ''' <summary>names × names 的系数矩阵，取值通常为 -1~1</summary>
    Public Property Matrix As Double(,) = Nothing

    ''' <summary>获取 / 设置 i,j 位置的相关系数</summary>
    Default Public Property Item(i As Integer, j As Integer) As Double
        Get
            Validate(i, j)
            Return Matrix(i, j)
        End Get
        Set(value As Double)
            Validate(i, j)
            Matrix(i, j) = value
        End Set
    End Property

    ''' <summary>矩阵阶数</summary>
    Public ReadOnly Property Size As Integer
        Get
            Return If(Names Is Nothing, 0, Names.Length)
        End Get
    End Property

    Private Sub Validate(i As Integer, j As Integer)
        If Matrix Is Nothing Then Throw New InvalidOperationException("Correlation matrix is empty.")
        If i < 0 OrElse i >= Size OrElse j < 0 OrElse j >= Size Then
            Throw New IndexOutOfRangeException($"({i},{j}) is outside of the {Size}x{Size} correlation matrix.")
        End If
    End Sub

    ''' <summary>校验矩阵与名称表尺寸是否自洽</summary>
    Public Function ValidateShape() As Boolean
        If Matrix Is Nothing OrElse Names Is Nothing Then Return False
        Return Matrix.GetLength(0) = Names.Length AndAlso Matrix.GetLength(1) = Names.Length
    End Function

    ''' <summary>把对角线强制为 1（相关系数矩阵自相关恒为 1）</summary>
    Public Sub ForceDiagonalOne()
        If Matrix Is Nothing Then Return
        For i = 0 To Size - 1
            Matrix(i, i) = 1
        Next
    End Sub
End Class

''' <summary>分组样本，用于箱线图 / 小提琴图 / 双直方图 / 抖动图的分类输入</summary>
Public Class CategoryGroup

    Public Property Name As String = ""
    Public Property Data As Double() = {}
    ''' <summary>留空时按主题调色板取色</summary>
    Public Property Color As Color? = Nothing
End Class

''' <summary>ROC 曲线数据。TPR/FPR 由调用方根据阈值扫描算出，AUC 同理。</summary>
Public Class ROCCurve

    Public Property Name As String = "ROC"
    ''' <summary>假阳性率（FPR = 1 - Specificity）</summary>
    Public Property FPR As Double() = {}
    ''' <summary>真阳性率（TPR = Sensitivity）</summary>
    Public Property TPR As Double() = {}
    ''' <summary>对应的判定阈值（可选，用于图上标注）</summary>
    Public Property Thresholds As Double() = {}
    ''' <summary>曲线下面积</summary>
    Public Property AUC As Double = Double.NaN
    ''' <summary>曲线颜色</summary>
    Public Property Color As Color? = Nothing

    ''' <summary>由 sensitivity / specificity 构造 ROC（内部做 1-specificity 换算）</summary>
    Public Shared Function FromRates(sensitivity As IEnumerable(Of Double),
                                     specificity As IEnumerable(Of Double),
                                     Optional auc As Double = Double.NaN,
                                     Optional name As String = "ROC") As ROCCurve
        Dim tpr = sensitivity.SafeQuery.ToArray()
        Dim fpr = specificity.SafeQuery.Select(Function(sp) 1 - sp).ToArray()

        Return New ROCCurve With {
            .Name = name,
            .TPR = tpr,
            .FPR = fpr,
            .AUC = auc
        }
    End Function
End Class

''' <summary>森林图的一行：效应量与置信区间</summary>
Public Class ForestEntry

    Public Property Label As String = ""
    ''' <summary>效应量点估计</summary>
    Public Property Effect As Double = 0
    ''' <summary>置信区间下界</summary>
    Public Property Lower As Double = Double.NaN
    ''' <summary>置信区间上界</summary>
    Public Property Upper As Double = Double.NaN
    ''' <summary>可选的方块尺寸（用于 meta 分析的权重），小于等于 0 时按统一尺寸绘制</summary>
    Public Property Weight As Double = 0
    ''' <summary>可选的附加文本（如 HR 95% CI）</summary>
    Public Property Note As String = ""
End Class

''' <summary>曼哈顿图的一个位点</summary>
Public Class ManhattanPoint

    ''' <summary>染色体 / 分组名</summary>
    Public Property Chromosome As String = ""
    ''' <summary>位点在染色体上的坐标</summary>
    Public Property Position As Double = 0
    ''' <summary>原始 p 值（绘制时取 -log10）</summary>
    Public Property PValue As Double = 1
    ''' <summary>可选的位点标签，达到标签阈值时才会绘制</summary>
    Public Property Label As String = ""
End Class

''' <summary>双直方图的一组输入</summary>
Public Class BiHistogramSample

    Public Property Name As String = ""
    Public Property Data As Double() = {}
    Public Property Color As Color? = Nothing
End Class

''' <summary>变宽条形图的单根条：宽度不再固定，而是承载第二个维度的信息</summary>
Public Class VariableBarData

    Public Property Name As String = ""
    ''' <summary>条的高度（数值）</summary>
    Public Property Value As Double = 0
    ''' <summary>条的宽度（通常是另一维度的量，如样本量）</summary>
    Public Property Width As Double = 0
    Public Property Color As Color? = Nothing
End Class

''' <summary>时间趋势图的一个时间采样点</summary>
Public Structure TimePoint

    Public Property Time As Double
    Public Property Value As Double
    Public Property Label As String

    Public Sub New(time As Double, value As Double, Optional label As String = Nothing)
        Me.Time = time
        Me.Value = value
        Me.Label = If(label, "")
    End Sub
End Structure

''' <summary>多边形组：一个标签对应若干闭合多边形子区域</summary>
Public Class PolygonGroup

    Public Property Label As String = ""
    ''' <summary>每个元素是一个闭合多边形的顶点序列</summary>
    Public Property SubRegions As PointF()() = {}
    Public Property Color As Color? = Nothing
    ''' <summary>填充透明度（0~1）</summary>
    Public Property Alpha As Double = 0.5
End Class

''' <summary>自定义刷子的单根柱：允许调用方完全控制柱子的样式</summary>
Public Class BarSerial

    Public Property Label As String = ""
    Public Property Value As Double = 0
    ''' <summary>填充色，留空时用主题调色板</summary>
    Public Property Brush As Color? = Nothing
    Public Property Color As Color? = Nothing
End Class

''' <summary>ODE 数值解的一条曲线</summary>
Public Class ODESeries

    Public Property Name As String = ""
    Public Property X As Double() = {}
    Public Property Y As Double() = {}
    Public Property Color As Color? = Nothing
End Class

''' <summary>Z-score 图的一行：一个观测对象在多个变量上的标准化取值</summary>
Public Class ZScoreEntry

    Public Property Name As String = ""
    ''' <summary>与变量值等长的 Z 值</summary>
    Public Property Values As Double() = {}
    Public Property Group As String = ""
    Public Property Color As Color? = Nothing
End Class

''' <summary>
''' 文氏图的一个集合。<paramref name="Intersections"/> 的 key 是另一个集合的
''' <see cref="Name"/>，value 是与该集合的交集元素数量。
''' </summary>
Public Class VennSet

    Public Property Name As String = ""
    ''' <summary>集合元素数量（决定圆的大小）</summary>
    Public Property Size As Integer = 0
    ''' <summary>与其它集合的两两交集数量，key = 对方集合名</summary>
    Public Property Intersections As New Dictionary(Of String, Integer)()
    ''' <summary>圆的颜色，留空时取主题调色板</summary>
    Public Property Color As Color? = Nothing

    ''' <summary>取与指定集合的交集数量，未登记时返回 0</summary>
    Public Function IntersectWith(otherName As String) As Integer
        If Intersections Is Nothing OrElse Not Intersections.ContainsKey(otherName) Then Return 0
        Return Intersections(otherName)
    End Function

    ''' <summary>对称补齐：保证 a∩b 与 b∩a 都能查到</summary>
    Public Shared Sub FixSetCompleteness(sets As IList(Of VennSet))
        If sets Is Nothing Then Return

        For i = 0 To sets.Count - 1
            For j = 0 To sets.Count - 1
                If i = j Then Continue For

                Dim a = sets(i), b = sets(j)
                If Not a.Intersections.ContainsKey(b.Name) Then
                    a.Intersections(b.Name) = b.IntersectWith(a.Name)
                End If
            Next
        Next
    End Sub
End Class

''' <summary>条形图数据组里的一个分组：分组名 + 每个序列在该组下的取值</summary>
Public Class BarDataSample : Implements INamedValue

    ''' <summary>分组名称</summary>
    Public Property tag As String Implements INamedValue.Key
    ''' <summary>当前分组下每个序列的数据值</summary>
    Public Property data As Double() = {}
    ''' <summary>该分组下所有序列的求和（堆叠柱的总高度）</summary>
    Public ReadOnly Property StackedSum As Double
        Get
            Return If(data Is Nothing, 0.0, data.Sum())
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"{tag} = {If(data Is Nothing, 0, data.Length)} series"
    End Function
End Class

''' <summary>条形图数据组：若干分组 + 每个序列的配色</summary>
Public Class BarDataGroup

    ''' <summary>序列名与配色，顺序与 <see cref="BarDataSample.data"/> 一致</summary>
    Public Property Serials As NamedValue(Of Color)() = {}
    ''' <summary>分组数据</summary>
    Public Property Samples As BarDataSample() = {}

    ''' <summary>按各组的平均值降序重排</summary>
    Public Function Desc() As BarDataGroup
        Dim order = Samples _
            .OrderByDescending(Function(s) s.data.Average()) _
            .Select(Function(s) s.tag) _
            .ToArray()

        Return Reorder(order)
    End Function

    ''' <summary>按给定的分组名顺序重排样本</summary>
    Public Function Reorder(order As String()) As BarDataGroup
        Dim index As New Dictionary(Of String, BarDataSample)()
        For Each s In Samples
            index(s.tag) = s
        Next

        Dim list As New List(Of BarDataSample)()
        For Each name In order
            If index.ContainsKey(name) Then list.Add(index(name))
        Next

        Return New BarDataGroup With {.Serials = Me.Serials, .Samples = list.ToArray()}
    End Function

    ''' <summary>转成 [序列, 分类] 的二维矩阵，便于直接喂给 <see cref="BarPlot"/></summary>
    Public Function ToMatrix() As Double(,)
        Dim nSer = Serials.Length
        Dim nSample = Samples.Length
        Dim out(nSer - 1, nSample - 1) As Double

        For i = 0 To nSample - 1
            For j = 0 To std.Min(nSer, Samples(i).data.Length) - 1
                out(j, i) = Samples(i).data(j)
            Next
        Next

        Return out
    End Function
End Class

''' <summary>样本正态性视图需要的矩估计结果（由统计量被调用方填进来）</summary>
Public Class SampleMoments

    Public Property SampleName As String = ""
    Public Property Mean As Double = 0
    Public Property StandardDeviation As Double = 1
    Public Property Data As Double() = {}
End Class
