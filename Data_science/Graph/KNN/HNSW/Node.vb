Namespace KNearNeighbors.HNSW

    ''' <summary>
    ''' The abstract node implementation.
    ''' The <see cref="SelectBestForConnecting(IList(Of Node(Of TItem, TDistance)))"/> must be implemented by the subclass.
    ''' </summary>
    Friend MustInherit Class Node(Of TItem, TDistance As IComparable(Of TDistance))

        ''' <summary>
        ''' Gets the identifier of the node.
        ''' </summary>
        Dim _Id As Integer
        ''' <summary>
        ''' Gets the maximum level of the node.
        ''' </summary>
        Dim _MaxLevel As Integer

        ''' <summary>
        ''' Gets the item associated with the node.
        ''' </summary>
        Dim _Item As TItem
        ''' <summary>
        ''' Gets traveling costs from any other node to this one.
        ''' </summary>
        Dim _TravelingCosts As TravelingCosts(Of Node(Of TItem, TDistance), TDistance)
        ''' <summary>
        ''' Gets parameters of the algorithm.
        ''' </summary>
        Dim _Parameters As Parameters(Of TItem, TDistance)
        ''' <summary>
        ''' Gets all connections of the node on all layers.
        ''' </summary>
        ''' 
        Dim _Connections As System.Collections.Generic.IList(Of IList(Of Node(Of TItem, TDistance)))

        ''' <summary>
        ''' Initializes a new instance of the <see cref="Node"/> class.
        ''' </summary>
        ''' <param name="id">The identifier of the node.</param>
        ''' <param name="item">The item which is represented by the node.</param>
        ''' <param name="maxLevel">The maximum level until which the node exists.</param>
        ''' <param name="distance">The distance function for attached items.</param>
        ''' <param name="parameters">The parameters of the algorithm.</param>
        Public Sub New(id As Integer, item As TItem, maxLevel As Integer, distance As Func(Of TItem, TItem, TDistance), parameters As Parameters(Of TItem, TDistance))
            Me.Id = id
            Me.Item = item
            Me.MaxLevel = maxLevel
            Me.Parameters = parameters

            Connections = New List(Of IList(Of Node(Of TItem, TDistance)))(Me.MaxLevel + 1)
            Dim level = 0

            While level <= Me.MaxLevel
                Connections.Add(New List(Of Node(Of TItem, TDistance))(GetM(Me.Parameters.M, level)))
                Threading.Interlocked.Increment(level)
            End While

            Dim nodesDistance As Func(Of Node(Of TItem, TDistance), Node(Of TItem, TDistance), TDistance) = Function(x, y) distance(x.Item, y.Item)
            TravelingCosts = New TravelingCosts(Of Node(Of TItem, TDistance), TDistance)(nodesDistance, Me)
        End Sub

        Public Property Id As Integer
            Get
                Return _Id
            End Get
            Private Set(value As Integer)
                _Id = value
            End Set
        End Property

        Public Property MaxLevel As Integer
            Get
                Return _MaxLevel
            End Get
            Private Set(value As Integer)
                _MaxLevel = value
            End Set
        End Property

        Public Property Item As TItem
            Get
                Return _Item
            End Get
            Private Set(value As TItem)
                _Item = value
            End Set
        End Property

        Public Property TravelingCosts As TravelingCosts(Of Node(Of TItem, TDistance), TDistance)
            Get
                Return _TravelingCosts
            End Get
            Private Set(value As TravelingCosts(Of Node(Of TItem, TDistance), TDistance))
                _TravelingCosts = value
            End Set
        End Property

        Protected Property Parameters As Parameters(Of TItem, TDistance)
            Get
                Return _Parameters
            End Get
            Private Set(value As Parameters(Of TItem, TDistance))
                _Parameters = value
            End Set
        End Property

        Protected Property Connections As IList(Of IList(Of Node(Of TItem, TDistance)))
            Get
                Return _Connections
            End Get
            Private Set(value As IList(Of IList(Of Node(Of TItem, TDistance))))
                _Connections = value
            End Set
        End Property

        ''' <summary>
        ''' Get connections of the node on the given layer.
        ''' </summary>
        ''' <param name="level">The level of the layer.</param>
        ''' <returns>List of connected nodes.</returns>
        Public Function GetConnections(level As Integer) As IReadOnlyList(Of Node(Of TItem, TDistance))
            If level < Connections.Count Then
                ' this cast is needed
                ' https://visualstudio.uservoice.com/forums/121579-visual-studio-ide/suggestions/2845892-make-ilist-t-inherited-from-ireadonlylist-t
                Return CType(Connections(level), List(Of Node(Of TItem, TDistance)))
            End If

            Return Enumerable.Empty(Of Node(Of TItem, TDistance))().ToList()
        End Function

        ''' <summary>
        ''' Add connections to the node on the specific layer.
        ''' </summary>
        ''' <param name="newNeighbour">The node to connect with.</param>
        ''' <param name="level">The level of the layer.</param>
        Public Sub AddConnection(newNeighbour As Node(Of TItem, TDistance), level As Integer)
            Dim levelNeighbours = Connections(level)
            levelNeighbours.Add(newNeighbour)
            If levelNeighbours.Count > GetM(Parameters.M, level) Then
                Connections(level) = SelectBestForConnecting(levelNeighbours)
            End If
        End Sub

        ''' <summary>
        ''' The algorithm which selects best neighbours from the candidates for this node.
        ''' </summary>
        ''' <param name="candidates">The candidates for connecting.</param>
        ''' <returns>Best nodes selected from the candidates.</returns>
        Public MustOverride Function SelectBestForConnecting(candidates As IList(Of Node(Of TItem, TDistance))) As IList(Of Node(Of TItem, TDistance))

        ''' <summary>
        ''' Get maximum allowed connections for the given layer.
        ''' </summary>
        ''' <remarks>
        ''' Article: Section 4.1:
        ''' "Selection of the Mmax0 (the maximum number of connections that an element can have in the zero layer) also
        ''' has a strong influence on the search performance, especially in case of high quality(high recall) search.
        ''' Simulations show that setting Mmax0 to M(this corresponds to kNN graphs on each layer if the neighbors
        ''' selection heuristic is not used) leads to a very strong performance penalty at high recall.
        ''' Simulations also suggest that 2∙M is a good choice for Mmax0;
        ''' setting the parameter higher leads to performance degradation and excessive memory usage".
        ''' </remarks>
        ''' <param name="baseM">Base M parameter of the algorithm.</param>
        ''' <param name="level">The level of the layer.</param>
        ''' <returns>The maximum number of connections.</returns>
        Protected Shared Function GetM(baseM As Integer, level As Integer) As Integer
            Return If(level = 0, 2 * baseM, baseM)
        End Function
    End Class

End Namespace