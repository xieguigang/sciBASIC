Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.Data.visualize.Network.Canvas
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts

Module testRenderAVI

    Sub Main()
        Dim graph = CytoscapeExportAsGraph(
            App.HOME & "\Resources\xcb-main-Edges.csv",
            App.HOME & "\Resources\xcb-main-Nodes.csv")

        Dim video = AVI.DoRenderVideo(graph, {1024, 768}, render3D:=True, drawFrames:=512)

        Call video.WriteBuffer("D:\network.avi")
    End Sub
End Module
