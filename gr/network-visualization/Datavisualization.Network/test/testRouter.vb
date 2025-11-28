Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis.Dijkstra
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Module testRouter

    Sub Main()
        Dim g As New NetworkGraph

        For Each c As String In {"a", "b", "c", "d", "e", "f", "g", "h"}
            Call g.CreateNode(c)
        Next

        Call g.CreateEdge("a", "b", 1)
        Call g.CreateEdge("b", "c", 1)
        Call g.CreateEdge("c", "e", 1)
        Call g.CreateEdge("d", "e", 1)
        Call g.CreateEdge("e", "g", 1)
        Call g.CreateEdge("g", "f", 1)
        Call g.CreateEdge("f", "a", 1)
        Call g.CreateEdge("f", "h", 1)
        Call g.CreateEdge("h", "a", 1)

        Dim router As DijkstraRouter = DijkstraRouter.FromNetwork(g)
        Dim path = router.CalculateMinCost(router.GetLocation("a"), router.GetLocation("h"))

        Pause()
    End Sub
End Module
