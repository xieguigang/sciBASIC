#Region "Microsoft.VisualBasic::a862fa266963ae182346e35f9d6898d8, Data_science\Visualization\Plots-statistics\Heatmap\CorrelationHeatmap.vb"

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

'     Module CorrelationHeatmap
' 
'         Function: Plot
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.DataFrame
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports stdNum = System.Math

Namespace Heatmap

    Public Module CorrelationHeatmap

        ''' <summary>
        ''' 只能够用来表示两两变量之间的相关度
        ''' </summary>
        ''' <param name="rowLabelFontStyle">因为是三角形的矩阵，所以行和列的字体都使用相同的值了</param>
        ''' <param name="variantSize">热图之中的圆圈的半径大小是否随着相关度的值而发生改变？</param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function Plot(data As DistanceMatrix,
                             Optional mapLevels% = 30,
                             Optional mapName$ = "lighter(" & ColorBrewer.DivergingSchemes.RdBu11 & ",0.05)",
                             Optional size$ = "1600,1600",
                             Optional padding$ = g.SmallPadding,
                             Optional bg$ = "white",
                             Optional logScale# = 0,
                             Optional rowDendrogramHeight% = 200,
                             Optional rowDendrogramClass As Dictionary(Of String, String) = Nothing,
                             Optional rowLabelFontStyle$ = CSSFont.PlotTitle,
                             Optional legendTitle$ = "Correlation Colors",
                             Optional legendFont$ = CSSFont.Win7VeryLarge,
                             Optional legendLabelFont$ = CSSFont.PlotTitle,
                             Optional range As DoubleRange = Nothing,
                             Optional mainTitle$ = "heatmap",
                             Optional titleFont$ = CSSFont.Win7VeryVeryLarge,
                             Optional drawGrid As Boolean = False,
                             Optional drawValueLabel As Boolean = False,
                             Optional valuelabelFontCSS$ = CSSFont.PlotLabelNormal,
                             Optional variantSize As Boolean = True,
                             Optional gridCSS$ = "stroke: lightgray; stroke-width: 1px; stroke-dash: solid;",
                             Optional driver As Drivers = Drivers.Default) As GraphicsData

            Dim margin As Padding = padding
            Dim valuelabelFont As Font = CSSFont.TryParse(valuelabelFontCSS)
            Dim min#, max#
            Dim gridBrush As Pen = Stroke.TryParse(gridCSS).GDIObject
            Dim rowLabelFont As Font = CSSFont.TryParse(rowLabelFontStyle).GDIObject
            Dim keys$() = data.keys
            Dim leftOffSet% = margin.Left / 1.5

            With range Or data _
                .PopulateRows _
                .IteratesALL _
                .ToArray _
                .Range _
                .AsDefault

                min = .Min
                max = .Max

                range = {0, .Max}
            End With

            Dim colors = Designer.GetColors(mapName, mapLevels).Reverse.GetBrushes
            Dim plotInternal = Sub(ByRef g As IGraphics, region As GraphicsRegion)
                                   Dim maxLabelSize = data _
                                        .keys _
                                        .MaxLengthString _
                                        .MeasureSize(g, rowLabelFont) _
                                        .Width

                                   Dim dStep As New SizeF With {
                                        .Width = (region.PlotRegion.Width - maxLabelSize) / data.size,
                                        .Height = region.PlotRegion.Height / data.size
                                   }
                                   ' 在绘制上三角的时候假设每一个对象的keys的顺序都是相同的
                                   Dim dw! = dStep.Width - gridBrush.Width
                                   Dim dh! = dStep.Height - gridBrush.Width
                                   Dim legendSize = region.PlotRegion.Width / 5
                                   ' 每一个方格的大小是不变的
                                   Dim blockSize As New SizeF(stdNum.Min(dw, dh), stdNum.Min(dw, dh))
                                   Dim i% = 1
                                   Dim text As New GraphicsText(DirectCast(g, Graphics2D).Graphics)
                                   Dim radius As DoubleRange = {0R, dw}
                                   Dim getRadius = Function(corr#) As Double
                                                       If variantSize Then
                                                           Return range.ScaleMapping(stdNum.Abs(corr), radius)
                                                       Else
                                                           Return dw
                                                       End If
                                                   End Function
                                   Dim r!
                                   Dim dr!
                                   Dim rawLeft! = region.PlotRegion.Left + maxLabelSize
                                   Dim top = region.Padding.Top + g.MeasureString(data.keys.First, rowLabelFont).Width
                                   Dim levels = data.PopulateRowObjects(Of DataSet).ToArray.DataScaleLevels(data.keys, -1, DrawElements.None, mapLevels)
                                   Dim llayout As New Rectangle With {
                        .Location = New Point(region.PlotRegion.Right - legendSize, region.Padding.Top),
                        .Size = New Size(legendSize, legendSize * 2)
                    }

                                   ' legend位于整个图片的右上角
                                   Call Legends.ColorMapLegend(g, llayout, colors, AxisScalling.CreateAxisTicks(data:={-1, 1}),
                                                 titleFont:=CSSFont.TryParse(legendFont),
                                                   title:=legendTitle,
                                                 tickFont:=CSSFont.TryParse(legendLabelFont),
                                                    Stroke.TryParse(Stroke.StrongHighlightStroke))


                                   ' 在这里绘制具体的矩阵
                                   For Each x As SeqValue(Of String) In data.keys.SeqIterator(offset:=1)
                                       Dim levelRow As DataSet = levels(x.value)
                                       Dim left = rawLeft

                                       ' X为矩阵之中的行数据
                                       ' 下面的循环为横向绘制出三角形的每一行的图形
                                       For Each key As String In keys
                                           Dim c# = If(x.value = key, 1, data(x.value, key))
                                           Dim labelbrush As SolidBrush = Nothing
                                           Dim gridDraw As Boolean = drawGrid
                                           Dim rect As New RectangleF With {
                                .Location = New PointF(left, top),
                                .Size = blockSize
                            }

                                           If i > x.i Then
                                               ' 上三角部分不绘制任何图形
                                               gridDraw = False
                                               ' 绘制标签
                                               If i = x.i + 1 Then
                                                   Call text.DrawString(key, rowLabelFont, Brushes.Black, rect.Location, angle:=-45)
                                               End If
                                           Else
                                               ' 得到等级
                                               Dim level% = levelRow(key)
                                               Dim index% = If(
                                    level% > colors.Length - 1,
                                    colors.Length - 1,
                                    level)
                                               ' 得到当前的方格的颜色
                                               Dim b As SolidBrush = colors(index)

                                               If drawValueLabel Then
                                                   labelbrush = Brushes.White
                                               End If

                                               r = getRadius(corr:=c)
                                               dr = (dw - r) / 2

                                               If r <> 0! Then
                                                   Call g.FillPie(b, rect.Left + dr, rect.Top + dr, r, r, 0, 360)
                                               End If
                                           End If

                                           If gridDraw Then
                                               Call g.DrawRectangle(gridBrush, rect)
                                           End If
                                           If Not labelbrush Is Nothing Then

                                               With c.ToString("F2")
                                                   Dim ksz As SizeF = g.MeasureString(.ByRef, valuelabelFont)
                                                   Dim kpos As New PointF With {
                                        .X = rect.Left + (rect.Width - ksz.Width) / 2,
                                        .Y = rect.Top + (rect.Height - ksz.Height) / 2
                                    }
                                                   Call g.DrawString(.ByRef, valuelabelFont, labelbrush, kpos)
                                               End With
                                           End If

                                           left += dw!
                                           i += 1
                                       Next

                                       left = rawLeft
                                       top += dw!
                                       i = 1

                                       Dim sz As SizeF = g.MeasureString(x.value, rowLabelFont)
                                       Dim y As Single = top - dw - (sz.Height - dw) / 2
                                       Dim lx! = rawLeft - sz.Width - stdNum.Min(dw, dh)

                                       Call g.DrawString(x.value, rowLabelFont, Brushes.Black, New PointF(lx, y))
                                   Next

                                   rawLeft -= dw / 1.5
                               End Sub

            Return g.GraphicsPlots(size.SizeParser, margin, bg, plotInternal, driver:=driver)
        End Function
    End Module

End Namespace
