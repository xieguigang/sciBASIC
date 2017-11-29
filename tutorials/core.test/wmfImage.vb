Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage

Module wmfImage

    Public Function cccc() As Color

    End Function

    Sub Main()



        Call replaceColorsTest()

        Using wmf As New Wmf(New Size(200, 200), "./test.wmf")
            Call wmf.Graphics.DrawString("Hello world!", New Font(FontFace.BookmanOldStyle, 16, FontStyle.Bold), Brushes.Red, New Point(20, 20))
            Call wmf.Graphics.FillPie(Brushes.Green, New Rectangle(100, 100, 50, 50), 0, 196)
            Call wmf.Graphics.FillRectangle(Brushes.Blue, New RectangleF(0, 100, 60, 60))
        End Using
    End Sub

    Sub replaceColorsTest()

        Dim bitmap = New Bitmap("C:\Users\xieguigang\Downloads\3.bmp")

        bitmap = bitmap.ColorReplace(Color.FromArgb(239, 239, 239), Color.White)
        bitmap = bitmap.ColorReplace(Color.FromArgb(236, 236, 236), Color.White)
        bitmap = bitmap.ColorReplace(Color.FromArgb(241, 241, 241), Color.White)
        bitmap = bitmap.ColorReplace(Color.FromArgb(245, 245, 245), Color.White)
        bitmap = bitmap.ColorReplace(Color.FromArgb(242, 242, 242), Color.White)
        bitmap = bitmap.ColorReplace(Color.FromArgb(250, 250, 250), Color.White)

        Call bitmap.SaveAs("C:\Users\xieguigang\Downloads\3.png", ImageFormats.Png)


        Pause()
    End Sub
End Module
