''' <summary>
''' Double ended queue
''' </summary>
''' <typeparamname="T">type of items in a Deque</typeparam>
Partial Public Class Deque(Of T) : Implements IDeque(Of T)
    ''' <summary>
    ''' contains actuall data od deque, Deque<T> provides abstraction above this data
    ''' </summary>
    ''' <typeparamname="S"></typeparam></summary>   
    Private Class n_Data(Of S)
        ''' <summary>
        ''' Number of elements in Deque<T>
        ''' </summary></summary>        Private _Count As Integer
        Private Shared ReadOnly sizeOfBlock = 128
        ''' <summary>
        ''' Current number of allocated references to data blocks, data blocks themselves doesn't have to be allocated yet
        ''' </summary>
        Private Property NumOfBlockRefs As Integer = 2
        ''' <summary>
        ''' Current number of actually allocated blocks
        ''' </summary>
        Private Property NumOfBlockInitialized As Integer = 2
        ''' <summary>
        ''' number of allocated indices before the Head of a Deque
        ''' to be able to allocate blocks only when its necessary - only one block at time
        ''' </summary>
        Private Property beforeFirst As Integer = sizeOfBlock
        ''' <summary>
        ''' index of the first allocated block - in array of block references
        ''' </summary>
        Private Property headBlockIndex As Integer = 0
        ''' <summary>
        ''' number of allocated indices after the Tail of a Deque
        ''' to be able to allocate blocks only when its necessary - only one block at time
        ''' </summary>
        Private ReadOnly Property afterLast As Integer
            Get
                Return (NumOfBlockInitialized + headBlockIndex) * sizeOfBlock - (HeadIndex + Count)
            End Get
        End Property

        ''' <summary>
        ''' index of the firts item in the Deque - relative to the first index in the first block there is reference to (not to the first block actually allocated)
        ''' pretends that data is stored linearly
        ''' </summary>
        Private Property HeadIndex As Integer = sizeOfBlock
        ''' <summary>
        ''' index of last item in the Deque
        ''' </summary>
        Private ReadOnly Property TailIndex As Integer
            Get

                If Count = 0 Then
                    Return 0
                Else
                    Return HeadIndex + Count - 1
                End If
            End Get
        End Property

        Public Property Count As Integer = 0

        'private int currCapacity { get => (NumOfBlockRefs * sizeOfBlock) - Count; }

        Private data = New S(1)() {}

        Public Sub New()
            data(0) = New S(sizeOfBlock - 1) {}
            data(1) = New S(sizeOfBlock - 1) {}
        End Sub

        ''' <summary>
        ''' allows to treat the Deque<T> as if it stored data linearly
        ''' </summary>
        ''' <paramname="i"></param>
        ''' <returns></returns></summary>      
        Default Public Property Item(ByVal i As Integer) As S
            Get

                If i < 0 Then
                    Throw New ArgumentOutOfRangeException()
                End If

                Dim index = i + HeadIndex

                If index > TailIndex Then
                    Throw New ArgumentOutOfRangeException()
                End If

                Return data(GetIndexOfBlock(index))(GetIndexInBlock(index))
            End Get
            Set(ByVal value As S)

                If i < 0 Then
                    Throw New ArgumentOutOfRangeException()
                End If

                Dim index = i + HeadIndex

                If index > TailIndex Then
                    Throw New ArgumentOutOfRangeException()
                End If

                data(GetIndexOfBlock(index))(GetIndexInBlock(index)) = value
            End Set
        End Property

        Private Function GetIndexOfBlock(ByVal i As Integer) As Integer
            Return i / sizeOfBlock
        End Function

        Private Function GetIndexInBlock(ByVal i As Integer) As Integer
            Return i Mod sizeOfBlock
        End Function

        ''' <summary>
        ''' Doubles the number of references to data blocks, copies existing data blocks to the middle of new reference array of double size
        ''' do not actually allocate any data blocks
        ''' </summary>
        Private Sub DoubleSize()
            Dim oldNumOfRefs = NumOfBlockRefs
            NumOfBlockRefs *= 2
            Dim newData = New S(NumOfBlockRefs - 1)() {}
            Dim occupiedIndex As Integer = NumOfBlockRefs / 4
            data.CopyTo(newData, occupiedIndex)
            data = newData
            HeadIndex = occupiedIndex * sizeOfBlock + HeadIndex
            headBlockIndex = occupiedIndex + headBlockIndex
        End Sub
        ''' <summary>
        ''' allocs one data block in front of the first block currently allocated
        ''' doubles the size of reference array if necessary
        ''' </summary>
        Private Sub AllocBlockBeginning()
            If HeadIndex <= 0 Then
                DoubleSize()
            End If

            headBlockIndex -= 1
            data(headBlockIndex) = New S(sizeOfBlock - 1) {}
            beforeFirst = sizeOfBlock
            NumOfBlockInitialized += 1
        End Sub
        ''' <summary>
        ''' allocs one data block in front of the first block currently allocated
        ''' doubles the size of reference array if necessary
        ''' </summary>
        Private Sub AllocBlockEnd()
            If TailIndex >= NumOfBlockRefs * sizeOfBlock - 1 Then '-1 to avoid accessing non existing array
                DoubleSize()
            End If

            data(headBlockIndex + NumOfBlockInitialized) = New S(sizeOfBlock - 1) {}
            NumOfBlockInitialized += 1 'this will increment afterLast by  128 - size of block
        End Sub
        ''' <summary>
        ''' Adds Item as a new Head of the Deque<T>, Count is incremented
        ''' </summary>
        ''' <paramname="item"></param></summary>       
        Public Sub AddBegining(ByVal item As S)
            If beforeFirst <= 0 Then
                AllocBlockBeginning()
            End If

            beforeFirst -= 1
            HeadIndex -= 1
            Count += 1
            Me(0) = item 'to 0 index relative to head index
        End Sub
        ''' <summary>
        ''' Adds Item as a new Tail of the Deque<T>, Count is incremented
        ''' </summary>
        ''' <paramname="item"></param></summary>   
        Public Sub AddEnd(ByVal item As S)
            If afterLast <= 0 Then
                AllocBlockEnd()
            End If

            Count += 1 'incrementing count without dekrementing head index will decrement afterLast
            Me(Count - 1) = item 'indexing relative to head index
        End Sub
        ''' <summary>
        ''' returns the firts element of the Deque<T> while removing it from Deque<T>
        ''' </summary>
        ''' <returns></returns></T></summary>       
        Public Function RemoveHead() As S
            If Count = 0 Then
                Throw New InvalidOperationException("Deque<T> is empty")
            End If

            Dim item = Me(0)
            Me(0) = Nothing
            HeadIndex += 1
            Count -= 1
            beforeFirst += 1
            Return item
        End Function
        ''' <summary>
        ''' returns the last element of the Deque<T> and removes it from Deque<T>
        ''' </summary>
        ''' <returns></returns></T></summary>     
        Public Function RemoveTail() As S
            If Count = 0 Then
                Throw New InvalidOperationException("Deque<T> is empty")
            End If

            Dim item = Me(Count - 1)
            Me(Count - 1) = Nothing
            Count -= 1
            Return item
        End Function
        ''' <summary>
        ''' Inserts element on a specified index id Deque<T>
        ''' Insert at the beggining or end in O(1)
        ''' Allows to insert item on index 0 of an empty Deque<T> - List<T> behaves the same way, I recon
        ''' </summary>
        ''' <paramname="index"></param>
        ''' <paramname="item"></param></T></T></summary>    
        Public Sub Insert(ByVal index As Integer, ByVal item As S)
            If index = Count Then
                AddEnd(item)
                Return
            End If

            If index = 0 Then
                AddBegining(item)
                Return
            End If

            If index < 0 OrElse index > Count Then
                Throw New ArgumentOutOfRangeException("My awesome exception")
            End If

            If afterLast <= 0 Then
                AllocBlockEnd()
            End If

            Count += 1
            'shift element to tail of the Deque<T> to make space to insert the new element to
            For i = Count - 1 To index + 1 Step -1
                Me(i) = Me(i - 1)
            Next

            Me(index) = item
        End Sub
        ''' <summary>
        ''' Removes an element from the specified index of Deque<T>, removal of the first and the last item in O(1)
        ''' </summary>
        ''' <paramname="index"></param></summary>      
        Public Sub RemoveAt(ByVal index As Integer)
            If index < 0 OrElse index >= Count Then
                Throw New ArgumentOutOfRangeException("My awesome exception")
            End If

            If index = 0 Then
                RemoveHead()
                Return
            End If

            If index = Count - 1 Then
                RemoveTail()
                Return
            End If

            For i = index To Count - 1 - 1
                Me(i) = Me(i + 1)
            Next

            Me(Count - 1) = Nothing
            Count -= 1
        End Sub
        ''' <summary>
        ''' Searches for the specified object and returns the zero-based index of the first occurrence within the entire Deque<T>.
        ''' </summary>
        ''' <paramname="S"></param>
        ''' <returns>e zero-based index of the first occurrence of item within the entire Deque<T>, if found; otherwise, -1.</returns></returns></summary> 
        Public Function IndexOf(ByVal item As S) As Integer
            For index = 0 To Count - 1

                If Equals(Me(index), item) Then
                    Return index
                End If
            Next

            Return -1
        End Function
        ''' <summary>
        ''' Removes item from the Deque<T>, removal of the first and the last item in O(1)
        ''' </summary>
        ''' <paramname="item"></param>
        ''' <returns></returns></summary>       
        Public Function Remove(ByVal item As S) As Boolean
            Dim index = IndexOf(item)

            If index = -1 Then
                Return False
            End If

            RemoveAt(index)
            Return True
        End Function
        ''' <summary>
        ''' Determines whether an element is in the Deque<T>.
        ''' </summary>
        ''' <paramname="item"></param>
        ''' <returns>true if item is found in the List<T>; otherwise, false</returns></returns></summary> 
        Public Function Contains(ByVal item As S) As Boolean
            For index = 0 To Count - 1

                If Equals(Me(index), item) Then
                    Return True
                End If
            Next

            Return False
        End Function
        ''' <summary>
        ''' Removes all elements from the Deque<T>.
        ''' </summary>
        Public Sub Clear()
            For i = 0 To Count - 1
                Me(i) = Nothing
            Next

            Count = 0
            HeadIndex = NumOfBlockRefs * sizeOfBlock / 2 + 1
            ' to reset a size of Deque allocated?
            'NumOfBlockRefs = 2;
            'HeadIndex = sizeOfBlock
            'headBlockIndex = 0
            'beforeFirst = sizeOfBlock
            'NumOfBlockInitialized = 2
            'data = New S(NumOfBlockRefs - 1)() {}
            'data(0) = New S(sizeOfBlock - 1) {}
            'data(1) = New S(sizeOfBlock - 1) {}
        End Sub
        ''' <summary>
        ''' Copies the entire Deque<T> to a compatible one-dimensional array, starting at the specified index of the target array.
        ''' </summary>
        ''' <paramname="array"></param>
        ''' <paramname="arrayIndex"></param>
        ''' </summary>
        ''' <paramname="array"></param>
        ''' <paramname="arrayIndex"></param>
        ''' <paramname="reversed"></param></summary>     
        Public Sub CopyTo(ByVal array As S(), ByVal arrayIndex As Integer, ByVal reversed As Boolean)
            If array Is Nothing Then
                Throw New ArgumentNullException()
            End If

            If arrayIndex < 0 OrElse arrayIndex >= array.Length Then
                Throw New ArgumentOutOfRangeException()
            End If

            If arrayIndex + Count >= array.Length Then
                Throw New ArgumentException("Target array is not long enought to contain source array beggining at given arrayIndex")
            End If

            If reversed Then
                For i = Count - 1 To 0 Step -1
                    array(arrayIndex + Count - 1 - i) = Me(i)
                Next
            Else

                For i = 0 To Count - 1
                    array(arrayIndex + i) = Me(i)
                Next
            End If
        End Sub
    End Class
End Class

