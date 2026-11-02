Imports std = System.Math

Namespace Kernel.BayesianBeliefNetwork

    ''' <summary>
    ''' 贝叶斯网络计算所需要的数值工具集合。所有的概率连乘(条件概率表、Dirichlet-多项边际似然、
    ''' 以及 Matrix-Tree 定理之中的生成树求和)都在对数空间之中完成，以避免 21 个残基字母表、
    ''' 数百个比对列的条件下出现下溢或者溢出。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 本模块刻意不依赖任何外部数值库：<see cref="LogGamma"/> 使用 Lanczos 近似，
    ''' <see cref="LogDet"/> 使用带部分主元的 LU 分解，两者都精确到双精度浮点数的
    ''' 有效数字范围之内，因此可以作为测试工程之中的对照基准。
    ''' </para>
    ''' <para>
    ''' 约定：<see cref="LogZero"/> 表示概率 0(即对数负无穷)，<see cref="LogOne"/> 表示概率 1。
    ''' </para>
    ''' </remarks>
    Public Module NumericLogSpace

        ''' <summary>
        ''' 概率 0 的对数值，即负无穷
        ''' </summary>
        Public ReadOnly LogZero As Double = Double.NegativeInfinity
        ''' <summary>
        ''' 概率 1 的对数值，即 0
        ''' </summary>
        Public ReadOnly LogOne As Double = 0.0

        ''' <summary>
        ''' 判断一个对数值是否代表一个有效的、非零的概率
        ''' </summary>
        Public Function IsFiniteLog(logValue As Double) As Boolean
            Return Not (Double.IsNegativeInfinity(logValue) OrElse Double.IsNaN(logValue))
        End Function

#Region "LogGamma (Lanczos approximation, g = 7, n = 9)"

        Private ReadOnly LanczosCoefficients As Double() = {
            0.99999999999980993,
            676.5203681218851,
            -1259.1392167224028,
            771.32342877765313,
            -176.61502916214059,
            12.507343278686905,
            -0.13857109526572012,
            9.9843695780195716E-06,
            1.5056327351493116E-07
        }

        Private Const LanczosG As Double = 7.0

        ''' <summary>
        ''' 对数 Gamma 函数 <c>ln Γ(x)</c>，使用 Lanczos 近似实现。
        ''' </summary>
        ''' <param name="x">自变量，必须为大于 0 的实数</param>
        ''' <returns>对数 Gamma 函数值</returns>
        ''' <remarks>
        ''' 当 <c>x &lt; 0.5</c> 的时候使用反射公式 <c>Γ(x)Γ(1-x) = π / sin(πx)</c> 转换到
        ''' <c>[0.5, 1.5]</c> 区间之后再求值，这样可以保证 Lanczos 级数的收敛性。
        ''' </remarks>
        Public Function LogGamma(x As Double) As Double
            If Double.IsNaN(x) OrElse x <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(x), $"LogGamma is undefined for x={x}")
            End If

            If x < 0.5 Then
                ' Γ(x) = π / (sin(πx) · Γ(1-x))
                Return std.Log(std.PI / std.Sin(std.PI * x)) - LogGamma(1.0 - x)
            End If

            Dim z As Double = x - 1.0
            Dim series As Double = LanczosCoefficients(0)

            For i As Integer = 1 To LanczosCoefficients.Length - 1
                series += LanczosCoefficients(i) / (z + i)
            Next

            Dim t As Double = z + LanczosG + 0.5

            Return 0.5 * std.Log(2.0 * std.PI) + (z + 0.5) * std.Log(t) - t + std.Log(series)
        End Function

        ''' <summary>
        ''' Gamma 函数本身，由 <see cref="LogGamma"/> 指数还原
        ''' </summary>
        Public Function GammaOf(x As Double) As Double
            Return std.Exp(LogGamma(x))
        End Function

#End Region

#Region "LogSumExp"

        ''' <summary>
        ''' 稳定地计算 <c>ln(e^a + e^b)</c>，当两个参数都是负无穷的时候返回负无穷
        ''' </summary>
        Public Function LogSumExp(a As Double, b As Double) As Double
            If Double.IsNegativeInfinity(a) Then
                Return b
            ElseIf Double.IsNegativeInfinity(b) Then
                Return a
            End If

            Dim max As Double = If(a > b, a, b)
            Return max + std.Log(std.Exp(a - max) + std.Exp(b - max))
        End Function

        ''' <summary>
        ''' 稳定地计算 <c>ln Σ e^(values_i)</c>
        ''' </summary>
        Public Function LogSumExp(values As IEnumerable(Of Double)) As Double
            Dim max As Double = Double.NegativeInfinity
            Dim count As Integer = 0

            For Each v As Double In values
                If Double.IsNaN(v) Then
                    Return Double.NaN
                End If
                If v > max Then
                    max = v
                End If
                count += 1
            Next

            If count = 0 OrElse Double.IsNegativeInfinity(max) Then
                Return Double.NegativeInfinity
            End If

            Dim sum As Double = 0.0

            For Each v As Double In values
                If Not Double.IsNegativeInfinity(v) Then
                    sum += std.Exp(v - max)
                End If
            Next

            Return max + std.Log(sum)
        End Function

        ''' <summary>
        ''' 稳定地计算 <c>ln Σ e^(values_i)</c>
        ''' </summary>
        Public Function LogSumExp(values As Double(), Optional count As Integer = -1) As Double
            If values Is Nothing OrElse values.Length = 0 Then
                Return Double.NegativeInfinity
            End If

            Dim n As Integer = If(count < 0, values.Length, count)
            Dim max As Double = Double.NegativeInfinity

            For i As Integer = 0 To n - 1
                If Double.IsNaN(values(i)) Then
                    Return Double.NaN
                End If
                If values(i) > max Then
                    max = values(i)
                End If
            Next

            If Double.IsNegativeInfinity(max) Then
                Return Double.NegativeInfinity
            End If

            Dim sum As Double = 0.0

            For i As Integer = 0 To n - 1
                If Not Double.IsNegativeInfinity(values(i)) Then
                    sum += std.Exp(values(i) - max)
                End If
            Next

            Return max + std.Log(sum)
        End Function

#End Region

#Region "概率表 <-> 对数表"

        ''' <summary>
        ''' 把一组概率转换为对数概率
        ''' </summary>
        Public Function Log(probabilities As Double()) As Double()
            Dim result As Double() = New Double(probabilities.Length - 1) {}

            For i As Integer = 0 To probabilities.Length - 1
                result(i) = If(probabilities(i) <= 0, Double.NegativeInfinity, std.Log(probabilities(i)))
            Next

            Return result
        End Function

        ''' <summary>
        ''' 把一组对数概率归一化为概率分布(和式为 1)
        ''' </summary>
        ''' <remarks>
        ''' 如果全部元素都是负无穷(即所有事件的概率都为 0)则返回全 0 的数组，
        ''' 调用者可以通过检查返回值的和式是否为 0 来识别这种退化情形。
        ''' </remarks>
        Public Function NormalizeLog(logValues As Double()) As Double()
            Dim z As Double = LogSumExp(logValues)
            Dim result As Double() = New Double(logValues.Length - 1) {}

            If Double.IsNegativeInfinity(z) Then
                Return result
            End If

            For i As Integer = 0 To logValues.Length - 1
                If Not Double.IsNegativeInfinity(logValues(i)) Then
                    result(i) = std.Exp(logValues(i) - z)
                End If
            Next

            Return result
        End Function

        ''' <summary>
        ''' 归一化一组对数概率，同时保留对数形式
        ''' </summary>
        Public Function NormalizeLogSpace(logValues As Double()) As Double()
            Dim z As Double = LogSumExp(logValues)
            Dim result As Double() = New Double(logValues.Length - 1) {}

            If Double.IsNegativeInfinity(z) Then
                Return result
            End If

            For i As Integer = 0 To logValues.Length - 1
                result(i) = logValues(i) - z
            Next

            Return result
        End Function

#End Region

#Region "LogDet"

        ''' <summary>
        ''' 计算方阵行列式的对数值 <c>ln|det(A)|</c>，使用带部分主元的高斯消元。
        ''' </summary>
        ''' <param name="matrix">方阵，函数内部会复制一份，不会修改传入的数组</param>
        ''' <returns>行列式绝对值的对数；矩阵奇异的时候返回 <see cref="LogZero"/></returns>
        ''' <remarks>
        ''' 在 Matrix-Tree 定理之中我们需要的是 <c>det(M)</c>，其中 M 是由 <c>R_ij</c> 构造的
        ''' Laplacian 矩阵去掉一行一列之后的结果。由于 R_ij 恒为正数，M 是对称正定的，
        ''' 因此这里的部分主元消元在数值上是稳定的；对数形式则保证了几百阶矩阵不会溢出。
        ''' </remarks>
        Public Function LogDet(matrix As Double(,)) As Double
            Dim n As Integer = matrix.GetLength(0)

            If n <> matrix.GetLength(1) Then
                Throw New ArgumentException("The matrix must be square.", NameOf(matrix))
            End If
            If n = 0 Then
                Return 0.0
            End If

            Dim a As Double(,) = DirectCast(matrix.Clone(), Double(,))
            Dim logAbsDet As Double = 0.0

            For k As Integer = 0 To n - 1
                ' partial pivoting
                Dim pivot As Integer = k
                Dim maxAbs As Double = std.Abs(a(k, k))

                For i As Integer = k + 1 To n - 1
                    Dim v As Double = std.Abs(a(i, k))

                    If v > maxAbs Then
                        maxAbs = v
                        pivot = i
                    End If
                Next

                If maxAbs <= 1.0E-300 Then
                    Return Double.NegativeInfinity
                End If

                If pivot <> k Then
                    For j As Integer = k To n - 1
                        Dim t As Double = a(k, j)
                        a(k, j) = a(pivot, j)
                        a(pivot, j) = t
                    Next
                End If

                Dim diagonal As Double = a(k, k)
                logAbsDet += std.Log(std.Abs(diagonal))

                For i As Integer = k + 1 To n - 1
                    Dim factor As Double = a(i, k) / diagonal

                    If factor = 0.0 Then
                        Continue For
                    End If

                    For j As Integer = k + 1 To n - 1
                        a(i, j) -= factor * a(k, j)
                    Next
                Next
            Next

            Return logAbsDet
        End Function

        ''' <summary>
        ''' 对<b>对称正定</b>矩阵计算对数行列式，使用 Cholesky 分解。
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' 带主元的高斯消元在对角占优程度差异很大的矩阵之上会产生严重的相消误差，
        ''' 而 Laplacian 矩阵去掉一行一列之后是一个对称正定矩阵，用 Cholesky 分解
        ''' <c>A = L Lᵀ</c> 可以得到 <c>ln det(A) = 2 Σ ln L_ii</c>，
        ''' 全程只有正数的开方与除法，数值上远比 LU 稳定。
        ''' </para>
        ''' <para>
        ''' 矩阵非正定(或者数值上奇异)的时候返回负无穷。
        ''' </para>
        ''' </remarks>
        Public Function LogDetSpd(matrix As Double(,)) As Double
            Dim n As Integer = matrix.GetLength(0)

            If n <> matrix.GetLength(1) Then
                Throw New ArgumentException("The matrix must be square.", NameOf(matrix))
            End If
            If n = 0 Then
                Return 0.0
            End If

            Dim lower As Double(,) = New Double(n - 1, n - 1) {}

            For i As Integer = 0 To n - 1
                For j As Integer = 0 To i
                    Dim sum As Double = matrix(i, j)

                    For k As Integer = 0 To j - 1
                        sum -= lower(i, k) * lower(j, k)
                    Next

                    If i = j Then
                        If sum <= 0 OrElse Double.IsNaN(sum) Then
                            Return Double.NegativeInfinity
                        End If

                        lower(i, i) = std.Sqrt(sum)
                    Else
                        If lower(j, j) = 0 Then
                            Return Double.NegativeInfinity
                        End If

                        lower(i, j) = sum / lower(j, j)
                    End If
                Next
            Next

            Dim logDeterminant As Double = 0.0

            For i As Integer = 0 To n - 1
                If lower(i, i) <= 0 Then
                    Return Double.NegativeInfinity
                End If

                logDeterminant += 2.0 * std.Log(lower(i, i))
            Next

            Return logDeterminant
        End Function

#End Region
    End Module
End Namespace
