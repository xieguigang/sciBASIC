#Region "Microsoft.VisualBasic::a41f9b09877416a87831bfb735a2c60f, Data_science\Mathematica\Math\Math\Algebra\LP\LpSparseMatrix.vb"

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

    '   Total Lines: 201
    '    Code Lines: 123 (61.19%)
    ' Comment Lines: 40 (19.90%)
    '    - Xml Docs: 90.00%
    ' 
    '   Blank Lines: 38 (18.91%)
    '     File Size: 7.61 KB


    '     Class LpSparseMatrix
    ' 
    '         Properties: NonZeros
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Empty, FromJagged, FromTriplets, ToJagged, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading.Tasks
Imports std = System.Math
' the root namespace of this project has its own Parallel type, so that the
' TPL Parallel type should be referenced via an alias to avoid the conflict.
Imports ParallelTask = System.Threading.Tasks.Parallel

Namespace LinearAlgebra.LinearProgramming

    ''' <summary>
    ''' A sparse matrix in the compressed sparse row(CSR) format.
    ''' </summary>
    ''' <remarks>
    ''' This matrix object is specially designed for the large scale linear
    ''' programming problem, example as the FBA problem of the genome scale
    ''' metabolic network: the stoichiometric matrix of a GEM model is a
    ''' highly sparse matrix, so that a dense matrix storage in such a large
    ''' scale problem will run out of the memory.
    '''
    ''' The column index in each row is always kept in ascending order, so
    ''' that the element lookup in a row can be done via binary search.
    ''' </remarks>
    Public Class LpSparseMatrix

        ''' <summary>number of the constraint rows</summary>
        Public ReadOnly Rows As Integer
        ''' <summary>number of the variables</summary>
        Public ReadOnly Columns As Integer
        ''' <summary>row pointer, the length is <see cref="Rows"/> + 1</summary>
        Public ReadOnly RowPtr As Integer()
        ''' <summary>the column index of each non-zero element</summary>
        Public ReadOnly ColIdx As Integer()
        ''' <summary>the value of each non-zero element</summary>
        Public ReadOnly Values As Double()

        ''' <summary>
        ''' the number of the non-zero elements in this matrix
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property NonZeros As Integer
            Get
                Return If(Values Is Nothing, 0, Values.Length)
            End Get
        End Property

        Sub New(rows As Integer, columns As Integer, rowPtr As Integer(), colIdx As Integer(), values As Double())
            Me.Rows = rows
            Me.Columns = columns
            Me.RowPtr = rowPtr
            Me.ColIdx = colIdx
            Me.Values = values
        End Sub

        ''' <summary>
        ''' create a sparse matrix from a dense jagged matrix
        ''' </summary>
        ''' <param name="jagged"></param>
        ''' <returns></returns>
        Public Shared Function FromJagged(jagged As Double()()) As LpSparseMatrix
            If jagged Is Nothing OrElse jagged.Length = 0 Then
                Return Empty()
            End If

            Dim m As Integer = jagged.Length
            Dim n As Integer = If(jagged(0) Is Nothing, 0, jagged(0).Length)
            Dim counts As Integer() = New Integer(m - 1) {}

            ParallelTask.For(0, m,
                Sub(i)
                    Dim row As Double() = jagged(i)

                    If row IsNot Nothing Then
                        Dim cnt As Integer = 0

                        For j As Integer = 0 To row.Length - 1
                            If row(j) <> 0.0 Then
                                cnt += 1
                            End If
                        Next

                        counts(i) = cnt
                    End If
                End Sub)

            Dim rowPtr As Integer() = New Integer(m) {}

            For i As Integer = 0 To m - 1
                rowPtr(i + 1) = rowPtr(i) + counts(i)
            Next

            Dim nnz As Integer = rowPtr(m)
            Dim colIdx As Integer() = New Integer(std.Max(nnz, 1) - 1) {}
            Dim values As Double() = New Double(std.Max(nnz, 1) - 1) {}

            ParallelTask.For(0, m,
                Sub(i)
                    Dim row As Double() = jagged(i)

                    If row IsNot Nothing Then
                        Dim p As Integer = rowPtr(i)

                        For j As Integer = 0 To row.Length - 1
                            If row(j) <> 0.0 Then
                                colIdx(p) = j
                                values(p) = row(j)
                                p += 1
                            End If
                        Next
                    End If
                End Sub)

            Return New LpSparseMatrix(m, n, rowPtr, colIdx, values)
        End Function

        ''' <summary>
        ''' create a sparse matrix from the triplet data
        ''' </summary>
        Public Shared Function FromTriplets(rows As Integer, columns As Integer,
                                            rowIdx As Integer(),
                                            colIdx As Integer(),
                                            vals As Double()) As LpSparseMatrix

            If rows <= 0 Then
                Return New LpSparseMatrix(rows, columns, New Integer(rows) {}, New Integer() {}, New Double() {})
            End If

            Dim nnz As Integer = If(vals Is Nothing, 0, vals.Length)

            If nnz = 0 Then
                Return New LpSparseMatrix(rows, columns, New Integer(rows) {}, New Integer() {}, New Double() {})
            End If

            Dim counts As Integer() = New Integer(rows - 1) {}
            Dim rowPtr As Integer() = New Integer(rows) {}

            For k As Integer = 0 To nnz - 1
                counts(rowIdx(k)) += 1
            Next
            For i As Integer = 0 To rows - 1
                rowPtr(i + 1) = rowPtr(i) + counts(i)
            Next

            Dim cursor As Integer() = New Integer(rows - 1) {}
            Dim idx As Integer() = New Integer(nnz - 1) {}
            Dim x As Double() = New Double(nnz - 1) {}

            Array.Copy(rowPtr, 0, cursor, 0, rows)

            For k As Integer = 0 To nnz - 1
                Dim r As Integer = rowIdx(k)
                Dim p As Integer = cursor(r)

                idx(p) = colIdx(k)
                x(p) = vals(k)
                cursor(r) = p + 1
            Next

            ' keeps the column index in each row in ascending order
            ParallelTask.For(0, rows,
                Sub(i)
                    Dim start As Integer = rowPtr(i)
                    Dim len As Integer = rowPtr(i + 1) - start

                    If len > 1 Then
                        Array.Sort(Of Integer, Double)(idx, x, start, len)
                    End If
                End Sub)

            Return New LpSparseMatrix(rows, columns, rowPtr, idx, x)
        End Function

        ''' <summary>
        ''' the empty matrix
        ''' </summary>
        Public Shared Function Empty() As LpSparseMatrix
            Return New LpSparseMatrix(0, 0, New Integer() {0}, New Integer() {}, New Double() {})
        End Function

        ''' <summary>
        ''' expands the sparse matrix as a dense jagged matrix, note that this
        ''' operation may cost a lot of memory in a large scale problem
        ''' </summary>
        Public Function ToJagged() As Double()()
            Dim jagged As Double()() = New Double(Rows - 1)() {}

            For i As Integer = 0 To Rows - 1
                jagged(i) = New Double(Columns - 1) {}

                For p As Integer = RowPtr(i) To RowPtr(i + 1) - 1
                    jagged(i)(ColIdx(p)) = Values(p)
                Next
            Next

            Return jagged
        End Function

        Public Overrides Function ToString() As String
            Return $"[{Rows}x{Columns}] sparse matrix, {NonZeros} non-zeros"
        End Function
    End Class

End Namespace

