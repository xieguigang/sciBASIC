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
        Public Function RGB(bitmap As BitmapBuffer) As (R As BitmapBuffer, G As BitmapBuffer, B As BitmapBuffer)
            Dim h = bitmap.Height
            Dim w = bitmap.Width
            Dim pixels As Color(,) = bitmap.GetARGB
            Dim r As Color(,) = New Color(h - 1, w - 1) {}
            Dim g As Color(,) = New Color(h - 1, w - 1) {}
            Dim b As Color(,) = New Color(h - 1, w - 1) {}
            Dim size As New Size(w, h)

            For y As Integer = 0 To h
                For x As Integer = 0 To w
                    Dim pixel As Color = pixels(y, x)

                    r(y, x) = Color.FromArgb(pixel.R, 0, 0)
                    g(y, x) = Color.FromArgb(0, pixel.G, 0)
                    b(y, x) = Color.FromArgb(0, 0, pixel.B)
                Next
            Next

            Return (New BitmapBuffer(r, size), New BitmapBuffer(g, size), New BitmapBuffer(b, size))
        End Function

    End Module
End Namespace