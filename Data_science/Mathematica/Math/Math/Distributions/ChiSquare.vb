Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

Namespace Distributions

    ''' <summary>
    ''' 卡方分布（中心与非中心）的分布函数，对应 R 语言之中的 ``pchisq``。
    ''' 
    ''' 非中心卡方分布是 Skellam 分布累积分布函数的核心依赖
    ''' （参见 ``Resources\pskellam.txt``）：两个 Poisson 变量之差的
    ''' 累积分布可以通过非中心卡方分布精确表达。
    ''' </summary>
    Public Module ChiSquareDistribution

        ''' <summary>
        ''' 中心卡方分布的下尾概率 ``P(X &lt;= q)``，其中 ``X ~ chi2(df)``。
        ''' </summary>
        ''' <param name="q">分位点。</param>
        ''' <param name="df">自由度。</param>
        ''' <returns>下尾概率，等价于正则化下不完全 Gamma 函数 ``P(df/2, q/2)``。</returns>
        Public Function ChiSquareCDF(q As Double, df As Double) As Double
            If Double.IsNaN(q) OrElse df <= 0.0 Then
                Return Double.NaN
            End If
            If q <= 0.0 Then
                Return 0.0
            End If

            Return RegularizedGammaP(df / 2.0, q / 2.0)
        End Function

        ''' <summary>
        ''' 中心卡方分布的上尾概率 ``P(X &gt; q)``，其中 ``X ~ chi2(df)``。
        ''' </summary>
        Public Function ChiSquareSF(q As Double, df As Double) As Double
            If Double.IsNaN(q) OrElse df <= 0.0 Then
                Return Double.NaN
            End If
            If q <= 0.0 Then
                Return 1.0
            End If

            Return RegularizedGammaQ(df / 2.0, q / 2.0)
        End Function

        ''' <summary>
        ''' 非中心卡方分布的累积分布函数，严格对应 R 的
        ''' ``pchisq(q, df, ncp, lower.tail, log.p)``。
        ''' </summary>
        ''' <param name="q">分位点。</param>
        ''' <param name="df">自由度（可以为小数）。</param>
        ''' <param name="ncp">非中心参数 ``λ &gt;= 0``；为 0 时退化为（中心）卡方分布。</param>
        ''' <param name="lowerTail">``True`` 返回下尾 ``P(X &lt;= q)``，否则返回上尾 ``P(X &gt; q)``。</param>
        ''' <param name="logP">``True`` 时返回概率的自然对数。</param>
        ''' <returns>概率值（或其对数值）。</returns>
        ''' <remarks>
        ''' 采用 Poisson 加权混合级数：
        ''' 
        ''' ``P(chi'^2(df,ncp) &lt;= q) = Σ_j e^{-ncp/2}(ncp/2)^j / j! * P(chi^2(df+2j) &lt;= q)``
        ''' 
        ''' 并且利用上不完全 Gamma 的递推关系
        ''' ``Q(a+1,x) = Q(a,x) + x^a e^{-x}/Γ(a+1)``
        ''' 逐项增量更新中心卡方上尾值，从而将级数内部的不完全 Gamma 调用
        ''' 降为每次迭代的常数级运算。
        ''' </remarks>
        Public Function pchisq(q As Double,
                               df As Double,
                               Optional ncp As Double = 0.0,
                               Optional lowerTail As Boolean = True,
                               Optional logP As Boolean = False) As Double

            If Double.IsNaN(q) OrElse Double.IsNaN(df) OrElse Double.IsNaN(ncp) Then
                Return Double.NaN
            End If
            If df < 0.0 OrElse ncp < 0.0 Then
                Return Double.NaN
            End If

            Dim p As Double

            If ncp = 0.0 Then
                p = If(lowerTail, ChiSquareCDF(q, df), ChiSquareSF(q, df))
            ElseIf q <= 0.0 Then
                ' 非中心分布在 q=0 处的下尾为 0、上尾为 1
                p = If(lowerTail, 0.0, 1.0)
            Else
                Dim a0 As Double = df / 2.0
                Dim y As Double = q / 2.0
                Dim lambda As Double = ncp / 2.0

                ' Q_j = Q(a0 + j, y) 的初值
                Dim qValue As Double = RegularizedGammaQ(a0, y)

                ' t_j = y^{a0+j} e^{-y} / Γ(a0+j+1)，作为 Q_{j+1} - Q_j 的增量
                Dim t As Double = std.Exp(a0 * std.Log(y) - y - lngamm(a0 + 1.0))

                Dim w As Double = std.Exp(-lambda)      ' Poisson 权重：w_0
                Dim upperSum As Double = w * qValue
                Dim lowerSum As Double = w * (1.0 - qValue)

                ' 覆盖 Poisson 权重峰值的迭代上界
                Dim jMax As Integer = CInt(std.Ceiling(lambda + 12.0 * std.Sqrt(lambda) + 100.0))
                Dim j As Integer = 0

                Do While j < jMax
                    j += 1

                    w *= lambda / j
                    qValue += t
                    t *= y / (a0 + j)

                    If qValue > 1.0 Then
                        qValue = 1.0
                    End If

                    upperSum += w * qValue
                    lowerSum += w * (1.0 - qValue)

                    ' 权重越过峰值且已可忽略时提前结束
                    If j > lambda AndAlso w < 1.0E-17 Then
                        Exit Do
                    End If
                Loop

                p = If(lowerTail, lowerSum, upperSum)
            End If

            If p < 0.0 Then
                p = 0.0
            ElseIf p > 1.0 Then
                p = 1.0
            End If

            If logP Then
                If p = 0.0 Then
                    Return Double.NegativeInfinity
                End If

                Return std.Log(p)
            End If

            Return p
        End Function

        ''' <summary>
        ''' 正则化下不完全 Gamma 函数 ``P(a, x) = γ(a,x)/Γ(a)``。
        ''' </summary>
        ''' <param name="a">形状参数，要求 ``a &gt; 0``。</param>
        ''' <param name="x">自变量，要求 ``x &gt;= 0``。</param>
        ''' <returns>取值区间为 ``[0, 1]``。</returns>
        ''' <remarks>
        ''' 当 ``x &lt; a + 1`` 时使用级数展开；否则利用对称关系
        ''' ``P(a,x) = 1 - Q(a,x)`` 转为连分式计算，以保证数值稳定。
        ''' </remarks>
        Public Function RegularizedGammaP(a As Double, x As Double) As Double
            If a <= 0.0 OrElse x < 0.0 Then
                Return Double.NaN
            End If
            If x = 0.0 Then
                Return 0.0
            End If

            If x < a + 1.0 Then
                ' 级数展开：γ(a,x) = x^a e^{-x} Σ_{n>=0} x^n / (a(a+1)...(a+n))
                Dim ap As Double = a
                Dim sum As Double = 1.0 / a
                Dim del As Double = sum

                Do While ap < a + 1000000.0
                    ap += 1.0
                    del *= x / ap
                    sum += del

                    If std.Abs(del) <= std.Abs(sum) * 1.0E-16 Then
                        Exit Do
                    End If
                Loop

                Return sum * std.Exp(-x + a * std.Log(x) - lngamm(a))
            Else
                Return 1.0 - RegularizedGammaQ(a, x)
            End If
        End Function

        ''' <summary>
        ''' 正则化上不完全 Gamma 函数 ``Q(a, x) = Γ(a,x)/Γ(a) = 1 - P(a,x)``。
        ''' </summary>
        ''' <param name="a">形状参数，要求 ``a &gt; 0``。</param>
        ''' <param name="x">自变量，要求 ``x &gt;= 0``。</param>
        ''' <returns>取值区间为 ``[0, 1]``。</returns>
        ''' <remarks>
        ''' 当 ``x &gt;= a + 1`` 时使用 Lentz 连分式展开；否则利用对称关系
        ''' ``Q(a,x) = 1 - P(a,x)`` 转为级数计算。
        ''' </remarks>
        Public Function RegularizedGammaQ(a As Double, x As Double) As Double
            If a <= 0.0 OrElse x < 0.0 Then
                Return Double.NaN
            End If
            If x = 0.0 Then
                Return 1.0
            End If

            If x < a + 1.0 Then
                Return 1.0 - RegularizedGammaP(a, x)
            Else
                Const tiny As Double = 1.0E-300

                Dim b As Double = x + 1.0 - a
                Dim c As Double = 1.0 / tiny
                Dim d As Double = 1.0 / b
                Dim h As Double = d
                Dim i As Integer = 1

                Do While i < 1000000
                    Dim an As Double = -i * (i - a)

                    b += 2.0

                    d = an * d + b
                    If std.Abs(d) < tiny Then d = tiny

                    c = b + an / c
                    If std.Abs(c) < tiny Then c = tiny

                    d = 1.0 / d

                    Dim del As Double = d * c
                    h *= del

                    If std.Abs(del - 1.0) < 1.0E-16 Then
                        Exit Do
                    End If

                    i += 1
                Loop

                Return std.Exp(-x + a * std.Log(x) - lngamm(a)) * h
            End If
        End Function

        ''' <summary>
        ''' 按 R 的向量化语义（循环补齐）计算 ``pchisq``。
        ''' </summary>
        ''' <param name="q">分位点向量。</param>
        ''' <param name="df">自由度向量。</param>
        ''' <param name="ncp">非中心参数。</param>
        ''' <param name="lowerTail">是否返回下尾概率。</param>
        ''' <param name="logP">是否返回对数概率。</param>
        ''' <returns>长度等于两个输入向量长度的最大值的结果向量。</returns>
        Public Function pchisq(q As Double(), df As Double(), Optional ncp As Double = 0.0, Optional lowerTail As Boolean = True, Optional logP As Boolean = False) As Double()
            Dim n As Integer = std.Max(q.Length, df.Length)

            If n = 0 Then
                Return New Double() {}
            End If

            Dim ret As Double() = New Double(n - 1) {}

            For i As Integer = 0 To n - 1
                ret(i) = pchisq(q(i Mod q.Length), df(i Mod df.Length), ncp, lowerTail, logP)
            Next

            Return ret
        End Function

        ''' <summary>
        ''' 向量化入口（<see cref="Vector"/> 重载）。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function pchisq(q As Vector, df As Vector, Optional ncp As Double = 0.0, Optional lowerTail As Boolean = True, Optional logP As Boolean = False) As Vector
            Return New Vector(pchisq(q.ToArray, df.ToArray, ncp, lowerTail, logP))
        End Function
    End Module
End Namespace
