' <copyright file="SmallWorld.Node.cs" company="Microsoft">
' Copyright (c) Microsoft Corporation. All rights reserved.
' Licensed under the MIT License.
' </copyright>

Namespace KNearNeighbors.HNSW

    ' <content>
    ' The part with the implementaion of a node in the hnsw graph.
    ' </content>
    '

    Partial Public Class SmallWorld(Of TItem, TDistance As IComparable(Of TDistance))
        ''' <summary>
        ''' The abstract node implementation.
        ''' The <see cref="SelectBestForConnecting(IList(Of Node))"/> must be implemented by the subclass.
        ''' </summary>
        Friend MustInherit Class Node

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
            Dim _TravelingCosts As TravelingCosts(Of SmallWorld(Of TItem, TDistance).Node, TDistance)
            ''' <summary>
            ''' Gets parameters of the algorithm.
            ''' </summary>
            Dim _Parameters As SmallWorld(Of TItem, TDistance).Parameters
            ''' <summary>
            ''' Gets all connections of the node on all layers.
            ''' </summary>
            ''' 
            Dim _Connections As System.Collections.Generic.IList(Of System.Collections.Generic.IList(Of SmallWorld(Of TItem, TDistance).Node))

            ''' <summary>
            ''' Initializes a new instance of the <see cref="Node"/> class.
            ''' </summary>
            ''' <param name="id">The identifier of the node.</param>
            ''' <param name="item">The item which is represented by the node.</param>
            ''' <param name="maxLevel">The maximum level until which the node exists.</param>
            ''' <param name="distance">The distance function for attached items.</param>
            ''' <param name="parameters">The parameters of the algorithm.</param>
            Public Sub New(id As Integer, item As TItem, maxLevel As Integer, distance As Func(Of TItem, TItem, TDistance), parameters As Parameters)
                Me.Id = id
                Me.Item = item
                Me.MaxLevel = maxLevel
                Me.Parameters = parameters

                Connections = New List(Of IList(Of Node))(Me.MaxLevel + 1)
                Dim level = 0

                While level <= Me.MaxLevel
                    Connections.Add(New List(Of Node)(GetM(Me.Parameters.M, level)))
                    Threading.Interlocked.Increment(level)
                End While

                Dim nodesDistance As Func(Of Node, Node, TDistance) = Function(x, y) distance(x.Item, y.Item)
                TravelingCosts = New TravelingCosts(Of Node, TDistance)(nodesDistance, Me)
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

            Public Property TravelingCosts As TravelingCosts(Of Node, TDistance)
                Get
                    Return _TravelingCosts
                End Get
                Private Set(value As TravelingCosts(Of Node, TDistance))
                    _TravelingCosts = value
                End Set
            End Property

            Protected Property Parameters As Parameters
                Get
                    Return _Parameters
                End Get
                Private Set(value As Parameters)
                    _Parameters = value
                End Set
            End Property

            Protected Property Connections As IList(Of IList(Of Node))
                Get
                    Return _Connections
                End Get
                Private Set(value As IList(Of IList(Of Node)))
                    _Connections = value
                End Set
            End Property

            ''' <summary>
            ''' Get connections of the node on the given layer.
            ''' </summary>
            ''' <param name="level">The level of the layer.</param>
            ''' <returns>List of connected nodes.</returns>
            Public Function GetConnections(level As Integer) As IReadOnlyList(Of Node)
                If level < Connections.Count Then
                    ' this cast is needed
                    ' https://visualstudio.uservoice.com/forums/121579-visual-studio-ide/suggestions/2845892-make-ilist-t-inherited-from-ireadonlylist-t
                    Return CType(Connections(level), List(Of Node))
                End If

                Return Enumerable.Empty(Of Node)().ToList()
            End Function

            ''' <summary>
            ''' Add connections to the node on the specific layer.
            ''' </summary>
            ''' <param name="newNeighbour">The node to connect with.</param>
            ''' <param name="level">The level of the layer.</param>
            Public Sub AddConnection(newNeighbour As Node, level As Integer)
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
            Public MustOverride Function SelectBestForConnecting(candidates As IList(Of Node)) As IList(Of Node)

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

        ''' <summary>
        ''' The implementation of the SELECT-NEIGHBORS-SIMPLE(q, C, M) algorithm.
        ''' Article: Section 4. Algorithm 3.
        ''' </summary>
        Private Class NodeAlg3
            Inherits Node
            ''' <summary>
            ''' Initializes a new instance of the <see cref="NodeAlg3"/> class.
            ''' </summary>
            ''' <param name="id">The identifier of the node.</param>
            ''' <param name="item">The item which is represented by the node.</param>
            ''' <param name="maxLevel">The maximum level until which the node exists.</param>
            ''' <param name="distance">The distance function for attached items.</param>
            ''' <param name="parameters">The parameters of the algorithm.</param>
            Public Sub New(id As Integer, item As TItem, maxLevel As Integer, distance As Func(Of TItem, TItem, TDistance), parameters As Parameters)
                MyBase.New(id, item, maxLevel, distance, parameters)
            End Sub

            ''' <inheritdoc/>
            Public Overrides Function SelectBestForConnecting(candidates As IList(Of Node)) As IList(Of Node)
                ' 
                ' q ← this
                ' return M nearest elements from C to q

                Dim fartherIsLess As IComparer(Of Node) = TravelingCosts.Reverse()
                Dim candidatesHeap = New BinaryHeap(Of Node)(candidates, fartherIsLess)

                Dim result = New List(Of Node)(GetM(Parameters.M, MaxLevel) + 1)
                While candidatesHeap.Buffer.Any() AndAlso result.Count < GetM(Parameters.M, MaxLevel)
                    result.Add(candidatesHeap.Pop())
                End While

                Return result
            End Function
        End Class

        ''' <summary>
        ''' The implementation of the SELECT-NEIGHBORS-HEURISTIC(q, C, M, lc, extendCandidates, keepPrunedConnections) algorithm.
        ''' Article: Section 4. Algorithm 4.
        ''' </summary>
        Private Class NodeAlg4
            Inherits Node
            ''' <summary>
            ''' Initializes a new instance of the <see cref="NodeAlg4"/> class.
            ''' </summary>
            ''' <param name="id">The identifier of the node.</param>
            ''' <param name="item">The item which is represented by the node.</param>
            ''' <param name="maxLevel">The maximum level until which the node exists.</param>
            ''' <param name="distance">The distance function for attached items.</param>
            ''' <param name="parameters">The parameters of the algorithm.</param>
            Public Sub New(id As Integer, item As TItem, maxLevel As Integer, distance As Func(Of TItem, TItem, TDistance), parameters As Parameters)
                MyBase.New(id, item, maxLevel, distance, parameters)
            End Sub

            ''' <inheritdoc/>
            Public Overrides Function SelectBestForConnecting(candidates As IList(Of Node)) As IList(Of Node)
                ' 
                ' q ← this
                ' R ← ∅    // result
                ' W ← C    // working queue for the candidates
                ' if expandCandidates  // expand candidates
                '   for each e ∈ C
                '     for each eadj ∈ neighbourhood(e) at layer lc
                '       if eadj ∉ W
                '         W ← W ⋃ eadj
                '
                ' Wd ← ∅ // queue for the discarded candidates
                ' while │W│ gt 0 and │R│ lt M
                '   e ← extract nearest element from W to q
                '   if e is closer to q compared to any element from R
                '     R ← R ⋃ e
                '   else
                '     Wd ← Wd ⋃ e
                '
                ' if keepPrunedConnections // add some of the discarded connections from Wd
                '   while │Wd│ gt 0 and │R│ lt M
                '   R ← R ⋃ extract nearest element from Wd to q
                '
                ' return R


                Dim closerIsLess As IComparer(Of Node) = TravelingCosts
                Dim fartherIsLess As IComparer(Of Node) = closerIsLess.Reverse()

                Dim resultHeap = New BinaryHeap(Of Node)(New List(Of Node)(GetM(Parameters.M, MaxLevel) + 1), closerIsLess)
                Dim candidatesHeap = New BinaryHeap(Of Node)(candidates, fartherIsLess)

                ' expand candidates option is enabled
                If Parameters.ExpandBestSelection Then
                    Dim candidatesIds = New HashSet(Of Integer)(candidates.[Select](Function(c) c.Id))
                    For Each neighbour In GetConnections(MaxLevel)
                        If Not candidatesIds.Contains(neighbour.Id) Then
                            candidatesHeap.Push(neighbour)
                            candidatesIds.Add(neighbour.Id)
                        End If
                    Next
                End If

                ' main stage of moving candidates to result
                Dim discardedHeap = New BinaryHeap(Of Node)(New List(Of Node)(candidatesHeap.Buffer.Count), fartherIsLess)
                While candidatesHeap.Buffer.Any() AndAlso resultHeap.Buffer.Count < GetM(Parameters.M, MaxLevel)
                    Dim candidate = candidatesHeap.Pop()
                    Dim farestResult = resultHeap.Buffer.FirstOrDefault()

                    If farestResult Is Nothing OrElse DLt(TravelingCosts.From(candidate), TravelingCosts.From(farestResult)) Then
                        resultHeap.Push(candidate)
                    ElseIf Parameters.KeepPrunedConnections Then
                        discardedHeap.Push(candidate)
                    End If
                End While

                ' keep pruned option is enabled
                If Parameters.KeepPrunedConnections Then
                    While discardedHeap.Buffer.Any() AndAlso resultHeap.Buffer.Count < GetM(Parameters.M, MaxLevel)
                        resultHeap.Push(discardedHeap.Pop())
                    End While
                End If

                Return resultHeap.Buffer
            End Function
        End Class
    End Class
End Namespace
