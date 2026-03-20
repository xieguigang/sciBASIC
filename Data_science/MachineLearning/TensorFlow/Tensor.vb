Imports std = System.Math

''' <summary>
''' 张量类 - GNN中所有数值计算的基础数据结构
''' 支持多维数组的存储和基本数学运算
''' </summary>
Public Class Tensor : Implements ICloneable, IDisposable

    ''' <summary>
    ''' 张量的形状（各维度大小）
    ''' </summary>
    Public ReadOnly Property Shape As Integer()

    ''' <summary>
    ''' 底层数据数组
    ''' Underlying data array
    ''' </summary>
    ''' <remarks>
    ''' 存储张量数据的一维数组（行优先顺序）
    ''' </remarks>
    Public ReadOnly Property Data As Double()

    ''' <summary>
    ''' 张量的维度数
    ''' </summary>
    Public ReadOnly Property Rank As Integer
        Get
            Return Shape.Length
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
            Return _Data(row * Shape(1) + col)
        End Get
        Set
            _Data(row * Shape(1) + col) = Value
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
            Return _Data(row * Shape(1) * Shape(2) + col * Shape(2) + depth)
        End Get
        Set
            _Data(row * Shape(1) * Shape(2) + col * Shape(2) + depth) = Value
        End Set
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

    ''' <summary>
    ''' 创建指定形状的张量，并用零初始化
    ''' </summary>
    ''' <param name="shape">张量的形状</param>
    Public Sub New(ParamArray shape As Integer())
        Me.Shape = CType(shape.Clone(), Integer())
        Dim totalSize = shape.Aggregate(1, Function(a, b) a * b)
        _Data = New Double(totalSize - 1) {}
    End Sub

    ''' <summary>
    ''' 使用指定数据创建张量
    ''' </summary>
    ''' <param name="data">初始数据</param>
    ''' <param name="shape">张量形状</param>
    Public Sub New(data As Double(), ParamArray shape As Integer())
        Me.Shape = CType(shape.Clone(), Integer())
        _Data = DirectCast(data.Clone(), Double())

        Dim expectedSize = shape.Aggregate(1, Function(a, b) a * b)
        If data.Length <> expectedSize Then
            Throw New ArgumentException($"Data length {data.Length} does not match shape {String.Join(",", shape)}")
        End If
    End Sub

    ''' <summary>
    ''' 使用指定数据创建张量
    ''' </summary>
    ''' <param name="data">初始数据</param>
    ''' <param name="shape">张量形状</param>
    Public Sub New(data As Single(), ParamArray shape As Integer())
        Me.Shape = CType(shape.Clone(), Integer())
        _Data = (From f As Single In data Select CDbl(f)).ToArray

        Dim expectedSize = shape.Aggregate(1, Function(a, b) a * b)
        If data.Length <> expectedSize Then
            Throw New ArgumentException($"Data length {data.Length} does not match shape {String.Join(",", shape)}")
        End If
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
    End Sub

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
            remaining /= _Shape(i)
        Next

        Return indices
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

#Region "基本数学运算"

    ''' <summary>
    ''' 张量加法（逐元素）
    ''' </summary>
    Public Shared Operator +(a As Tensor, b As Tensor) As Tensor
        If Not a.Shape.SequenceEqual(b.Shape) Then Throw New ArgumentException("张量形状必须相同才能相加")

        Dim result = New Tensor(a.Shape)
        For i = 0 To a.Length - 1
            result._Data(i) = a._Data(i) + b._Data(i)
        Next
        Return result
    End Operator

    ''' <summary>
    ''' 张量减法（逐元素）
    ''' </summary>
    Public Shared Operator -(a As Tensor, b As Tensor) As Tensor
        If Not a.Shape.SequenceEqual(b.Shape) Then Throw New ArgumentException("张量形状必须相同才能相减")

        Dim result = New Tensor(a.Shape)
        For i = 0 To a.Length - 1
            result._Data(i) = a._Data(i) - b._Data(i)
        Next
        Return result
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
    ''' 逐元素乘法（Hadamard积）
    ''' </summary>
    Public Function ElementwiseMultiply(other As Tensor) As Tensor
        If Not Shape.SequenceEqual(other.Shape) Then Throw New ArgumentException("张量形状必须相同")

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
        If Rank <> 2 OrElse other.Rank <> 2 Then Throw New ArgumentException("矩阵乘法需要二维张量")

        If Shape(1) <> other.Shape(0) Then Throw New ArgumentException($"矩阵维度不匹配: {Shape(1)} != {other.Shape(0)}")

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
        If Rank <> 2 Then Throw New ArgumentException("只支持二维张量转置")

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
        If Rank <> 2 Then Throw New ArgumentException("当前只支持二维张量的轴求和")

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
            sum += _Data(i)
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
            sumSquares += _Data(i) * _Data(i)
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
            result._Data(i) = func(_Data(i))
        Next
        Return result
    End Function
#End Region

    ''' <summary>
    ''' 获取原始数据数组的副本
    ''' </summary>
    Public Function ToArray() As Single()
        Return CType(_Data.Clone(), Single())
    End Function

    ''' <summary>
    ''' 转换为二维数组
    ''' </summary>
    Public Function To2DArray() As Single(,)
        If Rank <> 2 Then Throw New InvalidOperationException("只能将二维张量转换为二维数组")

        Dim result = New Single(Shape(0) - 1, Shape(1) - 1) {}
        For i = 0 To Shape(0) - 1
            For j = 0 To Shape(1) - 1
                result(i, j) = Me(i, j)
            Next
        Next
        Return result
    End Function

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

    ''' <summary>
    ''' 打印张量内容（用于调试）
    ''' </summary>
    Public Sub Print(Optional name As String = Nothing)
        If Not Equals(name, Nothing) Then Console.WriteLine($"{name}:")

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

    ''' <summary>
    ''' 转换为字符串表示
    ''' Convert to string representation
    ''' </summary>
    Public Overrides Function ToString() As String
        Return $"Tensor(shape=[{String.Join(",", _Shape)}])"
    End Function

    Private _disposed As Boolean = False

    ''' <summary>
    ''' 释放资源
    ''' Dispose resources
    ''' </summary>
    Public Sub Dispose() Implements IDisposable.Dispose
        If Not _disposed Then
            Erase _Data
            Erase _Shape

            If Gradient IsNot Nothing Then
                Call Gradient.Dispose()
            End If

            _disposed = True
        End If
    End Sub
End Class

