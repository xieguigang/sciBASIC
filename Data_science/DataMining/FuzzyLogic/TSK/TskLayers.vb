Option Strict On
Option Explicit On

Imports System
Imports MatrixOps = Microsoft.VisualBasic.Math.LinearAlgebra.Matrix.MatrixOps
Imports std = System.Math

Namespace TSK

    ''' <summary>
    ''' TSK 五层网络的前向与解析反向传播。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 前向与反向都按行块工作（<see cref="TskInferenceTrace"/>），训练器因此可以做到
    ''' "不物化整张设计矩阵"：逐块前向 → 逐块累加 <c>A'A</c> / <c>A'y</c> 或逐块累加梯度。
    ''' </para>
    ''' <para>
    ''' 反向的关键是下面这个恒等式（<c>S = sum_r w_r</c>，<c>w-bar_r = w_r/S</c>）：
    ''' <code>
    '''   dE/dw_r = delta * (f_r - z) / S
    '''   dE/dp   = sum_r (dE/dw_r * w_r) * d(ln mu)/dp
    '''           = sum_r w-bar_r * delta * (f_r - z) * d(ln mu)/dp
    ''' </code>
    ''' 因为 <c>w_r/S = w-bar_r</c>，实现里**不需要**先算 <c>dE/dw</c> 再除以 <c>w_r</c> 或 <c>mu</c>，
    ''' 于是当某个隶属度下溢到 0 时梯度自然收敛到 0，而不会出现 0/0。
    ''' </para>
    ''' </remarks>
    Public Class TskLayers

        ''' <summary>触发强度之和的下限：低于它认为该样本的规则全部失活。</summary>
        Public Const MinimumActivationSum As Double = 0.000000000001

        ''' <summary>对数域的下溢截断：exp(-700) 已经接近 Double 的最小正规数。</summary>
        Private Const LogUnderflow As Double = -700.0

        Private ReadOnly _net As TskNetwork

        ''' <summary>绑定到某个网络结构（结论参数按引用读取，训练中就地更新即可生效）。</summary>
        Public Sub New(network As TskNetwork)
            If network Is Nothing Then
                Throw New ArgumentNullException(NameOf(network))
            End If

            _net = network
        End Sub

#Region "前向"

        ''' <summary>对 <paramref name="x"/> 的第 <c>[rowStart, rowStart+rowCount)</c> 行执行五层前向。</summary>
        Public Function Forward(x As Double()(), rowStart As Integer, rowCount As Integer) As TskInferenceTrace
            If x Is Nothing Then
                Throw New ArgumentNullException(NameOf(x))
            End If
            If rowCount <= 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(rowCount), rowCount, "the batch size must be positive")
            End If
            If rowStart < 0 OrElse rowStart + rowCount > x.Length Then
                Throw New ArgumentOutOfRangeException(NameOf(rowStart), $"the batch [{rowStart}, {rowStart + rowCount}) exceeds {x.Length} samples")
            End If

            Dim n As Integer = _net.Inputs.Length
            Dim rCount As Integer = _net.Rules.RuleCount
            Dim oCount As Integer = _net.OutputCount

            Dim fwd As New TskInferenceTrace With {
                .RowStart = rowStart,
                .SampleCount = rowCount,
                .InputCount = n,
                .RuleCount = rCount,
                .OutputCount = oCount
            }

            ' ---- 输入拷贝（后续所有计算都在连续的行主序数组上进行）----
            Dim xRows(rowCount - 1, n - 1) As Double

            For t As Integer = 0 To rowCount - 1
                Dim row As Double() = x(rowStart + t)

                If row Is Nothing OrElse row.Length <> n Then
                    Throw New ArgumentException(
                        $"the sample {rowStart + t} has {If(row Is Nothing, 0, row.Length)} features but the network expects {n}", NameOf(x))
                End If

                For i As Integer = 0 To n - 1
                    xRows(t, i) = row(i)
                Next
            Next

            fwd.X = xRows

            ' ---- 第 1 层：模糊化（所有输入的隶属度拼在一张表里，列偏移由 MfOffsets 给出）----
            Dim mfOffsets(n - 1) As Integer
            Dim totalMf As Integer = 0

            For i As Integer = 0 To n - 1
                mfOffsets(i) = totalMf
                totalMf += _net.Inputs(i).FunctionCount
            Next

            Dim mu(rowCount - 1, totalMf - 1) As Double
            Dim logMu(rowCount - 1, totalMf - 1) As Double

            For i As Integer = 0 To n - 1
                Dim mfs As IMembershipFunction() = _net.Inputs(i).Functions
                Dim m As Integer = mfs.Length
                Dim offset As Integer = mfOffsets(i)

                For t As Integer = 0 To rowCount - 1
                    Dim xi As Double = xRows(t, i)

                    For k As Integer = 0 To m - 1
                        Dim v As Double = mfs(k).Evaluate(xi)
                        mu(t, offset + k) = v
                        logMu(t, offset + k) = If(v > 0.0, std.Log(v), Double.NegativeInfinity)
                    Next
                Next
            Next

            fwd.Mu = mu
            fwd.LogMu = logMu
            fwd.MfOffsets = mfOffsets

            ' ---- 第 2、3 层：乘积 T-范数与归一化 ----
            Dim W(rowCount - 1, rCount - 1) As Double
            Dim wbar(rowCount - 1, rCount - 1) As Double
            Dim sumW(rowCount - 1) As Double
            Dim valid(rowCount - 1) As Boolean

            For t As Integer = 0 To rowCount - 1
                Dim total As Double = 0.0

                For r As Integer = 0 To rCount - 1
                    Dim logW As Double = 0.0

                    For i As Integer = 0 To n - 1
                        logW += logMu(t, mfOffsets(i) + _net.Rules.Antecedent(r, i))
                    Next

                    Dim wv As Double

                    If Double.IsNegativeInfinity(logW) OrElse logW < LogUnderflow Then
                        wv = 0.0
                    Else
                        wv = std.Exp(logW)
                    End If

                    W(t, r) = wv
                    total += wv
                Next

                sumW(t) = total

                If total > MinimumActivationSum Then
                    valid(t) = True

                    For r As Integer = 0 To rCount - 1
                        wbar(t, r) = W(t, r) / total
                    Next
                End If
            Next

            fwd.W = W
            fwd.Wbar = wbar
            fwd.SumW = sumW
            fwd.Valid = valid

            ' ---- 第 4 层：一阶 Sugeno 结论 f_r = x.p_r + q_r ----
            Dim fValues(rowCount - 1, rCount - 1, oCount - 1) As Double
            Dim pt(n - 1, rCount - 1) As Double
            Dim theta As Double(,) = _net.Theta

            For o As Integer = 0 To oCount - 1
                For r As Integer = 0 To rCount - 1
                    Dim offset As Integer = r * (n + 1)

                    For i As Integer = 0 To n - 1
                        pt(i, r) = theta(offset + i, o)
                    Next
                Next

                ' f = X * P'，稠密矩阵乘交给 Math 工程的行主序 FMA 点积内核
                Dim prod As Double(,) = MatrixOps.Multiply(xRows, pt)

                For t As Integer = 0 To rowCount - 1
                    For r As Integer = 0 To rCount - 1
                        fValues(t, r, o) = prod(t, r) + theta(r * (n + 1) + n, o)
                    Next
                Next
            Next

            fwd.F = fValues

            ' ---- 第 5 层：加权求和 ----
            Dim Z(rowCount - 1, oCount - 1) As Double

            For t As Integer = 0 To rowCount - 1
                If Not valid(t) Then
                    Continue For
                End If

                For o As Integer = 0 To oCount - 1
                    Dim acc As Double = 0.0

                    For r As Integer = 0 To rCount - 1
                        acc += wbar(t, r) * fValues(t, r, o)
                    Next

                    Z(t, o) = acc
                Next
            Next

            fwd.Z = Z

            Return fwd
        End Function

#End Region

#Region "设计矩阵与反向"

        ''' <summary>
        ''' 把本次前向的归一化触发强度展开成最小二乘的设计矩阵块。
        ''' </summary>
        ''' <remarks>
        ''' 第 <c>r</c> 条规则的参数块占据列 <c>[r*(n+1), r*(n+1)+n]</c>：
        ''' 前 n 列是 <c>w-bar_r * x_j</c>，最后一列是 <c>w-bar_r</c>（截距）。
        ''' <paramref name="a"/> 必须已经清零；第 0 行对应 <see cref="TskInferenceTrace.RowStart"/>。
        ''' </remarks>
        Public Sub BuildDesignMatrix(fwd As TskInferenceTrace, a As Double(,))
            Dim n As Integer = fwd.InputCount
            Dim rCount As Integer = fwd.RuleCount
            Dim p As Integer = _net.ConsequentParameterCount

            If a Is Nothing OrElse a.GetLength(0) <> fwd.SampleCount OrElse a.GetLength(1) <> p Then
                Throw New ArgumentException($"the design matrix must be ({fwd.SampleCount}, {p})", NameOf(a))
            End If

            For t As Integer = 0 To fwd.SampleCount - 1
                If Not fwd.Valid(t) Then
                    Continue For
                End If

                For r As Integer = 0 To rCount - 1
                    Dim wb As Double = fwd.Wbar(t, r)

                    If wb = 0.0 Then
                        Continue For
                    End If

                    Dim offset As Integer = r * (n + 1)

                    For j As Integer = 0 To n - 1
                        a(t, offset + j) = wb * fwd.X(t, j)
                    Next

                    a(t, offset + n) = wb
                Next
            Next
        End Sub

        ''' <summary>
        ''' 累加一次前向对应的梯度（前提参数 + 可选结论参数）。
        ''' </summary>
        ''' <param name="fwd">本次前向的中间量</param>
        ''' <param name="y">与 <paramref name="fwd"/> 行对齐的观测值，形状 <c>[SampleCount, OutputCount]</c></param>
        ''' <param name="computeThetaGradient">是否同时累加结论参数梯度（纯梯度下降模式需要）</param>
        Public Function Accumulate(fwd As TskInferenceTrace, y As Double(,), computeThetaGradient As Boolean) As TskGradient
            Dim sampleCount As Integer = fwd.SampleCount
            Dim n As Integer = fwd.InputCount
            Dim rCount As Integer = fwd.RuleCount
            Dim oCount As Integer = fwd.OutputCount

            If y Is Nothing OrElse y.GetLength(0) <> sampleCount OrElse y.GetLength(1) <> oCount Then
                Throw New ArgumentException($"the target must be ({sampleCount}, {oCount})", NameOf(y))
            End If

            Dim g As New TskGradient(_net) With {
                .SampleCount = sampleCount
            }

            Dim delta(sampleCount - 1, oCount - 1) As Double
            Dim coefficient(sampleCount - 1, rCount - 1) As Double
            Dim sse As Double = 0.0

            For t As Integer = 0 To sampleCount - 1
                For o As Integer = 0 To oCount - 1
                    Dim d As Double = fwd.Z(t, o) - y(t, o)
                    delta(t, o) = d
                    sse += d * d
                Next
            Next

            g.SquaredError = sse

            For t As Integer = 0 To sampleCount - 1
                For r As Integer = 0 To rCount - 1
                    Dim wb As Double = fwd.Wbar(t, r)

                    g.RuleActivation(r) += wb

                    If wb = 0.0 Then
                        Continue For
                    End If

                    Dim acc As Double = 0.0

                    For o As Integer = 0 To oCount - 1
                        acc += delta(t, o) * (fwd.F(t, r, o) - fwd.Z(t, o))
                    Next

                    coefficient(t, r) = wb * acc
                Next
            Next

            ' ---- 结论参数：dE/d(theta) = A' * delta ----
            If computeThetaGradient Then
                For t As Integer = 0 To sampleCount - 1
                    If Not fwd.Valid(t) Then
                        Continue For
                    End If

                    For r As Integer = 0 To rCount - 1
                        Dim wb As Double = fwd.Wbar(t, r)

                        If wb = 0.0 Then
                            Continue For
                        End If

                        Dim offset As Integer = r * (n + 1)

                        For j As Integer = 0 To n - 1
                            Dim xj As Double = wb * fwd.X(t, j)

                            For o As Integer = 0 To oCount - 1
                                g.Theta(offset + j, o) += xj * delta(t, o)
                            Next
                        Next

                        For o As Integer = 0 To oCount - 1
                            g.Theta(offset + n, o) += wb * delta(t, o)
                        Next
                    Next
                Next
            End If

            ' ---- 前提参数：sum_r w-bar_r * delta * (f_r - z) * d(ln mu)/dp ----
            For i As Integer = 0 To n - 1
                Dim mfs As IMembershipFunction() = _net.Inputs(i).Functions
                Dim m As Integer = mfs.Length
                Dim column(sampleCount - 1, m - 1) As Double

                For t As Integer = 0 To sampleCount - 1
                    For r As Integer = 0 To rCount - 1
                        If coefficient(t, r) <> 0.0 Then
                            column(t, _net.Rules.Antecedent(r, i)) += coefficient(t, r)
                        End If
                    Next
                Next

                Dim baseOffset As Integer = _net.PremiseOffsets(i)

                For k As Integer = 0 To m - 1
                    Dim mf As IMembershipFunction = mfs(k)
                    Dim pc As Integer = mf.ParameterCount
                    Dim logGrad(sampleCount - 1, pc - 1) As Double

                    For t As Integer = 0 To sampleCount - 1
                        If column(t, k) = 0.0 Then
                            Continue For
                        End If

                        Dim lg As Double() = mf.LogGradient(fwd.X(t, i))

                        For p As Integer = 0 To pc - 1
                            logGrad(t, p) = lg(p)
                        Next
                    Next

                    For p As Integer = 0 To pc - 1
                        Dim acc As Double = 0.0

                        For t As Integer = 0 To sampleCount - 1
                            acc += column(t, k) * logGrad(t, p)
                        Next

                        g.Premise(baseOffset + k * pc + p) += acc
                    Next
                Next
            Next

            Return g
        End Function

#End Region

    End Class

End Namespace
