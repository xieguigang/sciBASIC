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
    ''' 2. the row merge operation (AXPY) can be done via a backward merge,
    '''    which does not require any additional memory buffer.
    ''' </remarks>
    Public Class SparseTableauRow

        ''' <summary>the column index of each non-zero element, in ascending order</summary>
        Public Idx As Integer()
        ''' <summary>the value of each non-zero element</summary>
        Public Val As Double()
        ''' <summary>the number of the non-zero elements in this row</summary>
        Public Count As Integer

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
        ''' a backward merge is applied on this operation, so that there is no
        ''' additional memory buffer required by the merge result.
        ''' </remarks>
        Public Sub Axpy(other As SparseTableauRow, factor As Double, dropTol As Double)
            Dim n1 As Integer = Count
            Dim n2 As Integer = other.Count

            If n2 = 0 Then
                Return
            End If

            EnsureCapacity(n1 + n2)

            Dim i As Integer = n1 - 1
            Dim j As Integer = n2 - 1
            Dim k As Integer = n1 + n2 - 1
            Dim last As Integer = n1 + n2 - 1

            While j >= 0
                If i >= 0 AndAlso Idx(i) > other.Idx(j) Then
                    Idx(k) = Idx(i)
                    Val(k) = Val(i)
                    i -= 1
                    k -= 1
                ElseIf i >= 0 AndAlso Idx(i) = other.Idx(j) Then
                    Dim v As Double = Val(i) - factor * other.Val(j)

                    If std.Abs(v) > dropTol Then
                        Idx(k) = Idx(i)
                        Val(k) = v
                        k -= 1
                    End If

                    i -= 1
                    j -= 1
                Else
                    Dim v As Double = -factor * other.Val(j)

                    If std.Abs(v) > dropTol Then
                        Idx(k) = other.Idx(j)
                        Val(k) = v
                        k -= 1
                    End If

                    j -= 1
                End If
            End While

            While i >= 0
                Idx(k) = Idx(i)
                Val(k) = Val(i)
                i -= 1
                k -= 1
            End While

            Dim newCount As Integer = last - k

            If newCount > 0 AndAlso k >= 0 Then
                Array.Copy(Idx, k + 1, Idx, 0, newCount)
                Array.Copy(Val, k + 1, Val, 0, newCount)
            End If

            Count = newCount
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
