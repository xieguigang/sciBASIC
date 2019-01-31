Namespace Layouts.Cola

    Public Class PowerGraph(Of T)
        Public Property groups As List(Of IndexGroup)
        Public Property powerEdges As List(Of PowerEdge(Of T))
    End Class
End Namespace