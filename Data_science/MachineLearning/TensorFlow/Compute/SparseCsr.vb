#Region "Microsoft.VisualBasic::f849fe795315791abeaa0acc5dbf05b4, Data_science\MachineLearning\TensorFlow\Compute\SparseCsr.vb"

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

    '   Total Lines: 175
    '    Code Lines: 102 (58.29%)
    ' Comment Lines: 50 (28.57%)
    '    - Xml Docs: 56.00%
    ' 
    '   Blank Lines: 23 (13.14%)
    '     File Size: 7.64 KB


    '     Class SparseCsr
    ' 
    '         Properties: ColumnIndices, Columns, NonZeros, RowPointers, Rows
    '                     Values, Version
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: ToDense, ToString
    ' 
    '         Sub: MarkModified
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' SparseCsr —— CSR（Compressed Sparse Row）稀疏矩阵载体
'
' 约定：行 = 突触前神经元（pre），列 = 突触后神经元（post）。
'
' 它是「稀疏算子」的跨后端传输单元：<see cref="ITensorCompute.SpMM"/> 接收本对象
' 与一个稠密张量，计算出稀疏 × 稠密的结果。之所以把 CSR 数据封装成独立类型，
' 是因为：
'   * TensorFlow 工程不能反向依赖 SNN，故载体必须定义在 Compute 命名空间；
'   * GPU 后端需要以「CSR 数组驻留显存」的方式缓存，缓存键需要稳定的对象身份
'     与可检测的版本号 —— 这正是 <see cref="Version"/> / <see cref="MarkModified"/>。
'
' 生命周期约定（与 <see cref="Tensor.MarkHostModified"/> 同构）：
'   * 构造之后 CSR 被视为不可变；
'   * 若宿主**就地修改**了 <see cref="Values"/>（例如权重归一化），必须在修改后
'     调用 <see cref="MarkModified"/>，否则设备端缓存会继续复用旧的显存副本，
'     导致静默错误。
'
' 注意：本类**刻意不覆写 Equals/GetHashCode**，从而保证引用相等语义 —— 设备端
' 缓存以对象引用为键，覆写相等性会破坏缓存隔离。
' ---------------------------------------------------------------------------

Imports std = System.Math

Namespace Compute

    ''' <summary>
    ''' CSR 稀疏矩阵载体（行 = 突触前，列 = 突触后）。
    ''' 承载 <see cref="ITensorCompute.SpMM"/> 算子所需的全部稀疏结构信息。
    ''' </summary>
    Public Class SparseCsr

        Private ReadOnly _rows As Integer
        Private ReadOnly _columns As Integer
        Private ReadOnly _rowPtr As Integer()
        Private ReadOnly _colIdx As Integer()
        Private ReadOnly _values As Double()
        Private _version As Long = 0L

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

        ''' <summary>行偏移数组（长度 Rows+1，单调非降，首元素 0、末元素 = 非零数）</summary>
        Public ReadOnly Property RowPointers As Integer()
            Get
                Return _rowPtr
            End Get
        End Property

        ''' <summary>非零元素的列索引（长度 = 非零数，每段内非降序）</summary>
        Public ReadOnly Property ColumnIndices As Integer()
            Get
                Return _colIdx
            End Get
        End Property

        ''' <summary>非零元素的权重（长度 = 非零数；就地修改后必须调用 <see cref="MarkModified"/>)</summary>
        Public ReadOnly Property Values As Double()
            Get
                Return _values
            End Get
        End Property

        ''' <summary>非零元素（突触）数量</summary>
        Public ReadOnly Property NonZeros As Integer
            Get
                Return _values.Length
            End Get
        End Property

        ''' <summary>
        ''' 数据版本号。任何就地修改 <see cref="Values"/> 之后都应调用
        ''' <see cref="MarkModified"/> 使其自增，供设备端缓存判断失效。
        ''' </summary>
        Public ReadOnly Property Version As Long
            Get
                Return _version
            End Get
        End Property

        ''' <summary>
        ''' 构造 CSR 载体（不做数据拷贝，直接持有传入数组）。
        ''' </summary>
        ''' <param name="rows">行数（突触前神经元数）</param>
        ''' <param name="columns">列数（突触后神经元数）</param>
        ''' <param name="rowPointers">行偏移数组，长度必须为 rows+1，单调非降</param>
        ''' <param name="columnIndices">列索引数组，取值 [0, columns)</param>
        ''' <param name="values">权重数组，与 columnIndices 等长</param>
        Public Sub New(rows As Integer, columns As Integer,
                       rowPointers As Integer(), columnIndices As Integer(), values As Double())

            If rows <= 0 OrElse columns <= 0 Then
                Throw New ArgumentException($"rows/columns 必须为正整数，实际 {rows}x{columns}")
            End If
            If rowPointers Is Nothing OrElse columnIndices Is Nothing OrElse values Is Nothing Then
                Throw New ArgumentNullException(NameOf(rowPointers), "rowPointers/columnIndices/values 不能为空")
            End If
            If rowPointers.Length <> rows + 1 Then
                Throw New ArgumentException(
                    $"rowPointers 长度应为 Rows+1={rows + 1}，实际 {rowPointers.Length}")
            End If
            If columnIndices.Length <> values.Length Then
                Throw New ArgumentException(
                    $"columnIndices({columnIndices.Length}) 与 values({values.Length}) 长度必须一致")
            End If
            If rowPointers(0) <> 0 OrElse rowPointers(rows) <> values.Length Then
                Throw New ArgumentException(
                    $"rowPointers 必须从 0 开始、到 nnz({values.Length}) 结束，实际 [{rowPointers(0)}, {rowPointers(rows)}]")
            End If

            ' 校验行偏移单调非降（顺带校验每段列索引落在合法区间）
            For r As Integer = 0 To rows - 1
                Dim k = rowPointers(r)
                Dim kEnd = rowPointers(r + 1)

                If kEnd < k Then
                    Throw New ArgumentException($"rowPointers 必须单调非降，第 {r} 行 [{k}, {kEnd}] 非法")
                End If
                For i As Integer = k To kEnd - 1
                    Dim c = columnIndices(i)
                    If c < 0 OrElse c >= columns Then
                        Throw New ArgumentOutOfRangeException(
                            $"columnIndices[{i}]={c} 超出 [0, {columns})")
                    End If
                Next
            Next

            _rows = rows
            _columns = columns
            _rowPtr = rowPointers
            _colIdx = columnIndices
            _values = values
        End Sub

        ''' <summary>
        ''' 声明 <see cref="Values"/> 已被就地修改，使设备端缓存副本失效。
        ''' </summary>
        Public Sub MarkModified()
            System.Threading.Interlocked.Increment(_version)
        End Sub

        ''' <summary>
        ''' 展开为稠密张量 [Rows, Columns]（仅供小规模校验 / 调试，大规模连接组下请勿调用）。
        ''' </summary>
        Public Function ToDense() As Tensor
            Dim t = New Tensor(_rows, _columns)
            Dim d = t.Data

            For r As Integer = 0 To _rows - 1
                For k As Integer = _rowPtr(r) To _rowPtr(r + 1) - 1
                    d(r * _columns + _colIdx(k)) += _values(k)
                Next
            Next

            Return t
        End Function

        Public Overrides Function ToString() As String
            Return $"SparseCsr({_rows}x{_columns}, nnz={NonZeros}, v{_version})"
        End Function

    End Class

End Namespace
