Namespace ComponentModel.Algorithm

    Public Class QuickSortFunction

        Public Function QuickSort(src As Integer()) As Integer()
            Call QuickSort(src, Scan0, src.Length - 1)
            Return src
        End Function

        Private Sub QuickSort(src As Integer(), begin As Integer, [end] As Integer)
            If begin < [end] Then
                Dim key = src(begin)
                Dim i = begin
                Dim j = [end]

                While (i < j)
                    While (i < j AndAlso src(j) > key)
                        j -= 1
                    End While
                    If (i < j) Then
                        src(i) = src(j)
                        i += 1
                    End If
                    While (i < j AndAlso src(i) < key)
                        i += 1
                    End While
                    If i < j Then
                        src(j) = src(i)
                        j -= 1
                    End If
                End While

                src(i) = key

                Call QuickSort(src, begin, i - 1)
                Call QuickSort(src, i + 1, [end])
            End If
        End Sub
    End Class
End Namespace