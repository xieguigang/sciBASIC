#Region "Microsoft.VisualBasic::d865566954cecb016e8a9789c08f7908, Data_science\Visualization\Canvas3D\Device\Mouse.vb"

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

    '     Class Mouse
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: device_MouseDown, device_MouseMove, device_MouseUp
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Drawing3D

Namespace Drawing3D.Device

    Public Class Mouse : Inherits IDevice(Of UserControl)

        Dim press As Boolean

        Dim oldXY As Point
        Dim camera As Camera

        Public Sub New(dev As UserControl, camera As Camera)
            MyBase.New(dev)
            Me.camera = camera
        End Sub

        Private Sub device_MouseDown(sender As Object, e As MouseEventArgs) Handles device.MouseDown
            press = True
            oldXY = e.Location
        End Sub

        Private Sub device_MouseMove(sender As Object, e As MouseEventArgs) Handles device.MouseMove
            Dim xy = e.Location

            If Not press Then
                Return
            End If

            If e.Button = MouseButtons.Left Then

                ' 左键旋转

                If xy.X > oldXY.X Then  ' right
                    camera.angleY += 1
                End If
                If xy.X < oldXY.X Then ' left
                    camera.angleY -= 1
                End If
                If xy.Y > oldXY.Y Then ' down
                    'device._camera.angleZ -= 1
                    camera.angleX -= 1
                End If
                If xy.Y < oldXY.Y Then ' up
                    'device._camera.angleZ += 1
                    camera.angleX += 1
                End If

            ElseIf e.Button = MouseButtons.Right Then

                ' 右键进行位移
                Dim dx = xy.X - oldXY.X
                Dim dy = xy.Y - oldXY.Y

                camera.offset = New PointF With {
                    .X = camera.offset.X + dx,
                    .Y = camera.offset.Y + dy
                }

            Else

            End If

            oldXY = xy
        End Sub

        Private Sub device_MouseUp(sender As Object, e As MouseEventArgs) Handles device.MouseUp
            press = False
        End Sub
    End Class
End Namespace
