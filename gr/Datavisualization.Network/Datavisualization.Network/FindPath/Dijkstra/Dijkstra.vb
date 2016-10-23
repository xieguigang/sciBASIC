#Region "Microsoft.VisualBasic::6fbc70979c3833b57e879240f5755324, ..\visualbasic_App\gr\Datavisualization.Network\Datavisualization.Network\FindPath\Dijkstra\Dijkstra.vb"

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

'http://www.codeproject.com/Articles/22647/Dijkstra-Shortest-Route-Calculation-Object-Oriente
'Dijkstra:Shortest Route Calculation - Object Oriented
'Michael Demeersseman, 4 Jan 2008

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Dijkstra

    Public Class DijkstraRouteFind : Implements System.IDisposable

        Public ReadOnly Property Locations() As FileStream.Node()
        Public ReadOnly Property Connections() As Connection()

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Edges">这个是有方向的</param>
        ''' <param name="Nodes"></param>
        Sub New(Edges As IEnumerable(Of Connection), Nodes As IEnumerable(Of FileStream.Node))
            _Locations = Nodes.ToArray
            _Connections = Edges.ToArray
        End Sub

        Public Shared Function UndirectFinder(net As IEnumerable(Of Connection), nodes As IEnumerable(Of FileStream.Node)) As DijkstraRouteFind
            Dim source = net.ToList
            Dim rev = (From x In source Select New Connection(x.B, x.A, x.Weight)).ToArray
            Call source.AddRange(rev)
            Return New DijkstraRouteFind(source, nodes)
        End Function

        ''' <summary>
        ''' Calculates the shortest route to all the other locations
        ''' </summary>
        ''' <param name="startPos"></param>
        ''' <returns>List of all locations and their shortest route</returns>
        Public Function CalculateMinCost(startPos As FileStream.Node) As Dictionary(Of FileStream.Node, Route)
            'Initialise a new empty route list
            Dim _shortestPaths As New Dictionary(Of FileStream.Node, Route)()
            'Initialise a new empty handled locations list
            Dim _handledLocations As New List(Of FileStream.Node)()

            'Initialise the new routes. the constructor will set the route weight to in.max
            For Each location As FileStream.Node In _Locations
                _shortestPaths.Add(location, New Route(location.Identifier))
            Next

            'The startPosition has a weight 0. 
            _shortestPaths(startPos).Cost = 0

            'If all locations are handled, stop the engine and return the result
            While _handledLocations.Count <> _Locations.Length
                'Order the locations
                Dim _shortestLocations As List(Of FileStream.Node) = (From s In _shortestPaths Order By s.Value.Cost Select s.Key).ToList()
                Dim _locationToProcess As FileStream.Node = Nothing

                'Search for the nearest location that isn't handled
                For Each _location As FileStream.Node In _shortestLocations
                    If Not _handledLocations.Contains(_location) Then
                        'If the cost equals int.max, there are no more possible connections to the remaining locations
                        If _shortestPaths(_location).Cost = Integer.MaxValue Then
                            Return _shortestPaths
                        End If
                        _locationToProcess = _location
                        Exit For
                    End If
                Next

                'Select all connections where the startposition is the location to Process
                Dim _selectedConnections = (From c In _Connections Where c.A Is _locationToProcess Select c).ToArray

                'Iterate through all connections and search for a connection which is shorter
                For Each conn As Connection In _selectedConnections
                    If _shortestPaths(conn.B).Cost > conn.Weight + _shortestPaths(conn.A).Cost Then
                        _shortestPaths(conn.B).SetValue(_shortestPaths(conn.A).Connections)
                        _shortestPaths(conn.B).Add(conn)
                        _shortestPaths(conn.B).Cost = conn.Weight + _shortestPaths(conn.A).Cost
                    End If
                Next
                'Add the location to the list of processed locations
                _handledLocations.Add(_locationToProcess)
            End While

            Return _shortestPaths
        End Function

        Public Function CalculateMinCost(starts As String) As Route()
            Dim LQuery = (From node As FileStream.Node
                          In _Locations.AsParallel
                          Where String.Equals(starts, node.Identifier, StringComparison.OrdinalIgnoreCase)
                          Select node).FirstOrDefault
            If LQuery Is Nothing Then
                Return Nothing
            End If

            Dim routes As Route() = (From found In CalculateMinCost(LQuery)
                                     Where found.Value.Cost < Integer.MaxValue
                                     Select found.Value).ToArray
            Return routes
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
End Namespace
