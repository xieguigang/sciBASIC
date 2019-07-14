#Region "Microsoft.VisualBasic::eb8bb1c84e0a4977f09c2bb21a9d28c0, Data_science\Mathematica\Math\Math\Distributions\Gamma.vb"

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

'     Module MathGamma
' 
'         Function: gamma, lngamma
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports sys = System.Math

Namespace Distributions

    ''' <summary>
    ''' gamma function (Γ) from mathematics
    ''' </summary>
    Public Module MathGamma

        Const g = 7

        ReadOnly p As Double() = {
            0.99999999999980993,
            676.5203681218851,
            -1259.1392167224028,
            771.32342877765313,
            -176.61502916214059,
            12.507343278686905,
            -0.13857109526572012,
            0.0000099843695780195716,
            0.00000015056327351493116
        }

        Const g_ln = 607 / 128

        ReadOnly p_ln As Double() = {
            0.99999999999999711,
            57.156235665862923,
            -59.597960355475493,
            14.136097974741746,
            -0.49191381609762019,
            0.000033994649984811891,
            0.000046523628927048578,
            -0.000098374475304879565,
            0.00015808870322491249,
            -0.00021026444172410488,
            0.00021743961811521265,
            -0.00016431810653676389,
            0.00008441822398385275,
            -0.000026190838401581408,
            0.0000036899182659531625
        }

        ''' <summary>
        ''' Spouge approximation (suitable for large arguments)
        ''' </summary>
        ''' <param name="z"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function lngamma(z As Double) As Double
            If (z < 0) Then Return 0

            Dim x As Double = p_ln(0)

            For i As Integer = p_ln.Length - 1 To 0 Step -1
                x += p_ln(i) / (z + i)
            Next

            Dim t As Double = z + g_ln + 0.5
            Return 0.5 * sys.Log(2 * sys.PI) + (z + 0.5) * sys.Log(t) - t + sys.Log(x) - sys.Log(z)
        End Function

        ''' <summary>
        ''' gamma function ``Γ`` from mathematics
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' Alias for <see cref="gamma"/>
        ''' 
        ''' Test:
        ''' 
        ''' ```
        ''' > var gamma = require('gamma')
        ''' > gamma(5)
        ''' 23.999999999999996
        ''' > gamma(1.6)
        ''' 0.8935153492876909
        ''' ```
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Γ(x As Double) As Double
            Return x.gamma
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function gamma(a As Vector) As Vector
            Return New Vector(From x As Double In a Select gamma(x))
        End Function

        ''' <summary>
        ''' Γ
        ''' </summary>
        ''' <param name="z"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function gamma(z As Double) As Double
            If (z < 0.5) Then
                Return sys.PI / (sys.Sin(Math.PI * z) * gamma(1 - z))
            ElseIf (z > 100) Then
                Return sys.Exp(lngamma(z))
            Else
                Dim x As Double = p(0)

                z -= 1

                For i As Integer = 1 To g + 1
                    x += p(i) / (z + i)
                Next

                Dim t As Double = z + g + 0.5
                Return sys.Sqrt(2 * sys.PI) * sys.Pow(t, z + 0.5) * sys.Exp(-t) * x
            End If
        End Function
    End Module
End Namespace
