Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.DataMining.ComponentModel.Encoder
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports stdNum = System.Math

Public Class Circular : Inherits DendrogramPanel

    Public Sub New(hist As Cluster, theme As Theme,
                   Optional classes As ColorClass() = Nothing,
                   Optional classinfo As Dictionary(Of String, String) = Nothing,
                   Optional showAllLabels As Boolean = False,
                   Optional showAllNodes As Boolean = False,
                   Optional pointColor$ = "red")

        MyBase.New(hist, theme, classes, classinfo, showAllLabels, showAllNodes, pointColor)
    End Sub

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        Dim plotRegion = canvas.PlotRegion
        Dim maxRadius As Double = stdNum.Min(plotRegion.Width, plotRegion.Height) / 2
        ' 每一个样本点都平分一段长度
        Dim unitAngle As Double = (2 * stdNum.PI) / hist.Leafs
        Dim axisTicks As Double()

        If hist.DistanceValue <= 0.1 Then
            axisTicks = {0, hist.DistanceValue}.Range.CreateAxisTicks(decimalDigits:=-1)
        Else
            axisTicks = {0, hist.DistanceValue}.Range.CreateAxisTicks
        End If

        Dim scaleR As d3js.scale.LinearScale = d3js.scale _
            .linear() _
            .domain(axisTicks) _
            .range(integers:={0, maxRadius})

        ' 绘制距离标尺
        Dim outer = plotRegion.Left + plotRegion.Right - scaleR(axisTicks.Max)
        Dim inner = plotRegion.Left + plotRegion.Right - scaleR(0)
        Dim y = plotRegion.Top + unitAngle - unitAngle / 2
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

        Call g.DrawLine(axisPen, New PointF(outer, y), New PointF(inner, y))

        For Each tick As Double In axisTicks
            x = plotRegion.Left + plotRegion.Right - scaleR(tick)
            tickLable = tick.ToString(theme.axisTickFormat)
            tickLabelSize = g.MeasureString(tickLable, tickFont)

            g.DrawLine(axisPen, New PointF(x, y), New PointF(x, y - dh))
            g.DrawString(tickLable, tickFont, Brushes.Black, New PointF(x - tickLabelSize.Width / 2, y - dh - tickFontHeight))
        Next

        Call DendrogramPlot(hist, unitAngle, g, plotRegion, 0, scaleR, Nothing, labelPadding, charWidth)
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
        Dim x = plotRegion.Left + plotRegion.Right - scaleX(partition.DistanceValue)
        Dim y As Integer

        If partition.isLeaf Then
            y = plotRegion.Top + i * unitWidth + unitWidth
            labels += New NamedValue(Of PointF) With {
                .Name = partition.Name,
                .Value = New PointF(x, y)
            }
        Else
            ' 连接节点在中间？
            y = plotRegion.Top + (i + 0.5) * unitWidth + (partition.Leafs * unitWidth) / 2
        End If

        If Not parentPt.IsEmpty Then
            ' 绘制连接线
            Call g.DrawLine(linkColor, parentPt, New PointF(parentPt.X, y))
            Call g.DrawLine(linkColor, New PointF(x, y), New PointF(parentPt.X, y))
        End If

        If partition.isLeaf OrElse showAllNodes Then
            Call g.DrawCircle(New PointF(x, y), theme.PointSize, pointColor)
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
