Namespace Layouts.Cola

    Public Class PowerGraph
        Public Property groups As List(Of Group)
        Public Property powerEdges As List(Of PowerEdge(Of Node))
    End Class

    Public Class IndexPowerGraph
        Public Property groups As List(Of IndexGroup)
        Public Property powerEdges As List(Of PowerEdge(Of Node))
    End Class
End Namespace