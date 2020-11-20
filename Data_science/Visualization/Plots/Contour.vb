#Region "Microsoft.VisualBasic::c37806b8d7d87d4700cd53b534953125, Data_science\Visualization\Plots\Contour.vb"

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

    ' Module Contour
    ' 
    '     Function: __getData, Compile, (+3 Overloads) Plot
    '     Class __plotHelper
    ' 
    '         Function: GetColor, GetData
    ' 
    '         Sub: Plot
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports stdNum = System.Math

''' <summary>
''' Contour heatmap 
''' 
''' ###### 等高线图
''' 
''' 和普通的heatmap相比，这里的坐标轴是连续的数值变量，而普通的heatmap，其坐标轴都是离散的分类变量
''' </summary>
Public Module Contour

    ''' <summary>
    ''' Compile the math expression as a lambda expression for producing numeric values.
    ''' </summary>
    ''' <param name="exp$">A string math function expression: ``f(x,y)``</param>
    ''' <returns></returns>
    Public Function Compile(exp$) As Func(Of Double, Double, Double)
        With New ExpressionEngine()

            !x = 0
            !y = 0

            Dim func As Expression = New ExpressionTokenIcer(exp) _
                .GetTokens _
                .ToArray _
                .DoCall(AddressOf BuildExpression)

            Return Function(x, y)
                       !x = x
                       !y = y

                       Return .DoCall(AddressOf func.Evaluate)
                   End Function
        End With
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="exp$"></param>
    ''' <param name="xrange"></param>
    ''' <param name="yrange"></param>
    ''' <param name="colorMap$"></param>
    ''' <param name="mapLevels%"></param>
    ''' <param name="bg$"></param>
    ''' <param name="size"></param>
    ''' <param name="legendTitle$"></param>
    ''' <param name="legendFont"></param>
    ''' <param name="xsteps!"></param>
    ''' <param name="ysteps!"></param>
    ''' <param name="matrix">
    ''' 请注意：假若想要获取得到原始的矩阵数据，这个列表对象还需要实例化之后再传递进来，否则仍然会返回空集合
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function Plot(exp$, xrange As DoubleRange, yrange As DoubleRange,
                         Optional colorMap$ = "Spectral:c10",
                         Optional mapLevels% = 25,
                         Optional bg$ = "white",
                         Optional size$ = "3000,2700",
                         Optional padding$ = "padding: 100 400 100 400;",
                         Optional unit% = 5,
                         Optional legendTitle$ = "",
                         Optional legendFont$ = CSSFont.Win7Large,
                         Optional xsteps! = Single.NaN,
                         Optional ysteps! = Single.NaN,
                         Optional ByRef matrix As List(Of DataSet) = Nothing) As GraphicsData

        Dim fun As Func(Of Double, Double, Double) = Compile(exp)

        Try
            Return fun.Plot(
                xrange, yrange,
                colorMap,
                mapLevels,
                bg, size, padding,
                unit,
                legendTitle, legendFont,
                xsteps, ysteps,
                matrix:=matrix)
        Catch ex As Exception
            ex = New Exception(exp, ex)
            Throw ex
        End Try
    End Function

    ''' <summary>
    ''' steps步长值默认值为长度平分到每一个像素点
    ''' </summary>
    ''' <param name="fun"></param>
    ''' <param name="xrange"></param>
    ''' <param name="yrange"></param>
    ''' <param name="colorMap$">
    ''' Default using colorbrewer ``Spectral:c10`` schema.
    ''' </param>
    ''' <param name="size">3000, 2400</param>
    ''' <param name="xsteps!"></param>
    ''' <param name="matrix">
    ''' 请注意：假若想要获取得到原始的矩阵数据，这个列表对象还需要实例化之后再传递进来，否则仍然会返回空集合
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function Plot(fun As Func(Of Double, Double, Double),
                         xrange As DoubleRange,
                         yrange As DoubleRange,
                         Optional colorMap$ = "Spectral:c10",
                         Optional mapLevels% = 25,
                         Optional bg$ = "white",
                         Optional size$ = "3000,2700",
                         Optional padding$ = "padding: 100 400 100 400",
                         Optional unit% = 5,
                         Optional legendTitle$ = "Scatter Heatmap",
                         Optional legendFont$ = CSSFont.Win7Large,
                         Optional xsteps! = Single.NaN,
                         Optional ysteps! = Single.NaN,
                         Optional parallel As Boolean = False,
                         Optional ByRef matrix As List(Of DataSet) = Nothing,
                         Optional minZ# = Double.MinValue,
                         Optional maxZ# = Double.MaxValue,
                         Optional xlabel$ = "X",
                         Optional ylabel$ = "Y",
                         Optional logbase# = -1.0R,
                         Optional scale# = 1.0#,
                         Optional tickFont$ = CSSFont.Win7Normal) As GraphicsData

        Dim plotInternal As New __plotHelper With {
            .func = fun,
            .offset = New Point(-300, 0),
            .xrange = xrange,
            .yrange = yrange,
            .parallel = parallel,
            .xsteps = xsteps,
            .ysteps = ysteps,
            .colorMap = colorMap,
            .legendFont = CSSFont.TryParse(legendFont),
            .legendTitle = legendTitle,
            .mapLevels = mapLevels,
            .matrix = matrix,
            .unit = unit,
            .xlabel = xlabel,
            .ylabel = ylabel,
            .logBase = logbase,
            .maxZ = maxZ,
            .minZ = minZ,
            .scale = scale,
            .tickFont = CSSFont.TryParse(tickFont)
        }

        Return GraphicsPlots(
            size.SizeParser, padding,
            bg$,
            AddressOf plotInternal.Plot)
    End Function

    ''' <summary>
    ''' 从现有的矩阵数据之中绘制等高线图
    ''' </summary>
    ''' <param name="matrix"></param>
    ''' <param name="colorMap$"></param>
    ''' <param name="mapLevels%"></param>
    ''' <param name="bg$"></param>
    ''' <param name="size$"></param>
    ''' <param name="padding$"></param>
    ''' <param name="legendTitle$"></param>
    ''' <param name="legendFont"></param>
    ''' <param name="xlabel$"></param>
    ''' <param name="ylabel$"></param>
    ''' <param name="minZ#"></param>
    ''' <param name="maxZ#"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Plot(matrix As IEnumerable(Of DataSet),
                         Optional colorMap$ = "Spectral:c10",
                         Optional mapLevels% = 25,
                         Optional bg$ = "white",
                         Optional size$ = "3000,2500",
                         Optional padding$ = "padding: 100 400 100 400;",
                         Optional unit% = 5,
                         Optional legendTitle$ = "Scatter Heatmap",
                         Optional legendFont$ = CSSFont.Win10NormalLarge,
                         Optional tickFont$ = CSSFont.Win7Normal,
                         Optional xlabel$ = "X",
                         Optional ylabel$ = "Y",
                         Optional minZ# = Double.MinValue,
                         Optional maxZ# = Double.MaxValue) As GraphicsData

        Dim margin As Padding = padding

        Return GraphicsPlots(
           size.SizeParser,
           margin,
           bg$, AddressOf New __plotHelper With {
                .offset = New Point(-300, 0),
                .colorMap = colorMap,
                .legendFont = CSSFont.TryParse(legendFont),
                .legendTitle = legendTitle,
                .mapLevels = mapLevels,
                .matrix = matrix.AsList,
                .xlabel = xlabel,
                .ylabel = ylabel,
                .minZ = minZ,
                .maxZ = maxZ,
                .unit = unit,
                .tickFont = CSSFont.TryParse(tickFont)
           }.Plot)
    End Function

    ''' <summary>
    ''' 因为ByRef参数不能够再lambda表达式之中进行使用，所以在这里必须要使用一个helper对象来读取原始的矩阵数据
    ''' </summary>
    Private Class __plotHelper

        Public offset As Point
        Public func As Func(Of Double, Double, Double)
        Public xrange As DoubleRange, yrange As DoubleRange
        Public xsteps!, ysteps!
        Public parallel As Boolean
        Public legendFont As Font, legendTitle$
        Public mapLevels%, colorMap$
        Public matrix As List(Of DataSet)
        Public unit%
        Public xlabel$, ylabel$
        Public logBase#
        Public minZ, maxZ As Double
        Public scale# = 1
        Public tickFont As Font

        Public Function GetData(plotSize As Size) As (x#, y#, z#)()
            If func Is Nothing Then
                ' 直接返回矩阵数据
                Return LinqAPI.Exec(Of (x#, y#, Z#)) _
                    () <= From line As DataSet
                          In matrix
                          Let xi = Val(line.ID)
                          Let data = line.Properties.Select(Function(o) (X:=xi, Y:=Val(o.Key), Z:=o.Value))
                          Select data
            Else

                Return func _
                    .__getData(plotSize,  ' 得到通过计算返回来的数据
                               xrange, yrange,
                               xsteps, ysteps,
                               parallel, matrix,
                               unit)
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="colorReturns">从这里返回绘制legend所需要的数据</param>
        ''' <returns></returns>
        Public Function GetColor(data As Double(), ByRef colorReturns As SolidBrush()) As Func(Of String, SolidBrush)
            Dim reals As SeqValue(Of Double)()
            Dim indexLevels%()

            If logBase > 0 Then
                reals = data _
                    .SeqIterator _
                    .Where(Function(x) Not (+x).IsNaNImaginary AndAlso (+x) <> 0R) _
                    .ToArray
                indexLevels = reals _
                    .Select(Function(x) stdNum.Abs(+x)) _
                    .Log2Ranks(mapLevels)
            Else
                reals = data _
                    .SeqIterator _
                    .Where(Function(x) (Not (+x).IsNaNImaginary) AndAlso x.value >= minZ AndAlso x.value <= maxZ) _
                    .ToArray
                indexLevels = reals _
                    .Select(Function(x) x.value) _
                    .GenerateMapping(mapLevels)
            End If

            Dim realIndexs As Dictionary(Of String, Integer) = reals _
                .SeqIterator _
                .ToDictionary(Function(index) index.value.i.ToString,
                              Function(level) indexLevels(level.i))
            Dim colors As SolidBrush() =
                Designer.GetBrushes(colorMap, mapLevels)

            colorReturns = colors

            Return Function(index$)
                       If realIndexs.ContainsKey(index) Then
                           Dim level = realIndexs(index) - 1

                           If level < 0 Then
                               level = 0
                           ElseIf level >= colors.Length Then
                               level = colors.Length - 1
                           End If

                           Return colors(level)
                       Else
                           Return Brushes.Gray
                       End If
                   End Function
        End Function

        Public Sub Plot(ByRef g As IGraphics, region As GraphicsRegion)
            Dim data = GetData(region.PlotRegion.Size)
            Dim xTicks = data.Select(Function(d) d.x).Range.CreateAxisTicks
            Dim yTicks = data.Select(Function(d) d.y).Range.CreateAxisTicks
            Dim x = d3js.scale.linear() _
                .domain(xTicks) _
                .range(integers:={region.PlotRegion.Left, region.PlotRegion.Right})
            Dim y = d3js.scale.linear() _
                .domain(yTicks) _
                .range(integers:={region.PlotRegion.Top, region.PlotRegion.Bottom})
            Dim colorDatas As SolidBrush() = Nothing
            Dim getColors = GetColor(data.Select(Function(o) o.z).ToArray, colorDatas)
            Dim size As Size = region.Size
            Dim margin = region.Padding
            Dim plotWidth! = region.PlotRegion.Width
            Dim plotHeight! = region.PlotRegion.Height
            ' 图例位于右边，占1/5的绘图区域的宽度，高度为绘图区域的高度的2/3
            Dim legendLayout As New Rectangle With {
                .Width = plotWidth / 5,
                .Height = plotHeight * (1 / 3),
                .X = region.Width - margin.Right / 2 - .Width,
                .Y = margin.Top + (plotHeight - .Height) / 2
            }

            ' Call g.DrawAxis(size, margin, scaler, False, offset, xlabel, ylabel)

            offset = New Point With {
                .X = offset.X,
                .Y = offset.Y - unit / 2
            }

            Dim us% = unit * scale

            For i As Integer = 0 To data.Length - 1
                Dim p As (X#, y#, Z#) = data(i)
                Dim c As SolidBrush = getColors(i)
                Dim fill As New RectangleF With {
                    .X = x(p.X) + offset.X,
                    .Y = y(p.y) + offset.Y,
                    .Width = us,
                    .Height = us
                }

                Call g.FillRectangle(c, fill)
                Call g.DrawRectangle(New Pen(c),
                                     fill.Left, fill.Top,
                                     fill.Width,
                                     fill.Height)
            Next

            ' Draw legends
            Dim realData#() = data _
                .Select(Function(o) o.z) _
                .Where(Function(z) (Not z.IsNaNImaginary) AndAlso
                    z >= minZ AndAlso
                    z <= maxZ) _
                .ToArray
            Dim rangeTicks#() = realData.Range.CreateAxisTicks

            Call g.ColorMapLegend(
                legendLayout, colorDatas, rangeTicks,
                legendFont, legendTitle, tickFont,
                New Pen(Color.Black, 2),
                NameOf(Color.Gray))
        End Sub
    End Class

    ''' <summary>
    ''' 返回去的数据是和<paramref name="size"/>每一个像素点相对应的
    ''' </summary>
    ''' <param name="fun"></param>
    ''' <param name="size"></param>
    ''' <param name="xrange"></param>
    ''' <param name="yrange"></param>
    ''' <param name="xsteps!"></param>
    ''' <param name="ysteps!"></param>
    ''' <param name="parallel">
    ''' 对于例如ODEs计算这类比较重度的计算，可以考虑在这里使用并行
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Private Function __getData(fun As Func(Of Double, Double, Double),
                               size As Size,
                               xrange As DoubleRange,
                               yrange As DoubleRange,
                               ByRef xsteps!,
                               ByRef ysteps!,
                               parallel As Boolean,
                               ByRef matrix As List(Of DataSet), unit%) As (X#, y#, z#)()


        xsteps = xsteps Or (xrange.Length / size.Width).AsDefault(Function(n) Single.IsNaN(CSng(n)))
        ysteps = ysteps Or (yrange.Length / size.Height).AsDefault(Function(n) Single.IsNaN(CSng(n)))
        xsteps *= unit%
        ysteps *= unit%

        ' x: a -> b
        ' 每一行数据都是y在发生变化
        Dim data As (X#, y#, Z#)()() =
            DataProvider.Evaluate(
                fun, xrange, yrange,
                xsteps, ysteps,
                parallel, matrix).ToArray

        If data.Length > size.Width + 10 Then
            Dim stepDelta = data.Length / size.Width
            Dim splt = data.Split(stepDelta)

        Else ' 数据不足


        End If

        Return data.ToVector
    End Function
End Module
