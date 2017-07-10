Imports Microsoft.VisualBasic.Imaging.Physics
Imports Microsoft.VisualBasic.Language

''' <summary>
''' 
''' </summary>
Public Class Graph

#Region "Let G=(V, E) be a simple graph"
    Dim edges As List(Of Edge)
    Dim vertices As Dictionary(Of Vertex)
#End Region

    Public Function AddEdge(u As Vertex, v As Vertex) As Graph
        edges += New Edge With {
            .U = u,
            .V = v
        }
        If Not vertices.ContainsKey(u.ID) Then
            vertices += u
        End If
        If Not vertices.ContainsKey(v.ID) Then
            vertices += v
        End If

        Return Me
    End Function
End Class

''' <summary>
''' 图之中的节点
''' </summary>
Public Class Vertex : Inherits MassPoint
End Class

''' <summary>
''' 节点之间的边
''' </summary>
Public Class Edge

    Public Property U As Vertex
    Public Property V As Vertex

    Public Overrides Function ToString() As String
        Return $"{U} => {V}"
    End Function
End Class