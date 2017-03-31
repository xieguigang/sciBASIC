#Region "Microsoft.VisualBasic::4b69ee5c93f9d3b1b7e9ddf72de25096, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing3D\Device\AutoRotation.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

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
