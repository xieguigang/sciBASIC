Imports System.Drawing
Imports System.Windows.Forms

Namespace Drawing3D.Device

    Public Class Mouse

        Dim WithEvents device As GDIDevice
        Dim _rotate As Boolean
        Dim oldXY As Point

        Sub New(dev As GDIDevice)
            device = dev
        End Sub

        Private Sub device_MouseDown(sender As Object, e As MouseEventArgs) Handles device.MouseDown
            _rotate = True
            oldXY = e.Location
        End Sub

        Private Sub device_MouseMove(sender As Object, e As MouseEventArgs) Handles device.MouseMove
            Dim xy = e.Location

            If Not _rotate Then
                Return
            End If

            If xy.X > oldXY.X Then  ' right
                device._camera.angleY += 1
            End If
            If xy.X < oldXY.X Then ' left
                device._camera.angleY -= 1
            End If
            If xy.Y > oldXY.Y Then ' down
                'device._camera.angleZ -= 1
                device._camera.angleX -= 1
            End If
            If xy.Y < oldXY.Y Then ' up
                'device._camera.angleZ += 1
                device._camera.angleX += 1
            End If

            oldXY = xy
        End Sub

        Private Sub device_MouseUp(sender As Object, e As MouseEventArgs) Handles device.MouseUp
            _rotate = False
        End Sub
    End Class
End Namespace