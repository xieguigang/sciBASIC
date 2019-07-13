Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.AVIMedia

Module Module1

    Sub Main()
        Dim frame As Bitmap = "E:\ba3a4a086e061d95dfc331c47bf40ad163d9ca04.jpg".LoadImage
        Dim avi As New Encoder(New Settings With {.width = frame.Width, .height = frame.Height})
        Dim stream As New AVIStream(24, frame.Width, frame.Height)

        For i As Integer = 0 To 4096
            Call stream.addFrame(frame)
        Next

        Call avi.streams.Add(stream)
        Call avi.WriteBuffer("X:\avi.js-master\avi.js-master\src\test222222.avi")

        Pause()
    End Sub

End Module

