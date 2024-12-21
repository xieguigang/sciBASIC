#Region "Microsoft.VisualBasic::b7e652548c643d9623cdc2c1880a8587, Data_science\Visualization\Plots\Contour\HeatMap\ContourHeatMapPlot.vb"

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

    '   Total Lines: 215
    '    Code Lines: 171 (79.53%)
    ' Comment Lines: 22 (10.23%)
    '    - Xml Docs: 68.18%
    ' 
    '   Blank Lines: 22 (10.23%)
    '     File Size: 8.77 KB


    '     Class ContourHeatMapPlot
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetColor, GetData
    ' 
    '         Sub: PlotInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports std = System.Math

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
Imports GraphicsPath = System.Drawing.Drawing2D.GraphicsPath
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
Imports GraphicsPath = Microsoft.VisualBasic.Imaging.GraphicsPath
#End If

Namespace Contour.HeatMap

    ''' <summary>
    ''' Contour heatmap 
    ''' 
    ''' A contour plot is a graphical technique for representing a 3-dimensional 
    ''' surface by plotting constant z slices, called contours, on a 2-dimensional 
    ''' format. That is, given a value for z, lines are drawn for connecting the 
    ''' ``(x,y)`` coordinates where that z value occurs.
    ''' 
    ''' The contour plot Is an alternative To a 3-D surface plot.
    ''' 
    ''' ###### 等高线图
    ''' 
    ''' 和普通的heatmap相比，这里的坐标轴是连续的数值变量，而普通的heatmap，其坐标轴都是离散的分类变量
    ''' </summary>
    Public Class ContourHeatMapPlot : Inherits Plot

        Public matrix As EvaluatePoints
        Public xrange As DoubleRange, yrange As DoubleRange
        Public xsteps!, ysteps!
        Public parallel As Boolean
        Public mapLevels%

        Public unit%
        Public logBase#
        Public minZ, maxZ As Double

        Public Sub New(theme As Theme)
            MyBase.New(theme)
        End Sub

        Private Function GetData(plotSize As Size) As (x#, y#, z#)()
            Return matrix.__getData(plotSize, xrange, yrange, xsteps, ysteps, parallel, Nothing, unit)
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
                    .Select(Function(x) std.Abs(+x)) _
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
                              Function(level)
                                  Return indexLevels(level.i)
                              End Function)

            Dim colors As SolidBrush() = Designer.GetBrushes(theme.colorSet, mapLevels)

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

        Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim rect As Rectangle = canvas.PlotRegion(css)
            Dim data = GetData(rect.Size)
            Dim xTicks = data.Select(Function(d) d.x).Range.CreateAxisTicks
            Dim yTicks = data.Select(Function(d) d.y).Range.CreateAxisTicks
            Dim x = d3js.scale.linear() _
                .domain(values:=xTicks) _
                .range(integers:={rect.Left, rect.Right})
            Dim y = d3js.scale.linear() _
                .domain(values:=yTicks) _
                .range(integers:={rect.Top, rect.Bottom})
            Dim colorDatas As SolidBrush() = Nothing
            Dim getColors = GetColor(data.Select(Function(o) o.z).ToArray, colorDatas)
            Dim scaler As New DataScaler() With {
                .AxisTicks = (xTicks.AsVector, yTicks.AsVector),
                .region = rect,
                .X = x,
                .Y = y
            }
            Dim size As Size = canvas.Size
            Dim margin As PaddingLayout = PaddingLayout.EvaluateFromCSS(css, canvas.Padding)
            Dim plotWidth! = rect.Width
            Dim plotHeight! = rect.Height
            ' 图例位于右边，占1/5的绘图区域的宽度，高度为绘图区域的高度的2/3
            Dim legendLayout As New Rectangle With {
                .Width = plotWidth / 5,
                .Height = plotHeight * (1 / 3),
                .X = canvas.Width - margin.Right / 2 - .Width,
                .Y = margin.Top + (plotHeight - .Height) / 2
            }

            Call g.DrawAxis(canvas, scaler,
                showGrid:=False,
                xlabel:=xlabel,
                ylabel:=ylabel,
                htmlLabel:=False,
                tickFontStyle:=theme.axisTickCSS
            )

            Dim us As New Size With {
                .Width = scaler.TranslateWidth(0, xsteps),
                .Height = scaler.TranslateHeight(0, ysteps)
            }

            For i As Integer = 0 To data.Length - 1
                Dim p As (X#, y#, Z#) = data(i)
                Dim c As SolidBrush = getColors(i)
                Dim fill As New RectangleF With {
                    .X = x(p.X),
                    .Y = y(p.y),
                    .Width = us.Width,
                    .Height = us.Height
                }

                Call g.FillRectangle(c, fill)
            Next

            ' Draw legends
            Dim realData#() = data _
                .Select(Function(o) o.z) _
                .Where(Function(z)
                           Return (Not z.IsNaNImaginary) AndAlso
                               z >= minZ AndAlso
                               z <= maxZ
                       End Function) _
                .ToArray
            Dim rangeTicks#() = realData.Range.CreateAxisTicks
            Dim legendFont As Font = css.GetFont(CSSFont.TryParse(theme.legendLabelCSS))
            Dim tickFont As Font = css.GetFont(CSSFont.TryParse(theme.legendTickCSS))

            Call g.ColorMapLegend(
                layout:=legendLayout,
                designer:=colorDatas,
                ticks:=rangeTicks,
                titleFont:=legendFont,
                title:=legendTitle,
                tickFont:=tickFont,
                tickAxisStroke:=New Pen(Color.Black, 2),
                unmapColor:=NameOf(Color.Gray),
                format:=theme.legendTickFormat
            )
        End Sub
    End Class
End Namespace
