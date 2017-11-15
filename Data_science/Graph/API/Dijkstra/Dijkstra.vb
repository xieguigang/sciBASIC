#Region "Microsoft.VisualBasic::8b049bc30f15d54e6dc68bf887664d09, ..\sciBASIC#\Data_science\Graph\API\Dijkstra\Dijkstra.vb"

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

Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Language

Namespace Dijkstra

    ''' <summary>
    ''' ## Dijkstra:Shortest Route Calculation - Object Oriented
    ''' 
    ''' > Michael Demeersseman, 4 Jan 2008
    ''' > http://www.codeproject.com/Articles/22647/Dijkstra-Shortest-Route-Calculation-Object-Oriente
    ''' </summary>
    Public Class DijkstraRouteFind

        ''' <summary>
        ''' 存在方向的
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Links() As Edge()
        Public ReadOnly Property Points() As Vertex()

        ''' <summary>
        ''' 
        ''' </summary>
        Sub New(g As Graph, Optional undirected As Boolean = False)
            Points = g.Vertex

            If undirected Then
                Links = g + g _
                    .Select(Function(e) e.Reverse) _
                    .AsList
            Else
                Links = g.ToArray
            End If
        End Sub

        Public Function GetLocation(label$) As Vertex
            Return Points.Where(Function(x) x.Label = label).FirstOrDefault
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
            Dim handledLocations As New HashList(Of Vertex)(Points.Length)

            ' Initialise the new routes. the constructor will set the route weight to in.max
            For Each location As Vertex In _Points
                shortestPaths.Add(location, New Route(location.Label))
            Next

            ' The startPosition has a weight 0. 
            shortestPaths(startPos).Cost = 0

            ' If all locations are handled, stop the engine and return the result
            While handledLocations.Count <> _Points.Length
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
                Dim selectedConnections = From c As Edge In _Links Where c.U Is locationToProcess Select c
                Dim cost#

                ' Iterate through all connections and search for a connection which is shorter
                For Each conn As Edge In selectedConnections
                    cost = conn.Weight + shortestPaths(conn.U).Cost

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

        Public Function CalculateMinCost(startPos As Vertex, endPos As Vertex) As Route
            Return CalculateMinCost(startPos)(endPos)
        End Function

        Public Function CalculateMinCost(startVertex$) As Dictionary(Of Vertex, Route)
            Dim startPos = LinqAPI.DefaultFirst(Of Vertex) _
 _
                () <= From node As Vertex
                      In _Points
                      Where node.Label = startVertex
                      Select node

            If startPos Is Nothing Then
                Return Nothing
            Else
                Return CalculateMinCost(startPos)
            End If
        End Function
    End Class
End Namespace
