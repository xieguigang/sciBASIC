#Region "Microsoft.VisualBasic::4a7d636707ca415d2f82faeb5e9a1eae, Data_science\Visualization\Plots\Scatter\Scatter.vb"

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

    '   Total Lines: 614
    '    Code Lines: 507 (82.57%)
    ' Comment Lines: 65 (10.59%)
    '    - Xml Docs: 90.77%
    ' 
    '   Blank Lines: 42 (6.84%)
    '     File Size: 27.48 KB


    ' Module Scatter
    ' 
    '     Function: CreateAxisTicks, (+2 Overloads) FromPoints, FromVector, getSplinePoints, Jitter
    '               MakeJitter, (+5 Overloads) Plot, PlotFunction
    ' 
    '     Sub: Plot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Shapes
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Interpolation
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports randf2 = Microsoft.VisualBasic.Math.RandomExtensions
Imports Microsoft.VisualBasic.MIME.Html.Render


#If NET48 Then
Imports Pen = System.Drawing.Pen
Imports Pens = System.Drawing.Pens
Imports Brush = System.Drawing.Brush
Imports Font = System.Drawing.Font
Imports Brushes = System.Drawing.Brushes
Imports SolidBrush = System.Drawing.SolidBrush
Imports DashStyle = System.Drawing.Drawing2D.DashStyle
Imports Image = System.Drawing.Image
Imports Bitmap = System.Drawing.Bitmap
#Else
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports Pens = Microsoft.VisualBasic.Imaging.Pens
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Brushes = Microsoft.VisualBasic.Imaging.Brushes
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
#End If

Public Module Scatter

    ''' <summary>
    ''' A jitter function to randomly assign the x-axis 
    ''' positions for each x-parameter
    ''' </summary>
    ''' <param name="a"></param>
    ''' <param name="width_jit"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' this method can be affected by the <see cref="randf2.SetSeed(Integer)"/> method.
    ''' </remarks>
    Public Function Jitter(a As Vector, width_jit As Double) As Vector
        a += Vector.rand(a.Length) * width_jit - width_jit / 2
        Return a
    End Function

    Public Iterator Function MakeJitter(groups As IEnumerable(Of NamedValue(Of Vector)),
                                        width_jit As Double,
                                        Optional dodge_gap As Double = 1) As IEnumerable(Of NamedValue(Of Vector))
        For Each grp In groups
            Dim grpl As Double = grp.Value.Length
            Dim uniq As Double = Val(grp.Description)
            Dim x As Vector = (grp.Value - 0.25 * dodge_gap) + 0.5 * dodge_gap * (grpl - 1) / (If(uniq = 0.0, grpl, uniq) - 1)

            x = Jitter(x, width_jit)

            Yield New NamedValue(Of Vector) With {
                .Name = grp.Name,
                .Value = x
            }
        Next
    End Function

    <Extension>
    Public Function CreateAxisTicks(array As SerialData(),
                                    Optional preferPositive As Boolean = False,
                                    Optional scaleX# = 1.2,
                                    Optional scaleY# = 1.2) As (x As Double(), y As Double())
        Dim ptX#() = array _
            .Select(Function(s)
                        Return s.pts.Select(Function(pt) CDbl(pt.pt.X))
                    End Function) _
           .IteratesALL _
           .ToArray
        Dim w_steps# = If(scaleX <> 1.0, 0.8, 0.2)
        Dim w_bound# = If(scaleX <> 1.0, 0.1, 0.4)
        Dim XTicks = ptX _
           .Range(scaleX) _
           .CreateAxisTicks(w_steps:=w_steps, w_max:=w_bound, w_min:=w_bound)
        Dim ptY#() = array _
            .Select(Function(s)
                        Return s.pts _
                            .Select(Function(pt)
                                        Return {
                                            pt.pt.Y - pt.errMinus,
                                            pt.pt.Y + pt.errPlus
                                        }
                                    End Function)
                    End Function) _
            .IteratesALL _
            .IteratesALL _
            .ToArray

        w_steps# = If(scaleY <> 1.0, 0.8, 0.2)
        w_bound# = If(scaleY <> 1.0, 0.1, 0.4)

        Dim YTicks = ptY _
            .Range(scaleY) _
            .CreateAxisTicks(w_steps:=w_steps, w_max:=w_bound, w_min:=w_bound)

        If preferPositive AndAlso Not ptX.Any(Function(n) n < 0) Then
            ' 全部都是正实数，则将可能的负实数去掉
            '
            ' 因为在下面的Range函数里面，是根据scale来将最大值加上一个delta值，最小值减去一个delta值来得到scale之后的结果，
            ' 所以假若X有比较接近于零的值得花， scale之后会出现负数
            ' 这个负数很明显是不合理的，所以在这里将负数删除掉
            With ptX.Range(scaleX)
                XTicks = New DoubleRange(0, .Max).CreateAxisTicks
            End With
        End If

        Return (XTicks, YTicks)
    End Function

    <Extension>
    Public Function getSplinePoints(raw As PointData(), spline As Splines) As PointData()
        Select Case spline
            Case Splines.None
                Return raw
            Case Splines.B_Spline
                Dim pointdata As PointF() = raw _
                    .Select(Function(p) p.pt) _
                    .BSpline(degree:=2) _
                    .ToArray
                Dim interplot As PointData() = pointdata _
                    .Select(Function(p)
                                Return New PointData With {
                                    .pt = p
                                }
                            End Function) _
                    .OrderBy(Function(p) p.pt.X) _
                    .ToArray

                Return interplot
            Case Else
                Throw New NotImplementedException(spline.ToString)
        End Select
    End Function

    ''' <summary>
    ''' Scatter plot function.(绘图函数，默认的输出大小为``4300px,2000px``)
    ''' </summary>
    ''' <param name="c"></param>
    ''' <param name="bg"></param>
    ''' <param name="fill">是否对曲线下的区域进行填充？这个参数只有在<paramref name="drawLine"/>开启的情况下才会发生作用</param>
    ''' <param name="drawLine">
    ''' 是否绘制两个点之间的连接线段，当这个参数为False的时候，将不会绘制连线，就相当于绘制散点图了，而非折线图
    ''' </param>
    ''' <param name="legendSize">默认为(120,40)</param>
    ''' <param name="preferPositive"><see cref="CreateAxisTicks"/></param>
    ''' <param name="hullConvexList">
    ''' a list of <see cref="SerialData.title"/> for draw hull convex polygon.
    ''' </param>
    ''' <param name="interplot">
    ''' 是否对线条或者多边形数据进行插值圆滑处理
    ''' </param>
    <Extension>
    Public Sub Plot(c As IEnumerable(Of SerialData), g As IGraphics, rect As GraphicsRegion,
                    Optional bg$ = "white",
                    Optional showGrid As Boolean = True,
                    Optional showLegend As Boolean = True,
                    Optional legendPosition As Point = Nothing,
                    Optional legendSize$ = "100,50",
                    Optional drawLine As Boolean = True,
                    Optional legendBorder As Stroke = Nothing,
                    Optional legendRegionBorder As Stroke = Nothing,
                    Optional fill As Boolean = False,
                    Optional fillPie As Boolean = True,
                    Optional legendFontCSS$ = CSSFont.PlotSubTitle,
                    Optional absoluteScaling As Boolean = True,
                    Optional XaxisAbsoluteScalling As Boolean = False,
                    Optional YaxisAbsoluteScalling As Boolean = False,
                    Optional drawAxis As Boolean = True,
                    Optional Xlabel$ = "X",
                    Optional Ylabel$ = "Y",
                    Optional ylim As Double() = Nothing,
                    Optional xlim As Double() = Nothing,
                    Optional ablines As Line() = Nothing,
                    Optional htmlLabel As Boolean = False,
                    Optional ticksY# = -1,
                    Optional preferPositive As Boolean = False,
                    Optional interplot As Splines = Splines.None,
                    Optional densityColor As Boolean = False,
                    Optional tickFontStyle$ = CSSFont.Win7LargeBold,
                    Optional labelFontStyle$ = CSSFont.Win7VeryLarge,
                    Optional title$ = Nothing,
                    Optional titleFontCSS$ = CSSFont.Win7VeryVeryLarge,
                    Optional xlayout As XAxisLayoutStyles = XAxisLayoutStyles.Bottom,
                    Optional ylayout As YAxisLayoutStyles = YAxisLayoutStyles.Left,
                    Optional gridFill$ = "rgb(245,245,245)",
                    Optional gridColor$ = "white",
                    Optional legendBgFill As String = Nothing,
                    Optional legendSplit% = -1,
                    Optional hullConvexList As String() = Nothing,
                    Optional XtickFormat$ = "F2",
                    Optional YtickFormat$ = "F2",
                    Optional axisStroke$ = Stroke.AxisStroke,
                    Optional axisLabelCSS$ = CSSFont.Win10Normal,
                    Optional scatterReorder As Boolean = False,
                    Optional xAxisLabelRotate As Double = 0)

        Dim theme As New Theme With {
            .drawLegend = showLegend,
            .XaxisTickFormat = XtickFormat,
            .drawGrid = showGrid,
            .gridFill = gridFill,
            .background = bg,
            .axisStroke = axisStroke,
            .drawAxis = drawAxis,
            .axisLabelCSS = axisLabelCSS,
            .mainCSS = titleFontCSS,
            .xAxisLayout = xlayout,
            .yAxisLayout = ylayout,
            .legendBoxStroke = legendRegionBorder?.ToString,
            .axisTickCSS = tickFontStyle,
            .legendLabelCSS = legendFontCSS,
            .legendSplitSize = legendSplit,
            .YaxisTickFormat = YtickFormat,
            .xAxisRotate = xAxisLabelRotate
        }
        Dim plot As Plot

        If drawLine Then
            plot = New Plots.LinePlot2D(data:=c, theme:=theme, fill:=fill, interplot:=interplot) With {
                .xlabel = Xlabel,
                .ylabel = Ylabel,
                .main = title,
                .xlim = xlim,
                .ylim = ylim
            }
        Else
            plot = New Plots.Scatter2D(
                data:=c,
                theme:=theme,
                scatterReorder:=scatterReorder,
                fillPie:=fillPie,
                ablines:=ablines,
                hullConvexList:=hullConvexList
            ) With {
                .xlabel = Xlabel,
                .ylabel = Ylabel,
                .xlim = xlim,
                .ylim = ylim,
                .XaxisAbsoluteScalling = XaxisAbsoluteScalling,
                .YaxisAbsoluteScalling = YaxisAbsoluteScalling,
                .main = title
            }
        End If

        Call plot.Plot(g, rect.PlotRegion(g.LoadEnvironment))
    End Sub

    ''' <summary>
    ''' Scatter plot function.(绘图函数，默认的输出大小为``4300px,2000px``)
    ''' </summary>
    ''' <param name="c"></param>
    ''' <param name="size"></param>
    ''' <param name="bg"></param>
    ''' <param name="fill">是否对曲线下的区域进行填充？这个参数只有在<paramref name="drawLine"/>开启的情况下才会发生作用</param>
    ''' <param name="drawLine">
    ''' 是否绘制两个点之间的连接线段，当这个参数为False的时候，将不会绘制连线，就相当于绘制散点图了，而非折线图
    ''' </param>
    ''' <param name="legendSize">默认为(120,40)</param>
    ''' <param name="preferPositive"><see cref="CreateAxisTicks"/></param>
    ''' <param name="hullConvexList">
    ''' a list of <see cref="SerialData.title"/> for draw hull convex polygon.
    ''' </param>
    ''' <param name="interplot">
    ''' 是否对线条或者多边形数据进行插值圆滑处理
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function Plot(c As IEnumerable(Of SerialData),
                         Optional size$ = "3600,2700",
                         Optional padding$ = g.DefaultUltraLargePadding,
                         Optional bg$ = "white",
                         Optional showGrid As Boolean = True,
                         Optional showLegend As Boolean = True,
                         Optional legendPosition As Point = Nothing,
                         Optional legendSize$ = "100,50",
                         Optional drawLine As Boolean = True,
                         Optional legendBorder As Stroke = Nothing,
                         Optional legendRegionBorder As Stroke = Nothing,
                         Optional fill As Boolean = False,
                         Optional fillPie As Boolean = True,
                         Optional legendFontCSS$ = CSSFont.PlotSubTitle,
                         Optional absoluteScaling As Boolean = True,
                         Optional XaxisAbsoluteScalling As Boolean = False,
                         Optional YaxisAbsoluteScalling As Boolean = False,
                         Optional drawAxis As Boolean = True,
                         Optional Xlabel$ = "X",
                         Optional Ylabel$ = "Y",
                         Optional ylim As Double() = Nothing,
                         Optional xlim As Double() = Nothing,
                         Optional ablines As Line() = Nothing,
                         Optional htmlLabel As Boolean = False,
                         Optional ticksY# = -1,
                         Optional preferPositive As Boolean = False,
                         Optional interplot As Splines = Splines.None,
                         Optional densityColor As Boolean = False,
                         Optional tickFontStyle$ = CSSFont.Win7LargeBold,
                         Optional labelFontStyle$ = CSSFont.Win7VeryLarge,
                         Optional title$ = Nothing,
                         Optional titleFontCSS$ = CSSFont.Win7VeryVeryLarge,
                         Optional xlayout As XAxisLayoutStyles = XAxisLayoutStyles.Bottom,
                         Optional ylayout As YAxisLayoutStyles = YAxisLayoutStyles.Left,
                         Optional gridFill$ = "rgb(245,245,245)",
                         Optional gridColor$ = "white",
                         Optional legendBgFill As String = Nothing,
                         Optional legendSplit% = -1,
                         Optional hullConvexList As String() = Nothing,
                         Optional XtickFormat$ = "F2",
                         Optional YtickFormat$ = "F2",
                         Optional axisStroke$ = Stroke.AxisStroke,
                         Optional axisLabelCSS$ = CSSFont.Win10Normal,
                         Optional scatterReorder As Boolean = False,
                         Optional dpi As Integer = 100,
                         Optional driver As Drivers = Drivers.Default) As GraphicsData

        Dim plotInternal =
            Sub(ByRef g As IGraphics, layout As GraphicsRegion)
                Call c.Plot(
                    g:=g,
                    rect:=layout,
                    bg:=bg,
                    showGrid:=showGrid,
                    showLegend:=showLegend,
                    legendPosition:=legendPosition,
                    legendSize:=legendSize,
                    drawLine:=drawLine,
                    legendBorder:=legendBorder,
                    legendRegionBorder:=legendRegionBorder,
                    fill:=fill,
                    fillPie:=fillPie,
                    legendFontCSS:=legendFontCSS,
                    absoluteScaling:=absoluteScaling,
                    xlim:=xlim,
                    XaxisAbsoluteScalling:=XaxisAbsoluteScalling,
                    ylim:=ylim,
                    YaxisAbsoluteScalling:=YaxisAbsoluteScalling,
                    drawAxis:=drawAxis,
                    xlayout:=xlayout,
                    ylayout:=ylayout,
                    Xlabel:=Xlabel,
                    Ylabel:=Ylabel,
                    ablines:=ablines,
                    htmlLabel:=htmlLabel,
                    ticksY:=ticksY,
                    preferPositive:=preferPositive,
                    interplot:=interplot,
                    densityColor:=densityColor,
                    tickFontStyle:=tickFontStyle,
                    labelFontStyle:=labelFontStyle,
                    title:=title,
                    titleFontCSS:=titleFontCSS,
                    gridColor:=gridColor,
                    gridFill:=gridFill,
                    legendSplit:=legendSplit,
                    legendBgFill:=legendBgFill,
                    hullConvexList:=hullConvexList,
                    XtickFormat:=XtickFormat,
                    YtickFormat:=YtickFormat,
                    axisStroke:=axisStroke,
                    scatterReorder:=scatterReorder,
                    axisLabelCSS:=axisLabelCSS
                )
            End Sub

        Return g.GraphicsPlots(
            size:=size.SizeParser,
            padding:=padding,
            bg:=bg,
            plotAPI:=plotInternal,
            dpi:=dpi,
            driver:=driver
        )
    End Function

    Public Function Plot(x As Vector,
                         Optional size$ = "1600,1200",
                         Optional padding$ = g.DefaultPadding,
                         Optional bg As String = "white",
                         Optional ptSize As Single = 15,
                         Optional width As Single = 5,
                         Optional drawLine As Boolean = False) As GraphicsData
        Return {
            FromVector(x,,, ptSize, width)
        }.Plot(size, padding, bg, True, False, , drawLine:=drawLine)
    End Function

    Public Function FromVector(y As IEnumerable(Of Double),
                               Optional color As String = "black",
                               Optional dash As DashStyle = DashStyle.Dash,
                               Optional ptSize! = 30,
                               Optional width As Single = 5,
                               Optional xrange As IEnumerable(Of Double) = Nothing,
                               Optional title$ = "Vector Plot",
                               Optional alpha% = 255) As SerialData
        Dim array#()
        Dim y0#() = y.ToArray

        If xrange Is Nothing Then
            array = Math.seq(0, y0.Length, 1)
        Else
            array = xrange.ToArray
        End If

        Return New SerialData With {
            .color = color.ToColor.Alpha(alpha),
            .lineType = dash,
            .pointSize = ptSize,
            .title = title,
            .width = width,
            .pts = LinqAPI.Exec(Of PointData) <=
                                                _
                From o As SeqValue(Of Double)
                In y0.SeqIterator
                Where Not o.value.IsNaNImaginary
                Select New PointData With {
                    .pt = New PointF(array(o.i), CSng(o.value))
                }
                    }
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="range">这个参数之中的Name属性是为了设置表达式计算之中的变量的目的</param>
    ''' <param name="expression$"></param>
    ''' <param name="steps#"></param>
    ''' <param name="lineColor$"></param>
    ''' <param name="lineWidth!"></param>
    ''' <param name="bg$"></param>
    ''' <param name="yline#">
    ''' Combine with y line for visualize the numeric solve of the equation.
    ''' </param>
    ''' <param name="ylineColor$"></param>
    ''' <returns></returns>
    <Extension>
    Public Function PlotFunction(range As NamedValue(Of DoubleRange),
                                 expression$,
                                 Optional steps# = 0.01,
                                 Optional lineColor$ = "black",
                                 Optional lineWidth! = 10,
                                 Optional bg$ = "white",
                                 Optional variables As Dictionary(Of String, String) = Nothing,
                                 Optional yline# = Double.NaN,
                                 Optional ylineColor$ = "red") As GraphicsData

        Dim engine As New ExpressionEngine
        Dim ranges As Double() = range.Value.seq(steps).ToArray
        Dim y As New List(Of Double)
        Dim exp As Expression = New ExpressionTokenIcer(expression) _
            .GetTokens _
            .ToArray _
            .DoCall(AddressOf BuildExpression)

        If Not variables.IsNullOrEmpty Then
            For Each var In variables
                Call engine.SetSymbol(var.Key, var.Value)
            Next
        End If

        For Each x As Double In ranges
            y += engine.SetSymbol(range.Name, x).Evaluate(exp)
        Next

        Dim serial As SerialData = FromVector(y, lineColor,,, lineWidth, ranges, expression,)

        If Double.IsNaN(yline) Then
            Return Plot({serial}, ,, bg)
        Else
            Dim syline As New SerialData With {
                .color = ylineColor.ToColor,
                .pointSize = 3,
                .width = 3,
                .title = $"y={yline}",
                .pts = {
                    New PointData With {.pt = New PointF(range.Value.Min, yline)},
                    New PointData With {.pt = New PointF(range.Value.Max, yline)}
                }
            }
            Return Plot({syline, serial}, ,, bg)
        End If
    End Function

    <Extension>
    Public Function Plot(range As DoubleRange,
                         expression As Func(Of Double, Double),
                         Optional steps# = 0.01,
                         Optional lineColor$ = "black",
                         Optional lineWidth! = 10,
                         Optional bg$ = "white",
                         Optional title$ = "Function Plot") As GraphicsData

        Dim ranges As Double() = range.seq(steps).ToArray
        Dim y As New List(Of Double)

        For Each x As Double In ranges
            y += expression(x#)
        Next

        Dim serial As SerialData = FromVector(y, lineColor,,, lineWidth, ranges, title,)
        Return Plot({serial}, ,, bg)
    End Function

    Public Function Plot(points As IEnumerable(Of Point),
                         Optional size As Size = Nothing,
                         Optional padding$ = g.DefaultPadding,
                         Optional lineColor$ = "black",
                         Optional bg$ = "white",
                         Optional title$ = "Plot Of Points",
                         Optional lineWidth! = 5.0!,
                         Optional ptSize! = 15.0!,
                         Optional lineType As DashStyle = DashStyle.Solid) As GraphicsData
        Dim s As SerialData = points _
            .FromPoints(lineColor$,
                        title$,
                        lineWidth!,
                        ptSize!,
                        lineType)
        Return Bubble.Plot({s}, size:=$"{size.Width},{size.Height}", padding:=padding, bg:=bg)
    End Function

    <Extension>
    Public Function Plot(points As IEnumerable(Of PointF),
                         Optional size As Size = Nothing,
                         Optional padding$ = g.DefaultPadding,
                         Optional lineColor$ = "black",
                         Optional bg$ = "white",
                         Optional title$ = "Plot Of Points",
                         Optional lineWidth! = 5.0!,
                         Optional ptSize! = 15.0!,
                         Optional lineType As DashStyle = DashStyle.Solid,
                         Optional gridFill$ = "rgb(250,250,250)",
                         Optional driver As Drivers = Drivers.Default) As GraphicsData

        Dim s As SerialData = points.FromPoints(
            lineColor:=lineColor$,
            title:=title$,
            lineWidth:=lineWidth!,
            ptSize:=ptSize!,
            lineType:=lineType
        )

        Return Bubble.Plot({s}, size:=$"{size.Width},{size.Height}", padding:=padding, bg:=bg, gridFill:=gridFill, driver:=driver)
    End Function

    <Extension>
    Public Function FromPoints(points As IEnumerable(Of Point),
                               Optional lineColor$ = "black",
                               Optional title$ = "Plot Of Points",
                               Optional lineWidth! = 5.0!,
                               Optional ptSize! = 15.0!,
                               Optional lineType As DashStyle = DashStyle.Solid) As SerialData
        Return FromPoints(
            points.Select(
            Function(pt) New PointF With {
                .X = pt.X,
                .Y = pt.Y
            }),
            lineColor,
            title,
            lineWidth,
            ptSize,
            lineType)
    End Function

    <Extension>
    Public Function FromPoints(points As IEnumerable(Of PointF),
                               Optional lineColor$ = "black",
                               Optional title$ = "Plot Of Points",
                               Optional lineWidth! = 5.0!,
                               Optional ptSize! = 15.0!,
                               Optional lineType As DashStyle = DashStyle.Solid) As SerialData

        Return New SerialData With {
            .color = lineColor.TranslateColor,
            .lineType = lineType,
            .pointSize = ptSize,
            .width = lineWidth,
            .pts = points _
                .Select(Function(pt)
                            Return New PointData With {
                                .pt = pt
                            }
                        End Function) _
                .ToArray,
            .title = title
        }
    End Function
End Module
