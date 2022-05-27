Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Interpolation
Imports Microsoft.VisualBasic.Math.Quantile
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization.JSON
Imports stdNum = System.Math

Public Class Violin : Inherits Plot

    ReadOnly matrix As NamedCollection(Of Double)()

    Public Property showStats As Boolean
    Public Property splineDegree As Integer

    Public Sub New(matrix As IEnumerable(Of NamedCollection(Of Double)), theme As Theme)
        MyBase.New(theme)

        Me.matrix = matrix.ToArray
    End Sub

    Public Shared Iterator Function removesOutliers(matrix As IEnumerable(Of NamedCollection(Of Double))) As IEnumerable(Of NamedCollection(Of Double))
        For Each i As NamedCollection(Of Double) In matrix
            Dim quar = i.Quartile
            Dim normals = quar.Outlier(i).normal

            Yield New NamedCollection(Of Double) With {
                .name = i.name,
                .description = i.description,
                .value = normals
            }
        Next
    End Function

    Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
        ' 用来构建Y坐标轴的总体数据
        Dim alldata = matrix _
            .Select(Function(d)
                        Dim dq = d.Quartile
                        Return {dq.Q1 - 1.5 * dq.IQR, dq.Q3 + 1.5 * dq.IQR}.AsList + d.AsEnumerable
                    End Function) _
            .IteratesALL _
            .ToArray
        Dim ppi As Integer = g.Dpi
        Dim yticks = alldata.Range.CreateAxisTicks
        Dim yTickFont As Font = CSSFont.TryParse(theme.axisTickCSS).GDIObject(ppi)
        Dim yTickColor As Brush = CSSFont.TryParse(theme.axisTickCSS).color.GetBrush
        Dim colors = Designer.GetColors(theme.colorSet, matrix.Length)
        Dim labelSize As SizeF
        Dim labelFont As Font = CSSFont.TryParse(theme.axisLabelCSS).GDIObject(ppi)
        Dim labelColor As Brush = CSSFont.TryParse(theme.axisLabelCSS).color.GetBrush
        Dim labelPos As PointF
        Dim polygonStroke As Pen = Stroke.TryParse(theme.lineStroke)
        Dim titleFont As Font = CSSFont.TryParse(theme.mainCSS).GDIObject(ppi)
        Dim plotRegion As Rectangle = canvas.PlotRegion
        Dim Y = d3js.scale.linear.domain(values:=yticks).range(integers:={plotRegion.Top, plotRegion.Bottom})
        Dim yScale As New YScaler(False) With {
                    .region = plotRegion,
                    .Y = Y
                }

        Call Axis.DrawY(g,
                        Pens.Black,
                        ylabel,
                        yScale,
                        0,
                        yticks,
                        YAxisLayoutStyles.Left,
                        Nothing,
                        theme.axisLabelCSS,
                        labelColor,
                        yTickFont,
                        yTickColor,
                        htmlLabel:=False,
                        tickFormat:=theme.YaxisTickFormat
        )

        Dim groupInterval As Double = plotRegion.Width * 0.1
        Dim maxWidth As Double = (plotRegion.Width - groupInterval) / matrix.Length

        groupInterval = groupInterval / matrix.Length

        Dim semiWidth As Double = maxWidth / 2
        Dim X As Single = plotRegion.Left + groupInterval / 2 + semiWidth
        Dim index As i32 = Scan0

        labelSize = g.MeasureString(main, titleFont)
        labelPos = New PointF With {
            .X = plotRegion.Left + (plotRegion.Width - labelSize.Width) / 2,
            .Y = plotRegion.Y - labelSize.Height
        }
        g.DrawString(main, titleFont, Brushes.Black, labelPos)

        For Each group As NamedCollection(Of Double) In matrix
            Dim quartile As DataQuartile = group.Quartile
            Dim lowerBound = quartile.Q1 - 1.5 * quartile.IQR
            Dim upperBound = quartile.Q3 + 1.5 * quartile.IQR
            Dim upper = yScale.TranslateY(upperBound)
            Dim lower = yScale.TranslateY(lowerBound)
            ' 计算数据分布的密度之后，进行左右对称的线条的生成
            Dim line_l As New List(Of PointF)
            Dim line_r As New List(Of PointF)
            Dim q0 = lowerBound  'group.Min
            Dim n As Integer = 30
            Dim dstep = (upperBound - lowerBound) / n ' (group.Max - group.Min) / n
            Dim dy = stdNum.Abs(upper - lower) / n
            Dim outliers As New List(Of PointF)

            For p As Integer = 0 To n
                Dim q1 = q0 + dstep
                Dim range As DoubleRange = {q0, q1}
                Dim density = group.Count(AddressOf range.IsInside)

                line_l += New PointF With {.X = density, .Y = lower - p * dy}
                line_r += New PointF With {.X = density, .Y = lower - p * dy}
                q0 = q1
            Next

            Call $"{group.name} = {New Double() {group.Min, group.Max}.GetJson}".__DEBUG_ECHO

            ' 进行宽度伸缩映射
            Dim maxDensity As DoubleRange = line_l.X
            Dim densityWidth As Single

            For i As Integer = 0 To line_r.Count - 1
                densityWidth = (line_l(i).X - maxDensity.Min) / maxDensity.Length * semiWidth

                line_l(i) = New PointF With {.X = X - densityWidth, .Y = line_l(i).Y}
                line_r(i) = New PointF With {.X = X + densityWidth, .Y = line_r(i).Y}
            Next

            line_l = line_l.BSpline(degree:=splineDegree).AsList
            line_r = line_r.BSpline(degree:=splineDegree).AsList

            ' 需要插值么？
            ' 生成多边形
            Dim polygon As New List(Of PointF)

            ' 左下 -> 左上
            polygon += line_l
            ' 左上 -> 右上
            polygon += line_r.Last
            ' 右上 -> 右下
            polygon += line_r.ReverseIterator.Skip(1)
            ' 最后 右下 -> 左下会自动封闭

            ' 绘制当前的这个多边形
            Call g.DrawPolygon(polygonStroke, polygon)
            Call g.FillPolygon(New SolidBrush(Colors(++index)), polygon)

            ' 绘制quartile
            Dim yQ1 As Double = yScale.TranslateY(quartile.Q1)

            ' draw IQR
            Dim iqrBox As New RectangleF With {
                        .Width = 32,
                        .X = X - .Width / 2,
                        .Y = yScale.TranslateY(quartile.Q3),
                        .Height = stdNum.Abs(.Y - yQ1)
                    }

            Call g.FillRectangle(polygonStroke.Brush, iqrBox)

            ' draw 95% CI
            upperBound = group.Average + 1.96 * group.SD
            lowerBound = group.Average - 1.96 * group.SD

            Call g.DrawLine(
                        pen:=polygonStroke,
                        pt1:=New PointF(X, yScale.TranslateY(lowerBound)),
                        pt2:=New PointF(X, yScale.TranslateY(upperBound))
                    )

            ' draw median point
            Call g.DrawCircle(New PointF(X + 1, yScale.TranslateY(quartile.Q2) - 1), 12, color:=Pens.White)

            ' 在右上绘制数据的分布信息
            Dim sampleDescrib As String =
                    $"CI95%: {lowerBound.ToString(theme.YaxisTickFormat)} ~ {upperBound.ToString(theme.YaxisTickFormat)}" & vbCrLf &
                    $"Median: {quartile.Q2.ToString(theme.YaxisTickFormat)}" & vbCrLf &
                    $"Normal Range: {(quartile.Q1 - 1.5 * quartile.IQR).ToString(theme.YaxisTickFormat)} ~ {(quartile.Q3 + 1.5 * quartile.IQR).ToString(theme.YaxisTickFormat)}"

            labelSize = g.MeasureString(group.name, labelFont)

            If showStats Then
                Call g.DrawString(sampleDescrib, labelFont, Brushes.Black, New PointF(X + semiWidth / 5, upper + labelSize.Height * 2))
            End If

            If theme.xAxisRotate = 0.0 Then
                labelPos = New PointF With {
                    .X = X - labelSize.Width / 2,
                    .Y = plotRegion.Bottom + labelSize.Height * 1.125
                }

                Call g.DrawString(group.name, labelFont, Brushes.Black, labelPos)
            Else
                labelPos = New PointF With {
                            .X = X - labelSize.Width / 2,
                            .Y = plotRegion.Bottom + labelSize.Width * stdNum.Sin(stdNum.PI / 4)
                        }

                ' 绘制X坐标轴分组标签
                Call New GraphicsText(DirectCast(g, GDICanvas).Graphics).DrawString(
                    s:=group.name,
                    font:=labelFont,
                    brush:=Brushes.Black,
                    point:=labelPos,
                    angle:=theme.xAxisRotate
                )
            End If

            X += semiWidth + groupInterval + semiWidth
        Next
    End Sub
End Class
