#Region "Microsoft.VisualBasic::6e3a824633483da99a9d1758a675b37c, Data_science\Mathematica\Math\Math\Distributions\Gamma.vb"

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

'   Total Lines: 150
'    Code Lines: 86 (57.33%)
' Comment Lines: 43 (28.67%)
'    - Xml Docs: 93.02%
' 
'   Blank Lines: 21 (14.00%)
'     File Size: 4.76 KB


'     Module MathGamma
' 
'         Function: (+2 Overloads) gamma, lngamm, lngamma
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

Namespace Distributions

    ''' <summary>
    ''' gamma function (Γ) from mathematics
    ''' </summary>
    ''' <remarks>
    ''' |特性|lngamm函数|lngamma函数|
    ''' |----|----------|-----------|
    ''' |实现的方法|Lanczos 近似|Spouge 近似​|
    ''' |核心注释|引用 Lanczos 1964 年的论文，翻译自 Alan Miller 的 FORTRAN 代码。|注释标明适用于大参数（large arguments）。|
    ''' |系数数组|使用数组 p|使用数组 p_ln|
    ''' |函数定义|标准函数|扩展方法（带有 Extension 属性）|
    ''' |主要适用场景|通用的高精度计算|特别适合处理较大的参数值|
    ''' </remarks>
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

        ' 下面的两个函数 lngamm和 lngamma都实现了计算伽马函数的自然对数（LogGamma）的功能，但它们基于不同的数学近似方法。

        ' |特性|lngamm函数|lngamma函数|
        ' |----|----------|-----------|
        ' |实现的方法|Lanczos 近似|Spouge 近似​|
        ' |核心注释|引用 Lanczos 1964 年的论文，翻译自 Alan Miller 的 FORTRAN 代码。|注释标明适用于大参数（large arguments）。|
        ' |系数数组|使用数组 p|使用数组 p_ln|
        ' |函数定义|标准函数|扩展方法（带有 <Extension>属性）|
        ' |主要适用场景|通用的高精度计算|特别适合处理较大的参数值|

        ''' <summary>
        ''' Reference: "Lanczos, C. 'A precision approximation
        ''' of the gamma function', J. SIAM Numer. Anal., B, 1, 86-96, 1964."
        ''' Translation of  Alan Miller's FORTRAN-implementation
        ''' See http://lib.stat.cmu.edu/apstat/245
        ''' </summary>
        ''' <param name="Z"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' Lanczos 近似算法实现的log gamma函数，是一个通用的高精度计算方法
        '''
        ''' 参考资料：
        ''' 
        ''' - Lanczos, C. "A precision approximation of the gamma function", J. SIAM Numer. Anal., B, 1, 86-96, 1964.
        ''' - Alan Miller的FORTRAN实现翻译
        ''' - [http://lib.stat.cmu.edu/apstat/245](http://lib.stat.cmu.edu/apstat/245)
        ''' </remarks>
        Public Function lngamm(Z As Double) As Double
            Dim x As Double = 0.0

            x += 0.00000016594701874084621 / (Z + 7.0)
            x += 0.0000099349371139307475 / (Z + 6.0)
            x -= 0.1385710331296526 / (Z + 5.0)
            x += 12.50734324009056 / (Z + 4.0)
            x -= 176.61502914983859 / (Z + 3.0)
            x += 771.32342877576741 / (Z + 2.0)
            x -= 1259.1392167222889 / (Z + 1.0)
            x += 676.52036812188351 / (Z)
            x += 0.99999999999951827

            Return std.Log(x) - 5.5810614667953278 - Z + (Z - 0.5) * std.Log(Z + 6.5)
        End Function

        ''' <summary>
        ''' Spouge approximation (suitable for large arguments)
        ''' </summary>
        ''' <param name="z"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' Spouge 近似算法实现的log gamma函数，适用于大参数
        '''
        ''' 参考资料：
        ''' http://lib.stat.cmu.edu/apstat/245
        ''' </remarks>
        <Extension>
        Public Function lngamma(z As Double) As Double
            If (z < 0) Then Return 0

            Dim x As Double = p_ln(0)

            For i As Integer = p_ln.Length - 1 To 0 Step -1
                x += p_ln(i) / (z + i)
            Next

            Dim t As Double = z + g_ln + 0.5
            Dim lngm = 0.5 * std.Log(2 * std.PI) + (z + 0.5) * std.Log(t) - t + std.Log(x) - std.Log(z)

            Return lngm
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
                Return std.PI / (std.Sin(std.PI * z) * gamma(1 - z))
            ElseIf (z > 100) Then
                Return std.Exp(lngamma(z))
            Else
                Dim x As Double = p(0)

                z -= 1

                For i As Integer = 1 To g + 1
                    x += p(i) / (z + i)
                Next

                Dim t As Double = z + g + 0.5

                Return std.Sqrt(2 * std.PI) * std.Pow(t, z + 0.5) * std.Exp(-t) * x
            End If
        End Function
    End Module
End Namespace
