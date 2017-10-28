Imports Microsoft.VisualBasic.Data.visualize.Network.FindPath

Module endPointTest

    Sub Main()
        Dim g = ExampleNetwork()


        For Each subNet In g.IteratesSubNetworks

            Dim endPoints = subNet.EndPoints

            Pause()

        Next

        Pause()
    End Sub
End Module
