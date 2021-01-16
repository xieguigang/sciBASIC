Namespace ComponentModel.Collection

    Public Class JoinElement(Of T)

        Public Property previous As T
        Public Property index As Integer
        Public Property element As T
        Public Property [next] As T

        Public Shared Function GetByIndex(array As T(), index As Integer) As JoinElement(Of T)
            Return New JoinElement(Of T) With {
                .index = index,
                .element = array(index),
                .[next] = If(index = array.Length - 1, Nothing, array(index + 1)),
                .previous = If(index = 0, Nothing, array(index - 1))
            }
        End Function

        Public Shared Iterator Function FindElement(array As T(), find As Func(Of T, Boolean)) As IEnumerable(Of JoinElement(Of T))
            If Not array.IsNullOrEmpty Then
                For i As Integer = 0 To array.Length - 1
                    If find(array(i)) Then
                        Yield GetByIndex(array, i)
                    End If
                Next
            End If
        End Function
    End Class
End Namespace