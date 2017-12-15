Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Module visualEffectsTest
    Sub Main()
        Dim g As New NetworkGraph
        Dim nodes As New Dictionary(Of String, visualize.Network.Graph.Node)

        For i As Integer = 0 To 10
            Call nodes.Add(i, g.AddNode(New visualize.Network.Graph.Node With {.ID = i, .Label = "#" & i}))
        Next

        Call g.AddEdge(0, 1)
    End Sub
End Module
