Namespace Language

    ''' <summary>
    ''' Vectorization programming language feature for VisualBasic
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class VectorShadows(Of T) : Implements IEnumerable(Of T)

        Sub New(source As IEnumerable(Of T))

        End Sub

        Public Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            Throw New NotImplementedException()
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace