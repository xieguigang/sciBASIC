Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DocumentFormat.Csv
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Dijkstra

    <[PackageNamespace]("Path.Find.Dijkstra",
                        Publisher:="Michael Demeersseman",
                        Category:=APICategories.UtilityTools,
                        Description:="Calculation of the shortest path between x points",
                        Url:="http://www.codeproject.com/Articles/22647/Dijkstra-Shortest-Route-Calculation-Object-Oriente")>
    Public Module DijkstraAPI

        <ExportAPI("Network.Imports")>
        Public Function ImportsNetwork(net As IEnumerable(Of FileStream.NetworkEdge)) As Connection()
            Dim LQuery = (From edge In net Select Connection.CreateObject(edge)).ToArray
            Return LQuery
        End Function

        <ExportAPI("Read.Network")>
        Public Function ReadNetwork(path As String) As Connection()
            Return ImportsNetwork(path.LoadCsv(Of FileStream.NetworkEdge)(False))
        End Function

        <ExportAPI("Finder.Creates")>
        Public Function CreatePathwayFinder(net As IEnumerable(Of FileStream.NetworkEdge)) As DijkstraRouteFind
            Dim source = ImportsNetwork(net)
            Return CreatePathwayFinder(source)
        End Function

        <ExportAPI("Finder.Creates")>
        Public Function CreatePathwayFinder(net As IEnumerable(Of Connection)) As DijkstraRouteFind
            Dim lstNode As List(Of FileStream.Node) = New List(Of FileStream.Node)
            For Each edge As Connection In net
                Call lstNode.Add(edge.A)
                Call lstNode.Add(edge.B)
            Next
            Return New DijkstraRouteFind(net, lstNode.Distinct)
        End Function

        <ExportAPI("Find.Path.Shortest")>
        Public Function FindShortestPath(finder As DijkstraRouteFind, start As String, ends As String) As Route
            Return FindAllPath(finder, start, ends).FirstOrDefault
        End Function

        <ExportAPI("Network.Path.FindAll")>
        Public Function FindAllPath(finder As DijkstraRouteFind, start As String, ends As String) As Route()
            Dim Route = finder.CalculateMinCost(start)
            Dim LQuery = (From item In Route Where item.ContainsNode(ends) Select item Order By item.Cost Ascending).ToArray
            Return LQuery
        End Function
    End Module
End Namespace