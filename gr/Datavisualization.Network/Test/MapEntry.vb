Namespace pagerank

    '
    ' * map entry encodes all the nodes m linked with a given node n and corresponding edge weight
    ' 
    Public Class MapEntry

        Public Sub New(identifier As String, weight As Double)
            Me.identifier = identifier
            Me.Weight = weight
        End Sub

        Public Overridable Property Identifier As String
        Public Overridable Property Weight As Double
    End Class

End Namespace