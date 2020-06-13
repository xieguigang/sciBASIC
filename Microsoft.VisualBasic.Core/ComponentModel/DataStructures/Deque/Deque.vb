Namespace ComponentModel.Collection.Deque

    ''' <summary>
    ''' Double ended queue implementation
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Deque(Of T) : Implements IDeque(Of T)

        ''' <summary>
        ''' actuall data of this Deque(Of T)
        ''' </summary>
        Dim data As Data(Of T) = New Data(Of T)()

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
        ''' peek the firts element of the Deque(Of T)
        ''' </summary>
        Public ReadOnly Property First As T Implements IDeque(Of T).First
            Get
                Return data(0)
            End Get
        End Property

        ''' <summary>
        ''' peek the last element of the Deque(Of T)
        ''' </summary>
        Public ReadOnly Property Last As T Implements IDeque(Of T).Last
            Get
                Return data(Count - 1)
            End Get
        End Property

        ''' <summary>
        ''' Number of elements in Deque(Of T)
        ''' </summary>
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
        ''' Adds an object to the end of the Deque(Of T).
        ''' </summary>
        ''' <param name="item"></param>
        ''' 
        Public Sub Add(ByVal item As T) Implements ICollection(Of T).Add
            data.AddEnd(item)
            version += 1
        End Sub

        ''' <summary>
        ''' Adds an element to the beggining of the Deque(Of T)
        ''' </summary>
        ''' <param name="item"></param>
        Public Sub AddHead(ByVal item As T) Implements IDeque(Of T).AddHead
            data.AddBegining(item)
            version += 1
        End Sub

        ''' <summary>
        ''' returns the firts element of the Deque(Of T) and removes it from Deque(Of T)
        ''' </summary>
        ''' <returns></returns>
        Public Function RemoveHead() As T Implements IDeque(Of T).RemoveHead
            Dim item As T = data.RemoveHead()
            version += 1
            Return item
        End Function

        ''' <summary>
        ''' returns the last element of the Deque(Of T) and removes it from Deque(Of T)
        ''' </summary>
        ''' <returns></returns>
        Public Function RemoveTail() As T Implements IDeque(Of T).RemoveTail
            Dim item As T = data.RemoveTail()
            version += 1
            Return item
        End Function

        ''' <summary>
        ''' Removes all elements from the Deque(Of T).
        ''' </summary> 
        Public Sub Clear() Implements ICollection(Of T).Clear
            data.Clear()
            version += 1
        End Sub

        ''' <summary>
        ''' Determines whether an element is in the Deque(Of T).
        ''' </summary>
        ''' <param name="item"></param>
        ''' <returns>true if item is found in the Deque(Of T); otherwise, false.</returns>
        Public Function Contains(ByVal item As T) As Boolean Implements ICollection(Of T).Contains
            Return data.Contains(item)
        End Function

        ''' <summary>
        ''' Copies the entire Deque(Of T) to a compatible one-dimensional array, 
        ''' starting at the specified index of the target array.
        ''' </summary>
        ''' <param name="array"></param>
        ''' <param name="arrayIndex"></param>
        Public Sub CopyTo(ByVal array As T(), ByVal arrayIndex As Integer) Implements ICollection(Of T).CopyTo
            data.CopyTo(array, arrayIndex, False)
        End Sub

        ''' <summary>
        ''' Copies the entire Deque(Of T) to a compatible one-dimensional array, 
        ''' starting at the specified index of the target array, in reversed 
        ''' order
        ''' </summary>
        ''' <param name="array"></param>
        ''' <param name="arrayIndex"></param>
        Public Sub CopyToReversed(ByVal array As T(), ByVal arrayIndex As Integer)
            data.CopyTo(array, arrayIndex, True)
        End Sub

        Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            Return New Enumerator(Of T)(Me, version)
        End Function

        Private Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function

        ''' <summary>
        ''' Searches for the specified object and returns the zero-based index of the first occurrence within the entire Deque(Of T).
        ''' </summary>
        ''' <returns>e zero-based index of the first occurrence of item within the entire Deque(Of T), if found; otherwise, -1.</returns> 
        Public Function IndexOf(ByVal item As T) As Integer Implements IList(Of T).IndexOf
            Return data.IndexOf(item)
        End Function

        ''' <summary>
        ''' Inserts an element into the Deque(Of T) at the specified index.
        ''' </summary>
        ''' <param name="index"></param>
        ''' <param name="item"></param>
        Public Sub Insert(ByVal index As Integer, ByVal item As T) Implements IList(Of T).Insert
            data.Insert(index, item)
            version += 1
        End Sub

        ''' <summary>
        ''' Removes the first occurrence of a specific object from the Deque(Of T).
        ''' </summary>
        ''' <param name="item"></param>
        ''' <returns>true if item is successfully removed; otherwise, false. This method also returns false if item was not found in the List(Of T).</returns>
        Public Function Remove(ByVal item As T) As Boolean Implements ICollection(Of T).Remove
            'TODO: do not change version when removal fails?
            version += 1
            Return data.Remove(item)
        End Function

        ''' <summary>
        ''' Removes the element at the specified index of the Deque(Of T).
        ''' </summary>
        ''' <param name="index"></param> 
        Public Sub RemoveAt(ByVal index As Integer) Implements IList(Of T).RemoveAt
            data.RemoveAt(index)
            version += 1
        End Sub

        ''' <summary>
        ''' returns reverse view on this instance of Deque(Of T)
        ''' </summary>
        ''' <returns></returns> 
        Public Function Reverse() As IDeque(Of T) Implements IDeque(Of T).Reverse
            Return New ReverseQueue(Of T)(Me)
        End Function
    End Class
End Namespace