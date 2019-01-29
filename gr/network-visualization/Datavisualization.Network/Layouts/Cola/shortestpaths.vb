Imports number = System.Double

Namespace Layouts.Cola

    Class Neighbour

        Public id As number
        Public distance As number

        Public Sub New(id As number, distance As number)
            Me.id = id
            Me.distance = distance
        End Sub
    End Class

    Class Node

        Public id As number

        Public Sub New(id As number)
            Me.neighbours = New Neighbour() {}
        End Sub
        Public neighbours As Neighbour()
        Private d As number
        Private prev As Node
        Private q As PairingHeap(Of Node)
    End Class

    Class QueueEntry

        Public node As Node
        Public prev As QueueEntry
        Public d As number

        Public Sub New(node As Node, prev As QueueEntry, d As number)
            Me.node = node
            Me.prev = prev
            Me.d = d
        End Sub
    End Class

    '*
    ' * calculates all-pairs shortest paths or shortest paths from a single node
    ' * @class Calculator
    ' * @constructor
    ' * @param n {number} number of nodes
    ' * @param es {Edge[]} array of edges
    ' 

    Class Calculator(Of Link)
        Private neighbours As Node()
        Public n As number
        Public es As Link()

        Private Sub New(n As number, es As Link(), getSourceIndex As Func(Of Link, number), getTargetIndex As Func(Of Link, number), getLength As Func(Of Link, number))
            Me.neighbours = New Array(Me.n)
            Dim i = Me.n
            While System.Math.Max(System.Threading.Interlocked.Decrement(i), i + 1)
                Me.neighbours(i) = New Node(i)
            End While

            i = Me.es.length
            While System.Math.Max(System.Threading.Interlocked.Decrement(i), i + 1)
                Dim e = Me.es(i)
                Dim u = getSourceIndex(e)
                Dim v = getTargetIndex(e)
                Dim d = getLength(e)
                Me.neighbours(u).neighbours.push(New Neighbour(v, d))
                Me.neighbours(v).neighbours.push(New Neighbour(u, d))
            End While
        End Sub

        '*
        '     * compute shortest paths for graph over n nodes with edges an array of source/target pairs
        '     * edges may optionally have a length attribute.  1 is the default.
        '     * Uses Johnson's algorithm.
        '     *
        '     * @method DistanceMatrix
        '     * @return the distance matrix
        '     

        Private Function DistanceMatrix() As number()()
            Dim D = New Array(Me.n)
            For i As var = 0 To Me.n - 1
                D(i) = Me.dijkstraNeighbours(i)
            Next
            Return D
        End Function

        '*
        '     * get shortest paths from a specified start node
        '     * @method DistancesFromNode
        '     * @param start node index
        '     * @return array of path lengths
        '     

        Private Function DistancesFromNode(start As number) As number()
            Return Me.dijkstraNeighbours(start)
        End Function

        Private Function PathFromNodeToNode(start As number, [end] As number) As number()
            Return Me.dijkstraNeighbours(start, [end])
        End Function

        ' find shortest path from start to end, with the opportunity at
        ' each edge traversal to compute a custom cost based on the
        ' previous edge.  For example, to penalise bends.
        Private Function PathFromNodeToNodeWithPrevCost(start As Integer, [end] As number, prevCost As Func(Of number, number, number, number)) As number()
            Dim q = New PriorityQueue(Of QueueEntry)(Function(a, b) a.d <= b.d)
            Dim u = Me.neighbours(start)
            Dim qu = New QueueEntry(u, Nothing, 0)
            Dim visitedFrom = New Object() {}
            q.push(qu)
            While Not q.empty()
                qu = q.pop()
                u = qu.node
                If u.id = [end] Then
                    Exit While
                End If
                Dim i = u.neighbours.length
                While System.Math.Max(System.Threading.Interlocked.Decrement(i), i + 1)
                    Dim neighbour = u.neighbours(i)

                    Dim v = Me.neighbours(neighbour.id)

                    ' don't double back
                    If qu.prev AndAlso v.id = qu.prev.node.id Then
                        Continue While
                    End If

                    ' don't retraverse an edge if it has already been explored
                    ' from a lower cost route
                    Dim viduid = v.id + ","c + u.id
                    If visitedFrom.indexOf(viduid) > -1 AndAlso visitedFrom(viduid) <= qu.d Then
                        Continue While
                    End If

                    Dim cc = If(qu.prev, prevCost(qu.prev.node.id, u.id, v.id), 0)
                    Dim t = qu.d + neighbour.distance + cc

                    ' store cost of this traversal
                    visitedFrom(viduid) = t
                    q.push(New QueueEntry(v, qu, t))
                End While
            End While
            Dim path As number() = {}
            While qu.prev
                qu = qu.prev
                path.push(qu.node.id)
            End While
            Return path
        End Function

        Private Function dijkstraNeighbours(start As number, Optional dest As number = -1) As number()
            Dim q = New PriorityQueue(Of Node)(Function(a, b) a.d <= b.d)
            Dim i As Integer = Me.neighbours.length
            Dim d As number() = New Array(i)

            While System.Math.Max(System.Threading.Interlocked.Decrement(i), i + 1)
                Dim node = Me.neighbours(i)
                node.d = If(i = start, 0, Number.POSITIVE_INFINITY)
                node.q = q.push(node)
            End While
            While Not q.empty()
                ' console.log(q.toString(function (u) { return u.id + "=" + (u.d === Number.POSITIVE_INFINITY ? "\u221E" : u.d.toFixed(2) )}));
                Dim u = q.pop()
                d(u.id) = u.d
                If u.id = dest Then
                    Dim path As number = {}
                    Dim v = u
                    While v.prev IsNot Nothing
                        path.push(v.prev.id)
                        v = v.prev
                    End While
                    Return path
                End If
                i = u.neighbours.length
                While System.Math.Max(System.Threading.Interlocked.Decrement(i), i + 1)
                    Dim neighbour = u.neighbours(i)
                    Dim v = Me.neighbours(neighbour.id)
                    Dim t = u.d + neighbour.distance
                    If u.d <> Number.MAX_VALUE AndAlso v.d > t Then
                        v.d = t
                        v.prev = u
                        q.reduceKey(v.q, v, Function(e, q) InlineAssignHelper(e.q, q))
                    End If
                End While
            End While
            Return d
        End Function
        Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
            target = value
            Return value
        End Function
    End Class
End Namespace