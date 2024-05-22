#Region "Microsoft.VisualBasic::cc5ee750fec3f85eedeffd4537f53201, Microsoft.VisualBasic.Core\src\Extensions\Math\Correlations\Beta.vb"

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

    '   Total Lines: 148
    '    Code Lines: 115 (77.70%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 33 (22.30%)
    '     File Size: 4.54 KB


    '     Module Beta
    ' 
    '         Function: betacf, betai, erfcc, gammln
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports stdNum = System.Math

Namespace Math.Correlations

    Public Module Beta

        Const SWITCH As Integer = 3000
        Const EPS As Double = 0.0000003, FPMIN As Double = 1.0E-30

        Public MAXIT As Integer = 5000

        Public Function betai(a As Double, b As Double, x As Double, Optional throwMaxIterError As Boolean = True) As Double
            Dim bt As Double

            If x < 0.0 OrElse x > 1.0 Then
                Throw New ArgumentException($"Bad x:={x} in routine betai")
            End If

            If x = 0.0 OrElse x = 1.0 Then
                bt = 0.0
            Else
                bt = stdNum.Exp(gammln(a + b) - gammln(a) - gammln(b) + a * stdNum.Log(x) + b * stdNum.Log(1.0 - x))
            End If

            If x < (a + 1.0) / (a + b + 2.0) Then
                Return bt * betacf(a, b, x, throwMaxIterError) / a
            Else
                Return 1.0 - bt * betacf(b, a, 1.0 - x, throwMaxIterError) / b
            End If
        End Function

        Private Function gammln(xx As Double) As Double
            Dim x As Double = xx, y As Double, tmp As Double, ser As Double

            y = x
            tmp = x + 5.5
            tmp -= (x + 0.5) * stdNum.Log(tmp)
            ser = 1.00000000019001

            For j As Integer = 0 To 5
                y += 1 ' ++y
                ser += cof(j) / y
            Next

            Return -tmp + stdNum.Log(2.506628274631 * ser / x)
        End Function

        ReadOnly cof As Double() = {
            76.1800917294715,
            -86.5053203294168,
            24.0140982408309,
            -1.23173957245015,
            0.00120865097386618,
            -0.000005395239384953
        }

        Const MaxIterReachError$ = "a:={0} or b:={1} too big, or MAXIT too small in betacf"

        Private Function betacf(a As Double, b As Double, x As Double, throwMaxIterError As Boolean) As Double
            Dim m As Integer, m2 As Integer
            Dim aa As Double,
                c As Double,
                d As Double,
                del As Double,
                h As Double,
                qab As Double,
                qam As Double,
                qap As Double

            qab = a + b
            qap = a + 1.0
            qam = a - 1.0
            c = 1.0
            d = 1.0 - qab * x / qap

            If stdNum.Abs(d) < FPMIN Then
                d = FPMIN
            End If

            d = 1.0 / d
            h = d

            For m = 1 To MAXIT
                m2 = 2 * m
                aa = m * (b - m) * x / ((qam + m2) * (a + m2))
                d = 1.0 + aa * d

                If stdNum.Abs(d) < FPMIN Then
                    d = FPMIN
                End If

                c = 1.0 + aa / c

                If stdNum.Abs(c) < FPMIN Then
                    c = FPMIN
                End If

                d = 1.0 / d
                h *= d * c
                aa = -(a + m) * (qab + m) * x / ((a + m2) * (qap + m2))
                d = 1.0 + aa * d

                If stdNum.Abs(d) < FPMIN Then
                    d = FPMIN
                End If

                c = 1.0 + aa / c

                If stdNum.Abs(c) < FPMIN Then
                    c = FPMIN
                End If

                d = 1.0 / d
                del = d * c
                h *= del

                If stdNum.Abs(del - 1.0) < EPS Then
                    Exit For
                End If
            Next

            If m > MAXIT AndAlso throwMaxIterError Then
                Throw New ArgumentException(String.Format(MaxIterReachError, a, b))
            End If

            Return h
        End Function

        Public Function erfcc(x As Double) As Double
            Dim t As Double, z As Double, ans As Double

            z = stdNum.Abs(x)
            t = 1.0 / (1.0 + 0.5 * z)
            ans = t * stdNum.Exp(-z * z - 1.26551223 +
                           t * (1.00002368 +
                           t * (0.37409196 +
                           t * (0.09678418 +
                           t * (-0.18628806 +
                           t * (0.27886807 +
                           t * (-1.13520398 +
                           t * (1.48851587 +
                           t * (-0.82215223 +
                           t * 0.17087277)))))))))

            Return If(x >= 0.0, ans, 2.0 - ans)
        End Function
    End Module
End Namespace
