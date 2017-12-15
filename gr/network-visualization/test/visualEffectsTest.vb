Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts

Module visualEffectsTest
    Sub Main()
        Dim g As New NetworkGraph
        Dim nodes As New Dictionary(Of String, visualize.Network.Graph.Node)

        For i As Integer = 0 To 10
            Call nodes.Add(i, g.AddNode(New visualize.Network.Graph.Node With {.ID = i, .Label = "#" & i, .Data = New NodeData With {.Properties = New Dictionary(Of String, String)}}))
        Next

        Call g.AddEdge(0, 1)
        Call g.AddEdge(1, 2)
        Call g.AddEdge(2, 3)
        Call g.AddEdge(2, 4)
        Call g.AddEdge(2, 5)
        Call g.AddEdge(7, 8)
        Call g.AddEdge(8, 9)
        Call g.AddEdge(6, 7)
        Call g.AddEdge(7, 0)
        Call g.AddEdge(7, 10)

        Call g.doForceLayout
        Call g.ComputeNodeDegrees
        Call g.DrawImage("2000,2000", scale:=3.5, radiusScale:=5, fontSizeFactor:=5).Save("./test.png")
    End Sub
End Module
