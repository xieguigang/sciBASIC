Imports System.Drawing
Imports Microsoft.VisualBasic.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Math.MachineVision.CCL

Public Module labeltest

    Sub Main()
        Dim img = "Z:\aaa.bmp".LoadImage
        Dim CELLS = CCLabeling.Process(BitmapBuffer.FromImage(img), background:=Color.White, 0)
        Dim pen As New Pen(Color.Red, 2)

        Using gfx As Graphics2D = Graphics2D.CreateDevice(img.Size)
            Call gfx.DrawImage(img, New Point)

            For Each item In CELLS
                Dim rect = item.Value.GetRectangle

                Call gfx.DrawRectangle(pen, rect)
            Next

            Call gfx.Flush()
            Call gfx.ImageResource.SaveAs("Z:/label.png")
        End Using
    End Sub
End Module
