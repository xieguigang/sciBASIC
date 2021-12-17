Namespace ComponentModel

    Public Interface IDataIndex

        Sub SetByIndex(index As String, value As Object)
        Function GetByIndex(index As String, [default] As Object) As Object

    End Interface
End Namespace