#Region "Microsoft.VisualBasic::fa6af3709b8f3a77dfee3495e4fd639a, Data_science\Graph\API\Dijkstra\DijkstraAPI.vb"

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

    '     Module DijkstraAPI
    ' 
    '         Function: FindShortestPath
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

        <ExportAPI("Find.Path.Shortest")>
        <Extension>
        Public Function FindShortestPath(finder As DijkstraRouter, start$, ends$) As Route
            Dim startPos As Vertex = finder.GetLocation(start)
            Dim endPos As Vertex = finder.GetLocation(ends)
            Dim routine As Route = finder.CalculateMinCost(startPos, endPos)

            Return routine
        End Function
    End Module
End Namespace
