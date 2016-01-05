Imports Microsoft.VisualBasic.Datavisualization.DocumentFormat.Extensions

Module Program

    Sub Main()
        Dim modellist = "../XC_0077.xml".LoadXml(Of EntrySet)().ExtractNetwork.ToList
        '  Call modellist.AddRange("../XC_1651.xml".LoadXml(Of EntrySet).ExtractNetwork)

        Dim network = New DataVisualization.Network.Network(modellist.ToArray)

        network = Datavisualization.Network.BarycentricMethod.ForceDirectedLayout(network, 80)
        '    network = Microsoft.VisualBasic.Datavisualization.Network.LayoutCreator.ForceDirectedLayout(network, cutoff:=500)

        Dim image As String = My.Computer.FileSystem.SpecialDirectories.Temp & "/test.bmp"
        Call DataVisualization.Network.NetworkVisualizer.DrawImage(network).Save(image)
        Call Process.Start(image)
    End Sub
End Module
