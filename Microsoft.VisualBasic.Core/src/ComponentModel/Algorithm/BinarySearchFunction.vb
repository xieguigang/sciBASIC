Imports Microsoft.VisualBasic.Linq

Namespace ComponentModel.Algorithm

    Public Class BinarySearchFunction(Of K, T)

        ReadOnly sequence As (index As Integer, key As K, T)()
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
        ''' <returns>Returns ``-1`` means no search result</returns>
        Public Function BinarySearch(target As K) As Integer
            Dim x As (index As Integer, K, T)
            Dim min% = 0
            Dim max% = sequence.Length - 1
            Dim index%
            Dim key As K

            If max = -1 Then
                ' no elements
                Return -1
            ElseIf max = 0 Then
                ' one element
                If 0 = order(sequence(0).Item2, target) Then
                    Return 0
                Else
                    ' 序列只有一个元素，但是不相等，则返回-1，否则后面的while会无限死循环
                    Return -1
                End If
            End If

            Do While max <> (min + 1)
                index = (max - min) / 2 + min
                x = sequence(index)
                key = x.Item2

                If 0 = order(target, key) Then
                    Return x.index
                ElseIf order(target, key) > 0 Then
                    min = index
                Else
                    max = index
                End If
            Loop

            If 0 = order(sequence(min).key, target) Then
                Return sequence(min).index
            ElseIf 0 = order(sequence(max).key, target) Then
                Return sequence(max).index
            Else
                Return -1
            End If
        End Function
    End Class
End Namespace