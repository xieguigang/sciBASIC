#Region "Microsoft.VisualBasic::9e14a4dc536b4bb4c499f16e53b69d5d, gr\network-visualization\NetworkCanvas\Canvas3D\Input3D.vb"

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

    ' Class Input3D
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: Canvas_MouseDown, Canvas_MouseMove, Canvas_MouseUp
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Drawing3D

''' <summary>
''' Mouse input device in 3D space
''' </summary>
Public Class Input3D : Inherits InputDevice

    Sub New(canvas As Canvas)
        Call MyBase.New(canvas)
        renderer = DirectCast(canvas.fdgRenderer, Renderer3D)
    End Sub

    Dim renderer As Renderer3D
    Dim usrCursor, location As Point

    Protected Overrides Sub Canvas_MouseDown(sender As Object, e As MouseEventArgs)
        drag = True
        usrCursor = e.Location
    End Sub

    Protected Overrides Sub Canvas_MouseMove(sender As Object, e As MouseEventArgs)
        If Not drag Then
            Return
        End If

        renderer.rotate = renderer.rotate + (-usrCursor.X + e.X) / 100
    End Sub

    Protected Overrides Sub Canvas_MouseUp(sender As Object, e As MouseEventArgs)
        drag = False
    End Sub

    Dim vd As Integer

    '    Protected Overrides Sub Canvas_MouseWheel(sender As Object, e As MouseEventArgs)
    '        vd += e.Delta / 200

    '        Dim rect = renderer.ClientRegion

    '        For Each node In Canvas.Graph.nodes
    '            Dim pos As AbstractVector = Canvas.fdgPhysics.GetPoint(node).position
    '            Call Point3D.Project(pos.x, pos.y, pos.z, rect.Width, rect.Height, 1, vd)
    '#If DEBUG Then
    '            Call pos.__DEBUG_ECHO
    '#End If
    '        Next
    '    End Sub
End Class
