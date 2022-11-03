Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Drawing2D.HeatMap

Module hqx_test

    Dim sourcePixels As String = "D:\GCModeller\src\R-sharp\test\demo\imageProcessing\hqx\tumblr_lqb1p7EIoF1qcsurn.png"

    Sub Main1()
        ' Call bufferTest()
        Call hqx_4()
    End Sub

    Sub hqx_4()
        Dim img As Bitmap = New Bitmap(sourcePixels.LoadImage)
        Dim raster As New RasterScaler(img)

        Call raster.Scale(hqx:=hqx.HqxScales.Hqx_4x).SaveAs($"{App.HOME}/pixels_hqx_4x.png")

        Pause()
    End Sub

    Sub bufferTest()
        Dim img As Bitmap = New Bitmap(sourcePixels.LoadImage)
        Dim blank As New Bitmap(img.Width, img.Height)
        Dim buffer As BitmapBuffer = BitmapBuffer.FromBitmap(img)
        Dim bytes = buffer.GetARGBStream

        Dim anotherBlank As BitmapBuffer = BitmapBuffer.FromBitmap(blank)

        Call anotherBlank.WriteARGBStream(bytes)
        Call anotherBlank.Dispose()

        Call blank.SaveAs($"{App.HOME}/pixels.png")
    End Sub

End Module
