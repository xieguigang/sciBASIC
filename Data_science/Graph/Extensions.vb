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
        Call g.AddVertex(tree)

        For Each child As Tree(Of T) In tree.Childs.SafeQuery
            Call g.Add(child)
            Call g.AddEdge(tree, child)
        Next

        Return g
    End Function
End Module
