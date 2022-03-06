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
            indexRange = New Double() {0, mapLevels}

            If defaultColor Is Nothing Then
                Me.defaultColor = colors.First
            Else
                Me.defaultColor = defaultColor
            End If
        End Sub

        ''' <summary>
        ''' scale raw data into <see cref="indexRange"/> for get corresponding color data
        ''' </summary>
        ''' <param name="raw"></param>
        ''' <returns></returns>
        Public Iterator Function ScalePixels(raw As IEnumerable(Of Pixel)) As IEnumerable(Of Pixel)
            Dim allPixels As Pixel() = raw.ToArray
            Dim range As DoubleRange = allPixels.Select(Function(p) p.Scale).ToArray
            Dim newPixel As Pixel

            For Each pixel As Pixel In allPixels
                newPixel = New Pixel With {
                    .Scale = range.ScaleMapping(pixel.Scale, indexRange),
                    .X = pixel.X,
                    .Y = pixel.Y
                }

                Yield pixel
            Next
        End Function

        Public Function RenderRasterImage(pixels As IEnumerable(Of Pixel), size As Size) As Bitmap
            Dim raw As New Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb)
            Dim level As Integer
            Dim color As Color

            Call raw.CreateCanvas2D(directAccess:=True).FillRectangle(Brushes.Transparent, New Rectangle(0, 0, raw.Width, raw.Height))

            Using buffer As BitmapBuffer = BitmapBuffer.FromBitmap(raw, ImageLockMode.WriteOnly)
                For Each point As Pixel In ScalePixels(pixels)
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

            Return raw
        End Function

    End Class
End Namespace