Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing3D.Device

    Public Class AutoRotation
        Inherits IDevice

        Public Property X! = 1
        Public Property Y! = 1
        Public Property Z! = 1

        Public Sub New(dev As GDIDevice)
            MyBase.New(dev)
        End Sub

        Sub Tick()
            If Not device.AutoRotation Then
                Return
            End If

            Call RunRotate()
        End Sub

        Public Sub RunRotate()
            Dim camera As Camera = device._camera

            camera.angleX += X
            camera.angleY += Y
            camera.angleZ += Z
        End Sub

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Sub Reset()
            With device._camera
                .angleX = -90
                .angleY = 30
                .angleZ = 0
            End With
        End Sub
    End Class
End Namespace