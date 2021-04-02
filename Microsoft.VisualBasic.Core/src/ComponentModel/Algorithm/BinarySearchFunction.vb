Namespace ComponentModel.Algorithm

    Public Class BinarySearchFunction(Of K, T)

        ReadOnly sequence As (K, T)()
        ReadOnly order As Comparison(Of K)

        Sub New(source As IEnumerable(Of T), key As Func(Of T, K), compares As Comparison(Of K))
            sequence = source.Select(Function(d) (key(d), d)).OrderBy(compares).ToArray
        End Sub

        ''' <summary>
        ''' should be reorder in asc
        ''' </summary>
        ''' <param name="target"></param>
        ''' <param name="equals"></param>
        ''' <returns>Returns ``-1`` means no search result</returns>
        Public Function BinarySearch(target As K, equals As GenericLambda(Of K).IEquals) As Integer

        End Function
    End Class
End Namespace