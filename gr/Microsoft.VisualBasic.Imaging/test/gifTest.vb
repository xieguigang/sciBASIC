Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.Imaging

Module gifTest

    Sub Main()
        Using file As FileStream = "./test.gif".Open, gif As New GifEncoder(file)
            Dim frame1 As Graphics2D = New Size(200, 200).CreateGDIDevice
            Dim frame2 As Graphics2D = New Size(200, 200).CreateGDIDevice

            Call frame1.DrawString("Hello", New Font(FontFace.BookmanOldStyle, 20, FontStyle.Bold), Brushes.Red, 10, 10)
            Call frame2.DrawString("World", New Font(FontFace.BookmanOldStyle, 20, FontStyle.Bold), Brushes.Red, 10, 10)

            Call gif.AddFrame(frame1,,, New TimeSpan(0, 0, 3))
            Call gif.AddFrame(frame2,,, New TimeSpan(0, 0, 3))
        End Using
    End Sub
End Module
