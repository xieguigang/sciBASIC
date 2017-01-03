#Region "Microsoft.VisualBasic::7bc566e5b880a296ac3349efc0366a57, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Test.Project\Class2.vb"

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
Imports Microsoft.VisualBasic.Imaging.Drawing3D

Public Class CubeTest : Inherits GDIDevice

    Protected m_angle As Integer

    Dim cubeModel As New Cube

    Protected Overrides Sub ___animationLoop()
        ' Update the variable after each frame.
        m_angle += 1
    End Sub

    Protected Overrides Sub __init()

    End Sub

    Protected Overrides Sub __updateGraphics(sender As Object, ByRef g As Graphics, region As Rectangle)
        '' Clear the window
        'g.Clear(Color.LightBlue)

        'Dim view As ModelView = cubeModel.Rotate(m_angle, ClientSize, Aixs.All)
        'Call view.UpdateGraphics(g)
    End Sub
End Class
