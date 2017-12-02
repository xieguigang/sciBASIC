Imports Microsoft.VisualBasic.Data.Graph
Imports Microsoft.VisualBasic.Data.Graph.Analysis.PageRank
Imports Microsoft.VisualBasic.Serialization.JSON

Module test

    Sub Main()

        Call "PageRank:".__DEBUG_ECHO
        Call TestPageRank()
        Call "Weighted PageRank:".__DEBUG_ECHO
        Call Test_weightedPR()

        Pause()
    End Sub

    Sub TestPageRank()
        Dim g As New Graph

        For Each label In "ABCDEFGHIJK"
            Call g.AddVertex(CStr(label))
        Next

        Call g.AddEdges("B", {"C"})
        Call g.AddEdges("C", {"B"})
        Call g.AddEdges("D", {"A", "B"})
        Call g.AddEdges("E", {"D", "B", "F"})
        Call g.AddEdges("F", {"E", "B"})
        Call g.AddEdges("G", {"E", "B"})
        Call g.AddEdges("H", {"E", "B"})
        Call g.AddEdges("I", {"E", "B"})
        Call g.AddEdges("J", {"E"})
        Call g.AddEdges("K", {"E"})

        Dim matrix As New GraphMatrix(g)
        Dim pr As New PageRank(matrix)

        Dim result = matrix.TranslateVector(pr.ComputePageRank)

        Call result.GetJson(True).EchoLine
    End Sub

    Sub Test_weightedPR()
        Dim graph As New WeightedPRGraph

        graph.AddEdge(1, 2, 1.0)
        graph.AddEdge(1, 3, 2.0)
        graph.AddEdge(2, 3, 3.0)
        graph.AddEdge(2, 4, 4.0)
        graph.AddEdge(3, 1, 5.0)

        Dim ranks = graph.Rank(0.85, 0.000001)

        Call ranks.GetJson(True).EchoLine
    End Sub
End Module
