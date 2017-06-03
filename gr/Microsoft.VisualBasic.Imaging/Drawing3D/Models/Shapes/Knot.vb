#Region "Microsoft.VisualBasic::c790746d32f19a70f57d8cc9e7ef8c9b, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing3D\Models\Shapes\Knot.vb"

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

    Public Class Knot : Inherits Shape3D

        Public Sub New(origin As Point3D)
            Push((New Prism(Math3D.ORIGIN, 5, 1, 1)).paths)
            Push((New Prism(New Point3D(4, 1, 0), 1, 4, 1)).paths)
            Push((New Prism(New Point3D(4, 4, -2), 1, 1, 3)).paths)
            Push(New Path3D(New Point3D() {New Point3D(0, 0, 2), New Point3D(0, 0, 1), New Point3D(1, 0, 1), New Point3D(1, 0, 2)}))
            Push(New Path3D(New Point3D() {New Point3D(0, 0, 2), New Point3D(0, 1, 2), New Point3D(0, 1, 1), New Point3D(0, 0, 1)}))
            ScalePath3Ds(Math3D.ORIGIN, 1.0 / 5.0)
            TranslatePath3Ds(-0.1, 0.15, 0.4)
            TranslatePath3Ds(origin.X, origin.Y, origin.Z)
        End Sub
    End Class
End Namespace
