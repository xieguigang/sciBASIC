#Region "Microsoft.VisualBasic::69c51c5667af7c6c74f344e4842378ca, Data_science\MachineLearning\TensorFlow\Tensor.vb"

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

    '   Total Lines: 1014
    '    Code Lines: 575 (56.71%)
    ' Comment Lines: 282 (27.81%)
    '    - Xml Docs: 94.68%
    ' 
    '   Blank Lines: 157 (15.48%)
    '     File Size: 30.88 KB


    ' Class Tensor
    ' 
    '     Properties: Data, Dimensions, Gradient, IsVariable, Length
    '                 Rank, Shape, TotalLength
    ' 
    '     Constructor: (+4 Overloads) Sub New
    ' 
    '     Function: (+2 Overloads) Apply, BroadcastedAddition, Clone, DimsEqual, ElementwiseMultiply
    '               Filled, From2DArray, Get1DInd, GetColumn, GetFlatIndex
    '               GetIndices, GetRow, GetValue, HeInit, Identity
    '               L2Norm, MatMul, (+2 Overloads) Mean, MultAll, Ones
    '               Random, RandomNormal, Range, Reshape, Scalar
    '               Sum, To2DArray, To2DArrayDouble, ToArray, ToDoubleArray
    '               ToString, TotalSum, Transpose, Variable, XavierInit
    '               Zeros
    ' 
    '     Sub: Dispose, Finalize, Print, SetValue, UpdateDimProds
    ' 
    '     Operators: (+2 Overloads) -, (+2 Overloads) *, /, (+2 Overloads) +
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

''' <summary>
''' 张量类 - GNN中所有数值计算的基础数据结构
''' 支持多维数组的存储和基本数学运算
''' </summary>
Public Class Tensor : Implements ICloneable, IDisposable

#Region "私有字段"

    ''' <summary>
    ''' 底层数据数组
    ''' </summary>
    Private _Data As Double()

    ''' <summary>
    ''' 张量的形状（各维度大小）
    ''' </summary>
    Private _Shape As Integer()

    ''' <summary>
    ''' 维度乘积数组，用于快速计算多维索引
    ''' dimProds(i) = dims(0) * dims(1) * ... * dims(i-1)
    ''' </summary>
    Private _DimProds As Integer()

    ''' <summary>
    ''' 是否已释放资源
    ''' </summary>
    Private _disposed As Boolean = False

#End Region

#Region "属性"

    ''' <summary>
    ''' 张量的形状（各维度大小）
    ''' </summary>
    Public ReadOnly Property Shape As Integer()
        Get
            Return _Shape
        End Get
    End Property

    ''' <summary>
    ''' 底层数据数组
    ''' Underlying data array
    ''' </summary>
    ''' <remarks>
    ''' 存储张量数据的一维数组（行优先顺序）
    ''' </remarks>
    Public ReadOnly Property Data As Double()
        Get
            Return _Data
        End Get
    End Property

    ''' <summary>
    ''' 张量的维度数
    ''' </summary>
    Public ReadOnly Property Rank As Integer
        Get
            Return _Shape.Length
        End Get
    End Property

    ''' <summary>
    ''' 张量中元素的总数
    ''' </summary>
    Public ReadOnly Property Length As Integer
        Get
            Return _Data.Length
        End Get
    End Property

    ''' <summary>
    ''' 张量中元素的总数（兼容旧版本）
    ''' </summary>
    Public ReadOnly Property TotalLength As Integer
        Get
            Return _Data.Length
        End Get
    End Property

    ''' <summary>
    ''' 张量的维度数组（兼容旧版本）
    ''' </summary>
    Public ReadOnly Property Dimensions As Integer()
        Get
            Return _Shape
        End Get
    End Property

    ''' <summary>
    ''' 是否为变量（可训练）
    ''' Whether this is a variable (trainable)
    ''' </summary>
    Public Property IsVariable As Boolean = False

    ''' <summary>
    ''' 梯度（用于反向传播）
    ''' Gradient for backpropagation
    ''' </summary>
    Public Property Gradient As Tensor

#End Region

#Region "索引器"

    ''' <summary>
    ''' 获取或设置指定索引处的元素值（一维访问）
    ''' </summary>
    Default Public Property Item(index As Integer) As Double
        Get
            Return _Data(index)
        End Get
        Set
            _Data(index) = Value
        End Set
    End Property

    ''' <summary>
    ''' 获取或设置指定位置处的元素值（二维访问）
    ''' </summary>
    Default Public Property Item(row As Integer, col As Integer) As Double
        Get
            Return _Data(row * _Shape(1) + col)
        End Get
        Set
            _Data(row * _Shape(1) + col) = Value
        End Set
    End Property

    ''' <summary>
    ''' 获取或设置指定位置处的元素值（三维访问）
    ''' </summary>
    ''' <param name="row">第0维索引（通常对应行/高）</param>
    ''' <param name="col">第1维索引（通常对应列/宽）</param>
    ''' <param name="depth">第2维索引（通常对应深度/通道）</param>
    Default Public Property Item(row As Integer, col As Integer, depth As Integer) As Double
        ' 计算一维数组索引：
        ' row * Shape[1] * Shape[2]：跳过前 row 个平面，每个平面有 Shape[1]*Shape[2] 个元素
        ' col * Shape[2]：在当前平面中，跳过前 col 行，每行有 Shape[2] 个元素
        ' depth：当前行内的具体位置
        Get
            Return _Data(row * _Shape(1) * _Shape(2) + col * _Shape(2) + depth)
        End Get
        Set
            _Data(row * _Shape(1) * _Shape(2) + col * _Shape(2) + depth) = Value
        End Set
    End Property

    ''' <summary>
    ''' 获取或设置指定位置处的元素值（多维数组索引访问）
    ''' 支持任意维度的索引访问
    ''' </summary>
    Default Public Property Item(indexes As Integer()) As Double
        Get
            Dim ind = Get1DInd(indexes)
            Return _Data(ind)
        End Get
        Set
            Dim ind = Get1DInd(indexes)
            _Data(ind) = Value
        End Set
    End Property

#End Region

#Region "构造函数"

    ''' <summary>
    ''' 创建指定形状的张量，并用零初始化
    ''' </summary>
    ''' <param name="shape">张量的形状</param>
    Public Sub New(ParamArray shape As Integer())
        Me._Shape = CType(shape.Clone(), Integer())
        Dim totalSize = shape.Aggregate(1, Function(a, b) a * b)
        _Data = New Double(totalSize - 1) {}

        ' 初始化维度乘积数组
        Call UpdateDimProds()
    End Sub

    ''' <summary>
    ''' 使用指定数据创建张量
    ''' </summary>
    ''' <param name="data">初始数据</param>
    ''' <param name="shape">张量形状</param>
    Public Sub New(data As Double(), ParamArray shape As Integer())
        Me._Shape = CType(shape.Clone(), Integer())
        _Data = DirectCast(data.Clone(), Double())

        Dim expectedSize = shape.Aggregate(1, Function(a, b) a * b)
        If data.Length <> expectedSize Then
            Throw New ArgumentException($"Data length {data.Length} does not match shape {String.Join(",", shape)}")
        End If

        ' 初始化维度乘积数组
        Call UpdateDimProds()
    End Sub

    ''' <summary>
    ''' 使用指定数据创建张量（Single版本，兼容旧代码）
    ''' </summary>
    ''' <param name="data">初始数据</param>
    ''' <param name="shape">张量形状</param>
    Public Sub New(data As Single(), ParamArray shape As Integer())
        Me._Shape = CType(shape.Clone(), Integer())
        _Data = (From f As Single In data Select CDbl(f)).ToArray()

        Dim expectedSize = shape.Aggregate(1, Function(a, b) a * b)
        If data.Length <> expectedSize Then
            Throw New ArgumentException($"Data length {data.Length} does not match shape {String.Join(",", shape)}")
        End If

        ' 初始化维度乘积数组
        Call UpdateDimProds()
    End Sub

    ''' <summary>
    ''' 从二维数组创建张量
    ''' Create tensor from 2D array
    ''' </summary>
    Public Sub New(data As Double(,))
        Dim rows = data.GetLength(0)
        Dim cols = data.GetLength(1)
        _Shape = New Integer() {rows, cols}
        _Data = New Double(rows * cols - 1) {}

        For i = 0 To rows - 1
            For j = 0 To cols - 1
                _Data(i * cols + j) = data(i, j)
            Next
        Next

        ' 初始化维度乘积数组
        Call UpdateDimProds()
    End Sub

#End Region

#Region "私有辅助方法"

    ''' <summary>
    ''' 更新维度乘积数组
    ''' 用于快速计算多维索引到一维索引的转换
    ''' </summary>
    Private Sub UpdateDimProds()
        _DimProds = New Integer(_Shape.Length - 1) {}
        _DimProds(0) = 1

        For i = 1 To _Shape.Length - 1
            _DimProds(i) = _DimProds(i - 1) * _Shape(i - 1)
        Next
    End Sub

    ''' <summary>
    ''' 将多维索引转换为一维索引（来自旧版本）
    ''' </summary>
    ''' <param name="indexes">多维索引数组</param>
    ''' <returns>一维数组索引</returns>
    Private Function Get1DInd(ParamArray indexes As Integer()) As Integer
        If indexes.Length <> _Shape.Length Then
            Throw New ArgumentException($"Expected {_Shape.Length} indices, got {indexes.Length}")
        End If

        Dim ind = indexes(0)

        For i = 1 To indexes.Length - 1
            ind += _DimProds(i) * indexes(i)
        Next

        If ind < 0 OrElse ind >= _Data.Length Then
            Throw New IndexOutOfRangeException($"Index {ind} out of bounds for tensor with {_Data.Length} elements")
        End If

        Return ind
    End Function

    ''' <summary>
    ''' 计算数组所有元素的乘积（来自旧版本）
    ''' </summary>
    ''' <param name="array">整数数组</param>
    ''' <returns>所有元素的乘积</returns>
    Private Shared Function MultAll(array As Integer()) As Integer
        Dim mul = 1

        For i = 0 To array.Length - 1
            mul *= array(i)
        Next

        Return mul
    End Function

    ''' <summary>
    ''' 比较两个张量的维度是否相等（来自旧版本）
    ''' </summary>
    ''' <param name="t1">第一个张量</param>
    ''' <param name="t2">第二个张量</param>
    ''' <returns>如果维度相等返回True，否则返回False</returns>
    Private Shared Function DimsEqual(t1 As Tensor, t2 As Tensor) As Boolean
        If t1._Shape.Length <> t2._Shape.Length Then
            Return False
        End If

        For i = 0 To t1._Shape.Length - 1
            If t1._Shape(i) <> t2._Shape(i) Then
                Return False
            End If
        Next

        Return True
    End Function

    ''' <summary>
    ''' 广播加法（来自旧版本）
    ''' 当两个向量进行加法时，生成一个矩阵，其中每个元素是两个向量对应元素的和
    ''' </summary>
    ''' <param name="t1">第一个张量（行向量）</param>
    ''' <param name="t2">第二个张量（列向量）</param>
    ''' <returns>广播加法结果矩阵</returns>
    Private Shared Function BroadcastedAddition(t1 As Tensor, t2 As Tensor) As Tensor
        Dim dim1 = t1._Data.Length
        Dim dim2 = t2._Data.Length
        Dim t As New Tensor(New Integer() {dim1, dim2})
        Dim ind = New Integer() {0, 0}

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

#Region "静态工厂方法"

    ''' <summary>
    ''' 从二维数组创建张量
    ''' </summary>
    Public Shared Function From2DArray(array As Single(,)) As Tensor
        Dim rows = array.GetLength(0)
        Dim cols = array.GetLength(1)
        Dim result = New Tensor(rows, cols)
        For i = 0 To rows - 1
            For j = 0 To cols - 1
                result(i, j) = array(i, j)
            Next
        Next
        Return result
    End Function

    ''' <summary>
    ''' 创建填充指定值的张量
    ''' </summary>
    Public Shared Function Filled(shape As Integer(), value As Single) As Tensor
        Dim tensor = New Tensor(shape)
        Array.Fill(tensor._Data, value)
        Return tensor
    End Function

    ''' <summary>
    ''' 创建单位矩阵
    ''' </summary>
    Public Shared Function Identity(size As Integer) As Tensor
        Dim result = New Tensor(size, size)
        For i = 0 To size - 1
            result(i, i) = 1.0F
        Next
        Return result
    End Function

    ''' <summary>
    ''' 创建随机张量（均匀分布）
    ''' </summary>
    ''' <remarks>
    ''' RandomUniform
    ''' </remarks>
    Public Shared Function Random(shape As Integer(), Optional min As Single = -1.0F, Optional max As Single = 1.0F, Optional seed As Integer? = Nothing) As Tensor
        Dim lRandom = If(seed.HasValue, New Random(seed.Value), New Random())
        Dim tensor = New Tensor(shape)
        For i = 0 To tensor.Length - 1
            tensor._Data(i) = CSng((lRandom.NextDouble() * (max - min) + min))
        Next
        Return tensor
    End Function

    ''' <summary>
    ''' 创建随机张量（正态分布，Xavier初始化）
    ''' 用于神经网络权重初始化，有助于梯度流动
    ''' </summary>
    Public Shared Function RandomNormal(shape As Integer(), Optional mean As Single = 0.0F, Optional stdDev As Single = 1.0F, Optional seed As Integer? = Nothing) As Tensor
        Dim random = If(seed.HasValue, New Random(seed.Value), New Random())
        Dim tensor = New Tensor(shape)
        For i = 0 To tensor.Length - 1
            ' Box-Muller变换生成正态分布随机数
            Dim u1 As Double = 1.0 - random.NextDouble()
            Dim u2 As Double = 1.0 - random.NextDouble()
            Dim randStdNormal = std.Sqrt(-2.0 * std.Log(u1)) * std.Sin(2.0 * std.PI * u2)
            tensor._Data(i) = CSng(mean + stdDev * randStdNormal)
        Next
        Return tensor
    End Function

    ''' <summary>
    ''' 创建整数范围张量
    ''' Create integer range tensor
    ''' </summary>
    Public Shared Function Range(start As Integer, [end] As Integer, Optional [step] As Integer = 1) As Tensor
        Dim count As Integer = ([end] - start + [step] - 1) / [step]
        Dim tensor = New Tensor(New Integer() {count})

        For i = 0 To count - 1
            tensor._Data(i) = start + i * [step]
        Next

        Return tensor
    End Function

    ''' <summary>
    ''' Xavier初始化 - 适用于sigmoid/tanh激活函数
    ''' </summary>
    Public Shared Function XavierInit(fanIn As Integer, fanOut As Integer, Optional seed As Integer? = Nothing) As Tensor
        Dim stdDev As Single = std.Sqrt(2.0 / (fanIn + fanOut))
        Return RandomNormal(New Integer() {fanIn, fanOut}, 0.0F, stdDev, seed)
    End Function

    ''' <summary>
    ''' He初始化 - 适用于ReLU激活函数
    ''' </summary>
    Public Shared Function HeInit(fanIn As Integer, fanOut As Integer, Optional seed As Integer? = Nothing) As Tensor
        Dim stdDev As Single = std.Sqrt(2.0 / fanIn)
        Return RandomNormal(New Integer() {fanIn, fanOut}, 0.0F, stdDev, seed)
    End Function

    ''' <summary>
    ''' 创建标量张量
    ''' Create scalar tensor
    ''' </summary>
    Public Shared Function Scalar(value As Double) As Tensor
        Dim tensor = New Tensor(New Integer() {1})
        tensor._Data(0) = value
        Return tensor
    End Function

    ''' <summary>
    ''' 创建全零张量
    ''' Create zero tensor
    ''' </summary>
    Public Shared Function Zeros(shape As Integer()) As Tensor
        Return New Tensor(shape)
    End Function

    ''' <summary>
    ''' 创建全一张量
    ''' Create ones tensor
    ''' </summary>
    Public Shared Function Ones(shape As Integer()) As Tensor
        Dim tensor = New Tensor(shape)
        Array.Fill(tensor._Data, 1.0)
        Return tensor
    End Function

#End Region

#Region "张量操作方法"

    ''' <summary>
    ''' 克隆张量
    ''' Clone tensor
    ''' </summary>
    ''' <remarks>
    ''' <see cref="Tensor"/>
    ''' </remarks>
    Public Function Clone() As Object Implements ICloneable.Clone
        Return New Tensor(CType(_Data.Clone(), Double()), CType(_Shape.Clone(), Integer()))
    End Function

    ''' <summary>
    ''' 重塑张量形状（来自旧版本）
    ''' 改变张量的维度结构，但不改变数据
    ''' </summary>
    ''' <param name="newDims">新的维度数组</param>
    ''' <returns>如果重塑成功返回True，否则返回False</returns>
    Public Function Reshape(newDims As Integer()) As Boolean
        If MultAll(_Shape) <> MultAll(newDims) Then
            Return False
        Else
            _Shape = CType(newDims.Clone(), Integer())
        End If

        Call UpdateDimProds()

        Return True
    End Function

    ''' <summary>
    ''' 获取指定索引的值
    ''' Get value at specified indices
    ''' </summary>
    Public Function GetValue(ParamArray indices As Integer()) As Double
        Dim flatIndex = GetFlatIndex(indices)
        Return _Data(flatIndex)
    End Function

    ''' <summary>
    ''' 设置指定索引的值
    ''' Set value at specified indices
    ''' </summary>
    Public Sub SetValue(value As Double, ParamArray indices As Integer())
        Dim flatIndex = GetFlatIndex(indices)
        _Data(flatIndex) = value
    End Sub

    ''' <summary>
    ''' 创建变量张量
    ''' Create variable tensor
    ''' </summary>
    Public Shared Function Variable(value As Tensor) As Tensor
        Dim lVariable = CType(value.Clone(), Tensor)
        lVariable.IsVariable = True
        Return lVariable
    End Function

    ''' <summary>
    ''' 将多维索引转换为扁平索引
    ''' Convert multi-dimensional indices to flat index
    ''' </summary>
    Public Function GetFlatIndex(ParamArray indices As Integer()) As Integer
        If indices.Length <> Rank Then
            Throw New ArgumentException($"Expected {Rank} indices, got {indices.Length}")
        End If

        Dim flatIndex = 0
        Dim multiplier = 1

        For i = Rank - 1 To 0 Step -1
            If indices(i) < 0 OrElse indices(i) >= _Shape(i) Then
                Throw New IndexOutOfRangeException($"Index {indices(i)} out of bounds for dimension {i} with size {_Shape(i)}")
            End If
            flatIndex += indices(i) * multiplier
            multiplier *= _Shape(i)
        Next

        Return flatIndex
    End Function

    ''' <summary>
    ''' 将扁平索引转换为多维索引
    ''' Convert flat index to multi-dimensional indices
    ''' </summary>
    Public Function GetIndices(flatIndex As Integer) As Integer()
        Dim indices = New Integer(Rank - 1) {}
        Dim remaining = flatIndex

        For i = Rank - 1 To 0 Step -1
            indices(i) = remaining Mod _Shape(i)
            remaining = CInt(std.Floor(remaining / _Shape(i)))
        Next

        Return indices
    End Function

#End Region

#Region "基本数学运算 - 运算符重载"

    ''' <summary>
    ''' 张量加法（逐元素）
    ''' </summary>
    Public Shared Operator +(a As Tensor, b As Tensor) As Tensor
        ' 检查是否可以进行广播加法（来自旧版本）
        If a.Rank = 2 AndAlso b.Rank = 2 AndAlso
            (a._Shape(0) = 1 AndAlso b._Shape(1) = 1 OrElse a._Shape(1) = 1 AndAlso b._Shape(0) = 1) Then
            Return BroadcastedAddition(a, b)
        End If

        ' 普通逐元素加法
        If Not a.Shape.SequenceEqual(b.Shape) Then
            Throw New ArgumentException("张量形状必须相同才能相加")
        End If

        Dim result = New Tensor(a.Shape)
        For i = 0 To a.Length - 1
            result._Data(i) = a._Data(i) + b._Data(i)
        Next
        Return result
    End Operator

    ''' <summary>
    ''' 张量加标量（来自旧版本）
    ''' </summary>
    Public Shared Operator +(t1 As Tensor, f As Single) As Tensor
        Dim t As New Tensor(t1._Shape)

        For i = 0 To t.Length - 1
            t._Data(i) = t1._Data(i) + f
        Next

        Return t
    End Operator

    ''' <summary>
    ''' 张量减法（逐元素）
    ''' </summary>
    Public Shared Operator -(a As Tensor, b As Tensor) As Tensor
        If Not a.Shape.SequenceEqual(b.Shape) Then
            Throw New ArgumentException("张量形状必须相同才能相减")
        End If

        Dim result = New Tensor(a.Shape)
        For i = 0 To a.Length - 1
            result._Data(i) = a._Data(i) - b._Data(i)
        Next
        Return result
    End Operator

    ''' <summary>
    ''' 张量减标量（来自旧版本）
    ''' </summary>
    Public Shared Operator -(t1 As Tensor, f As Single) As Tensor
        Return t1 + (-f)
    End Operator

    ''' <summary>
    ''' 张量与标量相乘
    ''' </summary>
    Public Shared Operator *(a As Tensor, scalar As Single) As Tensor
        Dim result = New Tensor(a.Shape)
        For i = 0 To a.Length - 1
            result._Data(i) = a._Data(i) * scalar
        Next
        Return result
    End Operator

    ''' <summary>
    ''' 张量与标量相除
    ''' </summary>
    Public Shared Operator /(a As Tensor, scalar As Single) As Tensor
        Dim result = New Tensor(a.Shape)
        For i = 0 To a.Length - 1
            result._Data(i) = a._Data(i) / scalar
        Next
        Return result
    End Operator

    ''' <summary>
    ''' 矩阵乘法运算符（来自旧版本）
    ''' 两个二维张量进行矩阵乘法
    ''' </summary>
    Public Shared Operator *(t1 As Tensor, t2 As Tensor) As Tensor
        ' 检查是否为二维张量
        If t1.Rank <> 2 OrElse t2.Rank <> 2 Then
            Throw New ArgumentException("矩阵乘法需要二维张量")
        End If

        ' 检查维度是否匹配
        If t1._Shape(1) <> t2._Shape(0) Then
            Throw New ArgumentException($"矩阵维度不匹配: {t1._Shape(1)} != {t2._Shape(0)}")
        End If

        Dim t As New Tensor(New Integer() {t1._Shape(0), t2._Shape(1)})
        Dim sum As Double
        Dim ind1 = New Integer() {0, 0}
        Dim ind2 = New Integer() {0, 0}
        Dim ind3 = New Integer() {0, 0}

        For i = 0 To t1._Shape(0) - 1
            ind1(0) = i
            ind3(0) = i

            For k = 0 To t2._Shape(1) - 1
                ind2(1) = k
                ind3(1) = k
                sum = 0

                For j = 0 To t1._Shape(1) - 1
                    ind1(1) = j
                    ind2(0) = j
                    sum += t1(ind1) * t2(ind2)
                Next

                t(ind3) = sum
            Next
        Next

        Return t
    End Operator

#End Region

#Region "矩阵运算方法"

    ''' <summary>
    ''' 逐元素乘法（Hadamard积）
    ''' </summary>
    Public Function ElementwiseMultiply(other As Tensor) As Tensor
        If Not Shape.SequenceEqual(other.Shape) Then
            Throw New ArgumentException("张量形状必须相同")
        End If

        Dim result = New Tensor(Shape)
        For i = 0 To Length - 1
            result._Data(i) = _Data(i) * other._Data(i)
        Next
        Return result
    End Function

    ''' <summary>
    ''' 矩阵乘法（二维张量）
    ''' 这是GNN消息传递的核心运算
    ''' </summary>
    Public Function MatMul(other As Tensor) As Tensor
        If Rank <> 2 OrElse other.Rank <> 2 Then
            Throw New ArgumentException("矩阵乘法需要二维张量")
        End If

        If Shape(1) <> other.Shape(0) Then
            Throw New ArgumentException($"矩阵维度不匹配: {Shape(1)} != {other.Shape(0)}")
        End If

        Dim m = Shape(0)
        Dim n = other.Shape(1)
        Dim k = Shape(1)

        Dim result = New Tensor(m, n)

        For i = 0 To m - 1
            For j = 0 To n - 1
                Dim sum = 0.0F
                For p = 0 To k - 1
                    sum += Me(i, p) * other(p, j)
                Next
                result(i, j) = sum
            Next
        Next

        Return result
    End Function

    ''' <summary>
    ''' 转置
    ''' </summary>
    Public Function Transpose() As Tensor
        If Rank <> 2 Then
            Throw New ArgumentException("只支持二维张量转置")
        End If

        Dim result = New Tensor(Shape(1), Shape(0))
        For i = 0 To Shape(0) - 1
            For j = 0 To Shape(1) - 1
                result(j, i) = Me(i, j)
            Next
        Next
        Return result
    End Function

#End Region

#Region "聚合操作"

    ''' <summary>
    ''' 沿指定轴求和
    ''' </summary>
    Public Function Sum(axis As Integer) As Tensor
        If Rank <> 2 Then
            Throw New ArgumentException("当前只支持二维张量的轴求和")
        End If

        If axis = 0 Then
            ' 沿行方向求和，结果是一行
            Dim result = New Tensor(1, Shape(1))
            For j = 0 To Shape(1) - 1
                Dim lSum As Single = 0
                For i = 0 To Shape(0) - 1
                    lSum += Me(i, j)
                Next
                result(0, j) = lSum
            Next
            Return result
        ElseIf axis = 1 Then
            ' 沿列方向求和，结果是一列
            Dim result = New Tensor(Shape(0), 1)
            For i = 0 To Shape(0) - 1
                Dim lSum As Single = 0
                For j = 0 To Shape(1) - 1
                    lSum += Me(i, j)
                Next
                result(i, 0) = lSum
            Next
            Return result
        End If

        Throw New ArgumentException("轴参数必须是0或1")
    End Function

    ''' <summary>
    ''' 计算所有元素的和
    ''' </summary>
    Public Function TotalSum() As Single
        Dim sum As Single = 0
        For i = 0 To Length - 1
            sum += CSng(_Data(i))
        Next
        Return sum
    End Function

    ''' <summary>
    ''' 计算所有元素的平均值
    ''' </summary>
    Public Function Mean() As Single
        Return TotalSum() / Length
    End Function

    ''' <summary>
    ''' 沿指定轴计算平均值
    ''' </summary>
    Public Function Mean(axis As Integer) As Tensor
        Dim sumResult = Sum(axis)
        Dim count = If(axis = 0, Shape(0), Shape(1))
        Return sumResult / count
    End Function

    ''' <summary>
    ''' 计算L2范数（欧几里得范数）
    ''' </summary>
    Public Function L2Norm() As Single
        Dim sumSquares As Single = 0
        For i = 0 To Length - 1
            sumSquares += CSng(_Data(i) * _Data(i))
        Next
        Return std.Sqrt(sumSquares)
    End Function

#End Region

#Region "逐元素函数"

    ''' <summary>
    ''' 对每个元素应用函数
    ''' </summary>
    Public Function Apply(func As Func(Of Single, Single)) As Tensor
        Dim result = New Tensor(Shape)
        For i = 0 To Length - 1
            result._Data(i) = func(CSng(_Data(i)))
        Next
        Return result
    End Function

    ''' <summary>
    ''' 对每个元素应用函数（Double版本）
    ''' </summary>
    Public Function Apply(func As Func(Of Double, Double)) As Tensor
        Dim result = New Tensor(Shape)
        For i = 0 To Length - 1
            result._Data(i) = func(_Data(i))
        Next
        Return result
    End Function

#End Region

#Region "数据转换方法"

    ''' <summary>
    ''' 获取原始数据数组的副本（Single版本）
    ''' </summary>
    Public Function ToArray() As Single()
        Return (From d In _Data Select CSng(d)).ToArray()
    End Function

    ''' <summary>
    ''' 获取原始数据数组的副本（Double版本）
    ''' </summary>
    Public Function ToDoubleArray() As Double()
        Return CType(_Data.Clone(), Double())
    End Function

    ''' <summary>
    ''' 转换为二维数组
    ''' </summary>
    Public Function To2DArray() As Single(,)
        If Rank <> 2 Then
            Throw New InvalidOperationException("只能将二维张量转换为二维数组")
        End If

        Dim result = New Single(Shape(0) - 1, Shape(1) - 1) {}
        For i = 0 To Shape(0) - 1
            For j = 0 To Shape(1) - 1
                result(i, j) = CSng(Me(i, j))
            Next
        Next
        Return result
    End Function

    ''' <summary>
    ''' 转换为二维数组（Double版本）
    ''' </summary>
    Public Function To2DArrayDouble() As Double(,)
        If Rank <> 2 Then
            Throw New InvalidOperationException("只能将二维张量转换为二维数组")
        End If

        Dim result = New Double(Shape(0) - 1, Shape(1) - 1) {}
        For i = 0 To Shape(0) - 1
            For j = 0 To Shape(1) - 1
                result(i, j) = Me(i, j)
            Next
        Next
        Return result
    End Function

#End Region

#Region "行列操作"

    ''' <summary>
    ''' 获取指定行
    ''' </summary>
    Public Function GetRow(rowIndex As Integer) As Tensor
        Dim result = New Tensor(1, Shape(1))
        For j = 0 To Shape(1) - 1
            result(0, j) = Me(rowIndex, j)
        Next
        Return result
    End Function

    ''' <summary>
    ''' 获取指定列
    ''' </summary>
    Public Function GetColumn(colIndex As Integer) As Tensor
        Dim result = New Tensor(Shape(0), 1)
        For i = 0 To Shape(0) - 1
            result(i, 0) = Me(i, colIndex)
        Next
        Return result
    End Function

#End Region

#Region "调试和显示"

    ''' <summary>
    ''' 打印张量内容（用于调试）
    ''' </summary>
    Public Sub Print(Optional name As String = Nothing)
        If Not Equals(name, Nothing) Then
            Console.WriteLine($"{name}:")
        End If

        If Rank = 1 Then
            Console.WriteLine($"[{String.Join(", ", _Data.Select(Function(v) v.ToString("F4")))}]")
        ElseIf Rank = 2 Then
            Console.WriteLine("[")
            For i As Integer = 0 To Shape(0) - 1
                Dim idx As Integer = i
                Dim row = Enumerable.Range(0, Shape(1)).Select(Function(j) Me(idx, j).ToString("F4"))
                Console.WriteLine($"  [{String.Join(", ", row)}]")
            Next
            Console.WriteLine("]")
        Else
            Console.WriteLine($"Tensor with shape [{String.Join(", ", Shape)}]")
        End If
    End Sub

    ''' <summary>
    ''' 转换为字符串表示
    ''' Convert to string representation
    ''' </summary>
    Public Overrides Function ToString() As String
        Return $"Tensor(shape=[{String.Join(",", _Shape)}])"
    End Function

#End Region

#Region "IDisposable 实现"

    ''' <summary>
    ''' 释放资源
    ''' Dispose resources
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        If Not _disposed Then
            Erase _Data
            Erase _Shape
            Erase _DimProds

            If Gradient IsNot Nothing Then
                Call Gradient.Dispose()
            End If

            _disposed = True
        End If
    End Sub

    ''' <summary>
    ''' 析构函数
    ''' </summary>
    Protected Overrides Sub Finalize()
        Dispose()
    End Sub

#End Region

End Class
