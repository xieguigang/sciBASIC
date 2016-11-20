#Region "Microsoft.VisualBasic::c73382dade84ad443495a312262916e7, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\Legend.vb"

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
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D

Namespace Drawing2D.Colors

    Public Module Legends

        <Extension>
        Public Function ColorMapLegend(designer As Color(), title$, min$, max$, Optional bg$ = "transparent", Optional haveUnmapped As Boolean = True, Optional lsize As Size = Nothing, Optional lmargin As Size = Nothing) As Bitmap
            If lsize.IsEmpty Then
                lsize = New Size(800, 1000)
            End If
            If lmargin.IsEmpty Then
                lmargin = New Size(50, 50)
            End If

            Return GraphicsPlots(
                lsize, lmargin, bg,
                Sub(ByRef g, region)
                    Dim graphicsRegion As Rectangle = region.PlotRegion
                    Dim size As Size = region.Size
                    Dim margin As Size = region.Margin
                    Dim grayHeight As Integer = size.Height * 0.05
                    Dim y As Single
                    Dim font As New Font(FontFace.MicrosoftYaHei, 42)
                    Dim fSize As SizeF
                    Dim pt As Point
                    Dim rectWidth As Integer = 150
                    Dim legendsHeight As Integer = size.Height - (margin.Height * 3) - grayHeight * 3
                    Dim d As Single = legendsHeight / designer.Length
                    Dim left As Integer = margin.Width + 30 + rectWidth

                    Call g.DrawString(title, font, Brushes.Black, New Point(margin.Width, 0))

                    y = margin.Height * 2
                    font = New Font(FontFace.MicrosoftYaHei, 32)

                    Call g.DrawString(max, font, Brushes.Black, New Point(left, y))

                    For i As Integer = designer.Length - 1 To 0 Step -1
                        Call g.FillRectangle(
                        New SolidBrush(designer(i)),
                        New RectangleF(New PointF(margin.Width, y),
                                      New SizeF(rectWidth, d)))
                        y += d
                    Next

                    fSize = g.MeasureString(min, font)
                    Call g.DrawString(
                    min,
                    font,
                    Brushes.Black,
                    New Point(left, If(designer.Length > 100, d, 0) + y - fSize.Height))

                    y = size.Height - margin.Height - grayHeight
                    fSize = g.MeasureString("Unknown", font)
                    pt = New Point(
                    left,
                    y - (grayHeight - fSize.Height) / 2)
                    Call g.FillRectangle(
                    Brushes.LightGray,
                    New Rectangle(New Point(margin.Width, y),
                                  New Size(rectWidth, grayHeight)))
                    Call g.DrawString("Unknown", font, Brushes.Black, pt)
                End Sub)
        End Function
    End Module
End Namespace
