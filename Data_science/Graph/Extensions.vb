Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Public Module Extensions

    <Extension>
    Public Function CreateGraph(Of T)(tree As Tree(Of T)) As Graph
        Return New Graph().Add(tree)
    End Function

    <Extension>
    Private Function Add(Of T)(g As Graph, tree As Tree(Of T)) As Graph
        Dim childs = tree _
            .Childs _
            .SafeQuery _
            .Where(Function(c) Not c Is Nothing)

        Call g.AddVertex(tree)

        For Each child As Tree(Of T) In childs
            Call g.Add(child)
            Call g.AddEdge(tree, child)
        Next

        Return g
    End Function

    ''' <summary>
    ''' Swap the location of <see cref="Edge.U"/> and <see cref="Edge.V"/> in <paramref name="edge"/>.
    ''' </summary>
    ''' <param name="edge"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Reverse(edge As Edge) As Edge
        Return New Edge With {
            .U = edge.V,
            .V = edge.U,
            .Weight = edge.Weight
        }
    End Function
End Module
