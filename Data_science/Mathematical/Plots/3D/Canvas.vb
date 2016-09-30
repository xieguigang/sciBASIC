Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Imaging.Drawing3D

Namespace Plot3D

    Public Class Canvas : Inherits GDIDevice

        Protected Overrides Sub __init()
            data = Evaluate(Function(x, y) x * y, New DoubleRange(-1, 10), New DoubleRange(-10, 10))
        End Sub

        Protected Overrides Sub ___animationLoop()

        End Sub

        Dim data As Point3D()

        Protected Overrides Sub __updateGraphics(sender As Object, g As Graphics, region As Rectangle)
            Dim pSize As New Size(5, 5)

            With camera
                For Each pt In data
                    Call g.FillPie(Brushes.Black, New Rectangle(.Project(.Rotate(pt)).PointXY, pSize), 0, 360)
                Next
            End With
        End Sub

        Private Sub Canvas_RotateCamera(angle As Single) Handles Me.RotateCamera
            camera.angle = angle
        End Sub
    End Class
End Namespace