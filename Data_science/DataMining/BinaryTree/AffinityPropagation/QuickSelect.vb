Namespace AffinityPropagation

    Public Module QuickSelect

        Private Function qpartition(arr As Edge(), low As Integer, high As Integer) As Integer
            Dim temp As Edge
            Dim pivot = arr(high)
            Dim i = low - 1

            For j As Integer = low To high - 1
                If arr(j).Similarity <= pivot.Similarity Then
                    i += 1
                    temp = arr(i)
                    arr(i) = arr(j)
                    arr(j) = temp
                End If
            Next

            temp = arr(i + 1)
            arr(i + 1) = arr(high)
            arr(high) = temp

            Return i + 1
        End Function

        ''' <summary>
        ''' Implementation of QuickSelect
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="left"></param>
        ''' <param name="right"></param>
        ''' <param name="k"></param>
        ''' <returns></returns>
        Public Function k2thSmallest(ByRef a As Edge(), left As Integer, right As Integer, k As Integer) As Double()
            Dim temp = 0
            Dim s As Double() = {0, 0}

            While left <= right

                ' Partition a[left..right] around a pivot
                ' and find the position of the pivot
                Dim pivotIndex = qpartition(a, left, right)

                ' If pivot itself is the k-th smallest element
                If pivotIndex = k - 1 Then
                    s(1) = a(pivotIndex).Similarity
                    s(0) = a(temp).Similarity

                    Return s

                    ' If there are more than k-1 elements on
                    ' left of pivot, then k-th smallest must be
                    ' on left side.
                ElseIf pivotIndex > k - 1 Then

                    ' Else k-th smallest is on right side.
                    right = pivotIndex - 1
                Else
                    left = pivotIndex + 1
                End If

                temp = pivotIndex
            End While
            Return Nothing
        End Function


    End Module
End Namespace
