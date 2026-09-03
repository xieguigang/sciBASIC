Namespace Graph

    Public Class NetworkGraphStream

        Public Property id As String
        Public Property name As String

        Dim nodeSet As New Dictionary(Of String, Node)

        Public ReadOnly Property vertex As IEnumerable(Of Node)
            Get
                Return nodeSet.Values
            End Get
        End Property

        Public Function CreateNode(id As String, nodedata As NodeData) As Node
            nodeSet.Add(id, New Node With {.ID = nodeSet.Count + 1, .data = nodedata, .label = id})
            Return nodeSet(id)
        End Function

        Public Function GetElementById(id As String) As Node
            If nodeSet.ContainsKey(id) Then
                Return nodeSet(id)
            Else
                Return Nothing
            End If
        End Function


    End Class
End Namespace