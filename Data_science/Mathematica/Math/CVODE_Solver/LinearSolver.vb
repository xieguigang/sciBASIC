Imports std = System.Math

' ============================================================================
' LinearSolver.vb - 线性求解器
' Sundials CVODE求解器的线性代数求解模块
' 实现LU分解和前代/回代求解
' 仅基于.NET基础数学函数库实现，不依赖第三方库
' ============================================================================

''' <summary>
''' 线性求解器类型枚举
''' </summary>
Public Enum LinearSolverType
    ''' <summary>
    ''' 稠密矩阵直接求解（LU分解）
    ''' </summary>
    Dense
    ''' <summary>
    ''' 带状矩阵求解
    ''' </summary>
    Band
    ''' <summary>
    ''' 对角矩阵求解
    ''' </summary>
    Diagonal
End Enum

''' <summary>
''' 线性求解器返回状态
''' </summary>
Public Enum LinearSolverResult
    ''' <summary>
    ''' 求解成功
    ''' </summary>
    Success = 0
    ''' <summary>
    ''' 矩阵奇异
    ''' </summary>
    SingularMatrix = -1
    ''' <summary>
    ''' 求解失败
    ''' </summary>
    SolveFailed = -2
    ''' <summary>
    ''' 内存分配失败
    ''' </summary>
    MemoryFail = -3
End Enum

''' <summary>
''' 稠密线性求解器
''' 使用带部分主元选择的LU分解
''' </summary>
Public Class DenseLinearSolver

    ' LU分解后的矩阵
    Private _luMatrix As DenseMatrix
    ' 主元置换数组
    Private _pivots As Integer()
    ' 矩阵维度
    Private _n As Integer
    ' 是否已分解
    Private _isFactored As Boolean
    ' 条件数估计
    Private _conditionNumber As Double
    ' 奇异性阈值
    Private Const SINGULARITY_THRESHOLD As Double = 0.000000000000001

#Region "构造函数"

    ''' <summary>
    ''' 创建指定大小的线性求解器
    ''' </summary>
    ''' <param name="n">矩阵维度</param>
    Public Sub New(n As Integer)
        If n <= 0 Then
            Throw New ArgumentException("矩阵维度必须为正数", NameOf(n))
        End If
        _n = n
        _luMatrix = New DenseMatrix(n, n)
        _pivots = New Integer(n - 1) {}
        _isFactored = False
        _conditionNumber = 0.0
    End Sub

#End Region

#Region "属性"

    ''' <summary>
    ''' 获取矩阵维度
    ''' </summary>
    Public ReadOnly Property Dimension As Integer
        Get
            Return _n
        End Get
    End Property

    ''' <summary>
    ''' 获取是否已进行LU分解
    ''' </summary>
    Public ReadOnly Property IsFactored As Boolean
        Get
            Return _isFactored
        End Get
    End Property

    ''' <summary>
    ''' 获取条件数估计
    ''' </summary>
    Public ReadOnly Property ConditionNumber As Double
        Get
            Return _conditionNumber
        End Get
    End Property

#End Region

#Region "LU分解"

    ''' <summary>
    ''' 对矩阵进行LU分解（带部分主元选择）
    ''' 分解后 A = P * L * U，其中P为置换矩阵
    ''' </summary>
    ''' <param name="A">待分解的矩阵（将被覆盖）</param>
    ''' <returns>分解结果状态</returns>
    Public Function Factorize(A As DenseMatrix) As LinearSolverResult
        If A Is Nothing OrElse A.Rows <> _n OrElse A.Columns <> _n Then
            Throw New ArgumentException("矩阵维度不匹配")
        End If

        ' 复制矩阵到LU存储
        _luMatrix.CopyFrom(A)
        _isFactored = False

        Dim data As Double(,) = _luMatrix.Data

        ' LU分解主循环
        For k As Integer = 0 To _n - 1
            ' 寻找主元
            Dim maxVal As Double = std.Abs(data(k, k))
            Dim maxIdx As Integer = k

            For i As Integer = k + 1 To _n - 1
                Dim absVal As Double = std.Abs(data(i, k))
                If absVal > maxVal Then
                    maxVal = absVal
                    maxIdx = i
                End If
            Next

            ' 记录主元位置
            _pivots(k) = maxIdx

            ' 检查奇异性
            If maxVal < SINGULARITY_THRESHOLD Then
                _conditionNumber = Double.PositiveInfinity
                Return LinearSolverResult.SingularMatrix
            End If

            ' 交换行
            If maxIdx <> k Then
                For j As Integer = 0 To _n - 1
                    Dim temp As Double = data(k, j)
                    data(k, j) = data(maxIdx, j)
                    data(maxIdx, j) = temp
                Next
            End If

            ' 计算乘数并更新子矩阵
            Dim pivot As Double = data(k, k)
            For i As Integer = k + 1 To _n - 1
                data(i, k) /= pivot
                Dim factor As Double = data(i, k)
                For j As Integer = k + 1 To _n - 1
                    data(i, j) -= factor * data(k, j)
                Next
            Next
        Next

        ' 估计条件数
        _conditionNumber = EstimateConditionNumber()
        _isFactored = True

        Return LinearSolverResult.Success
    End Function

    ''' <summary>
    ''' 估计矩阵条件数（使用1-范数）
    ''' </summary>
    Private Function EstimateConditionNumber() As Double
        Dim normA As Double = _luMatrix.Norm1()
        If normA < SINGULARITY_THRESHOLD Then
            Return Double.PositiveInfinity
        End If

        ' 简化的条件数估计：使用对角元素的倒数
        Dim minDiag As Double = Double.MaxValue
        Dim maxDiag As Double = 0.0
        For i As Integer = 0 To _n - 1
            Dim diag As Double = std.Abs(_luMatrix(i, i))
            If diag < minDiag Then minDiag = diag
            If diag > maxDiag Then maxDiag = diag
        Next

        If minDiag < SINGULARITY_THRESHOLD Then
            Return Double.PositiveInfinity
        End If

        Return maxDiag / minDiag
    End Function

#End Region

#Region "求解"

    ''' <summary>
    ''' 求解线性方程组 A*x = b
    ''' 必须先调用Factorize进行LU分解
    ''' </summary>
    ''' <param name="b">右端向量</param>
    ''' <param name="x">解向量（输出）</param>
    ''' <returns>求解结果状态</returns>
    Public Function Solve(b As NVector, x As NVector) As LinearSolverResult
        If Not _isFactored Then
            Throw New InvalidOperationException("必须先调用Factorize进行LU分解")
        End If
        If b Is Nothing OrElse x Is Nothing Then
            Throw New ArgumentNullException()
        End If
        If b.Length <> _n OrElse x.Length <> _n Then
            Throw New ArgumentException("向量长度与矩阵维度不匹配")
        End If

        ' 复制b到x
        x.CopyFrom(b)

        ' 应用行置换
        For k As Integer = 0 To _n - 1
            Dim pivot As Integer = _pivots(k)
            If pivot <> k Then
                Dim temp As Double = x(k)
                x(k) = x(pivot)
                x(pivot) = temp
            End If
        Next

        Dim data As Double(,) = _luMatrix.Data

        ' 前代（解 L*y = x）
        For i As Integer = 1 To _n - 1
            Dim sum As Double = x(i)
            For j As Integer = 0 To i - 1
                sum -= data(i, j) * x(j)
            Next
            x(i) = sum
        Next

        ' 回代（解 U*x = y）
        For i As Integer = _n - 1 To 0 Step -1
            Dim sum As Double = x(i)
            For j As Integer = i + 1 To _n - 1
                sum -= data(i, j) * x(j)
            Next
            x(i) = sum / data(i, i)
        Next

        Return LinearSolverResult.Success
    End Function

    ''' <summary>
    ''' 求解线性方程组 A*x = b（创建新向量返回结果）
    ''' </summary>
    ''' <param name="b">右端向量</param>
    ''' <returns>解向量</returns>
    Public Function Solve(b As NVector) As NVector
        Dim x As New NVector(_n)
        Dim result As LinearSolverResult = Solve(b, x)
        If result <> LinearSolverResult.Success Then
            Throw New InvalidOperationException($"求解失败: {result}")
        End If
        Return x
    End Function

#End Region

#Region "矩阵-向量乘法"

    ''' <summary>
    ''' 计算矩阵-向量乘法：y = A * x
    ''' 使用原始矩阵（分解前）
    ''' </summary>
    Public Shared Sub MatrixVectorMultiply(A As DenseMatrix, x As NVector, y As NVector)
        If A.Columns <> x.Length OrElse A.Rows <> y.Length Then
            Throw New ArgumentException("维度不匹配")
        End If
        For i As Integer = 0 To A.Rows - 1
            Dim sum As Double = 0.0
            For j As Integer = 0 To A.Columns - 1
                sum += A(i, j) * x(j)
            Next
            y(i) = sum
        Next
    End Sub

#End Region

#Region "重置"

    ''' <summary>
    ''' 重置求解器状态
    ''' </summary>
    Public Sub Reset()
        _luMatrix.SetConstant(0.0)
        Array.Clear(_pivots, 0, _pivots.Length)
        _isFactored = False
        _conditionNumber = 0.0
    End Sub

#End Region

End Class

''' <summary>
''' 带状线性求解器
''' 针对带状矩阵优化的LU分解
''' </summary>
Public Class BandLinearSolver

    ' 带状矩阵存储
    Private _bandMatrix As Double(,)
    Private _n As Integer
    Private _lowerBandwidth As Integer
    Private _upperBandwidth As Integer
    Private _pivots As Integer()
    Private _isFactored As Boolean

#Region "构造函数"

    ''' <summary>
    ''' 创建带状线性求解器
    ''' </summary>
    ''' <param name="n">矩阵维度</param>
    ''' <param name="lowerBandwidth">下带宽</param>
    ''' <param name="upperBandwidth">上带宽</param>
    Public Sub New(n As Integer, lowerBandwidth As Integer, upperBandwidth As Integer)
        If n <= 0 Then
            Throw New ArgumentException("矩阵维度必须为正数", NameOf(n))
        End If
        _n = n
        _lowerBandwidth = std.Min(lowerBandwidth, n - 1)
        _upperBandwidth = std.Min(upperBandwidth, n - 1)
        ' 存储格式：每行存储带内的元素
        _bandMatrix = New Double(n - 1, _lowerBandwidth + _upperBandwidth) {}
        _pivots = New Integer(n - 1) {}
        _isFactored = False
    End Sub

#End Region

#Region "带状矩阵操作"

    ''' <summary>
    ''' 设置带状矩阵元素
    ''' </summary>
    Public Sub SetElement(row As Integer, col As Integer, value As Double)
        If row < 0 OrElse row >= _n OrElse col < 0 OrElse col >= _n Then
            Throw New IndexOutOfRangeException()
        End If
        If std.Abs(row - col) > std.Max(_lowerBandwidth, _upperBandwidth) Then
            Throw New ArgumentException("元素超出带宽范围")
        End If
        ' 带状存储索引
        Dim j As Integer = col - row + _lowerBandwidth
        _bandMatrix(row, j) = value
    End Sub

    ''' <summary>
    ''' 获取带状矩阵元素
    ''' </summary>
    Public Function GetElement(row As Integer, col As Integer) As Double
        If row < 0 OrElse row >= _n OrElse col < 0 OrElse col >= _n Then
            Throw New IndexOutOfRangeException()
        End If
        If std.Abs(row - col) > std.Max(_lowerBandwidth, _upperBandwidth) Then
            Return 0.0
        End If
        Dim j As Integer = col - row + _lowerBandwidth
        Return _bandMatrix(row, j)
    End Function

    ''' <summary>
    ''' 从稠密矩阵加载带状矩阵
    ''' </summary>
    Public Sub LoadFromDense(A As DenseMatrix)
        If A Is Nothing OrElse A.Rows <> _n OrElse A.Columns <> _n Then
            Throw New ArgumentException("矩阵维度不匹配")
        End If
        Array.Clear(_bandMatrix, 0, _bandMatrix.Length)
        For i As Integer = 0 To _n - 1
            For j As Integer = std.Max(0, i - _lowerBandwidth) To std.Min(_n - 1, i + _upperBandwidth)
                SetElement(i, j, A(i, j))
            Next
        Next
        _isFactored = False
    End Sub

#End Region

#Region "LU分解"

    ''' <summary>
    ''' 带状矩阵LU分解
    ''' </summary>
    Public Function Factorize() As LinearSolverResult
        _isFactored = False

        For k As Integer = 0 To _n - 1
            ' 寻找主元
            Dim maxVal As Double = std.Abs(GetElement(k, k))
            Dim maxIdx As Integer = k
            Dim limit As Integer = std.Min(_n - 1, k + _lowerBandwidth)

            For i As Integer = k + 1 To limit
                Dim absVal As Double = std.Abs(GetElement(i, k))
                If absVal > maxVal Then
                    maxVal = absVal
                    maxIdx = i
                End If
            Next

            _pivots(k) = maxIdx

            If maxVal < 0.000000000000001 Then
                Return LinearSolverResult.SingularMatrix
            End If

            ' 交换行
            If maxIdx <> k Then
                For j As Integer = std.Max(0, k - _lowerBandwidth) To std.Min(_n - 1, k + _upperBandwidth)
                    Dim temp As Double = GetElement(k, j)
                    SetElement(k, j, GetElement(maxIdx, j))
                    SetElement(maxIdx, j, temp)
                Next
            End If

            ' 更新
            Dim pivot As Double = GetElement(k, k)
            limit = std.Min(_n - 1, k + _lowerBandwidth)
            For i As Integer = k + 1 To limit
                Dim factor As Double = GetElement(i, k) / pivot
                SetElement(i, k, factor)
                For j As Integer = k + 1 To std.Min(_n - 1, k + _upperBandwidth)
                    SetElement(i, j, GetElement(i, j) - factor * GetElement(k, j))
                Next
            Next
        Next

        _isFactored = True
        Return LinearSolverResult.Success
    End Function

#End Region

#Region "求解"

    ''' <summary>
    ''' 求解带状线性方程组
    ''' </summary>
    Public Function Solve(b As NVector, x As NVector) As LinearSolverResult
        If Not _isFactored Then
            Throw New InvalidOperationException("必须先进行LU分解")
        End If

        x.CopyFrom(b)

        ' 应用置换
        For k As Integer = 0 To _n - 1
            Dim pivot As Integer = _pivots(k)
            If pivot <> k Then
                Dim temp As Double = x(k)
                x(k) = x(pivot)
                x(pivot) = temp
            End If
        Next

        ' 前代
        For i As Integer = 1 To _n - 1
            Dim sum As Double = x(i)
            Dim start As Integer = std.Max(0, i - _lowerBandwidth)
            For j As Integer = start To i - 1
                sum -= GetElement(i, j) * x(j)
            Next
            x(i) = sum
        Next

        ' 回代
        For i As Integer = _n - 1 To 0 Step -1
            Dim sum As Double = x(i)
            Dim [end] As Integer = std.Min(_n - 1, i + _upperBandwidth)
            For j As Integer = i + 1 To [end]
                sum -= GetElement(i, j) * x(j)
            Next
            x(i) = sum / GetElement(i, i)
        Next

        Return LinearSolverResult.Success
    End Function

#End Region

End Class


