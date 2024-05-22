#Region "Microsoft.VisualBasic::07923851bc993ca46173c88637f36761, Data_science\Mathematica\Math\Math.Statistics\Distributions\Distribution.vb"

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

    '   Total Lines: 352
    '    Code Lines: 236 (67.05%)
    ' Comment Lines: 69 (19.60%)
    '    - Xml Docs: 98.55%
    ' 
    '   Blank Lines: 47 (13.35%)
    '     File Size: 13.98 KB


    '     Class Distribution
    ' 
    '         Function: Beta, ChiSquare, ChiSquareInverse, Ex, FDistribution
    '                   FDistributionInverse, GammaLn, TDistribution, TDistributionInverse, ZInverse
    '                   ZNormal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports std = System.Math

Namespace Distributions

    Public Class Distribution

        Public Const Z_MAX As Double = 6.0
        Public Const Z_EPSILON As Double = 0.000001
        Public Const CHI_EPSILON As Double = 0.000001
        Public Const CHI_MAX As Double = 99999.0
        Public Const LOG_SQRT_PI As Double = 0.57236494292470008
        Public Const I_SQRT_PI As Double = 0.56418958354775628
        Public Const BIGX As Double = 200.0
        Public Const I_PI As Double = 0.31830988618379069
        Public Const F_EPSILON As Double = 0.000001
        Public Const F_MAX As Double = 9999.0

        ''' <summary>
        ''' The ex method takes a double x as an input, if x is less than -BIGX it returns 0, otherwise it returns Euler's number
        ''' <i>e</i> raised to the power of x.
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns>0 if input is less than -BIGX, Euler's number <i>e</i> raised to the power of x otherwise.</returns>
        Private Shared Function Ex(x As Double) As Double
            If x < -BIGX Then
                Return 0
            End If

            Return System.Math.Exp(x)
        End Function

        ''' <summary>
        ''' The beta method takes a double {@link java.lang.reflect.Array} x as an input. It loops through x and accumulates
        ''' the value of gammaLn(x), also it sums up the items of x and returns (accumulated result - gammaLn of this summation).
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns>gammaLn(sum).</returns>
        Public Shared Function Beta(x As Double()) As Double
            Dim sum = 0.0, result = 0.0
            Dim i As Integer
            For i = 0 To x.Length - 1
                result += GammaLn(x(i))
                sum += x(i)
            Next

            result -= GammaLn(sum)
            Return result
        End Function

        ''' <summary>
        ''' The gammaLn method takes a double x as an input and returns the logarithmic result of the gamma distribution at point x.
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns>the logarithmic result of the gamma distribution at point x.</returns>
        Public Shared Function GammaLn(x As Double) As Double
            Dim cof = {76.180091729471457, -86.505320329416776, 24.014098240830911, -1.231739572450155, 0.001208650973866179, -0.000005395239384953}
            Dim j As Integer
            Dim y As i32 = x + 1
            Dim tmp = x + 5.5
            tmp -= (x + 0.5) * System.Math.Log(tmp)
            Dim ser = 1.0000000001900149
            For j = 0 To 5
                ser += cof(j) / (++y)
            Next

            Return -tmp + System.Math.Log(2.5066282746310007 * ser / x)
        End Function

        ''' <summary>
        ''' The zNormal method performs the Z-Normalization. It ensures, that all elements of the input vector are transformed
        ''' into the output vector whose mean is approximately 0 while the standard deviation is in a range close to 1.
        ''' </summary>
        ''' <param name="z"></param>
        ''' <returns>normalized value of given input.</returns>
        Public Shared Function ZNormal(z As Double) As Double
            Dim x As Double
            If z = 0.0 Then
                x = 0.0
            Else
                Dim y = 0.5 * std.Abs(z)
                If y >= Z_MAX * 0.5 Then
                    x = 1.0
                Else
                    If y < 1.0 Then
                        Dim w = y * y
                        x = ((((((((0.000124818987 * w - 0.001075204047) * w + 0.005198775019) * w - 0.019198292004) * w + 0.059054035642) * w - 0.151968751364) * w + 0.319152932694) * w - 0.5319230073) * w + 0.797884560593) * y * 2.0
                    Else
                        y -= 2.0
                        x = (((((((((((((-0.000045255659 * y + 0.00015252929) * y - 0.000019538132) * y - 0.000676904986) * y + 0.001390604284) * y - 0.00079462082) * y - 0.002034254874) * y + 0.006549791214) * y - 0.010557625006) * y + 0.011630447319) * y - 0.009279453341) * y + 0.005353579108) * y - 0.002141268741) * y + 0.000535310849) * y + 0.999936657524
                    End If
                End If
            End If

            If z > 0.0 Then
                Return (x + 1.0) * 0.5
            End If

            Return (1.0 - x) * 0.5
        End Function

        ''' <summary>
        ''' the zInverse method returns the Z-Inverse of given probability value.
        ''' </summary>
        ''' <param name="p">double probability.</param>
        ''' <returns>the Z-Inverse of given probability.</returns>
        Public Shared Function ZInverse(p As Double) As Double
            Dim minZ = -Z_MAX
            Dim maxZ = Z_MAX
            Dim zValue = 0.0
            If p <= 0.0 OrElse p >= 1.0 Then
                Return 0.0
            End If

            While maxZ - minZ > Z_EPSILON
                Dim pValue = ZNormal(zValue)
                If pValue > p Then
                    maxZ = zValue
                Else
                    minZ = zValue
                End If

                zValue = (maxZ + minZ) * 0.5
            End While

            Return zValue
        End Function

        ''' <summary>The chiSquare method is used to determine whether there is a significant difference between the expected
        '''  frequencies and the observed frequencies in one or more categories. It takes a double input x and an integer freedom
        '''  for degrees of freedom as inputs. It returns the Chi Squared result.</summary>
        ''' <param name="x">a numeric input.</param>
        ''' <param name="freedom">integer input for degrees of freedom.</param>
        '''  
        ''' <returns>the Chi Squared result.</returns>
        Public Shared Function ChiSquare(x As Double, freedom As Integer) As Double
            Dim y As Double = 0
            If x <= 0.0 OrElse freedom < 1 Then
                Return 1.0
            End If

            Dim a = 0.5 * x
            Dim even = freedom Mod 2 = 0
            If freedom > 1 Then
                y = Ex(-a)
            End If

            Dim s = If(even, y, 2.0 * ZNormal(-System.Math.Sqrt(x)))

            If freedom > 2 Then
                x = 0.5 * (freedom - 1.0)
                Dim z = If(even, 1.0, 0.5)

                Dim e As Double
                Dim c As Double
                If a > BIGX Then
                    e = If(even, 0.0, LOG_SQRT_PI)

                    c = System.Math.Log(a)
                    While z <= x
                        e = System.Math.Log(z) + e
                        s += Ex(c * z - a - e)
                        z += 1.0
                    End While

                    Return s
                End If

                If even Then
                    e = 1.0
                Else
                    e = I_SQRT_PI / System.Math.Sqrt(a)
                End If

                c = 0.0
                While z <= x
                    e *= a / z
                    c += e
                    z += 1.0
                End While

                Return c * y + s
            End If

            Return s
        End Function

        ''' <summary>
        ''' The chiSquareInverse method returns the Chi Square-Inverse of given probability value with given degree of freedom.
        ''' </summary>
        ''' <param name="p">double probability.</param>
        ''' <param name="freedom">integer input for degrees of freedom.</param>
        ''' <returns>the chiSquare-Inverse of given probability.</returns>
        Public Shared Function ChiSquareInverse(p As Double, freedom As Integer) As Double
            Dim minChiSquare = 0.0
            Dim maxChiSquare = CHI_MAX
            If p <= 0.0 Then
                Return maxChiSquare
            End If

            If p >= 1.0 Then
                Return 0.0
            End If

            Dim chiSquareValue = freedom / System.Math.Sqrt(p)
            While maxChiSquare - minChiSquare > CHI_EPSILON
                If ChiSquare(chiSquareValue, freedom) < p Then
                    maxChiSquare = chiSquareValue
                Else
                    minChiSquare = chiSquareValue
                End If

                chiSquareValue = (maxChiSquare + minChiSquare) * 0.5
            End While

            Return chiSquareValue
        End Function

        ''' <summary>
        ''' The fDistribution method is used to observe whether two samples have the same variance. It takes a double input F
        ''' and two integer freedom1 and freedom2 for degrees of freedom as inputs. It returns the F-Distribution result.
        ''' </summary>
        ''' <param name="fValue"></param>
        ''' <param name="freedom1">integer input for degrees of freedom.</param>
        ''' <param name="freedom2">integer input for degrees of freedom.</param>
        ''' <returns>the F-Distribution result.</returns>
        Public Shared Function FDistribution(fValue As Double, freedom1 As Integer, freedom2 As Integer) As Double
            Dim i, j As Integer
            Dim y, d, p As Double
            If fValue < F_EPSILON OrElse freedom1 < 1 OrElse freedom2 < 1 Then
                Return 1.0
            End If

            Dim a = If(freedom1 Mod 2 <> 0, 1, 2)

            Dim b = If(freedom2 Mod 2 <> 0, 1, 2)

            Dim w = fValue * freedom1 / freedom2
            Dim z = 1.0 / (1.0 + w)
            If a = 1 Then
                If b = 1 Then
                    p = System.Math.Sqrt(w)
                    y = I_PI
                    d = y * z / p
                    p = 2.0 * y * System.Math.Atan(p)
                Else
                    p = System.Math.Sqrt(w * z)
                    d = 0.5 * p * z / w
                End If
            Else
                If b = 1 Then
                    p = System.Math.Sqrt(z)
                    d = 0.5 * z * p
                    p = 1.0 - p
                Else
                    d = z * z
                    p = w * z
                End If
            End If

            y = 2.0 * w / z
            For j = b + 2 To freedom2 Step 2
                d *= (1.0 + a / (j - 2.0)) * z
                If a = 1 Then
                    p = p + d * y / (j - 1.0)
                Else
                    p = (p + w) * z
                End If
            Next

            y = w * z
            z = 2.0 / z
            b = freedom2 - 2
            For i = a + 2 To freedom1 Step 2
                j = i + b
                d *= y * j / (i - 2.0)
                p -= z * d / j
            Next

            If p < 0.0 Then
                p = 0.0
            Else
                If p > 1.0 Then
                    p = 1.0
                End If
            End If

            Return 1.0 - p
        End Function

        ''' <summary>
        ''' The fDistributionInverse method returns the F-Distribution Inverse of given probability value.
        ''' </summary>
        ''' <param name="p">double probability.</param>
        ''' <param name="freedom1">integer input for degrees of freedom.</param>
        ''' <param name="freedom2">integer input for degrees of freedom.</param>
        ''' <returns>the F-Distribution Inverse of given probability.</returns>
        Public Shared Function FDistributionInverse(p As Double, freedom1 As Integer, freedom2 As Integer) As Double
            Dim maxF = F_MAX
            Dim minF = 0.0
            If p <= 0.0 OrElse p >= 1.0 Then
                Return 0.0
            End If

            If freedom1 = freedom2 AndAlso freedom1 > 2500 Then
                Return 1 + 4.0 / freedom1
            End If

            Dim fValue = 1.0 / p
            While std.Abs(maxF - minF) > F_EPSILON
                If FDistribution(fValue, freedom1, freedom2) < p Then
                    maxF = fValue
                Else
                    minF = fValue
                End If

                fValue = (maxF + minF) * 0.5
            End While

            Return fValue
        End Function

        ''' <summary>
        ''' The tDistribution method is used instead of the normal distribution when there is small samples. It takes a double input T
        ''' and an integer freedom for degree of freedom as inputs. It returns the T-Distribution result by using F-Distribution method.
        ''' </summary>
        ''' <param name="T"></param>
        ''' <param name="freedom">integer input for degrees of freedom.</param>
        ''' <returns>the T-Distribution result.</returns>
        Public Shared Function TDistribution(T As Double, freedom As Integer) As Double
            If T >= 0 Then
                Return FDistribution(T * T, 1, freedom) / 2
            Else
                Return 1 - FDistribution(T * T, 1, freedom) / 2
            End If
        End Function

        ''' <summary>
        ''' The tDistributionInverse method returns the T-Distribution Inverse of given probability value.
        ''' </summary>
        ''' <param name="p">double probability.</param>
        ''' <param name="freedom">integer input for degrees of freedom.</param>
        ''' <returns>the T-Distribution Inverse of given probability.</returns>
        Public Shared Function TDistributionInverse(p As Double, freedom As Integer) As Double
            If p < 0.5 Then
                Return System.Math.Sqrt(FDistributionInverse(p * 2, 1, freedom))
            Else
                Return -System.Math.Sqrt(FDistributionInverse((1 - p) * 2, 1, freedom))
            End If
        End Function
    End Class
End Namespace
