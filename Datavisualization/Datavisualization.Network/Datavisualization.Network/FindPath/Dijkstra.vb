'http://www.codeproject.com/Articles/22647/Dijkstra-Shortest-Route-Calculation-Object-Oriente
'Dijkstra:Shortest Route Calculation - Object Oriented
'Michael Demeersseman, 4 Jan 2008

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DocumentFormat.Csv.Extensions
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Dijkstra

    Public Class Connection : Implements Microsoft.VisualBasic.ComponentModel.Collection.Generic.IKeyValuePairObject(Of FileStream.Node, FileStream.Node)

        Dim DataModel As FileStream.NetworkEdge

        Public Property Selected As Boolean = False
        Public Property B As FileStream.Node Implements ComponentModel.Collection.Generic.IKeyValuePairObject(Of FileStream.Node, FileStream.Node).Value
        Public Property A As FileStream.Node Implements ComponentModel.Collection.Generic.IKeyValuePairObject(Of FileStream.Node, FileStream.Node).locusId
        Public Property Weight As Integer

        Public Sub New(a As FileStream.Node, b As FileStream.Node, weight As Integer)
            _A = a
            _B = b
            _Weight = weight
        End Sub

        Public Shared Function CreateObject(DataModel As FileStream.NetworkEdge) As Connection
            Dim ndA As New FileStream.Node With {.Identifier = DataModel.FromNode}
            Dim ndB As New FileStream.Node With {.Identifier = DataModel.ToNode}
            Return New Connection(ndA, ndB, weight:=DataModel.Confidence) With {.DataModel = DataModel}
        End Function
    End Class

    Public Class Route : Implements Generic.IList(Of Connection)

        Dim _Connections As List(Of Connection)
        Dim _identifier As String

        Public Function ContainsNode(Id As String) As Boolean
            Dim LQuery = (From conn In _Connections.AsParallel
                          Where String.Equals(conn.A.Identifier, Id, StringComparison.OrdinalIgnoreCase) OrElse
                              String.Equals(conn.B.Identifier, Id, StringComparison.OrdinalIgnoreCase)
                          Select conn).ToArray
            Return Not LQuery.IsNullOrEmpty
        End Function

        Public Sub New(identifier As String)
            _Cost = Integer.MaxValue
            _Connections = New List(Of Connection)()
            _identifier = identifier
        End Sub

        Public ReadOnly Property Connections() As Connection()
            Get
                Return _Connections.ToArray
            End Get
        End Property

        Public Property Cost As Integer

        Public Overrides Function ToString() As String
            Return "Id:" & _identifier & " Cost:" & Cost
        End Function

        Public Sub SetValue(Connections As Generic.IEnumerable(Of Connection))
            Call _Connections.Clear()
            Call _Connections.AddRange(Connections)
        End Sub

#Region "Implements Generic.IList(Of Connection)"

        Public Sub Add(item As Connection) Implements ICollection(Of Connection).Add
            Call _Connections.Add(item)
        End Sub

        Public Sub Clear() Implements ICollection(Of Connection).Clear
            Call _Connections.Clear()
        End Sub

        Public Function Contains(item As Connection) As Boolean Implements ICollection(Of Connection).Contains
            Return _Connections.Contains(item)
        End Function

        Public Sub CopyTo(array() As Connection, arrayIndex As Integer) Implements ICollection(Of Connection).CopyTo
            Call _Connections.CopyTo(array, arrayIndex)
        End Sub

        Public ReadOnly Property Count As Integer Implements ICollection(Of Connection).Count
            Get
                Return _Connections.Count
            End Get
        End Property

        Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of Connection).IsReadOnly
            Get
                Return False
            End Get
        End Property

        Public Function Remove(item As Connection) As Boolean Implements ICollection(Of Connection).Remove
            Return _Connections.Remove(item)
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Connection) Implements IEnumerable(Of Connection).GetEnumerator
            For Each cnnItem As Connection In _Connections
                Yield cnnItem
            Next
        End Function

        Public Function IndexOf(item As Connection) As Integer Implements IList(Of Connection).IndexOf
            Return _Connections.IndexOf(item)
        End Function

        Public Sub Insert(index As Integer, item As Connection) Implements IList(Of Connection).Insert
            Call _Connections.Insert(index, item)
        End Sub

        Default Public Property Item(index As Integer) As Connection Implements IList(Of Connection).Item
            Get
                Return _Connections(index)
            End Get
            Set(value As Connection)
                _Connections(index) = value
            End Set
        End Property

        Public Sub RemoveAt(index As Integer) Implements IList(Of Connection).RemoveAt
            Call _Connections.RemoveAt(index)
        End Sub

        Public Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
#End Region
    End Class

    <[PackageNamespace]("Path.Find.Dijkstra",
                        Publisher:="Michael Demeersseman",
                        Category:=APICategories.UtilityTools,
                        Description:="Calculation of the shortest path between x points",
                        Url:="http://www.codeproject.com/Articles/22647/Dijkstra-Shortest-Route-Calculation-Object-Oriente")>
    Public Class DijkstraRouteFind : Implements System.IDisposable

        Public ReadOnly Property Locations() As FileStream.Node()
        Public ReadOnly Property Connections() As Connection()

        Sub New(Edges As Generic.IEnumerable(Of Connection), Nodes As Generic.IEnumerable(Of FileStream.Node))
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

        <ExportAPI("Network.Imports")>
        Public Shared Function ImportsNetwork(CsvData As FileStream.NetworkEdge()) As Connection()
            Dim LQuery = (From edge In CsvData Select Connection.CreateObject(edge)).ToArray
            Return LQuery
        End Function

        <ExportAPI("read.network")>
        Public Shared Function ReadNetwork(path As String) As Connection()
            Dim Chunkbuffer = path.LoadCsv(Of FileStream.NetworkEdge)(False).ToArray
            Dim LQuery = (From edge In Chunkbuffer Select Connection.CreateObject(edge)).ToArray
            Return LQuery
        End Function

        <ExportAPI("Session.New()")>
        Public Shared Function CreatePathwayFinder(Network As Generic.IEnumerable(Of Connection)) As DijkstraRouteFind
            Dim Nodes As List(Of FileStream.Node) = New List(Of FileStream.Node)
            For Each Edge In Network
                Call Nodes.Add(Edge.A)
                Call Nodes.Add(Edge.B)
            Next
            Return New DijkstraRouteFind(Network, Nodes.Distinct)
        End Function

        <ExportAPI("Find.Path.Shortest")>
        Public Shared Function FindShortestPath(finder As DijkstraRouteFind, start As String, ends As String) As Route
            Dim Route = finder.CalculateMinCost(start)
            Dim LQuery = (From item In Route Where item.ContainsNode(ends) Select item Order By item.Cost Ascending).ToArray
            If LQuery.IsNullOrEmpty Then
                Return Nothing
            Else
                Return LQuery.First
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
