#Region "Microsoft.VisualBasic::c34ce6aa9105317d2f001cd07a8b2fb3, Data_science\Graph\API\Dijkstra\Dijkstra.vb"

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

'     Class DijkstraRouteFind
' 
'         Properties: Links, Points
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: (+3 Overloads) CalculateMinCost, GetLocation
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Data.GraphTheory.Network
Imports Microsoft.VisualBasic.Language

Namespace Dijkstra

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

        ''' <summary>
        ''' Create a new Dijkstra shortest path router model
        ''' </summary>
        Sub New(g As Graph, Optional undirected As Boolean = False)
            points = g.vertex.ToArray

            If undirected Then
                links = g + g _
                    .Select(Function(link) link.Reverse) _
                    .AsList
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
        Public Function GetLocation(label$) As Vertex
            Return points.Where(Function(x) x.label = label).FirstOrDefault
        End Function

        ''' <summary>
        ''' Calculates the shortest route to all the other locations.
        ''' (这个函数会枚举出从出发点<paramref name="startPos"/>到网络之中的所有节点的最短路径)
        ''' </summary>
        ''' <param name="startPos"></param>
        ''' <returns>List of all locations and their shortest route</returns>
        Public Function CalculateMinCost(startPos As Vertex) As Dictionary(Of Vertex, Route)
            ' Initialise a new empty route list
            Dim shortestPaths As New Dictionary(Of Vertex, Route)()
            ' Initialise a new empty handled locations list
            Dim handledLocations As New HashList(Of Vertex)(points.Length)

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
                        ' If the cost equals int.max, there are no more possible connections to the remaining locations
                        If shortestPaths(location).Cost = Integer.MaxValue Then
                            Return shortestPaths
                        End If

                        locationToProcess = location
                        Exit For
                    End If
                Next

                ' Select all connections where the startposition is the location to Process
                Dim selectedConnections = From c As VertexEdge In _links Where c.U Is locationToProcess Select c
                Dim cost#

                ' Iterate through all connections and search for a connection which is shorter
                For Each conn As VertexEdge In selectedConnections
                    cost = conn.weight + shortestPaths(conn.U).Cost

                    If shortestPaths(conn.V).Cost > cost Then
                        shortestPaths(conn.V).SetValue(shortestPaths(conn.U).Connections)
                        shortestPaths(conn.V).Add(conn)
                        shortestPaths(conn.V).Cost = cost
                    End If
                Next

                ' Add the location to the list of processed locations
                handledLocations.Add(locationToProcess)
            End While

            shortestPaths.Remove(startPos)
            Return shortestPaths
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CalculateMinCost(startPos As Vertex, endPos As Vertex) As Route
            Return CalculateMinCost(startPos)(endPos)
        End Function

        Public Function CalculateMinCost(startVertex$) As Dictionary(Of Vertex, Route)
            Dim startPos = LinqAPI.DefaultFirst(Of Vertex) _
 _
                () <= From node As Vertex
                      In _points
                      Where node.label = startVertex
                      Select node

            If startPos Is Nothing Then
                Return Nothing
            Else
                Return CalculateMinCost(startPos)
            End If
        End Function
    End Class
End Namespace
