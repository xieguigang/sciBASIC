#Region "Microsoft.VisualBasic::5dae9f36534d4ca05764e35eb48fb3d7, Data_science\Visualization\Plots-statistics\Heatmap\DensityPlot.vb"

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

    '   Total Lines: 208
    '    Code Lines: 172 (82.69%)
    ' Comment Lines: 22 (10.58%)
    '    - Xml Docs: 81.82%
    ' 
    '   Blank Lines: 14 (6.73%)
    '     File Size: 9.52 KB


    '     Module DensityPlot
    ' 
    '         Function: DensityMatrix, Plot
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.GraphTheory.GridGraph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Shapes
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Driver.CSS
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Scripting
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Heatmap

    ''' <summary>
    ''' 类似于<see cref="Contour"/>，但是这个是基于点的密度来绘图的
    ''' </summary>
    Public Module DensityPlot

        Public Const DriverName$ = "scatter.density.plot"

        ''' <summary>
        ''' Similar to the <see cref="Contour"/> plot but plot in scatter mode.
        ''' </summary>
        ''' <param name="points"></param>
        ''' <param name="size$"></param>
        ''' <param name="padding$"></param>
        ''' <param name="bg$"></param>
        ''' <param name="schema$"></param>
        ''' <returns></returns>
        <Driver(DriverName)>
        Public Function Plot(points As IEnumerable(Of PointF),
                             <GlobalCSSSelector(Types.Size)> Optional size$ = "1600,1200",
                             <GlobalCSSSelector(Types.Padding)> Optional padding$ = g.DefaultPadding,
                             <GlobalCSSSelector(Types.Brush)> Optional bg$ = "white",
                             Optional schema$ = "Jet",
                             Optional levels% = 20,
                             Optional steps$ = Nothing,
                             <CSSSelector(Types.Float)> Optional ptSize! = 5,
                             <CSSSelector(Types.Integer)> Optional legendWidth% = 150,
                             <CSSSelector(Types.Font)> Optional legendTitleFontCSS$ = CSSFont.Win7LargerNormal,
                             <CSSSelector(Types.Font)> Optional legendTickFontCSS$ = CSSFont.Win7Normal,
                             <CSSSelector(Types.Stroke)> Optional legendTickStrokeCSS$ = Stroke.AxisStroke,
                             Optional ablines As Line() = Nothing,
                             Optional labX$ = "X",
                             Optional labY$ = "Y",
                             <CSSSelector(Types.Font)> Optional labelFontCSS$ = CSSFont.Win10Normal,
                             Optional htmlLabel As Boolean = True,
                             Optional xMax# = Double.NaN,
                             Optional xMin# = Double.NaN,
                             Optional yMin# = Double.NaN) As GraphicsData

            Dim pointVector As IVector(Of PointF) = points _
                .Where(Function(pt)
                           Return Not New Double() {
                               pt.X, pt.Y
                           }.Any(Function(x)
                                     Return x.IsNaNImaginary
                                 End Function)
                       End Function) _
                .Shadows
            Dim xrange As DoubleRange = pointVector!X
            Dim yrange As DoubleRange = pointVector!Y
            Dim colors$() = Designer _
                .GetColors(schema, levels) _
                .Select(Function(c) c.ToHtmlColor) _
                .ToArray

            If Not xMax.IsNaNImaginary Then
                xrange = {xrange.Min, xMax}
            End If
            If Not xMin.IsNaNImaginary Then
                xrange = {xMin, xrange.Max}
            End If
            If Not yMin.IsNaNImaginary Then
                yrange = {yMin, yrange.Max}
            End If

            Dim density = (xrange, yrange) _
                .Grid(steps.FloatSizeParser) _
                .DensityMatrix(
                    pointVector,
                    schema:=colors,
                    r:=ptSize
                )
            Dim scatterPadding As Padding = padding

            scatterPadding.Right += legendWidth

            Using g As IGraphics = Scatter.Plot(
                c:={density},
                size:=size, padding:=scatterPadding, bg:=bg,
                drawLine:=False,
                showLegend:=False,
                fillPie:=True,
                ablines:=ablines,
                Xlabel:=labX,
                Ylabel:=labY,
                xlim:=xrange.Max,
                ylim:=yrange.Max,
                htmlLabel:=htmlLabel,
                labelFontStyle:=CSSFont.Win7VeryLarge,
                tickFontStyle:=CSSFont.Win7Large).CreateGraphics

                ' 在这里还需要绘制颜色谱的legend
                ' 计算出legend的layout信息
                ' 竖直样式的legend：右边居中，宽度为legendwidth，高度则是plotregion的高度的2/3
                Dim plotRegion As New GraphicsRegion With {
                    .Size = g.Size,
                    .Padding = scatterPadding
                }
                Dim scatterRegion As Rectangle = plotRegion.PlotRegion
                Dim legendHeight! = scatterRegion.Height * 2 / 3
                Dim legendLayout As New Rectangle With {
                    .Size = New Size With {
                        .Width = legendWidth,
                        .Height = legendHeight
                    },
                    .Location = New Point With {
                        .X = scatterRegion.Right,
                        .Y = (scatterRegion.Height - legendHeight) / 2 + scatterPadding.Top
                    }
                }
                Dim designer As SolidBrush() = colors _
                    .Select(AddressOf TranslateColor) _
                    .Select(Function(c) New SolidBrush(c)) _
                    .ToArray
                Dim rangeTicks = density _
                    .pts _
                    .Select(Function(pt) pt.Statics) _
                    .IteratesALL _
                    .Range _
                    .CreateAxisTicks
                Dim legendTitleFont As Font = CSSFont.TryParse(legendTitleFontCSS).GDIObject(g.Dpi)
                Dim legendTickFont As Font = CSSFont.TryParse(legendTickFontCSS).GDIObject(g.Dpi)
                Dim legendTickStroke As Pen = Stroke.TryParse(legendTickStrokeCSS).GDIObject

                Call Legends.ColorMapLegend(
                    g, legendLayout, designer, rangeTicks,
                    legendTitleFont, "Density",
                    tickFont:=legendTickFont,
                    tickAxisStroke:=legendTickStroke,
                    unmapColor:=NameOf(Color.Gray))

                If TypeOf g Is Graphics2D Then
                    Return New ImageData(DirectCast(g, Graphics2D).ImageResource, g.Size, padding)
                Else
                    Return New SVGData(g, g.Size, padding)
                End If
            End Using
        End Function

        ''' <summary>
        ''' Create point density function by using <see cref="Grid"/> model
        ''' </summary>
        ''' <param name="grid"></param>
        ''' <param name="points"></param>
        ''' <returns></returns>
        <Extension>
        Public Function DensityMatrix(grid As Grid, points As IEnumerable(Of PointF), schema$(), r!) As SerialData
            ' 先统计出网络之中的每一个方格的点的数量，然后将数量转换为颜色值，生成散点图的模型
            Dim pointData As PointF() = points.ToArray
            Dim gridIndex = pointData _
                .Select(AddressOf grid.Index) _
                .ToArray
            Dim counts = gridIndex _
                .GroupBy(Function(index) index.ToString) _
                .ToDictionary(Function(index) index.Key,
                              Function(n) n.Count)
            Dim range As New IntRange(counts.Values)
            Dim colorIndex As New IntRange({0, schema.Length - 1})
            Dim density = gridIndex _
                .Select(Function(index) counts(index.ToString)) _
                .Select(Function(d)
                            Return range _
                                .ScaleMapping(d, colorIndex) _
                                .As(Of Integer)
                        End Function) _
                .ToArray
            Dim serialData = pointData _
                .SeqIterator _
                .Select(Function(pt)
                            Return New PointData With {
                                .value = density(pt),
                                .color = schema(CInt(.value)),
                                .pt = pt,
                                .Statics = {
                                    CDbl(counts(gridIndex(pt).ToString))
                                }
                            }
                        End Function) _
                .OrderBy(Function(pt) pt.value) _
                .ToArray

            Return New SerialData With {
                .color = Color.Black,
                .pts = serialData,
                .pointSize = r!
            }
        End Function
    End Module
End Namespace
