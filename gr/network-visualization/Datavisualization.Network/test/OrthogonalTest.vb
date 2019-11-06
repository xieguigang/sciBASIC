Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports node = Microsoft.VisualBasic.Data.visualize.Network.Graph.Node

Module OrthogonalTest

    Sub Main()
        Dim g As New NetworkGraph

        For Each label As String In {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K"}
            Call g.AddNode(New node With {.label = label, .data = New NodeData With {.initialPostion = New FDGVector2, .size = {5, 5}}})
        Next

        Call g.AddEdge("A", "B")
        Call g.AddEdge("B", "C")
        Call g.AddEdge("C", "D")
        Call g.AddEdge("D", "E")
        Call g.AddEdge("C", "E")
        Call g.AddEdge("A", "E")
        Call g.AddEdge("A", "I")
        Call g.AddEdge("A", "J")
        Call g.AddEdge("J", "K")
        Call g.AddEdge("K", "H")
        Call g.AddEdge("F", "G")
        Call g.AddEdge("B", "F")
        Call g.AddEdge("G", "K")

        Call Orthogonal.Algorithm.DoLayout(g, New Size(10, 10), 10)

        Dim result = g.Tabular()

        Call result.Save("./OrthogonalTest")

        Pause()
    End Sub
End Module
