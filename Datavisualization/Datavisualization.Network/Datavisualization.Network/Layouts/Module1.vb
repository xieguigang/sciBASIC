Module Module1

    Public Function LayoutNetwork(Network As Network, Optional startNode As Integer = -1, Optional _DEBUG_EXPORT As String = "") As Network
        Dim Nodes = Network.Nodes
        If startNode = -1 Then

        End If
        Throw New NotImplementedException
    End Function

    Private Function GetWeight(Node As Node, Network As Network) As Double
        Dim value As Double = Node.Degree
        For Each p In Node.Neighbours
            value += Network(p).Degree
        Next
        Return value
    End Function
End Module
