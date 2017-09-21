Namespace ComponentModel.Collection

    Public Interface IPriorityQueue(Of T)
        Inherits ICollection
        Inherits ICloneable
        Inherits IList

        Function Push(O As T) As Integer
        Function Pop() As T
        Function Peek() As T

    End Interface
End Namespace