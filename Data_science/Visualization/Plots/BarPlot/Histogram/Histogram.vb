#Region "Microsoft.VisualBasic::ac7e8d63daa414e9bc0152bf9b73b254, Data_science\Visualization\Plots\BarPlot\Histogram\Histogram.vb"

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

    '     Module Histogram
    ' 
    '         Function: (+2 Overloads) HistogramPlot, (+5 Overloads) Plot
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js.scale
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Math.Distributions.BinBox
Imports Microsoft.VisualBasic.Math.Scripting
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime

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
        ''' <param name="showGrid"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Plot(data As IEnumerable(Of HistogramData),
                             Optional color$ = "darkblue",
                             Optional bg$ = "white",
                             Optional size$ = "1600,1200",
                             Optional padding$ = g.DefaultPadding,
                             Optional showGrid As Boolean = True) As GraphicsData

            Return New HistogramGroup With {
                .Serials = {
                    New NamedValue(Of Color) With {
                        .Name = NameOf(data),
                        .Value = color.ToColor(Drawing.Color.Blue)
                    }
                },
                .Samples = {
                    New HistProfile With {
                        .legend = New LegendObject With {
                            .color = color,
                            .fontstyle = CSSFont.Win10Normal,
                            .style = LegendStyles.Rectangle,
                            .title = NameOf(data)
                        },
                        .data = data.ToArray
                    }
                }
            }.Plot(bg, size, padding, showGrid)
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
        ''' <param name="showGrid"></param>
        ''' <returns></returns>
        Public Function Plot(data As IEnumerable(Of Double), xrange As DoubleRange,
                             Optional color$ = "darkblue",
                             Optional bg$ = "white",
                             Optional size$ = "1600,1200",
                             Optional padding$ = g.DefaultPadding,
                             Optional showGrid As Boolean = True) As GraphicsData

            Dim hist As New HistProfile(data, xrange)
            Return Plot(hist.data, color, bg, size, padding, showGrid)
        End Function

        Public Function Plot(xrange As DoubleRange, expression As Func(Of Double, Double),
                         Optional steps# = 0.01,
                         Optional color$ = "darkblue",
                         Optional bg$ = "white",
                         Optional size$ = "1600,1200",
                         Optional padding$ = g.DefaultPadding,
                         Optional showGrid As Boolean = True) As GraphicsData
            Dim data As IEnumerable(Of Double) =
                xrange _
                .seq(steps) _
                .Select(expression)
            Return Plot(data, xrange, color, bg, size, padding, showGrid)
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
        ''' <param name="showGrid"></param>
        ''' <returns></returns>
        Public Function Plot(xrange As NamedValue(Of DoubleRange), expression$,
                             Optional steps# = 0.01,
                             Optional color$ = "darkblue",
                             Optional bg$ = "white",
                             Optional size$ = "1600,1200",
                             Optional padding$ = g.DefaultPadding,
                             Optional showGrid As Boolean = True) As GraphicsData
            Dim data As New List(Of Double)
            Dim engine As New ExpressionEngine
            Dim exp As Expression = New ExpressionTokenIcer(expression) _
                .GetTokens _
                .ToArray _
                .DoCall(AddressOf BuildExpression)

            For Each x As Double In xrange.Value.seq(steps)
                data += engine.SetSymbol(xrange.Name, x#).Evaluate(exp)
            Next

            Return Plot(data, xrange.Value, color, bg, size, padding, showGrid)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="groups"></param>
        ''' <param name="bg$"></param>
        ''' <param name="size"></param>
        ''' <param name="showGrid"></param>
        ''' <param name="legendPos">The legend position on the output image.</param>
        ''' <param name="legendBorder"></param>
        ''' <param name="alpha">Fill color alpha value, [0, 255]</param>
        ''' <returns></returns>
        <Extension>
        Public Function Plot(groups As HistogramGroup,
                             Optional bg$ = "white",
                             Optional size$ = "1600,1200",
                             Optional padding$ = g.DefaultPadding,
                             Optional showGrid As Boolean = True,
                             Optional legendPos As Point = Nothing,
                             Optional legendBorder As Stroke = Nothing,
                             Optional showLegend As Boolean = True,
                             Optional alpha% = 255,
                             Optional drawRect As Boolean = True,
                             Optional showTagChartLayer As Boolean = False,
                             Optional xlabel$ = "X",
                             Optional Ylabel$ = "Y",
                             Optional axisLabelFontStyle$ = CSSFont.Win7LargerBold,
                             Optional xAxis$ = Nothing,
                             Optional title$ = Nothing,
                             Optional titleCss$ = CSSFont.PlotTitle) As GraphicsData

            Dim margin As Padding = padding
            Dim plotInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)
                    If groups.Samples.Length = 1 AndAlso groups.Samples.First.data.Length = 0 Then
                        Call "No content data for plot histogram chart...".Warning
                        Return
                    End If

                    Dim scalerData As New Scaling(groups, False)
                    Dim annotations As Dictionary(Of NamedValue(Of Color)) = groups.Serials.ToDictionary
                    Dim gSize As Size = region.Size
                    Dim X, Y As d3js.scale.LinearScale
                    Dim XTicks#() = groups.XRange.CreateAxisTicks
                    Dim YTicks#() = groups.YRange.CreateAxisTicks

                    With region.PlotRegion
                        If Not xAxis.StringEmpty Then
                            XTicks = AxisProvider.TryParse(xAxis).AxisTicks
                            X = XTicks.LinearScale.range(integers:={ .Left, .Right})
                        Else
                            X = d3js.scale.linear _
                                .domain(XTicks) _
                                .range(integers:={ .Left, .Right})
                        End If

                        ' Y 为什么是从零开始的？
                        Y = d3js.scale.linear _
                            .domain(YTicks) _
                            .range(integers:={ .Bottom, .Top})
                    End With

                    Dim scaler As New DataScaler With {
                        .X = X,
                        .Y = Y,
                        .region = region.PlotRegion,
                        .AxisTicks = (XTicks, YTicks)
                    }

                    Call g.DrawAxis(
                        region, scaler, showGrid, xlabel:=xlabel, ylabel:=Ylabel,
                        htmlLabel:=False)

                    If Not title.StringEmpty Then
                        Dim titleFont As Font = CSSFont.TryParse(titleCss)
                        Dim titleSize As SizeF = g.MeasureString(title, titleFont)
                        Dim titlePos As New PointF With {
                            .X = region.PlotRegion.Left + (region.PlotRegion.Width - titleSize.Width) / 2,
                            .Y = region.PlotRegion.Top - titleSize.Height * 1.125
                        }

                        Call g.DrawString(title, titleFont, Brushes.Black, titlePos)
                    End If

                    For Each hist As HistProfile In groups.Samples
                        Dim ann As NamedValue(Of Color) = annotations(hist.legend.title)
                        Dim b As New SolidBrush(Color.FromArgb(alpha, ann.Value))

                        For Each block As HistogramData In hist.data
                            Dim pos As PointF = scaler.Translate(block.x1, block.y)
                            Dim sizeF As New SizeF With {
                                .Width = scaler.TranslateX(block.x2) - scaler.TranslateX(block.x1),
                                .Height = region.PlotRegion.Bottom - scaler.TranslateY(block.y)
                            }
                            Dim rect As New RectangleF With {
                                .Location = pos,
                                .Size = sizeF
                            }

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

                        Dim chart As GraphicsData = Scatter.Plot(
                            serials,
                            size:=size,
                            padding:=margin,
                            bg:="transparent",
                            showGrid:=False,
                            showLegend:=False,
                            drawAxis:=False)

                        ' 合并图层
                        Call g.DrawImageUnscaled(chart, New Rectangle(New Point, gSize))
                    End If

                    If showLegend Then
                        If legendPos.IsEmpty Then
                            legendPos = New Point With {
                                .X = CInt(gSize.Width * 0.7),
                                .Y = margin.Top
                            }
                        End If

                        Call g.DrawLegends(
                            topLeft:=legendPos,
                            legends:=groups.Samples.Select(Function(h) h.legend),
                            regionBorder:=legendBorder
                        )
                    End If
                End Sub

            Return g.GraphicsPlots(size.SizeParser, margin, bg$, plotInternal)
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
        ''' <param name="showGrid"></param>
        ''' <returns></returns>
        <Extension>
        Public Function HistogramPlot(data As IEnumerable(Of Double),
                                      Optional step! = 1,
                                      Optional serialsTitle$ = "histogram plot",
                                      Optional color$ = "lightblue",
                                      Optional bg$ = "white",
                                      Optional size$ = "1600,1200",
                                      Optional padding$ = DefaultPadding,
                                      Optional showGrid As Boolean = True,
                                      Optional ByRef histData As HistogramData() = Nothing,
                                      Optional xLabel$ = "X",
                                      Optional yLabel$ = "Y",
                                      Optional xAxis$ = Nothing,
                                      Optional showLegend As Boolean = True) As GraphicsData
            Return data.ToArray _
                .Hist([step]) _
                .HistogramPlot([step]:=[step],
                               serialsTitle:=serialsTitle,
                               color:=color,
                               bg:=bg,
                               size:=size,
                               padding:=padding,
                               showGrid:=showGrid,
                               histData:=histData,
                               xLabel:=xLabel,
                               yLabel:=yLabel,
                               xAxis:=xAxis,
                               showLegend:=showLegend
                )
        End Function

        ''' <summary>
        ''' 绘制频数
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="step">The step width value for create the input <paramref name="data"/> bin box.</param>
        ''' <param name="serialsTitle$"></param>
        ''' <param name="color$"></param>
        ''' <param name="bg$"></param>
        ''' <param name="size"></param>
        ''' <param name="showGrid"></param>
        ''' <returns></returns>
        <Extension>
        Public Function HistogramPlot(data As IEnumerable(Of DataBinBox(Of Double)),
                                      Optional step! = 1,
                                      Optional serialsTitle$ = "histogram plot",
                                      Optional color$ = "lightblue",
                                      Optional bg$ = "white",
                                      Optional size$ = "1600,1200",
                                      Optional padding$ = DefaultPadding,
                                      Optional showGrid As Boolean = True,
                                      Optional ByRef histData As HistogramData() = Nothing,
                                      Optional xLabel$ = "X",
                                      Optional yLabel$ = "Y",
                                      Optional xAxis$ = Nothing,
                                      Optional showLegend As Boolean = True) As GraphicsData
            Dim histLegend As New LegendObject With {
                .color = color,
                .fontstyle = CSSFont.Win7LargerBold,
                .style = LegendStyles.Rectangle,
                .title = serialsTitle
            }
            Dim s As HistProfile = data.NewModel([step], histLegend)
            Dim group As New HistogramGroup With {
                .Samples = {s},
                .Serials = {s.SerialData}
            }

            histData = s.data

            Return group.Plot(
                bg:=bg, padding:=padding, size:=size,
                showGrid:=showGrid,
                showTagChartLayer:=False,
                xlabel:=xLabel, Ylabel:=yLabel,
                xAxis:=xAxis,
                showLegend:=showLegend
            )
        End Function
    End Module
End Namespace
