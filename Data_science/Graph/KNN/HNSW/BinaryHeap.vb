' <copyright file="BinaryHeap.cs" company="Microsoft">
' Copyright (c) Microsoft Corporation. All rights reserved.
' Licensed under the MIT License.
' </copyright>

Namespace KNearNeighbors.HNSW

    ''' <summary>
    ''' Binary heap wrapper around the <see cref="IList(Of T)"/>
    ''' It's a max-heap implementation i.e. the maximum element is always on top.
    ''' But the order of elements can be customized by providing <see cref="IComparer(Of T)"/> instance.
    ''' </summary>
    ''' <typeparam name="T">The type of the items in the source list.</typeparam>
    Public Class BinaryHeap(Of T)

        ''' <summary>
        ''' Gets the heap comparer.
        ''' </summary>
        Private _Comparer As System.Collections.Generic.IComparer(Of T)
        ''' <summary>
        ''' Gets the buffer of the heap.
        ''' </summary>
        Dim _Buffer As System.Collections.Generic.IList(Of T)

        ''' <summary>
        ''' Initializes a new instance of the <see cref="BinaryHeap(Of T)"/> class.
        ''' </summary>
        ''' <param name="buffer">The buffer to store heap items.</param>
        Public Sub New(buffer As IList(Of T))
            Me.New(buffer, Generic.Comparer(Of T).Default)
        End Sub

        ''' <summary>
        ''' Initializes a new instance of the <see cref="BinaryHeap(Of T)"/> class.
        ''' </summary>
        ''' <param name="buffer">The buffer to store heap items.</param>
        ''' <param name="comparer">The comparer which defines order of items.</param>
        Public Sub New(buffer As IList(Of T), comparer As IComparer(Of T))
            If buffer Is Nothing Then
                Throw New ArgumentNullException(NameOf(buffer))
            End If

            Me.Buffer = buffer
            Me.Comparer = comparer
            Dim i = 1

            While i < Me.Buffer.Count
                SiftUp(i)
                i=i+1
            End While
        End Sub

        Public Property Comparer As IComparer(Of T)
            Get
                Return _Comparer
            End Get
            Private Set(value As IComparer(Of T))
                _Comparer = value
            End Set
        End Property

        Public Property Buffer As IList(Of T)
            Get
                Return _Buffer
            End Get
            Private Set(value As IList(Of T))
                _Buffer = value
            End Set
        End Property

        ''' <summary>
        ''' Pushes item to the heap.
        ''' </summary>
        ''' <param name="item">The item to push.</param>
        Public Sub Push(item As T)
            Buffer.Add(item)
            SiftUp(Buffer.Count - 1)
        End Sub

        ''' <summary>
        ''' Pops the item from the heap.
        ''' </summary>
        ''' <returns>The popped item.</returns>
        Public Function Pop() As T
            If Buffer.Any() Then
                Dim result = Buffer.First()

                Buffer(0) = Buffer.Last()
                Buffer.RemoveAt(Buffer.Count - 1)
                SiftDown(0)

                Return result
            End If

            Throw New InvalidOperationException("Heap is empty")
        End Function

        ''' <summary>
        ''' Restores the heap property starting from i'th position down to the bottom
        ''' given that the downstream items fulfill the rule.
        ''' </summary>
        ''' <param name="i">The position of item where heap property is violated.</param>
        Private Sub SiftDown(i As Integer)
            While i < Buffer.Count
                Dim l = 2 * i + 1
                Dim r = l + 1
                If l >= Buffer.Count Then
                    Exit While
                End If

                Dim m = If(r < Buffer.Count AndAlso Comparer.Compare(Buffer(l), Buffer(r)) < 0, r, l)
                If Comparer.Compare(Buffer(m), Buffer(i)) <= 0 Then
                    Exit While
                End If

                Swap(i, m)
                i = m
            End While
        End Sub

        ''' <summary>
        ''' Restores the heap property starting from i'th position up to the head
        ''' given that the upstream items fulfill the rule.
        ''' </summary>
        ''' <param name="i">The position of item where heap property is violated.</param>
        Private Sub SiftUp(i As Integer)
            While i > 0
                Dim p As Integer = (i - 1) / 2
                If Comparer.Compare(Buffer(i), Buffer(p)) <= 0 Then
                    Exit While
                End If

                Swap(i, p)
                i = p
            End While
        End Sub

        ''' <summary>
        ''' Swaps items with the specified indicies.
        ''' </summary>
        ''' <param name="i">The first index.</param>
        ''' <param name="j">The second index.</param>
        Private Sub Swap(i As Integer, j As Integer)
            Dim temp = Buffer(i)
            Buffer(i) = Buffer(j)
            Buffer(j) = temp
        End Sub
    End Class
End Namespace
