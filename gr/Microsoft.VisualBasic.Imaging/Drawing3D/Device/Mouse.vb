#Region "Microsoft.VisualBasic::1cf41f314fb8685eb4e6c1f1c9ee4bbf, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing3D\Device\Mouse.vb"

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
