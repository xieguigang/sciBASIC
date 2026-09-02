#Region "Microsoft.VisualBasic::33d8ac1dfe418648af51cb986ee187ca, Data_science\DataMining\UMAP\Components\ABParams.vb"

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

    '   Total Lines: 274
    '    Code Lines: 127 (46.35%)
    ' Comment Lines: 106 (38.69%)
    '    - Xml Docs: 70.75%
    ' 
    '   Blank Lines: 41 (14.96%)
    '     File Size: 10.36 KB


    ' Module ABParams
    ' 
    '     Properties: DefaultAB
    ' 
    '     Function: EvalSSR, FindABParams, Fit, LevenbergMarquardt
    ' 
    '     Sub: ClearCache
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Concurrent
Imports System.Runtime.CompilerServices
Imports std = System.Math

''' <summary>
''' Fit the ``a``, ``b`` parameters for the differentiable curve that is 
''' used in the construction of the low dimensional fuzzy simplicial complex.
''' </summary>
''' <remarks>
''' This module is the VB.NET equivalent of the ``umap.umap_.find_ab_params`` 
''' function of the python implementation:
''' 
''' ```
''' def curve(x, a, b):
'''     return 1.0 / (1.0 + a * x ** (2 * b))
''' 
''' xv = np.linspace(0, spread * 3, 300)
''' yv = np.zeros(xv.shape)
''' yv[xv &lt; min_dist] = 1.0
''' yv[xv &gt;= min_dist] = np.exp(-(xv[xv &gt;= min_dist] - min_dist) / spread)
''' params, covar = curve_fit(curve, xv, yv)
''' return params[0], params[1]
''' ```
''' 
''' the ``curve_fit`` function of the scipy is implemented based on the 
''' Levenberg-Marquardt algorithm, a lightweight damped least squares solver 
''' is implemented at here for fit the two parameters of the curve function, 
''' so that there is no need to reference any external numerical library.
''' </remarks>
Public Module ABParams

    ''' <summary>
    ''' the number of the sample points for curve fitting, this value is 
    ''' the same as the python implementation.
    ''' </summary>
    Const nSamples As Integer = 300
    ''' <summary>
    ''' the tolerance of the parameter comparison of the fast path
    ''' </summary>
    Const tolerance As Double = 0.000000001

    ''' <summary>
    ''' the pre-calculated result of the default configuration 
    ''' (spread = 1, minDist = 0.1), this value is the same as the original 
    ''' hardcoded constant of this VB.NET implementation, so that the 
    ''' default behaviour is not changed at all.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property DefaultAB As (a As Double, b As Double) = (1.56947052F, 0.8941996F)

    ''' <summary>
    ''' cache of the fitted result: [spread|minDist] => (a, b)
    ''' </summary>
    ReadOnly cache As New ConcurrentDictionary(Of String, (a As Double, b As Double))()

    ''' <summary>
    ''' Fit a, b params for the differentiable curve used in lower dimensional 
    ''' fuzzy simplicial complex construction. We want the smooth curve (from 
    ''' a pre-defined family with simple gradient) that best matches an offset 
    ''' exponential decay.
    ''' </summary>
    ''' <param name="spread">
    ''' The effective scale of embedded points, must be a positive value.
    ''' </param>
    ''' <param name="minDist">
    ''' The effective minimum distance between embedded points, must be a 
    ''' non-negative value.
    ''' </param>
    ''' <returns>
    ''' the ``a`` and ``b`` parameters of the curve 
    ''' ``1.0 / (1.0 + a * x ^ (2 * b))``.
    ''' </returns>
    Public Function FindABParams(spread As Double, minDist As Double) As (a As Double, b As Double)
        If spread <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(spread), "the spread parameter should be a positive value!")
        End If
        If minDist < 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(minDist), "the minDist parameter should be a non-negative value!")
        End If
        If Double.IsNaN(spread) OrElse Double.IsNaN(minDist) Then
            Throw New ArgumentException("the spread/minDist parameter can not be NaN!")
        End If

        ' keep the original hardcoded result for the default configuration
        ' so that the default behaviour is bit-level identical to the 
        ' previous version of this implementation
        '
        ' a tolerance is applied at here: the same logical parameter value 
        ' should always produce the same result, no matter the value comes 
        ' from the optional default (a Single 0.1F literal) or from an 
        ' explicit Double 0.1 literal of the caller.
        If std.Abs(spread - 1.0) < tolerance AndAlso std.Abs(minDist - 0.1) < tolerance Then
            Return DefaultAB
        End If

        Dim key As String = $"{spread.ToString("R")}|{minDist.ToString("R")}"

        Return cache.GetOrAdd(key, Function(any) Fit(spread, minDist))
    End Function

    ''' <summary>
    ''' remove all of the cached fitting result
    ''' </summary>
    Public Sub ClearCache()
        Call cache.Clear()
    End Sub

    Private Function Fit(spread As Double, minDist As Double) As (a As Double, b As Double)
        Dim xv As Double() = New Double(nSamples - 1) {}
        Dim yv As Double() = New Double(nSamples - 1) {}
        Dim right As Double = spread * 3
        Dim stepX As Double = right / (nSamples - 1)

        For i As Integer = 0 To nSamples - 1
            Dim x As Double = i * stepX

            xv(i) = x

            If x < minDist Then
                yv(i) = 1.0
            Else
                yv(i) = std.Exp(-(x - minDist) / spread)
            End If
        Next

        ' the initial guess of the curve_fit function of the scipy is a 
        ' vector of all ones
        Dim a As Double = 1.0
        Dim b As Double = 1.0

        If Not LevenbergMarquardt(xv, yv, a, b) Then
            ' degradation: keep the result of the default configuration 
            ' if the solver is failed to converge
            Return DefaultAB
        End If

        Return (a, b)
    End Function

    ''' <summary>
    ''' evaluate the sum of squared residuals of the current curve parameters
    ''' </summary>
    Private Function EvalSSR(x As Double(), y As Double(), a As Double, b As Double) As Double
        Dim ssr As Double = 0

        For i As Integer = 0 To x.Length - 1
            Dim p As Double = If(x(i) > 0, std.Pow(x(i), 2 * b), 0)
            Dim f As Double = 1.0 / (1.0 + a * p)
            Dim r As Double = y(i) - f

            ssr += r * r
        Next

        Return ssr
    End Function

    ''' <summary>
    ''' A lightweight Levenberg-Marquardt (damped least squares) solver for 
    ''' fit the two parameters curve ``f(x) = 1 / (1 + a * x ^ (2 * b))``.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <param name="a">[in/out] the initial guess of the parameter ``a``</param>
    ''' <param name="b">[in/out] the initial guess of the parameter ``b``</param>
    ''' <param name="maxIter"></param>
    ''' <param name="tol"></param>
    ''' <returns>
    ''' TRUE if the solver is converged, otherwise FALSE.
    ''' </returns>
    Private Function LevenbergMarquardt(x As Double(), y As Double(),
                                        ByRef a As Double,
                                        ByRef b As Double,
                                        Optional maxIter As Integer = 200,
                                        Optional tol As Double = 0.000000000001) As Boolean

        Dim m As Integer = x.Length
        Dim lambda As Double = 0.001
        Dim ssr As Double = EvalSSR(x, y, a, b)

        If Double.IsNaN(ssr) Then
            Return False
        End If

        For iter As Integer = 0 To maxIter - 1
            ' accumulates the normal equation of the Gauss-Newton approximation:
            ' 
            '   JtJ * d = Jtr
            '
            ' where J is the jacobian matrix of the curve function and r is 
            ' the residual vector r = y - f(x)
            Dim j00 As Double = 0, j01 As Double = 0, j11 As Double = 0
            Dim g0 As Double = 0, g1 As Double = 0

            For i As Integer = 0 To m - 1
                Dim p As Double = If(x(i) > 0, std.Pow(x(i), 2 * b), 0)
                Dim f As Double = 1.0 / (1.0 + a * p)
                ' residual: r = y - f(x)
                Dim r As Double = y(i) - f

                ' df/da = -f^2 * x^(2b)
                Dim da As Double = -f * f * p
                ' df/db = -f^2 * a * x^(2b) * 2 * ln(x)
                ' note about that the limit of x^(2b) * ln(x) is zero when x -> 0
                Dim db As Double = 0

                If x(i) > 0 Then
                    db = -f * f * a * p * 2 * std.Log(x(i))
                End If

                j00 += da * da
                j01 += da * db
                j11 += db * db
                g0 += da * r
                g1 += db * r
            Next

            If Double.IsNaN(j00) OrElse Double.IsNaN(j11) Then
                Return False
            End If

            ' the damping term: (JtJ + lambda * diag(JtJ)) * d = Jtr
            Dim h00 As Double = j00 * (1.0 + lambda) + 1.0E-12
            Dim h11 As Double = j11 * (1.0 + lambda) + 1.0E-12
            Dim det As Double = h00 * h11 - j01 * j01

            If std.Abs(det) < 1.0E-30 Then
                ' the hessian matrix is singular, stop the iteration
                Exit For
            End If

            Dim da2 As Double = (h11 * g0 - j01 * g1) / det
            Dim db2 As Double = (h00 * g1 - j01 * g0) / det
            Dim newA As Double = std.Max(a + da2, 1.0E-12)
            Dim newB As Double = std.Max(b + db2, 1.0E-12)
            Dim newSSR As Double = EvalSSR(x, y, newA, newB)

            If Double.IsNaN(newSSR) OrElse Double.IsInfinity(newSSR) Then
                lambda *= 10

                If lambda > 1.0E+12 Then
                    Exit For
                End If

                Continue For
            End If

            If newSSR < ssr Then
                Dim gain As Double = ssr - newSSR

                ssr = newSSR
                a = newA
                b = newB
                lambda = std.Max(lambda * 0.3, 1.0E-12)

                If gain < tol * (ssr + tol) Then
                    ' converged
                    Return True
                End If
            Else
                lambda *= 10

                If lambda > 1.0E+12 Then
                    Exit For
                End If
            End If
        Next

        ' the solver is stopped by the max iteration limits, the current 
        ' parameters is still a valid result if the sum of squared residuals 
        ' is a finite number
        Return Not (Double.IsNaN(a) OrElse Double.IsNaN(b))
    End Function

End Module
