Imports Microsoft.VisualBasic.Linq
Imports std = System.Math

Namespace Distributions

    ''' <summary>
    ''' Skellam 分布：两个独立 Poisson 变量之差 ``X = Y1 - Y2`` 的分布
    ''' （``Y1 ~ Poisson(lambda1)``，``Y2 ~ Poisson(lambda2)``）。
    ''' 
    ''' 本模块移植自 R 语言的 ``skellam`` 包
    ''' （参见项目 ``Resources\pskellam.txt``、``Resources\dskellam.txt``、
    ''' ``Resources\dskellam.sp.txt``），并复用基础库的
    ''' 非中心卡方分布（<see cref="Microsoft.VisualBasic.Math.Distributions.ChiSquareDistribution"/>）
    ''' 与修正贝塞尔函数（<see cref="Microsoft.VisualBasic.Math.Distributions.BesselFunctions"/>）。
    ''' 
    ''' TSSAR 算法使用 <see cref="pskellam"/> 计算 dRNA-seq 中
    ''' [+] 与 [-] 文库同一位置 read 起始计数差的显著性 p 值。
    ''' </summary>
    Public Module Skellam

        ''' <summary>
        ''' 双精度浮点的最小正规格化数 ``.Machine$double.xmin``。
        ''' </summary>
        Public Const DoubleXMin As Double = 2.2250738585072014E-308

        ''' <summary>
        ''' 判断一个双精度值是否为有限值。
        ''' </summary>
        Private Function Finite(x As Double) As Boolean
            Return Not Double.IsNaN(x) AndAlso Not Double.IsInfinity(x)
        End Function

        ''' <summary>
        ''' Poisson 分布的概率质量函数 ``dpois(x, lambda, log.p)``。
        ''' </summary>
        ''' <param name="x">取值（自动向零取整）。</param>
        ''' <param name="lambda">Poisson 均值。</param>
        ''' <param name="logP">是否返回对数概率。</param>
        Public Function dpois(x As Double, lambda As Double, Optional logP As Boolean = False) As Double
            If lambda < 0.0 OrElse Double.IsNaN(lambda) Then
                Return Double.NaN
            End If

            Dim v As Double = std.Truncate(x)

            If v < 0.0 Then
                Return If(logP, Double.NegativeInfinity, 0.0)
            End If

            If lambda = 0.0 Then
                If v = 0.0 Then
                    Return If(logP, 0.0, 1.0)
                Else
                    Return If(logP, Double.NegativeInfinity, 0.0)
                End If
            End If

            Dim lp As Double = -lambda + v * std.Log(lambda) -
                               Microsoft.VisualBasic.Math.Distributions.MathGamma.lngamm(v + 1.0)

            Return If(logP, lp, std.Exp(lp))
        End Function

        ''' <summary>
        ''' 零膨胀 Poisson 分布（zero-inflated Poisson, ZIP）的概率质量函数，
        ''' 对应 VGAM 包的 ``dzipois(x, lambda, pstr0)``。
        ''' </summary>
        ''' <param name="x">取值。</param>
        ''' <param name="lambda">Poisson 均值。</param>
        ''' <param name="pstr0">结构零（structural zero）的概率 ``φ``。</param>
        ''' <param name="logP">是否返回对数概率。</param>
        ''' <remarks>
        ''' ``P(X = 0) = φ + (1 - φ)e^{-λ}``；
        ''' ``P(X = k) = (1 - φ)·Poisson(k; λ)``，``k &gt; 0``。
        ''' </remarks>
        Public Function dzipois(x As Double, lambda As Double, pstr0 As Double, Optional logP As Boolean = False) As Double
            If lambda < 0.0 OrElse pstr0 < 0.0 OrElse pstr0 > 1.0 Then
                Return Double.NaN
            End If

            Dim v As Double = std.Truncate(x)

            If v < 0.0 Then
                Return If(logP, Double.NegativeInfinity, 0.0)
            End If

            If v = 0.0 Then
                Dim p0 As Double = pstr0 + (1.0 - pstr0) * dpois(0.0, lambda)
                Return If(logP, std.Log(p0), p0)
            Else
                Dim d As Double = (1.0 - pstr0) * dpois(v, lambda)
                Return If(logP, std.Log(d), d)
            End If
        End Function

        ''' <summary>
        ''' Skellam 分布的概率质量函数 ``dskellam(x, lambda1, lambda2, log)``。
        ''' </summary>
        ''' <param name="x">整数取值。</param>
        ''' <param name="lambda1">第一个 Poisson 均值。</param>
        ''' <param name="lambda2">第二个 Poisson 均值；缺省时等于 <paramref name="lambda1"/>。</param>
        ''' <param name="logP">是否返回对数概率。</param>
        Public Function dskellam(x As Double, lambda1 As Double, Optional lambda2 As Double = Double.NaN, Optional logP As Boolean = False) As Double
            If Double.IsNaN(lambda2) Then
                lambda2 = lambda1
            End If

            If Not (Finite(lambda1) AndAlso lambda1 >= 0.0) OrElse
               Not (Finite(lambda2) AndAlso lambda2 >= 0.0) Then
                Return Double.NaN
            End If

            Dim xx As Double = std.Truncate(x)

            ' 零 lambda 的退化情形
            If lambda1 = 0.0 Then
                Return dpois(-xx, lambda2, logP)
            ElseIf lambda2 = 0.0 Then
                Return dpois(xx, lambda1, logP)
            End If

            Dim sqL12 As Double = std.Sqrt(lambda1 * lambda2)
            Dim ret As Double

            If logP Then
                ret = std.Log(Microsoft.VisualBasic.Math.Distributions.BesselFunctions.BesselI(2.0 * sqL12, std.Abs(xx), True)) +
                      2.0 * sqL12 - lambda1 - lambda2 + xx / 2.0 * std.Log(lambda1 / lambda2)
            Else
                ret = Microsoft.VisualBasic.Math.Distributions.BesselFunctions.BesselI(2.0 * sqL12, std.Abs(xx), True) *
                      std.Exp(2.0 * sqL12 - lambda1 - lambda2 + xx / 2.0 * std.Log(lambda1 / lambda2))
            End If

            ' 直接计算可能溢出/下溢，此时回退到鞍点近似
            If Not Finite(ret) OrElse (Not logP AndAlso ret < 1.0E-308) Then
                ret = dskellamSP(xx, lambda1, lambda2, logP)
            End If

            Return ret
        End Function

        ''' <summary>
        ''' Skellam 分布的累积分布函数 ``pskellam(q, lambda1, lambda2, lower.tail, log.p)``。
        ''' </summary>
        ''' <param name="q">分位点（自动向负无穷取整，即 ``floor``）。</param>
        ''' <param name="lambda1">第一个 Poisson 均值。</param>
        ''' <param name="lambda2">第二个 Poisson 均值；缺省时等于 <paramref name="lambda1"/>。</param>
        ''' <param name="lowerTail">``True`` 返回下尾 ``P(X &lt;= q)``，否则返回上尾 ``P(X &gt; q)``。</param>
        ''' <param name="logP">是否返回对数概率。</param>
        ''' <remarks>
        ''' 计算完全对应 R 的 ``pskellam``：
        ''' 
        ''' - ``q &lt; 0``：``pchisq(2λ2, -2q, 2λ1, lower.tail)``
        ''' - ``q &gt;= 0``：``pchisq(2λ1, 2(q+1), 2λ2, lower.tail = !lower.tail)``
        ''' </remarks>
        Public Function pskellam(q As Double,
                                 lambda1 As Double,
                                 Optional lambda2 As Double = Double.NaN,
                                 Optional lowerTail As Boolean = True,
                                 Optional logP As Boolean = False) As Double

            If Double.IsNaN(lambda2) Then
                lambda2 = lambda1
            End If

            If Double.IsNaN(q) OrElse Double.IsNaN(lambda1) OrElse Double.IsNaN(lambda2) Then
                Return Double.NaN
            End If
            If lambda1 < 0.0 OrElse lambda2 < 0.0 Then
                Return Double.NaN
            End If

            Dim x As Double = std.Floor(q)
            Dim ret As Double

            If x < 0.0 Then
                ' 张量核心使用非中心卡方分布
                ret = Microsoft.VisualBasic.Math.Distributions.ChiSquareDistribution.pchisq(
                    2.0 * lambda2, -2.0 * x, 2.0 * lambda1, lowerTail, logP)
            Else
                ret = Microsoft.VisualBasic.Math.Distributions.ChiSquareDistribution.pchisq(
                    2.0 * lambda1, 2.0 * (x + 1.0), 2.0 * lambda2, Not lowerTail, logP)
            End If

            ' 超出非中心卡方工作范围时回退到鞍点近似
            Dim outOfRange As Boolean = If(logP,
                                           Not Finite(ret),
                                           ret < 1.0E-308)

            If Not Double.IsNaN(ret) AndAlso outOfRange Then
                ret = pskellamSP(x, lambda1, lambda2, lowerTail, logP)
            End If

            Return ret
        End Function

        ''' <summary>
        ''' Skellam 概率质量函数的鞍点近似（saddlepoint approximation），
        ''' 对应 R 的 ``dskellam.sp``（``Resources\dskellam.sp.txt``）。
        ''' </summary>
        ''' <param name="x">整数取值。</param>
        ''' <param name="lambda1">第一个 Poisson 均值。</param>
        ''' <param name="lambda2">第二个 Poisson 均值。</param>
        ''' <param name="logP">是否返回对数概率。</param>
        Public Function dskellamSP(x As Double, lambda1 As Double, Optional lambda2 As Double = Double.NaN, Optional logP As Boolean = False) As Double
            If Double.IsNaN(lambda2) Then
                lambda2 = lambda1
            End If

            If lambda1 <= 0.0 OrElse lambda2 <= 0.0 Then
                Return dskellam(x, lambda1, lambda2, logP)
            End If

            Dim s As Double = std.Log(0.5 * (x + std.Sqrt(x * x + 4.0 * lambda1 * lambda2)) / lambda1)
            Dim expS As Double = std.Exp(s)
            Dim K As Double = lambda1 * (expS - 1.0) + lambda2 * (1.0 / expS - 1.0)
            Dim K2 As Double = lambda1 * expS + lambda2 / expS

            If K2 <= 0.0 Then
                Return If(logP, Double.NegativeInfinity, 0.0)
            End If

            Dim c As Double = (1.0 - ((lambda1 * expS - lambda2 / expS) / K2) ^ 2 * 5.0 / 3.0) / K2 * 0.125 + 1.0
            Dim logRet As Double = K - x * s - 0.5 * std.Log(2.0 * std.PI * K2) + std.Log((1.0 + c) * 0.5)

            Return If(logP, logRet, std.Exp(logRet))
        End Function

        ''' <summary>
        ''' Skellam 累积分布函数的鞍点近似（saddlepoint approximation）。
        ''' </summary>
        ''' <param name="q">已取整的分位点。</param>
        ''' <param name="lambda1">第一个 Poisson 均值。</param>
        ''' <param name="lambda2">第二个 Poisson 均值。</param>
        ''' <param name="lowerTail">是否返回下尾概率。</param>
        ''' <param name="logP">是否返回对数概率。</param>
        ''' <remarks>
        ''' 使用 Lugannani-Rice 公式（含格点连续性校正）。
        ''' 当两个 lambda 中存在 0 时退化为精确的 Poisson 尾概率。
        ''' 该分支仅在常规非中心卡方计算下溢（概率小于 ``1e-308``）时被调用，
        ''' 此时结果远低于 TSSAR 的任何判定阈值，因此对 TSS 判定没有实质影响。
        ''' </remarks>
        Public Function pskellamSP(q As Double,
                                   lambda1 As Double,
                                   lambda2 As Double,
                                   Optional lowerTail As Boolean = True,
                                   Optional logP As Boolean = False) As Double

            Dim x As Double = std.Floor(q)
            Dim lower As Double

            If lambda1 = 0.0 AndAlso lambda2 = 0.0 Then
                ' X 恒为 0
                lower = If(x >= 0.0, 1.0, 0.0)
            ElseIf lambda1 = 0.0 Then
                ' X = -Y, Y ~ Poisson(lambda2)；P(X <= x) = P(Y >= -x)
                Dim k As Double = -x
                lower = If(k <= 0.0, 1.0,
                           Microsoft.VisualBasic.Math.Distributions.ChiSquareDistribution.RegularizedGammaP(k, lambda2))
            ElseIf lambda2 = 0.0 Then
                ' X = Y ~ Poisson(lambda1)；P(X <= x) = P(Y <= x)
                lower = If(x < 0.0, 0.0,
                           Microsoft.VisualBasic.Math.Distributions.ChiSquareDistribution.RegularizedGammaQ(x + 1.0, lambda1))
            Else
                lower = SaddlepointCDF(x + 0.5, lambda1, lambda2)
            End If

            Dim p As Double = If(lowerTail, lower, 1.0 - lower)

            If p < 0.0 Then p = 0.0
            If p > 1.0 Then p = 1.0

            If logP Then
                If p = 0.0 Then Return Double.NegativeInfinity
                Return std.Log(p)
            End If

            Return p
        End Function

        ''' <summary>
        ''' Lugannani-Rice 鞍点近似的下尾累积分布。
        ''' </summary>
        ''' <param name="xc">带连续性校正的分位点。</param>
        ''' <param name="lambda1">第一个 Poisson 均值。</param>
        ''' <param name="lambda2">第二个 Poisson 均值。</param>
        Private Function SaddlepointCDF(xc As Double, lambda1 As Double, lambda2 As Double) As Double
            Dim sq As Double = std.Sqrt(xc * xc + 4.0 * lambda1 * lambda2)
            Dim shat As Double = std.Log((xc + sq) / (2.0 * lambda1))     ' 鞍点
            Dim expS As Double = std.Exp(shat)

            Dim K As Double = lambda1 * (expS - 1.0) + lambda2 * (1.0 / expS - 1.0)    ' CGF
            Dim K2 As Double = lambda1 * expS + lambda2 / expS                          ' CGF''

            If K2 <= 0.0 Then
                Return If(xc >= 0.0, 1.0, 0.0)
            End If

            Dim w As Double = std.Sign(shat) * std.Sqrt(2.0 * (shat * xc - K))
            Dim u As Double = shat * std.Sqrt(K2)

            Dim phi As Double = Microsoft.VisualBasic.Math.Distributions.pnorm.eval(w)
            Dim pdf As Double = Microsoft.VisualBasic.Math.Distributions.pnorm.ProbabilityDensity(w, 0.0, 1.0)

            Dim correction As Double

            If std.Abs(w) < 1.0E-08 Then
                ' w -> 0 时的极限形式：使用 Temme 的均匀展开的简单近似
                correction = pdf * (1.0 / 3.0 - 1.0 / u)
            Else
                correction = pdf * (1.0 / w - 1.0 / u)
            End If

            Dim ret As Double = phi + correction

            If ret < 0.0 Then ret = 0.0
            If ret > 1.0 Then ret = 1.0

            Return ret
        End Function

        ''' <summary>
        ''' 按 R 的向量化语义（循环补齐）计算 <see cref="pskellam"/>。
        ''' </summary>
        Public Function pskellam(q As Double(),
                                 lambda1 As Double(),
                                 Optional lambda2 As Double() = Nothing,
                                 Optional lowerTail As Boolean = True,
                                 Optional logP As Boolean = False) As Double()

            If lambda2 Is Nothing Then
                lambda2 = lambda1
            End If

            Dim n As Integer = std.Max(std.Max(q.Length, lambda1.Length), lambda2.Length)

            If n = 0 Then
                Return New Double() {}
            End If

            Dim ret As Double() = New Double(n - 1) {}

            For i As Integer = 0 To n - 1
                ret(i) = pskellam(q(i Mod q.Length),
                                  lambda1(i Mod lambda1.Length),
                                  lambda2(i Mod lambda2.Length),
                                  lowerTail, logP)
            Next

            Return ret
        End Function
    End Module
End Namespace
