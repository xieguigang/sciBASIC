#Region "Microsoft.VisualBasic::13e99a2aaa102db3432cf23086cc4a88, gr\network-visualization\Datavisualization.Network\test\endPointTest.vb"

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

    ' Module endPointTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.GraphTheory.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Module endPointTest

    Sub Main()
        Dim g = ExampleNetwork()


        For Each subNet In g.IteratesSubNetworks(Of NetworkGraph)

            Dim endPoints = subNet.EndPoints

            Pause()

        Next

        Pause()
    End Sub
End Module
