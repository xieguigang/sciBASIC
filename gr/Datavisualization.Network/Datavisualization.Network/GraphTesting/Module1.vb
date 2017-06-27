Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Module Module1

    Sub Main()

        Dim graph = GraphAPI.CytoscapeExportAsGraph(
        "D:\GCModeller\src\runtime\sciBASIC#\gr\Datavisualization.Network\net_test\xcb-main-Edges.csv",
        "D:\GCModeller\src\runtime\sciBASIC#\gr\Datavisualization.Network\net_test\xcb-main-Nodes.csv")

        Dim treeModel As New GraphTree(graph)

    End Sub
End Module
