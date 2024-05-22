Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Expressions

Namespace MinimumSpanningTree

    ''' <summary>
    ''' Minimum spanning tree via Kruskal algorithm
    ''' </summary>
    Public Class Kruskal

        Private djset As New DisjointSet()
        Private edge As New List(Of VertexEdge)
        Private numNodes As Integer

        Public Sub New(edges As IEnumerable(Of VertexEdge))
            edge = edges.AsList
            edge.Sort(Function(e1, e2)
                          If e1.weight > e2.weight Then Return 1
                          If e1.weight < e2.weight Then Return -1
                          Return 0
                      End Function)

            numNodes = calculateNumNodes(edge)
        End Sub

        Public Function calculateNumNodes(edges As IEnumerable(Of VertexEdge)) As Integer
            Return (From i In edges.AsEnumerable Select NodeIdTuple(i)).IteratesALL.Distinct.Count
        End Function

        Private Iterator Function NodeIdTuple(i As VertexEdge) As IEnumerable(Of Integer)
            Yield i.U.ID
            Yield i.V.ID
        End Function

        Public Iterator Function findMinTree() As IEnumerable(Of VertexEdge)
            Dim i = 1

            While i <= numNodes
                djset.makeset(i)
                i += 1
            End While

            Dim count = 0
            Dim idx = 0

            While count < numNodes - 1
                If djset.findset(edge(idx).V.ID) <> djset.findset(edge(idx).U.ID) Then
                    Yield edge(idx)

                    count += 1
                    djset.union(edge(idx).V.ID, edge(idx).U.ID)
                End If

                idx += 1
            End While
        End Function
    End Class
End Namespace
