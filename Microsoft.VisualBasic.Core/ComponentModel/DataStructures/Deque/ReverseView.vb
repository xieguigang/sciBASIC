Public Class ReverseView(Of T)
    Implements IDeque(Of T)
    ''' <summary>
    ''' Deque<T> that this instance of ReverseView wraps and allows to access in the reversed order
    ''' </summary></summary> 
    Private Property deque As Deque(Of T)

    Public Sub New(ByVal que As Deque(Of T))
        deque = que
    End Sub

    Default Public Property Item(ByVal index As Integer) As T Implements IList(Of T).Item
        Get
            Return deque(Count - 1 - index)
        End Get
        Set(ByVal value As T)
            deque(Count - 1 - index) = value
        End Set
    End Property

    ''' <summary>
    ''' peek the firts element of the reversed Deque<T>
    ''' </summary></summary>   
    Public ReadOnly Property First As T Implements IDeque(Of T).First
        Get
            Return deque.Last
        End Get
    End Property
    ''' <summary>
    ''' peek the last element of the reversed Deque<T>
    ''' </summary></summary>   
    Public ReadOnly Property Last As T Implements IDeque(Of T).Last
        Get
            Return deque.First
        End Get
    End Property
    ''' <summary>
    ''' Number of elements in Deque<T>
    ''' </summary></summary>   
    Public ReadOnly Property Count As Integer Implements ICollection(Of T).Count
        Get
            Return deque.Count
        End Get
    End Property

    Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of T).IsReadOnly
        Get
            Return deque.IsReadOnly
        End Get
    End Property
    ''' <summary>
    ''' Adds an object to the end of the reversed Deque<T>.
    ''' </summary>
    ''' <paramname="item"></param></summary> 
    Public Sub Add(ByVal item As T) Implements ICollection(Of T).Add
        deque.AddHead(item)
    End Sub
    ''' <summary>
    ''' Adds an element to the beggining of the reversed Deque<T>
    ''' </summary>
    ''' <paramname="item"></param></summary>  
    Public Sub AddHead(ByVal item As T) Implements IDeque(Of T).AddHead
        deque.Add(item)
    End Sub
    ''' <summary>
    ''' Removes all elements from the Deque<T>.
    ''' </summary></summary>    
    Public Sub Clear() Implements ICollection(Of T).Clear
        deque.Clear()
    End Sub
    ''' <summary>
    ''' Determines whether an element is in the Deque<T>.
    ''' </summary>
    ''' <paramname="item"></param>
    ''' <returns>true if item is found in the List<T>; otherwise, false</returns></returns></summary>  
    Public Function Contains(ByVal item As T) As Boolean Implements ICollection(Of T).Contains
        Return deque.Contains(item)
    End Function
    ''' <summary>
    ''' Copies the entire Deque<T> to a compatible one-dimensional array, starting at the specified index of the target array.
    ''' </summary>
    ''' <paramname="array"></param>
    ''' <paramname="arrayIndex"></param></summary>  
    Public Sub CopyTo(ByVal array As T(), ByVal arrayIndex As Integer) Implements ICollection(Of T).CopyTo
        deque.CopyToReversed(array, arrayIndex)
    End Sub

    Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
        Return New ReversedEnumerator(Of T)(deque, deque.version)
    End Function
    ''' <summary>
    ''' Searches for the specified object and returns the zero-based index of the first occurrence within the entire Deque<T>.
    ''' </summary>
    ''' <paramname="T"></param>
    ''' <returns>e zero-based index of the first occurrence of item within the entire Deque<T>, if found; otherwise, -1.</returns></returns></summary> 
    Public Function IndexOf(ByVal item As T) As Integer Implements IList(Of T).IndexOf
        For index = 0 To Count - 1

            If Equals(Me(index), item) Then
                Return index
            End If
        Next

        Return -1
    End Function
    ''' <summary>
    ''' Inserts an element into the reversedDeque<T> at the specified index.
    ''' </summary>
    ''' <paramname="index"></param>
    ''' <paramname="item"></param></summary>  
    Public Sub Insert(ByVal index As Integer, ByVal item As T) Implements IList(Of T).Insert
        'user wants to add item to begining of reversed list
        If index = 0 Then
            'I need to add it to end of actual list
            deque.Add(item)
            Return
        Else
            deque.Insert(Count - index, item)
        End If
    End Sub
    ''' <summary>
    ''' /// Removes the first occurrence of a specific object from the Deque<T>.
    ''' </summary>
    ''' <paramname="item"></param>
    ''' <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the List<T>.</returns></returns></summary>   
    Public Function Remove(ByVal item As T) As Boolean Implements ICollection(Of T).Remove
        Return deque.Remove(item)
    End Function
    ''' <summary>
    ''' Removes the element at the specified index of the Deque<T>.
    ''' </summary>
    ''' <paramname="index"></param></summary> 
    Public Sub RemoveAt(ByVal index As Integer) Implements IList(Of T).RemoveAt
        deque.RemoveAt(Count - 1 - index)
    End Sub
    ''' <summary>
    ''' returns the firts element of the reversed Deque<T> and removes it from Deque<T>
    ''' </summary>
    ''' <returns></returns></T></summary>  
    Public Function RemoveHead() As T Implements IDeque(Of T).RemoveHead
        Return deque.RemoveTail()
    End Function
    ''' <summary>
    ''' returns the last element of the reversed Deque<T> and removes it from Deque<T>
    ''' </summary>
    ''' <returns></returns></T></summary> 
    Public Function RemoveTail() As T Implements IDeque(Of T).RemoveTail
        Return deque.RemoveHead()
    End Function
    ''' <summary>
    ''' returns 
    ''' </summary>
    ''' <returns></returns>
    Public Function Reverse() As IDeque(Of T) Implements IDeque(Of T).Reverse
        Return deque
    End Function

    Private Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Throw New NotImplementedException()
    End Function

    Private Class ReversedEnumerator(Of S)
        Implements IEnumerator(Of S)

        Private Property curIndex As Integer
        ''' <summary>
        ''' version of Deque<T> this Enumerator is enumerating from the moment this enumerator has been created
        ''' </summary></summary>      
        Private Property version As Long
        ''' <summary>
        ''' Deque<T> this enumerator is enumerating
        ''' </summary></summary>    
        Private Property Que As Deque(Of S)

        Public Sub New(ByVal que As Deque(Of S), ByVal version As Long)
            Me.version = version
            Me.Que = que
            'initialize with que.Count to ensure that InvalidOperationException is thrown when Current is called befor the first call of MoveNext
            curIndex = que.Count
        End Sub

        Public ReadOnly Property p_Current As S
            Get

                If curIndex < 0 OrElse curIndex >= Que.Count OrElse version <> Que.version Then
                    Throw New InvalidOperationException()
                Else
                    Return Que(curIndex)
                End If
            End Get
        End Property

        Private ReadOnly Property Current As Object Implements IEnumerator.Current
            Get
                Return CSharpImpl.__Throw(Of Object)(New NotImplementedException())
            End Get
        End Property

        Public Sub Dispose() Implements IDisposable.Dispose
            Que = Nothing
            curIndex = Nothing
            version = Nothing
        End Sub

        Public Function MoveNext() As Boolean Implements IEnumerator.MoveNext
            If version <> Que.version Then
                Throw New InvalidOperationException()
            End If

            curIndex -= 1

            If curIndex < 0 Then
                Return False
            End If

            Return True
        End Function

        Public Sub Reset() Implements IEnumerator.Reset
            curIndex = Que.Count - 1
        End Sub

        Private Class CSharpImpl
            <Obsolete("Please refactor calling code to use normal throw statements")>
            Shared Function __Throw(Of T)(ByVal e As Exception) As T
                Throw e
            End Function
        End Class
    End Class

    Private Class CSharpImpl
        <Obsolete("Please refactor calling code to use normal throw statements")>
        Shared Function __Throw(Of T)(ByVal e As Exception) As T
            Throw e
        End Function
    End Class
End Class
