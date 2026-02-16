' <copyright file="SmallWorld.Utils.cs" company="Microsoft">
' Copyright (c) Microsoft Corporation. All rights reserved.
' Licensed under the MIT License.
' </copyright>

Imports System.Diagnostics.CodeAnalysis

Namespace KNearNeighbors.HNSW

    ' <content>
    ' The part with the auxiliary tools for hnsw algorithm.
    ' </content>
    Partial Public Class SmallWorld(Of TItem, TDistance As IComparable(Of TDistance))
        ''' <summary>
        ''' Distance is Lower Than.
        ''' </summary>
        ''' <param name="x">Left argument.</param>
        ''' <param name="y">Right argument.</param>
        ''' <returns>True if x &lt; y.</returns>
        <SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification:="By Design")>
        Public Shared Function DLt(x As TDistance, y As TDistance) As Boolean
            Return x.CompareTo(y) < 0
        End Function

        ''' <summary>
        ''' Distance is Greater Than.
        ''' </summary>
        ''' <param name="x">Left argument.</param>
        ''' <param name="y">Right argument.</param>
        ''' <returns>True if x &gt; y.</returns>
        <SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification:="By Design")>
        Public Shared Function DGt(x As TDistance, y As TDistance) As Boolean
            Return x.CompareTo(y) > 0
        End Function

        ''' <summary>
        ''' Distances are Equal.
        ''' </summary>
        ''' <param name="x">Left argument.</param>
        ''' <param name="y">Right argument.</param>
        ''' <returns>True if x == y.</returns>
        <SuppressMessage("Design", "CA1000:Do not declare static members on generic types", Justification:="By Design")>
        Public Shared Function DEq(x As TDistance, y As TDistance) As Boolean
            Return x.CompareTo(y) = 0
        End Function

        ''' <summary>
        ''' Runs breadth first search.
        ''' </summary>
        ''' <param name="entryPoint">The entry point.</param>
        ''' <param name="level">The level of the graph where to run BFS.</param>
        ''' <param name="visitAction">The action to perform on each node.</param>
        Friend Shared Sub BFS(entryPoint As Node(Of TItem, TDistance), level As Integer, visitAction As Action(Of Node(Of TItem, TDistance)))
            Dim visitedIds = New HashSet(Of Integer)()
            Dim expansionQueue = New Queue(Of Node(Of TItem, TDistance))({entryPoint})

            While expansionQueue.Any()
                Dim currentNode = expansionQueue.Dequeue()
                If Not visitedIds.Contains(currentNode.Id) Then
                    visitAction(currentNode)
                    visitedIds.Add(currentNode.Id)
                    For Each neighbour In currentNode.GetConnections(level)
                        expansionQueue.Enqueue(neighbour)
                    Next
                End If
            End While
        End Sub
    End Class
End Namespace
