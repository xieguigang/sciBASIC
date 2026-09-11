#Region "Microsoft.VisualBasic::2301e89d9cdeb10e19829b4099a7156c, Data_science\Mathematica\Math\Math\Algebra\LP\SparseTableauRow.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 277
    '    Code Lines: 169 (61.01%)
    ' Comment Lines: 57 (20.58%)
    '    - Xml Docs: 87.72%
    ' 
    '   Blank Lines: 51 (18.41%)
    '     File Size: 9.15 KB


    '     Class SparseTableauRow
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Item, RemoveFrom, ToString
    ' 
    '         Sub: Add, Axpy, Compress, EnsureCapacity, EnsureSpare
    '              Scale
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

            ' a backward merge does not require any additional memory buffer,
            ' it writes the merge result from the tail of the row buffer, so
            ' that the source data will never be overwritten before it is
            ' read by this operation.
            Dim need As Integer = n1 + n2
            Dim inPlace As Boolean = Idx.Length >= need

            If Not inPlace Then
                ' the row buffer is too small, merges into the spare buffer
                ' and then swaps them, so that no array copy is required.
                Call EnsureSpare(need)
            End If

            Dim outIdx As Integer() = If(inPlace, Idx, idxB)
            Dim outVal As Double() = If(inPlace, Val, valB)
            Dim i As Integer = n1 - 1
            Dim j As Integer = n2 - 1
            Dim k As Integer = need - 1
            Dim last As Integer = need - 1

            While j >= 0
                If i >= 0 AndAlso Idx(i) > other.Idx(j) Then
                    outIdx(k) = Idx(i)
                    outVal(k) = Val(i)
                    i -= 1
                    k -= 1
                ElseIf i >= 0 AndAlso Idx(i) = other.Idx(j) Then
                    Dim v As Double = Val(i) - factor * other.Val(j)

                    If std.Abs(v) > dropTol Then
                        outIdx(k) = Idx(i)
                        outVal(k) = v
                        k -= 1
                    End If

                    i -= 1
                    j -= 1
                Else
                    Dim v As Double = -factor * other.Val(j)

                    If std.Abs(v) > dropTol Then
                        outIdx(k) = other.Idx(j)
                        outVal(k) = v
                        k -= 1
                    End If

                    j -= 1
                End If
            End While

            While i >= 0
                outIdx(k) = Idx(i)
                outVal(k) = Val(i)
                i -= 1
                k -= 1
            End While

            Dim merged As Integer = last - k

            If merged > 0 AndAlso k >= 0 Then
                Array.Copy(outIdx, k + 1, outIdx, 0, merged)
                Array.Copy(outVal, k + 1, outVal, 0, merged)
            End If

            If Not inPlace Then
                Dim swapIdx As Integer() = Idx
                Dim swapVal As Double() = Val

                Idx = idxB
                Val = valB
                idxB = swapIdx
                valB = swapVal
            End If

            Count = merged
        End Sub

        ''' <summary>
        ''' removes all of the columns which index is greater than or equals 
        ''' to the given <paramref name="minCol"/> from this row.
        ''' </summary>
        ''' <param name="minCol"></param>
        ''' <returns>the number of the removed elements</returns>
        Public Function RemoveFrom(minCol As Integer) As Integer
            Dim lo As Integer = 0
            Dim hi As Integer = Count - 1
            Dim hit As Integer = Count

            While lo <= hi
                Dim mid As Integer = (lo + hi) \ 2

                If Idx(mid) >= minCol Then
                    hit = mid
                    hi = mid - 1
                Else
                    lo = mid + 1
                End If
            End While

            Dim removed As Integer = Count - hit

            Count = hit

            Return removed
        End Function

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

