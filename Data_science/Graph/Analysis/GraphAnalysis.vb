
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis.Dijkstra

Namespace Analysis

    Public Module GraphAnalysis

        ''' <summary>
        ''' Calculation of the shortest path between x points
        ''' </summary>
        ''' <param name="finder"></param>
        ''' <param name="start$"></param>
        ''' <param name="ends$"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' http://www.codeproject.com/Articles/22647/Dijkstra-Shortest-Route-Calculation-Object-Oriente
        ''' </remarks>
        <Extension>
        Public Function FindShortestPath(finder As DijkstraRouter, start$, ends$) As Route
            Dim startPos As Vertex = finder.GetLocation(start)
            Dim endPos As Vertex = finder.GetLocation(ends)
            Dim routine As Route = finder.CalculateMinCost(startPos, endPos)

            Return routine
        End Function

        ''' <summary>
        ''' 中介中心度，计算经过一个点的最短路径的数量。经过一个点的最短路径的数量越多，就说明它的中介中心度越高。
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function BetweennessCentrality(graph As DijkstraRouter) As Dictionary(Of String, Integer)
            Dim hits As New Dictionary(Of String, Counter)
            Dim routes As New List(Of Route)

            For Each node As Vertex In graph.points
                hits.Add(node.label, 0)
                routes.AddRange(graph.CalculateMinCost(node).Values)
            Next

            For Each route As Route In routes
                For Each point In route.Connections
                    Call hits(point.U.label).Hit()
                    Call hits(point.V.label).Hit()
                Next
            Next

            Return hits.AsInteger
        End Function
    End Module
End Namespace