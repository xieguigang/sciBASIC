Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace Analysis.Model

    Public Interface EdgeTraversalPolicy
        Function edges(v As DirectedVertex) As ISet(Of Edge)
        Function vertex(e As Edge) As Node
    End Interface

    Friend Class ForwardTraversal : Implements EdgeTraversalPolicy

        Public Sub New()
        End Sub

        Public Overridable Function edges(ByVal v As DirectedVertex) As ISet(Of Edge) Implements EdgeTraversalPolicy.edges
            Return v.outgoingEdges
        End Function

        Public Overridable Function vertex(ByVal e As Edge) As Node Implements EdgeTraversalPolicy.vertex
            Return e.U
        End Function
    End Class

    Friend Class BackwardTraversal : Implements EdgeTraversalPolicy

        Public Sub New()
        End Sub

        Public Overridable Function edges(ByVal v As DirectedVertex) As ISet(Of Edge) Implements EdgeTraversalPolicy.edges
            Return v.incomingEdges
        End Function

        Public Overridable Function vertex(ByVal e As Edge) As Node Implements EdgeTraversalPolicy.vertex
            Return e.V
        End Function
    End Class
End Namespace