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