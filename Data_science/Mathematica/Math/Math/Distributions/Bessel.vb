Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

Namespace Distributions

    ''' <summary>
    ''' 修正贝塞尔函数（modified Bessel function）第一类 ``I_ν(x)`` 的实现。
    ''' 
    ''' 本模块对应 R 语言之中的 ``besselI(x, nu, expon.scaled = FALSE)``，
    ''' 用于 Skellam 分布的概率密度函数计算（参见 ``dskellam``）。
    ''' </summary>
    ''' <remarks>
    ''' 实现策略：
    ''' 
    ''' 1. 中等参数区域（``x &lt; 30 + ν``）使用幂级数定义
    '''    ``I_ν(x) = Σ_k (x/2)^(2k+ν) / (k! Γ(k+ν+1))``，
    '''    并在对数域使用 log-sum-exp 累加，以避免中间项上下溢出；
    ''' 2. 大参数区域使用 Abramowitz &amp; Stegun 9.7.1 的渐近展开，
    '''    该渐近级数是发散的，因此在最小项处截断；
    ''' 3. 指数缩放形式 ``e^{-x} I_ν(x)`` 通过乘以 ``e^{-x}`` 得到，
    '''    对应 R 之中 ``expon.scaled = TRUE`` 的行为。
    ''' </remarks>
    Public Module BesselFunctions

        ''' <summary>
        ''' 修正贝塞尔函数第一类 ``I_ν(x)``。
        ''' </summary>
        ''' <param name="x">自变量，要求 ``x &gt;= 0``（对于整数阶亦支持 ``x &lt; 0``）。</param>
        ''' <param name="nu">阶数 ``ν``，要求 ``ν &gt;= 0``（负整数阶会退化为 ``I_{-n} = I_n``）。</param>
        ''' <param name="exponScaled">
        ''' 若为 ``True`` 则返回指数缩放值 ``e^{-x} I_ν(x)``，以避免大 ``x`` 时的溢出。
        ''' </param>
        ''' <returns>修正贝塞尔函数值；参数非法时返回 <see cref="Double.NaN"/>。</returns>
        Public Function BesselI(x As Double, nu As Double, Optional exponScaled As Boolean = False) As Double
            If Double.IsNaN(x) OrElse Double.IsNaN(nu) Then
                Return Double.NaN
            End If

            If x < 0.0 Then
                ' 对于整数阶：I_n(-x) = (-1)^n I_n(x)
                Dim n As Double = std.Round(nu)

                If std.Abs(nu - n) < 1.0E-09 Then
                    Dim value As Double = BesselI(-x, nu, exponScaled)

                    If (CLng(n) And 1L) = 1L Then
                        Return -value
                    Else
                        Return value
                    End If
                End If

                Return Double.NaN
            End If

            If nu < 0.0 Then
                ' 对于整数阶：I_{-n} = I_n ；非整数负阶超出本实现范围
                Dim n As Double = std.Round(nu)

                If std.Abs(nu - n) < 1.0E-09 Then
                    Return BesselI(x, -nu, exponScaled)
                End If

                Return Double.NaN
            End If

            If x = 0.0 Then
                ' I_0(0) = 1，I_ν(0) = 0 (ν > 0)
                If nu = 0.0 Then
                    Return 1.0
                Else
                    Return 0.0
                End If
            End If

            If x >= 30.0 + nu Then
                Return BesselIAsymptotic(x, nu, exponScaled)
            Else
                Return BesselISeries(x, nu, exponScaled)
            End If
        End Function

        ''' <summary>
        ''' 使用幂级数定义计算 ``I_ν(x)``（对数域求和）。
        ''' </summary>
        Private Function BesselISeries(x As Double, nu As Double, exponScaled As Boolean) As Double
            Dim logHalfX As Double = std.Log(x / 2.0)
            Dim logSum As Double = Double.NegativeInfinity
            Dim k As Integer = 0

            Do While k < 100000
                ' (x/2)^(2k+ν) / (k! Γ(k+ν+1))
                Dim logTerm As Double = (2.0 * k + nu) * logHalfX -
                                        lngamm(k + 1.0) -
                                        lngamm(k + nu + 1.0)

                logSum = LogAddExp(logSum, logTerm)
                k += 1

                If k > 2 AndAlso logTerm < logSum - 40.0 Then
                    Exit Do
                End If
            Loop

            If Double.IsNegativeInfinity(logSum) Then
                Return 0.0
            End If

            Dim value As Double = std.Exp(logSum)

            If exponScaled Then
                value *= std.Exp(-x)
            End If

            Return value
        End Function

        ''' <summary>
        ''' 使用渐近展开计算大参数下的 ``I_ν(x)``（Abramowitz &amp; Stegun 9.7.1）。
        ''' </summary>
        Private Function BesselIAsymptotic(x As Double, nu As Double, exponScaled As Boolean) As Double
            Dim mu As Double = 4.0 * nu * nu
            Dim sum As Double = 1.0
            Dim prevAbs As Double = Double.MaxValue
            Dim factorialPart As Double = 1.0
            Dim powerPart As Double = 1.0
            Dim product As Double = 1.0
            Dim m As Integer = 1

            Do While m <= 30
                product *= mu - CDbl((2 * m - 1)) ^ 2
                factorialPart *= m
                powerPart *= 8.0 * x

                Dim term As Double = product / (factorialPart * powerPart)

                If (m And 1) = 1 Then
                    term = -term
                End If

                ' 渐近级数在最小项处截断
                If std.Abs(term) > prevAbs Then
                    Exit Do
                End If

                sum += term
                prevAbs = std.Abs(term)
                m += 1
            Loop

            Dim value As Double = sum / std.Sqrt(2.0 * std.PI * x)

            If Not exponScaled Then
                value *= std.Exp(x)
            End If

            Return value
        End Function

        ''' <summary>
        ''' 在对数域安全地计算 ``log(exp(a) + exp(b))``。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function LogAddExp(a As Double, b As Double) As Double
            If Double.IsNegativeInfinity(a) Then Return b
            If Double.IsNegativeInfinity(b) Then Return a

            If a > b Then
                Return a + std.Log(1.0 + std.Exp(b - a))
            Else
                Return b + std.Log(1.0 + std.Exp(a - b))
            End If
        End Function

        ''' <summary>
        ''' 按 R 的向量化语义（循环补齐）计算 ``besselI``。
        ''' </summary>
        ''' <param name="x">自变量向量。</param>
        ''' <param name="nu">阶数向量。</param>
        ''' <param name="exponScaled">是否返回指数缩放值。</param>
        ''' <returns>长度等于两个输入向量长度的最大值的结果向量。</returns>
        Public Function BesselI(x As Double(), nu As Double(), Optional exponScaled As Boolean = False) As Double()
            Dim n As Integer = std.Max(x.Length, nu.Length)

            If n = 0 Then
                Return New Double() {}
            End If

            Dim ret As Double() = New Double(n - 1) {}

            For i As Integer = 0 To n - 1
                ret(i) = BesselI(x(i Mod x.Length), nu(i Mod nu.Length), exponScaled)
            Next

            Return ret
        End Function

        ''' <summary>
        ''' 向量化入口（<see cref="Vector"/> 重载）。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function BesselI(x As Vector, nu As Vector, Optional exponScaled As Boolean = False) As Vector
            Return New Vector(BesselI(x.ToArray, nu.ToArray, exponScaled))
        End Function

        ''' <summary>
        ''' 标量阶数、向量自变量的便捷入口。
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function BesselI(x As Vector, nu As Double, Optional exponScaled As Boolean = False) As Vector
            Dim ret As Double() = New Double(x.Length - 1) {}

            For i As Integer = 0 To ret.Length - 1
                ret(i) = BesselI(x(i), nu, exponScaled)
            Next

            Return New Vector(ret)
        End Function
    End Module
End Namespace
