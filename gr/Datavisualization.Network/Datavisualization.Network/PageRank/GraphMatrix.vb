Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class GraphMatrix

    Dim indices As New Dictionary(Of String, List(Of Integer))
    Dim nodes As FileStream.Node()
    Dim edges As NetworkEdge()

    Sub New(net As FileStream.Network)
        nodes = net.Nodes
        edges = net.Edges

        Dim index As New IndexOf(Of String)(
            nodes.Select(Function(x) x.Identifier))

        For Each node As FileStream.Node In nodes
            Call indices.Add(
                node.Identifier,
                New List(Of Integer))
        Next

        For Each edge As NetworkEdge In edges
            Call indices(edge.FromNode) _
                .Add(index(edge.ToNode))
        Next

        For Each k In indices.Keys.ToArray
            indices(k) = indices(k).Distinct.ToList
        Next
    End Sub

    Sub New(g As NetworkGraph)
        Call Me.New(g.Tabular)
    End Sub

    Public Function TranslateVector(v#(), Optional reorder As Boolean = False) As Dictionary(Of String, Double)
        If Not reorder Then
            Return nodes _
                .SeqIterator _
                .ToDictionary(Function(n) (+n).Identifier,
                              Function(i) v(i))
        Else
            Dim orders As SeqValue(Of Double)() = v _
                .SeqIterator _
                .OrderByDescending(Function(x) x.value) _
                .ToArray

            Return orders.ToDictionary(
                Function(i) nodes(i).Identifier,
                Function(value) +value)
        End If
    End Function

    Public Overrides Function ToString() As String
        Return indices.GetJson
    End Function

    Public Shared Narrowing Operator CType(gm As GraphMatrix) As List(Of Integer)()
        Return gm.nodes.ToArray(Function(k) gm.indices(k.Identifier))
    End Operator
End Class
