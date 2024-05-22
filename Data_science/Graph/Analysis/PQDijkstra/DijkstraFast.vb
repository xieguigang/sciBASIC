#Region "Microsoft.VisualBasic::6348fc46c6eb133242e3ac29a5b16625, Data_science\Graph\Analysis\PQDijkstra\DijkstraFast.vb"

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

    '   Total Lines: 269
    '    Code Lines: 132 (49.07%)
    ' Comment Lines: 103 (38.29%)
    '    - Xml Docs: 62.14%
    ' 
    '   Blank Lines: 34 (12.64%)
    '     File Size: 11.55 KB


    '     Class DijkstraFast
    ' 
    ' 
    '         Delegate Function
    ' 
    ' 
    '         Delegate Function
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Function: (+2 Overloads) GetMinimumPath, GetStartingBestPath, GetStartingTraversalCost, Perform, Perform2
    '         Structure Results
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    '         Class QueueElement
    ' 
    '             Constructor: (+2 Overloads) Sub New
    '             Function: CompareTo
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'http://www.codeproject.com/Articles/24816/A-Fast-Priority-Queue-Implementation-of-the-Dijkst?msg=4908520#xx4908520xx
'A Fast Priority Queue Implementation of the Dijkstra Shortest Path Algorithm
'Tolga Birdal, 5 Aug 2013

Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace Dijkstra.PQDijkstra

    ''' <summary> 
    ''' Implements a generalized Dijkstra's algorithm to calculate 
    ''' both minimum distance and minimum path. 
    ''' </summary> 
    ''' <remarks> 
    ''' For this algorithm, all nodes should be provided, and handled 
    ''' in the delegate methods, including the start and finish nodes. 
    ''' </remarks> 
    Public Class DijkstraFast
        ''' <summary> 
        ''' An optional delegate that can help optimize the algorithm 
        ''' by showing it a subset of nodes to consider. Very useful 
        ''' for limited connectivity graphs. (like pixels on a screen!) 
        ''' </summary> 
        ''' <param name="startingNode"> 
        ''' The node that is being traveled away FROM. 
        ''' </param> 
        ''' <returns> 
        ''' An array of nodes that might be reached from the  
        ''' <paramref name="startingNode"/>. 
        ''' </returns> 
        Public Delegate Function NearbyNodesHint(startingNode As Integer) As IEnumerable(Of Integer)
        ''' <summary> 
        ''' Determines the cost of moving from a given node to another given node. 
        ''' </summary> 
        ''' <param name="start"> 
        ''' The node being moved away from. 
        ''' </param> 
        ''' <param name="finish"> 
        ''' The node that may be moved to. 
        ''' </param> 
        ''' <returns> 
        ''' The cost of the transition from <paramref name="start"/> to 
        ''' <paramref name="finish"/>, or <see cref="Int32.MaxValue"/> 
        ''' if the transition is impossible (i.e. there is no edge between  
        ''' the two nodes). 
        ''' </returns> 
        Public Delegate Function InternodeTraversalCost(start As Integer, finish As Integer) As Single

        ''' <summary> 
        ''' Creates an instance of the <see cref="Dijkstra"/> class. 
        ''' </summary> 
        ''' <param name="totalNodeCount"> 
        ''' The total number of nodes in the graph. 
        ''' </param> 
        ''' <param name="traversalCost"> 
        ''' The delegate that can provide the cost of a transition between 
        ''' any two nodes. 
        ''' </param> 
        ''' <param name="hint"> 
        ''' An optional delegate that can provide a small subset of nodes 
        ''' that a given node may be connected to. 
        ''' </param> 
        Public Sub New(totalNodeCount As Integer, traversalCost As InternodeTraversalCost, hint As NearbyNodesHint)
            If totalNodeCount < 3 Then
                Throw New ArgumentOutOfRangeException("totalNodeCount", totalNodeCount, "Expected a minimum of 3.")
            End If
            If traversalCost Is Nothing Then
                Throw New ArgumentNullException("traversalCost")
            End If

            Me.Hint = hint
            Me.TraversalCost = traversalCost
            Me.TotalNodeCount = totalNodeCount
        End Sub

        Protected ReadOnly Hint As NearbyNodesHint
        Protected ReadOnly TraversalCost As InternodeTraversalCost
        Protected ReadOnly TotalNodeCount As Integer

        Public Structure Results
            ''' <summary> 
            ''' Prepares a Dijkstra results package. 
            ''' </summary> 
            ''' <param name="minimumPath__1"> 
            ''' The minimum path array, where each array element index corresponds  
            ''' to a node designation, and the array element value is a pointer to 
            ''' the node that should be used to travel to this one. 
            ''' </param> 
            ''' <param name="minimumDistance__2"> 
            ''' The minimum distance from the starting node to the given node. 
            ''' </param> 
            Public Sub New(minimumPath__1 As Integer(), minimumDistance__2 As Single())
                MinimumDistance = minimumDistance__2
                MinimumPath = minimumPath__1
            End Sub

            ''' The minimum path array, where each array element index corresponds  
            ''' to a node designation, and the array element value is a pointer to 
            ''' the node that should be used to travel to this one. 
            Public ReadOnly MinimumPath As Integer()

            ''' The minimum distance from the starting node to the given node. 
            Public ReadOnly MinimumDistance As Single()
        End Structure

        Public Class QueueElement
            Implements IComparable
            Public index As Integer
            Public weight As Single

            Public Sub New()
            End Sub
            Public Sub New(i As Integer, val As Single)
                index = i
                weight = val
            End Sub

            Public Function CompareTo(obj As Object) As Integer Implements IComparable.CompareTo
                Dim outer As QueueElement = DirectCast(obj, QueueElement)

                If Me.weight > outer.weight Then
                    Return 1
                ElseIf Me.weight < outer.weight Then
                    Return -1
                Else
                    Return 0
                End If
            End Function
        End Class


        ' start: The node to use as a starting location. 
        ' A struct containing both the minimum distance and minimum path 
        ' to every node from the given <paramref name="start"/> node. 
        Public Overridable Function Perform(start As Integer) As Results
            ' Initialize the distance to every node from the starting node. 
            Dim d As Single() = GetStartingTraversalCost(start)

            ' Initialize best path to every node as from the starting node. 
            Dim p As Integer() = GetStartingBestPath(start)
            Dim Q As New BasicHeap()
            'FastHeap Q = new FastHeap(TotalNodeCount);

            Dim i As Integer = 0
            While i <> TotalNodeCount
                Q.Push(i, d(i))
                i += 1
            End While

            While Q.Count <> 0
                Dim v As Integer = Q.Pop()
                For Each w As Integer In Hint(v)
                    If (w < 0 OrElse w > Q.Count - 1) Then Continue For 'Potential bugs

                    Dim cost As Single = TraversalCost(v, w)
                    If cost < Single.MaxValue AndAlso d(v) + cost < d(w) Then
                        ' don't let wrap-around negatives slip by 
                        ' We have found a better way to get at relative 
                        d(w) = d(v) + cost
                        ' record new distance 
                        p(w) = v
                        Q.Push(w, d(w))
                    End If
                Next
            End While

            Return New Results(p, d)
        End Function

        ' start: The node to use as a starting location. 
        ' A struct containing both the minimum distance and minimum path 
        ' to every node from the given <paramref name="start"/> node. 
        Public Overridable Function Perform2(start As Integer) As Results
            ' Initialize the distance to every node from the starting node. 
            Dim d As Single() = GetStartingTraversalCost(start)

            ' Initialize best path to every node as from the starting node. 
            Dim p As Integer() = GetStartingBestPath(start)
            Dim Q As New BinaryPriorityQueue()

            Dim i As Integer = 0
            While i <> TotalNodeCount
                Q.Push(New QueueElement(i, d(i)))
                i += 1
            End While

            While Q.Count <> 0
                Dim v As Integer = DirectCast(Q.Pop(), QueueElement).index

                For Each w As Integer In Hint(v)
                    'if (w <0 || w > Q.Count-1) continue;

                    Dim cost As Single = TraversalCost(v, w)
                    If cost < Single.MaxValue AndAlso d(v) + cost < d(w) Then
                        ' don't let wrap-around negatives slip by 
                        ' We have found a better way to get at relative 
                        d(w) = d(v) + cost
                        ' record new distance 
                        p(w) = v
                        Q.Push(New QueueElement(w, d(w)))
                    End If
                Next
            End While

            Return New Results(p, d)
        End Function

        ' Uses the Dijkstra algorithhm to find the minimum path 
        ' from one node to another. 
        ' Return a struct containing both the minimum distance and minimum path 
        ' to every node from the given start node. 
        Public Overridable Function GetMinimumPath(start As Integer, finish As Integer) As Integer()
            If start < finish Then
                Dim tmp As Integer = start
                start = finish
                finish = tmp
            End If

            Dim results As Results = Perform(start)
            Return GetMinimumPath(start, finish, results.MinimumPath)
        End Function

        ' Finds an array of nodes that provide the shortest path 
        ' from one given node to another. 
        ' ShortestPath : P array of the completed algorithm:
        ' The list of nodes that provide the one step at a time path from 
        Protected Overridable Function GetMinimumPath(start As Integer, finish As Integer, shortestPath As Integer()) As Integer()
            Dim path As New Stack(Of Integer)()

            Do
                path.Push(finish)
                ' step back one step toward the start point 
                finish = shortestPath(finish)
            Loop While finish <> start
            Return path.ToArray()
        End Function

        ' Initializes the P array for the algorithm. 
        ' A fresh P array will set every single node's source node to be  
        ' the starting node, including the starting node itself. 
        Protected Overridable Function GetStartingBestPath(startingNode As Integer) As Integer()
            Dim p As Integer() = New Integer(TotalNodeCount - 1) {}
            For i As Integer = 0 To p.Length - 1
                p(i) = startingNode
            Next
            Return p
        End Function

        ' Initializes the D array for the start of the algorithm.
        ' The traversal cost for every node will be set to impossible 
        ' (int.MaxValue) unless a connecting edge is found between the 
        ' starting node and the node in question.
        Protected Overridable Function GetStartingTraversalCost(start As Integer) As Single()
            Dim subset As Single() = New Single(TotalNodeCount - 1) {}
            Dim i As Integer = 0
            While i <> subset.Length
                subset(i) = Single.MaxValue
                i += 1
            End While
            ' all are unreachable 
            subset(start) = 0
            ' zero cost from start to start 
            For Each nearby As Integer In Hint(start)
                subset(nearby) = TraversalCost(start, nearby)
            Next
            Return subset
        End Function

    End Class
End Namespace
