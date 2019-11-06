Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace Layouts.Orthogonal

    Friend Class Workspace

        Public g As NetworkGraph
        Public grid As Grid
        Public V As Node()
        ''' <summary>
        ''' c
        ''' </summary>
        Public cellSize As Double
        Public delta As Double

        Public width As Dictionary(Of String, Double)
        Public height As Dictionary(Of String, Double)

        Public ReadOnly Property totalEdgeLength As Double
            Get
                Dim len As Double

                For Each edge As Edge In g.graphEdges
                    len += distance(edge.U, edge.V, cellSize, delta)
                Next

                Return len
            End Get
        End Property
    End Class
End Namespace