Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.BitmapImage

Module hqx_test

    Dim sourcePixels As String = "D:\GCModeller\src\R-sharp\test\demo\imageProcessing\hqx\tumblr_lqb1p7EIoF1qcsurn.png"

    Sub Main1()
        Call bufferTest()


    End Sub

    Sub bufferTest()
        Dim img As Bitmap = New Bitmap(sourcePixels.LoadImage)
        Dim buffer As BitmapBuffer = BitmapBuffer.FromBitmap(img)
        Dim bytes = buffer.GetARGBStream

        Call buffer.WriteARGBStream(bytes)
        Call buffer.Dispose()

        Call img.SaveAs($"{App.HOME}/pixels.png")
    End Sub

End Module
