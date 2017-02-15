#Region "Microsoft.VisualBasic::e2fcc6f580c8bb43bd7659f2cd5c8f7b, ..\sciBASIC#\Data_science\Mathematical\Plots\Histogram.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Vector.Shapes
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.Scripting
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace BarPlot.Histogram

    ''' <summary>
    ''' 对经由函数生成的连续数据的图形表述
    ''' </summary>
    Public Module Histogram

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="color$">histogram bar fill color</param>
        ''' <param name="bg$">Output image background color</param>
        ''' <param name="size"></param>
        ''' <param name="margin"></param>
        ''' <param name="showGrid"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Plot(data As IEnumerable(Of HistogramData),
                         Optional color$ = "darkblue",
                         Optional bg$ = "white",
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional showGrid As Boolean = True) As Bitmap

            Return New HistogramGroup With {
                .Serials = {
                New NamedValue(Of Color) With {
                    .Name = NameOf(data),
                    .Value = color.ToColor(Drawing.Color.Blue)
                }
            },
            .Samples = {
                New HistProfile With {
                    .legend = New Legend With {
                        .color = color,
                        .fontstyle = CSSFont.Win10Normal,
                        .style = LegendStyles.Rectangle,
                        .title = NameOf(data)
                    },
                    .data = data.ToArray
                }
            }
        }.Plot(bg, size, margin, showGrid)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data">
        ''' 向量之中的每一个数值表示某个指定的区间内的数据，即直方图的高度
        ''' </param>
        ''' <param name="xrange"></param>
        ''' <param name="color$"></param>
        ''' <param name="bg$"></param>
        ''' <param name="size"></param>
        ''' <param name="margin"></param>
        ''' <param name="showGrid"></param>
        ''' <returns></returns>
        Public Function Plot(data As IEnumerable(Of Double), xrange As DoubleRange,
                             Optional color$ = "darkblue",
                             Optional bg$ = "white",
                             Optional size As Size = Nothing,
                             Optional margin As Size = Nothing,
                             Optional showGrid As Boolean = True) As Bitmap

            Dim hist As New HistProfile(data, xrange)
            Return Plot(hist.data, color, bg, size, margin, showGrid)
        End Function

        Public Function Plot(xrange As DoubleRange, expression As Func(Of Double, Double),
                         Optional steps# = 0.01,
                         Optional color$ = "darkblue",
                         Optional bg$ = "white",
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional showGrid As Boolean = True) As Bitmap
            Dim data As IEnumerable(Of Double) =
            xrange _
            .seq(steps) _
            .Select(expression)
            Return Plot(data, xrange, color, bg, size, margin, showGrid)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="xrange">For generates the variable value sequence for evaluate the <paramref name="expression"/></param>
        ''' <param name="expression$">Math expression in string format</param>
        ''' <param name="steps#">for <see cref="seq"/> function</param>
        ''' <param name="color$">The histogram bar fill color</param>
        ''' <param name="bg$"></param>
        ''' <param name="size"></param>
        ''' <param name="margin"></param>
        ''' <param name="showGrid"></param>
        ''' <returns></returns>
        Public Function Plot(xrange As NamedValue(Of DoubleRange), expression$,
                         Optional steps# = 0.01,
                         Optional color$ = "darkblue",
                         Optional bg$ = "white",
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional showGrid As Boolean = True) As Bitmap
            Dim data As New List(Of Double)
            Dim engine As New Expression

            For Each x# In xrange.Value.seq(steps)
                Call engine.SetVariable(xrange.Name, x#)
                data += engine.Evaluation(expression$)
            Next

            Return Plot(data, xrange.Value, color, bg, size, margin, showGrid)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="groups"></param>
        ''' <param name="bg$"></param>
        ''' <param name="size"></param>
        ''' <param name="margin"></param>
        ''' <param name="showGrid"></param>
        ''' <param name="legendPos">The legend position on the output image.</param>
        ''' <param name="legendBorder"></param>
        ''' <param name="alpha">Fill color alpha value, [0, 255]</param>
        ''' <returns></returns>
        <Extension>
        Public Function Plot(groups As HistogramGroup,
                             Optional bg$ = "white",
                             Optional size As Size = Nothing,
                             Optional margin As Size = Nothing,
                             Optional showGrid As Boolean = True,
                             Optional legendPos As Point = Nothing,
                             Optional legendBorder As Border = Nothing,
                             Optional alpha% = 255,
                             Optional drawRect As Boolean = True,
                             Optional showTagChartLayer As Boolean = False) As Bitmap

            Return GraphicsPlots(
                size, margin,
                bg$,
                Sub(ByRef g, region)
                    Dim mapper As New Scaling(groups, False)  ' 这里也不是使用y值来表示数量的，也用相对值
                    Dim annotations = groups.Serials.ToDictionary

                    Call g.DrawAxis(size, margin, mapper, showGrid)

                    For Each hist As HistProfile In mapper.ForEach_histSample(size, margin)
                        Dim ann As NamedValue(Of Color) = annotations(hist.legend.title)
                        Dim b As New SolidBrush(Color.FromArgb(alpha, ann.Value))

                        For Each block As HistogramData In hist.data
                            Dim rect As New RectangleF(
                                New PointF(block.x1, block.y),
                                New SizeF(block.width, region.PlotRegion.Bottom - block.y))

                            Call g.FillRectangle(b, rect)

                            If drawRect Then
                                Call g.DrawRectangle(
                                    Pens.Black,
                                    rect.Left, rect.Top,
                                    rect.Width, rect.Height)
                            End If
                        Next
                    Next

                    If showTagChartLayer Then
                        Dim serials As New List(Of SerialData)

                        For Each hist As SeqValue(Of HistProfile) In groups.Samples.SeqIterator
                            serials += (+hist).GetLine(groups.Serials(hist).Value, 2, 5)
                        Next

                        Dim chart As Bitmap = Scatter.Plot(
                            serials,
                            size:=size,
                            margin:=margin,
                            bg:="transparent",
                            showGrid:=False,
                            showLegend:=False,
                            drawAxis:=False)
                    End If

                    If legendPos.IsEmpty Then
                        legendPos = New Point(
                            CInt(size.Width * 0.8),
                            margin.Height)
                    End If

                    Call g.DrawLegends(
                        legendPos,
                        groups.Samples _
                            .Select(Function(x) x.legend),
                        ,,
                        legendBorder)
                End Sub)
        End Function

        ''' <summary>
        ''' 绘制频数
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="step!"></param>
        ''' <param name="serialsTitle$"></param>
        ''' <param name="color$"></param>
        ''' <param name="bg$"></param>
        ''' <param name="size"></param>
        ''' <param name="margin"></param>
        ''' <param name="showGrid"></param>
        ''' <returns></returns>
        <Extension>
        Public Function HistogramPlot(data As IEnumerable(Of Double),
                                      Optional step! = 1,
                                      Optional serialsTitle$ = "histogram plot",
                                      Optional color$ = "lightblue",
                                      Optional bg$ = "white",
                                      Optional size As Size = Nothing,
                                      Optional margin As Size = Nothing,
                                      Optional showGrid As Boolean = True,
                                      Optional ByRef histData As IntegerTagged(Of Double)() = Nothing,
                                      Optional showAverageCurve As Boolean = True) As Bitmap

            With data.ToArray.Hist([step])

                Dim histLegend As New Legend With {
                    .color = color,
                    .fontstyle = CSSFont.Win7LargerBold,
                    .style = LegendStyles.Rectangle,
                    .title = serialsTitle
                }

                Dim s As HistProfile = .NewModel([step], histLegend)
                Dim group As New HistogramGroup With {
                    .Samples = {s},
                    .Serials = {s.SerialData}
                }

                histData = .Values.ToArray

                Return group.Plot(
                    bg:=bg, margin:=margin, size:=size,
                    showGrid:=showGrid,
                    showTagChartLayer:=showAverageCurve)
            End With
        End Function
    End Module
End Namespace