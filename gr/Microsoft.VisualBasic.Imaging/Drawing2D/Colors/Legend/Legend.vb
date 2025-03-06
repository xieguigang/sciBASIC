#Region "Microsoft.VisualBasic::4385c9f43f5c5b14b0c6e95a939e86e1, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\Legend\Legend.vb"

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

    '   Total Lines: 346
    '    Code Lines: 243 (70.23%)
    ' Comment Lines: 51 (14.74%)
    '    - Xml Docs: 78.43%
    ' 
    '   Blank Lines: 52 (15.03%)
    '     File Size: 14.38 KB


    '     Module Legends
    ' 
    '         Function: ColorLegendHorizontal, (+2 Overloads) ColorMapLegend
    ' 
    '         Sub: ColorLegendHorizontal, ColorMapLegend
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.SVG
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports Microsoft.VisualBasic.MIME.Html.Render

Namespace Drawing2D.Colors

    Public Module Legends

        ''' <summary>
        ''' Draw color legend for the color sequnece.
        ''' (通过这个函数只是生成了legend的图片，还需要自己将图片放置到图表上的合适的位置)
        ''' </summary>
        ''' <param name="designer"></param>
        ''' <param name="title$">The legend title</param>
        ''' <param name="min$"></param>
        ''' <param name="max$"></param>
        ''' <param name="bg$"></param>
        ''' <param name="haveUnmapped"></param>
        ''' <param name="lsize"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ColorMapLegend(designer As Color(),
                                       title$,
                                       min$, max$,
                                       Optional bg$ = "transparent",
                                       Optional haveUnmapped As Boolean = True,
                                       Optional lsize As Size = Nothing,
                                       Optional padding$ = DefaultPadding,
                                       Optional titleFont As Font = Nothing,
                                       Optional labelFont As Font = Nothing,
                                       Optional legendWidth! = -1) As GraphicsData

            Dim br As SolidBrush() = designer _
                .Select(Function(c) New SolidBrush(c)) _
                .ToArray

            Return br.ColorMapLegend(
                title,
                min, max,
                bg,
                haveUnmapped,
                lsize, padding,
                titleFont, labelFont,
                legendWidth
            )
        End Function

        Public Const DefaultPadding$ = "padding:50px 50px 100px 50px;"

        ReadOnly defaultLegendSize As [Default](Of Size) = New Size(800, 1024)

        ''' <summary>
        ''' 竖直的颜色图例，输出的图例的大小默认为：``{800, 1024}``
        ''' </summary>
        ''' <param name="designer"></param>
        ''' <param name="title$"></param>
        ''' <param name="min$"></param>
        ''' <param name="max$"></param>
        ''' <param name="bg$"></param>
        ''' <param name="haveUnmapped"></param>
        ''' <param name="lsize"></param>
        ''' <param name="titleFont"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ColorMapLegend(designer As SolidBrush(),
                                       title$,
                                       min$, max$,
                                       Optional bg$ = "transparent",
                                       Optional haveUnmapped As Boolean = True,
                                       Optional lsize As Size = Nothing,
                                       Optional padding$ = DefaultPadding,
                                       Optional titleFont As Font = Nothing,
                                       Optional labelFont As Font = Nothing,
                                       Optional legendWidth! = -1) As GraphicsData

            Dim margin As Padding = padding

            If titleFont Is Nothing Then
                titleFont = New Font(FontFace.MicrosoftYaHei, 36)
            End If
            If labelFont Is Nothing Then
                labelFont = New Font(FontFace.BookmanOldStyle, 24)
            End If

            Dim plotInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)
                    Dim titleFontHeight As Single = g.MeasureString("A", titleFont).Height
                    Dim layout As New Rectangle With {
                        .X = 0,
                        .Y = 0,
                        .Width = region.Width,
                        .Height = region.Height - titleFontHeight - 5
                    }

                    Call g.ColorMapLegend(
                        layout:=layout, designer:=designer,
                        ticks:={Val(min), Val(max)},
                        titleFont:=titleFont,
                        title:=title,
                        tickFont:=labelFont,
                        tickAxisStroke:=Pens.Black
                    )
                End Sub

            Return GraphicsPlots(lsize Or defaultLegendSize, margin, bg, plotInternal)
        End Function

        ''' <summary>
        ''' 垂直的颜色谱的绘制：左边为颜色谱，右边为标尺，左边的颜色谱的上方为标题
        ''' </summary>
        ''' <param name="g"></param>
        ''' <param name="layout">legend的大小和位置</param>
        ''' <param name="unmapColor">
        ''' 当这个参数为空值的时候，将不会绘制未映射颜色示例
        ''' </param>
        <Extension>
        Public Sub ColorMapLegend(ByRef g As IGraphics, layout As Rectangle,
                                  designer As SolidBrush(),
                                  ticks#(),
                                  titleFont As Font, title$,
                                  tickFont As Font,
                                  tickAxisStroke As Pen,
                                  Optional unmapColor$ = Nothing,
                                  Optional ruleOffset! = 10,
                                  Optional format$ = "F2",
                                  Optional legendOffsetLeft! = -99999,
                                  Optional noLeftBlank As Boolean = False,
                                  Optional foreColor As String = "black",
                                  Optional maxWidth As Single = -1)

            Dim titleSize As SizeF = g.MeasureString(title, titleFont)
            Dim legendOffsetTop!
            Dim legendWidth! = layout.Width / 3 ' 颜色谱的宽度为layout的 1/3
            Dim legendHeight!
            Dim d!
            Dim offsetAuto As Boolean = legendOffsetLeft < 0
            Dim fontColor As Brush = foreColor.GetBrush

            If maxWidth > 0 Then
                If maxWidth < 1 Then
                    ' is percentage
                    maxWidth = layout.Width * maxWidth
                End If

                If legendWidth > maxWidth Then
                    legendWidth = maxWidth
                End If
            End If

            ' 首先计算出layout
            legendOffsetTop = titleSize.Height * 2 + 5

            If offsetAuto Then
                If noLeftBlank Then
                    legendOffsetLeft = 0
                Else
                    ' 下面的三个元素在宽度上面各自占1/3
                    ' 空白 | legend | 标尺
                    legendOffsetLeft = legendWidth
                End If
            End If

            If unmapColor.StringEmpty Then
                ' 没有unmap的颜色，则颜色谱的高度占据剩下的所有高度
                legendHeight = layout.Height - legendOffsetTop
                d = legendHeight / designer.Length
            Else
                legendHeight = layout.Height - legendOffsetTop
                d = legendHeight / (designer.Length + 2)
                legendHeight -= 2 * d
            End If

            Dim point As PointF
            Dim x!, y!
            Dim rect As RectangleF

            ' 绘制标题
            x = layout.Left + legendOffsetLeft - titleSize.Width / title.Length
            y = layout.Top
            point = New PointF(x, y)

            Call g.DrawString(title, titleFont, fontColor, point)

            ' 绘制出颜色谱
            y = legendOffsetTop + layout.Top
            legendOffsetLeft += layout.Left

            If TypeOf g Is GraphicsSVG Then
                If legendOffsetLeft < point.X Then
                    legendOffsetLeft = point.X
                End If
            End If

            For i As Integer = designer.Length - 1 To 0 Step -1
                rect = New RectangleF With {
                    .Location = New PointF(legendOffsetLeft, y),
                    .Size = New SizeF(legendWidth, d)
                }
                g.FillRectangle(brush:=designer(i), rect:=rect)
                y += d
            Next

            Dim tickFontHeight As Single = g.MeasureString("A", tickFont).Height

            If Not unmapColor.StringEmpty Then
                Dim color As Brush = unmapColor.GetBrush

                y += d * 3
                rect = New RectangleF With {
                    .Location = New PointF(legendOffsetLeft, y),
                    .Size = New SizeF(legendWidth, d)
                }
                point = New PointF With {
                    .X = legendOffsetLeft + legendWidth + 5,
                    .Y = y + (d - tickFontHeight) / 2
                }
                g.FillRectangle(color, rect:=rect)
                g.DrawString("Unknown", tickFont, fontColor, point)
            Else
                y += d
            End If

            ' 绘制出标尺
            x = legendOffsetLeft + legendWidth + ruleOffset
            y = layout.Top + legendOffsetTop
            g.DrawLine(tickAxisStroke, x, y, x, y + legendHeight)

            ' 绘制最大值和最小值
            g.DrawLine(Pens.Black, x, y, x + ruleOffset, y)
            g.DrawLine(Pens.Black, x, y + legendHeight, x + ruleOffset, y + legendHeight)

            Dim tickOffset As Double = If(App.IsMicrosoftPlatform, tickFontHeight / 2, 0)

            y -= tickFontHeight
            x += ruleOffset + 5
            point = New PointF(x, y - tickOffset)
            g.DrawString(ticks.Max.ToString(format), tickFont, fontColor, point)

            point = New PointF(x, y + legendHeight - tickOffset)
            g.DrawString(ticks.Min.ToString(format), tickFont, fontColor, point)

            ticks = ticks _
                .Skip(1) _
                .Take(ticks.Length - 2) _
                .OrderByDescending(Function(n) n) _
                .ToArray

            Dim delta As Single = legendHeight / If(ticks.Length = 0, 1, ticks.Length + 1)
            Dim tickStr As String

            y += delta
            x -= ruleOffset
            tickFont = New Font(tickFont.Name, tickFont.Size * 2.5 / 3)
            tickOffset = If(App.IsMicrosoftPlatform, tickFontHeight, 0)

            ' 画出剩余的小标尺
            For Each tick As Double In ticks
                tickStr = tick.ToString(format)

                If tick >= 0 Then
                    tickStr = " " & tickStr
                End If

                point = New PointF With {
                    .X = x + 2,
                    .Y = y - tickOffset
                }
                g.DrawLine(Pens.Black, x, y, x - 5, y)
                g.DrawString(tickStr, tickFont, fontColor, point)

                y += delta
            Next
        End Sub

        ''' <summary>
        ''' 横向的颜色legend
        ''' </summary>
        ''' <param name="designer"></param>
        ''' <param name="size"></param>
        ''' <param name="padding$"></param>
        ''' <param name="labelFontCSS$"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ColorLegendHorizontal(designer As SolidBrush(),
                                              ticks#(),
                                              size As Size,
                                              Optional padding$ = g.ZeroPadding,
                                              Optional labelFontCSS$ = CSSFont.Win7Normal) As GraphicsData
            Dim plotInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)
                    Call designer.ColorLegendHorizontal(
                        ticks,
                        g, region.PlotRegion(g.LoadEnvironment),
                        padding,
                        labelFontCSS)
                End Sub

            Return g.GraphicsPlots(size, padding, "transparent", plotInternal)
        End Function

        <Extension>
        Public Sub ColorLegendHorizontal(designer As SolidBrush(),
                                         ticks#(),
                                         ByRef g As IGraphics,
                                         region As Rectangle,
                                         Optional padding$ = g.ZeroPadding,
                                         Optional labelFontCSS$ = CSSFont.Win7Normal,
                                         Optional AxisStroke$ = Stroke.AxisStroke,
                                         Optional scientificNotation As Boolean = False)

            Dim env As CSSEnvirnment = New CSSEnvirnment(g.Size, g.Dpi)
            Dim font As Font = env.GetFont(CSSFont.TryParse(labelFontCSS))
            Dim l = designer.Length
            Dim dx = region.Width / l
            Dim h = region.Height * (2 / 3)
            Dim x = region.Left, y = region.Top

            ' 绘制出水平的颜色渐变条
            For i As Integer = 0 To l - 1
                Dim b = designer(i)
                Dim rect As New Rectangle(x, y, dx, h)

                g.FillRectangle(b, rect)
                x += dx
            Next

            ' 绘制出水平标尺刻度
            y = y + h + 10

            With region

                g.DrawLine(env.GetPen(Stroke.TryParse(AxisStroke)), New Point(.Left, y), New Point(x, y))
                y += 5

                For Each i As SeqValue(Of Double) In ticks _
                    .RangeTransform(New DoubleRange(.Left, x)) _
                    .SeqIterator

                    Dim tick$ = If(scientificNotation, ticks(i).ToString("G2"), ticks(i))
                    Dim fsize = g.MeasureString(tick, font)

                    x = i.value
                    g.DrawLine(Pens.Black, New PointF(x, y), New Point(x, y - 5))
                    g.DrawString(tick, font, Brushes.Black, New PointF(x - fsize.Width / 2, y))
                Next
            End With

        End Sub
    End Module
End Namespace
