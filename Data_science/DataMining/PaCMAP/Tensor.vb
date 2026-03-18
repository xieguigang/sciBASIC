Imports System
Imports System.Linq

Namespace PaCMAP
    ''' <summary>
    ''' 多维张量类，支持类似TensorFlow.js的操作
    ''' Multi-dimensional tensor class supporting TensorFlow.js-like operations
    ''' </summary>
    Public Class Tensor
        Implements ICloneable, IDisposable
        Private _data As Double()
        Private _shape As Integer()
        Private _disposed As Boolean = False

        ''' <summary>
        ''' 张量的形状
        ''' Shape of the tensor
        ''' </summary>
        Public ReadOnly Property Shape As Integer()
            Get
                Return _shape
            End Get
        End Property

        ''' <summary>
        ''' 张量的维度数
        ''' Number of dimensions
        ''' </summary>
        Public ReadOnly Property Rank As Integer
            Get
                Return _shape.Length
            End Get
        End Property

        ''' <summary>
        ''' 张量中元素的总数
        ''' Total number of elements
        ''' </summary>
        Public ReadOnly Property Length As Integer
            Get
                Return _data.Length
            End Get
        End Property

        ''' <summary>
        ''' 底层数据数组
        ''' Underlying data array
        ''' </summary>
        Public ReadOnly Property Data As Double()
            Get
                Return _data
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

        ''' <summary>
        ''' 创建指定形状的张量
        ''' Create a tensor with specified shape
        ''' </summary>
        Public Sub New(shape As Integer())
            _shape = CType(shape.Clone(), Integer())
            Dim size = shape.Aggregate(1, Function(a, b) a * b)
            _data = New Double(size - 1) {}
        End Sub

        ''' <summary>
        ''' 从数据数组创建张量
        ''' Create tensor from data array
        ''' </summary>
        Public Sub New(data As Double(), shape As Integer())
            _data = CType(data.Clone(), Double())
            _shape = CType(shape.Clone(), Integer())

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
            _shape = New Integer() {rows, cols}
            _data = New Double(rows * cols - 1) {}

            For i = 0 To rows - 1
                For j = 0 To cols - 1
                    _data(i * cols + j) = data(i, j)
                Next
            Next
        End Sub

        ''' <summary>
        ''' 创建标量张量
        ''' Create scalar tensor
        ''' </summary>
        Public Shared Function Scalar(value As Double) As Tensor
            Dim tensor = New Tensor(New Integer() {1})
            tensor._data(0) = value
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
            Array.Fill(tensor._data, 1.0)
            Return tensor
        End Function

        ''' <summary>
        ''' 创建随机正态分布张量
        ''' Create random normal tensor
        ''' </summary>
        Public Shared Function RandomNormal(shape As Integer(), Optional mean As Double = 0, Optional stdDev As Double = 1, Optional seed As Integer? = Nothing) As Tensor
            Dim random = If(seed.HasValue, New Random(seed.Value), New Random())
            Dim tensor = New Tensor(shape)

            ' Box-Muller变换生成正态分布
            For i As Integer = 0 To tensor._data.Length - 1 Step 2
                Dim u1 As Double = random.NextDouble()
                Dim u2 As Double = random.NextDouble()

                ' 避免log(0)
                While u1 = 0
                    u1 = random.NextDouble()
                End While

                Dim z0 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2)
                tensor._data(i) = z0 * stdDev + mean

                If i + 1 < tensor._data.Length Then
                    Dim z1 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2)
                    tensor._data(i + 1) = z1 * stdDev + mean
                End If
            Next

            Return tensor
        End Function

        ''' <summary>
        ''' 创建随机均匀分布张量
        ''' Create random uniform tensor
        ''' </summary>
        Public Shared Function RandomUniform(shape As Integer(), Optional min As Double = 0, Optional max As Double = 1, Optional seed As Integer? = Nothing) As Tensor
            Dim random = If(seed.HasValue, New Random(seed.Value), New Random())
            Dim tensor = New Tensor(shape)

            For i As Integer = 0 To tensor._data.Length - 1
                tensor._data(i) = min + random.NextDouble() * (max - min)
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
                tensor._data(i) = start + i * [step]
            Next

            Return tensor
        End Function

        ''' <summary>
        ''' 获取指定索引的值
        ''' Get value at specified indices
        ''' </summary>
        Public Function GetValue(ParamArray indices As Integer()) As Double
            Dim flatIndex = GetFlatIndex(indices)
            Return _data(flatIndex)
        End Function

        ''' <summary>
        ''' 设置指定索引的值
        ''' Set value at specified indices
        ''' </summary>
        Public Sub SetValue(value As Double, ParamArray indices As Integer())
            Dim flatIndex = GetFlatIndex(indices)
            _data(flatIndex) = value
        End Sub

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
                If indices(i) < 0 OrElse indices(i) >= _shape(i) Then
                    Throw New IndexOutOfRangeException($"Index {indices(i)} out of bounds for dimension {i} with size {_shape(i)}")
                End If
                flatIndex += indices(i) * multiplier
                multiplier *= _shape(i)
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
                indices(i) = remaining Mod _shape(i)
                remaining /= _shape(i)
            Next

            Return indices
        End Function

        ''' <summary>
        ''' 克隆张量
        ''' Clone tensor
        ''' </summary>
        Public Function Clone() As Object Implements ICloneable.Clone
            Return New Tensor(CType(_data.Clone(), Double()), CType(_shape.Clone(), Integer()))
        End Function

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
        ''' 转换为字符串表示
        ''' Convert to string representation
        ''' </summary>
        Public Overrides Function ToString() As String
            Return $"Tensor(shape=[{String.Join(",", _shape)}])"
        End Function

        ''' <summary>
        ''' 释放资源
        ''' Dispose resources
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            If Not _disposed Then
                _data = Nothing
                _shape = Nothing
                Gradient?.Dispose()
                _disposed = True
            End If
        End Sub
    End Class
End Namespace
