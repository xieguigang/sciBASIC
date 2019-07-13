#Region "Microsoft.VisualBasic::345fbff285748351ce8c5ff011f25344, gr\network-visualization\Datavisualization.Network\test\Module1.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module Module1
    ' 
    '     Function: ExampleNetwork
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Linq

Module Module1

    Sub Main()

        Dim graph = GraphAPI.CytoscapeExportAsGraph(
        "D:\GCModeller\src\runtime\sciBASIC#\gr\Datavisualization.Network\net_test\xcb-main-Edges.csv",
        "D:\GCModeller\src\runtime\sciBASIC#\gr\Datavisualization.Network\net_test\xcb-main-Nodes.csv")

        Dim treeModel As New GraphTree(graph)

        Call graph.ToString.__DEBUG_ECHO
        Call treeModel.ToString.__DEBUG_ECHO

        Pause()

    End Sub

    Function ExampleNetwork() As NetworkGraph
        Dim graph As New NetworkGraph
        Dim nodes = 20&.Sequence.Select(Function(x) New Graph.Node With {.ID = "#" & x}).ToArray

        nodes.DoEach(Function(node) graph.AddNode(node))

        Dim add = Sub(i%, j%)
                      graph.AddEdge(New Edge With {.U = nodes(i), .V = nodes(j), .ID = $"{i} --> {j}"})
                  End Sub

        ' sub 1
        add(0, 1)
        add(0, 2)
        add(0, 3)
        add(0, 4)
        add(6, 1)
        add(7, 1)
        add(5, 6)

        ' sub 2
        add(8, 9)
        add(8, 10)
        add(8, 11)
        add(12, 11)
        add(12, 13)
        add(13, 8)

        ' sub 3
        add(14, 15)
        add(15, 16)
        add(15, 17)
        add(15, 18)
        add(16, 19)

        Return graph
    End Function
End Module
