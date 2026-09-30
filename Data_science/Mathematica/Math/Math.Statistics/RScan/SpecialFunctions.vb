' ============================================================================
' SpecialFunctions.vb — r-scan 所需特殊函数（纯 BCL，无第三方依赖）
' ----------------------------------------------------------------------------
' LogGamma：Lanczos 近似（g=7, n=9 系数）+ x<0.5 反射公式，精度 ~1e-15
' RegLowerGamma(a,x) = γ(a,x)/Γ(a)：x < a+1 级数法；否则 1−连分式
' RegUpperGamma(a,x) = Γ(a,x)/Γ(a)：x ≥ a+1 连分式（Lentz 算法）；否则 1−级数
' Poisson 尾部恒等式（readme §三 式(3)）：
'   P(Pois(μ) ≥ r) = P_reg(r, μ) = 1 − e^{−μ}·Σ_{j=0}^{r−1} μ^j/j!
'   P(Pois(μ) ≤ k) = Q(k+1, μ)
' 镜像验证：恒等式对拍 + 与直接求和公式 1e-12 一致（63 项测试之一）
' ============================================================================

Imports System
Imports std = System.Math

Namespace RScan

    Public Module SpecialFunctions

        ' Lanczos 系数（g = 7）
        Private ReadOnly Lanczos() As Double = {
            0.99999999999980993, 676.5203681218851, -1259.1392167224028,
            771.32342877765313, -176.61502916214059, 12.507343278686905,
            -0.13857109526572012, 9.9843695780195716E-6, 1.5056327351493116E-7}

        Private Const LanczosG As Double = 7.0

        ''' <summary>ln Γ(x)，x > 0（x &lt; 0.5 走反射公式）</summary>
        Public Function LogGamma(x As Double) As Double
            If x <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(x), "LogGamma 要求 x > 0")
            If x < 0.5 Then
                ' Γ(x)·Γ(1−x) = π / sin(πx)
                Return std.Log(std.PI / std.Sin(std.PI * x)) - LogGamma(1.0 - x)
            End If
            Dim z = x - 1.0
            Dim a = Lanczos(0)
            For i = 1 To Lanczos.Length - 1
                a += Lanczos(i) / (z + i)
            Next
            Dim t = z + LanczosG + 0.5
            Return 0.5 * std.Log(2.0 * std.PI) + (z + 0.5) * std.Log(t) - t + std.Log(a)
        End Function

        ''' <summary>级数法求 P(a,x)（适用 x &lt; a+1）</summary>
        Private Function GammaSeries(a As Double, x As Double) As Double
            Dim ap = a
            Dim summ = 1.0 / a
            Dim delt = summ
            For it = 1 To 200
                ap += 1.0
                delt *= x / ap
                summ += delt
                If std.Abs(delt) < std.Abs(summ) * 0.0000000000000003 Then Exit For
            Next
            Return summ * std.Exp(-x + a * std.Log(x) - LogGamma(a))
        End Function

        ''' <summary>连分式法求 Q(a,x)（适用 x ≥ a+1，Lentz 算法）</summary>
        Private Function GammaCF(a As Double, x As Double) As Double
            Const TINY As Double = 1.0E-300
            Dim b = x + 1.0 - a
            Dim c = 1.0 / TINY
            Dim d = 1.0 / b
            Dim h = d
            For i = 1 To 200
                Dim an = -i * (i - a)
                b += 2.0
                d = an * d + b
                If std.Abs(d) < TINY Then d = TINY
                c = b + an / c
                If std.Abs(c) < TINY Then c = TINY
                d = 1.0 / d
                Dim de = d * c
                h *= de
                If std.Abs(de - 1.0) < 0.0000000000000003 Then Exit For
            Next
            Return std.Exp(-x + a * std.Log(x) - LogGamma(a)) * h
        End Function

        ''' <summary>正则化下不完全 Gamma：P(a,x) = γ(a,x)/Γ(a)</summary>
        Public Function RegLowerGamma(a As Double, x As Double) As Double
            If a <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(a), "要求 a > 0")
            If x <= 0 Then Return 0.0
            If x < a + 1.0 Then
                Return GammaSeries(a, x)
            Else
                Return 1.0 - GammaCF(a, x)
            End If
        End Function

        ''' <summary>正则化上不完全 Gamma：Q(a,x) = Γ(a,x)/Γ(a)</summary>
        Public Function RegUpperGamma(a As Double, x As Double) As Double
            If a <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(a), "要求 a > 0")
            If x <= 0 Then Return 1.0
            If x < a + 1.0 Then
                Return 1.0 - GammaSeries(a, x)
            Else
                Return GammaCF(a, x)
            End If
        End Function

        ''' <summary>P(Pois(μ) ≥ r)（r ≥ 1 整数）= P_reg(r, μ) [readme §三 式(3)]</summary>
        Public Function PoissonTail(mu As Double, r As Integer) As Double
            If r <= 0 Then Return 1.0
            If mu <= 0 Then Return 0.0
            Return RegLowerGamma(r, mu)
        End Function

        ''' <summary>P(Pois(μ) ≤ k)（k ≥ 0 整数）= Q(k+1, μ)</summary>
        Public Function PoissonCdf(mu As Double, k As Integer) As Double
            If k < 0 Then Return 0.0
            If mu <= 0 Then Return 1.0
            Return RegUpperGamma(k + 1, mu)
        End Function

    End Module

End Namespace
