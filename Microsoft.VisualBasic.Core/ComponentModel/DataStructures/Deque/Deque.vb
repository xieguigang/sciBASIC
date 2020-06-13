Public Interface IDeque(Of T)
    Inherits IList(Of T)

    ReadOnly Property First As T
    ReadOnly Property Last As T
    Sub AddHead(ByVal item As T)
    Function RemoveHead() As T
    Function RemoveTail() As T
    Function Reverse() As IDeque(Of T)
End Interface

''' <summary>
''' Double ended queue implementation
''' </summary>
''' <typeparam name="T"></typeparam>
Public Partial Class Deque(Of T)
    Implements IDeque(Of T)

    ''' <summary>
    ''' actuall data of this Deque<T>
    ''' </summary></summary>    
    Private data As n_Data(Of T) = New n_Data(Of T)()

    ''' <summary>
    ''' to detect changes during enumeration
    ''' </summary>
    ''' <returns></returns>
    Public Property version As Long = 0

    Default Public Property Item(ByVal i As Integer) As T Implements IList(Of T).Item
        Get
            Return data(i)
        End Get
        Set(ByVal value As T)
            data(i) = value
            version += 1
        End Set
    End Property
    ''' <summary>
    ''' peek the firts element of the Deque<T>
    ''' </summary></summary>   
    Public ReadOnly Property First As T Implements IDeque(Of T).First
        Get
            Return data(0)
        End Get
    End Property
    ''' <summary>
    ''' peek the last element of the Deque<T>
    ''' </summary></summary>   
    Public ReadOnly Property Last As T Implements IDeque(Of T).Last
        Get
            Return data(Count - 1)
        End Get
    End Property
    ''' <summary>
    ''' Number of elements in Deque<T>
    ''' </summary></summary>  
    Public ReadOnly Property Count As Integer Implements ICollection(Of T).Count
        Get
            Return data.Count
        End Get
    End Property

    Public ReadOnly Property IsReadOnly As Boolean Implements ICollection(Of T).IsReadOnly
        Get
            Return False
        End Get
    End Property
    ''' <summary>
    ''' Adds an object to the end of the Deque<T>.
    ''' </summary>
    ''' <paramname="item"></param></summary> 
    ''' 
    Public Sub Add(ByVal item As T) Implements ICollection(Of T).Add
        data.AddEnd(item)
        version += 1
    End Sub
    ''' <summary>
    ''' Adds an element to the beggining of the Deque<T>
    ''' </summary>
    ''' <paramname="item"></param></summary>   
    Public Sub AddHead(ByVal item As T) Implements IDeque(Of T).AddHead
        data.AddBegining(item)
        version += 1
    End Sub
    ''' <summary>
    ''' returns the firts element of the Deque<T> and removes it from Deque<T>
    ''' </summary>
    ''' <returns></returns></T></summary>   
    Public Function RemoveHead() As T Implements IDeque(Of T).RemoveHead
        Dim item As T = data.RemoveHead()
        version += 1
        Return item
    End Function
    ''' <summary>
    ''' returns the last element of the Deque<T> and removes it from Deque<T>
    ''' </summary>
    ''' <returns></returns></T></summary>   
    Public Function RemoveTail() As T Implements IDeque(Of T).RemoveTail
        Dim item As T = data.RemoveTail()
        version += 1
        Return item
    End Function

    ''' <summary>
    ''' Removes all elements from the Deque<T>.
    ''' </summary></summary>   
    Public Sub Clear() Implements ICollection(Of T).Clear
        data.Clear()
        version += 1
    End Sub
    ''' <summary>
    ''' Determines whether an element is in the Deque<T>.
    ''' </summary>
    ''' <paramname="item"></param>
    ''' <returns>true if item is found in the Deque<T>; otherwise, false.</returns></returns></summary>  
    Public Function Contains(ByVal item As T) As Boolean Implements ICollection(Of T).Contains
        Return data.Contains(item)
    End Function
    ''' <summary>
    ''' Copies the entire Deque<T> to a compatible one-dimensional array, starting at the specified index of the target array.
    ''' </summary>
    ''' <paramname="array"></param>
    ''' <paramname="arrayIndex"></param></summary>   
    Public Sub CopyTo(ByVal array As T(), ByVal arrayIndex As Integer) Implements ICollection(Of T).CopyTo
        data.CopyTo(array, arrayIndex, False)
    End Sub
    ''' <summary>
    ''' Copies the entire Deque<T> to a compatible one-dimensional array, starting at the specified index of the target array, in reversed order
    ''' </summary>
    ''' <paramname="array"></param>
    ''' <paramname="arrayIndex"></param></summary> 
    Public Sub CopyToReversed(ByVal array As T(), ByVal arrayIndex As Integer)
        data.CopyTo(array, arrayIndex, True)
    End Sub

    Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
        Return New Enumerator(Of T)(Me, version)
    End Function

    ''' <summary>
    ''' Searches for the specified object and returns the zero-based index of the first occurrence within the entire Deque<T>.
    ''' </summary>
    ''' <paramname="T"></param>
    ''' <returns>e zero-based index of the first occurrence of item within the entire Deque<T>, if found; otherwise, -1.</returns></returns></summary>  
    Public Function IndexOf(ByVal item As T) As Integer Implements IList(Of T).IndexOf
        Return data.IndexOf(item)
    End Function
    ''' <summary>
    ''' Inserts an element into the Deque<T> at the specified index.
    ''' </summary>
    ''' <paramname="index"></param>
    ''' <paramname="item"></param></summary>   
    Public Sub Insert(ByVal index As Integer, ByVal item As T) Implements IList(Of T).Insert
        data.Insert(index, item)
        version += 1
    End Sub
    ''' <summary>
    ''' /// Removes the first occurrence of a specific object from the Deque<T>.
    ''' </summary>
    ''' <paramname="item"></param>
    ''' <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the List<T>.</returns></returns></summary> 
    Public Function Remove(ByVal item As T) As Boolean Implements ICollection(Of T).Remove
        'TODO: do not change version when removal fails?
        version += 1
        Return data.Remove(item)
    End Function
    ''' <summary>
    ''' Removes the element at the specified index of the Deque<T>.
    ''' </summary>
    ''' <paramname="index"></param></summary>   
    Public Sub RemoveAt(ByVal index As Integer) Implements IList(Of T).RemoveAt
        data.RemoveAt(index)
        version += 1
    End Sub

    Private Function GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
        Throw New NotImplementedException()
    End Function
    ''' <summary>
    ''' returns reverse view on this instance of Deque<T>
    ''' </summary>
    ''' <returns></returns></summary>  
    Public Function Reverse() As IDeque(Of T) Implements IDeque(Of T).Reverse
        Return New ReverseView(Of T)(Me)
    End Function
End Class
