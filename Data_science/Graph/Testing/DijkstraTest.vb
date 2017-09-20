Imports Microsoft.VisualBasic.Data.Graph

Module DijkstraTest

    Sub Main()
        Dim g As New Graph

        For i As Integer = 10 To 20
            g.AddVertex(label:=i)
        Next

        g.AddEdge(0, 1)
        g.AddEdge(1, 2)
        g.AddEdge(2, 3)
        g.AddEdge(3, 4)
        g.AddEdge(4, 5)
        g.AddEdge(5, 2)
        g.AddEdge(5, 3)
        g.AddEdge(4, 8)
        g.AddEdge(8, 9)
        g.AddEdge(3, 7)
        g.AddEdge(7, 9)

        Dim Dijkstra As New Dijkstra.DijkstraRouteFind(g)
        Dim route = Dijkstra.CalculateMinCost(g.Vertex(0))

        Pause()
    End Sub
End Module
