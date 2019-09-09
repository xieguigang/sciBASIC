#Region "Microsoft.VisualBasic::00fcc7f5faff49dbb52400b2ae2b02e6, Data_science\Visualization\Plots-statistics\VariableWidthBarPlot.vb"

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

    ' Module VariableWidthBarPlot
    ' 
    '     Function: Plot
    ' 
    ' Class VariableBarData
    ' 
    '     Properties: Color, Data, Name
    ' 
    '     Function: ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Text

Public Module VariableWidthBarPlot

    <Extension>
    Public Function Plot(data As IEnumerable(Of VariableBarData),
                         Optional size$ = "3000,2100",
                         Optional padding$ = g.DefaultUltraLargePadding,
                         Optional bg$ = "white",
                         Optional colorSchema$ = "Set1:c9",
                         Optional title$ = Nothing,
                         Optional titleFontCSS$ = CSSFont.Win7VeryVeryLarge,
                         Optional axisPenCSS$ = Stroke.AxisStroke,
                         Optional ticksPenCSS$ = Stroke.AxisStroke,
                         Optional XLabelFontCSS$ = CSSFont.Win7VeryLarge,
                         Optional tickFontCSS$ = CSSFont.Win7Large,
                         Optional tickLength% = 20,
                         Optional showDataLabel As Boolean = True,
                         Optional dataLabelFontCSS$ = CSSFont.Win7Large) As GraphicsData

        Dim colors As Color() = Designer.GetColors(colorSchema)
        Dim list = data.ToArray
        Dim X As Vector = list.Select(Function(b) b.Data.width).AsVector
        Dim Y As Vector = list.Select(Function(b) b.Data.height).AsVector.CreateAxisTicks
        Dim sumX# = X.Sum
        Dim p As i32 = Scan0
        Dim axisPen As Pen = Stroke.TryParse(axisPenCSS).GDIObject
        Dim tickPen As Pen = Stroke.TryParse(ticksPenCSS).GDIObject
        Dim XLabelFont As Font = CSSFont.TryParse(XLabelFontCSS).GDIObject
        Dim tickFont As Font = CSSFont.TryParse(tickFontCSS).GDIObject
        Dim titleFont As Font = CSSFont.TryParse(titleFontCSS).GDIObject
        Dim dataLabelFont As Font = CSSFont.TryParse(dataLabelFontCSS).GDIObject
        Dim plotInternal =
            Sub(ByRef g As IGraphics, region As GraphicsRegion)

                Dim plotRect As Rectangle = region.PlotRegion
                Dim scaler As New DataScaler With {
                    .Region = plotRect,
                    .X = d3js.scale.linear.domain(X).range(integers:={plotRect.Left, plotRect.Right}),
                    .Y = d3js.scale.linear.domain(Y).range(integers:={plotRect.Top, plotRect.Bottom})
                }

                ' 绘制出坐标轴
                ' X 坐标轴
                Call g.DrawLine(axisPen, New Point(plotRect.Left, plotRect.Bottom), New Point(plotRect.Right, plotRect.Bottom))
                ' Y 坐标轴
                Call g.DrawLine(axisPen, New Point(plotRect.Left, plotRect.Bottom), New Point(plotRect.Left, plotRect.Top))

                ' 绘制变宽度的条形图
                Dim left% = plotRect.Left
                Dim totalWidth% = plotRect.Width * 0.98
                Dim width%
                Dim rect As RectangleF
                Dim color As Color
                Dim labelSize As SizeF
                Dim label$

                ' 绘制Y坐标轴ticks
                Dim ty#
                Dim textDrawer As New GraphicsText(DirectCast(g, GDICanvas).Graphics)

                For Each tick As Double In Y
                    ty = scaler.TranslateY(tick)
                    g.DrawLine(tickPen, New PointF(left, ty), New PointF(left - tickLength, ty))
                    label = tick.ToString("G2")
                    labelSize = g.MeasureString(label, tickFont)
                    g.DrawString(label, tickFont, Brushes.Black, New Point(left - tickLength - labelSize.Width - tickLength / 2, ty - labelSize.Height / 2))
                Next

                left += plotRect.Width * 0.01

                For Each bar As VariableBarData In list
                    width = bar.Data.width / sumX * totalWidth
                    rect = New RectangleF With {
                        .X = left,
                        .Y = scaler.TranslateY(bar.Data.height),
                        .Height = plotRect.Bottom - .Y,
                        .Width = width
                    }
                    color = colors(++p)
                    labelSize = g.MeasureString(bar.Name, XLabelFont)

                    Call g.FillRectangle(New SolidBrush(color), rect)

                    ' 绘制数据系列标签
                    Call textDrawer.DrawString(bar.Name, XLabelFont, Brushes.Black, New PointF(left + (width - labelSize.Width) / 2, plotRect.Bottom + labelSize.Width), angle:=-45.0!)
                    ' 绘制数据点标签
                    If showDataLabel Then
                        label = bar.Data.height.ToString("G2")
                        labelSize = g.MeasureString(label, dataLabelFont)
                        g.DrawString(label, dataLabelFont, Brushes.Black, New Point(left + (width - labelSize.Width) / 2, rect.Y - labelSize.Height - 5))
                    End If

                    left += width
                Next

                ' 绘制title和各种标签
                If Not title.StringEmpty Then
                    labelSize = g.MeasureString(title, titleFont)
                    g.DrawString(title, titleFont, Brushes.Black, New Point(plotRect.Left + (plotRect.Width - labelSize.Width) / 2, plotRect.Top - labelSize.Height / 2))
                End If
            End Sub

        Return g.GraphicsPlots(size.SizeParser, padding, bg, plotInternal)
    End Function
End Module

Public Class VariableBarData

    Public Property Name As String
    Public Property Data As (width#, height#)
    Public Property Color As String

    Public Overrides Function ToString() As String
        Return $"{Name} [{Data.width} @ {Data.height}]"
    End Function
End Class
