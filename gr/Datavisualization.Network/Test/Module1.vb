Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Serialization.JSON

Module Module1

    Sub Main()
        Dim g As New Network

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

        Pause()
    End Sub
End Module
