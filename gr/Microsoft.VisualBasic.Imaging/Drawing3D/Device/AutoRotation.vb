#Region "Microsoft.VisualBasic::9f0de6203bc1c5185c3342353d98db44, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Device\AutoRotation.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class AutoRotation
    ' 
    '         Properties: X, Y, Z
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: Reset, RunRotate, Tick
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing3D.Device

    Public Class AutoRotation : Inherits IDevice(Of GDIDevice)

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
