Option Strict On
Option Explicit On

Imports System
Imports Tensor = Global.Microsoft.VisualBasic.MachineLearning.TensorFlow.Tensor
Imports std = System.Math

''' <summary>
''' 一次批量前向传播的全部中间量（五层网络的激活值）。
''' </summary>
''' <remarks>
''' 全部按"行 = 样本"存放，便于按行块切分做最小二乘累加，也便于直接看某一条样本
''' 的隶属度与规则触发强度（可解释性诊断）。
''' </remarks>
Public Class AnfisForward

    ''' <summary>本批次在原始数据中的起始行号。</summary>
    Public Property RowStart As Integer
    ''' <summary>本批次的样本数。</summary>
    Public Property SampleCount As Integer
    ''' <summary>输入维数 n。</summary>
    Public Property InputCount As Integer
    ''' <summary>规则数 R。</summary>
    Public Property RuleCount As Integer
    ''' <summary>输出维数。</summary>
    Public Property OutputCount As Integer

    ''' <summary>输入矩阵，形状 <c>[SampleCount, InputCount]</c>。</summary>
    Public Property X As Double(,)
    ''' <summary>每个输入的隶属度，形状 <c>[SampleCount, m_i]</c>（第 1 层）。</summary>
    Public Property Mu As Double()(,)
    ''' <summary>每个输入的隶属度对数（乘积在对数域累加，避免下溢）。</summary>
    Public Property LogMu As Double()(,)
    ''' <summary>规则触发强度 w，形状 <c>[SampleCount, RuleCount]</c>（第 2 层）。</summary>
    Public Property W As Double(,)
    ''' <summary>归一化触发强度 <c>w-bar</c>，形状 <c>[SampleCount, RuleCount]</c>（第 3 层）。</summary>
    Public Property Wbar As Double(,)
    ''' <summary>每行的触发强度之和 S。</summary>
    Public Property SumW As Double()
    ''' <summary>每行是否有效（S 未下溢到 0）。</summary>
    Public Property Valid As Boolean()
    ''' <summary>每条规则的线性结论值 <c>f_r</c>，按输出各存一份，形状 <c>[SampleCount, RuleCount]</c>（第 4 层）。</summary>
    Public Property F As Double()(,)
    ''' <summary>网络输出，形状 <c>[SampleCount, OutputCount]</c>（第 5 层）。</summary>
    Public Property Z As Double(,)

End Class

''' <summary>
''' 一次批量反向传播得到的梯度：前提参数（扁平向量）、结论参数矩阵与误差统计。
''' </summary>
Public Class AnfisGradient

    ''' <summary>前提参数梯度，布局与 <see cref="AnfisNetwork.GetPremiseParameters"/> 完全一致。</summary>
    Public Property Premise As Double()
    ''' <summary>结论参数梯度，形状 <c>[ConsequentParameterCount, OutputCount]</c>。</summary>
    Public Property Theta As Double(,)
    ''' <summary>本批次的平方误差和 <c>sum((z-y)^2)</c>。</summary>
    Public Property SquaredError As Double
    ''' <summary>本批次包含的样本数。</summary>
    Public Property SampleCount As Integer
    ''' <summary>每条规则的归一化触发强度之和（用于剪枝与可解释性诊断）。</summary>
    Public Property RuleActivation As Double()

    ''' <summary>按网络的参数布局创建零梯度。</summary>
    Public Sub New(network As AnfisNetwork)
        Premise = New Double(network.PremiseParameterCount - 1) {}
        Theta = New Double(network.ConsequentParameterCount - 1, network.OutputCount - 1) {}
        RuleActivation = New Double(network.Rules.RuleCount - 1) {}
    End Sub

End Class

''' <summary>
''' ANFIS 五层网络的前向与解析反向传播。
''' </summary>
''' <remarks>
''' <para>
''' 前向与反向都按行块工作（<see cref="AnfisForward"/>），训练器因此可以做到
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
Public Class AnfisLayers

    ''' <summary>触发强度之和的下限：低于它认为该样本的规则全部失活。</summary>
    Public Const MinimumActivationSum As Double = 0.000000000001

    ''' <summary>对数域的下溢截断：exp(-700) 已经接近 Double 的最小正规数。</summary>
    Private Const LogUnderflow As Double = -700.0

    Private ReadOnly _net As AnfisNetwork

    ''' <summary>绑定到某个网络结构（结论参数按引用读取，训练中就地更新即可生效）。</summary>
    Public Sub New(network As AnfisNetwork)
        If network Is Nothing Then
            Throw New ArgumentNullException(NameOf(network))
        End If

        _net = network
    End Sub

#Region "前向"

    ''' <summary>对 <paramref name="x"/> 的第 <c>[rowStart, rowStart+rowCount)</c> 行执行五层前向。</summary>
    Public Function Forward(x As Double()(), rowStart As Integer, rowCount As Integer) As AnfisForward
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

        Dim fwd As New AnfisForward With {
            .RowStart = rowStart,
            .SampleCount = rowCount,
            .InputCount = n,
            .RuleCount = rCount,
            .OutputCount = oCount
        }

        ' ---- 输入拷贝（后续所有计算都在连续的行主序数组上进行）----
        Dim X(rowCount - 1, n - 1) As Double

        For t As Integer = 0 To rowCount - 1
            Dim row As Double() = x(rowStart + t)

            If row Is Nothing OrElse row.Length <> n Then
                Throw New ArgumentException($"the sample {rowStart + t} has {If(row Is Nothing, 0, row.Length)} features but the network expects {n}", NameOf(x))
            End If

            For i As Integer = 0 To n - 1
                X(t, i) = row(i)
            Next
        Next

        fwd.X = X

        ' ---- 第 1 层：模糊化 ----
        Dim muAll(n - 1) As Double(,)
        Dim logMuAll(n - 1) As Double(,)

        For i As Integer = 0 To n - 1
            Dim mfs As IMembershipFunction() = _net.Inputs(i).Functions
            Dim m As Integer = mfs.Length
            Dim mu(rowCount - 1, m - 1) As Double
            Dim logMu(rowCount - 1, m - 1) As Double

            For t As Integer = 0 To rowCount - 1
                Dim xi As Double = X(t, i)

                For k As Integer = 0 To m - 1
                    Dim v As Double = mfs(k).Evaluate(xi)
                    mu(t, k) = v
                    logMu(t, k) = If(v > 0.0, std.Log(v), Double.NegativeInfinity)
                Next
            Next

            muAll(i) = mu
            logMuAll(i) = logMu
        Next

        fwd.Mu = muAll
        fwd.LogMu = logMuAll

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
                    logW += logMuAll(i)(t, _net.Rules.Antecedent(r, i))
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
        Dim F(oCount - 1) As Double(,)
        Dim pt(n - 1, rCount - 1) As Double
        Dim theta As Double(,) = _net.Theta

        For o As Integer = 0 To oCount - 1
            For r As Integer = 0 To rCount - 1
                Dim offset As Integer = r * (n + 1)

                For i As Integer = 0 To n - 1
                    pt(i, r) = theta(offset + i, o)
                Next
            Next

            ' f = X * P' 走张量后端的矩阵乘（SIMD / 未来可切 CUDA）
            Dim prod As Double(,) = AnfisMatrix.MatMul(X, pt)

            For t As Integer = 0 To rowCount - 1
                For r As Integer = 0 To rCount - 1
                    prod(t, r) += theta(r * (n + 1) + n, o)
                Next
            Next

            F(o) = prod
        Next

        fwd.F = F

        ' ---- 第 5 层：加权求和 ----
        Dim Z(rowCount - 1, oCount - 1) As Double

        For t As Integer = 0 To rowCount - 1
            If Not valid(t) Then
                Continue For
            End If

            For o As Integer = 0 To oCount - 1
                Dim Fo As Double(,) = F(o)
                Dim acc As Double = 0.0

                For r As Integer = 0 To rCount - 1
                    acc += wbar(t, r) * Fo(t, r)
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
    ''' <paramref name="a"/> 必须已经清零；第 0 行对应 <see cref="AnfisForward.RowStart"/>。
    ''' </remarks>
    Public Sub BuildDesignMatrix(fwd As AnfisForward, a As Double(,))
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
    Public Function Accumulate(fwd As AnfisForward, y As Double(,), computeThetaGradient As Boolean) As AnfisGradient
        Dim sampleCount As Integer = fwd.SampleCount
        Dim n As Integer = fwd.InputCount
        Dim rCount As Integer = fwd.RuleCount
        Dim oCount As Integer = fwd.OutputCount

        If y Is Nothing OrElse y.GetLength(0) <> sampleCount OrElse y.GetLength(1) <> oCount Then
            Throw New ArgumentException($"the target must be ({sampleCount}, {oCount})", NameOf(y))
        End If

        Dim g As New AnfisGradient(_net) With {
            .SampleCount = sampleCount
        }

        Dim delta(sampleCount - 1, oCount - 1) As Double
        Dim coef(sampleCount - 1, rCount - 1) As Double
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
                    acc += delta(t, o) * (fwd.F(o)(t, r) - fwd.Z(t, o))
                Next

                coef(t, r) = wb * acc
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
                    If coef(t, r) <> 0.0 Then
                        column(t, _net.Rules.Antecedent(r, i)) += coef(t, r)
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

''' <summary>
''' 委托给 <c>TensorFlow</c> 后端的矩阵乘法包装。
''' </summary>
''' <remarks>
''' 这里刻意只用张量库的 <c>MatMul</c>：ANFIS 的前向里只有"输入 × 结论系数"这一步是
''' 真正的稠密矩阵乘，交给 <see cref="Tensor.computeKernel"/> 可以自动获得 SIMD 加速
''' （以及未来注册 CUDA 后端后的设备加速）。其余步骤形状不规则（按规则下标取列），
''' 用普通循环反而更清楚也更快。
''' </remarks>
Friend NotInheritable Class AnfisMatrix

    Private Sub New()
    End Sub

    ''' <summary>二维矩阵乘 <c>a(m,k) × b(k,n)</c>，结果按行主序返回。</summary>
    Public Shared Function MatMul(a As Double(,), b As Double(,)) As Double(,)
        Dim m As Integer = a.GetLength(0)
        Dim k As Integer = a.GetLength(1)
        Dim n As Integer = b.GetLength(1)
        Dim result(m - 1, n - 1) As Double

        If k = 0 Then
            Return result
        End If

        Dim ta As Tensor = Nothing
        Dim tb As Tensor = Nothing
        Dim tc As Tensor = Nothing

        Try
            ta = New Tensor(a)
            tb = New Tensor(b)
            tc = Tensor.computeKernel.MatMul(ta, tb)

            Dim data As Double() = tc.Data

            For i As Integer = 0 To m - 1
                Dim rowOffset As Integer = i * n

                For j As Integer = 0 To n - 1
                    result(i, j) = data(rowOffset + j)
                Next
            Next
        Finally
            If tc IsNot Nothing Then
                Call tc.Dispose()
            End If
            If tb IsNot Nothing Then
                Call tb.Dispose()
            End If
            If ta IsNot Nothing Then
                Call ta.Dispose()
            End If
        End Try

        Return result
    End Function

End Class
