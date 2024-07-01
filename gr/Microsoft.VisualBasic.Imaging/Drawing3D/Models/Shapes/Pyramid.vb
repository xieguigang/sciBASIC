#Region "Microsoft.VisualBasic::676c5f023c4bc8e1645090a7f18b94be, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Models\Shapes\Pyramid.vb"

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

    '   Total Lines: 41
    '    Code Lines: 28 (68.29%)
    ' Comment Lines: 6 (14.63%)
    '    - Xml Docs: 50.00%
    ' 
    '   Blank Lines: 7 (17.07%)
    '     File Size: 1.48 KB


    '     Class Pyramid
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports std = System.Math

Namespace Drawing3D.Models.Isometric.Shapes

    ''' <summary>
    ''' Created by fabianterhorst on 02.04.17.
    ''' </summary>
    Public Class Pyramid : Inherits Shape3D

        Public Sub New(origin As Point3D)
            Me.New(origin, 1, 1, 1)
        End Sub

        Public Sub New(origin As Point3D, dx As Double, dy As Double, dz As Double)
            MyBase.New()

            ' Path parallel to the x-axis 
            Dim face1 As Path3D = {
                origin,
                New Point3D(origin.X + dx, origin.Y, origin.Z),
                New Point3D(origin.X + dx / 2.0, origin.Y + dy / 2.0, origin.Z + dz)
            }
            Dim paths As Path3D() = New Path3D(3) {}
            ' Push the face, and its opposite face, by rotating around the Z-axis 
            paths(0) = face1
            paths(1) = face1.RotateZ(origin.Translate(dx / 2.0, dy / 2.0, 0), std.PI)

            ' Path parallel to the y-axis 
            Dim face2 As Path3D = {
                origin,
                New Point3D(origin.X + dx / 2, origin.Y + dy / 2, origin.Z + dz),
                New Point3D(origin.X, origin.Y + dy, origin.Z)
            }
            paths(2) = face2
            paths(3) = face2.RotateZ(origin.Translate(dx / 2.0, dy / 2.0, 0), std.PI)

            MyBase.paths = paths.AsList
        End Sub
    End Class
End Namespace
