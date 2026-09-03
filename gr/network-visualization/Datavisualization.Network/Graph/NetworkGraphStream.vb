Namespace Graph

    Public Class NetworkGraphStream

        Public Property id As String
        Public Property name As String

        Dim nodeSet As New Dictionary(Of String, Node)
        Dim createEdgeSet As Func(Of IEnumerable(Of Edge))

        Public ReadOnly Property vertex As IEnumerable(Of Node)
            Get
                Return nodeSet.Values
            End Get
        End Property

        Public ReadOnly Property graphEdges As IEnumerable(Of Edge)
            Get
                Return createEdgeSet()
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

        Public Function SetEdgeStream(stream As Func(Of IEnumerable(Of Edge))) As NetworkGraphStream
            createEdgeSet = stream
            Return Me
        End Function

        Public Function MakeGraph() As NetworkGraph
            Dim g As New NetworkGraph

            For Each node As Node In vertex
                Call g.AddNode(node, assignId:=False)
            Next

            For Each link As Edge In createEdgeSet()
                Call g.CreateEdge(link.U, link.V, link.weight, link.data)
            Next

            Return g
        End Function

    End Class
End Namespace