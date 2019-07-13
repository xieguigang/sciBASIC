#Region "Microsoft.VisualBasic::ee9976bb0048d3f7e7bc264670ea1c51, gr\network-visualization\Datavisualization.Network\test\subnetworkTest.vb"

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

    ' Module subnetworkTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.GraphTheory.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Module subnetworkTest

    Sub Main()
        Dim g = ExampleNetwork()

        Dim subNetworks = g.IteratesSubNetworks(Of NetworkGraph).ToArray

        Pause()
    End Sub
End Module
