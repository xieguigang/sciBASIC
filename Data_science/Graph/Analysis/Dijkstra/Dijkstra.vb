#Region "Microsoft.VisualBasic::75d58ac684b6e1dfaacfc3ccd132ed14, Data_science\Graph\Analysis\Dijkstra\Dijkstra.vb"

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

    '   Total Lines: 168
    '    Code Lines: 102 (60.71%)
    ' Comment Lines: 39 (23.21%)
    '    - Xml Docs: 58.97%
    ' 
    '   Blank Lines: 27 (16.07%)
    '     File Size: 6.88 KB


    '     Class DijkstraRouter
    ' 
    '         Properties: links, points, undirectedGraph
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: (+3 Overloads) CalculateMinCost, CreateLink, FromNetwork, GetLocation
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Data.GraphTheory.Network
Imports Microsoft.VisualBasic.Linq

Namespace Analysis.Dijkstra

    ''' <summary>
    ''' ## Dijkstra:Shortest Route Calculation - Object Oriented
    ''' 
    ''' > Michael Demeersseman, 4 Jan 2008
    ''' > http://www.codeproject.com/Articles/22647/Dijkstra-Shortest-Route-Calculation-Object-Oriente
    ''' </summary>
    Public Class DijkstraRouter

        ''' <summary>
        ''' 存在方向的
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property links As VertexEdge()
        Public ReadOnly Property points As Vertex()
        Public ReadOnly Property undirectedGraph As Boolean = False

        ''' <summary>
        ''' Create a new Dijkstra shortest path router model
        ''' </summary>
        Sub New(g As Graph, Optional undirected As Boolean = False)
            points = g.vertex.ToArray

            If undirected Then
                links = g + g _
                    .Select(Function(link) link.Reverse) _
                    .AsList
                undirectedGraph = True
            Else
                links = g.ToArray
            End If
        End Sub

        Private Sub New()
        End Sub

        Public Shared Function FromNetwork(Of TNode As {New, Network.Node}, TEdge As {New, Network.Edge(Of TNode)})(g As NetworkGraph(Of TNode, TEdge), Optional undirected As Boolean = False) As DijkstraRouter
            Dim router As New DijkstraRouter

            router._points = g.vertex _
                .Select(Function(n)
                            Return DirectCast(n, Vertex)
                        End Function) _
                .ToArray
            router._links = g.graphEdges _
                .Select(AddressOf CreateLink(Of TNode, TEdge)) _
                .ToArray

            If undirected Then
                router._links = router.links + router.links _
                    .Select(Function(link) link.Reverse) _
                    .AsList
            End If

            Return router
        End Function

        Private Shared Function CreateLink(Of TNode As {New, Network.Node}, TEdge As {New, Network.Edge(Of TNode)})(link As TEdge) As VertexEdge
            Return New VertexEdge With {
                .U = link.U,
                .V = link.V,
                .weight = link.weight,
                .ID = link.ID
            }
        End Function

        ''' <summary>
        ''' Get graph node element by label id
        ''' </summary>
        ''' <param name="label$"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetLocation(label As String) As Vertex
            Return points.Where(Function(x) x.label = label).FirstOrDefault
        End Function

        ''' <summary>
        ''' Calculates the shortest route to all the other locations.
        ''' </summary>
        ''' <param name="startPos"></param>
        ''' <returns>List of all locations and their shortest route</returns>
        ''' <remarks>
        ''' (这个函数会枚举出从出发点<paramref name="startPos"/>到网络之中的所有节点的最短路径)
        ''' </remarks>
        Public Function CalculateMinCost(startPos As Vertex) As Dictionary(Of Vertex, Route)
            ' Initialise a new empty route list
            Dim shortestPaths As New Dictionary(Of Vertex, Route)()
            ' Initialise a new empty handled locations list
            Dim handledLocations As New HashList(Of Vertex)(points.Length)
            Dim cost#

            ' Initialise the new routes. the constructor will set the route weight to in.max
            For Each location As Vertex In _points
                shortestPaths.Add(location, New Route(location.label))
            Next

            ' The startPosition has a weight 0. 
            shortestPaths(startPos).Cost = 0

            ' If all locations are handled, stop the engine and return the result
            While handledLocations.Count <> _points.Length
                ' Order the locations
                Dim shortestLocations = From s In shortestPaths Order By s.Value.Cost Select s.Key
                Dim locationToProcess As Vertex = Nothing

                ' Search for the nearest location that isn't handled
                For Each location As Vertex In shortestLocations
                    If Not handledLocations.Contains(location) Then
                        ' If the cost equals int.max, there are no more 
                        ' possible connections to the remaining 
                        ' locations
                        If shortestPaths(location).Cost = Integer.MaxValue Then
                            Return shortestPaths
                        End If

                        locationToProcess = location
                        Exit For
                    End If
                Next

                ' Select all connections where the startposition 
                ' Is the location to Process
                Dim selectedConnections = From c As VertexEdge
                                          In _links
                                          Where c.U Is locationToProcess
                                          Select c

                ' Iterate through all connections and search 
                ' For a connection which is shorter
                For Each conn As VertexEdge In selectedConnections
                    cost = conn.weight + shortestPaths(conn.U).Cost

                    If shortestPaths(conn.V).Cost > cost Then
                        shortestPaths(conn.V).SetValue(shortestPaths(conn.U).Connections)
                        shortestPaths(conn.V).Add(conn)
                        shortestPaths(conn.V).Cost = cost
                    End If
                Next

                ' Add the location to the list of processed locations
                Call handledLocations.Replace(locationToProcess)
            End While

            Call shortestPaths.Remove(startPos)

            Return shortestPaths
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CalculateMinCost(startPos As Vertex, endPos As Vertex) As Route
            ' 边界条件检查
            If startPos Is Nothing OrElse endPos Is Nothing Then Return Nothing
            If startPos.Equals(endPos) Then Return New Route(endPos.label) With {.Cost = 0}

            ' 初始化数据结构
            Dim shortestPaths As New Dictionary(Of Vertex, Route)()
            Dim handledLocations As New HashList(Of Vertex)(points.Length)
            Dim cost#

            ' 初始化所有节点的路径
            For Each location As Vertex In points
                shortestPaths.Add(location, New Route(location.label))
            Next
            shortestPaths(startPos).Cost = 0

            ' 主循环：当未处理完所有节点且未找到终点时
            While handledLocations.Count <> points.Length AndAlso Not handledLocations.Contains(endPos)
                ' 找到当前未处理节点中代价最小的
                Dim locationToProcess As Vertex = Nothing
                Dim minCost As Double = Double.MaxValue

                For Each kvp In shortestPaths
                    If Not handledLocations.Contains(kvp.Key) AndAlso kvp.Value.Cost < minCost Then
                        minCost = kvp.Value.Cost
                        locationToProcess = kvp.Key
                    End If
                Next

                ' 如果没有可达节点，提前终止
                If locationToProcess Is Nothing OrElse minCost = Integer.MaxValue Then
                    Exit While
                End If

                ' 处理当前节点的所有出边
                Dim selectedConnections = From c In links
                                          Where c.U Is locationToProcess
                                          Select c

                For Each conn As VertexEdge In selectedConnections
                    cost = conn.weight + shortestPaths(conn.U).Cost

                    If shortestPaths(conn.V).Cost > cost Then
                        shortestPaths(conn.V).SetValue(shortestPaths(conn.U).Connections)
                        shortestPaths(conn.V).Add(conn)
                        shortestPaths(conn.V).Cost = cost
                    End If
                Next

                ' 标记当前节点为已处理
                handledLocations.Replace(locationToProcess)
            End While

            ' 返回结果
            Return If(shortestPaths.ContainsKey(endPos), shortestPaths(endPos), Nothing)
        End Function

        Public Function CalculateMinCost(startVertex As String) As Dictionary(Of Vertex, Route)
            For Each node As Vertex In points
                If node.label = startVertex Then
                    Return node.DoCall(AddressOf CalculateMinCost)
                End If
            Next

            Return Nothing
        End Function
    End Class
End Namespace
