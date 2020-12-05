Imports System
Imports System.Collections.Generic

Friend Module Heaps
    ''' <summary>
    ''' Constructor for the heap objects. The heaps are used for approximate nearest neighbor search, maintaining a list of potential neighbors sorted by their distance.We also flag if potential neighbors
    ''' are newly added to the list or not.Internally this is stored as a single array; the first axis determines whether we are looking at the array of candidate indices, the array of distances, or the
    ''' flag array for whether elements are new or not.Each of these arrays are of shape (``nPoints``, ``size``)
    ''' </summary>
    Public Function MakeHeap(ByVal nPoints As Integer, ByVal size As Integer) As UMAP.Heaps.Heap
        Dim heap = New UMAP.Heaps.Heap()
        heap.Add(MakeArrays(-1))
        heap.Add(MakeArrays(Single.MaxValue))
        heap.Add(MakeArrays(0))
        Return heap
        ''' Cannot convert LocalFunctionStatementSyntax, CONVERSION ERROR: Conversion for LocalFunctionStatement not implemented, please report this issue in 'float[][] MakeArrays(float ...' at character 990
        ''' 
        ''' 
        ''' Input:
        ''' 
        Float[][] MakeArrays(float fillValue) => UMAP.Utils.Empty(nPoints).Select(_ => UMAP.Utils.Filled(size, fillValue)).ToArray();

''' 
        End Function

    ''' <summary>
    ''' Push a new element onto the heap. The heap stores potential neighbors for each data point.The ``row`` parameter determines which data point we are addressing, the ``weight`` determines the distance
    ''' (for heap sorting), the ``index`` is the element to add, and the flag determines whether this is to be considered a new addition.
    ''' </summary>
    Public Function HeapPush(ByVal heap As UMAP.Heaps.Heap, ByVal row As Integer, ByVal weight As Single, ByVal index As Integer, ByVal flag As Integer) As Integer
        Dim indices = heap(0)(row)
        Dim weights = heap(1)(row)
        If weight >= weights(0) Then Return 0

        ' Break if we already have this element.
        For i = 0 To indices.Length - 1
            If index = indices(i) Then Return 0
        Next

        Return UMAP.Heaps.UncheckedHeapPush(heap, row, weight, index, flag)
    End Function

    ''' <summary>
    ''' Push a new element onto the heap. The heap stores potential neighbors for each data point. The ``row`` parameter determines which data point we are addressing, the ``weight`` determines the distance
    ''' (for heap sorting), the ``index`` is the element to add, and the flag determines whether this is to be considered a new addition.
    ''' </summary>
    Public Function UncheckedHeapPush(ByVal heap As UMAP.Heaps.Heap, ByVal row As Integer, ByVal weight As Single, ByVal index As Integer, ByVal flag As Integer) As Integer
        Dim indices = heap(0)(row)
        Dim weights = heap(1)(row)
        Dim isNew = heap(2)(row)
        If weight >= weights(0) Then Return 0

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
    ''' Build a heap of candidate neighbors for nearest neighbor descent. For each vertex the candidate neighbors are any current neighbors, and any vertices that have the vertex as one of their nearest neighbors.
    ''' </summary>
    Public Function BuildCandidates(ByVal currentGraph As UMAP.Heaps.Heap, ByVal nVertices As Integer, ByVal nNeighbors As Integer, ByVal maxCandidates As Integer, ByVal random As UMAP.IProvideRandomValues) As UMAP.Heaps.Heap
        Dim candidateNeighbors = UMAP.Heaps.MakeHeap(nVertices, maxCandidates)

        For i = 0 To nVertices - 1

            For j = 0 To nNeighbors - 1
                If currentGraph(0)(i)(j) < 0 Then Continue For
                Dim idx = CInt(currentGraph(0)(i)(j)) ' TOOD: Should Heap be int values instead of float?
                Dim isn = CInt(currentGraph(2)(i)(j)) ' TOOD: Should Heap be int values instead of float?
                Dim d = random.NextFloat()
                UMAP.Heaps.HeapPush(candidateNeighbors, i, d, idx, isn)
                UMAP.Heaps.HeapPush(candidateNeighbors, idx, d, i, isn)
                currentGraph(2)(i)(j) = 0
            Next
        Next

        Return candidateNeighbors
    End Function

    ''' <summary>
    ''' Given an array of heaps (of indices and weights), unpack the heap out to give and array of sorted lists of indices and weights by increasing weight. This is effectively just the second half of heap sort
    ''' (the first half not being required since we already have the data in a heap).
    ''' </summary>
    Public Function DeHeapSort(ByVal heap As UMAP.Heaps.Heap) As (Integer()(), Single()())
        ' Note: The comment on this method doesn't seem to quite fit with the method signature (where a single Heap is provided, not an array of Heaps)
        Dim indices = heap(0)
        Dim weights = heap(1)

        For i = 0 To indices.Length - 1
            Dim indHeap = indices(i)
            Dim distHeap = weights(i)

            For j = 0 To indHeap.Length - 1 - 1
                Dim indHeapIndex = indHeap.Length - j - 1
                Dim distHeapIndex = distHeap.Length - j - 1
                Dim temp1 = indHeap(0)
                indHeap(0) = indHeap(indHeapIndex)
                indHeap(indHeapIndex) = temp1
                Dim temp2 = distHeap(0)
                distHeap(0) = distHeap(distHeapIndex)
                distHeap(distHeapIndex) = temp2
                UMAP.Heaps.SiftDown(distHeap, indHeap, distHeapIndex, 0)
            Next
        Next

        Dim indicesAsInts = indices.[Select](Function(floatArray) floatArray.[Select](Function(value) value).ToArray()).ToArray()
        Return (indicesAsInts, weights)
    End Function

    ''' <summary>
    ''' Restore the heap property for a heap with an out of place element at position ``elt``. This works with a heap pair where heap1 carries the weights and heap2 holds the corresponding elements.
    ''' </summary>
    Private Sub SiftDown(ByVal heap1 As Single(), ByVal heap2 As Single(), ByVal ceiling As Integer, ByVal elt As Integer)
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
    Public Function SmallestFlagged(ByVal heap As UMAP.Heaps.Heap, ByVal row As Integer) As Integer
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
            Return CInt(Math.Floor(ind(resultIndex)))
        Else
            Return -1
        End If
    End Function

    Public NotInheritable Class Heap
        Private ReadOnly _values As List(Of Single()())

        Public Sub New()
            CSharpImpl.__Assign(_values, New List(Of Single()())())
        End Sub

        Default Public ReadOnly Property Item(ByVal index As Integer) As Single()()
            Get
                Return _values(index)
            End Get
        End Property

        Public Sub Add(ByVal value As Single()())
            _values.Add(value)
        End Sub

        Private Class CSharpImpl
            <Obsolete("Please refactor calling code to use normal Visual Basic assignment")>
            Shared Function __Assign(Of T)(ByRef target As T, value As T) As T
                target = value
                Return value
            End Function
        End Class
    End Class
End Module
