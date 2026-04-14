' ============================================================================
' NVector.vb - 向量操作类
' Sundials CVODE求解器的向量基础模块
' 仅基于.NET基础数学函数库实现，不依赖第三方库
' ============================================================================

Imports System

Namespace Sundials.CVODE

    ''' <summary>
    ''' NVector类实现了SUNDIALS中的N_Vector抽象数据类型
    ''' 提供向量的基本操作：创建、复制、算术运算、范数计算等
    ''' </summary>
    Public Class NVector
        Implements ICloneable

        ' 向量数据存储
        Private _data As Double()
        Private _length As Integer

#Region "构造函数"

        ''' <summary>
        ''' 创建指定长度的向量，元素初始化为0
        ''' </summary>
        ''' <param name="length">向量长度</param>
        Public Sub New(length As Integer)
            If length <= 0 Then
                Throw New ArgumentException("向量长度必须为正数", NameOf(length))
            End If
            _length = length
            _data = New Double(length - 1) {}
        End Sub

        ''' <summary>
        ''' 从数组创建向量
        ''' </summary>
        ''' <param name="data">数据数组</param>
        Public Sub New(data As Double())
            If data Is Nothing Then
                Throw New ArgumentNullException(NameOf(data))
            End If
            _length = data.Length
            _data = DirectCast(data.Clone(), Double())
        End Sub

        ''' <summary>
        ''' 复制构造函数
        ''' </summary>
        ''' <param name="other">要复制的向量</param>
        Public Sub New(other As NVector)
            If other Is Nothing Then
                Throw New ArgumentNullException(NameOf(other))
            End If
            _length = other._length
            _data = DirectCast(other._data.Clone(), Double())
        End Sub

#End Region

#Region "属性"

        ''' <summary>
        ''' 获取向量长度
        ''' </summary>
        Public ReadOnly Property Length As Integer
            Get
                Return _length
            End Get
        End Property

        ''' <summary>
        ''' 获取或设置指定索引处的元素
        ''' </summary>
        ''' <param name="index">索引</param>
        ''' <returns>元素值</returns>
        Default Public Property Item(index As Integer) As Double
            Get
                If index < 0 OrElse index >= _length Then
                    Throw New IndexOutOfRangeException($"索引 {index} 超出范围 [0, {_length - 1}]")
                End If
                Return _data(index)
            End Get
            Set(value As Double)
                If index < 0 OrElse index >= _length Then
                    Throw New IndexOutOfRangeException($"索引 {index} 超出范围 [0, {_length - 1}]")
                End If
                _data(index) = value
            End Set
        End Property

        ''' <summary>
        ''' 获取内部数据数组的引用（只读）
        ''' </summary>
        Public ReadOnly Property Data As Double()
            Get
                Return _data
            End Get
        End Property

#End Region

#Region "静态工厂方法"

        ''' <summary>
        ''' 创建全零向量
        ''' </summary>
        Public Shared Function Zeros(length As Integer) As NVector
            Return New NVector(length)
        End Function

        ''' <summary>
        ''' 创建全1向量
        ''' </summary>
        Public Shared Function Ones(length As Integer) As NVector
            Dim v As New NVector(length)
            For i As Integer = 0 To length - 1
                v(i) = 1.0
            Next
            Return v
        End Function

        ''' <summary>
        ''' 创建常数向量
        ''' </summary>
        Public Shared Function Constant(length As Integer, value As Double) As NVector
            Dim v As New NVector(length)
            For i As Integer = 0 To length - 1
                v(i) = value
            Next
            Return v
        End Function

        ''' <summary>
        ''' 创建线性间隔向量
        ''' </summary>
        Public Shared Function LinSpace(start As Double, [end] As Double, count As Integer) As NVector
            If count <= 0 Then
                Throw New ArgumentException("元素数量必须为正数", NameOf(count))
            End If
            Dim v As New NVector(count)
            If count = 1 Then
                v(0) = start
            Else
                Dim [step] As Double = ([end] - start) / (count - 1)
                For i As Integer = 0 To count - 1
                    v(i) = start + i * [step]
                Next
            End If
            Return v
        End Function

#End Region

#Region "向量运算"

        ''' <summary>
        ''' 向量加法：this = x + y
        ''' </summary>
        Public Shared Function Add(x As NVector, y As NVector) As NVector
            If x.Length <> y.Length Then
                Throw New ArgumentException("向量长度不匹配")
            End If
            Dim result As New NVector(x.Length)
            For i As Integer = 0 To x.Length - 1
                result(i) = x(i) + y(i)
            Next
            Return result
        End Function

        ''' <summary>
        ''' 向量减法：this = x - y
        ''' </summary>
        Public Shared Function Subtract(x As NVector, y As NVector) As NVector
            If x.Length <> y.Length Then
                Throw New ArgumentException("向量长度不匹配")
            End If
            Dim result As New NVector(x.Length)
            For i As Integer = 0 To x.Length - 1
                result(i) = x(i) - y(i)
            Next
            Return result
        End Function

        ''' <summary>
        ''' 向量点乘
        ''' </summary>
        Public Shared Function Dot(x As NVector, y As NVector) As Double
            If x.Length <> y.Length Then
                Throw New ArgumentException("向量长度不匹配")
            End If
            Dim sum As Double = 0.0
            For i As Integer = 0 To x.Length - 1
                sum += x(i) * y(i)
            Next
            Return sum
        End Function

        ''' <summary>
        ''' 向量逐元素乘法
        ''' </summary>
        Public Shared Function MultiplyElementWise(x As NVector, y As NVector) As NVector
            If x.Length <> y.Length Then
                Throw New ArgumentException("向量长度不匹配")
            End If
            Dim result As New NVector(x.Length)
            For i As Integer = 0 To x.Length - 1
                result(i) = x(i) * y(i)
            Next
            Return result
        End Function

        ''' <summary>
        ''' 向量逐元素除法
        ''' </summary>
        Public Shared Function DivideElementWise(x As NVector, y As NVector) As NVector
            If x.Length <> y.Length Then
                Throw New ArgumentException("向量长度不匹配")
            End If
            Dim result As New NVector(x.Length)
            For i As Integer = 0 To x.Length - 1
                If Math.Abs(y(i)) < Double.Epsilon Then
                    Throw New DivideByZeroException($"向量y在索引{i}处为零")
                End If
                result(i) = x(i) / y(i)
            Next
            Return result
        End Function

        ''' <summary>
        ''' 标量乘法
        ''' </summary>
        Public Shared Function Scale(scalar As Double, v As NVector) As NVector
            Dim result As New NVector(v.Length)
            For i As Integer = 0 To v.Length - 1
                result(i) = scalar * v(i)
            Next
            Return result
        End Function

        ''' <summary>
        ''' 线性组合：result = a*x + b*y
        ''' </summary>
        Public Shared Function LinearSum(a As Double, x As NVector, b As Double, y As NVector) As NVector
            If x.Length <> y.Length Then
                Throw New ArgumentException("向量长度不匹配")
            End If
            Dim result As New NVector(x.Length)
            For i As Integer = 0 To x.Length - 1
                result(i) = a * x(i) + b * y(i)
            Next
            Return result
        End Function

        ''' <summary>
        ''' 线性组合：this = a*x + this
        ''' </summary>
        Public Sub LinearSumInPlace(a As Double, x As NVector)
            If Length <> x.Length Then
                Throw New ArgumentException("向量长度不匹配")
            End If
            For i As Integer = 0 To _length - 1
                _data(i) = a * x(i) + _data(i)
            Next
        End Sub

        ''' <summary>
        ''' 向量取反
        ''' </summary>
        Public Shared Function Negate(v As NVector) As NVector
            Dim result As New NVector(v.Length)
            For i As Integer = 0 To v.Length - 1
                result(i) = -v(i)
            Next
            Return result
        End Function

        ''' <summary>
        ''' 绝对值
        ''' </summary>
        Public Shared Function Abs(v As NVector) As NVector
            Dim result As New NVector(v.Length)
            For i As Integer = 0 To v.Length - 1
                result(i) = Math.Abs(v(i))
            Next
            Return result
        End Function

        ''' <summary>
        ''' 平方根
        ''' </summary>
        Public Shared Function Sqrt(v As NVector) As NVector
            Dim result As New NVector(v.Length)
            For i As Integer = 0 To v.Length - 1
                If v(i) < 0 Then
                    Throw New ArgumentException($"向量在索引{i}处为负数，无法计算平方根")
                End If
                result(i) = Math.Sqrt(v(i))
            Next
            Return result
        End Function

#End Region

#Region "范数计算"

        ''' <summary>
        ''' L2范数（欧几里得范数）
        ''' </summary>
        Public Function L2Norm() As Double
            Dim sum As Double = 0.0
            For i As Integer = 0 To _length - 1
                sum += _data(i) * _data(i)
            Next
            Return Math.Sqrt(sum)
        End Function

        ''' <summary>
        ''' L1范数（绝对值之和）
        ''' </summary>
        Public Function L1Norm() As Double
            Dim sum As Double = 0.0
            For i As Integer = 0 To _length - 1
                sum += Math.Abs(_data(i))
            Next
            Return sum
        End Function

        ''' <summary>
        ''' 无穷范数（最大绝对值）
        ''' </summary>
        Public Function InfinityNorm() As Double
            Dim maxVal As Double = 0.0
            For i As Integer = 0 To _length - 1
                Dim absVal As Double = Math.Abs(_data(i))
                If absVal > maxVal Then
                    maxVal = absVal
                End If
            Next
            Return maxVal
        End Function

        ''' <summary>
        ''' 加权RMS范数
        ''' </summary>
        ''' <param name="weights">权重向量</param>
        Public Function WRMSNorm(weights As NVector) As Double
            If Length <> weights.Length Then
                Throw New ArgumentException("向量长度不匹配")
            End If
            Dim sum As Double = 0.0
            For i As Integer = 0 To _length - 1
                Dim temp As Double = weights(i) * _data(i)
                sum += temp * temp
            Next
            Return Math.Sqrt(sum / _length)
        End Function

        ''' <summary>
        ''' 加权RMS范数的平方
        ''' </summary>
        Public Function WRMSNormSquare(weights As NVector) As Double
            If Length <> weights.Length Then
                Throw New ArgumentException("向量长度不匹配")
            End If
            Dim sum As Double = 0.0
            For i As Integer = 0 To _length - 1
                Dim temp As Double = weights(i) * _data(i)
                sum += temp * temp
            Next
            Return sum / _length
        End Function

#End Region

#Region "原地操作"

        ''' <summary>
        ''' 原地加法：this = this + c
        ''' </summary>
        Public Sub AddConstant(c As Double)
            For i As Integer = 0 To _length - 1
                _data(i) += c
            Next
        End Sub

        ''' <summary>
        ''' 原地加法：this = this + v
        ''' </summary>
        Public Sub AddVector(v As NVector)
            If Length <> v.Length Then
                Throw New ArgumentException("向量长度不匹配")
            End If
            For i As Integer = 0 To _length - 1
                _data(i) += v(i)
            Next
        End Sub

        ''' <summary>
        ''' 原地减法：this = this - v
        ''' </summary>
        Public Sub SubtractVector(v As NVector)
            If Length <> v.Length Then
                Throw New ArgumentException("向量长度不匹配")
            End If
            For i As Integer = 0 To _length - 1
                _data(i) -= v(i)
            Next
        End Sub

        ''' <summary>
        ''' 原地标量乘法：this = c * this
        ''' </summary>
        Public Sub ScaleInPlace(c As Double)
            For i As Integer = 0 To _length - 1
                _data(i) *= c
            Next
        End Sub

        ''' <summary>
        ''' 原地逐元素乘法：this = this .* v
        ''' </summary>
        Public Sub MultiplyElementWiseInPlace(v As NVector)
            If Length <> v.Length Then
                Throw New ArgumentException("向量长度不匹配")
            End If
            For i As Integer = 0 To _length - 1
                _data(i) *= v(i)
            Next
        End Sub

        ''' <summary>
        ''' 原地逐元素除法：this = this ./ v
        ''' </summary>
        Public Sub DivideElementWiseInPlace(v As NVector)
            If Length <> v.Length Then
                Throw New ArgumentException("向量长度不匹配")
            End If
            For i As Integer = 0 To _length - 1
                If Math.Abs(v(i)) < Double.Epsilon Then
                    Throw New DivideByZeroException($"向量v在索引{i}处为零")
                End If
                _data(i) /= v(i)
            Next
        End Sub

        ''' <summary>
        ''' 设置所有元素为指定值
        ''' </summary>
        Public Sub SetConstant(value As Double)
            For i As Integer = 0 To _length - 1
                _data(i) = value
            Next
        End Sub

        ''' <summary>
        ''' 复制另一个向量的值到当前向量
        ''' </summary>
        Public Sub CopyFrom(source As NVector)
            If Length <> source.Length Then
                Throw New ArgumentException("向量长度不匹配")
            End If
            Array.Copy(source._data, _data, _length)
        End Sub

#End Region

#Region "比较操作"

        ''' <summary>
        ''' 比较两个向量是否相等（在容差范围内）
        ''' </summary>
        Public Function EqualsApprox(other As NVector, tolerance As Double) As Boolean
            If other Is Nothing OrElse Length <> other.Length Then
                Return False
            End If
            For i As Integer = 0 To _length - 1
                If Math.Abs(_data(i) - other(i)) > tolerance Then
                    Return False
                End If
            Next
            Return True
        End Function

        ''' <summary>
        ''' 查找最大值及其索引
        ''' </summary>
        Public Function Max() As (value As Double, index As Integer)
            Dim maxVal As Double = _data(0)
            Dim maxIdx As Integer = 0
            For i As Integer = 1 To _length - 1
                If _data(i) > maxVal Then
                    maxVal = _data(i)
                    maxIdx = i
                End If
            Next
            Return (maxVal, maxIdx)
        End Function

        ''' <summary>
        ''' 查找最小值及其索引
        ''' </summary>
        Public Function Min() As (value As Double, index As Integer)
            Dim minVal As Double = _data(0)
            Dim minIdx As Integer = 0
            For i As Integer = 1 To _length - 1
                If _data(i) < minVal Then
                    minVal = _data(i)
                    minIdx = i
                End If
            Next
            Return (minVal, minIdx)
        End Function

        ''' <summary>
        ''' 查找最大绝对值及其索引
        ''' </summary>
        Public Function MaxAbs() As (value As Double, index As Integer)
            Dim maxVal As Double = Math.Abs(_data(0))
            Dim maxIdx As Integer = 0
            For i As Integer = 1 To _length - 1
                Dim absVal As Double = Math.Abs(_data(i))
                If absVal > maxVal Then
                    maxVal = absVal
                    maxIdx = i
                End If
            Next
            Return (maxVal, maxIdx)
        End Function

#End Region

#Region "接口实现"

        ''' <summary>
        ''' 克隆向量
        ''' </summary>
        Public Function Clone() As Object Implements ICloneable.Clone
            Return New NVector(Me)
        End Function

        ''' <summary>
        ''' 转换为字符串
        ''' </summary>
        Public Overrides Function ToString() As String
            Const maxDisplay As Integer = 10
            Dim sb As New Text.StringBuilder()
            sb.Append("NVector[")
            sb.Append(_length)
            sb.Append("]: {")
            Dim displayCount As Integer = Math.Min(_length, maxDisplay)
            For i As Integer = 0 To displayCount - 1
                If i > 0 Then sb.Append(", ")
                sb.Append(_data(i).ToString("G6"))
            Next
            If _length > maxDisplay Then
                sb.Append(", ...")
            End If
            sb.Append("}")
            Return sb.ToString()
        End Function

#End Region

    End Class

End Namespace
