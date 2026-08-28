#Region "Microsoft.VisualBasic::7eaef0f1f9de4ea9225973c396668d29, Data_science\DataMining\UMAP\Components\SparseMatrix\SparseMatrix.vb"

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

    '   Total Lines: 275
    '    Code Lines: 202 (73.45%)
    ' Comment Lines: 21 (7.64%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 52 (18.91%)
    '     File Size: 10.35 KB


    ' Class SparseMatrix
    ' 
    '     Properties: Dims
    ' 
    '     Constructor: (+3 Overloads) Sub New
    ' 
    '     Function: [Get], Add, Combine, ElementWiseWith, GetAll
    '               GetCols, GetCSR, GetRows, GetValues, (+2 Overloads) Map
    '               MultiplyScalar, newTaskPool, PairwiseMultiply, Subtract, ToArray
    '               Transpose
    ' 
    '     Sub: [Set], CheckDims, ForEach
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading.Tasks
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Public NotInheritable Class SparseMatrix

    ReadOnly _entries As Dictionary(Of RowCol, Double)
    ReadOnly _dims As (rows%, cols%)

    Public ReadOnly Property Dims As (rows As Integer, cols As Integer)
        Get
            Return _dims
        End Get
    End Property

    Public Sub New(rows As IEnumerable(Of Integer), cols As IEnumerable(Of Integer), values As IEnumerable(Of Double), dims As (Integer, Integer))
        Me.New(SparseMatrix.Combine(rows, cols, values), dims)
    End Sub

    Private Sub New(entries As IEnumerable(Of (row As Integer, col As Integer, value As Double)), dims As (rows As Integer, cols As Integer))
        _dims = dims
        _entries = New Dictionary(Of RowCol, Double)()

        For Each entryIndex In entries.[Select](Function(entry, index) (entry, index))
            Dim entry = entryIndex.Item1
            Dim index = entryIndex.Item2

            Call CheckDims(entry.row, entry.col)

            _entries(New RowCol(entry.row, entry.col)) = entry.value
        Next
    End Sub

    Private Sub New(entries As Dictionary(Of RowCol, Double), dims As (Integer, Integer))
        _dims = dims
        _entries = entries
    End Sub

    Private Shared Iterator Function Combine(rows As IEnumerable(Of Integer),
                                             cols As IEnumerable(Of Integer),
                                             values As IEnumerable(Of Double)) As IEnumerable(Of (Integer, Integer, Double))
        Dim rowsArray = rows.ToArray()
        Dim colsArray = cols.ToArray()
        Dim valuesArray = values.ToArray()

        If rowsArray.Length <> valuesArray.Length OrElse colsArray.Length <> valuesArray.Length Then
            Throw New ArgumentException($"The input lists {NameOf(rows)}, {NameOf(cols)} and {NameOf(values)} must all have the same number of elements")
        End If

        For i = 0 To valuesArray.Length - 1
            Yield (rowsArray(i), colsArray(i), valuesArray(i))
        Next
    End Function

    Public Sub [Set](row As Integer, col As Integer, value As Double)
        CheckDims(row, col)
        _entries(New RowCol(row, col)) = value
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function [Get](row As Integer, col As Integer, Optional defaultValue As Double = 0) As Double
        CheckDims(row, col)
        Dim v As Double = Nothing
        Return If(_entries.TryGetValue(New RowCol(row, col), v), v, defaultValue)
    End Function

    Public Function GetAll() As IEnumerable(Of (Integer, Integer, Double))
        Return _entries.Select(Function(kv) (kv.Key.Row, kv.Key.Col, kv.Value))
    End Function

    Public Function GetRows() As IEnumerable(Of Integer)
        Return _entries.Keys.Select(Function(k) k.Row)
    End Function

    Public Function GetCols() As IEnumerable(Of Integer)
        Return _entries.Keys.Select(Function(k) k.Col)
    End Function

    Public Function GetValues() As IEnumerable(Of Double)
        Return _entries.Values
    End Function

    Public Sub ForEach(fn As Action(Of Double, Integer, Integer))
        For Each kv In _entries
            fn(kv.Value, kv.Key.Row, kv.Key.Col)
        Next
    End Sub

    Public Function Map(fn As Func(Of Double, Double), Optional parallelism As ParallelConfig = Nothing) As SparseMatrix
        Return Map(Function(value, row, col) fn(value), parallelism)
    End Function

    ''' <summary>
    ''' apply the given <paramref name="fn"/> on each of the non-zero elements 
    ''' of this sparse matrix
    ''' </summary>
    ''' <param name="fn"></param>
    ''' <param name="parallelism">
    ''' the parallelism configuration, the map function will be evaluated in 
    ''' parallel when the number of the non-zero elements is large enough.
    ''' </param>
    ''' <returns></returns>
    Public Function Map(fn As Func(Of Double, Integer, Integer, Double), Optional parallelism As ParallelConfig = Nothing) As SparseMatrix
        Dim src As KeyValuePair(Of RowCol, Double)() = EntryArray()
        Dim newEntries As New Dictionary(Of RowCol, Double)(src.Length)
        Dim degree As Integer = EffectiveDegree(parallelism, src.Length)

        If degree > 1 Then
            Dim values As Double() = New Double(src.Length - 1) {}

            Call System.Threading.Tasks.Parallel.For(
                fromInclusive:=0,
                toExclusive:=src.Length,
                parallelOptions:=New ParallelOptions With {.MaxDegreeOfParallelism = degree},
                body:=Sub(i)
                          Dim key As RowCol = src(i).Key

                          values(i) = fn(src(i).Value, key.Row, key.Col)
                      End Sub)

            For i As Integer = 0 To src.Length - 1
                newEntries(src(i).Key) = values(i)
            Next
        Else
            For i As Integer = 0 To src.Length - 1
                Dim key As RowCol = src(i).Key

                newEntries(key) = fn(src(i).Value, key.Row, key.Col)
            Next
        End If

        Return New SparseMatrix(newEntries, Dims)
    End Function

    ''' <summary>
    ''' snapshot of the non-zero elements of this matrix into a random 
    ''' accessible array, so that the entries could be processed in parallel.
    ''' </summary>
    ''' <returns></returns>
    Private Function EntryArray() As KeyValuePair(Of RowCol, Double)()
        Dim src As KeyValuePair(Of RowCol, Double)() = New KeyValuePair(Of RowCol, Double)(_entries.Count - 1) {}
        Dim i As Integer = 0

        For Each kv As KeyValuePair(Of RowCol, Double) In _entries
            src(i) = kv
            i += 1
        Next

        Return src
    End Function

    ''' <summary>
    ''' the effective parallelism degree of a workload with the given size
    ''' </summary>
    Private Shared Function EffectiveDegree(parallelism As ParallelConfig, workSize As Integer) As Integer
        Return If(parallelism, ParallelConfig.Default).EffectiveDegree(workSize)
    End Function

    Public Function ToArray() As Double()()
        Dim output = Enumerable.Range(0, Dims.rows).[Select](Function(__) New Double(Dims.cols - 1) {}).ToArray()

        For Each kv In _entries
            output(kv.Key.Row)(kv.Key.Col) = kv.Value
        Next

        Return output
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Sub CheckDims(row As Integer, col As Integer)
        If row >= Dims.rows OrElse col >= Dims.cols Then
            Throw New Exception("array index out of bounds")
        End If
    End Sub

    ''' <summary>
    ''' transpose of this sparse matrix
    ''' </summary>
    ''' <param name="parallelism">
    ''' the parallelism configuration, the transposed key of each element 
    ''' will be evaluated in parallel when the matrix is large enough.
    ''' </param>
    Public Function Transpose(Optional parallelism As ParallelConfig = Nothing) As SparseMatrix
        Dim dims = (Me.Dims.cols, Me.Dims.rows)
        Dim src As KeyValuePair(Of RowCol, Double)() = EntryArray()
        Dim entries = New Dictionary(Of RowCol, Double)(src.Length)
        Dim degree As Integer = EffectiveDegree(parallelism, src.Length)

        If degree > 1 Then
            Dim keys As RowCol() = New RowCol(src.Length - 1) {}

            Call System.Threading.Tasks.Parallel.For(
                fromInclusive:=0,
                toExclusive:=src.Length,
                parallelOptions:=New ParallelOptions With {.MaxDegreeOfParallelism = degree},
                body:=Sub(i)
                          keys(i) = New RowCol(src(i).Key.Col, src(i).Key.Row)
                      End Sub)

            ' the transposition is a bijection, so that all of the keys are 
            ' unique and the result could be merged without any conflict
            For i As Integer = 0 To src.Length - 1
                entries(keys(i)) = src(i).Value
            Next
        Else
            For Each entry In _entries
                entries(New RowCol(entry.Key.Col, entry.Key.Row)) = entry.Value
            Next
        End If

        Return New SparseMatrix(entries, dims)
    End Function

    ''' <summary>
    ''' Element-wise multiplication of two matrices
    ''' </summary>
    ''' <param name="other"></param>
    ''' <param name="parallelism">
    ''' the parallelism configuration, the lookup of the other matrix will 
    ''' be done in parallel when the matrix is large enough.
    ''' </param>
    Public Function PairwiseMultiply(other As SparseMatrix, Optional parallelism As ParallelConfig = Nothing) As SparseMatrix
        Dim src As KeyValuePair(Of RowCol, Double)() = EntryArray()
        Dim newEntries = New Dictionary(Of RowCol, Double)(src.Length)
        Dim degree As Integer = EffectiveDegree(parallelism, src.Length)

        If degree > 1 Then
            Dim hits As Boolean() = New Boolean(src.Length - 1) {}
            Dim values As Double() = New Double(src.Length - 1) {}

            Call System.Threading.Tasks.Parallel.For(
                fromInclusive:=0,
                toExclusive:=src.Length,
                parallelOptions:=New ParallelOptions With {.MaxDegreeOfParallelism = degree},
                body:=Sub(i)
                          Dim v As Double = 0

                          If other._entries.TryGetValue(src(i).Key, v) Then
                              values(i) = src(i).Value * v
                              hits(i) = True
                          End If
                      End Sub)

            For i As Integer = 0 To src.Length - 1
                If hits(i) Then
                    newEntries(src(i).Key) = values(i)
                End If
            Next
        Else
            Dim v As Double = Nothing

            For Each kv In _entries
                If other._entries.TryGetValue(kv.Key, v) Then
                    newEntries(kv.Key) = kv.Value * v
                End If
            Next
        End If

        Return New SparseMatrix(newEntries, Dims)
    End Function

    ''' <summary>
    ''' Element-wise addition of two matrices
    ''' </summary>
    Public Function Add(other As SparseMatrix, Optional parallelism As ParallelConfig = Nothing) As SparseMatrix
        Return Me.ElementWiseWith(other, Function(x, y) x + y, parallelism)
    End Function

    ''' <summary>
    ''' Element-wise subtraction of two matrices
    ''' </summary>
    Public Function Subtract(other As SparseMatrix, Optional parallelism As ParallelConfig = Nothing) As SparseMatrix
        Return Me.ElementWiseWith(other, Function(x, y) x - y, parallelism)
    End Function

    ''' <summary>
    ''' Scalar multiplication of a matrix
    ''' </summary>
    Public Function MultiplyScalar(scalar As Double, Optional parallelism As ParallelConfig = Nothing) As SparseMatrix
        Return Map(Function(value, row, cols) value * scalar, parallelism)
    End Function

    ''' <summary>
    ''' Helper function for element-wise operations
    ''' </summary>
    ''' <remarks>
    ''' the previous implementation builds an union enumerable of the keys 
    ''' of the two matrices and then splits it into task pools for run the 
    ''' operation in parallel, which produces a lot of the intermediate 
    ''' objects.
    ''' 
    ''' this implementation evaluates the operation of the keys of the 
    ''' current matrix in parallel at first, and then folds the keys that 
    ''' only exists inside the other matrix.
    ''' </remarks>
    Private Function ElementWiseWith(other As SparseMatrix,
                                     op As Func(Of Double, Double, Double),
                                     Optional parallelism As ParallelConfig = Nothing) As SparseMatrix

        Dim src As KeyValuePair(Of RowCol, Double)() = EntryArray()
        Dim newEntries As New Dictionary(Of RowCol, Double)(src.Length + other._entries.Count)
        Dim degree As Integer = EffectiveDegree(parallelism, src.Length)

        If degree > 1 Then
            Dim values As Double() = New Double(src.Length - 1) {}

            ' op(x, y) for the common keys and op(x, 0) for the keys that 
            ' only exists inside the current matrix
            Call System.Threading.Tasks.Parallel.For(
                fromInclusive:=0,
                toExclusive:=src.Length,
                parallelOptions:=New ParallelOptions With {.MaxDegreeOfParallelism = degree},
                body:=Sub(i)
                          Dim v As Double = 0

                          If other._entries.TryGetValue(src(i).Key, v) Then
                              values(i) = op(src(i).Value, v)
                          Else
                              values(i) = op(src(i).Value, 0F)
                          End If
                      End Sub)

            For i As Integer = 0 To src.Length - 1
                newEntries(src(i).Key) = values(i)
            Next
        Else
            Dim v As Double = 0

            For i As Integer = 0 To src.Length - 1
                If other._entries.TryGetValue(src(i).Key, v) Then
                    newEntries(src(i).Key) = op(src(i).Value, v)
                Else
                    newEntries(src(i).Key) = op(src(i).Value, 0F)
                End If
            Next
        End If

        ' op(0, y) for the keys that only exists inside the other matrix
        For Each kv As KeyValuePair(Of RowCol, Double) In other._entries
            If Not newEntries.ContainsKey(kv.Key) Then
                newEntries(kv.Key) = op(0F, kv.Value)
            End If
        Next

        Return New SparseMatrix(newEntries, Dims)
    End Function

    ''' <summary>
    ''' Helper function for getting data, indices, and indptr arrays from a sparse matrix 
    ''' to follow csr matrix conventions. Super inefficient (and kind of defeats the 
    ''' purpose of this convention) but a lot of the ported python tree search logic depends 
    ''' on this data format.
    ''' </summary>
    Public Function GetCSR() As (indices As Integer(), values As Double(), indptr As Integer())
        Dim entries As New List(Of (value As Double, row As Integer, col As Integer))()

        Call ForEach(Sub(value, row, col) entries.Add((value, row, col)))
        Call entries.Sort(Function(a, b)
                              If a.row = b.row Then
                                  Return a.col - b.col
                              Else
                                  Return a.row - b.row
                              End If
                          End Function)

        Dim indices = New List(Of Integer)()
        Dim values = New List(Of Double)()
        Dim indptr = New List(Of Integer)()
        Dim currentRow = -1
        Dim xi As (value As Double, row As Integer, col As Integer)

        For i As Integer = 0 To entries.Count - 1
            xi = entries(i)

            If xi.row <> currentRow Then
                currentRow = xi.row
                indptr.Add(i)
            End If

            indices.Add(xi.col)
            values.Add(xi.value)
        Next

        Return (indices.ToArray(), values.ToArray(), indptr.ToArray())
    End Function
End Class
