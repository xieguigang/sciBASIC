''' <summary>
''' 列压缩（CSC）稀疏设计矩阵。
''' </summary>
''' <remarks>
''' 既有 <c>Math.NET5</c> 的 <c>SparseMatrix</c> 是按行字典结构，
''' 不适合坐标下降"固定列、扫描样本"的热循环；本类型提供 CSC 三元组，
''' 并可展开为内核使用的列主序数组。
''' </remarks>
Public NotInheritable Class SparseCSC

    ''' <summary>样本数</summary>
    Public ReadOnly Property N As Integer
    ''' <summary>特征数</summary>
    Public ReadOnly Property P As Integer
    ''' <summary>非零元素个数</summary>
    Public ReadOnly Property Nnz As Integer
    ''' <summary>非零值，按列分组</summary>
    Public ReadOnly Property Values As Double()
    ''' <summary>每个非零值所在的行号</summary>
    Public ReadOnly Property RowIndices As Integer()
    ''' <summary>列指针，长度 p+1</summary>
    Public ReadOnly Property ColumnPointers As Integer()

    Public Sub New(n As Integer, p As Integer, columnPointers As Integer(), rowIndices As Integer(), values As Double())
        If columnPointers Is Nothing OrElse columnPointers.Length <> p + 1 Then
            Throw New ArgumentException("columnPointers must have length p+1!", NameOf(columnPointers))
        End If
        If rowIndices Is Nothing OrElse values Is Nothing OrElse rowIndices.Length <> values.Length Then
            Throw New ArgumentException("rowIndices and values must have the same length!", NameOf(values))
        End If
        If columnPointers(p) <> values.Length Then
            Throw New ArgumentException("columnPointers(p) must equal the number of stored values!", NameOf(columnPointers))
        End If
        For i As Integer = 0 To values.Length - 1
            If Not Double.IsFinite(values(i)) Then
                Throw New ArgumentException($"values({i}) must be finite!", NameOf(values))
            End If
            If rowIndices(i) < 0 OrElse rowIndices(i) >= n Then
                Throw New ArgumentException($"rowIndices({i}) is out of range!", NameOf(rowIndices))
            End If
        Next

        Me.N = n
        Me.P = p
        Me.Nnz = values.Length
        Me.ColumnPointers = columnPointers
        Me.RowIndices = rowIndices
        Me.Values = values
    End Sub

    ''' <summary>从稠密行主序矩阵构造 CSC（自动丢弃精确零）。</summary>
    Public Shared Function FromDense(x As Double()()) As SparseCSC
        Dim n As Integer = x.Length
        Dim p As Integer = x(0).Length
        Dim ptr As New List(Of Integer)()
        Dim rows As New List(Of Integer)()
        Dim vals As New List(Of Double)()

        ptr.Add(0)
        For j As Integer = 0 To p - 1
            For i As Integer = 0 To n - 1
                If x(i)(j) <> 0.0R Then
                    rows.Add(i)
                    vals.Add(x(i)(j))
                End If
            Next
            ptr.Add(vals.Count)
        Next

        Return New SparseCSC(n, p, ptr.ToArray(), rows.ToArray(), vals.ToArray())
    End Function

    ''' <summary>展开为列主序稠密数组，供标准坐标下降内核使用。</summary>
    Public Function ToColumnArrays() As Double()()
        Dim columns As Double()() = New Double(P - 1)() {}

        For j As Integer = 0 To P - 1
            Dim col As Double() = New Double(N - 1) {}
            For k As Integer = ColumnPointers(j) To ColumnPointers(j + 1) - 1
                col(RowIndices(k)) = Values(k)
            Next
            columns(j) = col
        Next

        Return columns
    End Function

    ''' <summary>展开为行主序稠密矩阵，供 <c>GlmnetData</c> 使用。</summary>
    Public Function ToDenseRows() As Double()()
        Dim x As Double()() = New Double(N - 1)() {}

        For i As Integer = 0 To N - 1
            x(i) = New Double(P - 1) {}
        Next
        For j As Integer = 0 To P - 1
            For k As Integer = ColumnPointers(j) To ColumnPointers(j + 1) - 1
                x(RowIndices(k))(j) = Values(k)
            Next
        Next

        Return x
    End Function

End Class
