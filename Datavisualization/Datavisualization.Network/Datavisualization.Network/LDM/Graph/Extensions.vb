Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataVisualization.Network.Abstract

Namespace Graph

    Public Module Extensions

        <Extension>
        Public Sub ApplyAnalysis(ByRef net As NetworkGraph)

            For Each node In net.nodes
                node.Data.Neighbours = net.GetNeighbours(node.ID).ToArray
            Next
        End Sub

        <Extension>
        Public Iterator Function GetNeighbours(net As NetworkGraph, node As String) As IEnumerable(Of Integer)
            For Each edge As Edge In net.edges
                Dim connected As String = edge.GetConnectedNode(node)
                If Not String.IsNullOrEmpty(connected) Then
                    Yield CInt(connected)
                End If
            Next
        End Function
    End Module
End Namespace