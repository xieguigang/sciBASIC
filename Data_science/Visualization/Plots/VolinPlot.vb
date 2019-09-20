#Region "Microsoft.VisualBasic::b7d9f95d37c33f555e7dcd5ce2173ed2, Data_science\Visualization\Plots\VolinPlot.vb"

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

' Module VolinPlot
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
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Interpolation
Imports Microsoft.VisualBasic.Math.Quantile
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' ## 小提琴图
''' 
''' + 高度为数据的分布位置
''' + 宽度为对应的百分位上的数据点的数量
''' + 长度为最小值与最大值之间的差值
''' </summary>
Public Module VolinPlot

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
                         Optional yTickFormat$ = "F2") As GraphicsData
        With dataset.ToArray
            Return .PropertyNames _
                   .Select(Function(label)
                               Return New NamedCollection(Of Double)(label, .Vector(label))
                           End Function) _
                   .DoCall(Function(data)
                               Return VolinPlot.Plot(
                                   dataset:=data,
                                   size:=size,
                                   margin:=margin,
                                   bg:=bg,
                                   colorset:=colorset,
                                   Ylabel:=Ylabel,
                                   yLabelFontCSS:=yLabelFontCSS,
                                   ytickFontCSS:=ytickFontCSS,
                                   removesOutliers:=removesOutliers,
                                   yTickFormat:=yTickFormat
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
                         Optional colorset$ = DesignerTerms.TSFShellColors,
                         Optional Ylabel$ = "y axis",
                         Optional yLabelFontCSS$ = Canvas.Resolution2K.PlotSmallTitle,
                         Optional ytickFontCSS$ = Canvas.Resolution2K.PlotLabelNormal,
                         Optional splineDegree% = 2,
                         Optional removesOutliers As Boolean = True,
                         Optional yTickFormat$ = "F2") As GraphicsData

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
            .Select(Function(d) d.AsEnumerable) _
            .IteratesALL _
            .ToArray
        Dim yticks = alldata.Range.CreateAxisTicks
        Dim yTickFont As Font = CSSFont.TryParse(ytickFontCSS)
        Dim colors = Designer.GetColors(colorset, matrix.Length)
        Dim labelSize As SizeF
        Dim labelFont As Font = CSSFont.TryParse(yLabelFontCSS)
        Dim labelPos As PointF

        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)
                Dim plotRegion As Rectangle = region.PlotRegion
                Dim Y = d3js.scale.linear.domain(yticks).range(integers:={plotRegion.Top, plotRegion.Bottom})
                Dim yScale As New YScaler(False) With {
                    .region = plotRegion,
                    .Y = Y
                }

                Call Axis.DrawY(g, Pens.Black, Ylabel, yScale, 0, yticks, YAxisLayoutStyles.Left, Nothing, yLabelFontCSS, yTickFont,
                                htmlLabel:=False,
                                tickFormat:=yTickFormat
                )

                Dim groupInterval = plotRegion.Width * 0.1
                Dim maxWidth = (plotRegion.Width - groupInterval) / matrix.Length

                groupInterval = groupInterval / matrix.Length

                Dim semiWidth = maxWidth / 2
                Dim X As Single = plotRegion.Left + groupInterval + semiWidth
                Dim index As i32 = Scan0

                For Each group As NamedCollection(Of Double) In matrix
                    ' Dim q = quantiles(group)
                    Dim upper = yScale.TranslateY(group.Max)
                    Dim lower = yScale.TranslateY(group.Min)
                    ' 计算数据分布的密度之后，进行左右对称的线条的生成
                    Dim line_l As New List(Of PointF)
                    Dim line_r As New List(Of PointF)
                    Dim q0 = group.Min
                    Dim dstep = (group.Max - group.Min) / 10
                    Dim dy = Math.Abs(upper - lower) / 10
                    Dim outliers As New List(Of PointF)

                    For p As Integer = 1 To 10
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

                    line_l = line_l.BSpline(degree:=splineDegree)
                    line_r = line_r.BSpline(degree:=splineDegree)

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
                    Call g.DrawPolygon(Pens.LightGray, polygon)
                    Call g.FillPolygon(New SolidBrush(colors(++index)), polygon)

                    labelSize = g.MeasureString(group.name, labelFont)
                    labelPos = New PointF With {
                        .X = X - labelSize.Width / 2,
                        .Y = plotRegion.Bottom + 10
                    }

                    ' 绘制X坐标轴分组标签
                    Call g.DrawString(group.name, labelFont, Brushes.Black, labelPos)

                    X += semiWidth + groupInterval + semiWidth
                Next
            End Sub

        Return g.GraphicsPlots(size.SizeParser, margin, bg, plotInternal)
    End Function
End Module

