#Region "Microsoft.VisualBasic::60cf23a4bf3af201dd7fd667a7b6a967, Data_science\MachineLearning\TensorFlow\TensorF.vb"

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

    '   Total Lines: 379
    '    Code Lines: 200 (52.77%)
    ' Comment Lines: 113 (29.82%)
    '    - Xml Docs: 78.76%
    ' 
    '   Blank Lines: 66 (17.41%)
    '     File Size: 13.46 KB


    ' Class TensorF
    ' 
    '     Properties: computeKernelF, Data, IsDisposed, Length, Rank
    '                 Shape, TotalLength
    ' 
    '     Constructor: (+3 Overloads) Sub New
    ' 
    '     Function: Clone, CloneT, Filled, FromDoubles, FromTensor
    '               Get1DInd, Ones, Scalar, ToDoubles, ToString
    '               ToTensor, Wrap, Zeros
    ' 
    '     Sub: Clear, CopyFrom, Dispose, Fill, UpdateDimProds
    ' 
    ' /********************************************************************************/

#End Region

' ---------------------------------------------------------------------------
' TensorF —— 单精度 (Single / float32) 张量
'
' 与既有的 <see cref="Tensor"/>（Double 存储）并列存在，而不是替换它：
'   * Tensor  是深度学习 / 自动微分生态的依赖，改成 Single 会波及整个 sciBASIC#
'   * TensorF 面向科学计算与 CFD 这类"内存带宽受限 + 可视化无需双精度"的场景
'
' 相对 Double 的收益：
'   * 内存占用与内存带宽减半（CFD 的 Jacobi / stencil 全部是带宽受限算子）
'   * SIMD 通道数翻倍：Vector(Of Single) 在 AVX2 下 8 lane，Double 只有 4 lane
'   * 与 GPU / 可视化管线（float32 纹理）天然同构，避免导出时的精度转换
'
' 设计要点：
'   * 索引器**不含**任何版本计数 / 原子操作。Double 版 Tensor 的索引器 Setter 内含
'     Interlocked.Increment，在 128³ 网格上仅压力泊松迭代就是 6300 万次原子操作/步，
'     是 CFD 热循环的头号开销。TensorF 的索引器是纯数组读写。
'   * 通过 <see cref="computeKernelF"/> 持有可插拔后端，与 Double 栈的
'     Tensor.computeKernel 结构一致，后续可注册 GPU 后端（CudaTensorF）。
'
' 用法：
'     Dim f As TensorF = TensorF.Zeros({64, 64, 64})
'     f(1, 2, 3) = 1.5F
'     Dim raw As Single() = f.Data      ' 热循环直连底层数组
' ---------------------------------------------------------------------------

Imports System.Threading
Imports tfCompute = Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute

''' <summary>
''' 单精度（Single / float32）多维张量。与双精度 <see cref="Tensor"/> 并列，
''' 通过 <see cref="ToTensor"/> / <see cref="FromTensor"/> 双向互转。
''' </summary>
''' <remarks>
''' 索引器被刻意设计为"纯数组读写"，不包含版本计数或任何原子操作，
''' 以便 CFD 这类每秒上亿次随机访问的热循环可以直接使用。
''' </remarks>
Public Class TensorF : Implements ICloneable, IDisposable

    ''' <summary>底层数据数组（单精度）</summary>
    Private _Data As Single()

    ''' <summary>张量形状（各维度大小）</summary>
    Private _Shape As Integer()

    ''' <summary>各维度步长（用于多维索引 -> 一维下标）</summary>
    Private _DimProds As Integer()

    Private _disposed As Boolean

    ''' <summary>切换计算后端时使用的同步根对象</summary>
    Public Shared ReadOnly SyncRoot As New Object()

    ''' <summary>
    ''' 单精度张量当前生效的计算后端。默认是 SIMD 加速的 CPU 实现
    ''' （<see cref="tfCompute.SIMDTensorF.Default"/>）；后续可切换为 GPU 后端。
    ''' </summary>
    Public Shared Property computeKernelF As tfCompute.ITensorComputeF = tfCompute.SIMDTensorF.Default

#Region "基础属性"

    ''' <summary>
    ''' 存储张量数据的一维数组（行优先顺序）。热循环可直接使用以避免索引器开销。
    ''' </summary>
    Public ReadOnly Property Data As Single()
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

    ''' <summary>元素总数（与 <see cref="Length"/> 相同，为与 Double 版保持命名一致而提供）</summary>
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

#End Region

#Region "构造函数"

    ''' <summary>创建空张量。</summary>
    Public Sub New()
        Me._Shape = New Integer() {0}
        Me._Data = New Single() {}
        Call UpdateDimProds()
    End Sub

    ''' <summary>
    ''' 创建指定形状的零张量。
    ''' </summary>
    ''' <param name="shape">各维度大小</param>
    Public Sub New(ParamArray shape As Integer())
        Me._Shape = CType(shape.Clone(), Integer())
        Dim totalSize = shape.Aggregate(1, Function(a, b) a * b)
        Me._Data = New Single(totalSize - 1) {}
        Call UpdateDimProds()
    End Sub

    ''' <summary>
    ''' 用已有数据创建张量（数据会被复制）。
    ''' </summary>
    ''' <param name="data">源数据（行优先）</param>
    ''' <param name="shape">各维度大小</param>
    Public Sub New(data As Single(), ParamArray shape As Integer())
        Me._Shape = CType(shape.Clone(), Integer())
        Dim expectedSize = shape.Aggregate(1, Function(a, b) a * b)

        If data.Length <> expectedSize Then
            Throw New ArgumentException(
                $"数据长度 {data.Length} 与形状 {String.Join("×", shape)} 所需的 {expectedSize} 不一致")
        End If

        Me._Data = DirectCast(data.Clone(), Single())
        Call UpdateDimProds()
    End Sub

#End Region

#Region "工厂方法"

    ''' <summary>创建指定形状的零张量。</summary>
    ''' <param name="shape">各维度大小</param>
    Public Shared Function Zeros(shape As Integer()) As TensorF
        Return New TensorF(shape)
    End Function

    ''' <summary>创建指定形状、元素全为 1 的张量。</summary>
    ''' <param name="shape">各维度大小</param>
    Public Shared Function Ones(shape As Integer()) As TensorF
        Dim t = New TensorF(shape)
        Array.Fill(t._Data, 1.0F)
        Return t
    End Function

    ''' <summary>创建指定形状、元素全为给定值的张量。</summary>
    ''' <param name="shape">各维度大小</param>
    ''' <param name="value">填充值</param>
    Public Shared Function Filled(shape As Integer(), value As Single) As TensorF
        Dim t = New TensorF(shape)
        Array.Fill(t._Data, value)
        Return t
    End Function

    ''' <summary>创建只含一个元素的标量张量。</summary>
    ''' <param name="value">标量值</param>
    Public Shared Function Scalar(value As Single) As TensorF
        Dim t = New TensorF(New Integer() {1})
        t._Data(0) = value
        Return t
    End Function

    ''' <summary>
    ''' 零拷贝包装一个已有的单精度数组。底层数组由调用方持有，
    ''' 张量直接引用它（不复制），适用于把 CFD 场缓冲暴露成张量的场景。
    ''' </summary>
    ''' <param name="data">底层数组（不被复制）</param>
    ''' <param name="shape">各维度大小</param>
    Public Shared Function Wrap(data As Single(), shape As Integer()) As TensorF
        Dim t As New TensorF()
        t._Data = data
        t._Shape = CType(shape.Clone(), Integer())
        Call t.UpdateDimProds()
        Return t
    End Function

#End Region

#Region "索引访问"

    ''' <summary>一维索引访问。</summary>
    ''' <param name="index">扁平下标</param>
    Default Public Property Item(index As Integer) As Single
        Get
            Return _Data(index)
        End Get
        Set
            _Data(index) = Value
        End Set
    End Property

    ''' <summary>二维索引访问 (row, col)。</summary>
    ''' <param name="row">行下标</param>
    ''' <param name="col">列下标</param>
    Default Public Property Item(row As Integer, col As Integer) As Single
        Get
            Return _Data(row * _Shape(1) + col)
        End Get
        Set
            _Data(row * _Shape(1) + col) = Value
        End Set
    End Property

    ''' <summary>
    ''' 三维索引访问 (row, col, depth)，对应 CFD 的 (i, j, k)。
    ''' 注意：本索引器不含任何原子操作，可安全用于高频热循环。
    ''' </summary>
    ''' <param name="row">X / i 方向下标</param>
    ''' <param name="col">Y / j 方向下标</param>
    ''' <param name="depth">Z / k 方向下标</param>
    Default Public Property Item(row As Integer, col As Integer, depth As Integer) As Single
        Get
            Return _Data(row * _Shape(1) * _Shape(2) + col * _Shape(2) + depth)
        End Get
        Set
            _Data(row * _Shape(1) * _Shape(2) + col * _Shape(2) + depth) = Value
        End Set
    End Property

    ''' <summary>任意维度的索引访问。</summary>
    ''' <param name="indexes">各维度下标</param>
    Default Public Property Item(indexes As Integer()) As Single
        Get
            Return _Data(Get1DInd(indexes))
        End Get
        Set
            _Data(Get1DInd(indexes)) = Value
        End Set
    End Property

    ''' <summary>把多维下标换算为一维扁平下标。</summary>
    ''' <param name="indexes">各维度下标</param>
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
    Public Sub Fill(value As Single)
        Array.Fill(_Data, value)
    End Sub

    ''' <summary>创建深拷贝。</summary>
    Public Function Clone() As Object Implements ICloneable.Clone
        Return New TensorF(CType(_Data.Clone(), Single()), CType(_Shape.Clone(), Integer()))
    End Function

    ''' <summary>
    ''' 创建深拷贝（强类型版本，避免调用方反复 CType）。
    ''' </summary>
    Public Function CloneT() As TensorF
        Return New TensorF(CType(_Data.Clone(), Single()), CType(_Shape.Clone(), Integer()))
    End Function

    ''' <summary>把另一个同形张量的数据复制到本张量。</summary>
    ''' <param name="other">源张量</param>
    Public Sub CopyFrom(other As TensorF)
        Array.Copy(other._Data, _Data, _Data.Length)
    End Sub

#End Region

#Region "与双精度 Tensor 互转"

    ''' <summary>
    ''' 转换为双精度 <see cref="Tensor"/>（数据会被复制并逐元素拓宽）。
    ''' </summary>
    Public Function ToTensor() As Tensor
        Dim d(_Data.Length - 1) As Double

        For i = 0 To _Data.Length - 1
            d(i) = _Data(i)
        Next

        Return New Tensor(d, CType(_Shape.Clone(), Integer()))
    End Function

    ''' <summary>从双精度 <see cref="Tensor"/> 转换而来（数据会被复制并逐元素窄化）。</summary>
    ''' <param name="t">源张量</param>
    Public Shared Function FromTensor(t As Tensor) As TensorF
        If t Is Nothing Then Return Nothing
        Return FromDoubles(t.Data, t.Shape)
    End Function

    ''' <summary>从双精度数组转换而来。</summary>
    ''' <param name="data">源数据</param>
    ''' <param name="shape">各维度大小</param>
    Public Shared Function FromDoubles(data As Double(), shape As Integer()) As TensorF
        Dim s(data.Length - 1) As Single

        For i = 0 To data.Length - 1
            s(i) = CSng(data(i))
        Next

        Return New TensorF(s, shape)
    End Function

    ''' <summary>导出为双精度数组的副本。</summary>
    Public Function ToDoubles() As Double()
        Dim d(_Data.Length - 1) As Double

        For i = 0 To _Data.Length - 1
            d(i) = _Data(i)
        Next

        Return d
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
        If _disposed OrElse _Data Is Nothing Then Return "<disposed TensorF>"
        Return $"TensorF[{String.Join("×", _Shape)}] ({_Data.Length} singles)"
    End Function

End Class

