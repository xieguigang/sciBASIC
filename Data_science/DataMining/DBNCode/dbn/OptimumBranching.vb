Imports DBNCode.utils
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language.Java

Namespace dbn

    Public Class OptimumBranching

        Public branchingField As IList(Of Edge)

        Public Shared root As Integer

        Public Shared N As Integer

        Public Shared Adj As IList(Of LinkedList(Of Integer))

        ' 
        ' 		 * public static List<Edge> evaluate(double[][] scoresMatrix) { return
        ' 		 * evaluate(scoresMatrix, -1, false); }
        ' 		 
        '

        Public Overridable ReadOnly Property Branching As IList(Of Edge)
            Get
                Return branchingField
            End Get
        End Property

        Public Sub New(scoresMatrix As Double()(), finalRoot As Integer, spanning As Boolean)

            ' INIT phase

            Dim n = scoresMatrix.Length

            Dim root_final = 0

            ' set of strongly-connected graph components
            Dim scc As DisjointSets = New DisjointSets(n)

            ' set of weakly-connected graph components
            Dim wcc As DisjointSets = New DisjointSets(n)

            ' maintains track of edges hierarchy to build final tree
            Dim forest As Forest(Of Edge) = New Forest(Of Edge)()

            Dim incidentEdges As IList(Of IList(Of Edge)) = New List(Of IList(Of Edge))(n)

            Dim cycleEdges As IList(Of IList(Of Edge)) = New List(Of IList(Of Edge))(n)

            Dim enteringEdge As IList(Of Edge) = New List(Of Edge)(n)

            Dim forestLeaf As IList(Of TreeNode(Of Edge)) = New List(Of TreeNode(Of Edge))(n)

            Dim min = New Integer(n - 1) {}

            Dim branchingEdges As IList(Of Edge) = New List(Of Edge)()

            Dim vertices As LinkedList(Of Integer) = New LinkedList(Of Integer)()

            ' stupid initialization
            Dim roots As ISet(Of Integer) = New HashSet(Of Integer)()
            If finalRoot >= 0 Then
                roots.Add(finalRoot)
            End If

            For i = 0 To n - 1

                incidentEdges.Add(New List(Of Edge)())

                cycleEdges.Add(New List(Of Edge)(n))

                enteringEdge.Add(Nothing)
                forestLeaf.Add(Nothing)

                ' initial root of the strongly connected component of i
                min(i) = i

                vertices.AddLast(i)
            Next

            ' remove supplied final root node
            vertices.Remove(finalRoot)

            ' fill incident edges, already sorted by source
            For i = 0 To n - 1
                For j = 0 To n - 1
                    ' skip self edges
                    If i <> j Then
                        incidentEdges(i).Add(New Edge(j, i, scoresMatrix(i)(j)))
                    End If
                Next
            Next

            ' BRANCH phase
            While vertices.Count > 0
                Dim r = vertices.First.Value
                vertices.RemoveFirst()
                Dim inEdges = incidentEdges(r)
                ' input graph assumed strongly connected
                ' if there is no edge incident on r, then r is a super-node
                ' containing all vertices
                If inEdges.Count = 0 Then
                    ' root of the final MWDST
                    roots.Add(min(r))
                    root_final = min(r)
                Else

                    ' get heaviest edge (i,j) incident on r
                    Dim maxIndex = 0
                    For i = 1 To inEdges.Count - 1
                        If inEdges(i).Weight > inEdges(maxIndex).Weight Then
                            maxIndex = i
                        End If
                    Next
                    ' edge is deleted from I[r]
                    Dim heaviest = inEdges(maxIndex)
                    inEdges.RemoveAt(maxIndex)
                    If Not spanning AndAlso heaviest.Weight <= 0 Then
                        roots.Add(min(r))
                    Else

                        Dim i = heaviest.Tail
                        Dim j = heaviest.Head
                        Dim iWeakComponentRoot = wcc.find(i)
                        Dim jWeakComponentRoot = wcc.find(j)

                        ' add heaviest edge to forest of edges
                        Dim tn As TreeNode(Of Edge) = forest.add(heaviest, cycleEdges(r))
                        If cycleEdges(r).Count = 0 Then
                            forestLeaf(j) = tn ' points leaf edge in F
                        End If

                        ' no cycle is created by heaviest edge
                        If iWeakComponentRoot <> jWeakComponentRoot Then
                            ' join i and j in the same weakly-connected set
                            wcc.union(iWeakComponentRoot, jWeakComponentRoot)
                            ' heaviest is the only chosen edge incident on r
                            enteringEdge(r) = heaviest
                        Else
                            ' heaviest edge introduces a cycle
                            ' reset cycle edges
                            cycleEdges(r).Clear()

                            Dim lightest = heaviest
                            ' find cycle edges and obtain the lightest one
                            Dim cycleEdge = heaviest

                            While cycleEdge IsNot Nothing

                                If cycleEdge.Weight < lightest.Weight Then
                                    lightest = cycleEdge
                                End If

                                ' add (x,y) to the list of cycle edges
                                cycleEdges(r).Add(cycleEdge)
                                cycleEdge = enteringEdge(scc.find(cycleEdge.Tail))
                            End While

                            ' update incident edges on r
                            For Each E As Edge In inEdges
                                E.Weight = E.Weight + lightest.Weight - heaviest.Weight
                            Next

                            ' keep track of root for the spanning tree
                            min(r) = min(scc.find(lightest.Head))

                            ' loop over cycle edges excluding heaviest
                            cycleEdge = enteringEdge(scc.find(i))

                            While cycleEdge IsNot Nothing

                                Dim headStrongComponentRoot = scc.find(cycleEdge.Head)

                                ' update incident edges on other nodes of the cycle
                                For Each E As Edge In incidentEdges(headStrongComponentRoot)
                                    E.Weight = E.Weight + lightest.Weight - cycleEdge.Weight
                                Next

                                ' join vertices of the cycle into one scc
                                scc.union(r, headStrongComponentRoot)

                                ' join incident edges lists;
                                incidentEdges(r) = merge(incidentEdges(r), incidentEdges(headStrongComponentRoot), scc, r)
                                cycleEdge = enteringEdge(scc.find(cycleEdge.Tail))
                            End While

                            vertices.AddFirst(r)
                        End If
                    End If
                End If
            End While

            ' LEAF phase
            For Each root In roots
                Dim rootLeaf = forestLeaf(root)
                If rootLeaf IsNot Nothing Then
                    forest.deleteUp(rootLeaf)
                End If
            Next

            While Not forest.Empty
                Dim forestRoot As TreeNode(Of Edge) = forest.Root
                Dim e = forestRoot.Data
                branchingEdges.Add(e)
                Dim forestRootLeaf = forestLeaf(e.Head)
                forest.deleteUp(forestRootLeaf)
            End While

            branchingField = branchingEdges
            root = root_final
            OptimumBranching.N = n

            Dim adj As IList(Of LinkedList(Of Integer)) = New List(Of LinkedList(Of Integer))()
            For i = 0 To OptimumBranching.N - 1
                adj.Add(New LinkedList(Of Integer)())
            Next

            For Each e As Edge In branchingField
                adj(e.Tail).AddLast(e.Head)
            Next

            OptimumBranching.Adj = adj
        End Sub

        Public Sub New(scoresMatrix As Double()())
            Me.New(scoresMatrix, -1, True)

        End Sub

        Public Overridable Function ancestors(i As Integer) As List(Of Integer)
            Dim anc As List(Of Integer) = New List(Of Integer)()
            Dim b = True

            Dim node = i

            While b = True
                Dim b2 = False
                For Each E As Edge In branchingField

                    Dim head = E.Head
                    Dim tail = E.Tail

                    If head = node Then
                        anc.Add(tail)
                        node = tail
                        b2 = True
                        Exit For
                    End If

                Next

                If b2 = False Then
                    b = False
                End If

            End While

            Return anc
        End Function

        Public Shared Function Subsets(anc As List(Of Integer), k As Integer) As List(Of List(Of Integer))

            Dim total As List(Of List(Of Integer)) = New List(Of List(Of Integer))()

            Dim n = anc.Count

            ' Run a loop for printing all 2^n
            ' subsets one by one
            For i = 0 To (1 << n) - 1

                If 0 < i.bitCount() AndAlso i.bitCount() <= k Then

                    Dim part As List(Of Integer) = New List(Of Integer)()

                    ' Print current subset
                    For j = 0 To n - 1

                        ' (1<<j) is a number with jth bit 1
                        ' so when we 'and' them with the
                        ' subset number we get which numbers
                        ' are present in the subset and which
                        ' are not
                        If (i And 1 << j) > 0 Then
                            part.Add(anc(j))
                        End If

                    Next

                    total.Add(part)

                End If
            Next

            total.Add(New List(Of Integer)())

            Return total
        End Function

        Public Overridable Function Anc(scoresMatrix As Double()(), k As Integer) As IList(Of IList(Of Integer))

            Dim parents As IList(Of IList(Of Integer)) = New List(Of IList(Of Integer))()

            For i = 0 To N - 1

                Dim lAnc = ancestors(i)

                If lAnc.Count > 0 Then

                    Dim total = Subsets(lAnc, k)

                    Dim score_max = Double.NegativeInfinity

                    Dim best_anc As List(Of Integer) = New List(Of Integer)()

                    For j = 0 To total.Count - 1

                        Dim score As Double = 0

                        For h = 0 To total(j).Count - 1
                            score += scoresMatrix(i)(total(j)(h))
                        Next

                        If score > score_max Then
                            score_max = score
                            best_anc = total(j)
                        End If

                    Next

                    parents.Add(best_anc)
                Else
                    parents.Add(New List(Of Integer)())
                End If

            Next
            Return parents
        End Function

        Public Overridable Sub Ckg(scoresMatrix As Double()(), sf As ScoringFunction, observations As Observations, k As Integer)
            ' Get BFS order of branchingEdges_partial
            BFS()

            ' Consistent graph
            Dim branchingEdges2 As IList(Of Edge) = New List(Of Edge)()

            For i = 0 To N - 1
                Dim anc = ancestors(i)

                If anc.Count > 0 Then

                    Dim total = Subsets(anc, k)

                    Dim score_max = Double.NegativeInfinity

                    Dim best_anc As List(Of Integer) = New List(Of Integer)()

                    For j = 0 To total.Count - 1
                        Dim score As Double = 0

                        For h = 0 To total(j).Count - 1
                            score += scoresMatrix(i)(total(j)(h))
                        Next

                        If score > score_max Then
                            score_max = score
                            best_anc = total(j)
                        End If

                    Next

                    For m = 0 To best_anc.Count - 1
                        Dim e As Edge = New Edge(best_anc(m), i)
                        branchingEdges2.Add(e)
                    Next

                End If

            Next

            branchingField = branchingEdges2

        End Sub

        Public Overridable Sub BFS()
            ' Mark all the vertices as not visited(By default

            Dim order As IList(Of Integer) = New List(Of Integer)()

            Dim branching_total As IList(Of Edge) = New List(Of Edge)()

            ' set as false)
            Dim visited = New Boolean(N - 1) {}

            ' Create a queue for BFS
            Dim queue As LinkedList(Of Integer) = New LinkedList(Of Integer)()

            ' Mark the current node as visited and enqueue it
            visited(root) = True
            queue.AddLast(root)

            While queue.Count <> 0
                ' Dequeue a vertex from queue and print it
                root = queue.First.Value
                queue.RemoveFirst()
                order.Add(root)

                ' Get all adjacent vertices of the dequeued vertex s
                ' If a adjacent has not been visited, then mark it
                ' visited and enqueue it
                Dim i As IEnumerator(Of Integer) = Adj(root).GetEnumerator()
                While i.MoveNext()
                    Dim m = i.Current
                    If Not visited(m) Then
                        visited(m) = True
                        queue.AddLast(m)
                    End If
                End While
            End While

            For i = 0 To order.Count - 1 - 1
                branching_total.Add(New Edge(order(i), order(i + 1)))

            Next

            branchingField = branching_total
        End Sub

        ''' <summary>
        ''' Merges two sorted list of edges, eliminating those that are inside the
        ''' strongly-connected component passed as argument. Incoming lists must be
        ''' sorted by tail/source. If there is more than one edge with the same source,
        ''' keeps only the heaviest.
        ''' </summary>
        ''' <param name="l1">        first sorted list </param>
        ''' <param name="l2">        second sorted list </param>
        ''' <param name="scc">       strongly-connect components </param>
        ''' <param name="component"> id of the relevant component </param>
        ''' <returns> merged list </returns>
        Private Shared Function merge(l1 As IList(Of Edge), l2 As IList(Of Edge), scc As DisjointSets, component As Integer) As IList(Of Edge)
            Dim merged As IList(Of Edge) = New List(Of Edge)(l1.Count + l2.Count)

            Dim i1 As RewindableEnumerator(Of Edge) = l1.GetEnumerator().AsRewindable()
            Dim i2 As RewindableEnumerator(Of Edge) = l2.GetEnumerator().AsRewindable()

            While i1.MoveNext() AndAlso i2.MoveNext()
                ' skip edges inside the strongly-connected component
                While i1.MoveNext()
                    If scc.find(i1.Current.Tail) <> component Then
                        i1.Previous()
                        Exit While
                    End If
                End While
                While i2.MoveNext()
                    If scc.find(i2.Current.Tail) <> component Then
                        i2.Previous()
                        Exit While
                    End If
                End While


                If Not i1.MoveNext() AndAlso Not i2.MoveNext() Then
                    Exit While
                End If


                If Not i1.MoveNext() Then

                    merged.Add(i2.Current)


                ElseIf Not i2.MoveNext() Then

                    ' i1.hasNext() && i2.hasNext()
                    merged.Add(i1.Current)
                Else
                    Dim e1 = i1.Current

                    Dim e2 = i2.Current

                    If e1.Tail < e2.Tail Then
                        merged.Add(e1)
                        i2.Previous()

                    ElseIf e1.Tail > e2.Tail Then
                        merged.Add(e2)

                        ' if both have the same source, keep the heaviest
                        i1.Previous()
                    Else
                        If e1.Weight > e2.Weight Then
                            merged.Add(e1)
                        Else
                            merged.Add(e2)
                        End If
                    End If
                End If
            End While

            Return merged
        End Function

    End Class

End Namespace
