#Region "Microsoft.VisualBasic::f4148b2af1444a7894c729f14af017ba, Data_science\Visualization\Plots\BarPlot\Histogram\Histogram.vb"

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

    '   Total Lines: 302
    '    Code Lines: 214 (70.86%)
    ' Comment Lines: 69 (22.85%)
    '    - Xml Docs: 94.20%
    ' 
    '   Blank Lines: 19 (6.29%)
    '     File Size: 14.08 KB


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
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Distributions
Imports Microsoft.VisualBasic.Math.Distributions.BinBox
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl
Imports Microsoft.VisualBasic.MIME.Html.CSS

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
                             Optional titleCss$ = CSSFont.PlotTitle,
                             Optional xlabelRotate As Double = 0,
                             Optional xTickFormat As String = "F2",
                             Optional yTickFormat As String = "F0",
                             Optional dpi As Integer = 100) As GraphicsData

            Dim theme As New Theme With {
                .padding = padding,
                .background = bg,
                .axisLabelCSS = axisLabelFontStyle,
                .mainCSS = titleCss,
                .drawLegend = showLegend,
                .legendBoxStroke = If(legendBorder Is Nothing, Nothing, legendBorder.ToString),
                .drawGrid = showGrid,
                .XaxisTickFormat = xTickFormat,
                .YaxisTickFormat = yTickFormat,
                .xAxisRotate = xlabelRotate
            }
            Dim app As New HistogramPlot(groups, alpha, drawRect, theme) With {
                .xlabel = xlabel,
                .ylabel = Ylabel,
                .main = title,
                .xAxis = xAxis,
                .showTagChartLayer = showTagChartLayer
            }

            If Not legendPos.IsEmpty Then
                theme.legendLayout = New Absolute(legendPos)
            End If

            Return app.Plot(size, ppi:=dpi)
        End Function

        ''' <summary>
        ''' 绘制频数
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="step">
        ''' The step width value for create the input <paramref name="data"/> bin box.
        ''' </param>
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
                                      Optional xlabelRotate As Double = 0,
                                      Optional xTickFormat As String = "F2",
                                      Optional yTickFormat As String = "F0",
                                      Optional showLegend As Boolean = True,
                                      Optional dpi As Integer = 100) As GraphicsData
            Return data.ToArray _
                .Hist([step]) _
                .HistogramPlot(serialsTitle:=serialsTitle,
                               color:=color,
                               bg:=bg,
                               size:=size,
                               padding:=padding,
                               showGrid:=showGrid,
                               histData:=histData,
                               xLabel:=xLabel,
                               yLabel:=yLabel,
                               xAxis:=xAxis,
                               showLegend:=showLegend,
                               dpi:=dpi,
                               xTickFormat:=xTickFormat,
                               yTickFormat:=yTickFormat,
                               xlabelRotate:=xlabelRotate
                )
        End Function

        ''' <summary>
        ''' 绘制频数
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="serialsTitle$"></param>
        ''' <param name="color$"></param>
        ''' <param name="bg$"></param>
        ''' <param name="size"></param>
        ''' <param name="showGrid"></param>
        ''' <returns></returns>
        <Extension>
        Public Function HistogramPlot(data As IEnumerable(Of DataBinBox(Of Double)),
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
                                      Optional showLegend As Boolean = True,
                                      Optional xlabelRotate As Double = 0,
                                      Optional xTickFormat As String = "F2",
                                      Optional yTickFormat As String = "F0",
                                      Optional dpi As Integer = 100) As GraphicsData

            Dim histLegend As New LegendObject With {
                .color = color,
                .fontstyle = CSSFont.Win7LargerBold,
                .style = LegendStyles.Rectangle,
                .title = serialsTitle
            }
            Dim s As HistProfile = data.NewModel(histLegend)
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
                showLegend:=showLegend,
                dpi:=dpi,
                xTickFormat:=xTickFormat,
                yTickFormat:=yTickFormat,
                xlabelRotate:=xlabelRotate
            )
        End Function
    End Module
End Namespace
