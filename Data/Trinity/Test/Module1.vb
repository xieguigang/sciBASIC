Imports Microsoft.VisualBasic.Data.NLP
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Serialization.JSON

Module Module1

    Sub Main()
        Dim s = "
This module implements TextRank, an unsupervised keyword
significance scoring algorithm. TextRank builds a weighted
graph representation Of a document Using words As nodes
And coocurrence frequencies between pairs of words as edge 
weights.It then applies PageRank to this graph, And
treats the PageRank score of each word as its significance.
The original research paper proposing this algorithm Is
available here"


        s = "the important pagerank. show on pagerank. have significance pagerank. implements pagerank algorithm. textrank base on pagerank."

        Dim ps = TextRank.Sentences(s.TrimNewLine)
        Dim g = ps.TextGraph
        Dim pr As New PageRank(g)
        Dim result = g.TranslateVector(pr.ComputePageRank, True)

        Call result.GetJson(True).EchoLine

        Pause()
    End Sub
End Module
