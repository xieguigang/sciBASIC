Imports System.Drawing
Imports System.Runtime.CompilerServices

Namespace Imaging.BitmapImage

    Public Module BitmapChannels

        ''' <summary>
        ''' split the bitmap into rgb channels
        ''' </summary>
        ''' <param name="bitmap"></param>
        ''' <returns></returns>
        <Extension>
        Public Function RGB(bitmap As BitmapBuffer, Optional flip As Boolean = False) As (R As BitmapBuffer, G As BitmapBuffer, B As BitmapBuffer)
            Dim h = bitmap.Height
            Dim w = bitmap.Width
            Dim pixels As Color(,) = bitmap.GetARGB
            Dim r As Color(,) = New Color(h - 1, w - 1) {}
            Dim g As Color(,) = New Color(h - 1, w - 1) {}
            Dim b As Color(,) = New Color(h - 1, w - 1) {}
            Dim size As New Size(w, h)

            For y As Integer = 0 To h - 1
                For x As Integer = 0 To w - 1
                    Dim pixel As Color = pixels(y, x)

                    If flip Then
                        r(y, x) = Color.FromArgb(255 - pixel.R, 255, 255)
                        g(y, x) = Color.FromArgb(255, 255 - pixel.G, 255)
                        b(y, x) = Color.FromArgb(255, 255, 255 - pixel.B)
                    Else
                        r(y, x) = Color.FromArgb(pixel.R, 0, 0)
                        g(y, x) = Color.FromArgb(0, pixel.G, 0)
                        b(y, x) = Color.FromArgb(0, 0, pixel.B)
                    End If
                Next
            Next

            Return (New BitmapBuffer(r, size), New BitmapBuffer(g, size), New BitmapBuffer(b, size))
        End Function

        <Extension>
        Public Function CMYK(bitmap As BitmapBuffer, Optional flip As Boolean = False) As (C As BitmapBuffer, M As BitmapBuffer, Y As BitmapBuffer, K As BitmapBuffer)
            Dim h = bitmap.Height
            Dim w = bitmap.Width
            Dim pixels As Color(,) = bitmap.GetARGB
            Dim c As Color(,) = New Color(h - 1, w - 1) {}
            Dim m As Color(,) = New Color(h - 1, w - 1) {}
            Dim y As Color(,) = New Color(h - 1, w - 1) {}
            Dim k As Color(,) = New Color(h - 1, w - 1) {}
            Dim size As New Size(w, h)

            For yi As Integer = 0 To h - 1
                For xi As Integer = 0 To w - 1
                    Dim pixel As CMYKColor = pixels(yi, xi)

                    If flip Then
                        c(yi, xi) = New CMYKColor(1 - pixel.C, 1, 1, 1)
                        m(yi, xi) = New CMYKColor(1, 1 - pixel.M, 1, 1)
                        y(yi, xi) = New CMYKColor(1, 1, 1 - pixel.Y, 1)
                        k(yi, xi) = New CMYKColor(1, 1, 1, 1 - pixel.K)
                    Else
                        c(yi, xi) = New CMYKColor(pixel.C, 0, 0, 0)
                        m(yi, xi) = New CMYKColor(0, pixel.M, 0, 0)
                        y(yi, xi) = New CMYKColor(0, 0, pixel.Y, 0)
                        k(yi, xi) = New CMYKColor(0, 0, 0, pixel.K)
                    End If
                Next
            Next

            Return (
                New BitmapBuffer(c, size),
                New BitmapBuffer(m, size),
                New BitmapBuffer(y, size),
                New BitmapBuffer(k, size)
            )
        End Function

    End Module
End Namespace