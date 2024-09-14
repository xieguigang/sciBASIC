#Region "Microsoft.VisualBasic::197127ab1e79490b517d8059122f0993, Data_science\Mathematica\Math\Math.Statistics\SpecialFunctions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 731
    '    Code Lines: 548 (74.97%)
    ' Comment Lines: 129 (17.65%)
    '    - Xml Docs: 89.15%
    ' 
    '   Blank Lines: 54 (7.39%)
    '     File Size: 22.97 KB


    ' Module SpecialFunctions
    ' 
    '     Function: BetaFunction, Binom, Binomial, BinomialCoefficient, Choose
    '               Combination, Correlation, Covariance, EvaluatePolynomial, Factorial
    '               (+2 Overloads) FactorialCalculation, gamma, gammaln, incbcf, incbd
    '               IncompleteGamma, IncompleteGammaComplement, InvBinomal, MutualProbability, Permutation
    '               pseries, RegularizedIncompleteBetaFunction, safeDiv, StirlingsFormula
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports std = System.Math

''' <summary>
''' @author Will_and_Sara
''' </summary>
Public Module SpecialFunctions

    Const MACHEP As Double = 0.00000000000000011102230246251565
    Const MAXLOG As Double = 709.782712893384
    Const MINLOG As Double = -745.13321910194122
    Const MAXGAM As Double = 171.62437695630271
    Const MAXNUM As Double = 1.7976931348623157E+308

    Public Function Covariance(array1 As Double(), array2 As Double()) As Double
        Dim tmp As Double = 0
        If array1.Length <> array2.Length Then
            'not supported
            tmp = Double.NaN
        Else
            Dim BPM1 As New MomentFunctions.BasicProductMoments(array1)
            Dim BPM2 As New MomentFunctions.BasicProductMoments(array2)
            Dim number As Integer = std.Min(array1.Length, array2.Length)
            For i As Integer = 0 To number - 1
                tmp += (array1(i) - BPM1.Mean()) * (array2(i) * BPM2.Mean())
            Next
        End If
        Return tmp
    End Function

    Public Function Correlation(array1 As Double(), array2 As Double()) As Double
        Dim tmp As Double = 0
        If array1.Length <> array2.Length Then
            'not supported
            tmp = Double.NaN
        Else
            Dim BPM1 As New MomentFunctions.BasicProductMoments(array1)
            Dim BPM2 As New MomentFunctions.BasicProductMoments(array2)
            Dim number As Integer = std.Min(array1.Length, array2.Length)
            For i As Integer = 0 To number - 1
                tmp += (array1(i) - BPM1.Mean()) * (array2(i) * BPM2.Mean())
            Next
            Return tmp / (BPM1.StDev() * BPM2.StDev())
        End If
        Return tmp
    End Function

    ''' <summary>
    ''' could be improved with unsigned integers or the gamma function
    ''' </summary>
    ''' <param name="N"></param>
    ''' <param name="k"></param>
    ''' <returns></returns>
    Public Function Factorial(N As Integer, k As Integer) As Integer
        If N = 1 Then Return 1
        If k + 1 >= N Then Return N
        Dim ret As Integer = 1
        For i As Integer = k + 1 To N - 1
            ret *= i
        Next
        Return ret
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Choose(n As Integer, k As Integer) As Integer
        Return Factorial(n, k) \ VBMath.Factorial(n - k)
    End Function

    ''' <summary>
    ''' n!
    ''' </summary>
    ''' <param name="n"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' calculates `n!` === `P(n, n)`.
    ''' 
    ''' n! = permutation(n, n)
    ''' 
    ''' https://en.wikipedia.org/wiki/Factorial
    ''' </remarks>
    Public Function FactorialCalculation(n As Integer) As ULong
        Dim result As ULong = 1
        For i = 1 To n
            result *= CULng(i)
        Next
        Return result
    End Function

    ''' <summary>
    ''' calculates `C(n, k)`.
    ''' </summary>
    ''' <param name="n"></param>
    ''' <param name="k"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' implements the excel function COMBIN(number, number_chosen).
    ''' 
    ''' COMBIN(8,2) = 28
    ''' 
    ''' https://en.wikipedia.org/wiki/Combination
    ''' </remarks>
    Public Function Combination(n As Integer, k As Integer) As Double
        If k = 0 Then
            Return 1
        ElseIf n = k Then
            Return 1
        ElseIf n < k Then
            Return 0
        End If

        Return Permutation(n, k) / Permutation(k, k)
    End Function

    Public Function BinomialCoefficient(n As Integer, k As Integer) As ULong
        If k = 0 Then
            Return 1
        ElseIf n = k Then
            Return 1
        Else
            Dim nkfactor = FactorialCalculation(n, k)
            Dim kfactor = FactorialCalculation(k)
            If kfactor <> 0 Then
                Return nkfactor / kfactor
            Else
                Return 1
            End If
            'return coefficient;
        End If
    End Function

    ''' <summary>
    ''' calculates `P(n, k)`.
    ''' </summary>
    ''' <param name="n"></param>
    ''' <param name="k"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' https://en.wikipedia.org/wiki/Permutation
    ''' </remarks>
    Public Function Permutation(n As Integer, k As Integer) As Double
        If 0 = k Then
            Return 1
        ElseIf n < k Then
            Return 0
        End If

        Dim bp As Double = 1

        Do While k > 0
            k -= 1
            bp *= n
            n -= 1
        Loop

        Return bp
    End Function

    Public Function FactorialCalculation(n As Integer, k As Integer) As ULong
        Dim result As ULong = 1
        For i = n - k + 1 To n
            result *= CULng(i)
        Next
        Return result
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="n"></param>
    ''' <param name="k"></param>
    ''' <returns></returns>
    Public Function Binom(n As Integer, k As Integer) As Double
        Return VBMath.Factorial(n) / (VBMath.Factorial(k) * VBMath.Factorial(n - k))
    End Function

    Public Function Binomial(probability As Double, n As Integer, k As Integer) As Double
        Dim value As Double = 0
        For i As Integer = 0 To k
            value = value + Choose(n, i) * (std.Pow(probability, i)) * (std.Pow(1 - probability, n - i))
        Next
        Return value + 1
    End Function

    Public Function InvBinomal(probability As Double, n As Integer, k As Integer) As Double
        For i As Integer = 0 To n - 1 'need to validate this one.
            If Binomial(probability, n, i) > k Then Return i
        Next
        Return n
    End Function

    ''' <summary>
    ''' http://lethalman.blogspot.com/2011/08/probability-of-union-of-independent.html
    ''' </summary>
    ''' <param name="probabilities"></param>
    ''' <returns></returns>
    Public Function MutualProbability(probabilities As Double()) As Double
        Dim ret As Double = 0
        For i As Integer = 0 To probabilities.Length - 1
            ret += probabilities(i) * (1 - ret)
        Next
        Return ret
    End Function

    Public Function BetaFunction(a As Double, b As Double) As Double
        Return std.Exp(gammaln(a) + gammaln(b) - gammaln(a - b))
    End Function

    ''' <summary>
    ''' ######  The regularized incomplete beta function
    ''' https://en.wikipedia.org/wiki/Beta_function#Incomplete_beta_function
    ''' </summary>
    ''' <param name="aa"></param>
    ''' <param name="bb"></param>
    ''' <param name="xx"></param>
    ''' <returns></returns>
    Public Function RegularizedIncompleteBetaFunction(aa As Double, bb As Double, xx As Double) As Double
        Dim a, b, t, x, xc, w, y As Double
        Dim flag As Boolean

        If aa <= 0.0 OrElse bb <= 0.0 Then
            Throw New ArithmeticException("ibeta: Domain error!")
        End If

        If (xx <= 0.0) OrElse (xx >= 1.0) Then
            If xx = 0.0 Then Return 0.0
            If xx = 1.0 Then Return 1.0
            Throw New ArithmeticException("ibeta: Domain error!")
        End If

        flag = False
        If (bb * xx) <= 1.0 AndAlso xx <= 0.95 Then
            t = pseries(aa, bb, xx)
            Return t
        End If

        w = 1.0 - xx

        ' Reverse a and b if x is greater than the mean. 
        If xx > (aa / (aa + bb)) Then
            flag = True
            a = bb
            b = aa
            xc = xx
            x = w
        Else
            a = aa
            b = bb
            xc = w
            x = xx
        End If

        If flag AndAlso (b * x) <= 1.0 AndAlso x <= 0.95 Then
            t = pseries(a, b, x)
            If t <= MACHEP Then
                t = 1.0 - MACHEP
            Else
                t = 1.0 - t
            End If
            Return t
        End If

        ' Choose expansion for better convergence. 
        y = x * (a + b - 2.0) - (a - 1.0)
        If y < 0.0 Then
            w = incbcf(a, b, x)
        Else
            w = incbd(a, b, x) / xc
        End If

        '         Multiply w by the factor
        '           a      b   _             _     _
        '          x  (1-x)   | (a+b) / ( a | (a) | (b) ) .   

        y = a * std.Log(x)
        t = b * std.Log(xc)
        If (a + b) < MAXGAM AndAlso std.Abs(y) < MAXLOG AndAlso std.Abs(t) < MAXLOG Then
            t = std.Pow(xc, b)
            t *= std.Pow(x, a)
            t /= a
            t *= w
            t *= safeDiv(gamma(a + b), gamma(a) * gamma(b))
            If flag Then
                If t <= MACHEP Then
                    t = 1.0 - MACHEP
                Else
                    t = 1.0 - t
                End If
            End If
            Return t
        End If
        ' Resort to logarithms.  
        y += t + gammaln(a + b) - gammaln(a) - gammaln(b)
        y += std.Log(w / a)
        If y < MINLOG Then
            t = 0.0
        Else
            t = std.Exp(y)
        End If

        If flag Then
            If t <= MACHEP Then
                t = 1.0 - MACHEP
            Else
                t = 1.0 - t
            End If
        End If
        Return t
    End Function

    ''' <summary>
    ''' Continued fraction expansion #1
    ''' * for incomplete beta integral
    ''' </summary>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <param name="x"></param>
    ''' <returns></returns>
    Private Function incbcf(a As Double, b As Double, x As Double) As Double
        Dim xk, pk, pkm1, pkm2, qk, qkm1, qkm2 As Double
        Dim k1, k2, k3, k4, k5, k6, k7, k8 As Double
        Dim r, t, ans, thresh As Double
        Dim n As Integer
        Dim big As Double = 4503599627370496.0
        Dim biginv As Double = 0.00000000000000022204460492503131
        k1 = a
        k2 = a + b
        k3 = a
        k4 = a + 1.0
        k5 = 1.0
        k6 = b - 1.0
        k7 = k4
        k8 = a + 2.0
        pkm2 = 0.0
        qkm2 = 1.0
        pkm1 = 1.0
        qkm1 = 1.0
        ans = 1.0
        r = 1.0
        n = 0
        thresh = 3.0 * MACHEP
        Do
            xk = -(x * k1 * k2) / (k3 * k4)
            pk = pkm1 + pkm2 * xk
            qk = qkm1 + qkm2 * xk
            pkm2 = pkm1
            pkm1 = pk
            qkm2 = qkm1
            qkm1 = qk
            xk = (x * k5 * k6) / (k7 * k8)
            pk = pkm1 + pkm2 * xk
            qk = qkm1 + qkm2 * xk
            pkm2 = pkm1
            pkm1 = pk
            qkm2 = qkm1
            qkm1 = qk
            If qk <> 0 Then r = pk / qk
            If r <> 0 Then
                t = std.Abs((ans - r) / r)
                ans = r
            Else
                t = 1.0
            End If
            If t < thresh Then Return ans
            k1 += 1.0
            k2 += 1.0
            k3 += 2.0
            k4 += 2.0
            k5 += 1.0
            k6 -= 1.0
            k7 += 2.0
            k8 += 2.0
            If (std.Abs(qk) + std.Abs(pk)) > big Then
                pkm2 *= biginv
                pkm1 *= biginv
                qkm2 *= biginv
                qkm1 *= biginv
            End If
            If (std.Abs(qk) < biginv) OrElse (std.Abs(pk) < biginv) Then
                pkm2 *= big
                pkm1 *= big
                qkm2 *= big
                qkm1 *= big
            End If
            n += 1
        Loop While n < 1000
        Return ans
    End Function

    ''' <summary>
    ''' Continued fraction expansion #2
    ''' * for incomplete beta integral
    ''' </summary>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <param name="x"></param>
    ''' <returns></returns>
    Private Function incbd(a As Double, b As Double, x As Double) As Double
        Dim xk, pk, pkm1, pkm2, qk, qkm1, qkm2 As Double
        Dim k1, k2, k3, k4, k5, k6, k7, k8 As Double
        Dim r, t, ans, z, thresh As Double
        Dim n As Integer
        Dim big As Double = 4503599627370496.0
        Dim biginv As Double = 0.00000000000000022204460492503131

        k1 = a
        k2 = b - 1.0
        k3 = a
        k4 = a + 1.0
        k5 = 1.0
        k6 = a + b
        k7 = a + 1.0
        k8 = a + 2.0

        pkm2 = 0.0
        qkm2 = 1.0
        pkm1 = 1.0
        qkm1 = 1.0
        z = x / (1.0 - x)
        ans = 1.0
        r = 1.0
        n = 0
        thresh = 3.0 * MACHEP
        Do
            xk = -(z * k1 * k2) / (k3 * k4)
            pk = pkm1 + pkm2 * xk
            qk = qkm1 + qkm2 * xk
            pkm2 = pkm1
            pkm1 = pk
            qkm2 = qkm1
            qkm1 = qk

            xk = (z * k5 * k6) / (k7 * k8)
            pk = pkm1 + pkm2 * xk
            qk = qkm1 + qkm2 * xk
            pkm2 = pkm1
            pkm1 = pk
            qkm2 = qkm1
            qkm1 = qk

            If qk <> 0 Then r = pk / qk
            If r <> 0 Then
                t = std.Abs((ans - r) / r)
                ans = r
            Else
                t = 1.0
            End If

            If t < thresh Then Return ans

            k1 += 1.0
            k2 -= 1.0
            k3 += 2.0
            k4 += 2.0
            k5 += 1.0
            k6 += 1.0
            k7 += 2.0
            k8 += 2.0

            If (std.Abs(qk) + std.Abs(pk)) > big Then
                pkm2 *= biginv
                pkm1 *= biginv
                qkm2 *= biginv
                qkm1 *= biginv
            End If
            If (std.Abs(qk) < biginv) OrElse (std.Abs(pk) < biginv) Then
                pkm2 *= big
                pkm1 *= big
                qkm2 *= big
                qkm1 *= big
            End If
            n += 1
        Loop While n < 1000

        Return ans
    End Function

    Private Function safeDiv(x#, y#) As Double
        If y.IsNaNImaginary Then
            If Not x.IsNaNImaginary Then
                Return 0
            Else
                Return std.Sign(x) / std.Sign(y)
            End If
        ElseIf x = 0.0 Then
            Return 0
        Else
            If x.IsNaNImaginary Then
                Return x
            Else
                Return x / y
            End If
        End If
    End Function

    ''' <summary>
    ''' Power series for incomplete beta integral.
    ''' Use when b*x is small and x not too close to 1.  
    ''' </summary>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <param name="x"></param>
    ''' <returns></returns>
    Private Function pseries(a As Double, b As Double, x As Double) As Double
        Dim s, t, u, v, n, t1, z, ai As Double

        ai = 1.0 / a
        u = (1.0 - b) * x
        v = u / (a + 1.0)
        t1 = v
        t = u
        n = 2.0
        s = 0.0
        z = MACHEP * ai
        Do While std.Abs(v) > z
            u = (n - b) * x / n
            t *= u
            v = t / (a + n)
            s += v
            n += 1.0
        Loop
        s += t1
        s += ai

        u = a * std.Log(x)

        If (a + b) < MAXGAM AndAlso std.Abs(u) < MAXLOG Then
            n = gamma(a + b)
            v = gamma(a) * gamma(b)
            t = safeDiv(n, v)
            s = s * t * std.Pow(x, a)
        Else
            t = gammaln(a + b) - gammaln(a) - gammaln(b) + u + std.Log(s)

            If t < MINLOG Then
                s = 0.0
            Else
                s = std.Exp(t)
            End If
        End If

        Return s
    End Function

    ''' <summary>
    ''' https://www.ncnr.nist.gov/resources/sansmodels/SpecialFunction.java //previous name was stirf
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    Private Function StirlingsFormula(x As Double) As Double
        Dim stir As Double() = {0.00078731139579309368, -0.00022954996161337813, -0.0026813261780578124, 0.0034722222160545866, 0.08333333333334822}
        Dim MaxStir As Double = 143.01608
        Dim w As Double = 1 / x
        Dim y As Double = std.Exp(x)
        w = 1 + w * EvaluatePolynomial(w, stir)
        If x > MaxStir Then
            Dim v As Double = std.Pow(x, 0.5 * x - 0.25)
            y = v * (v / y)
        Else
            y = std.Pow(x, x - 0.5) / y
        End If
        Return std.Sqrt(std.PI) * y * w
    End Function

    ''' <summary>
    ''' https://www.ncnr.nist.gov/resources/sansmodels/SpecialFunction.java //previous name was polevl
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="coefficients"></param>
    ''' <returns></returns>
    Private Function EvaluatePolynomial(x As Double, coefficients As Double()) As Double
        Dim answer As Double = coefficients(0)
        For i As Integer = 1 To coefficients.Length - 1
            answer = answer * x + coefficients(i)
        Next
        Return answer
    End Function

    ''' <summary>
    ''' https://www.ncnr.nist.gov/resources/sansmodels/SpecialFunction.java //previous name was igamc
    ''' </summary>
    ''' <param name="a"></param>
    ''' <param name="x"></param>
    ''' <returns></returns>
    Public Function IncompleteGammaComplement(a As Double, x As Double) As Double
        Dim big As Double = 4503599627370496.0
        Dim biginv As Double = 0.00000000000000022204460492503131
        Dim ans, ax, c, yc, r, t, y, z As Double
        Dim pk, pkm1, pkm2, qk, qkm1, qkm2 As Double
        If x <= 0 OrElse a <= 0 Then Return 1.0
        If x < 1.0 OrElse x < a Then Return 1.0 - IncompleteGamma(a, x)
        ax = a * std.Log(x) - x - gammaln(a)
        If ax < -MAXLOG Then Return 0.0
        ax = std.Exp(ax)
        y = 1.0 - a
        z = x + y + 1.0
        c = 0.0
        pkm2 = 1.0
        qkm2 = x
        pkm1 = x + 1.0
        qkm1 = z * x
        ans = pkm1 / qkm1
        Do
            c += 1.0
            y += 1.0
            z += 2.0
            yc = y * c
            pk = pkm1 * z - pkm2 * yc
            qk = qkm1 * z - qkm2 * yc
            If qk <> 0 Then
                r = pk / qk
                t = std.Abs((ans - r) / r)
                ans = r
            Else
                t = 1.0
            End If
            pkm2 = pkm1
            pkm1 = pk
            qkm2 = qkm1
            qkm1 = qk
            If std.Abs(pk) > big Then
                pkm2 *= biginv
                pkm1 *= biginv
                qkm2 *= biginv
                qkm1 *= biginv
            End If
        Loop While t > MACHEP
        Return ans * ax
    End Function

    ''' <summary>
    ''' https://www.ncnr.nist.gov/resources/sansmodels/SpecialFunction.java //previous name was igam
    ''' </summary>
    ''' <param name="a"></param>
    ''' <param name="x"></param>
    ''' <returns></returns>
    Public Function IncompleteGamma(a As Double, x As Double) As Double
        Dim ans, ax, c, r As Double
        If x <= 0 OrElse a <= 0 Then Return 0.0
        If x > 1.0 AndAlso x > a Then Return 1.0 - IncompleteGammaComplement(a, x)
        ax = a * std.Log(x) - x - gammaln(a)
        If ax < -MAXLOG Then Return (0.0)
        ax = std.Exp(ax)
        r = a
        c = 1.0
        ans = 1.0
        Do
            r += 1.0
            c *= x / r
            ans += c
        Loop While c / ans > MACHEP
        Return (ans * ax / a)
    End Function

    ''' <summary>
    ''' testing showed Ben's code and this code were roughly equivalent (also to excel) however, Ben's code executed faster in the time trials.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    Public Function gammaln(x As Double) As Double
        If x <= 0 Then Return Double.NaN
        Dim c As Double() = New Double(5) {}
        c(0) = 76.180091729471457
        c(1) = -86.505320329416776
        c(2) = 24.014098240830911
        c(3) = -1.231739572450155
        c(4) = 0.001208650973866179
        c(5) = -0.000005395239384953
        Dim tmp As Double = x + 5.5
        tmp = (x + 0.5) * std.Log(tmp) - tmp
        Dim err As Double = 1.0000000001900149
        For i As Integer = 0 To 4
            err += c(i) / (x + i + 1)
        Next
        Return tmp + std.Log(std.Sqrt(std.PI * 2) * err / x)
    End Function

    ''' <summary>
    ''' testing showed Ben's code and this code were roughly equivalent (also to excel) however, Ben's code executed faster in the time trials.
    ''' https://www.ncnr.nist.gov/resources/sansmodels/SpecialFunction.java
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    Public Function gamma(x As Double) As Double
        Dim lp As Double
        Dim lq As Double = std.Abs(x)
        Dim lz As Double
        If lq > 33.0 Then
            If x < 0 Then
                lp = std.Floor(lq)
                If lp = lq Then Return Double.NaN 'gammaoverflow 
                lz = lq - lp
            End If
            If lz > 0.5 Then
                lp += 1
                lz = lq - lp
            End If
            lz = lq * std.Sin(std.PI * lz)
            If lz <> 0.0 Then 'gamma overflow Return Double.NaN
                lz = std.Abs(lz)
                lz = std.PI / (lz * StirlingsFormula(lq))
                Return -lz
            Else
                Return StirlingsFormula(x)
            End If
        End If
        lz = 1.0
        Do While x >= 3.0
            x -= 1.0
            lz *= x
        Loop
        Do While x < 0.0
            If x = 0.0 Then Return Double.NaN 'gamma singular. 
            If x > -0.000000001 Then Return lz / ((1.0 + 0.57721566490153287 * x) * x)
            lz /= x
            x += 1.0
        Loop
        Do While x < 2.0
            If x = 0 Then Return Double.NaN 'gamma singular 
            If x < 0.000000001 Then Return lz / ((1.0 + 0.57721566490153287 * x) * x)
            lz /= x
            x += 1.0
        Loop
        If x = 2.0 OrElse x = 3.0 Then Return lz
        Dim p As Double() = {0.00016011952247675185, 0.0011913514700658638, 0.010421379756176158, 0.047636780045713721, 0.20744822764843598, 0.49421482680149709, 1.0}
        Dim q As Double() = {-0.000023158187332412014, 0.00053960558049330335, -0.0044564191385179728, 0.011813978522206043, 0.035823639860549865, -0.23459179571824335, 0.0714304917030273, 1.0}
        x -= 2.0
        lp = EvaluatePolynomial(x, p)
        lq = EvaluatePolynomial(x, q)
        Return lz * lp / lq
    End Function
End Module
