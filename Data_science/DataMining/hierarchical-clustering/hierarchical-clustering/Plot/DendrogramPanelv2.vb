Imports System.Drawing
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Public Class DendrogramPanelV2 : Inherits Plot

    Friend ReadOnly hist As Cluster
    Friend ReadOnly layout As Layouts
    Friend ReadOnly classIndex As Dictionary(Of String, ColorClass)

    ''' <summary>
    ''' leaf id map to <see cref="ColorClass.name"/>
    ''' </summary>
    Friend ReadOnly classinfo As Dictionary(Of String, String)
    Friend ReadOnly showAllLabels As Boolean

    ReadOnly labelFont As Font

    Public Sub New(hist As Cluster, theme As Theme,
                   Optional classes As ColorClass() = Nothing,
                   Optional classinfo As Dictionary(Of String, String) = Nothing,
                   Optional showAllLabels As Boolean = False)

        MyBase.New(theme)

        Me.hist = hist
        Me.classIndex = classes.SafeQuery.ToDictionary(Function(a) a.name)
        Me.classinfo = classinfo
        Me.showAllLabels = showAllLabels
        Me.labelFont = CSSFont.TryParse(theme.tagCSS)
    End Sub

    Private Function GetColor(id As String) As Color
        If classinfo Is Nothing OrElse Not classinfo.ContainsKey(id) Then
            Return Nothing
        Else
            Return classIndex(classinfo(id)).color.TranslateColor
        End If
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
        Dim y = unitWidth / 3
        Dim x!
        Dim tickFont As Font = CSSFont.TryParse(theme.axisTickCSS)
        Dim tickFontHeight As Single = g.MeasureString("0", tickFont).Height
        Dim dh As Double = tickFontHeight / 3
        Dim tickLable As String
        Dim tickLabelSize As SizeF

        Call g.DrawLine(Pens.Black, New PointF(left, y), New PointF(right, y))

        For Each tick As Double In axisTicks.Take(axisTicks.Length - 1)
            x = plotRegion.Right - scaleX(tick)
            tickLable = tick.ToString
            tickLabelSize = g.MeasureString(tickLable, tickFont)

            g.DrawLine(Pens.Black, New PointF(x, y), New PointF(x, y - dh))
            g.DrawString(tick, tickFont, Brushes.Black, New PointF(x - tickLabelSize.Width / 2, y - dh - tickFontHeight))
        Next

        Call DendrogramPlot(hist, unitWidth, g, plotRegion, 0, scaleX, Nothing)
    End Sub

    Private Overloads Sub DendrogramPlot(partition As Cluster,
                                         unitWidth As Double,
                                         g As IGraphics,
                                         plotRegion As Rectangle,
                                         i As Integer,
                                         scaleX As d3js.scale.LinearScale,
                                         parentPt As PointF)

        Dim orders As Cluster() = partition.Children.OrderBy(Function(a) a.Leafs).ToArray
        Dim x = plotRegion.Right - scaleX(partition.DistanceValue)
        Dim y As Integer

        If partition.isLeaf Then
            y = i * unitWidth + unitWidth / 2
        Else
            ' 连接节点在中间？
            y = i * unitWidth + (partition.Leafs * unitWidth) / 2
        End If

        If Not parentPt.IsEmpty Then
            ' 绘制连接线
            Call g.DrawLine(Pens.Blue, parentPt, New PointF(parentPt.X, y))
            Call g.DrawLine(Pens.Blue, New PointF(x, y), New PointF(parentPt.X, y))
        End If

        Call g.DrawCircle(New PointF(x, y), 15, Brushes.Red)

        If partition.isLeaf OrElse showAllLabels Then
            Dim lsize As SizeF = g.MeasureString(partition.Name, labelFont)
            Dim lpos As New PointF(x, y - lsize.Height / 2)

            Call g.DrawString(partition.Name, labelFont, Brushes.Black, lpos)
        End If

        If partition.isLeaf Then
            ' 绘制class颜色块
            Dim color As New SolidBrush(GetColor(partition.Name))
            Dim layout As New Rectangle With {
                .Location = New Point(x, y),
                .Size = New Size(10, unitWidth)
            }

            Call g.FillRectangle(color, layout)
        Else
            Dim n As Integer = 0

            parentPt = New PointF(x, y)

            For Each part As Cluster In orders
                Call DendrogramPlot(part, unitWidth, g, plotRegion, i + n, scaleX, parentPt)
                n += part.Leafs
            Next
        End If
    End Sub
End Class
