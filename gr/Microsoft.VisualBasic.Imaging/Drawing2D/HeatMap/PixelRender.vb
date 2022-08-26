#Region "Microsoft.VisualBasic::dfbd698bfbe1eefcea66306869017ca8, sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\HeatMap\PixelRender.vb"

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

    '   Total Lines: 109
    '    Code Lines: 75
    ' Comment Lines: 17
    '   Blank Lines: 17
    '     File Size: 4.11 KB


    '     Class PixelRender
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: RenderRasterImage, ScalePixels
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Imaging
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors

Namespace Drawing2D.HeatMap

    Public Class PixelRender

        ReadOnly colors As Color()
        ReadOnly indexRange As DoubleRange
        ReadOnly defaultColor As Color

        Sub New(colorSet As String, mapLevels As Integer, Optional defaultColor As Color? = Nothing)
            colors = Designer.GetColors(colorSet, mapLevels)
            indexRange = New Double() {0, mapLevels - 1}

            If defaultColor Is Nothing Then
                Me.defaultColor = colors.First
            Else
                Me.defaultColor = defaultColor
            End If
        End Sub

        ''' <summary>
        ''' scale raw data into <see cref="indexRange"/> for get 
        ''' corresponding color data.
        ''' </summary>
        ''' <param name="raw"></param>
        ''' <returns></returns>
        Public Iterator Function ScalePixels(raw As IEnumerable(Of Pixel)) As IEnumerable(Of Pixel)
            Dim allPixels As Pixel() = raw.ToArray
            Dim range As DoubleRange = allPixels _
                .Select(Function(p) p.Scale) _
                .ToArray
            Dim newPixel As Pixel

            For Each pixel As Pixel In allPixels
                newPixel = New PixelData With {
                    .Scale = range.ScaleMapping(pixel.Scale, indexRange),
                    .X = pixel.X,
                    .Y = pixel.Y
                }

                Yield newPixel
            Next
        End Function

        ''' <summary>
        ''' rendering pixels with no size scaling.
        ''' </summary>
        ''' <param name="pixels"></param>
        ''' <param name="size"></param>
        ''' <returns></returns>
        Public Function RenderRasterImage(Of T As Pixel)(pixels As IEnumerable(Of T), size As Size, Optional fillRect As Boolean = True) As Bitmap
            Dim raw As New Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb)
            Dim level As Integer
            Dim color As Color
            Dim full As New Rectangle(0, 0, raw.Width, raw.Height)
            Dim pixel As RectangleF
            Dim g As IGraphics = raw.CreateCanvas2D(directAccess:=True)

            Call g.Clear(defaultColor)
            '
            ' 20220525 set pixels is not working on the linux server platform
            '
            If fillRect Then
                Dim solids As SolidBrush() = colors _
                    .Select(Function(c) New SolidBrush(c)) _
                    .ToArray
                Dim paint As SolidBrush
                Dim defaultPaint As New SolidBrush(defaultColor)

                For Each point As Pixel In ScalePixels(pixels.Select(Function(i) DirectCast(i, Pixel)))
                    level = CInt(point.Scale)

                    If level <= 0.0 Then
                        paint = defaultPaint
                    Else
                        paint = solids(level)
                    End If

                    pixel = New RectangleF(point.X, point.Y, 1, 1)
                    g.FillRectangle(paint, pixel)
                Next
            Else
                Using buffer As BitmapBuffer = BitmapBuffer.FromBitmap(raw, ImageLockMode.WriteOnly)
                    For Each point As Pixel In ScalePixels(pixels.Select(Function(i) DirectCast(i, Pixel)))
                        level = CInt(point.Scale)

                        If level <= 0.0 Then
                            color = defaultColor
                        Else
                            color = colors(level)
                        End If

                        ' imzXML里面的坐标是从1开始的
                        ' 需要减一转换为.NET中从零开始的位置
                        Call buffer.SetPixel(point.X - 1, point.Y - 1, color)
                    Next
                End Using
            End If

            Return raw
        End Function

    End Class
End Namespace
