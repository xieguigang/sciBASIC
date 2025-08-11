Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Filters

Module filter_test

    Sub Main()
        Dim img = "Z:\Capture.PNG".LoadImage
        Dim bitmap = BitmapBuffer.FromImage(img)
        Dim bin = bitmap.ostuFilter(flip:=True)

        Call bin.Save("Z:/aaa.bmp")
    End Sub
End Module
