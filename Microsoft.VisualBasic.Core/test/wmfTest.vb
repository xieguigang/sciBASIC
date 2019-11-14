Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

Module wmfTest

    Sub Main()
        Using wmf As New Wmf(New Size(1200, 500), "./test.wmf", backgroundColor:="white")
            Call wmf.DrawString("Hello world", New Font("Microsoft YaHei", 64, FontStyle.Bold), Brushes.Red, New PointF(100, 100))
        End Using

        Using png As Graphics2D = New Size(1200, 500).CreateGDIDevice(Color.White)
            Call png.DrawString("Hello world", New Font("Microsoft YaHei", 64, FontStyle.Bold), Brushes.Red, New PointF(100, 100))
            Call png.ImageResource.SaveAs("./test.png")
        End Using
    End Sub
End Module
