#Region "Microsoft.VisualBasic::01185c2ae5b47e9818982345c83543d0, Data_science\Visualization\Plots\Contour\HeatMap\Utils.vb"

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

'   Total Lines: 310
'    Code Lines: 203 (65.48%)
' Comment Lines: 86 (27.74%)
'    - Xml Docs: 95.35%
' 
'   Blank Lines: 21 (6.77%)
'     File Size: 13.63 KB


'     Module Utils
' 
'         Function: __getData, Compile, CreatePlot, (+3 Overloads) Plot
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl
Imports Microsoft.VisualBasic.MIME.Html.CSS

Namespace Contour.HeatMap

    Public Module Utils

        ''' <summary>
        ''' Compile the math expression as a lambda expression for producing numeric values.
        ''' </summary>
        ''' <param name="exp$">A string math function expression: ``f(x,y)``</param>
        ''' <returns></returns>
        Public Function Compile(exp As String) As Func(Of Double, Double, Double)
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
                    matrix:=matrix
                )
            Catch ex As Exception
                Throw New Exception(exp, ex)
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
                             Optional size$ = "3600,2700",
                             Optional padding$ = "padding: 100px 900px 250px 300px;",
                             Optional unit% = 5,
                             Optional legendTitle$ = "Contour Heatmap",
                             Optional legendFont$ = CSSFont.Win7LittleLarge,
                             Optional xsteps! = Single.NaN,
                             Optional ysteps! = Single.NaN,
                             Optional parallel As Boolean = False,
                             Optional ByRef matrix As List(Of DataSet) = Nothing,
                             Optional minZ# = Double.MinValue,
                             Optional maxZ# = Double.MaxValue,
                             Optional xlabel$ = "X",
                             Optional ylabel$ = "Y",
                             Optional logbase# = -1.0R,
                             Optional tickFont$ = CSSFont.Win7Normal) As GraphicsData

            Dim theme As New Theme With {
                .padding = padding,
                .axisTickCSS = tickFont,
                .legendLabelCSS = legendFont,
                .colorSet = colorMap,
                .background = bg
            }
            Dim plotInternal As New ContourHeatMapPlot(theme) With {
                .xrange = xrange,
                .yrange = yrange,
                .parallel = parallel,
                .xsteps = xsteps,
                .ysteps = ysteps,
                .legendTitle = legendTitle,
                .mapLevels = mapLevels,
                .matrix = New FormulaEvaluate With {.formula = fun},
                .unit = unit,
                .xlabel = xlabel,
                .ylabel = ylabel,
                .logBase = logbase,
                .maxZ = maxZ,
                .minZ = minZ
            }

            Return plotInternal.Plot(size)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="matrix">[x => [y, z]]</param>
        ''' <param name="colorMap$"></param>
        ''' <param name="mapLevels%"></param>
        ''' <param name="bg$"></param>
        ''' <param name="padding$"></param>
        ''' <param name="unit%"></param>
        ''' <param name="legendTitle$"></param>
        ''' <param name="legendFont$"></param>
        ''' <param name="tickFont$"></param>
        ''' <param name="xlabel$"></param>
        ''' <param name="ylabel$"></param>
        ''' <param name="minZ#"></param>
        ''' <param name="maxZ#"></param>
        ''' <returns></returns>
        Public Function CreatePlot(Of DataSet As {INamedValue, DynamicPropertyBase(Of Double)})(matrix As IEnumerable(Of DataSet),
                                   Optional colorMap$ = "Spectral:c10",
                                   Optional mapLevels% = 25,
                                   Optional bg$ = "white",
                                   Optional padding$ = "padding: 100 400 100 400;",
                                   Optional unit% = 5,
                                   Optional legendTitle$ = "Scatter Heatmap",
                                   Optional legendFont$ = CSSFont.Win10NormalLarge,
                                   Optional tickFont$ = CSSFont.Win7Normal,
                                   Optional xlabel$ = "X",
                                   Optional ylabel$ = "Y",
                                   Optional minZ# = Double.MinValue,
                                   Optional maxZ# = Double.MaxValue,
                                   Optional legendTickFormat$ = "F2",
                                   Optional xsteps! = 1,
                                   Optional ysteps! = 1) As ContourHeatMapPlot

            Dim margin As Padding = padding
            Dim theme As New Theme With {
                .colorSet = colorMap,
                .background = bg,
                .legendLabelCSS = legendFont,
                .axisTickCSS = tickFont,
                .padding = padding,
                .legendTickFormat = legendTickFormat
            }
            Dim matrixData As DataSet() = matrix.ToArray
            Dim xrange As DoubleRange = matrixData.Select(Function(d) Val(d.Key)).ToArray
            Dim yrange As DoubleRange = matrixData.Select(Function(a) a.Properties.Keys).IteratesALL.Distinct.Select(Function(a) Val(a)).ToArray

            Return New ContourHeatMapPlot(theme) With {
                .legendTitle = legendTitle,
                .mapLevels = mapLevels,
                .matrix = New MatrixEvaluate(Of DataSet)(matrixData, New SizeF(unit, unit)),
                .xlabel = xlabel,
                .ylabel = ylabel,
                .minZ = minZ,
                .maxZ = maxZ,
                .unit = 5,
                .xrange = xrange,
                .yrange = yrange,
                .xsteps = xsteps,
                .ysteps = ysteps
            }
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
        Public Function Plot(Of DataSet As {INamedValue, DynamicPropertyBase(Of Double)})(matrix As IEnumerable(Of DataSet),
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

            Return CreatePlot(
                matrix:=matrix,
                colorMap:=colorMap,
                mapLevels:=mapLevels,
                bg:=bg,
                padding:=padding,
                unit:=unit,
                legendTitle:=legendTitle,
                legendFont:=legendFont,
                tickFont:=tickFont,
                xlabel:=xlabel,
                ylabel:=ylabel,
                minZ:=minZ,
                maxZ:=maxZ
            ).Plot(size)
        End Function

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
        Friend Function __getData(fun As EvaluatePoints,
                                  size As Size,
                                  xrange As DoubleRange,
                                  yrange As DoubleRange,
                                  ByRef xsteps!,
                                  ByRef ysteps!,
                                  parallel As Boolean,
                                  ByRef matrix As List(Of Scatter3DPoint), unit%) As (X#, y#, z#)()

            xsteps = xsteps Or (xrange.Length / size.Width).AsDefault(Function(n) Single.IsNaN(CSng(n)))
            ysteps = ysteps Or (yrange.Length / size.Height).AsDefault(Function(n) Single.IsNaN(CSng(n)))

            ' x: a -> b
            ' 每一行数据都是y在发生变化
            Dim data As (X#, y#, Z#)()() = DataProvider.Evaluate(
                f:=AddressOf fun.Evaluate,
                x:=xrange,
                y:=yrange,
                xsteps:=xsteps,
                ysteps:=ysteps,
                parallel:=parallel,
                matrix:=matrix
            ).ToArray

            Return data.ToVector
        End Function
    End Module
End Namespace
