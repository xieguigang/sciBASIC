Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports OCR

Module Module1

    Sub Main()
        Dim font As New Font(FontFace.MicrosoftYaHei, 14, FontStyle.Bold)
        Dim obj As Image

        Using g = New Size(font.Height, font.Height).CreateGDIDevice
            Call g.DrawString("8", font, Brushes.Black, New Point)
            obj = g.ImageResource
        End Using

        Dim view As Image

        Using g = New Size(200, 200).CreateGDIDevice
            Call g.DrawString("858++++", font, Brushes.Black, New Point)
            Call g.DrawString("008665", font, Brushes.Black, New Point(5, font.Height + 6))

            view = g.ImageResource
        End Using

        Dim locations = view.FindObjects(obj).ToArray

        Pause()
    End Sub
End Module
