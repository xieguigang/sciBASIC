#Region "Microsoft.VisualBasic::7d6bd68c5ad94bbc7441b4d1f78f0082, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing3D\Models\Shapes\Octahedron.vb"

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

Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D

Namespace Drawing3D.Models.Isometric.Shapes

    ''' <summary>
    ''' Created by fabianterhorst on 02.04.17.
    ''' </summary>
    Public Class Octahedron : Inherits Shape3D

        Public Sub New(origin As Point3D)
            MyBase.New()

            Dim center As Point3D = origin.Translate(0.5, 0.5, 0.5)
            Dim paths As Path3D() = New Path3D(7) {}
            Dim count As Integer = 0
            Dim upperTriangle As Path3D = {
                origin.Translate(0, 0, 0.5),
                origin.Translate(0.5, 0.5, 1),
                origin.Translate(0, 1, 0.5)
            }
            Dim lowerTriangle As Path3D = {
                origin.Translate(0, 0, 0.5),
                origin.Translate(0, 1, 0.5),
                origin.Translate(0.5, 0.5, 0)
            }

            For i As Integer = 0 To 3
                paths(count) = upperTriangle.RotateZ(center, i * Math.PI / 2.0)
                count += 1
                paths(count) = lowerTriangle.RotateZ(center, i * Math.PI / 2.0)
                count += 1
            Next

            MyBase.paths = paths.AsList
            ScalePath3Ds(center, Math.Sqrt(2) / 2.0, Math.Sqrt(2) / 2.0, 1)
        End Sub
    End Class
End Namespace
