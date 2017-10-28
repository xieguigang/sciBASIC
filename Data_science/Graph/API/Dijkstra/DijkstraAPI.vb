#Region "Microsoft.VisualBasic::f137a33c10ca9f2e637bc46efa20fcfb, ..\sciBASIC#\Data_science\Graph\API\Dijkstra\DijkstraAPI.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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
