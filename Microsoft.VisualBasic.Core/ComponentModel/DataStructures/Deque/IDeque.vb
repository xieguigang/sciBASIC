Namespace ComponentModel.Collection.Deque

    Public Interface IDeque(Of T)
        Inherits IList(Of T)

        ReadOnly Property First As T
        ReadOnly Property Last As T
        Sub AddHead(ByVal item As T)
        Function RemoveHead() As T
        Function RemoveTail() As T
        Function Reverse() As IDeque(Of T)
    End Interface
End Namespace