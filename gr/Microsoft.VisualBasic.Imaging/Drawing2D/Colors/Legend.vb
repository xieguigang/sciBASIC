#Region "Microsoft.VisualBasic::d77a22d08655bebc4759e76e6f064450, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\Legend.vb"

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
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML
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
                                       Optional legendWidth! = -1) As GraphicsData
            Dim br As SolidBrush() =
                designer.ToArray(Function(c) New SolidBrush(c))
            Return br.ColorMapLegend(
                title,
                min, max,
                bg,
                haveUnmapped,
                lsize, padding,
                titleFont,
                legendWidth)
        End Function

        Public Const DefaultPadding$ = "padding:50px 50px 50px 50px;"

        ''' <summary>
        ''' 输出的图例的大小默认为：``{800, 1000}``
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
                                       Optional legendWidth! = -1) As GraphicsData
            If lsize.IsEmpty Then
                lsize = New Size(800, 1000)
            End If

            Return GraphicsPlots(
                lsize, CSS.Padding.op_Implicit(padding),
                bg,
                Sub(ByRef g, region)
                    Dim graphicsRegion As Rectangle = region.PlotRegion
                    Dim size As Size = region.Size
                    Dim margin As Padding = region.Padding
                    Dim grayHeight As Integer = size.Height * 0.05
                    Dim y As Single
                    Dim font As Font = If(titleFont Is Nothing,
                        New Font(FontFace.MicrosoftYaHei, 36),
                        titleFont)
                    Dim fSize As SizeF
                    Dim pt As Point
                    Dim rectWidth As Integer = If(legendWidth <= 0, size.Width - margin.Horizontal, legendWidth)
                    Dim legendsHeight As Integer = size.Height - (margin.Top * 3) - grayHeight * 3
                    Dim d As Single = legendsHeight / designer.Length
                    Dim left As Integer = margin.Left + 30 + rectWidth

                    Call g.DrawString(title, font, Brushes.Black, New Point(margin.Left, 0))

                    font = New Font(FontFace.BookmanOldStyle, 24)
                    y = margin.Top * 2

                    Call g.DrawString(max, font, Brushes.Black, New Point(left, y))

                    For i As Integer = designer.Length - 1 To 0 Step -1
                        Call g.FillRectangle(
                        designer(i),
                        New RectangleF(New PointF(margin.Left, y),
                                       New SizeF(rectWidth, d)))
                        y += d
                    Next

                    fSize = g.MeasureString(min, font)
                    Call g.DrawString(
                    min, font,
                    Brushes.Black,
                    New Point(left, If(designer.Length > 100, d, 0) + y - fSize.Height))

                    If haveUnmapped Then
                        y = size.Height - margin.Top - grayHeight
                        fSize = g.MeasureString("Unknown", font)
                        pt = New Point(left, y - (grayHeight - fSize.Height) / 2)
                        graphicsRegion = New Rectangle(New Point(margin.Left, y), New Size(rectWidth, grayHeight))

                        Call g.DrawString("Unknown", font, Brushes.Black, pt)
                        Call g.FillRectangle(Brushes.LightGray, graphicsRegion)
                    End If
                End Sub)
        End Function
    End Module
End Namespace
