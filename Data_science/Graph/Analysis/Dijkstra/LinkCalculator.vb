#Region "Microsoft.VisualBasic::501d840aeedf2f0300b14221d9165c2b, Data_science\Graph\Analysis\Dijkstra\LinkCalculator.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 230
    '    Code Lines: 147 (63.91%)
    ' Comment Lines: 36 (15.65%)
    '    - Xml Docs: 86.11%
    ' 
    '   Blank Lines: 47 (20.43%)
    '     File Size: 7.96 KB


    '     Class Neighbour
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class Node
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class QueueEntry
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Class Calculator
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: dijkstraNeighbours, DistanceMatrix, DistancesFromNode, PathFromNodeToNode, PathFromNodeToNodeWithPrevCost
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports number = System.Double
Imports std = System.Math

Namespace Analysis.Dijkstra

    Public Class Neighbour

        Public id As Integer
        Public distance As Double

        Public Sub New(id As Double, distance As Double)
            Me.id = id
            Me.distance = distance
        End Sub
    End Class

    Public Class Node

        Public id As Integer

        Public Sub New(id As Double)
            Me.neighbours = New Neighbour() {}
        End Sub

        Public neighbours As Neighbour()
        Public d As Integer
        Public prev As Node
        Public q As PairingHeap(Of Node)
    End Class

    Class QueueEntry

        Public node As Node
        Public prev As QueueEntry
        Public d As Double

        Public Sub New(node As Node, prev As QueueEntry, d As Double)
            Me.node = node
            Me.prev = prev
            Me.d = d
        End Sub
    End Class

    ''' <summary>
    ''' calculates all-pairs shortest paths or shortest paths from a single node
    ''' </summary>
    ''' <typeparam name="Link"></typeparam>
    Public Class Calculator(Of Link)

        Dim neighbours As Node()

        Public n As Integer
        Public es As Link()

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="n">number of nodes</param>
        ''' <param name="es">array of edges</param>
        ''' <param name="getSourceIndex"></param>
        ''' <param name="getTargetIndex"></param>
        ''' <param name="getLength"></param>
        Sub New(n As Integer, es As IEnumerable(Of Link),
                getSourceIndex As Func(Of Link, number),
                getTargetIndex As Func(Of Link, number),
                getLength As Func(Of Link, number))

            Dim i As Integer = n

            Me.n = n
            Me.es = es.ToArray
            Me.neighbours = New Node(Me.n - 1) {}

            While std.Max(Interlocked.Decrement(i), i + 1)
                Me.neighbours(i) = New Node(i)
            End While

            i = Me.es.Length

            While std.Max(Interlocked.Decrement(i), i + 1)
                Dim e = Me.es(i)
                Dim u = getSourceIndex(e)
                Dim v = getTargetIndex(e)
                Dim d = getLength(e)

                Me.neighbours(u).neighbours.Add(New Neighbour(v, d))
                Me.neighbours(v).neighbours.Add(New Neighbour(u, d))
            End While
        End Sub

        ''' <summary>
        ''' compute shortest paths for graph over n nodes with edges an array of source/target pairs
        ''' edges may optionally have a length attribute.  1 is the default.
        ''' Uses Johnson's algorithm.
        ''' </summary>
        ''' <returns>return the distance matrix</returns>
        Public Function DistanceMatrix() As Integer()()
            Dim D = New Integer(Me.n)() {}

            For i As Integer = 0 To Me.n - 1
                D(i) = Me.dijkstraNeighbours(i).ToArray
            Next

            Return D
        End Function

        ''' <summary>
        ''' get shortest paths from a specified start node
        ''' </summary>
        ''' <param name="start">start node index</param>
        ''' <returns>array of path lengths</returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function DistancesFromNode(start As Integer) As List(Of Integer)
            Return Me.dijkstraNeighbours(start)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function PathFromNodeToNode(start As Integer, [end] As Integer) As List(Of Integer)
            Return Me.dijkstraNeighbours(start, [end])
        End Function

        ''' <summary>
        ''' find shortest path from start to end, with the opportunity at
        ''' each edge traversal to compute a custom cost based on the
        ''' previous edge.  For example, to penalise bends.
        ''' </summary>
        ''' <param name="start"></param>
        ''' <param name="[end]"></param>
        ''' <param name="prevCost"></param>
        ''' <returns></returns>
        Public Function PathFromNodeToNodeWithPrevCost(start As Integer, [end] As Integer, prevCost As Func(Of Integer, Integer, Integer, Double)) As List(Of Integer)
            Dim q As New PriorityQueue(Of QueueEntry)(Function(a, b) a.d <= b.d)
            Dim u As Node = Me.neighbours(start)
            Dim qu As New QueueEntry(u, Nothing, 0)
            Dim visitedFrom As New Dictionary(Of String, Double)

            Call q.push(qu)

            While Not q.empty()
                qu = q.pop()
                u = qu.node

                If u.id = [end] Then
                    Exit While
                End If

                Dim i = u.neighbours.Length

                While std.Max(Interlocked.Decrement(i), i + 1)
                    Dim neighbour = u.neighbours(i)
                    Dim v = Me.neighbours(neighbour.id)

                    ' don't double back
                    If qu.prev IsNot Nothing AndAlso v.id = qu.prev.node.id Then
                        Continue While
                    End If

                    ' don't retraverse an edge if it has already been explored
                    ' from a lower cost route
                    Dim viduid = v.id & "," & u.id
                    If visitedFrom.ContainsKey(viduid) AndAlso visitedFrom(viduid) <= qu.d Then
                        Continue While
                    End If

                    Dim cc = If(qu.prev IsNot Nothing, prevCost(qu.prev.node.id, u.id, v.id), 0)
                    Dim t = qu.d + neighbour.distance + cc

                    ' store cost of this traversal
                    visitedFrom(viduid) = t
                    q.push(New QueueEntry(v, qu, t))
                End While
            End While

            Dim path As New List(Of Integer)

            While qu.prev IsNot Nothing
                qu = qu.prev
                path.Add(qu.node.id)
            End While

            Return path
        End Function

        Private Function dijkstraNeighbours(start As Integer, Optional dest As Integer = -1) As List(Of Integer)
            Dim q = New PriorityQueue(Of Node)(Function(a, b) a.d <= b.d)
            Dim i As Integer = Me.neighbours.Length
            Dim d As Integer() = New Integer(i - 1) {}

            While std.Max(Interlocked.Decrement(i), i + 1)
                Dim node = Me.neighbours(i)
                node.d = If(i = start, 0, number.PositiveInfinity)
                node.q = q.push(node)
            End While

            While Not q.empty()
                Dim u = q.pop()

                d(u.id) = u.d

                If u.id = dest Then
                    Dim path As New List(Of Integer)
                    Dim v = u
                    While v.prev IsNot Nothing
                        path.Add(v.prev.id)
                        v = v.prev
                    End While
                    Return path
                End If

                i = u.neighbours.Length

                While std.Max(Interlocked.Decrement(i), i + 1)
                    Dim neighbour = u.neighbours(i)
                    Dim v = Me.neighbours(neighbour.id)
                    Dim t = u.d + neighbour.distance
                    If u.d <> number.MaxValue AndAlso v.d > t Then
                        v.d = t
                        v.prev = u
                        q.reduceKey(v.q, v, Sub(e, qi) e.q = qi)
                    End If
                End While
            End While

            Return d.ToList
        End Function
    End Class
End Namespace
