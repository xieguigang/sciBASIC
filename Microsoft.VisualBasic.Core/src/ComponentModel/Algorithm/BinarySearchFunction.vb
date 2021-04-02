Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.Algorithm

    Public Class BinarySearchFunction(Of K, T)

        ReadOnly sequence As (index As Integer, K, T)()
        ReadOnly order As Comparison(Of K)

        Sub New(source As IEnumerable(Of T), key As Func(Of T, K), compares As Comparison(Of K))
            order = compares
            sequence = source _
                .Select(Function(d, i) (i, key(d), d)) _
                .DoCall(Function(data)
                            Return New QuickSortFunction(Of K, (index As Integer, K, T))(compares).QuickSort(
                                list:=data,
                                key:=Function(i) i.Item2
                            )
                        End Function)
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