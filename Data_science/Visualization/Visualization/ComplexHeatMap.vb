Imports System.Drawing
Imports System.Text
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering
Imports Microsoft.VisualBasic.Data.Plots
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports FontStyle = Microsoft.VisualBasic.Imaging.FontStyle
Imports std = System.Math
Imports Microsoft.VisualBasic.Math.Correlations

' ---------------------------------------------------------------------------
'  Visualization / ComplexHeatMap.vb
'  Copyright (c) 2018-2026 sciBASIC.NET Foundation, GPL3 Licensed
'
'  复合热图（Complex heatmap）：把「行列层次聚类树 + 行列颜色标签注释 +
'  热图主体 + 连续/离散图例 + 行列标签」拼装在同一张画布之上，
'  版式与 R 的 pheatmap / ComplexHeatmap 输出保持一致，可直接用于论文插图。
' ---------------------------------------------------------------------------

''' <summary>
''' 热图数据的标准化方式（z-score）。
''' </summary>
Public Enum HeatMapScale
    ''' <summary>
    ''' 不做标准化，直接使用原始数值。
    ''' </summary>
    None = 0
    ''' <summary>
    ''' 按行标准化：<c>(x - mean(row)) / sd(row)</c>，标准差取样本标准差（n-1，与 R 的 <c>scale()</c> 一致）。
    ''' </summary>
    Row = 1
    ''' <summary>
    ''' 按列标准化：<c>(x - mean(col)) / sd(col)</c>。
    ''' </summary>
    Column = 2
End Enum

''' <summary>
''' 层次聚类所使用的连接策略。
''' </summary>
Public Enum LinkageMethod
    ''' <summary>单连接（最近邻）。</summary>
    SingleLinkage = 0
    ''' <summary>全连接（最远邻）。</summary>
    CompleteLinkage = 1
    ''' <summary>平均连接（UPGMA）。</summary>
    AverageLinkage = 2
    ''' <summary>加权平均连接（WPGMA）。</summary>
    WeightedLinkage = 3
    ''' <summary>
    ''' Ward.D2 最小方差法（与 R 的 <c>hclust(method = "ward.D2")</c> 一致）。
    '''
    ''' 既有的 <c>LinkageStrategy</c> 接口只传递两个距离值、拿不到簇大小，无法表达
    ''' Ward 准则，因此该策略由本模块内部实现。
    ''' </summary>
    Ward = 4
End Enum

''' <summary>
''' 颜色标签注释的数据类型。
''' </summary>
Public Enum AnnotationKind
    ''' <summary>
    ''' 自动判定：<see cref="HeatMapAnnotation.Values"/> 非空时为离散分类，
    ''' 否则按 <see cref="HeatMapAnnotation.NumericValues"/> 作连续数值处理。
    ''' </summary>
    Auto = 0
    ''' <summary>离散分类（色块 + 分类图例）。</summary>
    Discrete = 1
    ''' <summary>连续数值（渐变条 + 迷你色标）。</summary>
    Continuous = 2
End Enum

''' <summary>
''' 热图行/列的颜色标签注释条。
'''
''' <para>
''' 同一个类型同时承载两种注释：<see cref="Values"/>（离散分类，例如品种 Variety）与
''' <see cref="NumericValues"/>（连续数值，例如 pH、含量），可以挂在列方向
''' （<see cref="ComplexHeatMap.ColAnnotations"/>，画在热图上方）或者行方向
''' （<see cref="ComplexHeatMap.RowAnnotations"/>，画在热图左侧）。
''' </para>
''' </summary>
Public Class HeatMapAnnotation

    ''' <summary>
    ''' 注释的名称，会作为图例标题显示（例如 <c>Variety</c>）。
    ''' </summary>
    Public Property Name As String

    ''' <summary>
    ''' 离散注释值（长度必须与行数或者列数一致）。
    ''' </summary>
    Public Property Values As String()

    ''' <summary>
    ''' 连续注释值（长度必须与行数或者列数一致）。
    ''' </summary>
    Public Property NumericValues As Double()

    ''' <summary>
    ''' 颜色方案。
    '''
    ''' 离散注释中它是候选色板（未被 <see cref="ColorMap"/> 显式指定的类别按顺序取色）；
    ''' 连续注释中它是渐变的颜色停靠点（从最小值到最大值）。
    ''' </summary>
    Public Property Colors As Color()

    ''' <summary>
    ''' 离散注释的显式「类别 → 颜色」映射（优先级高于 <see cref="Colors"/>）。
    ''' </summary>
    Public Property ColorMap As Dictionary(Of String, Color)

    ''' <summary>
    ''' 数据类型，缺省 <see cref="AnnotationKind.Auto"/> 自动判定。
    ''' </summary>
    Public Property Kind As AnnotationKind = AnnotationKind.Auto

    ''' <summary>
    ''' 是否在右侧图例区显示该注释的图例。
    ''' </summary>
    Public Property ShowInLegend As Boolean = True

    ''' <summary>
    ''' 缺失值（空字符串 / <see cref="Double.NaN"/>）所使用的颜色。
    ''' </summary>
    Public Property MissingColor As Color = Color.FromArgb(216, 216, 216)

    Private _colorsCache As Dictionary(Of String, Color) = Nothing
    Private _colorsPalette As Color() = Nothing
    Private _rangeCache As (min As Double, max As Double)
    Private _rangeValid As Boolean = False

    ''' <summary>
    ''' 创建一个离散（分类）注释。
    ''' </summary>
    ''' <param name="name">注释名称（图例标题）</param>
    ''' <param name="values">与行数或列数一致的分类值</param>
    ''' <param name="colors">可选的候选色板</param>
    Public Shared Function Discrete(name As String, values As String(), Optional colors As Color() = Nothing) As HeatMapAnnotation
        Return New HeatMapAnnotation With {
            .Name = name,
            .Values = values,
            .Colors = colors,
            .Kind = AnnotationKind.Discrete
        }
    End Function

    ''' <summary>
    ''' 创建一个离散注释，并显式指定「类别 → 颜色」映射。
    ''' </summary>
    ''' <param name="name">注释名称（图例标题）</param>
    ''' <param name="values">与行数或列数一致的分类值</param>
    ''' <param name="map">类别到颜色的映射</param>
    Public Shared Function Discrete(name As String, values As String(), map As Dictionary(Of String, Color)) As HeatMapAnnotation
        Return New HeatMapAnnotation With {
            .Name = name,
            .Values = values,
            .ColorMap = map,
            .Kind = AnnotationKind.Discrete
        }
    End Function

    ''' <summary>
    ''' 创建一个连续（数值）注释。
    ''' </summary>
    ''' <param name="name">注释名称（图例标题）</param>
    ''' <param name="values">与行数或列数一致的数值</param>
    ''' <param name="colors">可选的渐变停靠点</param>
    Public Shared Function Continuous(name As String, values As Double(), Optional colors As Color() = Nothing) As HeatMapAnnotation
        Return New HeatMapAnnotation With {
            .Name = name,
            .NumericValues = values,
            .Colors = colors,
            .Kind = AnnotationKind.Continuous
        }
    End Function

    ''' <summary>
    ''' 解析出实际的注释类型（把 <see cref="AnnotationKind.Auto"/> 展开成离散或者连续）。
    ''' </summary>
    Public Function ResolvedKind() As AnnotationKind
        If Kind <> AnnotationKind.Auto Then
            Return Kind
        End If
        If Values IsNot Nothing Then
            Return AnnotationKind.Discrete
        End If

        Return AnnotationKind.Continuous
    End Function

    ''' <summary>
    ''' 逐个数据项的颜色（<paramref name="idx"/> 为原始行列下标）。
    ''' </summary>
    ''' <param name="idx">原始（未重排的）行列下标</param>
    ''' <param name="palette">调用方（主题）提供的后备色板</param>
    Public Function ColorAt(idx As Integer, palette As Color()) As Color
        If ResolvedKind() = AnnotationKind.Discrete Then
            If Values Is Nothing OrElse idx < 0 OrElse idx >= Values.Length Then
                Return MissingColor
            End If

            Dim key As String = Values(idx)

            If String.IsNullOrEmpty(key) Then
                Return MissingColor
            End If

            Dim color As Color

            If ResolvedColors(palette).TryGetValue(key, color) Then
                Return color
            End If

            Return MissingColor
        Else
            If NumericValues Is Nothing OrElse idx < 0 OrElse idx >= NumericValues.Length Then
                Return MissingColor
            End If

            Dim v As Double = NumericValues(idx)

            If Double.IsNaN(v) Then
                Return MissingColor
            End If

            Dim range = NumericRange()
            Dim stops As Color() = ContinuousColors()

            If range.max <= range.min Then
                Return ColorScale.LerpPalette(0.5, stops)
            Else
                Return ColorScale.LerpPalette((v - range.min) / (range.max - range.min), stops)
            End If
        End If
    End Function

    ''' <summary>
    ''' 连续注释的取值范围（忽略 NaN 与无穷大）。
    ''' </summary>
    Public Function NumericRange() As (min As Double, max As Double)
        If _rangeValid Then
            Return _rangeCache
        End If

        Dim lo As Double = Double.MaxValue
        Dim hi As Double = Double.MinValue

        If NumericValues IsNot Nothing Then
            For Each v As Double In NumericValues
                If Double.IsNaN(v) OrElse Double.IsInfinity(v) Then
                    Continue For
                End If
                If v < lo Then lo = v
                If v > hi Then hi = v
            Next
        End If

        If lo > hi Then
            _rangeCache = (0.0, 1.0)
        Else
            _rangeCache = (lo, hi)
        End If

        _rangeValid = True

        Return _rangeCache
    End Function

    ''' <summary>
    ''' 连续注释的渐变颜色停靠点（缺省为蓝色单色渐变）。
    ''' </summary>
    Public Function ContinuousColors() As Color()
        If Colors IsNot Nothing AndAlso Colors.Length > 0 Then
            Return Colors
        End If

        Return {
            Color.FromArgb(247, 251, 255),
            Color.FromArgb(198, 219, 239),
            Color.FromArgb(107, 174, 214),
            Color.FromArgb(33, 113, 181),
            Color.FromArgb(8, 48, 107)
        }
    End Function

    ''' <summary>
    ''' 离散注释的「类别 → 颜色」完整映射（未被显式指定的类别按 <paramref name="palette"/> 顺序补足）。
    ''' </summary>
    ''' <param name="palette">后备色板</param>
    Public Function ResolvedColors(palette As Color()) As Dictionary(Of String, Color)
        ' 缓存的失效条件：色板换了（按引用比较）。
        ' 单元格是逐个取色的，这里必须缓存，否则每一格都要重新建立一次类别映射表。
        If _colorsCache IsNot Nothing AndAlso _colorsPalette Is palette Then
            Return _colorsCache
        End If

        Dim map As New Dictionary(Of String, Color)
        Dim cats As String() = Categories()

        If ColorMap IsNot Nothing Then
            For Each key As String In cats
                Dim color As Color

                If ColorMap.TryGetValue(key, color) Then
                    map(key) = color
                End If
            Next
        End If

        Dim pool As Color() = Colors

        If pool Is Nothing OrElse pool.Length = 0 Then
            pool = palette
        End If
        If pool Is Nothing OrElse pool.Length = 0 Then
            pool = {Color.Gray}
        End If

        Dim i As Integer = 0

        For Each key As String In cats
            If Not map.ContainsKey(key) Then
                map(key) = pool(i Mod pool.Length)
                i += 1
            End If
        Next

        _colorsCache = map
        _colorsPalette = palette

        Return map
    End Function

    ''' <summary>
    ''' 离散注释的全部类别（按首次出现的顺序）。
    ''' </summary>
    Public Function Categories() As String()
        If Values Is Nothing Then
            Return New String() {}
        End If

        Dim list As New List(Of String)
        Dim seen As New HashSet(Of String)

        For Each v As String In Values
            If String.IsNullOrEmpty(v) Then
                Continue For
            End If
            If seen.Add(v) Then
                list.Add(v)
            End If
        Next

        Return list.ToArray
    End Function

    ''' <summary>
    ''' 文本形式（用于诊断输出）。
    ''' </summary>
    Public Overrides Function ToString() As String
        Return $"{Name} ({ResolvedKind()})"
    End Function
End Class

''' <summary>
''' 复合热图：在普通热图（<see cref="HeatmapPlot"/>）的基础上增加
''' 「行列层次聚类树 + 行列颜色标签注释 + 完整图例」。
'''
''' <para>
''' 典型用法（对应 R 的 <c>pheatmap(scale = "row", clustering_method = "ward.D2")</c>）：
''' </para>
'''
''' ```vb
''' Using plt As New ComplexHeatMap(1074, 955, PlotTheme.Light())
'''     plt.Matrix = matrix
'''     plt.RowLabels = compoundNames
'''     plt.ColLabels = sampleNames
'''     plt.ScaleMode = HeatMapScale.Row
'''     plt.MinValue = -2
'''     plt.MaxValue = 2
'''     plt.ColAnnotations.Add(HeatMapAnnotation.Discrete("Variety", varieties))
'''     plt.Plot()
'''     plt.SavePng("heatmap.png", 300)
''' End Using
''' ```
'''
''' <para>
''' 行列顺序由聚类树决定：叶子顺序与 <see cref="Cluster.OrderLeafs"/>（子节点按叶数升序）
''' 保持完全一致，聚类树、注释条、热图主体与标签共用同一份顺序索引，因此永远对齐。
''' </para>
''' </summary>
Public Class ComplexHeatMap : Inherits HeatmapPlot

    ''' <summary>
    ''' 参考图风格的蓝-白-红发散色阶（等价 R 的 <c>colorRampPalette(c("#2166AC", "white", "#B2182B"))</c>）。
    ''' </summary>
    Public Shared ReadOnly Property DivergingColors As Color()
        Get
            Return {
                ColorTranslator.FromHtml("#2166AC"),
                Color.White,
                ColorTranslator.FromHtml("#B2182B")
            }
        End Get
    End Property

    ''' <summary>
    ''' 自定义色阶停靠点，优先级高于继承的 <see cref="HeatmapPlot.ColorMap"/>；
    ''' 置为 <c>Nothing</c> 时回退到 <see cref="HeatmapPlot.ColorMap"/> 枚举色阶。
    '''
    ''' 构造函数中的缺省值是 <see cref="DivergingColors"/>（蓝-白-红发散色阶）。
    ''' </summary>
    Public Property PaletteColors As Color() = Nothing

    ''' <summary>数据的 z-score 标准化方式，缺省为按行标准化。</summary>
    Public Property ScaleMode As HeatMapScale = HeatMapScale.Row

    ''' <summary>行聚类的连接策略，缺省 <see cref="LinkageMethod.Ward"/>。</summary>
    Public Property RowLinkage As LinkageMethod = LinkageMethod.Ward
    ''' <summary>列聚类的连接策略，缺省 <see cref="LinkageMethod.Ward"/>。</summary>
    Public Property ColLinkage As LinkageMethod = LinkageMethod.Ward

    ''' <summary>行聚类的距离度量（缺省欧氏距离）。</summary>
    Public Property RowMetric As Func(Of Double(), Double(), Double) = Nothing
    ''' <summary>列聚类的距离度量（缺省欧氏距离）。</summary>
    Public Property ColMetric As Func(Of Double(), Double(), Double) = Nothing

    ''' <summary>
    ''' 行聚类树。为 <c>Nothing</c> 且 <see cref="AutoClusterRows"/> 为 True 时自动计算；
    ''' 也可以由调用方传入已有的聚类结果（此时树中各叶节点的名称必须出现在 <see cref="HeatmapPlot.RowLabels"/> 中）。
    ''' </summary>
    Public Property RowTree As Cluster = Nothing
    ''' <summary>列聚类树，语义同 <see cref="RowTree"/>。</summary>
    Public Property ColTree As Cluster = Nothing

    ''' <summary>是否在 <see cref="RowTree"/> 为空时自动执行行聚类。</summary>
    Public Property AutoClusterRows As Boolean = True
    ''' <summary>是否在 <see cref="ColTree"/> 为空时自动执行列聚类。</summary>
    Public Property AutoClusterColumns As Boolean = True

    ''' <summary>是否绘制行（左侧）聚类树。</summary>
    Public Property ShowRowDendrogram As Boolean = True
    ''' <summary>是否绘制列（顶部）聚类树。</summary>
    Public Property ShowColDendrogram As Boolean = True

    ''' <summary>聚类树线条颜色。</summary>
    Public Property DendrogramColor As Color = Color.FromArgb(64, 64, 64)
    ''' <summary>聚类树线条宽度（像素）。</summary>
    Public Property DendrogramLineWidth As Single = 0.8F

    ''' <summary>行聚类树占用的宽度（像素）。</summary>
    Public Property RowDendrogramWidth As Single = 90
    ''' <summary>列聚类树占用的高度（像素）。</summary>
    Public Property ColDendrogramHeight As Single = 90

    ''' <summary>列注释条集合（画在列聚类树与热图主体之间，多条则自上而下堆叠）。</summary>
    Public Property ColAnnotations As New List(Of HeatMapAnnotation)
    ''' <summary>行注释条集合（画在热图主体左侧，多条则自左向右并排）。</summary>
    Public Property RowAnnotations As New List(Of HeatMapAnnotation)

    ''' <summary>单条列注释条的高度（像素）。</summary>
    Public Property ColAnnotationHeight As Single = 12
    ''' <summary>单条行注释条的宽度（像素）。</summary>
    Public Property RowAnnotationWidth As Single = 12

    ''' <summary>是否在列注释条的右端显示加粗的注释名称（例如 <c>Variety</c>）。</summary>
    Public Property ShowAnnotationName As Boolean = True

    ''' <summary>行标签是否绘制在热图主体右侧（缺省 True）；置 False 时绘制在左侧。</summary>
    Public Property RowLabelsOnRight As Boolean = True

    ''' <summary>
    ''' 列标签的旋转角度：缺省 270（与 pheatmap 的 <c>angle_col = 270</c> 一致，文字自下往上读）；
    ''' 取 90 时文字自上往下读，取 0 时不旋转。
    ''' </summary>
    Public Property ColLabelAngle As Single = 270

    ''' <summary>行标签字体，缺省使用 <see cref="PlotTheme.TickLabelFont"/>。</summary>
    Public Property RowLabelFont As Font = Nothing
    ''' <summary>列标签字体，缺省使用 <see cref="PlotTheme.TickLabelFont"/>。</summary>
    Public Property ColLabelFont As Font = Nothing

    ''' <summary>是否绘制单元格边框（参考图的 R 参数 <c>border_color = NA</c>，故缺省为 False）。</summary>
    Public Property ShowCellBorder As Boolean = False
    ''' <summary>单元格边框颜色。</summary>
    Public Property CellBorderColor As Color = Color.White
    ''' <summary>单元格边框宽度。</summary>
    Public Property CellBorderWidth As Single = 0.5F

    ''' <summary>缺失值（NaN）单元格的颜色。</summary>
    Public Property NaColor As Color = Color.FromArgb(235, 235, 235)

    ''' <summary>是否绘制右侧的图例区（连续色标 + 注释分类图例）。</summary>
    Public Property ShowAnnotationsLegend As Boolean = True

    ''' <summary>图例标题字体，缺省为 <see cref="PlotTheme.LegendFont"/> 的加粗版本。</summary>
    Public Property LegendTitleFont As Font = Nothing

    ''' <summary>行标签与热图主体之间的间距。</summary>
    Public Property RowLabelGap As Single = 6
    ''' <summary>列标签与热图主体之间的间距。</summary>
    Public Property ColLabelGap As Single = 6
    ''' <summary>图例区与其它元素之间的间距。</summary>
    Public Property LegendGap As Single = 22

    ''' <summary>连续色标标题（为空则不显示）。</summary>
    Public Property ColorBarTitle As String = ""

    ''' <summary>连续色标的偏好高度，实际高度不会超过热图主体高度。</summary>
    Public Property ColorBarHeight As Single = 240

    ''' <summary>
    ''' 调试开关：设置环境变量 <c>HEATMAP_DEBUG=1</c> 时输出逐行/逐列的绘制信息。
    ''' </summary>
    Friend Shared ReadOnly DebugDump As Boolean =
        String.Equals(Environment.GetEnvironmentVariable("HEATMAP_DEBUG"), "1", StringComparison.Ordinal)

    Private _rowOrder As String() = Nothing
    Private _colOrder As String() = Nothing
    Private _rowMap As Integer() = Nothing
    Private _colMap As Integer() = Nothing
    Private _rowNames As String() = Nothing
    Private _colNames As String() = Nothing
    Private _body As RectangleF

    ''' <summary>
    ''' 构造复合热图（引擎自建画布）。
    ''' </summary>
    ''' <param name="width">画布宽度（像素）</param>
    ''' <param name="height">画布高度（像素）</param>
    ''' <param name="theme">主题</param>
    ''' <param name="driver">绘图驱动</param>
    Public Sub New(width As Integer, height As Integer, Optional theme As PlotTheme = Nothing, Optional driver As Drivers = Drivers.Default)
        MyBase.New(width, height, theme, driver)

        Me.PaletteColors = DivergingColors
        Me.ColorMap = ScalerPalette.Seismic
    End Sub

    ''' <summary>
    ''' 直接在外部提供的绘图设备上绘制（图层叠加模式 / 宿主画布）。
    ''' </summary>
    ''' <param name="g">宿主绘图设备</param>
    ''' <param name="theme">主题</param>
    Public Sub New(g As IGraphics, Optional theme As PlotTheme = Nothing)
        MyBase.New(g, theme)

        Me.PaletteColors = DivergingColors
        Me.ColorMap = ScalerPalette.Seismic
    End Sub

#Region "绘制结果"

    ''' <summary>
    ''' 最近一次绘制所使用的行顺序（叶节点名称，自上而下）。绘制之前为 <c>Nothing</c>。
    ''' </summary>
    Public ReadOnly Property RowOrder As String()
        Get
            Return _rowOrder
        End Get
    End Property

    ''' <summary>
    ''' 最近一次绘制所使用的列顺序（叶节点名称，自左向右）。绘制之前为 <c>Nothing</c>。
    ''' </summary>
    Public ReadOnly Property ColOrder As String()
        Get
            Return _colOrder
        End Get
    End Property

    ''' <summary>
    ''' 最近一次绘制时热图主体所占的矩形区域（像素）。
    ''' </summary>
    Public ReadOnly Property HeatmapBody As RectangleF
        Get
            Return _body
        End Get
    End Property

#End Region

#Region "数据入口"

    ''' <summary>
    ''' 从二维表构建复合热图：矩阵取 <see cref="NumericTable.features"/>（行=热图行，列=热图列），
    ''' 行名取 <see cref="NumericTable.rowNames"/>，列名取 <see cref="NumericTable.featureNames"/>，
    ''' 数值标签列（<see cref="NumericTable.labelNames"/>）会自动成为**连续行注释**。
    ''' </summary>
    ''' <param name="table">二维表对象</param>
    ''' <param name="labelColumns">需要作为连续行注释的标签列名称；为 <c>Nothing</c> 时使用全部标签列</param>
    ''' <param name="colAnnotations">额外附加的列注释</param>
    ''' <param name="scale">标准化方式</param>
    ''' <param name="width">画布宽度</param>
    ''' <param name="height">画布高度</param>
    ''' <param name="theme">主题</param>
    Public Shared Function FromTable(table As NumericTable,
                                     Optional labelColumns As String() = Nothing,
                                     Optional colAnnotations As IEnumerable(Of HeatMapAnnotation) = Nothing,
                                     Optional scale As HeatMapScale = HeatMapScale.Row,
                                     Optional width As Integer = 1074,
                                     Optional height As Integer = 955,
                                     Optional theme As PlotTheme = Nothing) As ComplexHeatMap

        If table Is Nothing Then
            Throw New ArgumentNullException(NameOf(table))
        End If

        Dim nRow As Integer = table.nsamples
        Dim nCol As Integer = table.nfeatures

        If nRow = 0 OrElse nCol = 0 Then
            Throw New ArgumentException("the numeric table has no sample/feature data for the heatmap!", NameOf(table))
        End If

        Dim data(nRow - 1, nCol - 1) As Double

        For i As Integer = 0 To nRow - 1
            Dim row As Double() = table.Row(i)

            For j As Integer = 0 To nCol - 1
                data(i, j) = row(j)
            Next
        Next

        Dim heatmap As New ComplexHeatMap(width, height, theme) With {
            .Matrix = data,
            .RowLabels = table.RowNamesOrDefault(),
            .ColLabels = If(table.featureNames, Enumerable.Range(1, nCol).Select(Function(i) $"F{i}").ToArray),
            .ScaleMode = scale
        }

        Dim labels As String() = labelColumns

        If labels Is Nothing Then
            labels = If(table.labelNames, New String() {})
        End If

        For Each name As String In labels
            If String.IsNullOrEmpty(name) OrElse Not table.HasLabel(name) Then
                Continue For
            End If

            Call heatmap.RowAnnotations.Add(HeatMapAnnotation.Continuous(name, table.GetLabel(name)))
        Next

        If colAnnotations IsNot Nothing Then
            Call heatmap.ColAnnotations.AddRange(colAnnotations)
        End If

        Return heatmap
    End Function

    ''' <summary>
    ''' 从 csv 文件构建复合热图：**数值列**进入热图矩阵，**文本列**自动成为离散行注释。
    '''
    ''' <para>
    ''' 文件格式约定与 sciBASIC 的其它 csv 读取器一致：第一行为表头，
    ''' 若 <paramref name="rowHeader"/> 为 True 则第一列是行名；单元格内不支持包含分隔符、
    ''' 双引号或者换行符的复杂格式。
    ''' </para>
    ''' </summary>
    ''' <param name="path">csv 文件路径</param>
    ''' <param name="rowHeader">第一列是否为行名</param>
    ''' <param name="delimiter">分隔符</param>
    ''' <param name="scale">标准化方式</param>
    ''' <param name="width">画布宽度</param>
    ''' <param name="height">画布高度</param>
    ''' <param name="theme">主题</param>
    Public Shared Function FromCsv(path As String,
                                   Optional rowHeader As Boolean = True,
                                   Optional delimiter As Char = ","c,
                                   Optional scale As HeatMapScale = HeatMapScale.Row,
                                   Optional width As Integer = 1074,
                                   Optional height As Integer = 955,
                                   Optional theme As PlotTheme = Nothing) As ComplexHeatMap

        If String.IsNullOrEmpty(path) Then
            Throw New ArgumentNullException(NameOf(path))
        End If
        If Not System.IO.File.Exists(path) Then
            Throw New System.IO.FileNotFoundException($"the csv table file is not found: {path}", path)
        End If

        Dim lines As String() = System.IO.File.ReadAllLines(path, Encoding.UTF8)
        Dim rows As New List(Of String())

        For Each line As String In lines
            If String.IsNullOrWhiteSpace(line) Then
                Continue For
            End If
            Call rows.Add(line.Split(delimiter))
        Next

        If rows.Count < 2 Then
            Throw New System.IO.InvalidDataException($"the csv table file has no data rows: {path}")
        End If

        Dim header As String() = rows(0)
        Dim offset As Integer = If(rowHeader, 1, 0)
        Dim rowsData As New List(Of String())

        For i As Integer = 1 To rows.Count - 1
            Dim tokens As String() = rows(i)

            If tokens.Length < header.Length Then
                Dim padding(header.Length - 1) As String

                Call Array.Copy(tokens, padding, tokens.Length)
                tokens = padding
            End If

            Call rowsData.Add(tokens)
        Next

        ' 判定每一列是数值列还是文本列
        Dim isNumeric(header.Length - 1) As Boolean

        For j As Integer = offset To header.Length - 1
            Dim numeric As Boolean = False

            For Each tokens As String() In rowsData
                If String.IsNullOrWhiteSpace(tokens(j)) Then
                    Continue For
                End If

                Dim v As Double

                If Double.TryParse(tokens(j), v) Then
                    numeric = True
                Else
                    numeric = False
                    Exit For
                End If
            Next

            isNumeric(j) = numeric
        Next

        Dim numericCols As Integer() = Enumerable.Range(offset, header.Length - offset).Where(Function(j) isNumeric(j)).ToArray
        Dim textCols As Integer() = Enumerable.Range(offset, header.Length - offset).Where(Function(j) Not isNumeric(j)).ToArray

        If numericCols.Length = 0 Then
            Throw New System.IO.InvalidDataException($"the csv table file contains no numeric column for the heatmap matrix: {path}")
        End If

        Dim nRow As Integer = rowsData.Count
        Dim nCol As Integer = numericCols.Length
        Dim matrix(nRow - 1, nCol - 1) As Double
        Dim rowNames(nRow - 1) As String
        Dim colNames(nCol - 1) As String

        For j As Integer = 0 To nCol - 1
            colNames(j) = header(numericCols(j))
        Next
        For i As Integer = 0 To nRow - 1
            rowNames(i) = If(rowHeader, rowsData(i)(0), $"R{i + 1}")

            For j As Integer = 0 To nCol - 1
                Dim v As Double

                If Not Double.TryParse(rowsData(i)(numericCols(j)), v) Then
                    v = Double.NaN
                End If

                matrix(i, j) = v
            Next
        Next

        Dim heatmap As New ComplexHeatMap(width, height, theme) With {
            .Matrix = matrix,
            .RowLabels = rowNames,
            .ColLabels = colNames,
            .ScaleMode = scale
        }

        For Each j As Integer In textCols
            Dim values(nRow - 1) As String

            For i As Integer = 0 To nRow - 1
                values(i) = rowsData(i)(j)
            Next

            Call heatmap.RowAnnotations.Add(HeatMapAnnotation.Discrete(header(j), values))
        Next

        Return heatmap
    End Function

#End Region

#Region "聚类"

    ''' <summary>
    ''' 依据当前的 <see cref="HeatmapPlot.Matrix"/>（先按 <see cref="ScaleMode"/> 标准化）
    ''' 重新计算行、列聚类树并写入 <see cref="RowTree"/> 与 <see cref="ColTree"/>。
    ''' </summary>
    Public Sub AutoCluster()
        Dim data As Double(,) = WorkingMatrix()

        Me.RowTree = BuildTree(ToRowVectors(data), EffectiveRowNames(), RowLinkage, RowMetric)
        Me.ColTree = BuildTree(Transpose(data), EffectiveColumnNames(), ColLinkage, ColMetric)
    End Sub

    ''' <summary>
    ''' 对给定的观测矩阵（每一行是一个观测）执行层次聚类。
    ''' </summary>
    ''' <param name="data">观测矩阵</param>
    ''' <param name="names">观测名称（必须唯一）</param>
    ''' <param name="linkage">连接策略</param>
    ''' <param name="metric">距离度量，缺省欧氏距离</param>
    Public Shared Function BuildTree(data As Double()(), names As String(),
                                     Optional linkage As LinkageMethod = LinkageMethod.Ward,
                                     Optional metric As Func(Of Double(), Double(), Double) = Nothing) As Cluster

        If data Is Nothing OrElse data.Length = 0 Then
            Return Nothing
        End If

        Dim uniq As String() = UniqueNames(names)

        If uniq.Length <= 1 Then
            Return New Cluster(uniq(0))
        End If

        If linkage = LinkageMethod.Ward Then
            Return WardClustering.Agglomerate(data, uniq, metric)
        End If

        Dim strategy As LinkageStrategy

        Select Case linkage
            Case LinkageMethod.SingleLinkage : strategy = New SingleLinkageStrategy()
            Case LinkageMethod.CompleteLinkage : strategy = New CompleteLinkageStrategy()
            Case LinkageMethod.WeightedLinkage : strategy = New WeightedLinkageStrategy()
            Case Else : strategy = New AverageLinkageStrategy()
        End Select

        Dim algorithm As New DefaultClusteringAlgorithm With {.Silent = True}

        Return algorithm.performClustering(
            distances:=DistanceMatrix(data, metric),
            clusterNames:=uniq,
            linkageStrategy:=strategy)
    End Function

    ''' <summary>
    ''' 计算对称距离矩阵（上三角计算后镜像到下三角）。
    ''' </summary>
    ''' <param name="data">观测矩阵</param>
    ''' <param name="metric">距离度量，缺省欧氏距离</param>
    Public Shared Function DistanceMatrix(data As Double()(), Optional metric As Func(Of Double(), Double(), Double) = Nothing) As Double()()
        Dim n As Integer = data.Length
        Dim matrix As Double()() = New Double(n - 1)() {}

        For i As Integer = 0 To n - 1
            matrix(i) = New Double(n - 1) {}
        Next

        Dim f As Func(Of Double(), Double(), Double) = If(metric, AddressOf DistanceMethods.EuclideanDistance)

        For i As Integer = 0 To n - 1
            For j As Integer = i + 1 To n - 1
                Dim d As Double = f(data(i), data(j))

                matrix(i)(j) = d
                matrix(j)(i) = d
            Next
        Next

        Return matrix
    End Function

#End Region

#Region "数据处理"

    ''' <summary>
    ''' 把 <see cref="HeatmapPlot.Matrix"/> 复制一份，并按 <see cref="ScaleMode"/> 做 z-score 标准化。
    ''' </summary>
    Private Function WorkingMatrix() As Double(,)
        Dim nRow As Integer = Matrix.GetLength(0)
        Dim nCol As Integer = Matrix.GetLength(1)
        Dim data(nRow - 1, nCol - 1) As Double

        For i As Integer = 0 To nRow - 1
            For j As Integer = 0 To nCol - 1
                data(i, j) = Matrix(i, j)
            Next
        Next

        Select Case ScaleMode
            Case HeatMapScale.Row
                Dim buffer(nCol - 1) As Double

                For i As Integer = 0 To nRow - 1
                    For j As Integer = 0 To nCol - 1
                        buffer(j) = data(i, j)
                    Next

                    Call ZScore(buffer)

                    For j As Integer = 0 To nCol - 1
                        data(i, j) = buffer(j)
                    Next
                Next
            Case HeatMapScale.Column
                Dim buffer(nRow - 1) As Double

                For j As Integer = 0 To nCol - 1
                    For i As Integer = 0 To nRow - 1
                        buffer(i) = data(i, j)
                    Next

                    Call ZScore(buffer)

                    For i As Integer = 0 To nRow - 1
                        data(i, j) = buffer(i)
                    Next
                Next
        End Select

        Return data
    End Function

    ''' <summary>
    ''' 就地对一组数值做 z-score 标准化（样本标准差 n-1，与 R 的 <c>scale()</c> 一致）；
    ''' 有效样本不足或者标准差为 0 时整组置 0，NaN 也置 0。
    ''' </summary>
    ''' <param name="values">待标准化的数值</param>
    Public Shared Sub ZScore(values As Double())
        Dim count As Integer = 0
        Dim sum As Double = 0

        For Each v As Double In values
            If Double.IsNaN(v) OrElse Double.IsInfinity(v) Then
                Continue For
            End If
            sum += v
            count += 1
        Next

        If count <= 1 Then
            For i As Integer = 0 To values.Length - 1
                values(i) = 0
            Next

            Return
        End If

        Dim mean As Double = sum / count
        Dim sq As Double = 0

        For Each v As Double In values
            If Double.IsNaN(v) OrElse Double.IsInfinity(v) Then
                Continue For
            End If
            sq += (v - mean) * (v - mean)
        Next

        Dim sd As Double = std.Sqrt(sq / (count - 1))

        For i As Integer = 0 To values.Length - 1
            If Double.IsNaN(values(i)) OrElse Double.IsInfinity(values(i)) OrElse sd <= 0 Then
                values(i) = 0
            Else
                values(i) = (values(i) - mean) / sd
            End If
        Next
    End Sub

    Private Function EffectiveRowNames() As String()
        If _rowNames IsNot Nothing Then
            Return _rowNames
        End If

        Dim nRow As Integer = Matrix.GetLength(0)

        Return If(RowLabels, Enumerable.Range(1, nRow).Select(Function(i) $"R{i}").ToArray)
    End Function

    Private Function EffectiveColumnNames() As String()
        If _colNames IsNot Nothing Then
            Return _colNames
        End If

        Dim nCol As Integer = Matrix.GetLength(1)

        Return If(ColLabels, Enumerable.Range(1, nCol).Select(Function(i) $"C{i}").ToArray)
    End Function

    ''' <summary>
    ''' 生成唯一的观测名称：聚类树要求叶节点名称唯一，重复名会追加下标后缀。
    ''' </summary>
    ''' <param name="names">原始名称</param>
    Public Shared Function UniqueNames(names As String()) As String()
        Dim list As New List(Of String)
        Dim used As New HashSet(Of String)

        For i As Integer = 0 To names.Length - 1
            Dim name As String = If(names(i), $"#{i + 1}")

            If used.Add(name) Then
                list.Add(name)
            Else
                Dim k As Integer = 2

                While Not used.Add($"{name}#{k}")
                    k += 1
                End While

                list.Add($"{name}#{k}")
            End If
        Next

        Return list.ToArray
    End Function

    ''' <summary>
    ''' 把二维矩阵拆成行向量集合（每一行是一个观测，供聚类使用）。
    ''' </summary>
    ''' <param name="data">源矩阵</param>
    Public Shared Function ToRowVectors(data As Double(,)) As Double()()
        Dim nRow As Integer = data.GetLength(0)
        Dim nCol As Integer = data.GetLength(1)
        Dim rows(nRow - 1)() As Double

        For i As Integer = 0 To nRow - 1
            Dim v(nCol - 1) As Double

            For j As Integer = 0 To nCol - 1
                v(j) = data(i, j)
            Next

            rows(i) = v
        Next

        Return rows
    End Function

    ''' <summary>
    ''' 矩阵转置（把列当作观测送入聚类）。
    ''' </summary>
    ''' <param name="data">源矩阵</param>
    Public Shared Function Transpose(data As Double(,)) As Double()()
        Dim nRow As Integer = data.GetLength(0)
        Dim nCol As Integer = data.GetLength(1)
        Dim rows(nCol - 1)() As Double

        For j As Integer = 0 To nCol - 1
            Dim v(nRow - 1) As Double

            For i As Integer = 0 To nRow - 1
                v(i) = data(i, j)
            Next

            rows(j) = v
        Next

        Return rows
    End Function

#End Region

#Region "顺序映射"

    ''' <summary>
    ''' 依据聚类树计算绘制顺序以及「绘制位置 → 原始下标」的映射。
    ''' 树为空、或者树中的叶节点名称与标签不匹配时回退为原始顺序（此时返回的树为 <c>Nothing</c>）。
    ''' </summary>
    Private Shared Sub OrderOf(tree As Cluster, names As String(),
                               ByRef order As String(), ByRef map As Integer(), ByRef effectiveTree As Cluster)

        Dim index As New Dictionary(Of String, Integer)
        Dim uniq As String() = UniqueNames(names)

        For i As Integer = 0 To uniq.Length - 1
            index(uniq(i)) = i
        Next

        Dim leafs As String() = Nothing

        If tree IsNot Nothing Then
            Try
                leafs = HeatMapDendrogram.LeafOrder(tree)
            Catch ex As Exception
                leafs = Nothing
            End Try
        End If

        If leafs IsNot Nothing Then
            If leafs.Length <> uniq.Length Then
                leafs = Nothing
            Else
                For Each name As String In leafs
                    If Not index.ContainsKey(name) Then
                        leafs = Nothing
                        Exit For
                    End If
                Next
            End If
        End If

        If leafs Is Nothing Then
            order = uniq
            map = Enumerable.Range(0, uniq.Length).ToArray
            effectiveTree = Nothing
        Else
            order = leafs
            map = leafs.Select(Function(name) index(name)).ToArray
            effectiveTree = tree
        End If
    End Sub

#End Region

#Region "绘制"

    ''' <summary>
    ''' 复合热图的绘制主流程。
    ''' </summary>
    ''' <param name="g">绘图设备</param>
    ''' <param name="canvas">画布布局信息</param>
    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        If Matrix Is Nothing Then
            Throw New ArgumentException("the heatmap matrix is not initialized!", NameOf(Matrix))
        End If

        Dim nRow As Integer = Matrix.GetLength(0)
        Dim nCol As Integer = Matrix.GetLength(1)

        If nRow = 0 OrElse nCol = 0 Then
            Return
        End If

        Call DrawBackground()
        Call DrawTitle()

        _rowNames = EffectiveRowNames()
        _colNames = EffectiveColumnNames()

        Dim data As Double(,) = WorkingMatrix()
        Dim rowVectors As Double()() = ToRowVectors(data)

        If AutoClusterRows AndAlso Me.RowTree Is Nothing Then
            Me.RowTree = BuildTree(rowVectors, _rowNames, RowLinkage, RowMetric)
        End If
        If AutoClusterColumns AndAlso Me.ColTree Is Nothing Then
            Me.ColTree = BuildTree(Transpose(data), _colNames, ColLinkage, ColMetric)
        End If

        Dim rowRoot As Cluster = Me.RowTree
        Dim colRoot As Cluster = Me.ColTree
        Dim rowDendro As Cluster = Nothing
        Dim colDendro As Cluster = Nothing

        Call OrderOf(rowRoot, _rowNames, _rowOrder, _rowMap, rowDendro)
        Call OrderOf(colRoot, _colNames, _colOrder, _colMap, colDendro)

        Dim rowFont As Font = If(RowLabelFont, Theme.TickLabelFont)
        Dim colFont As Font = If(ColLabelFont, Theme.TickLabelFont)
        Dim titleFont As Font = If(LegendTitleFont,
            New Font(Theme.LegendFont.Name, Theme.LegendFont.Size, FontStyle.Bold))

        Dim vmin As Double = If(MinValue, FiniteMin(data))
        Dim vmax As Double = If(MaxValue, FiniteMax(data))

        If vmax <= vmin Then
            vmin -= 0.5
            vmax += 0.5
        End If

        ' ---------------------------------------------------------------
        '  布局：由外向内依次扣除树 / 注释条 / 标签 / 图例所占的空间
        ' ---------------------------------------------------------------
        Dim pad As Single = 8
        Dim titleHeight As Single = If(String.IsNullOrEmpty(Title), 0, Theme.TitleFont.GetHeight(g) + 14)
        Dim rowLabelWidth As Single = 0

        For i As Integer = 0 To nRow - 1
            rowLabelWidth = std.Max(rowLabelWidth, g.MeasureString(_rowNames(_rowMap(i)), rowFont).Width)
        Next

        Dim rowTreeWidth As Single = If(ShowRowDendrogram AndAlso rowDendro IsNot Nothing, RowDendrogramWidth, 0)
        Dim colTreeHeight As Single = If(ShowColDendrogram AndAlso colDendro IsNot Nothing, ColDendrogramHeight, 0)
        Dim colAnnoHeight As Single = ColAnnotations.Count * ColAnnotationHeight
        Dim rowAnnoWidth As Single = RowAnnotations.Count * RowAnnotationWidth

        Dim colLabelHeight As Single = 0

        For i As Integer = 0 To nCol - 1
            Dim size As SizeF = g.MeasureString(_colNames(_colMap(i)), colFont)

            colLabelHeight = std.Max(colLabelHeight, If(ColLabelAngle = 0, size.Height, size.Width))
        Next

        If colLabelHeight > 0 Then
            colLabelHeight += ColLabelGap
        End If

        ' 右侧图例区所需的总宽度
        Dim tickWidth As Single = 0
        Dim legendWidth As Single = 0
        Dim legendItemWidth As Single = 0
        Dim legends As List(Of HeatMapAnnotation) = AnnotationLegends()

        If ShowColorLegend AndAlso ShowAnnotationsLegend Then
            For Each tick As Double In Geometry.NiceTicks(vmin, vmax, 5)
                If tick < vmin OrElse tick > vmax Then
                    Continue For
                End If

                tickWidth = std.Max(tickWidth, g.MeasureString(Geometry.NumberLabel(tick), Theme.TickLabelFont).Width)
            Next

            legendWidth = LegendGap + Theme.ColorBarWidth + 8 + tickWidth
        End If

        If legends.Count > 0 Then
            For Each anno As HeatMapAnnotation In legends
                If anno.ResolvedKind() = AnnotationKind.Discrete Then
                    For Each category As String In anno.Categories()
                        legendItemWidth = std.Max(legendItemWidth, g.MeasureString(category, Theme.LegendFont).Width)
                    Next
                Else
                    Dim range = anno.NumericRange()

                    legendItemWidth = std.Max(legendItemWidth, g.MeasureString(
                        Geometry.NumberLabel(range.min) & " ~ " & Geometry.NumberLabel(range.max),
                        Theme.LegendFont).Width)
                End If
            Next

            legendItemWidth += 24
            legendWidth += If(legendWidth > 0, LegendGap, 0) + legendItemWidth
        End If

        If legendWidth > 0 Then
            legendWidth += LegendGap
        End If

        Dim bodyLeft As Single = pad + rowTreeWidth + rowAnnoWidth +
            If(RowLabelsOnRight, 0, rowLabelWidth + RowLabelGap)
        Dim bodyTop As Single = pad + titleHeight + colTreeHeight + colAnnoHeight
        Dim bodyRight As Single = _width - pad - legendWidth - If(RowLabelsOnRight, rowLabelWidth + RowLabelGap, 0)
        Dim bodyBottom As Single = _height - pad - colLabelHeight
        Dim body As New RectangleF(bodyLeft, bodyTop, bodyRight - bodyLeft, bodyBottom - bodyTop)

        _body = body
        _plotArea = body

        If body.Width <= 1 OrElse body.Height <= 1 Then
            Return
        End If

        Dim cellW As Single = body.Width / nCol
        Dim cellH As Single = body.Height / nRow

        ' 单元格边界取整到整像素后再绘制，否则相邻单元格之间会出现 1px 的白色缝隙
        Dim colEdges As Single() = PixelEdges(body.Left, body.Width, nCol)
        Dim rowEdges As Single() = PixelEdges(body.Top, body.Height, nRow)

        ' ---------------------------------------------------------------
        '  聚类树
        ' ---------------------------------------------------------------
        Using pen As New Pen(DendrogramColor, DendrogramLineWidth)
            If colDendro IsNot Nothing AndAlso ShowColDendrogram Then
                Call HeatMapDendrogram.DrawColumnTree(g, pen, colDendro, _colOrder, cellW, body.Left,
                                                      pad + titleHeight, pad + titleHeight + colTreeHeight)
            End If
            If rowDendro IsNot Nothing AndAlso ShowRowDendrogram Then
                Call HeatMapDendrogram.DrawRowTree(g, pen, rowDendro, _rowOrder, cellH, body.Top,
                                                   pad, pad + rowTreeWidth)
            End If
        End Using

        ' ---------------------------------------------------------------
        '  注释条
        ' ---------------------------------------------------------------
        Call DrawColumnAnnotations(g, colEdges, body.Top, titleFont)
        Call DrawRowAnnotations(g, rowEdges, body.Left)

        ' ---------------------------------------------------------------
        '  热图主体
        ' ---------------------------------------------------------------
        Call DrawCells(g, data, colEdges, rowEdges, vmin, vmax)

        ' ---------------------------------------------------------------
        '  行列标签
        ' ---------------------------------------------------------------
        ' 右侧模式下标签从主体右边缘外侧开始（左对齐）；
        ' 左侧模式下标签带位于热图最左侧（右对齐，其后才是行树）
        Dim rowLabelX As Single = If(RowLabelsOnRight, body.Right + RowLabelGap, pad + rowLabelWidth)

        Call DrawRowLabels(g, rowLabelX, rowEdges, rowFont)
        Call DrawColumnLabels(g, body, colEdges, colFont)

        ' ---------------------------------------------------------------
        '  图例
        ' ---------------------------------------------------------------
        If ShowAnnotationsLegend Then
            Dim legendX As Single = body.Right + If(RowLabelsOnRight, RowLabelGap + rowLabelWidth, 0) + LegendGap

            Call DrawLegends(g, legendX, body, legends, tickWidth, vmin, vmax, titleFont)
        End If
    End Sub

    ''' <summary>
    ''' 需要绘制图例的注释集合。
    ''' </summary>
    Private Function AnnotationLegends() As List(Of HeatMapAnnotation)
        Dim list As New List(Of HeatMapAnnotation)

        For Each anno As HeatMapAnnotation In ColAnnotations
            If anno IsNot Nothing AndAlso anno.ShowInLegend Then
                list.Add(anno)
            End If
        Next
        For Each anno As HeatMapAnnotation In RowAnnotations
            If anno IsNot Nothing AndAlso anno.ShowInLegend Then
                list.Add(anno)
            End If
        Next

        Return list
    End Function

    ''' <summary>
    ''' 单元格取色（越界值会被色阶自动裁剪）。
    ''' </summary>
    Private Function ColorOf(v As Double, vmin As Double, vmax As Double) As Color
        If PaletteColors IsNot Nothing AndAlso PaletteColors.Length > 0 Then
            Dim t As Double = If(vmax <= vmin, 0, (v - vmin) / (vmax - vmin))

            Return ColorScale.LerpPalette(t, PaletteColors)
        End If

        Return ColorScale.GetColor(v, vmin, vmax, ColorMap)
    End Function

    ''' <summary>
    ''' 绘制热图主体（逐单元格填充 + 可选的数值标签与边框）。
    ''' </summary>
    Private Sub DrawCells(g As IGraphics, data As Double(,), colEdges As Single(), rowEdges As Single(),
                          vmin As Double, vmax As Double)
        Dim nRow As Integer = data.GetLength(0)
        Dim nCol As Integer = data.GetLength(1)
        Dim textFont As Font = Theme.TickLabelFont
        Dim cellPen As Pen = Nothing

        If ShowCellBorder Then
            cellPen = New Pen(CellBorderColor, CellBorderWidth)
        End If

        Try
            For r As Integer = 0 To nRow - 1
                Dim y As Single = rowEdges(r)
                Dim h As Single = rowEdges(r + 1) - y
                Dim row As Integer = _rowMap(r)

                If DebugDump AndAlso r < 3 Then
                    Dim vals As String() = Enumerable.Range(0, nCol).Select(Function(c) data(row, _colMap(c)).ToString("F2")).ToArray

                    Call Console.WriteLine(
                        $"[heatmap] row {r} (source #{row}) y={y} h={h} values=[{String.Join(",", vals)}] colMap=[{String.Join(",", _colMap)}]")
                End If

                For c As Integer = 0 To nCol - 1
                    Dim v As Double = data(row, _colMap(c))
                    Dim color As Color = If(Double.IsNaN(v), NaColor, ColorOf(v, vmin, vmax))
                    Dim x As Single = colEdges(c)
                    Dim w As Single = colEdges(c + 1) - x

                    Using br As New SolidBrush(color)
                        ' 注意：GDI 驱动的 FillRectangle(brush, x, y, w, h) 重载是空实现，
                        ' 必须走 RectangleF 重载才能真的绘制出来
                        g.FillRectangle(br, New RectangleF(x, y, w, h))
                    End Using

                    If cellPen IsNot Nothing Then
                        g.DrawRectangle(cellPen, x, y, w, h)
                    End If

                    If ShowValues AndAlso Not Double.IsNaN(v) Then
                        Dim label As String = FormatNumber(v)
                        Dim size As SizeF = g.MeasureString(label, textFont)

                        If size.Width <= w AndAlso size.Height <= h Then
                            Using br As New SolidBrush(ColorScale.ReadableTextColor(color))
                                g.DrawString(label, textFont, br, x + (w - size.Width) / 2, y + (h - size.Height) / 2)
                            End Using
                        End If
                    End If
                Next
            Next
        Finally
            cellPen?.Dispose()
        End Try
    End Sub

    ''' <summary>
    ''' 把 [<paramref name="start"/>, <paramref name="start"/> + <paramref name="length"/>]
    ''' 均匀切成 <paramref name="n"/> 段，并返回 <c>n + 1</c> 个整像素对齐的边界，
    ''' 从而避免相邻单元格之间出现 1px 的白缝。
    ''' </summary>
    Private Shared Function PixelEdges(start As Single, length As Single, n As Integer) As Single()
        Dim edges(n) As Single

        For i As Integer = 0 To n
            edges(i) = CSng(std.Round(start + length * i / n))
        Next

        edges(0) = start
        edges(n) = start + length

        Return edges
    End Function

    Private Shared Function FiniteMin(data As Double(,)) As Double
        Dim min As Double = Double.MaxValue

        For Each v As Double In data
            If Double.IsNaN(v) OrElse Double.IsInfinity(v) Then
                Continue For
            End If
            If v < min Then min = v
        Next

        Return If(min = Double.MaxValue, 0, min)
    End Function

    Private Shared Function FiniteMax(data As Double(,)) As Double
        Dim max As Double = Double.MinValue

        For Each v As Double In data
            If Double.IsNaN(v) OrElse Double.IsInfinity(v) Then
                Continue For
            End If
            If v > max Then max = v
        Next

        Return If(max = Double.MinValue, 1, max)
    End Function

    ''' <summary>
    ''' 绘制列注释条（多条自上而下堆叠）以及注释名称。
    ''' </summary>
    Private Sub DrawColumnAnnotations(g As IGraphics, colEdges As Single(), bodyTop As Single, nameFont As Font)
        If ColAnnotations.Count = 0 Then
            Return
        End If

        Dim top As Single = bodyTop - ColAnnotations.Count * ColAnnotationHeight

        For k As Integer = 0 To ColAnnotations.Count - 1
            Dim anno As HeatMapAnnotation = ColAnnotations(k)

            If anno Is Nothing Then
                Continue For
            End If

            Dim y As Single = top + k * ColAnnotationHeight

            For c As Integer = 0 To _colMap.Length - 1
                Dim color As Color = anno.ColorAt(_colMap(c), Theme.Palette)
                Dim x As Single = colEdges(c)

                Using br As New SolidBrush(color)
                    g.FillRectangle(br, New RectangleF(x, y, colEdges(c + 1) - x, ColAnnotationHeight))
                End Using
            Next

            If ShowAnnotationName AndAlso Not String.IsNullOrEmpty(anno.Name) Then
                Dim size As SizeF = g.MeasureString(anno.Name, nameFont)

                Using br As New SolidBrush(Theme.TextColor)
                    g.DrawString(anno.Name, nameFont, br, colEdges(colEdges.Length - 1) + 4,
                                 y + (ColAnnotationHeight - size.Height) / 2)
                End Using
            End If
        Next
    End Sub

    ''' <summary>
    ''' 绘制行注释条（多条自左向右并排）。
    ''' </summary>
    Private Sub DrawRowAnnotations(g As IGraphics, rowEdges As Single(), bodyLeft As Single)
        If RowAnnotations.Count = 0 Then
            Return
        End If

        Dim left As Single = bodyLeft - RowAnnotations.Count * RowAnnotationWidth

        For k As Integer = 0 To RowAnnotations.Count - 1
            Dim anno As HeatMapAnnotation = RowAnnotations(k)

            If anno Is Nothing Then
                Continue For
            End If

            Dim x As Single = left + k * RowAnnotationWidth

            For r As Integer = 0 To _rowMap.Length - 1
                Dim color As Color = anno.ColorAt(_rowMap(r), Theme.Palette)
                Dim y As Single = rowEdges(r)

                Using br As New SolidBrush(color)
                    g.FillRectangle(br, New RectangleF(x, y, RowAnnotationWidth, rowEdges(r + 1) - y))
                End Using
            Next
        Next
    End Sub

    ''' <summary>
    ''' 绘制行标签：缺省位于热图主体右侧并**左对齐**（所有标签起始位置一致），
    ''' 放在左侧时改为**右对齐**并以标签带的最右端为锚点。
    ''' </summary>
    ''' <param name="g">绘图设备</param>
    ''' <param name="labelX">
    ''' 行标签的锚点：右侧模式下是标签的起始 x（左对齐），左侧模式下是标签带的最右端 x（右对齐）
    ''' </param>
    ''' <param name="rowEdges">行边界数组（标签逐行垂直居中）</param>
    ''' <param name="font">标签字体</param>
    Private Sub DrawRowLabels(g As IGraphics, labelX As Single, rowEdges As Single(), font As Font)
        Using br As New SolidBrush(Theme.TextColor)
            For r As Integer = 0 To _rowMap.Length - 1
                Dim label As String = _rowNames(_rowMap(r))
                Dim size As SizeF = g.MeasureString(label, font)
                Dim y As Single = (rowEdges(r) + rowEdges(r + 1) - size.Height) / 2

                If RowLabelsOnRight Then
                    g.DrawString(label, font, br, labelX, y)
                Else
                    g.DrawString(label, font, br, labelX - size.Width, y)
                End If
            Next
        End Using
    End Sub

    ''' <summary>
    ''' 绘制列标签（缺省旋转 270°，文字自下往上读，与 pheatmap 的 <c>angle_col = 270</c> 一致）。
    ''' </summary>
    Private Sub DrawColumnLabels(g As IGraphics, body As RectangleF, colEdges As Single(), font As Font)
        Using br As New SolidBrush(Theme.TextColor)
            For c As Integer = 0 To _colMap.Length - 1
                Dim label As String = _colNames(_colMap(c))
                Dim size As SizeF = g.MeasureString(label, font)
                Dim x As Single = (colEdges(c) + colEdges(c + 1)) / 2

                If ColLabelAngle = 0 Then
                    g.DrawString(label, font, br, x - size.Width / 2, body.Bottom + ColLabelGap)
                Else
                    ' 旋转之后局部 +x 轴指向文字的书写方向：270° 时自下往上、90° 时自上往下
                    Dim anchorY As Single = If(ColLabelAngle = 90,
                        body.Bottom + ColLabelGap,
                        body.Bottom + ColLabelGap + size.Width)

                    g.TranslateTransform(x - size.Height / 2, anchorY)
                    g.RotateTransform(ColLabelAngle)
                    g.DrawString(label, font, br, 0, 0)
                    g.ResetTransform()
                End If
            Next
        End Using
    End Sub

    ''' <summary>
    ''' 绘制右侧的连续色标（竖直）与注释图例。
    ''' </summary>
    Private Sub DrawLegends(g As IGraphics, x As Single, body As RectangleF,
                            legends As List(Of HeatMapAnnotation),
                            tickWidth As Single, vmin As Double, vmax As Double, titleFont As Font)

        If ShowColorLegend Then
            Dim barW As Single = Theme.ColorBarWidth
            Dim barH As Single = std.Min(std.Min(ColorBarHeight, body.Height), _height - body.Top - 8)
            Dim y As Single = body.Top

            Call DrawColorBar(g, x, y, barW, barH, vmin, vmax, ColorBarTitle, titleFont)

            x += barW + 8 + tickWidth + LegendGap
        End If

        Dim top As Single = body.Top

        For Each anno As HeatMapAnnotation In legends
            Call DrawAnnotationLegend(g, anno, x, top, titleFont)
        Next
    End Sub

    ''' <summary>
    ''' 自绘竖直连续色标（顶部为最大值），与单元格取色共用同一套颜色函数。
    ''' </summary>
    Private Sub DrawColorBar(g As IGraphics, x As Single, y As Single, w As Single, h As Single,
                             vmin As Double, vmax As Double, title As String, titleFont As Font)

        If w <= 0 OrElse h <= 0 Then
            Return
        End If

        Dim steps As Integer = CInt(std.Ceiling(h))

        For i As Integer = 0 To steps - 1
            Dim t As Double = 1 - i / CDbl(steps)
            Dim v As Double = vmin + t * (vmax - vmin)

            Using br As New SolidBrush(ColorOf(v, vmin, vmax))
                g.FillRectangle(br, New RectangleF(x, y + i, w, 1))
            End Using
        Next

        Using pen As New Pen(Theme.BorderColor, 0.7F),
              br As New SolidBrush(Theme.TextColor)
            g.DrawRectangle(pen, x, y, w, h)

            For Each tick As Double In Geometry.NiceTicks(vmin, vmax, 5)
                If tick < vmin OrElse tick > vmax Then
                    Continue For
                End If

                Dim pos As Single = CSng((tick - vmin) / (vmax - vmin))
                Dim py As Single = y + h - pos * h
                Dim label As String = Geometry.NumberLabel(tick)
                Dim size As SizeF = g.MeasureString(label, Theme.TickLabelFont)

                g.DrawLine(pen, x + w, py, x + w + 4, py)
                g.DrawString(label, Theme.TickLabelFont, br, x + w + 6, py - size.Height / 2)
            Next

            If Not String.IsNullOrEmpty(title) Then
                Dim size As SizeF = g.MeasureString(title, titleFont)

                g.DrawString(title, titleFont, br, x - size.Width / 2 + w / 2, y - size.Height - 2)
            End If
        End Using
    End Sub

    ''' <summary>
    ''' 绘制单个注释的图例（离散：标题 + 色块 + 类别名；连续：标题 + 渐变条 + 数值范围）。
    ''' </summary>
    Private Sub DrawAnnotationLegend(g As IGraphics, anno As HeatMapAnnotation, x As Single, ByRef y As Single, titleFont As Font)
        If anno Is Nothing Then
            Return
        End If

        Dim swatch As Single = 13
        Dim gap As Single = 5

        Using br As New SolidBrush(Theme.TextColor)
            If Not String.IsNullOrEmpty(anno.Name) Then
                Dim size As SizeF = g.MeasureString(anno.Name, titleFont)

                g.DrawString(anno.Name, titleFont, br, x, y)
                y += size.Height + 6
            End If

            If anno.ResolvedKind() = AnnotationKind.Discrete Then
                Dim map As Dictionary(Of String, Color) = anno.ResolvedColors(Theme.Palette)

                For Each category As String In anno.Categories()
                    Using swatchBrush As New SolidBrush(map(category))
                        g.FillRectangle(swatchBrush, New RectangleF(x, y, swatch, swatch))
                    End Using

                    Dim size As SizeF = g.MeasureString(category, Theme.LegendFont)

                    g.DrawString(category, Theme.LegendFont, br, x + swatch + gap, y + (swatch - size.Height) / 2)
                    y += std.Max(swatch, size.Height) + 3
                Next
            Else
                Dim range = anno.NumericRange()
                Dim stops As Color() = anno.ContinuousColors()
                Dim h As Single = 10
                Dim w As Single = 60

                For i As Integer = 0 To CInt(w) - 1
                    Dim t As Double = i / CDbl(w)

                    Using swatchBrush As New SolidBrush(ColorScale.LerpPalette(t, stops))
                        g.FillRectangle(swatchBrush, New RectangleF(x + i, y, 1, h))
                    End Using
                Next

                Using pen As New Pen(Theme.BorderColor, 0.7F)
                    g.DrawRectangle(pen, x, y, w, h)
                End Using

                Dim mm As String = Geometry.NumberLabel(range.min) & " ~ " & Geometry.NumberLabel(range.max)
                Dim size As SizeF = g.MeasureString(mm, Theme.LegendFont)

                g.DrawString(mm, Theme.LegendFont, br, x, y + h + 2)
                y += h + size.Height + 6
            End If
        End Using
    End Sub

#End Region
End Class

''' <summary>
''' 聚类树（dendrogram）的布局与绘制。
'''
''' <para>
''' 叶子在绘图方向等距排列（每行/每列占一格），内部节点的位置取其子节点位置的中点，
''' 距离按 <c>d / dmax</c> 归一化后映射到树的长度方向；连接线采用正交肘形
''' （两段平行于叶子排列方向的短线 + 一段垂直于它的长线），即论文中常见的样式。
''' </para>
''' </summary>
Friend Module HeatMapDendrogram

    ''' <summary>
    ''' 子节点按叶数升序排列——必须与 <see cref="Cluster.OrderLeafs"/> 的约定保持一致，
    ''' 否则树的形状与热图的行列顺序会不一致。
    ''' </summary>
    ''' <param name="node">父节点</param>
    Public Function OrderedChildren(node As Cluster) As Cluster()
        Dim children As New List(Of Cluster)

        For Each child As Cluster In node.Children
            Call children.Add(child)
        Next

        Return children.OrderBy(Function(c) c.Leafs).ToArray
    End Function

    ''' <summary>
    ''' 聚类树的叶子顺序（与 <see cref="Cluster.OrderLeafs"/> 等价，但保证与绘制时的递归顺序一致）。
    ''' </summary>
    ''' <param name="root">聚类树根节点</param>
    Public Function LeafOrder(root As Cluster) As String()
        Dim names As New List(Of String)

        Call CollectLeafs(root, names)

        Return names.ToArray
    End Function

    Private Sub CollectLeafs(node As Cluster, names As List(Of String))
        If node.Children.Count = 0 Then
            Call names.Add(node.Name)
        Else
            For Each child As Cluster In OrderedChildren(node)
                Call CollectLeafs(child, names)
            Next
        End If
    End Sub

    ''' <summary>
    ''' 树中最大的合并距离（用于归一化树高）。
    ''' </summary>
    ''' <param name="root">聚类树根节点</param>
    Public Function MaxDistance(root As Cluster) As Double
        Dim dmax As Double = 0

        Call Walk(root, dmax)

        Return dmax
    End Function

    Private Sub Walk(node As Cluster, ByRef dmax As Double)
        If node Is Nothing Then
            Return
        End If

        Dim d As Double = node.DistanceValue

        If Not Double.IsNaN(d) AndAlso Not Double.IsInfinity(d) AndAlso d > dmax Then
            dmax = d
        End If

        For Each child As Cluster In node.Children
            Call Walk(child, dmax)
        Next
    End Sub

    Private Function Ratio(d As Double, dmax As Double) As Double
        If dmax <= 0 OrElse Double.IsNaN(d) OrElse Double.IsInfinity(d) Then
            Return 0
        End If

        Return std.Max(0, std.Min(1, d / dmax))
    End Function

    ''' <summary>
    ''' 绘制列（顶部）聚类树：根在上，叶子等距落在每一列的中心。
    ''' </summary>
    ''' <param name="g">绘图设备</param>
    ''' <param name="pen">线条</param>
    ''' <param name="root">聚类树根节点</param>
    ''' <param name="order">叶子顺序（与热图的列顺序一致）</param>
    ''' <param name="cellW">单元格宽度</param>
    ''' <param name="x0">热图主体左边界</param>
    ''' <param name="yTop">树的顶部（根所在的一侧）</param>
    ''' <param name="yBottom">树的底部（叶子所在的一侧）</param>
    Public Sub DrawColumnTree(g As IGraphics, pen As Pen, root As Cluster, order As String(),
                              cellW As Single, x0 As Single, yTop As Single, yBottom As Single)

        Dim slots As Dictionary(Of String, Integer) = SlotIndex(order)
        Dim dmax As Double = MaxDistance(root)

        Call DrawColumnNode(g, pen, root, slots, x0, cellW, yTop, yBottom, dmax)
    End Sub

    Private Function DrawColumnNode(g As IGraphics, pen As Pen, node As Cluster,
                                    slots As Dictionary(Of String, Integer),
                                    x0 As Single, cellW As Single, yTop As Single, yBottom As Single,
                                    dmax As Double) As Single

        Dim y As Single = yBottom - CSng(Ratio(node.DistanceValue, dmax)) * (yBottom - yTop)

        If node.Children.Count = 0 Then
            Dim slot As Integer

            If Not slots.TryGetValue(node.Name, slot) Then
                Return Single.NaN
            End If

            Return x0 + (slot + 0.5) * cellW
        End If

        Dim children As Cluster() = OrderedChildren(node)
        Dim xs(children.Length - 1) As Single
        Dim left As Single = Single.MaxValue
        Dim right As Single = Single.MinValue

        For i As Integer = 0 To children.Length - 1
            xs(i) = DrawColumnNode(g, pen, children(i), slots, x0, cellW, yTop, yBottom, dmax)

            If Single.IsNaN(xs(i)) Then
                Return Single.NaN
            End If

            left = std.Min(left, xs(i))
            right = std.Max(right, xs(i))
        Next

        For i As Integer = 0 To children.Length - 1
            Dim childY As Single = yBottom - CSng(Ratio(children(i).DistanceValue, dmax)) * (yBottom - yTop)

            g.DrawLine(pen, xs(i), childY, xs(i), y)
        Next

        g.DrawLine(pen, left, y, right, y)

        Return (left + right) / 2
    End Function

    ''' <summary>
    ''' 绘制行（左侧）聚类树：根在最左，叶子等距落在每一行的中心。
    ''' </summary>
    ''' <param name="g">绘图设备</param>
    ''' <param name="pen">线条</param>
    ''' <param name="root">聚类树根节点</param>
    ''' <param name="order">叶子顺序（与热图的行顺序一致）</param>
    ''' <param name="cellH">单元格高度</param>
    ''' <param name="y0">热图主体上边界</param>
    ''' <param name="xLeft">树的左边界（根所在的一侧）</param>
    ''' <param name="xRight">树的右边界（叶子所在的一侧）</param>
    Public Sub DrawRowTree(g As IGraphics, pen As Pen, root As Cluster, order As String(),
                           cellH As Single, y0 As Single, xLeft As Single, xRight As Single)

        Dim slots As Dictionary(Of String, Integer) = SlotIndex(order)
        Dim dmax As Double = MaxDistance(root)

        Call DrawRowNode(g, pen, root, slots, cellH, y0, xLeft, xRight, dmax)
    End Sub

    Private Function DrawRowNode(g As IGraphics, pen As Pen, node As Cluster,
                                 slots As Dictionary(Of String, Integer),
                                 cellH As Single, y0 As Single, xLeft As Single, xRight As Single,
                                 dmax As Double) As Single

        Dim x As Single = xRight - CSng(Ratio(node.DistanceValue, dmax)) * (xRight - xLeft)

        If node.Children.Count = 0 Then
            Dim slot As Integer

            If Not slots.TryGetValue(node.Name, slot) Then
                Return Single.NaN
            End If

            Return y0 + (slot + 0.5) * cellH
        End If

        Dim children As Cluster() = OrderedChildren(node)
        Dim ys(children.Length - 1) As Single
        Dim top As Single = Single.MaxValue
        Dim bottom As Single = Single.MinValue

        For i As Integer = 0 To children.Length - 1
            ys(i) = DrawRowNode(g, pen, children(i), slots, cellH, y0, xLeft, xRight, dmax)

            If Single.IsNaN(ys(i)) Then
                Return Single.NaN
            End If

            top = std.Min(top, ys(i))
            bottom = std.Max(bottom, ys(i))
        Next

        For i As Integer = 0 To children.Length - 1
            Dim childX As Single = xRight - CSng(Ratio(children(i).DistanceValue, dmax)) * (xRight - xLeft)

            g.DrawLine(pen, childX, ys(i), x, ys(i))
        Next

        g.DrawLine(pen, x, top, x, bottom)

        Return (top + bottom) / 2
    End Function

    Private Function SlotIndex(order As String()) As Dictionary(Of String, Integer)
        Dim slots As New Dictionary(Of String, Integer)

        For i As Integer = 0 To order.Length - 1
            If Not slots.ContainsKey(order(i)) Then
                slots(order(i)) = i
            End If
        Next

        Return slots
    End Function
End Module
