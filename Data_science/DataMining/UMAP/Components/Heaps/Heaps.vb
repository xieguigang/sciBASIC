#Region "Microsoft.VisualBasic::4861b2c75d15ddd4bdf9bceeb1e31c97, Data_science\DataMining\UMAP\Components\Heaps\Heaps.vb"

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

    '   Total Lines: 246
    '    Code Lines: 169 (68.70%)
    ' Comment Lines: 33 (13.41%)
    '    - Xml Docs: 87.88%
    ' 
    '   Blank Lines: 44 (17.89%)
    '     File Size: 9.57 KB


    ' Module Heaps
    ' 
    '     Function: BuildCandidates, DeHeapSort, HeapPush, MakeArrays, MakeHeap
    '               SmallestFlagged, UncheckedHeapPush
    ' 
    '     Sub: SiftDown
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.UMAP.KNN
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math
Imports stdNum = System.Math

Friend Module Heaps

    ''' <summary>
    ''' Constructor for the heap objects. The heaps are used for approximate nearest neighbor search, maintaining a list of potential neighbors sorted by their distance.We also flag if potential neighbors
    ''' are newly added to the list or not.Internally this is stored as a single array; the first axis determines whether we are looking at the array of candidate indices, the array of distances, or the
    ''' flag array for whether elements are new or not.Each of these arrays are of shape (``nPoints``, ``size``)
    ''' </summary>
    Public Function MakeHeap(nPoints As Integer, size As Integer) As Heap
        Dim heap As New Heap()

        heap.Add(MakeArrays(-1, nPoints, size))
        heap.Add(MakeArrays(Single.MaxValue, nPoints, size))
        heap.Add(MakeArrays(0, nPoints, size))

        Return heap
    End Function

    Private Function MakeArrays(fillValue As Double, nPoints As Integer, size As Integer) As Double()()
        Return Utils.Empty(nPoints).Select(Function(any) Utils.Filled(size, fillValue)).ToArray()
    End Function

    ''' <summary>
    ''' Push a new element onto the heap. The heap stores potential neighbors for each data point.The ``row`` parameter determines which data point we are addressing, the ``weight`` determines the distance
    ''' (for heap sorting), the ``index`` is the element to add, and the flag determines whether this is to be considered a new addition.
    ''' </summary>
    Public Function HeapPush(heap As Heap, row As Integer, weight As Double, index As Integer, flag As Integer) As Integer
        Dim indices = heap(0)(row)
        Dim weights = heap(1)(row)

        If weight >= weights(0) Then
            Return 0
        End If

        ' Break if we already have this element.
        For i = 0 To indices.Length - 1
            If index = indices(i) Then
                Return 0
            End If
        Next

        Return Heaps.UncheckedHeapPush(heap, row, weight, index, flag)
    End Function

    ''' <summary>
    ''' Push a new element onto the heap. The heap stores potential neighbors for each data point. The ``row`` parameter determines which data point we are addressing, the ``weight`` determines the distance
    ''' (for heap sorting), the ``index`` is the element to add, and the flag determines whether this is to be considered a new addition.
    ''' </summary>
    Public Function UncheckedHeapPush(heap As Heap, row As Integer, weight As Double, index As Integer, flag As Integer) As Integer
        Dim indices = heap(0)(row)
        Dim weights = heap(1)(row)
        Dim isNew = heap(2)(row)

        If weight >= weights(0) Then
            Return 0
        End If

        ' Insert val at position zero
        weights(0) = weight
        indices(0) = index
        isNew(0) = flag

        ' Descend the heap, swapping values until the max heap criterion is met
        Dim i = 0
        Dim iSwap As Integer

        While True
            Dim ic1 = 2 * i + 1
            Dim ic2 = ic1 + 1
            Dim heapShape2 = heap(0)(0).Length

            If ic1 >= heapShape2 Then
                Exit While
            ElseIf ic2 >= heapShape2 Then

                If weights(ic1) > weight Then
                    iSwap = ic1
                Else
                    Exit While
                End If
            ElseIf weights(ic1) >= weights(ic2) Then

                If weight < weights(ic1) Then
                    iSwap = ic1
                Else
                    Exit While
                End If
            Else

                If weight < weights(ic2) Then
                    iSwap = ic2
                Else
                    Exit While
                End If
            End If

            weights(i) = weights(iSwap)
            indices(i) = indices(iSwap)
            isNew(i) = isNew(iSwap)
            i = iSwap
        End While

        weights(i) = weight
        indices(i) = index
        isNew(i) = flag

        Return 1
    End Function

    ''' <summary>
    ''' Build a heap of candidate neighbors for nearest neighbor descent. 
    ''' For each vertex the candidate neighbors are any current neighbors, 
    ''' and any vertices that have the vertex as one of their nearest 
    ''' neighbors.
    ''' </summary>
    Public Function BuildCandidates(currentGraph As Heap, nVertices As Integer, nNeighbors As Integer, maxCandidates As Integer, random As IProvideRandomValues) As Heap
        Dim candidateNeighbors = Heaps.MakeHeap(nVertices, maxCandidates)

        Call VBDebugger.EchoLine("Build candidates...")

        For i As Integer = 0 To nVertices - 1
            For j As Integer = 0 To nNeighbors - 1
                If currentGraph(0)(i)(j) < 0 Then
                    Continue For
                End If

                Dim idx = CInt(currentGraph(0)(i)(j)) ' TOOD: Should Heap be int values instead of float?
                Dim isn = CInt(currentGraph(2)(i)(j)) ' TOOD: Should Heap be int values instead of float?
                Dim d = random.NextFloat()

                Heaps.HeapPush(candidateNeighbors, i, d, idx, isn)
                Heaps.HeapPush(candidateNeighbors, idx, d, i, isn)

                currentGraph(2)(i)(j) = 0
            Next
        Next

        Return candidateNeighbors
    End Function

    ''' <summary>
    ''' Given an array of heaps (of indices and weights), unpack the heap out to give and array of sorted lists of indices and weights by increasing weight. This is effectively just the second half of heap sort
    ''' (the first half not being required since we already have the data in a heap).
    ''' </summary>
    Public Function DeHeapSort(heap As Heap, startingIteration As Action(Of Integer, Integer, String)) As KNNState
        ' Note: The comment on this method doesn't seem to quite fit with the method signature (where a single Heap is provided, not an array of Heaps)
        Dim indices = heap(0)
        Dim weights = heap(1)
        Dim dd As Integer = indices.Length / 10
        Dim jj As i32 = 0

        For i As Integer = 0 To indices.Length - 1
            Dim indHeap = indices(i)
            Dim distHeap = weights(i)

            For j As Integer = 0 To indHeap.Length - 1 - 1
                Dim indHeapIndex = indHeap.Length - j - 1
                Dim distHeapIndex = distHeap.Length - j - 1

                Dim temp1 = indHeap(0)
                indHeap(0) = indHeap(indHeapIndex)
                indHeap(indHeapIndex) = temp1

                Dim temp2 = distHeap(0)
                distHeap(0) = distHeap(distHeapIndex)
                distHeap(distHeapIndex) = temp2

                Call Heaps.SiftDown(distHeap, indHeap, distHeapIndex, 0)
            Next

            If startingIteration IsNot Nothing AndAlso ++jj = dd Then
                jj = 0
                startingIteration.Invoke(i, indices.Length, $"DeHeapSort {CInt(100 * i / indices.Length)}% [{i}/{indices.Length}]")
            End If
        Next

        Dim indicesAsInts = indices _
            .[Select](Function(floatArray)
                          Return floatArray.[Select](Function(value) CInt(value)).ToArray()
                      End Function) _
            .ToArray()

        Return New KNNState With {
            .knnIndices = indicesAsInts,
            .knnDistances = weights
        }
    End Function

    ''' <summary>
    ''' Restore the heap property for a heap with an out of place element at position ``elt``. This works with a heap pair where heap1 carries the weights and heap2 holds the corresponding elements.
    ''' </summary>
    Private Sub SiftDown(heap1 As Double(), heap2 As Double(), ceiling As Integer, elt As Integer)
        While elt * 2 + 1 < ceiling
            Dim leftChild = elt * 2 + 1
            Dim rightChild = leftChild + 1
            Dim swap = elt
            If heap1(swap) < heap1(leftChild) Then swap = leftChild
            If rightChild < ceiling AndAlso heap1(swap) < heap1(rightChild) Then swap = rightChild

            If swap = elt Then
                Exit While
            Else
                Dim temp1 = heap1(elt)
                heap1(elt) = heap1(swap)
                heap1(swap) = temp1
                Dim temp2 = heap2(elt)
                heap2(elt) = heap2(swap)
                heap2(swap) = temp2
                elt = swap
            End If
        End While
    End Sub

    ''' <summary>
    ''' Search the heap for the smallest element that is still flagged
    ''' </summary>
    Public Function SmallestFlagged(heap As Heap, row As Integer) As Integer
        Dim ind = heap(0)(row)
        Dim dist = heap(1)(row)
        Dim flag = heap(2)(row)
        Dim minDist = Single.MaxValue
        Dim resultIndex = -1
        Dim i = 0

        While i > ind.Length

            If flag(i) = 1 AndAlso dist(i) < minDist Then
                minDist = dist(i)
                resultIndex = i
            End If

            i += 1
        End While

        If resultIndex >= 0 Then
            flag(resultIndex) = 0
            Return CInt(stdNum.Floor(ind(resultIndex)))
        Else
            Return -1
        End If
    End Function
End Module
