Imports System.Drawing
Imports Microsoft.VisualBasic.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.MachineVision.CCL

Public Module labeltest

    Sub Main()
        Dim img = "Z:\Untitled.bmp".LoadImage
        Dim CELLS = CCLabeling.TwoPassProcess(BitmapBuffer.FromImage(img), background:=Color.White, 0).ToArray
        Dim pen As New Pen(Color.Red, 2)
        Dim pen2 As New Pen(Color.Blue, 2)

        Using gfx As Graphics2D = Graphics2D.CreateDevice(img.Size)
            Call gfx.DrawImage(img, New Point)

            For Each item In CELLS
                Dim rect = item.GetRectangle

                Call gfx.DrawRectangle(pen, rect)
                Call gfx.DrawPolygon(pen2, item.AsEnumerable.ToArray)
            Next

            Call gfx.Flush()
            Call gfx.ImageResource.SaveAs("Z:/label5.png")
        End Using
    End Sub
End Module
