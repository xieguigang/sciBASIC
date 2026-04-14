Imports std = System.Math

' ============================================================================
' LinearSolver.vb - 线性求解器
' Sundials CVODE求解器的线性代数求解模块
' 实现LU分解和前代/回代求解
' 仅基于.NET基础数学函数库实现，不依赖第三方库
' ============================================================================

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


