#Region "Microsoft.VisualBasic::b7c4d2e1f0a3948576b1c2d3e4f5a6b7, Data_science\MachineLearning\TensorFlow\TensorH.vb"

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

    '     Class TensorH

    '         Properties: Data, IsDisposed, Length, Rank, Shape, TotalLength, Version
    '         Constructor: (+3 Overloads) Sub New

    '         Function: Clone, CloneT, CopyFrom, FromDoubles, FromTensor, FromTensorF
    '                   GetFlatIndex, Scalar, ToDoubles, ToTensor, ToTensorF, Wrap
    '                   Zeros, Ones, Filled
    '         Sub: Clear, Dispose, Fill, InvalidateAllDeviceCaches, MarkHostModified
    '         Operators: (+2 Overloads) -, (+2 Overloads) *, /, (+2 Overloads) +

    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' TensorH —— 半精度 (System.Half / float16) 张量
'
' 与既有的 <see cref="Tensor"/>（Double 存储）、<see cref="TensorF"/>（Single 存储）
' 并列存在的第三条精度栈，定位与 TensorF 完全同构：
'
'   * 存储与带宽再减半：相对 Single 减半，相对 Double 只有 1/4；
'     SIMD 通道数与 Single 相同（HalfSimd 的 8 路 Vector128 内核）；
'   * 面向"内存带宽受限 + 精度要求不高"的场景（大 batch 推理、可视化中间量、
'     以及未来 fp16 tensor core 的 GPU 后端 —— 存储侧先就位）；
'   * 通过 <see cref="computeKernelH"/> 持有可插拔后端，与另两条栈的切换语义一致。
'
' 数值约束（使用前必读）：
'   * 表示范围仅 ±65520（正规数下界 6.1e-5），exp 在 x > 11.1 即溢出；
'   * 因此 Softmax / 损失 / 激活等算子在实现上**必须**"中间量升到 Single/Double、
'     出口一次舍入"（TensorComputeHBase 与 SIMDTensorH 均已遵循该约定）；
'   * 与 Tensor / TensorF 的双向互转是精确 widening 或有控 narrowing
'     （批量转换直接复用 <see cref="Numerics.HalfSimd"/> 的转换内核）。
'
' 设计要点（与 TensorF 一致）：
'   * 索引器**不含**任何版本计数 / 原子操作，可安全用于高频热循环；
'   * GPU 后端（未来的 CudaTensorH）需要调用方显式调用
'     <see cref="MarkHostModified"/> 或 <see cref="InvalidateAllDeviceCaches"/>。
'
' 用法：
'     Dim h As TensorH = TensorH.Zeros({64, 64, 64})
'     h(1, 2, 3) = CType(1.5F, Half)
'     Dim raw As Half() = h.Data      ' 热循环直连底层数组
'     Dim f As TensorF = h.ToTensorF()
' ---------------------------------------------------------------------------

Imports System.Threading
Imports halfSimd = Microsoft.VisualBasic.Math.Numerics
Imports tfCompute = Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute

''' <summary>
''' 半精度（<see cref="System.Half"/> / float16）多维张量。与双精度 <see cref="Tensor"/>、
''' 单精度 <see cref="TensorF"/> 并列，通过 <see cref="ToTensor"/> / <see cref="ToTensorF"/>
''' 等方法双向互转。
''' </summary>
''' <remarks>
''' 索引器被刻意设计为"纯数组读写"，不包含版本计数或任何原子操作，
''' 以便热循环可以直接使用。代价是 GPU 后端无法自动感知就地写入，需要调用方显式调用
''' <see cref="MarkHostModified"/> 或 <see cref="InvalidateAllDeviceCaches"/>
''' （与 <see cref="TensorF"/> 在这方面的语义完全一致）。
''' </remarks>
Public Class TensorH : Implements ICloneable, IDisposable

    ''' <summary>底层数据数组（半精度）</summary>
    Private _Data As Half()

    ''' <summary>张量形状（各维度大小）</summary>
    Private _Shape As Integer()

    ''' <summary>各维度步长（用于多维索引 -> 一维下标）</summary>
    Private _DimProds As Integer()

    Private _disposed As Boolean

    ''' <summary>
    ''' 本实例自身的主机写入计数（配合 <see cref="DeviceEpoch"/> 构成 <see cref="Version"/>）。
    ''' </summary>
    Private _version As Long

    ''' <summary>切换计算后端时使用的同步根对象</summary>
    Public Shared ReadOnly SyncRoot As New Object()

    ''' <summary>
    ''' 半精度张量当前生效的计算后端。默认是 SIMD 加速的 CPU 实现
    ''' （<see cref="tfCompute.SIMDTensorH.Default"/>）；后续可切换为 GPU 后端。
    ''' </summary>
    Public Shared Property computeKernelH As tfCompute.ITensorComputeH = tfCompute.SIMDTensorH.Default

    ''' <summary>
    ''' 全局的设备端缓存纪元号（半精度栈）。
    ''' </summary>
    ''' <remarks>
    ''' 与双精度 <see cref="Tensor.DeviceEpoch"/>、单精度 <see cref="TensorF.DeviceEpoch"/>
    ''' 完全同构，只是作用于第三条栈。GPU 后端（<c>CudaTensorH</c>）会把张量的主机数据
    ''' 上传到显存并做 LRU 缓存，缓存键是"主机数组引用 + <see cref="Version"/>"。
    ''' 当调用方<b>绕过</b>索引器、直接就地修改了底层 <c>Half()</c> 数组时，
    ''' 张量自身无从感知，显存副本会一直停留在旧值上，导致后续计算静默使用过期数据。
    '''
    ''' 这类调用方在修改完成之后必须调用一次 <see cref="InvalidateAllDeviceCaches"/>，
    ''' 让所有张量的缓存副本失效（<see cref="Version"/> 会随之改变）。
    ''' </remarks>
    Private Shared _deviceEpoch As Long = 0

    ''' <summary>当前的全局设备端缓存纪元号（半精度栈）</summary>
    Public Shared ReadOnly Property DeviceEpoch As Long
        Get
            Return Interlocked.Read(_deviceEpoch)
        End Get
    End Property

    ''' <summary>
    ''' 让所有半精度张量的设备端缓存副本失效。
    ''' </summary>
    ''' <remarks>
    ''' 仅在"绕过索引器直接就地修改底层数组"之后需要调用；
    ''' 通过索引器的写入不需要调用本方法。
    ''' </remarks>
    Public Shared Sub InvalidateAllDeviceCaches()
        Interlocked.Increment(_deviceEpoch)
    End Sub

#Region "基础属性"

    ''' <summary>
    ''' 存储张量数据的一维数组（行优先顺序）。热循环可直接使用以避免索引器开销。
    ''' </summary>
    Public ReadOnly Property Data As Half()
        Get
            Return _Data
        End Get
    End Property

    ''' <summary>张量的形状（各维度大小）</summary>
    Public ReadOnly Property Shape As Integer()
        Get
            Return _Shape
        End Get
    End Property

    ''' <summary>元素总数</summary>
    Public ReadOnly Property Length As Integer
        Get
            Return _Data.Length
        End Get
    End Property

    ''' <summary>元素总数（与 <see cref="Length"/> 相同，为与另两条栈保持命名一致而提供）</summary>
    Public ReadOnly Property TotalLength As Integer
        Get
            Return _Data.Length
        End Get
    End Property

    ''' <summary>张量的维度数</summary>
    Public ReadOnly Property Rank As Integer
        Get
            Return _Shape.Length
        End Get
    End Property

    ''' <summary>是否已被释放</summary>
    Public ReadOnly Property IsDisposed As Boolean
        Get
            Return _disposed
        End Get
    End Property

    ''' <summary>
    ''' 数据版本号（只读）。任何原地写入都应使其自增，供设备端缓存判断失效。
    ''' </summary>
    ''' <remarks>
    ''' 返回值 = 本实例自身的写入计数 + <see cref="DeviceEpoch"/>。
    ''' 注意：本类的索引器被刻意设计为纯数组读写、不做版本计数，
    ''' 因此这里<b>不会</b>自动跟踪写入。需要让设备端缓存失效的调用方必须显式调用
    ''' <see cref="MarkHostModified"/> 或 <see cref="InvalidateAllDeviceCaches"/>。
    ''' </remarks>
    Public ReadOnly Property Version As Long
        Get
            Return _version + DeviceEpoch
        End Get
    End Property

    ''' <summary>
    ''' 声明本张量的主机数据已被<b>绕过索引器</b>就地修改过，从而让设备端缓存副本失效。
    ''' </summary>
    Public Sub MarkHostModified()
        Interlocked.Increment(_version)
    End Sub

#End Region

#Region "构造函数"

    ''' <summary>创建空张量。</summary>
    Public Sub New()
        Me._Shape = New Integer() {0}
        Me._Data = New Half() {}
        Call UpdateDimProds()
    End Sub

    ''' <summary>
    ''' 创建指定形状的零张量。
    ''' </summary>
    ''' <param name="shape">各维度大小</param>
    Public Sub New(ParamArray shape As Integer())
        Me._Shape = CType(shape.Clone(), Integer())
        Dim totalSize = Tensor.ShapeSize(shape)
        Me._Data = New Half(totalSize - 1) {}
        Call UpdateDimProds()
    End Sub

    ''' <summary>
    ''' 用已有数据创建张量（数据会被复制）。
    ''' </summary>
    ''' <param name="data">源数据（行优先）</param>
    ''' <param name="shape">各维度大小</param>
    Public Sub New(data As Half(), ParamArray shape As Integer())
        Me._Shape = CType(shape.Clone(), Integer())
        Dim expectedSize = Tensor.ShapeSize(shape)

        If data.Length <> expectedSize Then
            Throw New ArgumentException(
                $"数据长度 {data.Length} 与形状 {String.Join("×", shape)} 所需的 {expectedSize} 不一致")
        End If

        Me._Data = DirectCast(data.Clone(), Half())
        Call UpdateDimProds()
    End Sub

#End Region

#Region "工厂方法"

    ''' <summary>创建指定形状的零张量。</summary>
    ''' <param name="shape">各维度大小</param>
    Public Shared Function Zeros(shape As Integer()) As TensorH
        Return New TensorH(shape)
    End Function

    ''' <summary>创建指定形状、元素全为 1 的张量。</summary>
    Public Shared Function Ones(shape As Integer()) As TensorH
        Dim t = New TensorH(shape)
        Array.Fill(t._Data, CType(1.0F, Half))
        Return t
    End Function

    ''' <summary>创建指定形状、元素全为给定值的张量。</summary>
    ''' <param name="value">填充值</param>
    Public Shared Function Filled(shape As Integer(), value As Half) As TensorH
        Dim t = New TensorH(shape)
        Array.Fill(t._Data, value)
        Return t
    End Function

    ''' <summary>创建只含一个元素的标量张量。</summary>
    ''' <param name="value">标量值</param>
    Public Shared Function Scalar(value As Half) As TensorH
        Dim t = New TensorH(New Integer() {1})
        t._Data(0) = value
        Return t
    End Function

    ''' <summary>
    ''' 零拷贝包装一个已有的半精度数组。底层数组由调用方持有，
    ''' 张量直接引用它（不复制），适用于把已有场缓冲暴露成张量的场景。
    ''' </summary>
    ''' <param name="data">底层数组（不被复制）</param>
    ''' <param name="shape">各维度大小</param>
    Public Shared Function Wrap(data As Half(), shape As Integer()) As TensorH
        Dim t As New TensorH()
        t._Data = data
        t._Shape = CType(shape.Clone(), Integer())
        Call t.UpdateDimProds()
        Return t
    End Function

#End Region

#Region "索引访问"

    ''' <summary>一维索引访问。</summary>
    ''' <param name="index">扁平下标</param>
    Default Public Property Item(index As Integer) As Half
        Get
            Return _Data(index)
        End Get
        Set
            _Data(index) = Value
        End Set
    End Property

    ''' <summary>二维索引访问 (row, col)。</summary>
    Default Public Property Item(row As Integer, col As Integer) As Half
        Get
            Return _Data(row * _Shape(1) + col)
        End Get
        Set
            _Data(row * _Shape(1) + col) = Value
        End Set
    End Property

    ''' <summary>
    ''' 三维索引访问 (row, col, depth)。
    ''' 注意：本索引器不含任何原子操作，可安全用于高频热循环。
    ''' </summary>
    Default Public Property Item(row As Integer, col As Integer, depth As Integer) As Half
        Get
            Return _Data(row * _Shape(1) * _Shape(2) + col * _Shape(2) + depth)
        End Get
        Set
            _Data(row * _Shape(1) * _Shape(2) + col * _Shape(2) + depth) = Value
        End Set
    End Property

    ''' <summary>任意维度的索引访问。</summary>
    Default Public Property Item(indexes As Integer()) As Half
        Get
            Return _Data(Get1DInd(indexes))
        End Get
        Set
            _Data(Get1DInd(indexes)) = Value
        End Set
    End Property

    ''' <summary>
    ''' 把多维下标换算为一维扁平下标。
    ''' </summary>
    Private Function Get1DInd(indexes As Integer()) As Integer
        Dim ind = 0

        For i = 0 To indexes.Length - 1
            ind += _DimProds(i) * indexes(i)
        Next

        If ind < 0 OrElse ind >= _Data.Length Then
            Throw New IndexOutOfRangeException(
                $"Index {ind} out of bounds for tensor with {_Data.Length} elements")
        End If

        Return ind
    End Function

    ''' <summary>重算各维度步长（形状变化后必须调用）。</summary>
    Private Sub UpdateDimProds()
        _DimProds = New Integer(_Shape.Length - 1) {}
        Dim prod = 1

        For i = _Shape.Length - 1 To 0 Step -1
            _DimProds(i) = prod
            prod *= _Shape(i)
        Next
    End Sub

#End Region

#Region "整体操作"

    ''' <summary>把所有元素清零。</summary>
    Public Sub Clear()
        Array.Clear(_Data, 0, _Data.Length)
    End Sub

    ''' <summary>用同一个值填满整个张量。</summary>
    ''' <param name="value">填充值</param>
    Public Sub Fill(value As Half)
        Array.Fill(_Data, value)
    End Sub

    ''' <summary>创建深拷贝。</summary>
    Public Function Clone() As Object Implements ICloneable.Clone
        Return New TensorH(CType(_Data.Clone(), Half()), CType(_Shape.Clone(), Integer()))
    End Function

    ''' <summary>
    ''' 创建深拷贝（强类型版本，避免调用方反复 CType）。
    ''' </summary>
    Public Function CloneT() As TensorH
        Return New TensorH(CType(_Data.Clone(), Half()), CType(_Shape.Clone(), Integer()))
    End Function

    ''' <summary>
    ''' 把另一个同形张量的数据复制到本张量。
    ''' </summary>
    Public Sub CopyFrom(other As TensorH)
        Array.Copy(other._Data, _Data, _Data.Length)
    End Sub

#End Region

#Region "与其它精度栈互转"

    ' 所有批量转换都直接复用 HalfSimd 的 8 路转换内核：
    '   * Half -> Single 是精确 widening；
    '   * Single -> Half / Double -> Half 走 BCL 直转路径（避免二次舍入）。

    ''' <summary>
    ''' 转换为双精度 <see cref="Tensor"/>（数据会被复制并逐元素拓宽）。
    ''' </summary>
    Public Function ToTensor() As Tensor
        Dim d(_Data.Length - 1) As Double

        For i = 0 To _Data.Length - 1
            d(i) = CDbl(_Data(i))
        Next

        Return New Tensor(d, CType(_Shape.Clone(), Integer()))
    End Function

    ''' <summary>
    ''' 转换为单精度 <see cref="TensorF"/>（批量走 <see cref="Microsoft.VisualBasic.Math.Numerics.HalfSimd.ToSingle"/>）。
    ''' </summary>
    Public Function ToTensorF() As TensorF
        Return New TensorF(halfSimd.ToSingle(_Data), CType(_Shape.Clone(), Integer()))
    End Function

    ''' <summary>从双精度 <see cref="Tensor"/> 转换而来（批量走 <see cref="Microsoft.VisualBasic.Math.Numerics.HalfSimd.ToHalf(Double())"/>）。</summary>
    Public Shared Function FromTensor(t As Tensor) As TensorH
        If t Is Nothing Then Return Nothing
        Return FromDoubles(t.Data, t.Shape)
    End Function

    ''' <summary>从单精度 <see cref="TensorF"/> 转换而来（批量走 <see cref="Microsoft.VisualBasic.Math.Numerics.HalfSimd.ToHalf(Single())"/>）。</summary>
    Public Shared Function FromTensorF(t As TensorF) As TensorH
        If t Is Nothing Then Return Nothing
        Return New TensorH(halfSimd.ToHalf(t.Data), CType(t.Shape.Clone(), Integer()))
    End Function

    ''' <summary>从双精度数组转换而来。</summary>
    Public Shared Function FromDoubles(data As Double(), shape As Integer()) As TensorH
        Return New TensorH(halfSimd.ToHalf(data), shape)
    End Function

    ''' <summary>导出为双精度数组的副本。</summary>
    Public Function ToDoubles() As Double()
        Dim d(_Data.Length - 1) As Double

        For i = 0 To _Data.Length - 1
            d(i) = CDbl(_Data(i))
        Next

        Return d
    End Function

#End Region

#Region "基本数学运算 - 运算符重载"

    ' 与双精度栈 Tensor 的运算符集合保持一致（单精度栈 TensorF 未提供运算符）；
    ' 语义边界（广播 / 形状校验）由本层负责，数值计算委托给 computeKernelH。

    ''' <summary>
    ''' 张量加法（逐元素，保留 (1,n)+(n,1) 的遗留广播语义）。
    ''' </summary>
    Public Shared Operator +(a As TensorH, b As TensorH) As TensorH
        If a.Rank = 2 AndAlso b.Rank = 2 AndAlso
            (a._Shape(0) = 1 AndAlso b._Shape(1) = 1 OrElse a._Shape(1) = 1 AndAlso b._Shape(0) = 1) Then
            Return BroadcastedAddition(a, b)
        End If

        If Not a.Shape.SequenceEqual(b.Shape) Then
            Throw New ArgumentException("张量形状必须相同才能相加")
        End If

        Return computeKernelH.Add(a, b)
    End Operator

    ''' <summary>张量加标量。</summary>
    Public Shared Operator +(t1 As TensorH, f As Half) As TensorH
        Return computeKernelH.AddScalar(t1, f)
    End Operator

    ''' <summary>张量减法（逐元素）。</summary>
    Public Shared Operator -(a As TensorH, b As TensorH) As TensorH
        If Not a.Shape.SequenceEqual(b.Shape) Then
            Throw New ArgumentException("张量形状必须相同才能相减")
        End If

        Return computeKernelH.Subtract(a, b)
    End Operator

    ''' <summary>张量减标量。</summary>
    Public Shared Operator -(t1 As TensorH, f As Half) As TensorH
        Return t1 + (-f)
    End Operator

    ''' <summary>张量与标量相乘。</summary>
    Public Shared Operator *(a As TensorH, scalar As Half) As TensorH
        Return computeKernelH.MultiplyScalar(a, scalar)
    End Operator

    ''' <summary>张量与标量相除。</summary>
    Public Shared Operator /(a As TensorH, scalar As Half) As TensorH
        Return computeKernelH.DivideScalar(a, scalar)
    End Operator

    ''' <summary>
    ''' 矩阵乘法运算符。两个二维张量进行矩阵乘法。
    ''' </summary>
    Public Shared Operator *(t1 As TensorH, t2 As TensorH) As TensorH
        If t1.Rank <> 2 OrElse t2.Rank <> 2 Then
            Throw New ArgumentException("矩阵乘法需要二维张量")
        End If

        If t1._Shape(1) <> t2._Shape(0) Then
            Throw New ArgumentException($"矩阵维度不匹配: {t1._Shape(1)} != {t2._Shape(0)}")
        End If

        Return computeKernelH.MatMul(t1, t2)
    End Operator

    ''' <summary>
    ''' 广播加法（来自旧版本的遗留语义）：行向量 (1,n) 与列向量 (n,1) 相加得到 (n,n) 外和矩阵。
    ''' </summary>
    Private Shared Function BroadcastedAddition(t1 As TensorH, t2 As TensorH) As TensorH
        Dim dim1 = t1._Data.Length
        Dim dim2 = t2._Data.Length
        Dim t As New TensorH(New Integer() {dim1, dim2})
        Dim ind As Integer() = New Integer() {0, 0}

        While ind(0) < dim1
            ind(1) = 0

            While ind(1) < dim2
                t(ind) = t1._Data(ind(0)) + t2._Data(ind(1))
                ind(1) += 1
            End While

            ind(0) += 1
        End While

        Return t
    End Function

#End Region

#Region "释放"

    ''' <summary>
    ''' 释放底层数组。注意：释放后 <see cref="Data"/> 变为 Nothing，
    ''' 仍处于使用中的场缓冲不应调用本方法。
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        If _disposed Then Return
        _disposed = True
        _Data = Nothing
    End Sub

#End Region

    ''' <summary>返回形状与元素总数的简要描述。</summary>
    Public Overrides Function ToString() As String
        If _disposed OrElse _Data Is Nothing Then Return "<disposed TensorH>"
        Return $"TensorH[{String.Join("×", _Shape)}] ({_Data.Length} halves)"
    End Function

End Class
