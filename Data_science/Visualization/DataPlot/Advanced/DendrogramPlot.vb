' ============================================================================
'  DendrogramPlot.vb - 层次聚类树图（竖直方向）
'
'  从 HCTreePlot 项目的 DendrogramPanel.vb + DendrogramPanelV2.vb 合并迁移而来：
'   1. 去掉对旧绘图库 Plot 基类与 Graphic.Theme 的依赖，
'      改用本文件内的 DendrogramTheme 与 g.GraphicsPlots 渲染入口；
'   2. 聚类数据模型（Cluster）仍来自 hctree 项目，ColorClass 仍来自 DataMining；
'   3. 渲染输出走 g.GraphicsPlots，支持 driver 参数（png / svg / postscript 等）。
' ============================================================================

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports std = System.Math

#If NET48 Then
Imports Brushes = System.Drawing.Brushes
Imports Font = System.Drawing.Font
Imports Pen = System.Drawing.Pen
Imports SolidBrush = System.Drawing.SolidBrush
#Else
Imports Brushes = Microsoft.VisualBasic.Imaging.Brushes
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
#End If

    ''' <summary>
    ''' 层次聚类树图的样式参数
    ''' </summary>
    Public Class DendrogramTheme

        Public Property padding As String = g.DefaultPadding
        Public Property background As String = "white"

        ''' <summary>树杈连接线的画笔 CSS</summary>
        Public Property gridStrokeX As String = Stroke.AxisGridStroke

        ''' <summary>标尺刻度字体 CSS</summary>
        Public Property axisTickCSS As String = CSSFont.PlotLabelNormal

        ''' <summary>标尺轴线画笔 CSS</summary>
        Public Property axisStroke As String = Stroke.AxisStroke

        ''' <summary>叶子标签字体 CSS</summary>
        Public Property tagCSS As String = CSSFont.PlotLabelNormal

        ''' <summary>距离标尺刻度数字的格式化字符串</summary>
        Public Property XaxisTickFormat As String = "F1"

        ''' <summary>样本节点圆点大小（0 表示不绘制）</summary>
        Public Property pointSize As Single = 10

    End Class

    ''' <summary>
    ''' 绘制层次聚类图(竖直方向)
    ''' </summary>
    Public Class DendrogramPlot

        Protected Friend ReadOnly hist As Cluster
        Protected Friend ReadOnly classIndex As Dictionary(Of String, ColorClass)

        ''' <summary>
        ''' leaf id map to <see cref="ColorClass.name"/>
        ''' </summary>
        Public ReadOnly Property classinfo As Dictionary(Of String, String)

        Protected Friend ReadOnly showAllLabels As Boolean
        Protected Friend ReadOnly showAllNodes As Boolean
        Protected Friend ReadOnly showLeafLabels As Boolean
        Protected Friend ReadOnly showRuler As Boolean

        Protected labelFont As Font
        Protected ReadOnly linkColor As Stroke
        Protected ReadOnly pointColor As SolidBrush
        Protected ReadOnly theme As DendrogramTheme

        Protected labels As New List(Of NamedValue(Of PointF))
        Protected legendWidth As Single = 20

        Public Sub New(hist As Cluster, theme As DendrogramTheme,
                       Optional classes As ColorClass() = Nothing,
                       Optional classinfo As Dictionary(Of String, String) = Nothing,
                       Optional showAllLabels As Boolean = False,
                       Optional showAllNodes As Boolean = False,
                       Optional pointColor$ = "red",
                       Optional showRuler As Boolean = True,
                       Optional showLeafLabels As Boolean = True)

            Me.hist = hist
            Me.theme = theme
            Me.classIndex = classes.SafeQuery.ToDictionary(Function(a) a.name)
            Me.classinfo = classinfo
            Me.showAllLabels = showAllLabels
            Me.linkColor = Stroke.TryParse(theme.gridStrokeX)
            Me.showAllNodes = showAllNodes
            Me.pointColor = pointColor.GetBrush
            Me.showLeafLabels = showLeafLabels
            Me.showRuler = showRuler
        End Sub

        ''' <summary>
        ''' get by <see cref="classinfo"/>
        ''' </summary>
        Protected Function GetColor(id As String) As Color
            If classinfo Is Nothing OrElse Not classinfo.ContainsKey(id) Then
                Return Nothing
            Else
                Return classIndex(classinfo(id)).color.TranslateColor
            End If
        End Function

        ''' <summary>
        ''' render the dendrogram plot image
        ''' </summary>
        Public Function Plot(Optional size$ = "2100,1600",
                             Optional dpi As Integer = 100,
                             Optional driver As Microsoft.VisualBasic.Imaging.Driver.Drivers = Microsoft.VisualBasic.Imaging.Driver.Drivers.Default) As GraphicsData

            Return g.GraphicsPlots(
                size:=size.SizeParser,
                padding:=theme.padding,
                bg:=theme.background,
                plotAPI:=AddressOf PlotInternal,
                driver:=driver,
                dpi:=dpi
            )
        End Function

        Private Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim plotRegion As Rectangle = canvas.PlotRegion(css)
            ' 每一个样本点都平分一段长度
            Dim unitWidth As Double = plotRegion.Height / hist.Leafs
            Dim axisTicks As Double()

            If hist.DistanceValue <= 0.1 Then
                axisTicks = {0, hist.DistanceValue}.Range.CreateAxisTicks(decimalDigits:=-1)
            Else
                axisTicks = {0, hist.DistanceValue}.Range.CreateAxisTicks
            End If

            labelFont = css.GetFont(CSSFont.TryParse(theme.tagCSS))

            Dim scaleX As d3js.scale.LinearScale = d3js.scale _
                .linear() _
                .domain(values:=axisTicks) _
                .range(integers:={plotRegion.Left, plotRegion.Right})

            ' 绘制距离标尺
            Dim left = plotRegion.Left + plotRegion.Right - scaleX(axisTicks.Max)
            Dim right = plotRegion.Left + plotRegion.Right - scaleX(0)
            Dim y = plotRegion.Top + unitWidth - unitWidth / 2
            Dim x!
            Dim tickFont As Font = css.GetFont(CSSFont.TryParse(theme.axisTickCSS))
            Dim tickFontHeight As Single = g.MeasureString("0", tickFont).Height
            Dim dh As Double = tickFontHeight / 3
            Dim tickLable As String
            Dim tickLabelSize As SizeF
            Dim labelPadding As Integer
            Dim charWidth As Integer = g.MeasureString("0", labelFont).Width
            Dim axisPen As Pen = css.GetPen(Stroke.TryParse(theme.axisStroke))

            If classinfo.IsNullOrEmpty Then
                labelPadding = g.MeasureString("0", labelFont).Width / 2
            Else
                labelPadding = g.MeasureString("00", labelFont).Width
            End If

            If showRuler Then
                Call g.DrawLine(axisPen, New PointF(left, y), New PointF(right, y))

                For Each tick As Double In axisTicks
                    x = plotRegion.Left + plotRegion.Right - scaleX(tick)
                    tickLable = tick.ToString(theme.XaxisTickFormat)
                    tickLabelSize = g.MeasureString(tickLable, tickFont)

                    g.DrawLine(axisPen, New PointF(x, y), New PointF(x, y - dh))
                    g.DrawString(tickLable, tickFont, Brushes.Black, New PointF(x - tickLabelSize.Width / 2, y - dh - tickFontHeight))
                Next
            End If

            Call DendrogramTree(hist, unitWidth, g, plotRegion, 0, scaleX, Nothing, labelPadding, charWidth)
        End Sub

        Protected Overridable Sub DendrogramTree(partition As Cluster,
                                                 unitWidth As Double,
                                                 g As IGraphics,
                                                 plotRegion As Rectangle,
                                                 i As Integer,
                                                 scaleX As d3js.scale.LinearScale,
                                                 parentPt As PointF,
                                                 labelPadding As Integer,
                                                 charWidth As Integer)

            Dim orders As Cluster() = partition.Children.OrderBy(Function(a) a.Leafs).ToArray
            Dim x = plotRegion.Left + plotRegion.Right - scaleX(partition.DistanceValue)
            Dim y As Integer
            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim linkColor As Pen = css.GetPen(Me.linkColor)

            If partition.isLeaf Then
                y = plotRegion.Top + i * unitWidth + unitWidth
                labels.Add(New NamedValue(Of PointF) With {
                    .Name = partition.Name,
                    .Value = New PointF(x, y)
                })
            Else
                ' 连接节点在中间？
                y = plotRegion.Top + (i + 0.5) * unitWidth + (partition.Leafs * unitWidth) / 2
            End If

            If Not parentPt.IsEmpty Then
                ' 绘制连接线
                Call g.DrawLine(linkColor, parentPt, New PointF(parentPt.X, y))
                Call g.DrawLine(linkColor, New PointF(x, y), New PointF(parentPt.X, y))
            End If

            If (partition.isLeaf OrElse showAllNodes) AndAlso theme.pointSize > 0 Then
                Call g.DrawCircle(New PointF(x, y), theme.pointSize, pointColor)
            End If

            If showLeafLabels AndAlso (partition.isLeaf OrElse showAllLabels) Then
                Dim lsize As SizeF = g.MeasureString(partition.Name, labelFont)
                Dim lpos As New PointF(x + labelPadding, y - lsize.Height / 2)

                Call g.DrawString(partition.Name, labelFont, Brushes.Black, lpos)
            End If

            If partition.isLeaf AndAlso Not classinfo.IsNullOrEmpty Then
                ' 绘制class颜色块
                Dim color As New SolidBrush(GetColor(partition.Name))
                Dim d As Double = std.Max(charWidth / 2, theme.pointSize)
                Dim layout As New Rectangle With {
                    .Location = New Point(x + d, y - unitWidth / 2),
                    .Size = New Size(legendWidth, unitWidth)
                }

                Call g.FillRectangle(color, layout)
            Else
                Dim n As Integer = 0

                parentPt = New PointF(x, y)

                For Each part As Cluster In orders
                    Call DendrogramTree(part, unitWidth, g, plotRegion, i + n, scaleX, parentPt, labelPadding, charWidth)
                    n += part.Leafs
                Next
            End If
        End Sub
    End Class