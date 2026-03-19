Imports System
Imports System.Linq

Namespace GNNSharp

    ''' <summary>
    ''' 张量类 - GNN中所有数值计算的基础数据结构
    ''' 支持多维数组的存储和基本数学运算
    ''' </summary>
    Public Class Tensor
        ''' <summary>
        ''' 存储张量数据的一维数组（行优先顺序）
        ''' </summary>
        Private ReadOnly _data As Single()

        ''' <summary>
        ''' 张量的形状（各维度大小）
        ''' </summary>
        Public ReadOnly Property Shape As Integer()

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
                Return _data.Length
            End Get
        End Property

        ''' <summary>
        ''' 获取或设置指定索引处的元素值（一维访问）
        ''' </summary>
        Default Public Property Item(index As Integer) As Single
            Get
                Return _data(index)
            End Get
            Set(value As Single)
                _data(index) = value
            End Set
        End Property

        ''' <summary>
        ''' 获取或设置指定位置处的元素值（二维访问）
        ''' </summary>
        Default Public Property Item(row As Integer, col As Integer) As Single
            Get
                Return _data(row * Shape(1) + col)
            End Get
            Set(value As Single)
                _data(row * Shape(1) + col) = value
            End Set
        End Property

        ''' <summary>
        ''' 获取或设置指定位置处的元素值（三维访问）
        ''' </summary>
        ''' <paramname="row">第0维索引（通常对应行/高）</param>
        ''' <paramname="col">第1维索引（通常对应列/宽）</param>
        ''' <paramname="depth">第2维索引（通常对应深度/通道）</param>
        Default Public Property Item(row As Integer, col As Integer, depth As Integer) As Single
            ' 计算一维数组索引：
            ' row * Shape[1] * Shape[2]：跳过前 row 个平面，每个平面有 Shape[1]*Shape[2] 个元素
            ' col * Shape[2]：在当前平面中，跳过前 col 行，每行有 Shape[2] 个元素
            ' depth：当前行内的具体位置
            Get
                Return _data(row * Shape(1) * Shape(2) + col * Shape(2) + depth)
            End Get
            Set(value As Single)
                _data(row * Shape(1) * Shape(2) + col * Shape(2) + depth) = value
            End Set
        End Property

        ''' <summary>
        ''' 创建指定形状的张量，并用零初始化
        ''' </summary>
        ''' <paramname="shape">张量的形状</param>
        Public Sub New(ParamArray shape As Integer())
            Me.Shape = CType(shape.Clone(), Integer())
            Dim totalSize = shape.Aggregate(1, Function(a, b) a * b)
            _data = New Single(totalSize - 1) {}
        End Sub

        ''' <summary>
        ''' 使用指定数据创建张量
        ''' </summary>
        ''' <paramname="data">初始数据</param>
        ''' <paramname="shape">张量形状</param>
        Public Sub New(data As Single(), ParamArray shape As Integer())
            Me.Shape = CType(shape.Clone(), Integer())
            _data = CType(data.Clone(), Single())
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
        ''' 创建填充指定值的张量
        ''' </summary>
        Public Shared Function Filled(shape As Integer(), value As Single) As Tensor
            Dim tensor = New Tensor(shape)
            Array.Fill(tensor._data, value)
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
        Public Shared Function Random(shape As Integer(), Optional min As Single = -1.0F, Optional max As Single = 1.0F, Optional seed As Integer? = Nothing) As Tensor
            Dim lRandom = If(seed.HasValue, New Random(seed.Value), New Random())
            Dim tensor = New Tensor(shape)
            For i = 0 To tensor.Length - 1
                tensor._data(i) = CSng((lRandom.NextDouble() * (max - min) + min))
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
                Dim randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2)
                tensor._data(i) = CSng(mean + stdDev * randStdNormal)
            Next
            Return tensor
        End Function

        ''' <summary>
        ''' Xavier初始化 - 适用于sigmoid/tanh激活函数
        ''' </summary>
        Public Shared Function XavierInit(fanIn As Integer, fanOut As Integer, Optional seed As Integer? = Nothing) As Tensor
            Dim stdDev As Single = Math.Sqrt(2.0 / (fanIn + fanOut))
            Return RandomNormal(New Integer() {fanIn, fanOut}, 0.0F, stdDev, seed)
        End Function

        ''' <summary>
        ''' He初始化 - 适用于ReLU激活函数
        ''' </summary>
        Public Shared Function HeInit(fanIn As Integer, fanOut As Integer, Optional seed As Integer? = Nothing) As Tensor
            Dim stdDev As Single = Math.Sqrt(2.0 / fanIn)
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
                result._data(i) = a._data(i) + b._data(i)
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
                result._data(i) = a._data(i) - b._data(i)
            Next
            Return result
        End Operator

        ''' <summary>
        ''' 张量与标量相乘
        ''' </summary>
        Public Shared Operator *(a As Tensor, scalar As Single) As Tensor
            Dim result = New Tensor(a.Shape)
            For i = 0 To a.Length - 1
                result._data(i) = a._data(i) * scalar
            Next
            Return result
        End Operator

        ''' <summary>
        ''' 张量与标量相除
        ''' </summary>
        Public Shared Operator /(a As Tensor, scalar As Single) As Tensor
            Dim result = New Tensor(a.Shape)
            For i = 0 To a.Length - 1
                result._data(i) = a._data(i) / scalar
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
                result._data(i) = _data(i) * other._data(i)
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
                sum += _data(i)
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
                sumSquares += _data(i) * _data(i)
            Next
            Return Math.Sqrt(sumSquares)
        End Function

#End Region

#Region "逐元素函数"

        ''' <summary>
        ''' 对每个元素应用函数
        ''' </summary>
        Public Function Apply(func As Func(Of Single, Single)) As Tensor
            Dim result = New Tensor(Shape)
            For i = 0 To Length - 1
                result._data(i) = func(_data(i))
            Next
            Return result
        End Function
#End Region

        ''' <summary>
        ''' 深拷贝
        ''' </summary>
        Public Function Clone() As Tensor
            Return New Tensor(_data, Shape)
        End Function

        ''' <summary>
        ''' 获取原始数据数组的副本
        ''' </summary>
        Public Function ToArray() As Single()
            Return CType(_data.Clone(), Single())
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
                Console.WriteLine($"[{String.Join(", ", _data.Select(Function(v) v.ToString("F4")))}]")
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

        Public Overrides Function ToString() As String
            Return $"Tensor({String.Join(", ", Shape)})"
        End Function
    End Class
End Namespace
