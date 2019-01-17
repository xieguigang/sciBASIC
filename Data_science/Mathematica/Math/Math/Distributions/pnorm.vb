Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.SyntaxAPI.MathExtension
Imports sys = System.Math

Namespace Distributions

    ''' <summary>
    ''' 正态分布帮助模块
    ''' </summary>
    Public Module pnorm

        '' rcpp_trunc_ndist
        ''
        '' Truncated normal distribution (mean 1, respective upper and lower limits of
        '' 0 and 2).
        ''
        '' @param len Number of elements to be simulated
        '' @param sd Standard deviation
        ''
        '' @return A vector of truncated normally distributed values
        ''
        ' [[Rcpp::export]]
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="len"></param>
        ''' <param name="sd"></param>
        ''' <returns></returns>
        ''' <remarks>https://github.com/mpadge/tnorm</remarks>
        Public Function TruncNDist(len%, sd#) As Vector
            ' Set up truncated normal distribution
            Dim eps As Vector
            Dim z As New List(Of Double)()

            While z.Count < len
                eps = Normal.rnorm(len, 1.0, sd)
                For Each it As Double In eps
                    If it >= 0.0 AndAlso it <= 2.0 Then
                        z.Add(it)
                    End If
                    it += 1
                Next
            End While

            Return New Vector(z)
        End Function

        ''' <summary>
        ''' 标准正态分布, delta = 1, u = 0
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Function StandardDistribution(x As Double) As Double
            Dim answer As Double = 1 / ((sys.Sqrt(2 * sys.PI)))
            Dim exp1 As Double = sys.Pow(x, 2) / 2
            Dim exp As Double = sys.Pow(sys.E, -(exp1))
            answer = answer * exp
            Return answer
        End Function

#Region "pnorm implementation"

        ''' <summary>
        ''' Density, distribution function, quantile function and random generation for the 
        ''' normal distribution with mean equal to mean and standard deviation equal to sd.
        ''' </summary>
        ''' <param name="q">vector of quantiles.</param>
        ''' <param name="mean">vector of means.</param>
        ''' <param name="sd">vector of standard deviations.</param>
        ''' <param name="lower_tail">logical; if TRUE (default), probabilities are ``P[X ≤ x]`` otherwise, ``P[X > x]``.</param>
        ''' <param name="logP">logical; if TRUE, probabilities p are given as log(p).</param>
        ''' <returns></returns>
        Public Function Eval(q#,
                             Optional mean# = 0,
                             Optional sd# = 1,
                             Optional lower_tail As Boolean = True,
                             Optional logP As Boolean = False,
                             Optional resolution% = 30000) As Double
            Dim p#

            If lower_tail Then
                p = pnorm.BelowStandardDistribution(q, resolution, mean, sd)
            Else
                p = pnorm.AboveStandardDistribution(q, resolution, mean, sd)
            End If

            If logP Then
                Return Math.Log10(p)
            Else
                Return p
            End If
        End Function

        ''' <summary>
        ''' Density, distribution function, quantile function and random generation for the 
        ''' normal distribution with mean equal to mean and standard deviation equal to sd.
        ''' </summary>
        ''' <param name="q">vector of quantiles.</param>
        ''' <param name="mean">vector of means.</param>
        ''' <param name="sd">vector of standard deviations.</param>
        ''' <param name="lower_tail">logical; if TRUE (default), probabilities are ``P[X ≤ x]`` otherwise, ``P[X > x]``.</param>
        ''' <param name="logP">logical; if TRUE, probabilities p are given as log(p).</param>
        ''' <returns></returns>
        Public Function Eval(q As Vector,
                             Optional mean# = 0,
                             Optional sd# = 1,
                             Optional lower_tail As Boolean = True,
                             Optional logP As Boolean = False,
                             Optional resolution% = 30000) As Vector
            Dim p As Vector
            Dim pdist As Func(Of Double, Double, Double, Double, Double)

            If lower_tail Then
                pdist = AddressOf pnorm.BelowStandardDistribution
            Else
                pdist = AddressOf pnorm.AboveStandardDistribution
            End If

            p = q.Select(Function(x)
                             Return pdist(x, resolution, mean, sd)
                         End Function) _
                 .AsVector

            If logP Then
                Return p.Log(base:=10)
            Else
                Return p
            End If
        End Function

        ''' <summary>
        ''' Normal Distribution.(正态分布)
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="m">Mean</param>
        ''' <param name="sd"></param>
        ''' <returns></returns>
        Public Function ProbabilityDensity(x#, m#, sd#) As Double
            Dim answer As Double = 1 / (sd * (sys.Sqrt(2 * sys.PI)))
            Dim exp As Double = sys.Pow((x - m), 2.0)
            Dim expP2 As Double = 2 * sys.Pow(sd, 2.0)
            Dim expP3 As Double = sys.Pow(sys.E, (-(exp / expP2)))
            answer = answer * expP3
            Return answer
        End Function

        <Extension>
        Public Function ProbabilityDensity(x As Vector, m#, sd#) As Vector
            Dim answer As Double = 1 / (sd * (sys.Sqrt(2 * sys.PI)))
            Dim exp = (x - m) ^ 2.0
            Dim expP2 As Double = 2 * sys.Pow(sd, 2.0)
            Dim expP3 = sys.E ^ -(exp / expP2)
            Dim y As Vector = answer * expP3

            Return y
        End Function

        Public Function AboveStandardDistribution(upperX As Double, resolution As Double, m As Double, sd As Double) As Double
            Dim lowerX As Double = m - 4.1 * sd
            Dim answer As Double = TrapezodialRule(lowerX, upperX, resolution, m, sd)

            Return 1 - answer
        End Function

        Public Function BelowStandardDistribution(upperX As Double, resolution As Double, m As Double, sd As Double) As Double
            Dim lowerX As Double = m + 4.1 * sd
            Dim answer As Double = TrapezodialRule(lowerX, upperX, resolution, m, sd)

            ' lol
            Return 1 + answer
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function BetweenStandardDistribution(lowerX As Double, upperX As Double, resolution As Double, m As Double, sd As Double) As Double
            Return TrapezodialRule(lowerX, upperX, resolution, m, sd)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function OutsideStandardDistribution(lowerX As Double, upperX As Double, resolution As Double, m As Double, sd As Double) As Double
            Return 1 - TrapezodialRule(lowerX, upperX, resolution, m, sd)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="a#"></param>
        ''' <param name="b#"></param>
        ''' <param name="resolution">计算的分辨率，越大越好</param>
        ''' <param name="m#"></param>
        ''' <param name="sd#"></param>
        ''' <returns></returns>
        Public Function TrapezodialRule(a#, b#, resolution%, m#, sd#) As Double
            Dim dx As Double = (b - a) / resolution
            Dim a1 As Double = ProbabilityDensity(a, m, sd)
            Dim b1 As Double = ProbabilityDensity(b, m, sd)
            Dim c As Double = 0.5 * (a1 + b1)

            For i As Integer = 1 To resolution - 1
                c = c + ProbabilityDensity((a + (i * dx)), m, sd)
            Next

            Return dx * c
        End Function
#End Region

    End Module
End Namespace