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
End Module
