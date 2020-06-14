
Imports System.Runtime.CompilerServices
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
        Public Function BetweennessCentrality()

        End Function
    End Module
End Namespace