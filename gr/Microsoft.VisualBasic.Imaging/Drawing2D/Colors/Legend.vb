#Region "Microsoft.VisualBasic::8224a4cac2a315d5ef14a92af9cf071c, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\Legend.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace Drawing2D.Colors

    Public Module Legends

        ''' <summary>
        ''' Draw color legend for the color sequnece
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
            Dim br As SolidBrush() =
                designer.ToArray(Function(c) New SolidBrush(c))
            Return br.ColorMapLegend(
                title,
                min, max,
                bg,
                haveUnmapped,
                lsize, padding,
                titleFont, labelFont,
                legendWidth)
        End Function

        Public Const DefaultPadding$ = "padding:50px 50px 50px 50px;"

        ''' <summary>
        ''' 竖直的颜色图例，输出的图例的大小默认为：``{800, 1000}``
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

            If lsize.IsEmpty Then
                lsize = New Size(800, 1000)
            End If
            If titleFont Is Nothing Then
                titleFont = New Font(FontFace.MicrosoftYaHei, 36)
            End If
            If labelFont Is Nothing Then
                labelFont = New Font(FontFace.BookmanOldStyle, 24)
            End If

            Dim plotInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)
                    Dim graphicsRegion As Rectangle = region.PlotRegion
                    Dim size As Size = region.Size
                    Dim grayHeight As Integer = size.Height * 0.05
                    Dim y As Single
                    Dim fSize As SizeF
                    Dim pt As Point
                    Dim rectWidth As Integer = If(legendWidth <= 0, size.Width - margin.Horizontal, legendWidth)
                    Dim legendsHeight As Integer = size.Height - (margin.Top * 3) - grayHeight * 3
                    Dim d As Single = legendsHeight / designer.Length
                    Dim left As Integer = margin.Left + 30 + rectWidth

                    Call g.DrawString(title, titleFont, Brushes.Black, New Point(margin.Left, 0))

                    fSize = g.MeasureString(max, labelFont)
                    y = margin.Top * 2

                    Call g.DrawString(max, labelFont, Brushes.Black, New Point(left, y - fSize.Height / 2))

                    For i As Integer = designer.Length - 1 To 0 Step -1
                        Call g.FillRectangle(
                            brush:=designer(i),
                            rect:=New RectangleF With {
                                .Location = New PointF(margin.Left, y),
                                .Size = New SizeF(rectWidth, d)
                            })
                        y += d
                    Next

                    fSize = g.MeasureString(min, labelFont)
                    Call g.DrawString(
                        min, labelFont, Brushes.Black,
                        New Point With {
                            .X = left,
                            .Y = If(designer.Length > 100, d, 0) + y - fSize.Height / 2
                        })

                    If haveUnmapped Then
                        y = size.Height - margin.Top - grayHeight
                        fSize = g.MeasureString("Unknown", labelFont)
                        pt = New Point(left, y + (grayHeight - fSize.Height) / 2)
                        graphicsRegion = New Rectangle With {
                            .Location = New Point(margin.Left, y),
                            .Size = New Size(rectWidth, grayHeight)
                        }

                        Call g.DrawString("Unknown", labelFont, Brushes.Black, pt)
                        Call g.FillRectangle(Brushes.LightGray, graphicsRegion)
                    End If
                End Sub

            Return GraphicsPlots(lsize, margin, bg, plotInternal)
        End Function

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
                        g, region.PlotRegion,
                        padding,
                        labelFontCSS)
                End Sub

            Return g.GraphicsPlots(
                size, padding,
                "transparent",
                plotInternal)
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

            Dim font As Font = CSSFont.TryParse(labelFontCSS)
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

                g.DrawLine(Stroke.TryParse(AxisStroke).GDIObject, New Point(.Left, y), New Point(.Right, y))
                y += 5

                For Each i As SeqValue(Of Double) In ticks _
                    .RangeTransform({region.Left, region.Right}) _
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
