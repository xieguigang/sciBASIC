#Region "Microsoft.VisualBasic::358e8c1d57e8d5c708000758cecc1995, Data_science\Graph\Analysis\GraphAnalysis.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 58
    '    Code Lines: 34 (58.62%)
    ' Comment Lines: 15 (25.86%)
    '    - Xml Docs: 93.33%
    ' 
    '   Blank Lines: 9 (15.52%)
    '     File Size: 2.18 KB


    '     Module GraphAnalysis
    ' 
    '         Function: BetweennessCentrality, FindShortestPath
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Data.GraphTheory.Analysis.Dijkstra
Imports Microsoft.VisualBasic.Linq

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

            For Each node As Vertex In graph.points
                hits.Add(node.label, 0)
            Next

            For Each route As Route In graph.points _
                .AsParallel _
                .Select(Function(node)
                            Return graph.CalculateMinCost(node).Values
                        End Function) _
                .IteratesALL

                For Each point As VertexEdge In route.Connections
                    Call hits(point.U.label).Hit()
                    Call hits(point.V.label).Hit()
                Next
            Next

            Return hits.AsInteger
        End Function
    End Module
End Namespace
