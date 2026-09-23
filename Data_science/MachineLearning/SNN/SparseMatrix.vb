#Region "Microsoft.VisualBasic::be95c86909cfce2d33fa285a93ed7a70, Data_science\MachineLearning\SNN\SparseMatrix.vb"

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

    '   Total Lines: 322
    '    Code Lines: 216 (67.08%)
    ' Comment Lines: 66 (20.50%)
    '    - Xml Docs: 59.09%
    ' 
    '   Blank Lines: 40 (12.42%)
    '     File Size: 12.41 KB


    ' Enum SparseNormalization
    ' 
    '     FanIn, FanOut, GlobalMax, None
    ' 
    '  
    ' 
    ' 
    ' 
    ' Class SparseMatrix
    ' 
    '     Properties: ColumnIndices, Columns, NonZeros, RowPointers, Rows
    '                 Values
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: FromTriplets, SpMM, ToDense, ToString
    ' 
    '     Sub: Normalize
    ' 
    ' /********************************************************************************/

#End Region

' ============================================================================
' SparseMatrix.vb — 稀疏突触连接矩阵（CSR：Compressed Sparse Row）
'
' 用途：承载 FlyWire 等真实神经连接组。
'   W[pre, post]：行 = 突触前神经元（pre），列 = 突触后神经元（post）。
'   全连接 LIF 层用稠密 Tensor [in, units] 存储权重；连接组规模（十万级神
'   经元 / 千万级突触）下稠密矩阵不可行，故此处用 CSR 逐行压缩存储非零边。
'
' 存储布局（行优先）：
'   _rowPtr(0..Rows)   第 r 行的非零边在 _colIdx/_values 中的区间 [rowPtr(r), rowPtr(r+1))
'   _colIdx(0..nnz-1)  每条边的突触后神经元索引（行内按列非降序）
'   _values(0..nnz-1)  每条边的权重
'
' 构建：FromTriplets 采用两次稳定计数排序（先按列、再按行）得到 (row,col) 字典序，
'       再线性合并重复边（同 (pre,post) 权重累加）。整体复杂度 O(nnz + rows + columns)，
'       不使用哈希字典，避免千万级突触下的巨大内存开销。
' 前向：SpMM 计算稀疏 × 稠密：X[batch, Rows] · W[Rows, Columns] → [batch, Columns]。
'       计算本身委托给当前张量计算后端（Tensor.computeKernel）——默认 CPU 标量实现，
'       注册 CUDA 后自动走 GPU 稀疏内核（见 cuda/ILCudaTensor/Kernels/spmm.cu）。
' ============================================================================

Imports Microsoft.VisualBasic.MachineLearning.TensorFlow
Imports Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute
Imports std = System.Math

''' <summary>突触权重归一化方式</summary>
Public Enum SparseNormalization
    ''' <summary>保留原始突触计数（不做任何缩放）</summary>
    None
    ''' <summary>按扇入（列 / 突触后）归一化：每个突触后神经元的所有入边权重和为 1</summary>
    FanIn
    ''' <summary>按扇出（行 / 突触前）归一化：每个突触前神经元的所有出边权重和为 1</summary>
    FanOut
    ''' <summary>按全局最大权重（绝对值）缩放</summary>
    GlobalMax
End Enum

''' <summary>
''' 稀疏突触连接矩阵（CSR 格式）。行 = 突触前神经元，列 = 突触后神经元。
''' </summary>
Public Class SparseMatrix

    Private ReadOnly _rows As Integer
    Private ReadOnly _columns As Integer
    Private ReadOnly _rowPtr As Integer()
    Private ReadOnly _colIdx As Integer()
    Private ReadOnly _values As Double()

    ''' <summary>
    ''' CSR 计算载体（零拷贝包装 <c>_rowPtr/_colIdx/_values</c>）。
    ''' 稀疏乘法委托给 <see cref="Tensor.computeKernel"/> 时传递它；
    ''' 权重就地修改后必须通过 <see cref="SparseCsr.MarkModified"/> 声明失效。
    ''' </summary>
    Private ReadOnly _csr As SparseCsr

    ''' <summary>突触前神经元数量（矩阵行数）</summary>
    Public ReadOnly Property Rows As Integer
        Get
            Return _rows
        End Get
    End Property

    ''' <summary>突触后神经元数量（矩阵列数）</summary>
    Public ReadOnly Property Columns As Integer
        Get
            Return _columns
        End Get
    End Property

    ''' <summary>非零边（突触）数量</summary>
    Public ReadOnly Property NonZeros As Integer
        Get
            Return _values.Length
        End Get
    End Property

    ''' <summary>行偏移数组（长度 Rows+1），只读访问（用于高级自定义算子）</summary>
    Public ReadOnly Property RowPointers As Integer()
        Get
            Return _rowPtr
        End Get
    End Property

    ''' <summary>非零边的列索引数组（长度 nnz）</summary>
    Public ReadOnly Property ColumnIndices As Integer()
        Get
            Return _colIdx
        End Get
    End Property

    ''' <summary>非零边的权重数组（长度 nnz）</summary>
    Public ReadOnly Property Values As Double()
        Get
            Return _values
        End Get
    End Property

    ''' <summary>
    ''' CSR 计算载体（零拷贝包装 <c>RowPointers/ColumnIndices/Values</c>）。
    ''' </summary>
    ''' <remarks>
    ''' <c>SparseLIFLayer</c> 用它把稀疏结构直接交给 <c>ITensorCompute.LifStep</c>；
    ''' 需要自行编写稀疏算子（或做性能分解）的调用方也可以直接用：
    ''' <code>
    '''   Tensor.computeKernel.LifStep(matrix.Csr, sPrev, ext, h, s, counts, beta, threshold, False)
    ''' </code>
    ''' 注意它包装的就是本类的三个数组，因此 <see cref="MarkModified"/> 的契约同样适用。
    ''' </remarks>
    Public ReadOnly Property Csr As SparseCsr
        Get
            Return _csr
        End Get
    End Property

    ''' <summary>
    ''' 声明 <see cref="Values"/> 已被就地修改，使设备端（GPU）缓存的 CSR 副本失效。
    ''' </summary>
    ''' <remarks>
    ''' 凡是在本类之外就地改过 <see cref="Values"/> 的调用方（典型例子：连接组的增益标定
    ''' <c>SynapseTriplets.ApplyGain</c>）都必须调用本方法。
    ''' 不调用的话，GPU 会继续复用显存里的旧权重 —— 表现为「CPU 与 GPU 结果不同，
    ''' 且差异无法从数据上解释」这类最难定位的伪 bug。
    ''' <see cref="Normalize"/> 内部已自行调用。
    ''' </remarks>
    Public Sub MarkModified()
        _csr.MarkModified()
    End Sub

    Private Sub New(rows As Integer, columns As Integer,
                    rowPtr As Integer(), colIdx As Integer(), values As Double())
        _rows = rows
        _columns = columns
        _rowPtr = rowPtr
        _colIdx = colIdx
        _values = values
        _csr = New SparseCsr(rows, columns, rowPtr, colIdx, values)
    End Sub

#Region "构建"

    ''' <summary>
    ''' 由三元组 (pre, post, weight) 构建 CSR 稀疏矩阵。
    ''' 相同 (pre, post) 的重复边按权重累加合并。
    ''' </summary>
    ''' <param name="pre">突触前神经元索引，取值 [0, rows)</param>
    ''' <param name="post">突触后神经元索引，取值 [0, columns)</param>
    ''' <param name="weight">突触权重（如归一化后的突触计数）</param>
    ''' <param name="rows">突触前神经元数量</param>
    ''' <param name="columns">突触后神经元数量</param>
    Public Shared Function FromTriplets(pre As Integer(), post As Integer(), weight As Double(),
                                        rows As Integer, columns As Integer) As SparseMatrix
        If pre Is Nothing OrElse post Is Nothing OrElse weight Is Nothing Then
            Throw New ArgumentNullException(NameOf(pre), "pre/post/weight 不能为空")
        End If
        If pre.Length <> post.Length OrElse pre.Length <> weight.Length Then
            Throw New ArgumentException("pre/post/weight 的长度必须一致")
        End If
        If rows <= 0 OrElse columns <= 0 Then
            Throw New ArgumentException("rows/columns 必须为正整数")
        End If

        Dim n = pre.Length

        ' 无任何连接：直接返回空矩阵
        If n = 0 Then
            Return New SparseMatrix(rows, columns, New Integer(rows) {}, New Integer() {}, New Double() {})
        End If

        ' 索引合法性校验（尽早失败，避免后续越界静默出错）
        For i = 0 To n - 1
            If pre(i) < 0 OrElse pre(i) >= rows Then
                Throw New ArgumentOutOfRangeException($"pre[{i}]={pre(i)} 超出 [0, {rows})")
            End If
            If post(i) < 0 OrElse post(i) >= columns Then
                Throw New ArgumentOutOfRangeException($"post[{i}]={post(i)} 超出 [0, {columns})")
            End If
        Next

        ' ---- 第一次稳定计数排序：按列（post）----
        Dim colCount = New Integer(columns - 1) {}
        For i = 0 To n - 1
            colCount(post(i)) += 1
        Next
        Dim colOffset = New Integer(columns - 1) {}
        Dim acc = 0
        For c = 0 To columns - 1
            colOffset(c) = acc
            acc += colCount(c)
        Next

        Dim tmpRow = New Integer(n - 1) {}
        Dim tmpCol = New Integer(n - 1) {}
        Dim tmpVal = New Double(n - 1) {}
        For i = 0 To n - 1
            Dim c = post(i)
            Dim p = colOffset(c)
            colOffset(c) = p + 1
            tmpRow(p) = pre(i)
            tmpCol(p) = c
            tmpVal(p) = weight(i)
        Next

        ' ---- 第二次稳定计数排序：按行（pre），并同时生成 CSR rowPtr ----
        Dim rowCount = New Integer(rows - 1) {}
        For i = 0 To n - 1
            rowCount(tmpRow(i)) += 1
        Next
        Dim rowPtr = New Integer(rows) {}
        acc = 0
        For r = 0 To rows - 1
            rowPtr(r) = acc
            acc += rowCount(r)
        Next
        rowPtr(rows) = acc

        Dim colIdx = New Integer(n - 1) {}
        Dim values = New Double(n - 1) {}
        Dim cursor = CType(rowPtr.Clone(), Integer())
        For i = 0 To n - 1
            Dim r = tmpRow(i)
            Dim p = cursor(r)
            cursor(r) = p + 1
            colIdx(p) = tmpCol(i)
            values(p) = tmpVal(i)
        Next

        ' ---- 同一行内合并重复列（列已升序），并压缩存储 ----
        Dim outCol = New Integer(n - 1) {}
        Dim outVal = New Double(n - 1) {}
        Dim newRowPtr = New Integer(rows) {}
        Dim w = 0
        For r = 0 To rows - 1
            newRowPtr(r) = w
            Dim k = rowPtr(r)
            Dim kEnd = rowPtr(r + 1)
            While k < kEnd
                Dim c = colIdx(k)
                Dim v = values(k)
                k += 1
                While k < kEnd AndAlso colIdx(k) = c
                    v += values(k)
                    k += 1
                End While
                outCol(w) = c
                outVal(w) = v
                w += 1
            End While
        Next
        newRowPtr(rows) = w

        If w < n Then
            Array.Resize(outCol, w)
            Array.Resize(outVal, w)
        End If

        Return New SparseMatrix(rows, columns, newRowPtr, outCol, outVal)
    End Function

#End Region

#Region "归一化"

    ''' <summary>
    ''' 就地按指定方式缩放权重。零和（无入边/出边）的神经元保持原值不变。
    ''' </summary>
    Public Sub Normalize(mode As SparseNormalization)
        Select Case mode
            Case SparseNormalization.None
                Return

            Case SparseNormalization.GlobalMax
                Dim mx = 0.0
                For i = 0 To _values.Length - 1
                    Dim a = std.Abs(_values(i))
                    If a > mx Then mx = a
                Next
                If mx > 0.0 Then
                    For i = 0 To _values.Length - 1
                        _values(i) /= mx
                    Next
                End If

            Case SparseNormalization.FanOut
                Dim sum = New Double(_rows - 1) {}
                For r = 0 To _rows - 1
                    Dim s = 0.0
                    For k = _rowPtr(r) To _rowPtr(r + 1) - 1
                        s += _values(k)
                    Next
                    sum(r) = s
                Next
                For r = 0 To _rows - 1
                    If sum(r) > 0.0 Then
                        For k = _rowPtr(r) To _rowPtr(r + 1) - 1
                            _values(k) /= sum(r)
                        Next
                    End If
                Next

            Case SparseNormalization.FanIn
                Dim sum = New Double(_columns - 1) {}
                For k = 0 To _values.Length - 1
                    sum(_colIdx(k)) += _values(k)
                Next
                For k = 0 To _values.Length - 1
                    Dim s = sum(_colIdx(k))
                    If s > 0.0 Then _values(k) /= s
                Next

            Case Else
                Throw New ArgumentOutOfRangeException(NameOf(mode))
        End Select

        ' 权重已就地修改：使设备端（GPU）缓存的 CSR 副本失效
        _csr.MarkModified()
    End Sub

#End Region

#Region "前向计算"

    ''' <summary>
    ''' 稀疏 × 稠密矩阵乘法：X[batch, Rows] · W[Rows, Columns] → [batch, Columns]。
    '''
    ''' 实际计算委托给当前生效的张量计算后端（<c>Tensor.computeKernel</c>）：
    ''' 默认走 CPU 标量实现；若调用方已调用 <c>CudaTensor.Register()</c>，
    ''' 则自动走 CUDA CSR-SpMM 内核（cuda/ILCudaTensor/Kernels/spmm.cu）。
    ''' 无连接 / 规模过小 / 内核不可用时后端内部会回退 CPU，语义保持一致。
    ''' </summary>
    Public Function SpMM(dense As Tensor) As Tensor
        If dense Is Nothing Then
            Throw New ArgumentNullException(NameOf(dense))
        End If
        If dense.Rank <> 2 OrElse dense.Shape(1) <> _rows Then
            Throw New ArgumentException(
                $"SpMM 输入形状应为 [batch, {_rows}]，实际 [{String.Join(",", dense.Shape)}]")
        End If

        Return Tensor.computeKernel.SpMM(_csr, dense)
    End Function

    ''' <summary>转成稠密张量（仅供小规模校验/调试，大规模连接组下请勿调用）</summary>
    Public Function ToDense() As Tensor
        Return _csr.ToDense()
    End Function

#End Region

    Public Overrides Function ToString() As String
        Return $"SparseMatrix({_rows}x{_columns}, nnz={_values.Length})"
    End Function

End Class
