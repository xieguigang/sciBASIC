#Region "Microsoft.VisualBasic::b8485cb2e3091bc543cb3af80b65af44, ..\visualbasic_App\gr\Datavisualization.Network\Datavisualization.Network\FindPath\Dijkstra\DijkstraAPI.vb"

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

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Dijkstra

    <[PackageNamespace]("Path.Find.Dijkstra",
                        Publisher:="Michael Demeersseman",
                        Category:=APICategories.UtilityTools,
                        Description:="Calculation of the shortest path between x points",
                        Url:="http://www.codeproject.com/Articles/22647/Dijkstra-Shortest-Route-Calculation-Object-Oriente")>
    Public Module DijkstraAPI

        <ExportAPI("Network.Imports")>
        Public Function ImportsNetwork(net As IEnumerable(Of FileStream.NetworkEdge), Optional weight As Integer = 0) As Connection()
            If weight <= 0 Then
                Return (From edge As FileStream.NetworkEdge
                        In net
                        Select Connection.CreateObject(edge)).ToArray
            Else
                Return (From edge As FileStream.NetworkEdge
                        In net
                        Select Connection.CreateObject(edge, weight)).ToArray
            End If
        End Function

        <ExportAPI("Read.Network")>
        Public Function ReadNetwork(path As String) As Connection()
            Return ImportsNetwork(path.LoadCsv(Of FileStream.NetworkEdge)(False))
        End Function

        <ExportAPI("Finder.Creates")>
        Public Function CreatePathwayFinder(net As IEnumerable(Of FileStream.NetworkEdge), Optional undirected As Boolean = False) As DijkstraRouteFind
            Dim source = ImportsNetwork(net)
            Return CreatePathwayFinder(source, undirected)
        End Function

        <ExportAPI("Finder.Creates")>
        Public Function CreatePathwayFinder(net As IEnumerable(Of Connection), Optional undirected As Boolean = False) As DijkstraRouteFind
            Dim lstNode As List(Of FileStream.Node) = New List(Of FileStream.Node)
            For Each edge As Connection In net
                Call lstNode.Add(edge.A)
                Call lstNode.Add(edge.B)
            Next

            If undirected Then
                Return DijkstraRouteFind.UndirectFinder(net, lstNode.Distinct)
            Else
                Return New DijkstraRouteFind(net, lstNode.Distinct)
            End If
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
