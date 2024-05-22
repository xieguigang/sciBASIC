#Region "Microsoft.VisualBasic::94f8e96a57c48d4c94839dfdbb5ec3e1, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Models\Mesh.vb"

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

    '   Total Lines: 30
    '    Code Lines: 22 (73.33%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (26.67%)
    '     File Size: 763 B


    '     Class Mesh
    ' 
    '         Properties: triangles, vertices
    ' 
    '         Sub: Clear, RecalculateBounds, RecalculateNormals, SetTriangles, SetVertices
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Numerics
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.ConcaveHull

Namespace Drawing3D.Models

    Public Class Mesh

        Public ReadOnly Property vertices As Vector3()
        Public ReadOnly Property triangles As Triangle()

        Sub Clear()
            Erase _vertices
            Erase _triangles
        End Sub

        Sub SetVertices(vertices As IEnumerable(Of Vector3))
            _vertices = vertices.ToArray
        End Sub

        Sub SetTriangles(indices As IEnumerable(Of Integer), v As Integer)
            Throw New NotImplementedException()
        End Sub

        Sub RecalculateBounds()
        End Sub

        Sub RecalculateNormals()
        End Sub
    End Class
End Namespace
