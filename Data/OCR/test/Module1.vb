Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Language
Imports OCR

Module Module1

    Sub translateTest()
        Dim viewSize As New Size(10, 12)
        Dim size As New Size(2, 3)

        Call 0.TranslateRegion(size, viewSize).__DEBUG_ECHO
        Call 8.TranslateRegion(size, viewSize).__DEBUG_ECHO
        Call 9.TranslateRegion(size, viewSize).__DEBUG_ECHO
        Call 10.TranslateRegion(size, viewSize).__DEBUG_ECHO

        Pause()
    End Sub

    Sub Main()

        ' Call translateTest()


        Dim font As New Font(FontFace.MicrosoftYaHei, 20, FontStyle.Bold)
        Dim obj As Image

        Using g = New Size(font.Height, font.Height).CreateGDIDevice
            Call g.DrawString("7", font, Brushes.Black, New Point)
            obj = g.ImageResource.CorpBlank
        End Using

        Dim view As Image

        Using g = New Size(500, 500).CreateGDIDevice
            Call g.DrawString("858++++", font, Brushes.Black, New Point)
            Call g.DrawString("0708665", font, Brushes.Black, New Point(50, font.Height + 60))

            view = g.ImageResource
        End Using

        'Using buffer = BitmapBuffer.FromImage(view)
        '    Dim objSize = obj.Size
        '    Dim i As int = 0

        '    For Each region In buffer.RegionScan(Color.White, objSize)
        '        Call region.DrawRegion(objSize).SaveAs($"./dddddd/{++i}.png")
        '    Next

        '    Pause()
        'End Using

        Dim locations = view.FindObjects(obj).ToArray

        Call obj.SaveAs("./obj.png")
        Call view.SaveAs("./view.png")

        Using g = view.CreateCanvas2D()
            For Each window In locations
                ' Call view.ImageCrop(window).SaveAs($"./sub/{window.ToString}.png")
                Call g.DrawRectangle(Pens.Red, window)
            Next

            Call g.ImageResource.SaveAs("./frame.png")
        End Using

        Pause()
    End Sub
End Module
