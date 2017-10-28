Imports Microsoft.VisualBasic.Data.visualize.Network.FindPath


Module subnetworkTest

    Sub Main()
        Dim g = ExampleNetwork()

        Dim subNetworks = g.IteratesSubNetworks.ToArray

        Pause()
    End Sub
End Module
