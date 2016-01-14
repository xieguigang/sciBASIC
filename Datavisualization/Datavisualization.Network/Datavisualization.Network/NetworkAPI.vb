Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DocumentFormat.Csv.Extensions
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports ______NETWORK__ = Microsoft.VisualBasic.DataVisualization.Network.FileStream.Network(Of
    Microsoft.VisualBasic.DataVisualization.Network.FileStream.Node,
    Microsoft.VisualBasic.DataVisualization.Network.FileStream.NetworkNode)

<[PackageNamespace]("DataVisualization.Network", Publisher:="xie.guigang@gmail.com")>
Public Module NetworkAPI

    <ExportAPI("Read.Network")>
    Public Function ReadnetWork(file As String) As FileStream.NetworkNode()
        Return file.LoadCsv(Of FileStream.NetworkNode)(False).ToArray
    End Function

    <ExportAPI("Find.NewSession")>
    Public Function CreatePathwayFinder(Network As Generic.IEnumerable(Of FileStream.NetworkNode)) As PathFinder(Of FileStream.NetworkNode)
        Return New PathFinder(Of FileStream.NetworkNode)(Network.ToArray)
    End Function

    <ExportAPI("Find.Path.Shortest")>
    Public Function FindShortestPath(finder As PathFinder(Of FileStream.NetworkNode), start As String, ends As String) As FileStream.NetworkNode()
        Dim ChunkBuffer = finder.FindShortestPath(start, ends)
        Dim List As List(Of FileStream.NetworkNode) = New List(Of FileStream.NetworkNode)
        For Each Line In ChunkBuffer
            Call List.AddRange(Line.Value)
        Next
        Return List.ToArray
    End Function

    <ExportAPI("Get.NetworkEdges")>
    Public Function GetNHetworkEdges(Network As ______NETWORK__) As Microsoft.VisualBasic.DataVisualization.Network.FileStream.NetworkNode()
        Return Network.Edges
    End Function

    <ExportAPI("Get.NetworkNodes")>
    Public Function GetNetworkNodes(Network As ______NETWORK__) As Microsoft.VisualBasic.DataVisualization.Network.FileStream.Node()
        Return Network.Nodes
    End Function

    <ExportAPI("Save")>
    Public Function SaveNetwork(network As ______NETWORK__, <Parameter("DIR.Export")> Export As String) As Boolean
        Return network.Save(Export, Encodings.UTF8)
    End Function

    <ExportAPI("Write.Network")>
    Public Function WriteNetwork(Network As FileStream.NetworkNode(), <Parameter("Path.Save")> SaveTo As String) As Boolean
        Call Network.SaveTo(SaveTo, False)
        Return True
    End Function
End Module
