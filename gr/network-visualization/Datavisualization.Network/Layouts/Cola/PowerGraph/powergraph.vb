Namespace Layouts.Cola

    Public Class PowerGraph(Of T, G, GroupBase As IGroup(Of G, T))
        Public Property groups As List(Of GroupBase)
        Public Property powerEdges As List(Of PowerEdge(Of T))
    End Class
End Namespace