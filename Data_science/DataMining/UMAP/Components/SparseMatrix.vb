Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Runtime.CompilerServices

Friend NotInheritable Class SparseMatrix

    ReadOnly _entries As Dictionary(Of SparseMatrix.RowCol, Single)

    Public Sub New(rows As IEnumerable(Of Integer), cols As IEnumerable(Of Integer), values As IEnumerable(Of Single), dims As (Integer, Integer))
        Me.New(SparseMatrix.Combine(rows, cols, values), dims)
    End Sub

    Private Sub New(entries As IEnumerable(Of (Integer, Integer, Single)), dims As (Integer, Integer))
        Me.Dims = dims
        _entries = New Dictionary(Of SparseMatrix.RowCol, Single)()

        For Each entryIndex In entries.[Select](Function(entry, index) (entry, index))
            Dim entry = entryIndex.Item1
            Dim index = entryIndex.Item2
            Me.CheckDims(entry.row, entry.col)
            _entries(New SparseMatrix.RowCol(entry.row, entry.col)) = entry.value
        Next
    End Sub

    Private Sub New(entries As Dictionary(Of SparseMatrix.RowCol, Single), dims As (Integer, Integer))
        Me.Dims = dims
        _entries = entries
    End Sub

    Private Shared Iterator Function Combine(rows As IEnumerable(Of Integer), cols As IEnumerable(Of Integer), values As IEnumerable(Of Single)) As IEnumerable(Of (Integer, Integer, Single))
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

    Public ReadOnly Property Dims As (rows As Integer, cols As Integer)

    Public Sub [Set](row As Integer, col As Integer, value As Single)
        CheckDims(row, col)
        _entries(New SparseMatrix.RowCol(row, col)) = value
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function [Get](row As Integer, col As Integer, Optional defaultValue As Single = 0) As Single
        CheckDims(row, col)
        Dim v As Single = Nothing
        Return If(_entries.TryGetValue(New SparseMatrix.RowCol(row, col), v), v, defaultValue)
    End Function

    Public Function GetAll() As IEnumerable(Of (Integer, Integer, Single))
        Return _entries.Select(Function(kv) (kv.Key.Row, kv.Key.Col, kv.Value))
    End Function

    Public Function GetRows() As IEnumerable(Of Integer)
        Return _entries.Keys.Select(Function(k) k.Row)
    End Function

    Public Function GetCols() As IEnumerable(Of Integer)
        Return _entries.Keys.Select(Function(k) k.Col)
    End Function

    Public Function GetValues() As IEnumerable(Of Single)
        Return _entries.Values
    End Function

    Public Sub ForEach(fn As Action(Of Single, Integer, Integer))
        For Each kv In _entries
            fn(kv.Value, kv.Key.Row, kv.Key.Col)
        Next
    End Sub

    Public Function Map(fn As Func(Of Single, Single)) As SparseMatrix
        Return Map(Function(value, row, col) fn(value))
    End Function

    Public Function Map(fn As Func(Of Single, Integer, Integer, Single)) As SparseMatrix
        Dim newEntries = _entries.ToDictionary(Function(kv) kv.Key, Function(kv) fn(kv.Value, kv.Key.Row, kv.Key.Col))
        Return New SparseMatrix(newEntries, Dims)
    End Function

    Public Function ToArray() As Single()()
        Dim output = Enumerable.Range(0, Dims.rows).[Select](Function(__) New Single(Dims.cols - 1) {}).ToArray()

        For Each kv In _entries
            output(kv.Key.Row)(kv.Key.Col) = kv.Value
        Next

        Return output
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Sub CheckDims(row As Integer, col As Integer)
#If DEBUG Then
        If row >= Dims.rows OrElse col >= Dims.cols Then Throw New Exception("array index out of bounds")
#End If
    End Sub

    Public Function Transpose() As SparseMatrix
        Dim dims = (Me.Dims.cols, Me.Dims.rows)
        Dim entries = New Dictionary(Of SparseMatrix.RowCol, Single)(_entries.Count)

        For Each entry In _entries
            entries(New SparseMatrix.RowCol(entry.Key.Col, entry.Key.Row)) = entry.Value
        Next

        Return New SparseMatrix(entries, dims)
    End Function

    ''' <summary>
    ''' Element-wise multiplication of two matrices
    ''' </summary>
    Public Function PairwiseMultiply(other As SparseMatrix) As SparseMatrix
        Dim newEntries = New Dictionary(Of SparseMatrix.RowCol, Single)(_entries.Count)
        Dim v As Single = Nothing

        For Each kv In _entries

            If other._entries.TryGetValue(kv.Key, v) Then
                newEntries(kv.Key) = kv.Value * v
            End If
        Next

        Return New SparseMatrix(newEntries, Dims)
    End Function

    ''' <summary>
    ''' Element-wise addition of two matrices
    ''' </summary>
    Public Function Add(other As SparseMatrix) As SparseMatrix
        Return Me.ElementWiseWith(other, Function(x, y) x + y)
    End Function

    ''' <summary>
    ''' Element-wise subtraction of two matrices
    ''' </summary>
    Public Function Subtract(other As SparseMatrix) As SparseMatrix
        Return Me.ElementWiseWith(other, Function(x, y) x - y)
    End Function

    ''' <summary>
    ''' Scalar multiplication of a matrix
    ''' </summary>
    Public Function MultiplyScalar(scalar As Single) As SparseMatrix
        Return Map(Function(value, row, cols) value * scalar)
    End Function

    ''' <summary>
    ''' Helper function for element-wise operations
    ''' </summary>
    Private Function ElementWiseWith(other As SparseMatrix, op As Func(Of Single, Single, Single)) As SparseMatrix
        Dim newEntries = New Dictionary(Of SparseMatrix.RowCol, Single)(_entries.Count)
        Dim x As Single = Nothing
        Dim y As Single = Nothing

        For Each k In _entries.Keys.Union(other._entries.Keys)
            newEntries(k) = op(If(_entries.TryGetValue(k, x), x, 0F), If(other._entries.TryGetValue(k, y), y, 0F))
        Next

        Return New SparseMatrix(newEntries, Dims)
    End Function

    ''' <summary>
    ''' Helper function for getting data, indices, and indptr arrays from a sparse matrix to follow csr matrix conventions. Super inefficient (and kind of defeats the purpose of this convention)
    ''' but a lot of the ported python tree search logic depends on this data format.
    ''' </summary>
    Public Function GetCSR() As (Integer(), Single(), Integer())
        Dim entries = New List(Of (Single, Integer, Integer))()
        ForEach(Sub(value, row, col) entries.Add((value, row, col)))
        entries.Sort(Function(a, b)
                         If a.row = b.row Then Return a.col - b.col
                         Return a.row - b.row
                     End Function)
        Dim indices = New List(Of Integer)()
        Dim values = New List(Of Single)()
        Dim indptr = New List(Of Integer)()
        Dim currentRow = -1
        Dim valueRowCol = Nothing

        For i = 0 To entries.Count - 1
            valueRowCol = entries(i)

            If row <> currentRow Then
                currentRow = row
                indptr.Add(i)
            End If

            indices.Add(col)
            values.Add(value)
        Next

        Return (indices.ToArray(), values.ToArray(), indptr.ToArray())
    End Function

    Private Class RowCol : Implements IEquatable(Of SparseMatrix.RowCol)

        Public Sub New(row As Integer, col As Integer)
            Me.Row = row
            Me.Col = col
        End Sub

        Public ReadOnly Property Row As Integer
        Public ReadOnly Property Col As Integer

        ' 2019-06-24 DWR: Structs get default Equals and GetHashCode implementations but they can be slow - having these versions makes the code run much quicker
        ' and it seems a good practice to throw in IEquatable<RowCol> to avoid boxing when Equals is called
        Public Overloads Function Equals(other As SparseMatrix.RowCol) As Boolean Implements IEquatable(Of RowCol).Equals
            Return other.Row = Row AndAlso other.Col = Col
        End Function

        Public Overrides Function Equals(obj As Object) As Boolean
            Dim rc = TryCast(obj, SparseMatrix.RowCol)
            Return (Not rc Is Nothing) AndAlso rc.Equals(Me)
        End Function

        Public Overrides Function GetHashCode() As Integer ' Courtesy of https://stackoverflow.com/a/263416/3813189
            ' BEGIN TODO : Visual Basic does Not support checked statements!
            Dim hash = 17 ' Overflow is fine, just wrap
            hash = hash * 23 + Row
            hash = hash * 23 + Col
            Return hash
            ' End TODO : Visual Basic does Not support checked statements!
        End Function
    End Class
End Class
