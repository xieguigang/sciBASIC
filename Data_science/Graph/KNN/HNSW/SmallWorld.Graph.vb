' <copyright file="SmallWorld.Graph.cs" company="Microsoft">
' Copyright (c) Microsoft Corporation. All rights reserved.
' Licensed under the MIT License.
' </copyright>

Imports System.Text
Imports std = System.Math

Namespace KNearNeighbors.HNSW

    ' <content>
    ' The part with the implemnation of a hierarchical small world graph.
    ' </content>
    Partial Public Class SmallWorld(Of TItem, TDistance As IComparable(Of TDistance))
        ''' <summary>
        ''' The layered graph implementation.
        ''' </summary>
        Friend Class Graph

            ''' <summary>
            ''' Gets parameters of the algorithm.
            ''' </summary>
            Private _Parameters As SmallWorld(Of TItem, TDistance).Parameters
            ''' <summary>
            ''' Gets the node factory associated with the graph.
            ''' The node construction arguments are:
            ''' 1st: int -> the id of the new node;
            ''' 2nd: TItem -> the item to attach to the node;
            ''' 3rd: int -> the level of the node.
            ''' </summary>
            Dim _NewNode As System.Func(Of Integer, TItem, Integer, SmallWorld(Of TItem, TDistance).Node)
            Private entryPoint As Node

            ''' <summary>
            ''' Initializes a new instance of the <see cref="Graph"/> class.
            ''' </summary>
            ''' <param name="distance">The distance funtion to use in the small world.</param>
            ''' <param name="parameters">The parameters of the algorithm.</param>
            Public Sub New(distance As Func(Of TItem, TItem, TDistance), parameters As Parameters)
                Me.Parameters = parameters
                Select Case Me.Parameters.NeighbourHeuristic
                    Case NeighbourSelectionHeuristic.SelectHeuristic
                        NewNode = Function(id, item, level) New NodeAlg4(id, item, level, distance, Me.Parameters)
                    Case Else
                        NewNode = Function(id, item, level) New NodeAlg3(id, item, level, distance, Me.Parameters)
                End Select
            End Sub

            Public Property Parameters As Parameters
                Get
                    Return _Parameters
                End Get
                Private Set(value As Parameters)
                    _Parameters = value
                End Set
            End Property

            Public Property NewNode As Func(Of Integer, TItem, Integer, Node)
                Get
                    Return _NewNode
                End Get
                Private Set(value As Func(Of Integer, TItem, Integer, Node))
                    _NewNode = value
                End Set
            End Property

            ''' <summary>
            ''' Creates graph from the given items.
            ''' Contains implementation of INSERT(hnsw, q, M, Mmax, efConstruction, mL) algorithm.
            ''' Article: Section 4. Algorithm 1.
            ''' </summary>
            ''' <param name="items">The items to insert.</param>
            ''' <param name="generator">The random number generator to use in <see cref="RandomLevel"/>.</param>
            Public Sub Create(items As IList(Of TItem), generator As Random)
                If If(Not items?.Any(), False) Then
                    Return
                End If

                Dim id = 0
                Dim entryPoint = NewNode(id, items(id), RandomLevel(generator, Parameters.LevelLambda))

                id = 1

                While id < items.Count
                    ' 
                    ' W ← ∅ // list for the currently found nearest elements
                    ' ep ← get enter point for hnsw
                    ' L ← level of ep // top layer for hnsw
                    ' l ← ⌊-ln(unif(0..1))∙mL⌋ // new element’s level
                    ' for lc ← L … l+1
                    '   W ← SEARCH-LAYER(q, ep, ef=1, lc)
                    '   ep ← get the nearest element from W to q
                    ' for lc ← min(L, l) … 0
                    '   W ← SEARCH-LAYER(q, ep, efConstruction, lc)
                    '   neighbors ← SELECT-NEIGHBORS(q, W, M, lc) // alg. 3 or alg. 4
                    '     for each e ∈ neighbors // shrink connections if needed
                    '       eConn ← neighbourhood(e) at layer lc
                    '       if │eConn│ > Mmax // shrink connections of e if lc = 0 then Mmax = Mmax0
                    '         eNewConn ← SELECT-NEIGHBORS(e, eConn, Mmax, lc) // alg. 3 or alg. 4
                    '         set neighbourhood(e) at layer lc to eNewConn
                    '   ep ← W
                    ' if l > L
                    '   set enter point for hnsw to q


                    ' zoom in and find the best peer on the same level as newNode
                    Dim bestPeer = entryPoint
                    Dim newNode = Me.NewNode(id, items(id), RandomLevel(generator, Parameters.LevelLambda))
                    Dim level = bestPeer.MaxLevel

                    While level > newNode.MaxLevel
                        bestPeer = KNearestAtLevel(bestPeer, newNode, 1, level).[Single]()
                        Threading.Interlocked.Decrement(level)
                    End While

                    ' connecting new node to the small world
                    level = std.Min(newNode.MaxLevel, entryPoint.MaxLevel)

                    While level >= 0
                        Dim potentialNeighbours = KNearestAtLevel(bestPeer, newNode, Parameters.ConstructionPruning, level)
                        Dim bestNeighbours = newNode.SelectBestForConnecting(potentialNeighbours)

                        For Each newNeighbour In bestNeighbours
                            newNode.AddConnection(newNeighbour, level)
                            newNeighbour.AddConnection(newNode, level)

                            ' if distance from newNode to newNeighbour is better than to bestPeer => update bestPeer
                            If DLt(newNode.TravelingCosts.From(newNeighbour), newNode.TravelingCosts.From(bestPeer)) Then
                                bestPeer = newNeighbour
                            End If
                        Next

                        Threading.Interlocked.Decrement(level)
                    End While

                    ' zoom out to the highest level
                    If newNode.MaxLevel > entryPoint.MaxLevel Then
                        entryPoint = newNode
                    End If

                    Threading.Interlocked.Increment(id)
                End While

                ' construction is done
                Me.entryPoint = entryPoint
            End Sub

            ''' <summary>
            ''' Get k nearest items for a given one.
            ''' Contains implementation of K-NN-SEARCH(hnsw, q, K, ef) algorithm.
            ''' Article: Section 4. Algorithm 5.
            ''' </summary>
            ''' <param name="destination">The given node to get the nearest neighbourhood for.</param>
            ''' <param name="k">The size of the neighbourhood.</param>
            ''' <returns>The list of the nearest neighbours.</returns>
            Public Function KNearest(destination As Node, k As Integer) As IList(Of Node)
                Dim bestPeer = entryPoint
                Dim level = entryPoint.MaxLevel

                While level > 0
                    bestPeer = KNearestAtLevel(bestPeer, destination, 1, level).[Single]()
                    Threading.Interlocked.Decrement(level)
                End While

                Return KNearestAtLevel(bestPeer, destination, k, 0)
            End Function

            ''' <summary>
            ''' Serializes edges of the graph.
            ''' </summary>
            ''' <returns>Bytes representing edges.</returns>
            Public Function Serialize() As Byte()
                'Using stream = New MemoryStream()
                '    Dim formatter = New BinaryFormatter()
                '    formatter.Serialize(stream, entryPoint.Id)
                '    formatter.Serialize(stream, entryPoint.MaxLevel)

                '    Dim level = entryPoint.MaxLevel

                '    While level >= 0
                '        Dim edges = New Dictionary(Of Integer, List(Of Integer))()
                '        Call BFS(entryPoint, level, Sub(node)
                '                                        edges(node.Id) = node.GetConnections(level).[Select](Function(x) x.Id).ToList()
                '                                    End Sub)

                '        formatter.Serialize(stream, edges)
                '        Threading.Interlocked.Decrement(level)
                '    End While

                '    Return stream.ToArray()
                'End Using
                Throw New NotImplementedException
            End Function

            ''' <summary>
            ''' Deserilaizes graph edges and assigns nodes to the items.
            ''' </summary>
            ''' <param name="items">The underlying items.</param>
            ''' <param name="bytes">The serialized edges.</param>
            Public Sub Deserialize(items As IList(Of TItem), bytes As Byte())
                'Dim nodeList = Enumerable.Repeat(Of Node)(Nothing, items.Count).ToList()
                'Dim getOrAdd As Func(Of Integer, Integer, Node) = Function(id, level) CSharpImpl.__Assign(nodeList(id), If(nodeList(id), NewNode(id, items(id), level)))

                'Using stream = New MemoryStream(bytes)
                '    Dim formatter = New BinaryFormatter()
                '    Dim entryId As Integer = formatter.Deserialize(stream)
                '    Dim maxLevel As Integer = formatter.Deserialize(stream)

                '    nodeList(entryId) = NewNode(entryId, items(entryId), maxLevel)
                '    Dim level = maxLevel

                '    While level >= 0
                '        Dim edges = CType(formatter.Deserialize(stream), Dictionary(Of Integer, List(Of Integer)))
                '        For Each pair In edges
                '            Dim currentNode = getOrAdd(pair.Key, level)
                '            For Each adjacentId In pair.Value
                '                Dim neighbour = getOrAdd(adjacentId, level)
                '                currentNode.AddConnection(neighbour, level)
                '            Next
                '        Next

                '        Threading.Interlocked.Decrement(level)
                '    End While

                '    entryPoint = nodeList(entryId)
                'End Using
                Throw New NotImplementedException
            End Sub

            ''' <summary>
            ''' Prints edges of the graph.
            ''' </summary>
            ''' <returns>String representation of the graph's edges.</returns>
            Friend Function Print() As String
                Dim buffer = New StringBuilder()
                Dim level = entryPoint.MaxLevel

                While level >= 0
                    buffer.AppendLine($"[LEVEL {level}]")
                    BFS(entryPoint, level, Sub(node)
                                               Dim neighbours = String.Join(", ", node.GetConnections(level).[Select](Function(x) x.Id))
                                               buffer.AppendLine($"({node.Id}) -> {{{neighbours}}}")
                                           End Sub)

                    buffer.AppendLine()
                    Threading.Interlocked.Decrement(level)
                End While

                Return buffer.ToString()
            End Function

            ''' <summary>
            ''' The implementaiton of SEARCH-LAYER(q, ep, ef, lc) algorithm.
            ''' Article: Section 4. Algorithm 2.
            ''' </summary>
            ''' <param name="entryPoint">The entry point for the search.</param>
            ''' <param name="destination">The search target.</param>
            ''' <param name="k">The number of the nearest neighbours to get from the layer.</param>
            ''' <param name="level">Level of the layer.</param>
            ''' <returns>The list of the nearest neighbours at the level.</returns>
            Private Shared Function KNearestAtLevel(entryPoint As Node, destination As Node, k As Integer, level As Integer) As IList(Of Node)
                ' 
                ' v ← ep // set of visited elements
                ' C ← ep // set of candidates
                ' W ← ep // dynamic list of found nearest neighbors
                ' while │C│ > 0
                '   c ← extract nearest element from C to q
                '   f ← get furthest element from W to q
                '   if distance(c, q) > distance(f, q)
                '     break // all elements in W are evaluated
                '   for each e ∈ neighbourhood(c) at layer lc // update C and W
                '     if e ∉ v
                '       v ← v ⋃ e
                '       f ← get furthest element from W to q
                '       if distance(e, q) < distance(f, q) or │W│ < ef
                '         C ← C ⋃ e
                '         W ← W ⋃ e
                '         if │W│ > ef
                '           remove furthest element from W to q
                ' return W

                ' prepare tools
                Dim closerIsLess As IComparer(Of Node) = destination.TravelingCosts
                Dim fartherIsLess As IComparer(Of Node) = closerIsLess.Reverse()

                ' prepare heaps
                Dim resultHeap = New BinaryHeap(Of Node)(New List(Of Node)(k + 1) From {
                    entryPoint
                }, closerIsLess)
                Dim expansionHeap = New BinaryHeap(Of Node)(New List(Of Node)() From {
                    entryPoint
                }, fartherIsLess)

                ' run bfs
                Dim visited = New HashSet(Of Integer)() From {
                    entryPoint.Id
                }
                While expansionHeap.Buffer.Any()
                    ' get next candidate to check and expand
                    Dim toExpand = expansionHeap.Pop()
                    Dim farthestResult = resultHeap.Buffer.First()
                    If DGt(destination.TravelingCosts.From(toExpand), destination.TravelingCosts.From(farthestResult)) Then
                        ' the closest candidate is farther than farthest result
                        Exit While
                    End If

                    ' expand candidate
                    For Each neighbour In toExpand.GetConnections(level)
                        If Not visited.Contains(neighbour.Id) Then
                            ' enque perspective neighbours to expansion list
                            farthestResult = resultHeap.Buffer.First()
                            If resultHeap.Buffer.Count < k OrElse DLt(destination.TravelingCosts.From(neighbour), destination.TravelingCosts.From(farthestResult)) Then
                                expansionHeap.Push(neighbour)
                                resultHeap.Push(neighbour)
                                If resultHeap.Buffer.Count > k Then
                                    resultHeap.Pop()
                                End If
                            End If

                            ' update visited list
                            visited.Add(neighbour.Id)
                        End If
                    Next
                End While

                Return resultHeap.Buffer
            End Function

            ''' <summary>
            ''' Gets the level for the layer.
            ''' </summary>
            ''' <param name="generator">The random numbers generator.</param>
            ''' <param name="lambda">Poisson lambda.</param>
            ''' <returns>The level value.</returns>
            Private Shared Function RandomLevel(generator As Random, lambda As Double) As Integer
                Dim r = -std.Log(generator.NextDouble()) * lambda
                Return r
            End Function
        End Class
    End Class
End Namespace
