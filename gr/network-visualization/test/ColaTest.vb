Imports System.Linq
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver

Imports Cola = Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola
Imports ColaNode = Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.Node
Imports ColaLayout = Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.Layout
Imports ColaLink = Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.Link(Of Microsoft.VisualBasic.Data.visualize.Network.Layouts.Cola.Node)
Imports inode = Microsoft.VisualBasic.Data.visualize.Network.Graph.Node

Module ColaTest

    Sub Main()
        Call ImageDriver.Register()

        ' Build a small network graph with a ring topology plus a few chords,
        ' so the stress-minimizing Cola layout has a clear symmetric target.
        Dim g As New NetworkGraph
        Dim labels As String() = {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11"}
        Dim rand As New Random(123)

        For i As Integer = 0 To labels.Length - 1
            ' scatter initial positions so the layout visibly does work
            Dim x = rand.Next(0, 400)
            Dim y = rand.Next(0, 400)

            Call g.AddNode(New inode With {
                .label = labels(i),
                .data = New NodeData With {
                    .initialPostion = New FDGVector2(x, y),
                    .size = {18.0F, 18.0F}
                }
            })
        Next

        ' ring edges
        For i As Integer = 0 To labels.Length - 1
            Dim a = (i + 1) Mod labels.Length
            Call g.AddEdge(labels(i), labels(a))
        Next

        ' a few chords to give the layout structure
        Call g.AddEdge("0", "6")
        Call g.AddEdge("1", "7")
        Call g.AddEdge("3", "9")
        Call g.AddEdge("4", "10")

        ' Bridge NetworkGraph <-> Cola Node/Link
        Dim allNodes As inode() = g.connectedNodes
        Dim nodes(allNodes.Length - 1) As ColaNode

        ' remember the mapping from inode -> cola node index
        Dim indexOf As New Dictionary(Of inode, Integer)

        For i As Integer = 0 To allNodes.Length - 1
            Dim n = allNodes(i)
            Dim p = n.data.initialPostion

            nodes(i) = New ColaNode With {
                .x = p.x,
                .y = p.y,
                .width = n.data.size(0),
                .height = n.data.size(1),
                .index = i
            }
            indexOf(n) = i
        Next

        Dim allEdges As Edge() = g.graphEdges.ToArray()
        Dim links(allEdges.Length - 1) As ColaLink

        For i As Integer = 0 To allEdges.Length - 1
            Dim e = allEdges(i)
            links(i) = New ColaLink With {
                .source = nodes(indexOf(e.U)),
                .target = nodes(indexOf(e.V))
            }
        Next

        ' Run the corrected Cola stress-minimization layout.
        ' symmetricDiffLinkLengths wires up the link-length calculator internally.
        Dim layout As New ColaLayout

        Call layout _
            .size({1000, 1000}) _
            .avoidOverlaps(True) _
            .symmetricDiffLinkLengths(80, 0.7) _
            .convergenceThreshold(0.01) _
            .nodes(nodes) _
            .links(links) _
            .start()

        ' Write the computed positions back into the network graph
        For i As Integer = 0 To nodes.Length - 1
            allNodes(i).data.initialPostion = New FDGVector2(nodes(i).x, nodes(i).y)
        Next

        ' Render the laid-out graph to an image for visual inspection
        Call NetworkVisualizer _
            .DrawImage(g, "1000,1000", displayId:=True, drawEdgeBends:=True, labelerIterations:=-1, minLinkWidth:=8) _
            .Save("./Cola_layout.png")

        Console.WriteLine("Cola layout complete. Output written to ./Cola_layout.png")
    End Sub

End Module
