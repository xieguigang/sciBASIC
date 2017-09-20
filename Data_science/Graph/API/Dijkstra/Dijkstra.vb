#Region "Microsoft.VisualBasic::d3e58e9b575a93b897d018e9cb6f9ab4, ..\sciBASIC#\gr\Datavisualization.Network\Datavisualization.Network\FindPath\Dijkstra\Dijkstra.vb"

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

Imports Microsoft.VisualBasic.Language

Namespace Dijkstra

    ''' <summary>
    ''' ## Dijkstra:Shortest Route Calculation - Object Oriented
    ''' 
    ''' > Michael Demeersseman, 4 Jan 2008
    ''' > http://www.codeproject.com/Articles/22647/Dijkstra-Shortest-Route-Calculation-Object-Oriente
    ''' </summary>
    Public Class DijkstraRouteFind

        Public ReadOnly Property Locations() As Vertex()

        ''' <summary>
        ''' 存在方向的
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Connections() As Edge()

        ''' <summary>
        ''' 
        ''' </summary>
        Sub New(g As Graph, Optional undirected As Boolean = False)
            Locations = g.Vertex

            If undirected Then
                Connections = g + g _
                    .Select(Function(e) e.Reverse) _
                    .AsList
            Else
                Connections = g.ToArray
            End If
        End Sub

        ''' <summary>
        ''' Calculates the shortest route to all the other locations
        ''' </summary>
        ''' <param name="startPos"></param>
        ''' <returns>List of all locations and their shortest route</returns>
        Public Function CalculateMinCost(startPos As Vertex) As Dictionary(Of Vertex, Route)
            ' Initialise a new empty route list
            Dim _shortestPaths As New Dictionary(Of Vertex, Route)()
            ' Initialise a new empty handled locations list
            Dim _handledLocations As New List(Of Vertex)()

            ' Initialise the new routes. the constructor will set the route weight to in.max
            For Each location As Vertex In _Locations
                _shortestPaths.Add(location, New Route(location.ID))
            Next

            ' The startPosition has a weight 0. 
            _shortestPaths(startPos).Cost = 0

            ' If all locations are handled, stop the engine and return the result
            While _handledLocations.Count <> _Locations.Length
                ' Order the locations
                Dim _shortestLocations As List(Of Vertex) = (From s In _shortestPaths Order By s.Value.Cost Select s.Key).AsList()
                Dim _locationToProcess As Vertex = Nothing

                ' Search for the nearest location that isn't handled
                For Each _location As Vertex In _shortestLocations
                    If Not _handledLocations.Contains(_location) Then
                        ' If the cost equals int.max, there are no more possible connections to the remaining locations
                        If _shortestPaths(_location).Cost = Integer.MaxValue Then
                            Return _shortestPaths
                        End If
                        _locationToProcess = _location
                        Exit For
                    End If
                Next

                ' Select all connections where the startposition is the location to Process
                Dim _selectedConnections = (From c In _Connections Where c.U Is _locationToProcess Select c).ToArray

                ' Iterate through all connections and search for a connection which is shorter
                For Each conn As Edge In _selectedConnections
                    If _shortestPaths(conn.V).Cost > conn.Weight + _shortestPaths(conn.U).Cost Then
                        _shortestPaths(conn.V).SetValue(_shortestPaths(conn.U).Connections)
                        _shortestPaths(conn.V).Add(conn)
                        _shortestPaths(conn.V).Cost = conn.Weight + _shortestPaths(conn.U).Cost
                    End If
                Next
                ' Add the location to the list of processed locations
                _handledLocations.Add(_locationToProcess)
            End While

            Return _shortestPaths
        End Function

        Public Function CalculateMinCost(startVertex$) As Dictionary(Of Vertex, Route)
            Dim startPos = LinqAPI.DefaultFirst(Of Vertex) _
 _
                () <= From node As Vertex
                      In _Locations
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
