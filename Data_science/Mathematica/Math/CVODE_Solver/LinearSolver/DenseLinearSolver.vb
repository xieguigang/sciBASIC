
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
