#Region "Microsoft.VisualBasic::86a35c77cc7e1bb5cf07ec9069a458ed, Data_science\Visualization\Plots\BarPlot\AlignmentPlot.vb"

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

    '   Total Lines: 230
    '    Code Lines: 188 (81.74%)
    ' Comment Lines: 23 (10.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 19 (8.26%)
    '     File Size: 11.81 KB


    '     Module AlignmentPlot
    ' 
    '         Function: createHits, Keys, PlotAlignment, PlotAlignmentGroups, Values
    '         Structure Signal
    ' 
    '             Function: ToString
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports signals = System.ValueTuple(Of Double, Double)
Imports stdNum = System.Math

Namespace BarPlot

    ''' <summary>
    ''' Visualize and comparing two discrete signals.(以条形图的方式可视化绘制两个离散的信号的比对的图形)
    ''' </summary>
    Public Module AlignmentPlot

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Keys(signals As signals()) As Double()
            Return signals.Select(Function(t) t.Item1).ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Friend Function Values(signals As signals()) As Double()
            Return signals.Select(Function(t) t.Item2).ToArray
        End Function

        ''' <summary>
        ''' 以条形图的方式可视化绘制两个离散的信号的比对的图形
        ''' </summary>
        ''' <param name="query">The query signals</param>
        ''' <param name="subject">The subject signal values</param>
        ''' <param name="cla$">Color expression for <paramref name="query"/></param>
        ''' <param name="clb$">Color expression for <paramref name="subject"/></param>
        ''' <param name="displayX">是否在信号的柱子上面显示出X坐标的信息</param>
        ''' <param name="htmlLabel">Draw axis label using html render?? default is no.</param>
        ''' <returns></returns>
        <Extension>
        Public Function PlotAlignment(query As (X#, value#)(), subject As (X#, value#)(),
                                      Optional xrange As DoubleRange = Nothing,
                                      Optional yrange As DoubleRange = Nothing,
                                      Optional size$ = "1200,800",
                                      Optional padding$ = "padding: 70 30 50 100;",
                                      Optional bg$ = "white",
                                      Optional cla$ = "steelblue",
                                      Optional clb$ = "brown",
                                      Optional xlab$ = "X",
                                      Optional ylab$ = "Y",
                                      Optional labelCSS$ = CSSFont.Win7Bold,
                                      Optional queryName$ = "query",
                                      Optional subjectName$ = "subject",
                                      Optional title$ = "Alignments Plot",
                                      Optional tickCSS$ = CSSFont.Win7Normal,
                                      Optional titleCSS$ = CSSFont.Win10NormalLarger,
                                      Optional legendFontCSS$ = CSSFont.Win10Normal,
                                      Optional bw! = 8,
                                      Optional format$ = "F2",
                                      Optional displayX As Boolean = True,
                                      Optional X_CSS$ = CSSFont.Win10Normal,
                                      Optional yAxislabelPosition As YlabelPosition = YlabelPosition.InsidePlot,
                                      Optional labelPlotStrength# = 0.25,
                                      Optional htmlLabel As Boolean = False,
                                      Optional idTag$ = Nothing,
                                      Optional rectangleStyle As RectangleStyling = Nothing,
                                      Optional drawLegend As Boolean = True,
                                      Optional drawGrid As Boolean = True,
                                      Optional tagXFormat$ = "F2",
                                      Optional legendLayout As String = "top-right",
                                      Optional driver As Drivers = Drivers.Default) As GraphicsData

            Dim q As New Signal With {
                .Name = queryName,
                .Color = cla,
                .signals = query
            }
            Dim s As New Signal With {
                .Name = subjectName,
                .Color = clb,
                .signals = subject
            }

            Return PlotAlignmentGroups(
                {q}, {s},
                xrange:=xrange, yrange:=yrange,
                size:=size, padding:=padding, bg:=bg,
                xlab, ylab, labelCSS, queryName, subjectName,
                title, tickCSS, titleCSS,
                legendFontCSS, bw, format, displayX, X_CSS,
                yAxislabelPosition,
                labelPlotStrength,
                idTag:=idTag,
                rectangleStyle:=rectangleStyle,
                drawLegend:=drawLegend,
                drawGrid:=drawGrid,
                tagXFormat:=tagXFormat,
                driver:=driver,
                legendLayout:=legendLayout
            )
        End Function

        Public Structure Signal
            Dim Name$
            Dim Color$
            Dim signals As signals()

            Public Overrides Function ToString() As String
                Return Name & $" ({Color})"
            End Function
        End Structure

        ''' <summary>
        ''' 以条形图的方式可视化绘制两个离散的信号的比对的图形，由于绘制的时候是分别对<paramref name="query"/>和<paramref name="subject"/>
        ''' 信号数据使用For循环进行绘图的，所以数组最后一个位置的元素会在最上层
        ''' 并且绘制图例的时候，使用的是最上层的信号的颜色
        ''' </summary>
        ''' <param name="query">The query signals</param>
        ''' <param name="subject">The subject signal values</param>
        ''' <param name="displayX">是否在信号的柱子上面显示出X坐标的信息</param>
        ''' <param name="format">the number format of the X axis ticks</param>
        ''' <returns></returns>
        <Extension>
        Public Function PlotAlignmentGroups(query As Signal(), subject As Signal(),
                                            Optional xrange As DoubleRange = Nothing,
                                            Optional yrange As DoubleRange = Nothing,
                                            Optional size$ = "1200,800",
                                            Optional padding$ = "padding: 70 30 50 100;",
                                            Optional bg$ = "white",
                                            Optional xlab$ = "X",
                                            Optional ylab$ = "Y",
                                            Optional labelCSS$ = CSSFont.Win7Bold,
                                            Optional queryName$ = "query",
                                            Optional subjectName$ = "subject",
                                            Optional title$ = "Alignments Plot",
                                            Optional tickCSS$ = CSSFont.Win7Normal,
                                            Optional titleCSS$ = CSSFont.Win10NormalLarger,
                                            Optional legendFontCSS$ = CSSFont.Win10Normal,
                                            Optional bw! = 8,
                                            Optional format$ = "F2",
                                            Optional displayX As Boolean = True,
                                            Optional X_CSS$ = CSSFont.Win10Normal,
                                            Optional yAxislabelPosition As YlabelPosition = YlabelPosition.InsidePlot,
                                            Optional labelPlotStrength# = 0.25,
                                            Optional hitsHightLights As Double() = Nothing,
                                            Optional xError# = 0.5,
                                            Optional highlight$ = Stroke.StrongHighlightStroke,
                                            Optional highlightMargin! = 2,
                                            Optional legendLayout As String = "top-right",
                                            Optional idTag$ = Nothing,
                                            Optional rectangleStyle As RectangleStyling = Nothing,
                                            Optional drawLegend As Boolean = True,
                                            Optional drawGrid As Boolean = True,
                                            Optional tagXFormat$ = "F2",
                                            Optional driver As Drivers = Drivers.Default) As GraphicsData

            Dim theme As New Theme With {
                .padding = padding,
                .axisTickCSS = tickCSS,
                .mainCSS = titleCSS,
                .legendLabelCSS = legendFontCSS,
                .background = bg,
                .axisLabelCSS = labelCSS,
                .yAxislabelPosition = yAxislabelPosition,
                .legendTickFormat = format,
                .XaxisTickFormat = format,
                .YaxisTickFormat = format,
                .tagFormat = tagXFormat,
                .lineStroke = highlight,
                .drawLegend = drawLegend,
                .drawGrid = drawGrid
            }
            Dim barplot As New PlotAlignmentGroup(query, subject, xrange, yrange, rectangleStyle, theme) With {
                .main = title,
                .xlabel = xlab,
                .ylabel = ylab,
                .XAxisLabelCss = X_CSS,
                .displayX = displayX,
                .queryName = queryName,
                .subjectName = subjectName,
                .highlightMargin = highlightMargin,
                .hitsHightLights = hitsHightLights,
                .labelPlotStrength = labelPlotStrength,
                .idTag = idTag,
                .bw = bw,
                .xError = xError,
                .legendLayout = legendLayout
            }

            Return barplot.Plot(size, ppi:=100, driver:=driver)
        End Function

        <Extension>
        Friend Function createHits(data As Signal(), ishighlight As Func(Of Double, (err#, x#, yes As Boolean))) As Dictionary(Of Double, (x As List(Of Double), y#))
            Dim hits As New Dictionary(Of Double, (x As List(Of Double), y#))
            Dim source As IEnumerable(Of signals) = data _
                .Select(Function(x) x.signals) _
                .IteratesALL

            For Each o As (x#, y#) In source
                Dim hit = ishighlight(o.x)

                If hit.yes Then
                    If Not hits.ContainsKey(hit.x) Then
                        hits(hit.x) = (New List(Of Double), -100)
                    End If

                    Dim value = hits(hit.x)
                    value.x.Add(o.x)

                    If value.y < o.y Then
                        value = (value.x, o.y)
                    End If

                    hits(hit.x) = value
                End If
            Next

            Return hits
        End Function
    End Module
End Namespace
