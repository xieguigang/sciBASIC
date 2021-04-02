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
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
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

Namespace Contour

    ''' <summary>
    ''' Contour heatmap 
    ''' 
    ''' ###### 等高线图
    ''' 
    ''' 和普通的heatmap相比，这里的坐标轴是连续的数值变量，而普通的heatmap，其坐标轴都是离散的分类变量
    ''' </summary>
    Public Class ContourPlot : Inherits Plot

        Public offset As Point

        Public xrange As DoubleRange, yrange As DoubleRange
        Public xsteps!, ysteps!
        Public parallel As Boolean
        Public legendTitle$
        Public mapLevels%

        Public unit%
        Public logBase#
        Public minZ, maxZ As Double
        Public scale# = 1

        Public Sub New(theme As Theme)
            MyBase.New(theme)
        End Sub

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
            Dim data = GetData(canvas.PlotRegion.Size)
            Dim xTicks = data.Select(Function(d) d.x).Range.CreateAxisTicks
            Dim yTicks = data.Select(Function(d) d.y).Range.CreateAxisTicks
            Dim x = d3js.scale.linear() _
            .domain(xTicks) _
            .range(integers:={canvas.PlotRegion.Left, canvas.PlotRegion.Right})
            Dim y = d3js.scale.linear() _
            .domain(yTicks) _
            .range(integers:={canvas.PlotRegion.Top, canvas.PlotRegion.Bottom})
            Dim colorDatas As SolidBrush() = Nothing
            Dim getColors = GetColor(data.Select(Function(o) o.z).ToArray, colorDatas)
            Dim size As Size = canvas.Size
            Dim margin = canvas.Padding
            Dim plotWidth! = canvas.PlotRegion.Width
            Dim plotHeight! = canvas.PlotRegion.Height
            ' 图例位于右边，占1/5的绘图区域的宽度，高度为绘图区域的高度的2/3
            Dim legendLayout As New Rectangle With {
            .Width = plotWidth / 5,
            .Height = plotHeight * (1 / 3),
            .X = canvas.Width - margin.Right / 2 - .Width,
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
            Dim legendFont As Font = CSSFont.TryParse(theme.legendLabelCSS)
            Dim tickFont As Font = CSSFont.TryParse(theme.axisTickCSS)

            Call g.ColorMapLegend(
            legendLayout, colorDatas, rangeTicks,
            legendFont, legendTitle, tickFont,
            New Pen(Color.Black, 2),
            NameOf(Color.Gray))
        End Sub
    End Class
End Namespace