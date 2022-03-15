#Region "Microsoft.VisualBasic::df4f21583a32f9fb5eba615b883895ff, sciBASIC#\Data_science\Visualization\Plots\ViolinPlot.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 299
    '    Code Lines: 219
    ' Comment Lines: 41
    '   Blank Lines: 39
    '     File Size: 14.10 KB


    ' Module ViolinPlot
    ' 
    '     Function: (+2 Overloads) Plot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Interpolation
Imports Microsoft.VisualBasic.Math.Quantile
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization.JSON
Imports stdNum = System.Math

''' <summary>
''' ## 小提琴图
''' 
''' + 高度为数据的分布位置
''' + 宽度为对应的百分位上的数据点的数量
''' + 长度为最小值与最大值之间的差值
''' </summary>
Public Module ViolinPlot

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="dataset">数据集中的样本数据可以不必等长</param>
    ''' <param name="size"></param>
    ''' <param name="margin"></param>
    ''' <param name="bg"></param>
    ''' <param name="colorset"></param>
    ''' <returns></returns>
    Public Function Plot(dataset As IEnumerable(Of DataSet),
                         Optional size$ = Canvas.Resolution2K.Size,
                         Optional margin$ = Canvas.Resolution2K.PaddingWithTopTitle,
                         Optional bg$ = "white",
                         Optional colorset$ = DesignerTerms.TSFShellColors,
                         Optional Ylabel$ = "y axis",
                         Optional yLabelFontCSS$ = Canvas.Resolution2K.PlotSmallTitle,
                         Optional ytickFontCSS$ = Canvas.Resolution2K.PlotLabelNormal,
                         Optional removesOutliers As Boolean = True,
                         Optional yTickFormat$ = "F2",
                         Optional stroke$ = Stroke.AxisStroke,
                         Optional title$ = "Volin Plot",
                         Optional titleFontCSS$ = Canvas.Resolution2K.PlotTitle,
                         Optional labelAngle As Double = -45,
                         Optional showStats As Boolean = True) As GraphicsData

        With dataset.ToArray
            Return .PropertyNames _
                   .Select(Function(label)
                               Return New NamedCollection(Of Double)(label, .Vector(label))
                           End Function) _
                   .DoCall(Function(data)
                               Return ViolinPlot.Plot(
                                   dataset:=data,
                                   size:=size,
                                   margin:=margin,
                                   bg:=bg,
                                   colorset:=colorset,
                                   Ylabel:=Ylabel,
                                   yLabelFontCSS:=yLabelFontCSS,
                                   ytickFontCSS:=ytickFontCSS,
                                   removesOutliers:=removesOutliers,
                                   yTickFormat:=yTickFormat,
                                   strokeCSS:=stroke,
                                   title:=title,
                                   titleFontCSS:=titleFontCSS,
                                   labelAngle:=labelAngle,
                                   showStats:=showStats
                               )
                           End Function)
        End With
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="dataset">数据集中的样本数据可以不必等长</param>
    ''' <param name="size"></param>
    ''' <param name="margin"></param>
    ''' <param name="bg"></param>
    ''' <param name="colorset"></param>
    ''' <returns></returns>
    Public Function Plot(dataset As IEnumerable(Of NamedCollection(Of Double)),
                         Optional size$ = Canvas.Resolution2K.Size,
                         Optional margin$ = Canvas.Resolution2K.PaddingWithTopTitle,
                         Optional bg$ = "white",
                         Optional colorset$ = "#94cac1",
                         Optional Ylabel$ = "y axis",
                         Optional yLabelFontCSS$ = Canvas.Resolution2K.PlotSmallTitle,
                         Optional ytickFontCSS$ = Canvas.Resolution2K.PlotLabelNormal,
                         Optional splineDegree% = 2,
                         Optional removesOutliers As Boolean = True,
                         Optional yTickFormat$ = "F2",
                         Optional strokeCSS$ = "stroke: #6e797a; stroke-width: 15px; stroke-dash: solid;",
                         Optional title$ = "Volin Plot",
                         Optional titleFontCSS$ = Canvas.Resolution2K.PlotTitle,
                         Optional labelAngle As Double = -45,
                         Optional showStats As Boolean = True,
                         Optional ppi As Integer = 100) As GraphicsData

        Dim matrix As NamedCollection(Of Double)() = dataset.ToArray

        If removesOutliers Then
            For i As Integer = 0 To matrix.Length - 1
                Dim quar = matrix(i).Quartile
                Dim normals = quar.Outlier(matrix(i)).normal

                matrix(i) = New NamedCollection(Of Double) With {
                    .name = matrix(i).name,
                    .description = matrix(i).description,
                    .value = normals
                }
            Next
        End If

        ' 用来构建Y坐标轴的总体数据
        Dim alldata = matrix _
            .Select(Function(d)
                        Dim dq = d.Quartile
                        Return {dq.Q1 - 1.5 * dq.IQR, dq.Q3 + 1.5 * dq.IQR}.AsList + d.AsEnumerable
                    End Function) _
            .IteratesALL _
            .ToArray
        Dim yticks = alldata.Range.CreateAxisTicks
        Dim yTickFont As Font = CSSFont.TryParse(ytickFontCSS).GDIObject(ppi)
        Dim yTickColor As Brush = CSSFont.TryParse(ytickFontCSS).color.GetBrush
        Dim colors = Designer.GetColors(colorset, matrix.Length)
        Dim labelSize As SizeF
        Dim labelFont As Font = CSSFont.TryParse(yLabelFontCSS).GDIObject(ppi)
        Dim labelColor As Brush = CSSFont.TryParse(yLabelFontCSS).color.GetBrush
        Dim labelPos As PointF
        Dim polygonStroke As Pen = Stroke.TryParse(strokeCSS)
        Dim titleFont As Font = CSSFont.TryParse(titleFontCSS).GDIObject(ppi)

        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)
                Dim plotRegion As Rectangle = region.PlotRegion
                Dim Y = d3js.scale.linear.domain(values:=yticks).range(integers:={plotRegion.Top, plotRegion.Bottom})
                Dim yScale As New YScaler(False) With {
                    .region = plotRegion,
                    .Y = Y
                }

                Call Axis.DrawY(g, Pens.Black, Ylabel, yScale, 0, yticks, YAxisLayoutStyles.Left, Nothing, yLabelFontCSS, labelColor, yTickFont, yTickColor,
                                htmlLabel:=False,
                                tickFormat:=yTickFormat
                )

                Dim groupInterval As Double = plotRegion.Width * 0.1
                Dim maxWidth As Double = (plotRegion.Width - groupInterval) / matrix.Length

                groupInterval = groupInterval / matrix.Length

                Dim semiWidth As Double = maxWidth / 2
                Dim X As Single = plotRegion.Left + groupInterval / 2 + semiWidth
                Dim index As i32 = Scan0

                labelSize = g.MeasureString(title, titleFont)
                labelPos = New PointF With {
                    .X = plotRegion.Left + (plotRegion.Width - labelSize.Width) / 2,
                    .Y = plotRegion.Y - labelSize.Height
                }
                g.DrawString(title, titleFont, Brushes.Black, labelPos)

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
                    Call g.FillPolygon(New SolidBrush(colors(++index)), polygon)

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
                    $"CI95%: {lowerBound.ToString(yTickFormat)} ~ {upperBound.ToString(yTickFormat)}" & vbCrLf &
                    $"Median: {quartile.Q2.ToString(yTickFormat)}" & vbCrLf &
                    $"Normal Range: {(quartile.Q1 - 1.5 * quartile.IQR).ToString(yTickFormat)} ~ {(quartile.Q3 + 1.5 * quartile.IQR).ToString(yTickFormat)}"

                    labelSize = g.MeasureString(group.name, labelFont)

                    If showStats Then
                        Call g.DrawString(sampleDescrib, labelFont, Brushes.Black, New PointF(X + semiWidth / 5, upper + labelSize.Height * 2))
                    End If

                    If labelAngle = 0.0 Then
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
                            angle:=labelAngle
                        )
                    End If

                    X += semiWidth + groupInterval + semiWidth
                Next
            End Sub

        Return g.GraphicsPlots(size.SizeParser, margin, bg, plotInternal)
    End Function
End Module
