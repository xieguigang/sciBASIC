Imports Microsoft.VisualBasic.Data.Graph

Module DijkstraTest

    Sub Main()
        Dim g As New Graph

        For i As Integer = 10 To 20
            g.AddVertex(label:=i)
        Next

        Dim weight As New Random

        g.AddEdge(0, 1, weight.NextDouble)
        g.AddEdge(1, 2, weight.NextDouble)
        g.AddEdge(2, 3, weight.NextDouble)
        g.AddEdge(3, 4, weight.NextDouble)
        g.AddEdge(4, 5, weight.NextDouble)
        g.AddEdge(5, 2, weight.NextDouble)
        g.AddEdge(5, 3, weight.NextDouble)
        g.AddEdge(4, 8, weight.NextDouble)
        g.AddEdge(8, 9, weight.NextDouble)
        g.AddEdge(3, 7, weight.NextDouble)
        g.AddEdge(7, 9, weight.NextDouble)
        g.AddEdge(9, 6, weight.NextDouble)

        Dim Dijkstra As New Dijkstra.DijkstraRouteFind(g)
        Dim route = Dijkstra.CalculateMinCost(g.Vertex(0))

        Pause()
    End Sub
End Module
