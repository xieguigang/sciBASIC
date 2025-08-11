Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Filters

Module filter_test

    Sub Main()
        Dim img = "C:\Users\Administrator\Downloads\OtsuThresholding-master\ll.jpg".LoadImage
        Dim bitmap = BitmapBuffer.FromImage(img)
        Dim bin = bitmap.ostuFilter

        Call bin.save()
    End Sub
End Module
