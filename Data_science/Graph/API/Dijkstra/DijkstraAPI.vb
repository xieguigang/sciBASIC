#Region "Microsoft.VisualBasic::f137a33c10ca9f2e637bc46efa20fcfb, Data_science\Graph\API\Dijkstra\DijkstraAPI.vb"

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

    '     Module DijkstraAPI
    ' 
    '         Function: CreatePathwayFinder, FindShortestPath
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Dijkstra

    <Package("Path.Find.Dijkstra",
             Publisher:="Michael Demeersseman",
             Category:=APICategories.UtilityTools,
             Description:="Calculation of the shortest path between x points",
             Url:="http://www.codeproject.com/Articles/22647/Dijkstra-Shortest-Route-Calculation-Object-Oriente")>
    Public Module DijkstraAPI

        <ExportAPI("Finder.Creates")>
        <Extension>
        Public Function CreatePathwayFinder(g As Graph, Optional undirected As Boolean = False) As DijkstraRouteFind
            Return New DijkstraRouteFind(g, undirected)
        End Function

        <ExportAPI("Find.Path.Shortest")>
        <Extension>
        Public Function FindShortestPath(finder As DijkstraRouteFind, start$, ends$) As Route
            Dim startPos As Vertex = finder.GetLocation(start)
            Dim endPos As Vertex = finder.GetLocation(ends)
            Return finder.CalculateMinCost(startPos, endPos)
        End Function
    End Module
End Namespace
