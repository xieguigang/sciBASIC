#Region "Microsoft.VisualBasic::3e7871b0e81604afaec1610632d7deed, gr\network-visualization\test\drawTest.vb"

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

    ' Module drawTest
    ' 
    '     Function: DrawTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Data.visualize.Network

Module drawTest

    Sub Main()

        'Call DrawTest()
        'Call Pause()

        Dim graph = CytoscapeExportAsGraph("C:\Users\xieguigang\Source\Repos\sciBASIC\gr\Datavisualization.Network\net_test\xcb-main-Edges.csv", "C:\Users\xieguigang\Source\Repos\sciBASIC\gr\Datavisualization.Network\net_test\xcb-main-Nodes.csv")
        Call graph.doForceLayout(iterations:=100, showProgress:=True)
        Call graph.Tabular.Save("./")
        Call graph.DrawImage("2000,2000", scale:=3.5).Save("./test.png")
        Call vbnet.Save(graph.Tabular, "./network.vbnet")
    End Sub

    Private Function DrawTest()
        Dim net = vbnet.Load("C:\Users\xieguigang\Source\Repos\sciBASIC\gr\Datavisualization.Network\ModelTest\ModelTest\bin\Debug\network.vbnet")
        Dim graph = net.CreateGraph
        Call graph.DrawImage("2000,2000", scale:=3).Save("./test.png")
    End Function
End Module
