Imports std = System.Math

' ============================================================================
' DenseMatrix.vb - 稠密矩阵类
' Sundials CVODE求解器的矩阵基础模块
' 仅基于.NET基础数学函数库实现，不依赖第三方库
' ============================================================================

''' <summary>
''' 稠密矩阵类，提供矩阵存储和基本操作
''' 采用列优先存储方式，便于与BLAS/LAPACK风格的操作兼容
''' </summary>
Public Class DenseMatrix : Implements ICloneable

    ' 矩阵数据存储（列优先）
    Private _data As Double(,)
    Private _rows As Integer
    Private _cols As Integer

#Region "构造函数"

    ''' <summary>
    ''' 创建指定大小的矩阵，元素初始化为0
    ''' </summary>
    ''' <param name="rows">行数</param>
    ''' <param name="cols">列数</param>
    Public Sub New(rows As Integer, cols As Integer)
        If rows <= 0 OrElse cols <= 0 Then
            Throw New ArgumentException("矩阵维度必须为正数")
        End If
        _rows = rows
        _cols = cols
        _data = New Double(rows - 1, cols - 1) {}
    End Sub

    ''' <summary>
    ''' 从二维数组创建矩阵
    ''' </summary>
    ''' <param name="data">数据数组（行优先格式）</param>
    Public Sub New(data As Double(,))
        If data Is Nothing Then
            Throw New ArgumentNullException(NameOf(data))
        End If
        _rows = data.GetLength(0)
        _cols = data.GetLength(1)
        _data = DirectCast(data.Clone(), Double(,))
    End Sub

    ''' <summary>
    ''' 复制构造函数
    ''' </summary>
    Public Sub New(other As DenseMatrix)
        If other Is Nothing Then
            Throw New ArgumentNullException(NameOf(other))
        End If
        _rows = other._rows
        _cols = other._cols
        _data = DirectCast(other._data.Clone(), Double(,))
    End Sub

#End Region

#Region "属性"

    ''' <summary>
    ''' 获取行数
    ''' </summary>
    Public ReadOnly Property Rows As Integer
        Get
            Return _rows
        End Get
    End Property

    ''' <summary>
    ''' 获取列数
    ''' </summary>
    Public ReadOnly Property Columns As Integer
        Get
            Return _cols
        End Get
    End Property

    ''' <summary>
    ''' 判断是否为方阵
    ''' </summary>
    Public ReadOnly Property IsSquare As Boolean
        Get
            Return _rows = _cols
        End Get
    End Property

    ''' <summary>
    ''' 获取或设置指定位置的元素
    ''' </summary>
    Default Public Property Item(row As Integer, col As Integer) As Double
        Get
            If row < 0 OrElse row >= _rows OrElse col < 0 OrElse col >= _cols Then
                Throw New IndexOutOfRangeException($"索引 ({row}, {col}) 超出范围")
            End If
            Return _data(row, col)
        End Get
        Set(value As Double)
            If row < 0 OrElse row >= _rows OrElse col < 0 OrElse col >= _cols Then
                Throw New IndexOutOfRangeException($"索引 ({row}, {col}) 超出范围")
            End If
            _data(row, col) = value
        End Set
    End Property

    ''' <summary>
    ''' 获取内部数据数组的引用
    ''' </summary>
    Public ReadOnly Property Data As Double(,)
        Get
            Return _data
        End Get
    End Property

#End Region

#Region "静态工厂方法"

    ''' <summary>
    ''' 创建单位矩阵
    ''' </summary>
    Public Shared Function Identity(n As Integer) As DenseMatrix
        Dim m As New DenseMatrix(n, n)
        For i As Integer = 0 To n - 1
            m(i, i) = 1.0
        Next
        Return m
    End Function

    ''' <summary>
    ''' 创建零矩阵
    ''' </summary>
    Public Shared Function Zeros(rows As Integer, cols As Integer) As DenseMatrix
        Return New DenseMatrix(rows, cols)
    End Function

    ''' <summary>
    ''' 创建全1矩阵
    ''' </summary>
    Public Shared Function Ones(rows As Integer, cols As Integer) As DenseMatrix
        Dim m As New DenseMatrix(rows, cols)
        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To cols - 1
                m(i, j) = 1.0
            Next
        Next
        Return m
    End Function

    ''' <summary>
    ''' 创建对角矩阵
    ''' </summary>
    Public Shared Function Diagonal(diag As NVector) As DenseMatrix
        Dim n As Integer = diag.Length
        Dim m As New DenseMatrix(n, n)
        For i As Integer = 0 To n - 1
            m(i, i) = diag(i)
        Next
        Return m
    End Function

#End Region

#Region "矩阵运算"

    ''' <summary>
    ''' 矩阵加法
    ''' </summary>
    Public Shared Function Add(A As DenseMatrix, B As DenseMatrix) As DenseMatrix
        If A.Rows <> B.Rows OrElse A.Columns <> B.Columns Then
            Throw New ArgumentException("矩阵维度不匹配")
        End If
        Dim result As New DenseMatrix(A.Rows, A.Columns)
        For i As Integer = 0 To A.Rows - 1
            For j As Integer = 0 To A.Columns - 1
                result(i, j) = A(i, j) + B(i, j)
            Next
        Next
        Return result
    End Function

    ''' <summary>
    ''' 矩阵减法
    ''' </summary>
    Public Shared Function Subtract(A As DenseMatrix, B As DenseMatrix) As DenseMatrix
        If A.Rows <> B.Rows OrElse A.Columns <> B.Columns Then
            Throw New ArgumentException("矩阵维度不匹配")
        End If
        Dim result As New DenseMatrix(A.Rows, A.Columns)
        For i As Integer = 0 To A.Rows - 1
            For j As Integer = 0 To A.Columns - 1
                result(i, j) = A(i, j) - B(i, j)
            Next
        Next
        Return result
    End Function

    ''' <summary>
    ''' 标量乘法
    ''' </summary>
    Public Shared Function Scale(scalar As Double, A As DenseMatrix) As DenseMatrix
        Dim result As New DenseMatrix(A.Rows, A.Columns)
        For i As Integer = 0 To A.Rows - 1
            For j As Integer = 0 To A.Columns - 1
                result(i, j) = scalar * A(i, j)
            Next
        Next
        Return result
    End Function

    ''' <summary>
    ''' 矩阵乘法
    ''' </summary>
    Public Shared Function Multiply(A As DenseMatrix, B As DenseMatrix) As DenseMatrix
        If A.Columns <> B.Rows Then
            Throw New ArgumentException("矩阵维度不匹配，无法相乘")
        End If
        Dim result As New DenseMatrix(A.Rows, B.Columns)
        For i As Integer = 0 To A.Rows - 1
            For j As Integer = 0 To B.Columns - 1
                Dim sum As Double = 0.0
                For k As Integer = 0 To A.Columns - 1
                    sum += A(i, k) * B(k, j)
                Next
                result(i, j) = sum
            Next
        Next
        Return result
    End Function

    ''' <summary>
    ''' 矩阵-向量乘法：y = A * x
    ''' </summary>
    Public Shared Function MultiplyVector(A As DenseMatrix, x As NVector) As NVector
        If A.Columns <> x.Length Then
            Throw New ArgumentException("矩阵列数与向量长度不匹配")
        End If
        Dim y As New NVector(A.Rows)
        For i As Integer = 0 To A.Rows - 1
            Dim sum As Double = 0.0
            For j As Integer = 0 To A.Columns - 1
                sum += A(i, j) * x(j)
            Next
            y(i) = sum
        Next
        Return y
    End Function

    ''' <summary>
    ''' 矩阵转置
    ''' </summary>
    Public Function Transpose() As DenseMatrix
        Dim result As New DenseMatrix(_cols, _rows)
        For i As Integer = 0 To _rows - 1
            For j As Integer = 0 To _cols - 1
                result(j, i) = _data(i, j)
            Next
        Next
        Return result
    End Function

#End Region

#Region "原地操作"

    ''' <summary>
    ''' 原地加法
    ''' </summary>
    Public Sub AddInPlace(B As DenseMatrix)
        If Rows <> B.Rows OrElse Columns <> B.Columns Then
            Throw New ArgumentException("矩阵维度不匹配")
        End If
        For i As Integer = 0 To _rows - 1
            For j As Integer = 0 To _cols - 1
                _data(i, j) += B(i, j)
            Next
        Next
    End Sub

    ''' <summary>
    ''' 原地标量乘法
    ''' </summary>
    Public Sub ScaleInPlace(scalar As Double)
        For i As Integer = 0 To _rows - 1
            For j As Integer = 0 To _cols - 1
                _data(i, j) *= scalar
            Next
        Next
    End Sub

    ''' <summary>
    ''' 设置所有元素为指定值
    ''' </summary>
    Public Sub SetConstant(value As Double)
        For i As Integer = 0 To _rows - 1
            For j As Integer = 0 To _cols - 1
                _data(i, j) = value
            Next
        Next
    End Sub

    ''' <summary>
    ''' 复制另一个矩阵的值
    ''' </summary>
    Public Sub CopyFrom(source As DenseMatrix)
        If Rows <> source.Rows OrElse Columns <> source.Columns Then
            Throw New ArgumentException("矩阵维度不匹配")
        End If
        Array.Copy(source._data, _data, _data.Length)
    End Sub

    ''' <summary>
    ''' 设置对角线元素
    ''' </summary>
    Public Sub SetDiagonal(value As Double)
        Dim n As Integer = Math.Min(_rows, _cols)
        For i As Integer = 0 To n - 1
            _data(i, i) = value
        Next
    End Sub

    ''' <summary>
    ''' 设置指定列
    ''' </summary>
    Public Sub SetColumn(colIndex As Integer, v As NVector)
        If colIndex < 0 OrElse colIndex >= _cols Then
            Throw New IndexOutOfRangeException("列索引超出范围")
        End If
        If v.Length <> _rows Then
            Throw New ArgumentException("向量长度与矩阵行数不匹配")
        End If
        For i As Integer = 0 To _rows - 1
            _data(i, colIndex) = v(i)
        Next
    End Sub

    ''' <summary>
    ''' 获取指定列
    ''' </summary>
    Public Function GetColumn(colIndex As Integer) As NVector
        If colIndex < 0 OrElse colIndex >= _cols Then
            Throw New IndexOutOfRangeException("列索引超出范围")
        End If
        Dim v As New NVector(_rows)
        For i As Integer = 0 To _rows - 1
            v(i) = _data(i, colIndex)
        Next
        Return v
    End Function

    ''' <summary>
    ''' 设置指定行
    ''' </summary>
    Public Sub SetRow(rowIndex As Integer, v As NVector)
        If rowIndex < 0 OrElse rowIndex >= _rows Then
            Throw New IndexOutOfRangeException("行索引超出范围")
        End If
        If v.Length <> _cols Then
            Throw New ArgumentException("向量长度与矩阵列数不匹配")
        End If
        For j As Integer = 0 To _cols - 1
            _data(rowIndex, j) = v(j)
        Next
    End Sub

    ''' <summary>
    ''' 获取指定行
    ''' </summary>
    Public Function GetRow(rowIndex As Integer) As NVector
        If rowIndex < 0 OrElse rowIndex >= _rows Then
            Throw New IndexOutOfRangeException("行索引超出范围")
        End If
        Dim v As New NVector(_cols)
        For j As Integer = 0 To _cols - 1
            v(j) = _data(rowIndex, j)
        Next
        Return v
    End Function

#End Region

#Region "范数计算"

    ''' <summary>
    ''' Frobenius范数
    ''' </summary>
    Public Function FrobeniusNorm() As Double
        Dim sum As Double = 0.0
        For i As Integer = 0 To _rows - 1
            For j As Integer = 0 To _cols - 1
                sum += _data(i, j) * _data(i, j)
            Next
        Next
        Return std.Sqrt(sum)
    End Function

    ''' <summary>
    ''' 1-范数（列和的最大值）
    ''' </summary>
    Public Function Norm1() As Double
        Dim maxSum As Double = 0.0
        For j As Integer = 0 To _cols - 1
            Dim colSum As Double = 0.0
            For i As Integer = 0 To _rows - 1
                colSum += std.Abs(_data(i, j))
            Next
            If colSum > maxSum Then
                maxSum = colSum
            End If
        Next
        Return maxSum
    End Function

    ''' <summary>
    ''' 无穷范数（行和的最大值）
    ''' </summary>
    Public Function InfinityNorm() As Double
        Dim maxSum As Double = 0.0
        For i As Integer = 0 To _rows - 1
            Dim rowSum As Double = 0.0
            For j As Integer = 0 To _cols - 1
                rowSum += std.Abs(_data(i, j))
            Next
            If rowSum > maxSum Then
                maxSum = rowSum
            End If
        Next
        Return maxSum
    End Function

#End Region

#Region "接口实现"

    Public Function Clone() As Object Implements ICloneable.Clone
        Return New DenseMatrix(Me)
    End Function

    Public Overrides Function ToString() As String
        Dim sb As New Text.StringBuilder()
        sb.AppendLine($"DenseMatrix({_rows}x{_cols}):")
        Const maxDisplay As Integer = 6
        Dim displayRows As Integer = std.Min(_rows, maxDisplay)
        Dim displayCols As Integer = std.Min(_cols, maxDisplay)
        For i As Integer = 0 To displayRows - 1
            sb.Append("  [")
            For j As Integer = 0 To displayCols - 1
                If j > 0 Then sb.Append(", ")
                sb.Append(_data(i, j).ToString("G8"))
            Next
            If _cols > maxDisplay Then sb.Append(", ...")
            sb.AppendLine("]")
        Next
        If _rows > maxDisplay Then sb.AppendLine("  ...")
        Return sb.ToString()
    End Function

#End Region

End Class


