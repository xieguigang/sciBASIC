#Region "Microsoft.VisualBasic::15ecd036f512fb986943c5906afefaab, sciBASIC#\Data_science\Visualization\Canvas3D\Device\AutoRotation.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 44
    '    Code Lines: 34
    ' Comment Lines: 0
    '   Blank Lines: 10
    '     File Size: 1.07 KB


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

Imports Microsoft.VisualBasic.Imaging.Drawing3D
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
