#Region "Microsoft.VisualBasic::9c8842e10b589bb981836a7bb2dfd69b, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing3D\Models\Shapes\Prism.vb"

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

Namespace Drawing3D.Models.Isometric.Shapes

    ''' <summary>
    ''' Created by fabianterhorst on 01.04.17.
    ''' (立方体)
    ''' </summary>
    Public Class Prism : Inherits Shape3D

        ''' <summary>
        ''' x,y,z的边长分别为1
        ''' </summary>
        ''' <param name="origin"></param>
        Public Sub New(origin As Point3D)
            Me.New(origin, 1, 1, 1)
        End Sub

        Public Sub New(origin As Point3D, dx As Double, dy As Double, dz As Double)
            MyBase.New()

            Dim paths As Path3D() = New Path3D(5) {}

            ' Squares parallel to the x-axis 
            Dim face1 As Path3D = {
                origin,
                New Point3D(origin.X + dx, origin.Y, origin.Z),
                New Point3D(origin.X + dx, origin.Y, origin.Z + dz),
                New Point3D(origin.X, origin.Y, origin.Z + dz)
            }

            ' Push this face and its opposite 
            paths(0) = face1
            paths(1) = face1.Reverse().TranslatePoints(0, dy, 0)

            ' Square parallel to the y-axis 
            Dim face2 As Path3D = {
                origin,
                New Point3D(origin.X, origin.Y, origin.Z + dz),
                New Point3D(origin.X, origin.Y + dy, origin.Z + dz),
                New Point3D(origin.X, origin.Y + dy, origin.Z)
            }
            paths(2) = face2
            paths(3) = face2.Reverse().TranslatePoints(dx, 0, 0)

            ' Square parallel to the xy-plane 
            Dim face3 As Path3D = {
                origin,
                New Point3D(origin.X + dx, origin.Y, origin.Z),
                New Point3D(origin.X + dx, origin.Y + dy, origin.Z),
                New Point3D(origin.X, origin.Y + dy, origin.Z)
            }
            ' This surface is oriented backwards, so we need to reverse the points 
            paths(4) = face3.Reverse()
            paths(5) = face3.TranslatePoints(0, 0, dz)

            MyBase.paths = paths.AsList
        End Sub
    End Class
End Namespace
