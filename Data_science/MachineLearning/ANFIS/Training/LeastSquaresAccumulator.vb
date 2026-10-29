Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Solvers
Imports SIMDIntrinsics = Microsoft.VisualBasic.Math.SIMD.SIMDIntrinsics
Imports std = System.Math

''' <summary>
''' 分块累加正规方程 <c>A'A</c> 与 <c>A'y</c>，然后用 Cholesky 求解结论参数。
''' </summary>
''' <remarks>
''' <para>
''' 之所以要有这个"累加器"而不是直接构造整张设计矩阵再调 <c>OLS</c>：
''' ANFIS 的设计矩阵是 <c>[N, R*(n+1)]</c>，N 大一点就会占用几十 MB 到上百 MB，
''' 而真正需要的只是 <c>P x P</c> 的正规方程矩阵。分块后峰值内存从
''' <c>O(N*P)</c> 降到 <c>O(chunk*P + P^2)</c>。
''' </para>
''' <para>
''' <c>A'A</c> 只累加上三角，求解时再镜像到下半 —— 这不是为了省一半计算，
''' 而是因为 <c>CholeskyDecomposition</c> 的对称性判据用的是**精确比较**
''' （<c>A(k)(j) = A(j)(k)</c>），浮点累加出来的两半会有 ULP 级差异，
''' 直接拿非精确对称的矩阵去分解会被判成"非正定"。
''' </para>
''' </remarks>
Public Class LeastSquaresAccumulator

    Private ReadOnly _parameterCount As Integer
    Private ReadOnly _outputCount As Integer
    Private ReadOnly _xtx As Double(,)
    Private ReadOnly _xty As Double(,)

    Private _rows As Long
    Private _columnBuffer As Double()()
    Private _columnBufferRows As Integer

    ''' <summary>创建累加器。</summary>
    ''' <param name="parameterCount">设计矩阵的列数 P</param>
    ''' <param name="outputCount">输出维数</param>
    ''' <param name="ridge">相对岭项系数（实际岭项 = ridge * tr(A'A) / P）</param>
    Public Sub New(parameterCount As Integer, outputCount As Integer, Optional ridge As Double = 0.000001)
        If parameterCount <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(parameterCount), parameterCount, "the parameter count must be positive")
        End If
        If outputCount <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(outputCount), outputCount, "the output count must be positive")
        End If
        If ridge < 0.0 Then
            Throw New ArgumentOutOfRangeException(NameOf(ridge), ridge, "the ridge factor must not be negative")
        End If

        _parameterCount = parameterCount
        _outputCount = outputCount
        Me.Ridge = ridge
        _xtx = New Double(parameterCount - 1, parameterCount - 1) {}
        _xty = New Double(parameterCount - 1, outputCount - 1) {}
    End Sub

    ''' <summary>相对岭项系数。</summary>
    Public ReadOnly Property Ridge As Double

    ''' <summary>已经累加的样本行数。</summary>
    Public ReadOnly Property RowCount As Long
        Get
            Return _rows
        End Get
    End Property

    ''' <summary>清空累加状态（求解失败重来、或者换一批数据时使用）。</summary>
    Public Sub Reset()
        Call Array.Clear(_xtx, 0, _xtx.Length)
        Call Array.Clear(_xty, 0, _xty.Length)
        _rows = 0
    End Sub

    ''' <summary>
    ''' 累加一个数据块。
    ''' </summary>
    ''' <param name="a">设计矩阵块，形状 <c>[rows, P]</c></param>
    ''' <param name="y">观测值块，形状 <c>[rows, OutputCount]</c></param>
    Public Sub AddChunk(a As Double(,), y As Double(,))
        Call AddChunk(a, y, If(a Is Nothing, 0, a.GetLength(0)))
    End Sub

    ''' <summary>
    ''' 累加一个数据块，只使用前 <paramref name="rows"/> 行（用于复用满容量的设计矩阵缓冲）。
    ''' </summary>
    ''' <param name="a">设计矩阵块，形状 <c>[&gt;= rows, P]</c>；超出 <paramref name="rows"/> 的行不会被读</param>
    ''' <param name="y">观测值块，形状 <c>[&gt;= rows, OutputCount]</c>；同样只读前 <paramref name="rows"/> 行</param>
    ''' <param name="rows">本块的有效行数</param>
    Public Sub AddChunk(a As Double(,), y As Double(,), rows As Integer)
        If a Is Nothing Then
            Throw New ArgumentNullException(NameOf(a))
        End If
        If y Is Nothing Then
            Throw New ArgumentNullException(NameOf(y))
        End If

        Dim p As Integer = _parameterCount
        Dim o As Integer = _outputCount

        If a.GetLength(1) <> p Then
            Throw New ArgumentException($"the design matrix must have {p} columns", NameOf(a))
        End If
        If rows < 0 OrElse rows > a.GetLength(0) OrElse rows > y.GetLength(0) OrElse y.GetLength(1) <> o Then
            Throw New ArgumentException($"the target must be ({rows}, {o})", NameOf(y))
        End If
        If rows = 0 Then
            Return
        End If

        ' ---- A'y ----
        For c As Integer = 0 To p - 1
            For k As Integer = 0 To o - 1
                Dim sum As Double = 0.0

                For t As Integer = 0 To rows - 1
                    sum += a(t, c) * y(t, k)
                Next

                _xty(c, k) += sum
            Next
        Next

        ' ---- A'A 的上三角：转置成列缓冲后用 SIMD 点积 ----
        EnsureColumnBuffer(rows)

        For c As Integer = 0 To p - 1
            Dim buffer As Double() = _columnBuffer(c)

            For t As Integer = 0 To rows - 1
                buffer(t) = a(t, c)
            Next
        Next

        For left As Integer = 0 To p - 1
            Dim rowLeft As Double() = _columnBuffer(left)

            For right As Integer = left To p - 1
                _xtx(left, right) += SIMDIntrinsics.DotFma(rowLeft, _columnBuffer(right))
            Next
        Next

        _rows += rows
    End Sub

    Private Sub EnsureColumnBuffer(rows As Integer)
        If _columnBuffer IsNot Nothing AndAlso _columnBufferRows >= rows Then
            ' 复用更大容量的缓冲：把超出本块的部分清零。
            ' 下面的 A'A 是对**整个缓冲长度**做点积（DotFma 不带长度参数），
            ' 残留的旧值会污染结果。
            If _columnBufferRows > rows Then
                For c As Integer = 0 To _columnBuffer.Length - 1
                    Call Array.Clear(_columnBuffer(c), rows, _columnBufferRows - rows)
                Next
            End If

            Return
        End If

        Dim buffer(_parameterCount - 1)() As Double

        For c As Integer = 0 To _parameterCount - 1
            buffer(c) = New Double(rows - 1) {}
        Next

        _columnBuffer = buffer
        _columnBufferRows = rows
    End Sub

    ''' <summary>
    ''' 求解 <c>(A'A + lambda*I) * theta = A'y</c>。
    ''' </summary>
    ''' <param name="warning">求解过程中的降级说明（无降级时为空串）</param>
    ''' <returns>解矩阵 <c>[P, OutputCount]</c>；彻底失败时返回 <c>Nothing</c></returns>
    ''' <remarks>
    ''' 降级顺序：加大岭项重试 → 高斯消元（<see cref="OLS.Solve(Double(,), Double())"/>）→ 放弃并返回
    ''' <c>Nothing</c>（调用方保留上一轮的结论参数，绝不返回 NaN）。
    ''' </remarks>
    Public Function Solve(ByRef warning As String) As Double(,)
        warning = ""

        If _rows = 0 Then
            warning = "the least squares accumulator has no sample"
            Return Nothing
        End If

        Dim p As Integer = _parameterCount
        Dim o As Integer = _outputCount
        Dim trace As Double = 0.0

        For i As Integer = 0 To p - 1
            trace += _xtx(i, i)
        Next

        Dim lambda As Double

        If trace > 0.0 Then
            lambda = Ridge * trace / p
        Else
            lambda = If(Ridge > 0.0, Ridge, 0.00000001)
        End If

        For attempt As Integer = 0 To 6
            Dim solved As Double(,) = TryCholesky(p, o, lambda)

            If solved IsNot Nothing Then
                Return solved
            End If

            If attempt < 6 Then
                lambda = If(lambda > 0.0, lambda * 10.0, 0.000001)
            End If
        Next

        ' 最后兜底：高斯消元（部分主元），对条件数更宽容但数值精度略差
        Dim fallback As Double(,) = TryGaussian(p, o, If(lambda > 0.0, lambda, 0.00000001))

        If fallback IsNot Nothing Then
            warning = "the normal equations were not positive definite; fell back to Gaussian elimination"
            Return fallback
        End If

        warning = "the least squares system is singular; the previous consequent parameters were kept"
        Return Nothing
    End Function

    Private Function TryCholesky(p As Integer, o As Integer, lambda As Double) As Double(,)
        Try
            Dim m(p - 1)() As Double

            For i As Integer = 0 To p - 1
                Dim row(p - 1) As Double

                For j As Integer = i To p - 1
                    Dim v As Double = _xtx(i, j)

                    If i = j Then
                        v += lambda
                    End If

                    row(j) = v
                Next

                m(i) = row
            Next

            ' 上三角已填好，镜像到下半（精确对称，满足 Cholesky 的对称判据）
            For i As Integer = 0 To p - 1
                For j As Integer = 0 To i - 1
                    m(i)(j) = m(j)(i)
                Next
            Next

            Dim rhs(p - 1)() As Double

            For i As Integer = 0 To p - 1
                Dim row(o - 1) As Double

                For k As Integer = 0 To o - 1
                    row(k) = _xty(i, k)
                Next

                rhs(i) = row
            Next

            Dim factor As NumericMatrix = New NumericMatrix(m)
            Dim chol As CholeskyDecomposition = factor.chol()

            If Not chol.SPD Then
                Return Nothing
            End If

            Dim rhsMatrix As New NumericMatrix(rhs)
            Dim sol As NumericMatrix = CType(chol.Solve(rhsMatrix), NumericMatrix)
            Dim theta(p - 1, o - 1) As Double

            For i As Integer = 0 To p - 1
                For k As Integer = 0 To o - 1
                    Dim v As Double = sol(i, k)

                    If Double.IsNaN(v) OrElse Double.IsInfinity(v) Then
                        Return Nothing
                    End If

                    theta(i, k) = v
                Next
            Next

            Return theta
        Catch ex As Exception
            ' Cholesky 抛异常（非 SPD / 维度不符）时交给上层降级
            Return Nothing
        End Try
    End Function

    Private Function TryGaussian(p As Integer, o As Integer, lambda As Double) As Double(,)
        Try
            Dim m(p - 1, p - 1) As Double

            For i As Integer = 0 To p - 1
                For j As Integer = i To p - 1
                    Dim v As Double = _xtx(i, j)

                    If i = j Then
                        v += lambda
                    End If

                    m(i, j) = v
                    m(j, i) = v
                Next
            Next

            Dim theta(p - 1, o - 1) As Double

            For k As Integer = 0 To o - 1
                Dim b(p - 1) As Double

                For i As Integer = 0 To p - 1
                    b(i) = _xty(i, k)
                Next

                Dim solution As Double() = OLS.Solve(m, b)

                For i As Integer = 0 To p - 1
                    If Double.IsNaN(solution(i)) OrElse Double.IsInfinity(solution(i)) Then
                        Return Nothing
                    End If

                    theta(i, k) = solution(i)
                Next
            Next

            Return theta
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    ''' <summary>对角占优程度的粗略指标 <c>max|diag| / min|diag|</c>，用于诊断病态程度。</summary>
    Public Function ConditionEstimate() As Double
        Dim maxValue As Double = 0.0
        Dim minValue As Double = Double.MaxValue

        For i As Integer = 0 To _parameterCount - 1
            Dim v As Double = std.Abs(_xtx(i, i))

            If v > maxValue Then
                maxValue = v
            End If
            If v < minValue Then
                minValue = v
            End If
        Next

        If minValue <= 0.0 Then
            Return Double.PositiveInfinity
        Else
            Return maxValue / minValue
        End If
    End Function

End Class
