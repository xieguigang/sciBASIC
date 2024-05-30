#Region "Microsoft.VisualBasic::745cc31ac8be43a67ed741e1501578f7, Data_science\Visualization\Plots\BarPlot\Plots\StackedPercentageBarPlot.vb"

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

    '   Total Lines: 171
    '    Code Lines: 125 (73.10%)
    ' Comment Lines: 14 (8.19%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 32 (18.71%)
    '     File Size: 7.95 KB


    '     Class StackedPercentageBarPlot
    ' 
    '         Properties: boxSeperator, columnCount, interval
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: DrawStackBars, DrawStackBarsFlip, PlotInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.BarPlot.Data
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render
Imports std = System.Math

Namespace BarPlot

    Public Class StackedPercentageBarPlot : Inherits Plot

        ReadOnly data As BarDataGroup

        Public Sub New(data As BarDataGroup, theme As Theme)
            MyBase.New(theme)

            Me.data = data
        End Sub

        Public Property interval As Single
        Public Property columnCount As Integer
        Public Property boxSeperator As Single

        <Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:验证平台兼容性", Justification:="<挂起>")>
        Public Shared Sub DrawStackBarsFlip(data As BarDataGroup,
                                            ByRef g As IGraphics,
                                            canvas As GraphicsRegion,
                                            interval As Single)

            Dim serialBrushes As NamedValue(Of SolidBrush)() = data.loadBrushes.ToArray
            ' 条形图区域的总高度
            Dim barRegionHeight = canvas.PlotRegion.Height
            Dim barRegionWidth = canvas.PlotRegion.Width
            Dim n = data.Samples.Length
            Dim wb = BarWidth(barRegionHeight, n, interval)
            Dim y0! = canvas.Padding.Top

            ' 遍历X轴上面的每一个分组
            For Each group As BarDataSample In data.Samples
                Dim sum# = group.StackedSum
                Dim x0! = canvas.Padding.Left

                ' 慢慢的从上面累加y到下面底部
                For Each serial As SeqValue(Of NamedValue(Of SolidBrush)) In serialBrushes.SeqIterator
                    Dim value As Double = group.data(serial) / sum  ' 百分比
                    Dim w = value * barRegionWidth
                    Dim bar As New RectangleF(New PointF(x0, y0), New SizeF(w, wb))

                    g.FillRectangle(serial.value.Value, rect:=bar)
                    x0 += w
                Next

                y0 += wb + interval
            Next
        End Sub

        <Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:验证平台兼容性", Justification:="<挂起>")>
        Public Shared Sub DrawStackBars(data As BarDataGroup,
                                        ByRef g As IGraphics,
                                        canvas As GraphicsRegion,
                                        interval As Single)

            Dim serialBrushes As NamedValue(Of SolidBrush)() = data.loadBrushes.ToArray
            ' 条形图区域的总高度
            Dim barRegionHeight = canvas.PlotRegion.Height
            Dim x0! = canvas.Padding.Left
            Dim barRegionWidth = canvas.PlotRegion.Width
            Dim n = data.Samples.Length
            Dim wb = BarWidth(barRegionWidth, n, interval)

            ' 遍历X轴上面的每一个分组
            For Each group As BarDataSample In data.Samples
                Dim y0! = canvas.Padding.Top
                Dim sum# = group.StackedSum

                ' 慢慢的从上面累加y到下面底部
                For Each serial As SeqValue(Of NamedValue(Of SolidBrush)) In serialBrushes.SeqIterator
                    Dim value As Double = group.data(serial) / sum  ' 百分比
                    Dim h = value * barRegionHeight
                    Dim bar As New RectangleF(New PointF(x0, y0), New SizeF(wb, h))

                    g.FillRectangle(serial.value.Value, rect:=bar)
                    y0 += h
                Next

                x0 += wb + interval
            Next
        End Sub

        <Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:验证平台兼容性", Justification:="<挂起>")>
        Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)
            Dim rect As Rectangle = canvas.PlotRegion
            Dim width = rect.Width
            Dim height = rect.Height
            Dim css As CSSEnvirnment = g.LoadEnvironment
            Dim tickFont As Font = css.GetFont(CSSFont.TryParse(theme.axisTickCSS))
            Dim axisFont As Font = css.GetFont(CSSFont.TryParse(theme.axisLabelCSS))
            Dim legendFont As Font = css.GetFont(CSSFont.TryParse(theme.legendLabelCSS))
            Dim tickSize = g.MeasureString("0.00", tickFont)
            Dim leftPart = axisFont.Height + tickSize.Width + 10
            Dim groupLabelFont As Font = css.GetFont(CSSFont.TryParse(theme.legendTitleCSS))
            Dim boxWidth% = legendFont.Height * 1.1
            Dim bottomPart = groupLabelFont.Height + 30 + (boxWidth + boxSeperator * 2) * columnCount
            ' 条形图区域的总高度
            Dim barRegionHeight = height - bottomPart
            Dim x0! = canvas.Padding.Left + leftPart
            Dim serialBrushes As NamedValue(Of SolidBrush)() = data.loadBrushes.ToArray
            Dim wb = BarWidth(width, data.Samples.Length, interval)

            Call New GraphicsText(DirectCast(g, Graphics2D).Graphics).DrawString(
                ylabel, axisFont,
                Brushes.Black,
                New PointF((canvas.Padding.Left - axisFont.Height), height / 2),
                angle:=-90)

            ' 绘制y轴
            For Each tick# In {0.00, 0.25, 0.5, 0.75, 1.0}
                Dim y# = rect.Height - canvas.Padding.Bottom - bottomPart - barRegionHeight * tick
                Dim location As New PointF(x0 - tickSize.Width - 20, y - tickSize.Height / 2)

                g.DrawLine(Pens.Black, New PointF(x0 - 10, y), New PointF(x0 - 20, y))
                g.DrawString(tick.ToString("F2"), tickFont, Brushes.Black, location)
            Next

            Call DrawStackBars(data, g, canvas, interval)

            ' 遍历X轴上面的每一个分组
            For Each group As BarDataSample In data.Samples
                Dim x!, y!
                Dim labelSize = g.MeasureString(group.tag, groupLabelFont)
                Dim y0! = canvas.Padding.Top

                x = x0 + (wb - labelSize.Width) / 2
                y = y0 + (30)

                Call g.DrawString(group.tag, groupLabelFont, Brushes.Black, New PointF(x, y))
            Next

            ' 绘制图例
            Dim bottomY = canvas.Padding.Top + barRegionHeight + boxWidth * 2 + groupLabelFont.Height
            Dim ly! = bottomY

            x0 = canvas.Padding.Left + leftPart

            For Each block As NamedValue(Of SolidBrush)() In serialBrushes.Split(columnCount)
                ' 似乎在for循环之中申明的变量必须要初始化，否则下一个循环使用的是上一个循环的结果值？？？
                ' 这是一个bug？
                Dim maxWidth% = 0

                For Each legend As NamedValue(Of SolidBrush) In block
                    Dim box As New Rectangle(x0, ly, boxWidth, boxWidth)
                    ' 绘制方形色块
                    g.FillRectangle(legend.Value, box)
                    ' 绘制系列标签
                    g.DrawString(legend.Name, legendFont, Brushes.Black, New PointF(x0 + boxWidth + 5, ly))

                    maxWidth = std.Max(maxWidth, g.MeasureString(legend.Name, legendFont).Width)
                    ly += boxSeperator + boxWidth
                Next

                ly = bottomY
                x0 += boxSeperator * 2 + boxWidth + maxWidth
                maxWidth = 0
            Next
        End Sub
    End Class
End Namespace
