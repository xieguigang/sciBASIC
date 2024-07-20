#Region "Microsoft.VisualBasic::c54dc1e0c0a7eeeaa1bc154a80f7e727, Data_science\Mathematica\Math\Math\NumberExtension.vb"

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

    '   Total Lines: 656
    '    Code Lines: 416 (63.41%)
    ' Comment Lines: 151 (23.02%)
    '    - Xml Docs: 28.48%
    ' 
    '   Blank Lines: 89 (13.57%)
    '     File Size: 23.71 KB


    ' Module NumberExtension
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: (+5 Overloads) [To], BuildDouble, Epsilon, EpsilonP, EpsilonPP
    '               Expm1, GetHighDWord, GetLowDWord, (+3 Overloads) GetRemainder, LeadingZeros
    '               (+2 Overloads) NextPowerOf2, Taylor
    ' 
    '     Sub: (+3 Overloads) MakeCT, (+3 Overloads) MakeIPT, (+2 Overloads) MakeWT
    ' 
    ' /********************************************************************************/

#End Region

' Copyright (c) 2017 - presented by Kei Nakai
'
' Original project is developed and published by OpenGamma Inc.
'
' Copyright (C) 2012 - present by OpenGamma Inc. and the OpenGamma group of companies
'
' Please see distribution for license.
'
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' 
'     http://www.apache.org/licenses/LICENSE-2.0
'     
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.

Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports std = System.Math

''' <summary>
''' https://github.com/cobaltblueocean/Mercury.Language.Extensions
''' </summary>
Public Module NumberExtension


    ' Coefficients for the Taylor expansion of (e^x-1)/x and its first two derivatives
    Private COEFF1 As Double()
    Private COEFF2 As Double()
    Private COEFF3 As Double()

    Sub New()
        COEFF1 = New Double() {1.0R / 24, 1.0R / 6, 1.0R / 2, 1.0R}
        COEFF2 = New Double() {1.0R / 144, 1.0R / 30, 1.0R / 8, 1.0R / 3, 1.0R / 2}
        COEFF3 = New Double() {1.0R / 168, 1.0R / 36, 1.0R / 10, 1.0R / 4, 1.0R / 3}
    End Sub

    <Extension()>
    Public Function [To](Of T As Structure)(i As Integer) As T
        Return Convert.ChangeType(i, GetType(T))
    End Function

    <Extension()>
    Public Function [To](Of T As Structure)(i As Double) As T
        Return Convert.ChangeType(i, GetType(T))
    End Function

    <Extension()>
    Public Function [To](Of T As Structure)(i As Single) As T
        Return Convert.ChangeType(i, GetType(T))
    End Function

    <Extension()>
    Public Function [To](Of T As Structure)(i As Long) As T
        Return Convert.ChangeType(i, GetType(T))
    End Function

    <Extension()>
    Public Function [To](Of T As Structure)(i As Decimal) As T
        Return Convert.ChangeType(i, GetType(T))
    End Function

    ''' <summary>
    ''' This is the Taylor expansion of $$\frac{\exp(x)-1}{x}$$ - note for $$|x| > 10^{-10}$$ the expansion is note used
    ''' </summary>
    ''' <param name="x">value</param>
    ''' <returns>result</returns>
    <Extension()>
    Public Function Epsilon(x As Double) As Double
        If std.Abs(x) > 0.0000000001 Then
            Return Expm1(x) / x
        End If
        Return x.Taylor(COEFF1)
    End Function

    ''' <summary>
    ''' This is the Taylor expansion of the first derivative of $$\frac{\exp(x)-1}{x}$$
    ''' </summary>
    ''' <param name="x">value</param>
    ''' <returns>result</returns>
    <Extension()>
    Public Function EpsilonP(x As Double) As Double

        If std.Abs(x) > 0.0000001 Then
            Return ((x - 1) * Expm1(x) + x) / x / x
        End If
        Return x.Taylor(COEFF2)
    End Function

    ''' <summary>
    ''' This is the Taylor expansion of the second derivative of $$\frac{\exp(x)-1}{x}$$
    ''' </summary>
    ''' <param name="x">value</param>
    ''' <returns>result</returns>
    <Extension()>
    Public Function EpsilonPP(x As Double) As Double

        If std.Abs(x) > 0.00001 Then
            Dim x2 = x * x
            Dim x3 = x * x2
            Return (Expm1(x) * (x2 - 2 * x + 2) + x2 - 2 * x) / x3
        End If
        Return x.Taylor(COEFF3)
    End Function

    <Extension()>
    Private Function Taylor(x As Double, coeff As Double()) As Double
        Dim sum = coeff(0)
        Dim n = coeff.Length
        For i = 1 To n - 1
            sum = coeff(i) + x * sum
        Next
        Return sum
    End Function

    <Extension()>
    Public Function LeadingZeros(x As Integer) As Integer
        ' compile time constant
        Static numIntBits As Integer = Marshal.SizeOf(GetType(Integer)) * 8

        'do the smearing
        x = x Or x >> 1
        x = x Or x >> 2
        x = x Or x >> 4
        x = x Or x >> 8
        x = x Or x >> 16
        'count the ones
        x -= x >> 1 And &H55555555
        x = (x >> 2 And &H33333333) + (x And &H33333333)
        x = (x >> 4) + x And &HF0F0F0F
        x += x >> 8
        x += x >> 16
        Return numIntBits - (x And &H3F) 'subtract # of 1s from 32
    End Function

    <Extension()>
    Public Function NextPowerOf2(x As Integer) As Integer
        If x < 0 Then Throw New ArgumentException()

        Return If(x = 0, 0, 32 - x - 1.LeadingZeros())
    End Function

    <Extension()>
    Public Function NextPowerOf2(x As Long) As Integer
        If x < 0 Then Throw New ArgumentException()

        Return If(x = 0, 0, 32 - CInt(x - 1).LeadingZeros())
    End Function

    <Extension()>
    Public Function GetRemainder(n As Integer, factors As Integer()) As Long
        Return CLng(n).GetRemainder(factors)
    End Function

    <Extension()>
    Public Function GetRemainder(n As Long, factors As Integer()) As Long
        Dim longfactors = Array.ConvertAll(Of Integer, Long)(factors, Function(i) i)

        Return n.GetRemainder(longfactors)
    End Function

    <Extension()>
    Public Function GetRemainder(n As Long, factors As Long()) As Long
        Dim reminder = n

        If n <= 0 Then
            Throw New ArgumentException("n_MUST_BE_POSITIVE_INT")
        End If

        Dim i = 0

        While i < factors.Length AndAlso reminder <> 1L
            Dim factor = factors(i)
            While reminder Mod factor = 0
                reminder /= factor
            End While

            i += 1
        End While
        Return reminder
    End Function

    <Extension()>
    Public Sub MakeIPT(nw As Integer, ByRef ip As Long())
        Dim j, l, m, m2, p, q As Long

        ip(2) = 0
        ip(3) = 16
        m = 2
        l = nw

        While l > 32
            m2 = m << 1
            q = m2 << 3
            For j = m To m2 - 1
                p = ip(j) << 2
                ip(m + j) = p
                ip(m2 + j) = p + q
            Next
            m = m2
            l >>= 2
        End While
    End Sub

    <Extension()>
    Public Sub MakeIPT(nw As Long, ByRef ip As Long())
        Dim j, l, m, m2, p, q As Long

        ip(2) = 0
        ip(3) = 16
        m = 2
        l = nw

        While l > 32
            m2 = m << 1
            q = m2 << 3
            For j = m To m2 - 1
                p = ip(j) << 2
                ip(m + j) = p
                ip(m2 + j) = p + q
            Next
            m = m2
            l >>= 2
        End While
    End Sub


    <Extension()>
    Public Sub MakeIPT(nw As Integer, ByRef ip As Integer())
        Dim j, l, m, m2, p, q As Integer

        ip(2) = 0
        ip(3) = 16
        m = 2
        l = nw

        While l > 32
            m2 = m << 1
            q = m2 << 3
            For j = m To m2 - 1
                p = ip(j) << 2
                ip(m + j) = p
                ip(m2 + j) = p + q
            Next
            m = m2
            l >>= 2
        End While
    End Sub

    <Extension()>
    Public Sub MakeWT(nw As Integer, ByRef ip As Integer(), ByRef w As Double())
        Dim lip = Array.ConvertAll(Of Integer, Long)(ip, Function(i) i)

        MakeWT(nw, lip, w)

        ip = Array.ConvertAll(Of Long, Integer)(lip, Function(i) i)
    End Sub

    <Extension()>
    Public Sub MakeWT(nw As Long, ByRef ip As Long(), ByRef w As Double())
        Dim j, nwh, nw0, nw1 As Long
        Dim delta, wn4r, wk1r, wk1i, wk3r, wk3i As Double
        Dim delta2, deltaj, deltaj3 As Double

        ip(0) = nw
        ip(1) = 1
        If nw > 2 Then
            nwh = nw >> 1
            delta = 0.78539816339744828 / nwh
            delta2 = delta * 2
            wn4r = std.Cos(delta * nwh)
            w(0) = 1
            w(1) = wn4r
            If nwh = 4 Then
                w(2) = std.Cos(delta2)
                w(3) = std.Sin(delta2)
            ElseIf nwh > 4 Then
                nw.MakeIPT(ip)
                w(2) = 0.5 / std.Cos(delta2)
                w(3) = 0.5 / std.Cos(delta * 6)
                For j = 4 To nwh - 1 Step 4
                    deltaj = delta * j
                    deltaj3 = 3 * deltaj
                    w(j) = std.Cos(deltaj)
                    w(j + 1) = std.Sin(deltaj)
                    w(j + 2) = std.Cos(deltaj3)
                    w(j + 3) = -std.Sin(deltaj3)
                Next
            End If
            nw0 = 0
            While nwh > 2
                nw1 = nw0 + nwh
                nwh >>= 1
                w(nw1) = 1
                w(nw1 + 1) = wn4r
                If nwh = 4 Then
                    wk1r = w(nw0 + 4)
                    wk1i = w(nw0 + 5)
                    w(nw1 + 2) = wk1r
                    w(nw1 + 3) = wk1i
                ElseIf nwh > 4 Then
                    wk1r = w(nw0 + 4)
                    wk3r = w(nw0 + 6)
                    w(nw1 + 2) = 0.5 / wk1r
                    w(nw1 + 3) = 0.5 / wk3r
                    For j = 4 To nwh - 1 Step 4
                        Dim idx1 = nw0 + 2 * j
                        Dim idx2 = nw1 + j
                        wk1r = w(idx1)
                        wk1i = w(idx1 + 1)
                        wk3r = w(idx1 + 2)
                        wk3i = w(idx1 + 3)
                        w(idx2) = wk1r
                        w(idx2 + 1) = wk1i
                        w(idx2 + 2) = wk3r
                        w(idx2 + 3) = wk3i
                    Next
                End If
                nw0 = nw1
            End While
        End If
    End Sub


    <Extension()>
    Public Sub MakeCT(nc As Integer, ByRef c As Double(), startc As Integer, ByRef ip As Integer())
        Dim j, nch As Integer
        Dim delta, deltaj As Double

        ip(1) = nc
        If nc > 1 Then
            nch = nc >> 1
            delta = 0.78539816339744828 / nch
            c(startc) = std.Cos(delta * nch)
            c(startc + nch) = 0.5 * c(startc)
            For j = 1 To nch - 1
                deltaj = delta * j
                c(startc + j) = 0.5 * std.Cos(deltaj)
                c(startc + nc - j) = 0.5 * std.Sin(deltaj)
            Next
        End If
    End Sub

    <Extension()>
    Public Sub MakeCT(nc As Long, ByRef c As Double(), startc As Long, ByRef ipl As Long())
        Dim j, nch As Long
        Dim delta, deltaj As Double

        ipl(1) = nc
        If nc > 1 Then
            nch = nc >> 1
            delta = 0.78539816339744828 / nch
            c(startc) = std.Cos(delta * nch)
            c(startc + nch) = 0.5 * c(startc)
            For j = 1 To nch - 1
                deltaj = delta * j
                c(startc + j) = 0.5 * std.Cos(deltaj)
                c(startc + nc - j) = 0.5 * std.Sin(deltaj)
            Next
        End If
    End Sub


    <Extension()>
    Public Sub MakeCT(nc As Integer, ByRef c As Single(), startc As Integer, ByRef ip As Integer())
        Dim j, nch As Integer
        Dim delta, deltaj As Single

        ip(1) = nc
        If nc > 1 Then
            nch = nc >> 1
            delta = 0.7853982F / nch
            c(startc) = CSng(std.Cos(delta * nch))
            c(startc + nch) = 0.5F * c(startc)
            For j = 1 To nch - 1
                deltaj = delta * j
                c(startc + j) = 0.5F * CSng(std.Cos(deltaj))
                c(startc + nc - j) = 0.5F * CSng(std.Sin(deltaj))
            Next
        End If
    End Sub


    Private NTZ_TABLE As SByte() = {32, 0, 1, 12, 2, 6, -1, 13, 3, -1, 7, -1, -1, -1, -1, 14, 10, 4, -1, -1, 8, -1, -1, 25, -1, -1, -1, -1, -1, 21, 27, 15, 31, 11, 5, -1, -1, -1, -1, -1, 9, -1, -1, 24, -1, -1, 20, 26, 30, -1, -1, -1, -1, 23, -1, 19, 29, -1, 22, 18, 28, 17, 16, -1}

    'private static double SQRT_1_5 = 1.224744871391589; // Long bits 0x3ff3988e1409212eL.
    'private static double SQRT_2 = 1.4142135623730951; // Long bits 0x3ff6a09e667f3bcdL.
    'private static double SQRT_3 = 1.7320508075688772; // Long bits 0x3ffbb67ae8584caaL.
    Private EXP_LIMIT_H As Double = 709.782712893384 ' Long bits 0x40862e42fefa39efL.
    'private static double EXP_LIMIT_L = -745.1332191019411; // Long bits 0xc0874910d52d3051L.
    'private static double CP = 0.9617966939259756; // Long bits 0x3feec709dc3a03fdL.
    'private static double CP_H = 0.9617967009544373; // Long bits 0x3feec709e0000000L.
    'private static double CP_L = -7.028461650952758e-9; // Long bits 0xbe3e2fe0145b01f5L.
    'private static double LN2 = 0.6931471805599453; // Long bits 0x3fe62e42fefa39efL.
    Private LN2_H As Double = 0.69314718036912382 ' Long bits 0x3fe62e42fee00000L.
    Private LN2_L As Double = 0.00000000019082149292705877 ' Long bits 0x3dea39ef35793c76L.
    Private INV_LN2 As Double = 1.4426950408889634 ' Long bits 0x3ff71547652b82feL.
    'private static double INV_LN2_H = 1.4426950216293335; // Long bits 0x3ff7154760000000L.
    Private EXPM1_Q1 As Double = -0.033333333333333132 ' Long bits  0xbfa11111111110f4L
    Private EXPM1_Q2 As Double = 0.0015873015872548146 ' Long bits  0x3f5a01a019fe5585L
    Private EXPM1_Q3 As Double = -0.0000793650757867488 ' Long bits  0xbf14ce199eaadbb7L
    Private EXPM1_Q4 As Double = 0.0000040082178273293624 ' Long bits  0x3ed0cfca86e65239L
    Private EXPM1_Q5 As Double = -0.00000020109921818362437 ' Long bits  0xbe8afdb76e09c32dL

    ''' <summary>
    ''' Returns <em>e</em><sup>x</sup> - 1.
    ''' Special cases:
    ''' <ul>
    ''' <li>If the argument is NaN, the result is NaN.</li>
    ''' <li>If the argument is positive infinity, the result is positive
    ''' infinity</li>
    ''' <li>If the argument is negative infinity, the result is -1.</li>
    ''' <li>If the argument is zero, the result is zero.</li>
    ''' </ul>
    ''' 
    ''' </summary>
    ''' <param name="x">the argument to <em>e</em><sup>x</sup> - 1.</param>
    ''' <returns><em>e</em> raised to the power <code>x</code> minus one.</returns>
    Public Function Expm1(x As Double) As Double
        ' Method
        '   1d Argument reduction:
        '    Given x, find r and integer k such that
        '
        '            x = k * ln(2) + r,  |r| <= 0.5 * ln(2)
        '
        '  Here a correction term c will be computed to compensate
        '    the error in r when rounded to a floating-point number.
        '
        '   2d Approximating expm1(r) by a special rational function on
        '    the interval [0, 0.5 * ln(2)]:
        '    Since
        '        r*(Exp(r)+1)/(Exp(r)-1) = 2 + r^2/6 - r^4/360 + ...
        '    we define R1(r*r) by
        '        r*(Exp(r)+1)/(Exp(r)-1) = 2 + r^2/6 * R1(r*r)
        '    That is,
        '        R1(r**2) = 6/r *((Exp(r)+1)/(Exp(r)-1) - 2/r)
        '             = 6/r * ( 1 + 2.0*(1/(Exp(r)-1) - 1/r))
        '             = 1 - r^2/60 + r^4/2520 - r^6/100800 + ...
        '  We use a special Remes algorithm on [0, 0.347] to generate
        '     a polynomial of degree 5 in r*r to approximate R1d The
        '    maximum error of this polynomial approximation is bounded
        '    by 2**-61d In other words,
        '        R1(z) ~ 1.0 + Q1*z + Q2*z**2 + Q3*z**3 + Q4*z**4 + Q5*z**5
        '    where     Q1  =  -1.6666666666666567384E-2,
        '         Q2  =   3.9682539681370365873E-4,
        '         Q3  =  -9.9206344733435987357E-6,
        '         Q4  =   2.5051361420808517002E-7,
        '         Q5  =  -6.2843505682382617102E-9;
        '      (where z=r*r, and Q1 to Q5 are called EXPM1_Qx in the source)
        '    with error bounded by
        '        |                  5           |     -61
        '        | 1.0+Q1*z+...+Q5*z   -  R1(z) | <= 2
        '        |                              |
        '
        '    expm1(r) = Exp(r)-1 is then computed by the following
        '     specific way which minimize the accumulation rounding error:
        '                   2     3
        '                  r     r    [ 3 - (R1 + R1*r/2)  ]
        '          expm1(r) = r + --- + --- * [--------------------]
        '                      2     2    [ 6 - r*(3 - R1*r/2) ]
        '
        '    To compensate the error in the argument reduction, we use
        '        expm1(r+c) = expm1(r) + c + expm1(r)*c
        '               ~ expm1(r) + c + r*c
        '    Thus c+r*c will be added in as the correction terms for
        '    expm1(r+c)d Now rearrange the term to avoid optimization
        '     screw up:
        '                (      2                                    2 )
        '                ({  ( r    [ R1 -  (3 - R1*r/2) ]  )  }    r  )
        '     expm1(r+c)~r - ({r*(--- * [--------------------]-c)-c} - --- )
        '                    ({  ( 2    [ 6 - r*(3 - R1*r/2) ]  )  }    2  )
        '                      (                                             )
        '
        '           = r - E
        '   3d Scale back to obtain expm1(x):
        '    From step 1, we have
        '       expm1(x) = either 2^k*[expm1(r)+1] - 1
        '            = or     2^k*[expm1(r) + (1-2^-k)]
        '   4d Implementation notes:
        '    (A)d To save one multiplication, we scale the coefficient Qi
        '         to Qi*2^i, and replace z by (x^2)/2.
        '    (B)d To achieve maximum accuracy, we compute expm1(x) by
        '      (i)   if x < -56*ln2, return -1.0, (raise inexact if x!=inf)
        '      (ii)  if k=0, return r-E
        '      (iii) if k=-1, return 0.5*(r-E)-0.5
        '        (iv)    if k=1 if r < -0.25, return 2*((r+0.5)- E)
        '                      else         return  1.0+2.0*(r-E);
        '      (v)   if (k<-2||k>56) return 2^K(1-(E-r)) - 1 (or Exp(x)-1)
        '      (vi)  if k <= 20, return 2^K((1-2^-k)-(E-r)), else
        '      (vii) return 2^K(1-((E+2^-k)-r))

        Dim negative = x < 0
        Dim y, hi, lo, c, t, e, hxs, hfx, r1 As Double
        Dim k As Integer

        Dim bits As Long
        Dim h_bits As ULong
        Dim l_bits As ULong

        c = 0.0
        y = std.Abs(x)

        bits = BitConverter.DoubleToInt64Bits(y)
        h_bits = GetHighDWord(bits)
        l_bits = GetLowDWord(bits)

        ' handle special cases and large arguments
        If h_bits >= &H4043687AL Then        ' if |x| >= 56 * ln(2)
            If h_bits >= &H40862E42L Then    ' if |x| >= EXP_LIMIT_H
                If h_bits >= &H7FF00000L Then
                    If (h_bits And &HFFFFFL Or l_bits And &HFFFFFFFFL) <> 0 Then
                        Return x                        ' Exp(NaN) = NaN
                    Else
                        Return If(negative, -1.0, x)
                    End If      ' Exp({+-inf}) = {+inf, -1}
                End If

                If x > EXP_LIMIT_H Then Return Double.PositiveInfinity     ' overflow
            End If

            If negative Then Return -1.0                ' x <= -56 * ln(2)
        End If

        ' argument reduction
        If h_bits > &H3FD62E42L Then        ' |x| > 0.5 * ln(2)
            If h_bits < &H3FF0A2B2L Then    ' |x| < 1.5 * ln(2)
                If negative Then
                    hi = x + LN2_H
                    lo = -LN2_L
                    k = -1
                Else
                    hi = x - LN2_H
                    lo = LN2_L
                    k = 1
                End If
            Else
                k = CInt(INV_LN2 * x + If(negative, -0.5, 0.5))
                t = k
                hi = x - t * LN2_H
                lo = t * LN2_L
            End If

            x = hi - lo
            c = hi - x - lo
        ElseIf h_bits < &H3C900000L Then   ' |x| < 2^-54 return x
            Return x
        Else
            k = 0
        End If

        ' x is now in primary range
        hfx = 0.5 * x
        hxs = x * hfx
        r1 = 1.0 + hxs * (EXPM1_Q1 + hxs * (EXPM1_Q2 + hxs * (EXPM1_Q3 + hxs * (EXPM1_Q4 + hxs * EXPM1_Q5))))
        t = 3.0 - r1 * hfx
        e = hxs * ((r1 - t) / (6.0 - x * t))

        If k = 0 Then
            Return x - (x * e - hxs)    ' c == 0
        Else
            e = x * (e - c) - c
            e -= hxs

            If k = -1 Then Return 0.5 * (x - e) - 0.5

            If k = 1 Then
                If x < -0.25 Then
                    Return -2.0 * (e - (x + 0.5))
                Else
                    Return 1.0 + 2.0 * (x - e)
                End If
            End If

            If k <= -2 OrElse k > 56 Then       ' sufficient to return Exp(x) - 1
                y = 1.0 - (e - x)

                bits = BitConverter.DoubleToInt64Bits(y)
                h_bits = GetHighDWord(bits)
                l_bits = GetLowDWord(bits)

                h_bits += CULng(k << 20)     ' add k to y's exponent

                y = BuildDouble(l_bits, h_bits)

                Return y - 1.0
            End If

            t = 1.0
            If k < 20 Then
                bits = BitConverter.DoubleToInt64Bits(t)
                h_bits = CULng(&H3FF00000L - (&H200000L >> k))
                l_bits = GetLowDWord(bits)

                t = BuildDouble(l_bits, h_bits)      ' t = 1 - 2^(-k)
                y = t - (e - x)

                bits = BitConverter.DoubleToInt64Bits(y)
                h_bits = GetHighDWord(bits)
                l_bits = GetLowDWord(bits)

                h_bits += CULng(k << 20)     ' add k to y's exponent

                y = BuildDouble(l_bits, h_bits)
            Else
                bits = BitConverter.DoubleToInt64Bits(t)
                h_bits = CULng(&H3FFL - k << 20)
                l_bits = GetLowDWord(bits)

                t = BuildDouble(l_bits, h_bits)      ' t = 2^(-k)

                y = x - (e + t)
                y += 1.0

                bits = BitConverter.DoubleToInt64Bits(y)
                h_bits = GetHighDWord(bits)
                l_bits = GetLowDWord(bits)

                h_bits += CULng(k << 20)     ' add k to y's exponent

                y = BuildDouble(l_bits, h_bits)
            End If
        End If

        Return y
    End Function

    ''' <summary>
    ''' Returns the lower two words of a long. This is intended to be
    ''' used like this: 
    ''' <code>getLowDWord(Double.doubleToLongBits(x))</code>.
    ''' </summary>
    Private Function GetLowDWord(x As Long) As ULong
        Return x And &HFFFFFFFFL
    End Function

    ''' <summary>
    ''' Returns the higher two words of a long. This is intended to be
    ''' used like this:
    ''' <code>getHighDWord(Double.doubleToLongBits(x))</code>.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    Private Function GetHighDWord(x As Long) As ULong
        ' Java is using 0xffffffff00000000L (ulong) since the data type is different.
        Const hdw As ULong = &HFFFFFFFF00000000L
        Return (CULng(x) And hdw) >> 32
    End Function

    Private Function BuildDouble(lowDWord As ULong, highDWord As ULong) As Double
        Return BitConverter.Int64BitsToDouble((highDWord And &HFFFFFFFFL) << 32 Or lowDWord And &HFFFFFFFFL)
    End Function
End Module
