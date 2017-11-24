Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

Module wmfImage

    Sub Main()

        Using wmf As New Wmf(New Size(200, 200), "./test.wmf")
            Call wmf.Graphics.DrawString("Hello world!", New Font(FontFace.BookmanOldStyle, 16, FontStyle.Bold), Brushes.Red, New Point(20, 20))
            Call wmf.Graphics.FillPie(Brushes.Green, New Rectangle(100, 100, 50, 50), 0, 196)
            Call wmf.Graphics.FillRectangle(Brushes.Blue, New RectangleF(0, 100, 60, 60))
        End Using
    End Sub
End Module
