#Region "Microsoft.VisualBasic::5052d70fcec8d94798c791c553235e65, Data_science\Graph\KNN\HNSW\NodeAlgorithm34.vb"

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

    '   Total Lines: 134
    '    Code Lines: 54 (40.30%)
    ' Comment Lines: 63 (47.01%)
    '    - Xml Docs: 41.27%
    ' 
    '   Blank Lines: 17 (12.69%)
    '     File Size: 6.70 KB


    '     Class NodeAlg3
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: SelectBestForConnecting
    ' 
    '     Class NodeAlg4
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: SelectBestForConnecting
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' <copyright file="SmallWorld.Node.cs" company="Microsoft">
' Copyright (c) Microsoft Corporation. All rights reserved.
' Licensed under the MIT License.
' </copyright>

Namespace KNearNeighbors.HNSW

    ' <content>
    ' The part with the implementaion of a node in the hnsw graph.
    ' </content>
    '

    ''' <summary>
    ''' The implementation of the SELECT-NEIGHBORS-SIMPLE(q, C, M) algorithm.
    ''' Article: Section 4. Algorithm 3.
    ''' </summary>
    Friend Class NodeAlg3(Of TItem, TDistance As IComparable(Of TDistance))
        Inherits Node(Of TItem, TDistance)
        ''' <summary>
        ''' Initializes a new instance of the <see cref="NodeAlg3"/> class.
        ''' </summary>
        ''' <param name="id">The identifier of the node.</param>
        ''' <param name="item">The item which is represented by the node.</param>
        ''' <param name="maxLevel">The maximum level until which the node exists.</param>
        ''' <param name="distance">The distance function for attached items.</param>
        ''' <param name="parameters">The parameters of the algorithm.</param>
        Public Sub New(id As Integer, item As TItem, maxLevel As Integer, distance As Func(Of TItem, TItem, TDistance), parameters As Parameters(Of TItem, TDistance))
            MyBase.New(id, item, maxLevel, distance, parameters)
        End Sub

        ''' <inheritdoc/>
        Public Overrides Function SelectBestForConnecting(candidates As IList(Of Node(Of TItem, TDistance))) As IList(Of Node(Of TItem, TDistance))
            ' 
            ' q ← this
            ' return M nearest elements from C to q

            Dim fartherIsLess As IComparer(Of Node(Of TItem, TDistance)) = TravelingCosts.Reverse()
            Dim candidatesHeap = New BinaryHeap(Of Node(Of TItem, TDistance))(candidates, fartherIsLess)

            Dim result = New List(Of Node(Of TItem, TDistance))(GetM(Parameters.M, MaxLevel) + 1)
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
    Friend Class NodeAlg4(Of TItem, TDistance As IComparable(Of TDistance))
        Inherits Node(Of TItem, TDistance)
        ''' <summary>
        ''' Initializes a new instance of the <see cref="NodeAlg4"/> class.
        ''' </summary>
        ''' <param name="id">The identifier of the node.</param>
        ''' <param name="item">The item which is represented by the node.</param>
        ''' <param name="maxLevel">The maximum level until which the node exists.</param>
        ''' <param name="distance">The distance function for attached items.</param>
        ''' <param name="parameters">The parameters of the algorithm.</param>
        Public Sub New(id As Integer, item As TItem, maxLevel As Integer, distance As Func(Of TItem, TItem, TDistance), parameters As Parameters(Of TItem, TDistance))
            MyBase.New(id, item, maxLevel, distance, parameters)
        End Sub

        ''' <inheritdoc/>
        Public Overrides Function SelectBestForConnecting(candidates As IList(Of Node(Of TItem, TDistance))) As IList(Of Node(Of TItem, TDistance))
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


            Dim closerIsLess As IComparer(Of Node(Of TItem, TDistance)) = TravelingCosts
            Dim fartherIsLess As IComparer(Of Node(Of TItem, TDistance)) = closerIsLess.Reverse()

            Dim resultHeap = New BinaryHeap(Of Node(Of TItem, TDistance))(New List(Of Node(Of TItem, TDistance))(GetM(Parameters.M, MaxLevel) + 1), closerIsLess)
            Dim candidatesHeap = New BinaryHeap(Of Node(Of TItem, TDistance))(candidates, fartherIsLess)

            ' expand candidates option is enabled
            If Parameters.ExpandBestSelection Then
                Dim candidatesIds = New HashSet(Of Integer)(candidates.Select(Function(c) c.Id))
                For Each neighbour In GetConnections(MaxLevel)
                    If Not candidatesIds.Contains(neighbour.Id) Then
                        candidatesHeap.Push(neighbour)
                        candidatesIds.Add(neighbour.Id)
                    End If
                Next
            End If

            ' main stage of moving candidates to result
            Dim discardedHeap = New BinaryHeap(Of Node(Of TItem, TDistance))(New List(Of Node(Of TItem, TDistance))(candidatesHeap.Buffer.Count), fartherIsLess)
            While candidatesHeap.Buffer.Any() AndAlso resultHeap.Buffer.Count < GetM(Parameters.M, MaxLevel)
                Dim candidate = candidatesHeap.Pop()
                Dim farestResult = resultHeap.Buffer.FirstOrDefault()

                If farestResult Is Nothing OrElse SmallWorld(Of TItem, TDistance).DLt(TravelingCosts.From(candidate), TravelingCosts.From(farestResult)) Then
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
End Namespace

