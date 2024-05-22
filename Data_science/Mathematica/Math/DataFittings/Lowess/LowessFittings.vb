#Region "Microsoft.VisualBasic::778bc3554a0ac39616dc79f52d2f3dba, Data_science\Mathematica\Math\DataFittings\Lowess\LowessFittings.vb"

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

    '   Total Lines: 301
    '    Code Lines: 187 (62.13%)
    ' Comment Lines: 66 (21.93%)
    '    - Xml Docs: 65.15%
    ' 
    '   Blank Lines: 48 (15.95%)
    '     File Size: 11.09 KB


    ' Module LowessFittings
    ' 
    '     Function: ascending, (+2 Overloads) Lowess, lowest
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math
Imports stdNum = System.Math

Public Module LowessFittings

    Private Function ascending(a As Double, b As Double) As Integer
        Dim d As Double = a - b

        If d = 0.0 Then
            Return 0
        Else
            Return stdNum.Sign(d)
        End If
    End Function

    ''' <summary>
    ''' Locally-weighted polynomial regression via the LOWESS algorithm.
    ''' </summary>
    ''' <param name="sample"></param>
    ''' <param name="f">smoother span (proportion of points which influence smoothing at each value)</param>
    ''' <param name="nsteps">number of iterations in the robust fit</param>
    ''' <returns></returns>
    <Extension>
    Public Function Lowess(sample As IEnumerable(Of PointF),
                           Optional f As Double = 2 / 3,
                           Optional nsteps As Integer = 3) As (x As Double(), y As Double())

        Dim data As PointF() = sample.OrderBy(Function(p) p.X).ToArray
        Dim x = data.Select(Function(p) CDbl(p.X)).ToArray
        Dim y = data.Select(Function(p) CDbl(p.Y)).ToArray
        Dim delta As Double = Interpolation.Range(data.Length, x, 1)

        Return LowessFittings.Lowess(x, y, data.Length, f, nsteps, delta)
    End Function

    ''' <summary>
    ''' Locally-weighted polynomial regression via the LOWESS algorithm.
    ''' </summary>
    ''' <param name="x">ordered x-axis values (abscissa values)</param>
    ''' <param name="y">corresponding y-axis values (ordinate values)</param>
    ''' <param name="n">number of observations</param>
    ''' <param name="f">smoother span (proportion of points which influence smoothing at each value)</param>
    ''' <param name="nsteps">number of iterations in the robust fit</param>
    ''' <param name="delta">nonnegative parameter which may be used to reduce the number of computations</param>
    ''' <returns>
    ''' sorted x-values and fitted values
    ''' </returns>
    ''' <remarks>
    ''' Calculates fitted values using a nearest neighbor function and robust locally weighted regression of degree one with the tricube weight function.
    ''' 
    ''' -   Cleveland, William S. 1979. "Robust Locally and Smoothing Weighted Regression Scatterplots." _Journal of the American Statistical Association_ 74 (368): 829–36. doi:[10.1080/01621459.1979.10481038](https://doi.org/10.1080/01621459.1979.10481038).
    ''' -   Cleveland, William S. 1981. "Lowess: A program for smoothing scatterplots by robust locally weighted regression." _American Statistician_ 35 (1) 54–55. doi:[10.2307/2683591](https://doi.org/10.2307/2683591).
    ''' </remarks>
    Public Function Lowess(x As Double(),
                           y As Double(),
                           n As Integer,
                           Optional f As Double = 2 / 3,
                           Optional nsteps As Integer = 3,
                           Optional delta As Double = 0.01) As (x As Double(), y As Double())

        Dim nright As Integer
        Dim denom As Double
        Dim nleft As Integer
        Dim alpha As Double
        Dim cmad As Double
        Dim iter As Integer
        Dim last As Integer
        Dim cut As Double
        Dim res = New Double(n - 1) {}
        Dim m1 As Integer
        Dim m2 As Integer
        Dim ns As Integer
        Dim c1 As Double
        Dim c9 As Double
        Dim d1 As Double
        Dim d2 As Double
        Dim rw = New Double(n - 1) {}
        Dim ys = New Double(n - 1) {}
        Dim i As Integer
        Dim j As Integer
        Dim r As Double

        If n < 2 Then
            Return (x, y)
        End If

        ' Use at least two and at most n points:
        ns = stdNum.Max(stdNum.Min(stdNum.Floor(f * n), n), 2)

        ' Robustness iterations:
        For iter = 1 To nsteps + 1
            nleft = 0
            nright = ns - 1
            ' index of previously estimated point
            last = -1
            ' index of current point
            i = 0

            Do
                While nright < n - 1
                    ' Move nleft, nright to the right if radius decreases:
                    d1 = x(i) - x(nleft)
                    d2 = x(nright + 1) - x(i)

                    ' If d1 <= d2 with x[nright+1] == x[nright], lowest fixes:
                    If d1 <= d2 Then
                        Exit While
                    End If
                    ' Radius will not decrease by a move to the right...
                    nleft += 1
                    nright += 1
                End While

                ' Fitted value at x[ i ]:
                ys(i) = lowest(x, y, n, i, nleft, nright, res, iter > 1, rw)

                If last < i - 1 Then
                    denom = x(i) - x(last)

                    For j = last + 1 To i - 1
                        alpha = (x(j) - x(last)) / denom
                        ys(j) = alpha * ys(i) + (1.0 - alpha) * ys(last)
                    Next
                End If

                last = i
                cut = x(last) + delta

                For i = last + 1 To n - 1

                    If x(i) > cut Then
                        Exit For
                    End If

                    If x(i) = x(last) Then
                        ys(i) = ys(last)
                        last = i
                    End If
                Next

                i = stdNum.Max(last + 1, i - 1)
            Loop While last < n - 1

            ' Calculate Residuals:
            For i = 0 To n - 1
                res(i) = y(i) - ys(i)
            Next

            ' Compute robustness weights except last time...
            If iter > nsteps Then
                Exit For
            End If

            For i = 0 To n - 1
                rw(i) = stdNum.Abs(res(i))
            Next

            Call Array.Sort(rw, AddressOf ascending)

            m1 = stdNum.Floor(n / 2.0)
            m2 = n - m1 - 1.0
            cmad = 3.0 * (rw(m1) + rw(m2))
            c9 = 0.999 * cmad
            c1 = 0.001 * cmad

            For i = 0 To n - 1
                r = stdNum.Abs(res(i))

                If r <= c1 Then
                    ' near 0, avoid underflow
                    rw(i) = 1.0
                ElseIf r > c9 Then
                    ' near 1, avoid underflow
                    rw(i) = 0.0
                Else
                    rw(i) = stdNum.Pow(1.0 - stdNum.Pow(r / cmad, 2.0), 2.0)
                End If
            Next
        Next

        Return (x, y:=ys)
    End Function

    ''' <summary>
    ''' Calculates the fitted value `ys` for a value `xs` on the horizontal axis.
    ''' </summary>
    ''' <param name="x">ordered x-axis values (abscissa values)</param>
    ''' <param name="y">corresponding y-axis values (ordinate values)</param>
    ''' <param name="n">number of observations</param>
    ''' <param name="i">current index</param>
    ''' <param name="nleft">index of the first point used in computing the fitted value</param>
    ''' <param name="nright">index of the last point used in computing the fitted value</param>
    ''' <param name="w">weights at indices from `nleft` to `nright` to be used in the calculation of the fitted value</param>
    ''' <param name="userw">boolean indicating whether a robust fit is carried out using the weights in `rw`</param>
    ''' <param name="rw">robustness weights</param>
    ''' <returns>fitted value</returns>
    ''' <remarks>
    ''' The smoothed value for the x-axis value at the current index is computed using a (robust) locally weighted regression of degree one.  
    ''' The tricube weight function is used with `h` equal to the maximum of `xs - x[ nleft ]` and `x[ nright ] - xs`.
    ''' 
    ''' -   Cleveland, William S. 1979. "Robust Locally and Smoothing Weighted Regression Scatterplots." _Journal of the American Statistical Association_ 74 (368): 829–36. doi:[10.1080/01621459.1979.10481038](https://doi.org/10.1080/01621459.1979.10481038).
    ''' -   Cleveland, William S. 1981. "Lowess: A program for smoothing scatterplots by robust locally weighted regression." _American Statistician_ 35 (1) 54–55. doi:[10.2307/2683591](https://doi.org/10.2307/2683591).
    ''' </remarks>
    Private Function lowest(x As Double(),
                            y As Double(),
                            n As Integer,
                            i As Integer,
                            nleft As Integer,
                            nright As Integer,
                            w As Double(),
                            userw As Boolean,
                            rw As Double()) As Double

        Dim nrt As Integer
        Dim xs As Double = x(i)
        Dim ys As Double
        Dim b As Double
        Dim c As Double
        Dim r As Double
        Dim j As Integer
        Dim range As Double = x(n - 1) - x(0)
        Dim h As Double = stdNum.Max(xs - x(nleft), x(nright) - xs)
        Dim h9 As Double = 0.999 * h
        Dim h1 As Double = 0.001 * h

        ' Compute weights (pick up all ties on right):
        ' sum of weights
        Dim a As Double = 0.0

        For j = nleft To n - 1
            w(j) = 0.0
            r = stdNum.Abs(x(j) - xs)

            ' small enough for non-zero weight
            If r <= h9 Then
                If r > h1 Then
                    w(j) = stdNum.Pow(1.0 - stdNum.Pow(r / h, 3.0), 3.0)
                Else
                    w(j) = 1.0
                End If

                If userw Then
                    w(j) *= rw(j)
                End If

                a += w(j)
            ElseIf x(j) > xs Then
                ' get out at first zero weight on right
                Exit For
            End If
        Next

        ' rightmost point (may be greater than `nright` because of ties)
        nrt = j - 1

        If a <= 0.0 Then
            Return y(i)
        End If

        ' Make sum of weights equal to one:
        For j = nleft To nrt
            w(j) /= a
        Next

        If h > 0.0 Then
            ' use linear fit
            ' Find weighted center of x values:
            a = 0.0

            For j = nleft To nrt
                a += w(j) * x(j)
            Next

            b = xs - a
            c = 0.0

            For j = nleft To nrt
                c += w(j) * stdNum.Pow(x(j) - a, 2.0)
            Next

            If stdNum.Sqrt(c) > 0.001 * range Then
                ' Points are spread out enough to compute slope:
                b /= c

                For j = nleft To nrt
                    w(j) *= 1.0 + b * (x(j) - a)
                Next
            End If
        End If

        ys = 0.0

        For j = nleft To nrt
            ys += w(j) * y(j)
        Next

        Return ys
    End Function
End Module
