Namespace graphology

    Public Class node

        Public Property id As String
        Public Property label As String
        Public Property x As Double
        Public Property y As Double
        Public Property size As Double
        Public Property color As String

    End Class

    Public Class edge

        Public Property id As String
        Public Property source As String
        Public Property target As String
        Public Property size As Double
        Public Property color As String

    End Class

    Public Class graph

        Public Property nodes As node()
        Public Property edges As edge()

        Sub New()
        End Sub

        Sub New(nodes As IEnumerable(Of node), edges As IEnumerable(Of edge))
            _nodes = nodes.ToArray
            _edges = edges.ToArray
        End Sub

    End Class

End Namespace
