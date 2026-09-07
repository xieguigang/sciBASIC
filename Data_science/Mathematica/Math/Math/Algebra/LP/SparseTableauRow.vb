Imports std = System.Math

Namespace LinearAlgebra.LinearProgramming

    ''' <summary>
    ''' A single row in the simplex tableau, stored in the sparse format.
    ''' </summary>
    ''' <remarks>
    ''' The column index in <see cref="Idx"/> is always kept in ascending
    ''' order, so that:
    '''
    ''' 1. the element lookup can be done via the binary search method
    ''' 2. the row merge operation (AXPY) can be done via a forward merge
    '''    into a spare buffer, and then the buffers are swapped: there is
    '''    no array copy and no extra allocation is required by this 
    '''    operation (the ping-pong buffer).
    ''' </remarks>
    Public Class SparseTableauRow

        ''' <summary>the column index of each non-zero element, in ascending order</summary>
        Public Idx As Integer()
        ''' <summary>the value of each non-zero element</summary>
        Public Val As Double()
        ''' <summary>the number of the non-zero elements in this row</summary>
        Public Count As Integer

        ''' <summary>the spare buffer of <see cref="Idx"/></summary>
        Dim idxB As Integer()
        ''' <summary>the spare buffer of <see cref="Val"/></summary>
        Dim valB As Double()

        Sub New(capacity As Integer)
            capacity = std.Max(capacity, 4)
            Idx = New Integer(capacity - 1) {}
            Val = New Double(capacity - 1) {}
            Count = 0
        End Sub

        Private Sub EnsureCapacity(capacity As Integer)
            If Idx.Length >= capacity Then
                Return
            End If

            Dim size As Integer = Idx.Length

            While size < capacity
                size *= 2
            End While

            ReDim Preserve Idx(size - 1)
            ReDim Preserve Val(size - 1)
        End Sub

        Private Sub EnsureSpare(capacity As Integer)
            If idxB IsNot Nothing AndAlso idxB.Length >= capacity Then
                Return
            End If

            Dim size As Integer = If(idxB Is Nothing, 4, idxB.Length)

            While size < capacity
                size *= 2
            End While

            idxB = New Integer(size - 1) {}
            valB = New Double(size - 1) {}
        End Sub

        ''' <summary>
        ''' append a new non-zero element at the end of this row, the given
        ''' <paramref name="col"/> should be greater than the last column
        ''' index in this row.
        ''' </summary>
        Public Sub Add(col As Integer, value As Double)
            If value = 0.0 Then
                Return
            End If

            EnsureCapacity(Count + 1)

            Idx(Count) = col
            Val(Count) = value
            Count += 1
        End Sub

        ''' <summary>
        ''' get the element value at the given column index
        ''' </summary>
        ''' <returns>zero when the given column is a structural zero</returns>
        Public Function Item(col As Integer) As Double
            Dim lo As Integer = 0
            Dim hi As Integer = Count - 1

            While lo <= hi
                Dim mid As Integer = (lo + hi) \ 2
                Dim c As Integer = Idx(mid)

                If c = col Then
                    Return Val(mid)
                ElseIf c < col Then
                    lo = mid + 1
                Else
                    hi = mid - 1
                End If
            End While

            Return 0.0
        End Function

        ''' <summary>
        ''' this row = this row * <paramref name="factor"/>
        ''' </summary>
        ''' <param name="dropTol">the tiny value will be removed from this row</param>
        Public Sub Scale(factor As Double, dropTol As Double)
            Dim w As Integer = 0

            For r As Integer = 0 To Count - 1
                Dim v As Double = Val(r) * factor

                If std.Abs(v) > dropTol Then
                    Idx(w) = Idx(r)
                    Val(w) = v
                    w += 1
                End If
            Next

            Count = w
        End Sub

        ''' <summary>
        ''' this row = this row - <paramref name="factor"/> * <paramref name="other"/>
        ''' </summary>
        ''' <param name="other">the pivot row, which is already scaled</param>
        ''' <param name="factor"></param>
        ''' <param name="dropTol"></param>
        ''' <remarks>
        ''' a forward merge into the spare buffer is applied by this operation,
        ''' then the spare buffer is swapped with the row buffer, so that there
        ''' is no array copy cost on this operation.
        ''' </remarks>
        Public Sub Axpy(other As SparseTableauRow, factor As Double, dropTol As Double)
            Dim n1 As Integer = Count
            Dim n2 As Integer = other.Count

            If n2 = 0 Then
                Return
            End If

            ' counts the size of the merged row at first: reserving the buffer
            ' with (n1 + n2) will make the row buffer grow on every single
            ' merge operation, which costs a lot of time on the array copy.
            Dim merged As Integer = 0
            Dim c1 As Integer = 0
            Dim c2 As Integer = 0

            While c1 < n1 OrElse c2 < n2
                If c2 >= n2 Then
                    merged += 1
                    c1 += 1
                ElseIf c1 >= n1 Then
                    If std.Abs(factor * other.Val(c2)) > dropTol Then
                        merged += 1
                    End If

                    c2 += 1
                ElseIf Idx(c1) > other.Idx(c2) Then
                    merged += 1
                    c1 += 1
                ElseIf Idx(c1) = other.Idx(c2) Then
                    If std.Abs(Val(c1) - factor * other.Val(c2)) > dropTol Then
                        merged += 1
                    End If

                    c1 += 1
                    c2 += 1
                Else
                    If std.Abs(factor * other.Val(c2)) > dropTol Then
                        merged += 1
                    End If

                    c2 += 1
                End If
            End While

            Call EnsureSpare(merged)

            Dim i As Integer = 0
            Dim j As Integer = 0
            Dim w As Integer = 0

            While i < n1 OrElse j < n2
                If j >= n2 Then
                    idxB(w) = Idx(i)
                    valB(w) = Val(i)
                    i += 1
                    w += 1
                ElseIf i >= n1 Then
                    Dim v As Double = -factor * other.Val(j)

                    If std.Abs(v) > dropTol Then
                        idxB(w) = other.Idx(j)
                        valB(w) = v
                        w += 1
                    End If

                    j += 1
                ElseIf Idx(i) > other.Idx(j) Then
                    idxB(w) = Idx(i)
                    valB(w) = Val(i)
                    i += 1
                    w += 1
                ElseIf Idx(i) = other.Idx(j) Then
                    Dim v As Double = Val(i) - factor * other.Val(j)

                    If std.Abs(v) > dropTol Then
                        idxB(w) = Idx(i)
                        valB(w) = v
                        w += 1
                    End If

                    i += 1
                    j += 1
                Else
                    Dim v As Double = -factor * other.Val(j)

                    If std.Abs(v) > dropTol Then
                        idxB(w) = other.Idx(j)
                        valB(w) = v
                        w += 1
                    End If

                    j += 1
                End If
            End While

            Dim swapIdx As Integer() = Idx
            Dim swapVal As Double() = Val

            Idx = idxB
            Val = valB
            idxB = swapIdx
            valB = swapVal
            Count = w
        End Sub

        ''' <summary>
        ''' remove all of the tiny value in this row
        ''' </summary>
        Public Sub Compress(dropTol As Double)
            Dim w As Integer = 0

            For r As Integer = 0 To Count - 1
                If std.Abs(Val(r)) > dropTol Then
                    Idx(w) = Idx(r)
                    Val(w) = Val(r)
                    w += 1
                End If
            Next

            Count = w
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Count} non-zeros"
        End Function
    End Class

End Namespace
