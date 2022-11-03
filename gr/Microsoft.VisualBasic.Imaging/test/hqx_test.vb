Imports System.Drawing
Imports System.Drawing.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Drawing2D.HeatMap

Module hqx_test

    Dim sourcePixels As String = "D:\GCModeller\src\R-sharp\test\demo\imageProcessing\hqx\pixel.bmp"

    Sub Main1()
        ' Call bufferTest()
        Call hqx_4()
    End Sub

    Sub hqx_4()
        Dim img As Bitmap = New Bitmap(sourcePixels.LoadImage)
        Dim raster As New RasterScaler(img)

        'Dim bmpData = img.LockBits(New Rectangle(Point.Empty, img.Size), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb)
        'Dim destData = img.LockBits(New Rectangle(Point.Empty, img.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb)
        'Dim sp As IntPtr = destData.Scan0


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
