#Region "Microsoft.VisualBasic::713e2a556f3fc8bb4fa5ce9b855e5987, Data_science\Visualization\Plots\BarPlot\LevelBarplot.vb"

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

    '   Total Lines: 185
    '    Code Lines: 152 (82.16%)
    ' Comment Lines: 8 (4.32%)
    '    - Xml Docs: 37.50%
    ' 
    '   Blank Lines: 25 (13.51%)
    '     File Size: 8.84 KB


    '     Module LevelBarplot
    ' 
    '         Function: Plot, trimLabel
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Legends
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports std = System.Math

Namespace BarPlot

    ''' <summary>
    ''' 只针对单组数据的条形图绘制
    ''' </summary>
    Public Module LevelBarplot

        Private Function trimLabel(maxLen As Integer) As Func(Of String, String)
            Return Function(lb) As String
                       If lb.Length > maxLen Then
                           Return Mid(lb, 1, maxLen) & "..."
                       Else
                           Return lb
                       End If
                   End Function
        End Function

        Public Function Plot(data As NamedValue(Of Double)(),
                             Optional size$ = "2700,2100",
                             Optional margin$ = Resolution2K.PaddingWithRightLegend,
                             Optional bg$ = "white",
                             Optional title$ = "BarPlot",
                             Optional titleFontCSS$ = CSSFont.Win7VeryLarge,
                             Optional labelFontCSS$ = CSSFont.Win7Large,
                             Optional chartBoxStroke$ = Stroke.ScatterLineStroke,
                             Optional maxLabelLength% = 48,
                             Optional levelColorSchema$ = ColorMap.PatternJet,
                             Optional colorLevels% = 30,
                             Optional tickFormat$ = "F1",
                             Optional tickFontCSS$ = CSSFont.Win7LargerNormal,
                             Optional legendTitle$ = "Value Levels",
                             Optional valueTitle$ = "Value Levels",
                             Optional valueTitleFontCSS$ = CSSFont.Win7LargerBold,
                             Optional nolabelTrim As Boolean = False,
                             Optional ppi As Integer = 100) As GraphicsData

            Dim trim As Func(Of String, String)

            If nolabelTrim Then
                trim = Function(s) s
            Else
                trim = trimLabel(maxLabelLength)
            End If

            Dim maxLengthLabel$ = data.Keys _
                .Select(trim) _
                .MaxLengthString
            Dim colors As SolidBrush() = Designer _
                .GetColors(levelColorSchema, colorLevels) _
                .Select(Function(c) New SolidBrush(c)) _
                .ToArray
            Dim colorIndex As DoubleRange = {0.0, colors.Length - 1}
            Dim indexScaler As DoubleRange = data.Select(Function(i) i.Value).ToArray

            Dim plotInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)
                    Dim css As CSSEnvirnment = g.LoadEnvironment
                    Dim pen As Pen = css.GetPen(Stroke.TryParse(chartBoxStroke))
                    Dim titleFont As Font = css.GetFont(CSSFont.TryParse(titleFontCSS))
                    Dim labelFont As Font = css.GetFont(CSSFont.TryParse(labelFontCSS))
                    Dim tickFont As Font = css.GetFont(CSSFont.TryParse(tickFontCSS))
                    Dim valueTitleFont As Font = css.GetFont(CSSFont.TryParse(valueTitleFontCSS))
                    Dim maxLabelSize As SizeF = g.MeasureString(maxLengthLabel, labelFont)
                    Dim plotRegion As Rectangle = region.PlotRegion
                    Dim pos As PointF
                    Dim chartBox As New Rectangle With {
                        .X = plotRegion.Left + maxLabelSize.Width + 5,
                        .Y = plotRegion.Top,
                        .Width = plotRegion.Width - maxLabelSize.Width - 5,
                        .Height = plotRegion.Height
                    }

                    ' draw main title
                    Dim titleSize As SizeF = g.MeasureString(title, titleFont)
                    Dim label As String

                    pos = New PointF With {
                        .X = plotRegion.Left + (plotRegion.Width - titleSize.Width) / 2,
                        .Y = plotRegion.Top - titleSize.Height - std.Min(10, titleSize.Height / 2)
                    }

                    Call g.DrawString(title, titleFont, Brushes.Black, pos)
                    Call g.DrawRectangle(pen, chartBox)
                    Call g.FillRectangle(bg.GetBrush, chartBox)

                    Dim ticks = {0, indexScaler.Max}.CreateAxisTicks
                    Dim widthScaler = d3js _
                        .scale _
                        .linear _
                        .domain(values:=ticks) _
                        .range(integers:={0, chartBox.Width})
                    Dim width As Integer
                    Dim dy As Integer = chartBox.Height / (data.Length + 1)
                    Dim dyInterval = (chartBox.Height / data.Length) / (data.Length)
                    Dim x As Integer
                    Dim y As Integer = chartBox.Top
                    Dim bar As Rectangle
                    Dim levelIndex As Integer

                    For Each item As NamedValue(Of Double) In data
                        label = trim(item.Name)
                        width = widthScaler(item.Value)
                        levelIndex = indexScaler.ScaleMapping(item.Value, colorIndex)
                        y += dyInterval
                        bar = New Rectangle With {
                            .X = chartBox.Left,
                            .Y = y,
                            .Width = width,
                            .Height = dy
                        }

                        ' label是右对齐的
                        titleSize = g.MeasureString(label, labelFont)
                        pos = New PointF With {
                            .X = chartBox.Left - 5 - titleSize.Width,
                            .Y = y + (dy - titleSize.Height) / 2
                        }

                        Call g.DrawString(label, labelFont, Brushes.Black, pos)
                        Call g.FillRectangle(colors(levelIndex), bar)

                        y += dy
                    Next

                    y = chartBox.Bottom + 5
                    pen.DashStyle = DashStyle.Solid

                    ' 绘制标尺
                    For Each tick As Double In ticks
                        label = tick.ToString(tickFormat)
                        x = chartBox.Left + widthScaler(tick)
                        pos = New PointF With {
                            .X = x - g.MeasureString(label, tickFont).Width / 2,
                            .Y = y + 15
                        }

                        Call g.DrawString(label, tickFont, Brushes.Black, pos)
                        Call g.DrawLine(pen, New PointF(x, y), New Point(x, y + 15))
                    Next

                    Call g.DrawLine(
                        pen:=pen,
                        pt1:=New PointF(chartBox.Left + widthScaler(0), y),
                        pt2:=New PointF(chartBox.Left + widthScaler(ticks.Max), y)
                    )

                    ' 绘制颜色标尺
                    Dim legendLayout As New Rectangle With {
                        .X = chartBox.Right + 20,
                        .Y = chartBox.Top + (chartBox.Height - 200) / 2,
                        .Width = region.Padding.Right * (2 / 3),
                        .Height = chartBox.Height / 2
                    }

                    Call g.ColorMapLegend(legendLayout, colors, ticks, valueTitleFont, legendTitle, tickFont, pen, Nothing)

                    ' 绘制底部的小标题
                    titleSize = g.MeasureString(valueTitle, valueTitleFont)
                    x = chartBox.Left + (chartBox.Width - titleSize.Width) / 2
                    y = chartBox.Bottom + (region.Padding.Bottom - titleSize.Height) / 2

                    Call g.DrawString(valueTitle, valueTitleFont, Brushes.Black, New PointF(x, y))
                End Sub

            Return g.GraphicsPlots(size.SizeParser, margin, bg, plotInternal)
        End Function
    End Module
End Namespace
