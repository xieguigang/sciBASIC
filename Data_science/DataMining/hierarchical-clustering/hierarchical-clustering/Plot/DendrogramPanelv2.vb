Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports stdNum = System.Math

''' <summary>
''' 绘制层次聚类图
''' </summary>
Public Class DendrogramPanelV2 : Inherits Plot

    Friend ReadOnly hist As Cluster
    Friend ReadOnly layout As Layouts
    Friend ReadOnly classIndex As Dictionary(Of String, ColorClass)

    ''' <summary>
    ''' leaf id map to <see cref="ColorClass.name"/>
    ''' </summary>
    Friend ReadOnly classinfo As Dictionary(Of String, String)
    Friend ReadOnly showAllLabels As Boolean
    Friend ReadOnly showAllNodes As Boolean

    ReadOnly labelFont As Font
    ReadOnly linkColor As Pen

    Public Sub New(hist As Cluster, theme As Theme,
                   Optional classes As ColorClass() = Nothing,
                   Optional classinfo As Dictionary(Of String, String) = Nothing,
                   Optional showAllLabels As Boolean = False,
                   Optional showAllNodes As Boolean = False)

        MyBase.New(theme)

        Me.hist = hist
        Me.classIndex = classes.SafeQuery.ToDictionary(Function(a) a.name)
        Me.classinfo = classinfo
        Me.showAllLabels = showAllLabels
        Me.labelFont = CSSFont.TryParse(theme.tagCSS)
        Me.linkColor = Stroke.TryParse(theme.gridStroke).GDIObject
        Me.showAllNodes = showAllNodes
    End Sub

    Private Function GetColor(id As String) As Color
        If classinfo Is Nothing OrElse Not classinfo.ContainsKey(id) Then
            Return Nothing
        Else
            Return classIndex(classinfo(id)).color.TranslateColor
        End If
    End Function

    Dim labels As New List(Of NamedValue(Of PointF))

    Public Function Paint(g As IGraphics, layout As Rectangle) As IEnumerable(Of NamedValue(Of PointF))
        Call labels.Clear()
        Call PlotInternal(g, EvaluateLayout(g, layout))

        Return labels
    End Function

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Dim plotRegion As Rectangle = canvas.PlotRegion
        ' 每一个样本点都平分一段长度
        Dim unitWidth As Double = plotRegion.Height / hist.Leafs
        Dim axisTicks = {0, hist.DistanceValue}.Range.CreateAxisTicks
        Dim scaleX As d3js.scale.LinearScale = d3js.scale.linear().domain(axisTicks).range(integers:={plotRegion.Left, plotRegion.Right})

        ' 绘制距离标尺
        Dim left = plotRegion.Right - scaleX(hist.DistanceValue)
        Dim right = plotRegion.Right - scaleX(0)
        Dim y = unitWidth - unitWidth * 0.1
        Dim x!
        Dim tickFont As Font = CSSFont.TryParse(theme.axisTickCSS)
        Dim tickFontHeight As Single = g.MeasureString("0", tickFont).Height
        Dim dh As Double = tickFontHeight / 3
        Dim tickLable As String
        Dim tickLabelSize As SizeF
        Dim labelPadding As Integer
        Dim charWidth As Integer = g.MeasureString("0", labelFont).Width
        Dim axisPen As Pen = Stroke.TryParse(theme.axisStroke)

        If classinfo.IsNullOrEmpty Then
            labelPadding = g.MeasureString("0", labelFont).Width / 2
        Else
            labelPadding = g.MeasureString("00", labelFont).Width
        End If

        Call g.DrawLine(axisPen, New PointF(left, y), New PointF(right, y))

        For Each tick As Double In axisTicks.Take(axisTicks.Length - 1)
            x = plotRegion.Right - scaleX(tick)
            tickLable = tick.ToString
            tickLabelSize = g.MeasureString(tickLable, tickFont)

            g.DrawLine(axisPen, New PointF(x, y), New PointF(x, y - dh))
            g.DrawString(tick, tickFont, Brushes.Black, New PointF(x - tickLabelSize.Width / 2, y - dh - tickFontHeight))
        Next

        Call DendrogramPlot(hist, unitWidth, g, plotRegion, 0, scaleX, Nothing, labelPadding, charWidth)
    End Sub

    Private Overloads Sub DendrogramPlot(partition As Cluster,
                                         unitWidth As Double,
                                         g As IGraphics,
                                         plotRegion As Rectangle,
                                         i As Integer,
                                         scaleX As d3js.scale.LinearScale,
                                         parentPt As PointF,
                                         labelPadding As Integer,
                                         charWidth As Integer)

        Dim orders As Cluster() = partition.Children.OrderBy(Function(a) a.Leafs).ToArray
        Dim x = plotRegion.Right - scaleX(partition.DistanceValue)
        Dim y As Integer

        If partition.isLeaf Then
            y = i * unitWidth + unitWidth
            labels += New NamedValue(Of PointF) With {
                .Name = partition.Name,
                .Value = New PointF(x, y)
            }
        Else
            ' 连接节点在中间？
            y = (i + 0.5) * unitWidth + (partition.Leafs * unitWidth) / 2
        End If

        If Not parentPt.IsEmpty Then
            ' 绘制连接线
            Call g.DrawLine(linkColor, parentPt, New PointF(parentPt.X, y))
            Call g.DrawLine(linkColor, New PointF(x, y), New PointF(parentPt.X, y))
        End If

        If partition.isLeaf OrElse showAllNodes Then
            Call g.DrawCircle(New PointF(x, y), theme.PointSize, Brushes.Red)
        End If

        If partition.isLeaf OrElse showAllLabels Then
            Dim lsize As SizeF = g.MeasureString(partition.Name, labelFont)
            Dim lpos As New PointF(x + labelPadding, y - lsize.Height / 2)

            Call g.DrawString(partition.Name, labelFont, Brushes.Black, lpos)
        End If

        If partition.isLeaf Then
            ' 绘制class颜色块
            Dim color As New SolidBrush(GetColor(partition.Name))
            Dim d As Double = stdNum.Max(charWidth / 2, theme.PointSize)
            Dim layout As New Rectangle With {
                .Location = New Point(x + d, y - unitWidth / 2),
                .Size = New Size(labelPadding - d * 1.25, unitWidth)
            }

            Call g.FillRectangle(color, layout)
        Else
            Dim n As Integer = 0

            parentPt = New PointF(x, y)

            For Each part As Cluster In orders
                DendrogramPlot(part, unitWidth, g, plotRegion, i + n, scaleX, parentPt, labelPadding, charWidth)
                n += part.Leafs
            Next
        End If
    End Sub
End Class
