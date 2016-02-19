'http://www.codeproject.com/Articles/22647/Dijkstra-Shortest-Route-Calculation-Object-Oriente
'Dijkstra:Shortest Route Calculation - Object Oriented
'Michael Demeersseman, 4 Jan 2008

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.DocumentFormat.Csv.Extensions
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Dijkstra

    Public Class DijkstraRouteFind : Implements System.IDisposable

        Public ReadOnly Property Locations() As FileStream.Node()
        Public ReadOnly Property Connections() As Connection()

        Sub New(Edges As IEnumerable(Of Connection), Nodes As Generic.IEnumerable(Of FileStream.Node))
            _Locations = Nodes.ToArray
            _Connections = Edges.ToArray
        End Sub

        ''' <summary>
        ''' Calculates the shortest route to all the other locations
        ''' </summary>
        ''' <param name="_startLocation"></param>
        ''' <returns>List of all locations and their shortest route</returns>
        Public Function CalculateMinCost(_startLocation As FileStream.Node) As Dictionary(Of FileStream.Node, Route)
            'Initialise a new empty route list
            Dim _shortestPaths As New Dictionary(Of FileStream.Node, Route)()
            'Initialise a new empty handled locations list
            Dim _handledLocations As New List(Of FileStream.Node)()

            'Initialise the new routes. the constructor will set the route weight to in.max
            For Each location As FileStream.Node In _Locations
                _shortestPaths.Add(location, New Route(location.Identifier))
            Next

            'The startPosition has a weight 0. 
            _shortestPaths(_startLocation).Cost = 0

            'If all locations are handled, stop the engine and return the result
            While _handledLocations.Count <> _Locations.Count
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
            Dim Lquery = (From Node In _Locations.AsParallel Where String.Equals(starts, Node.Identifier, StringComparison.OrdinalIgnoreCase) Select Node).ToArray
            If Lquery.IsNullOrEmpty Then
                Return Nothing
            Else
                Return (From item In CalculateMinCost(Lquery.First) Where item.Value.Cost < Integer.MaxValue Select item.Value).ToArray
            End If
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
