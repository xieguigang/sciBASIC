Imports System.Runtime.CompilerServices

Namespace ComponentModel.TagData

    Public Module IndexingExtensions

        <Extension>
        Public Function BinarySearch(Of K As IComparable(Of K), T)(source As IEnumerable(Of T), key As K, getKey As Func(Of T, K), Optional [default] As T = Nothing) As T
            Dim inputArray = source.OrderBy(getKey).ToArray
            Dim min = 0
            Dim max = inputArray.Length - 1
            Dim mid%

            Do While min <= max
                [mid] = (min + max) / 2

                If key.CompareTo(getKey(inputArray(mid))) = 0 Then
                    Return inputArray(mid)
                ElseIf key.CompareTo(getKey(inputArray(mid))) < 0 Then
                    max = mid - 1
                Else
                    min = mid + 1
                End If
            Loop

            Return [default]
        End Function
    End Module
End Namespace