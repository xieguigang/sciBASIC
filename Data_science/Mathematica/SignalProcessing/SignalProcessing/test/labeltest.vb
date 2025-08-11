Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Math.MachineVision.CCL

Public Module labeltest

    Sub Main()
        Dim img = "Z:\aaa.bmp".LoadImage
        Dim CELLS = CCLabeling.Process(BitmapBuffer.FromImage(img))

        For Each item In CELLS
            Call item.Value.Save($"Z:/ccl/{item.Key}.bmp")
        Next
    End Sub
End Module
