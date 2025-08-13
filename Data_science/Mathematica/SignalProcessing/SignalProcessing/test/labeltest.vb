Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.ConcaveHull
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.MachineVision.CCL

Public Module labeltest

    Sub Main()
        Dim img = "Z:\aaa.bmp".LoadImage
        Dim CELLS = CCLabeling.Process(BitmapBuffer.FromImage(img), background:=Color.White, 0).ToArray
        Dim pen As New Pen(Color.Red, 2)
        Dim pen2 As New Pen(Color.Blue, 2)
        Dim colors As LoopArray(Of Color) = Designer.GetColors("paper", 100)

        Using gfx As Graphics2D = Graphics2D.CreateDevice(img.Size)
            ' Call gfx.DrawImage(img, New Point)

            For Each item In CELLS
                If item.length > 3 AndAlso item.length < 1000 Then
                    Dim shape = item.AsEnumerable.ConcaveHull
                    Call gfx.DrawPolygon(New Pen(++colors, 2), shape)
                End If
            Next

            Call gfx.Flush()
            Call gfx.ImageResource.SaveAs("Z:/label6.png")
        End Using
    End Sub
End Module
