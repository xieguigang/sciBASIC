#Region "Microsoft.VisualBasic::51f572f6cf8766b30293e7396b4afc7f, Data_science\Mathematica\Math\Math.Statistics\Distributions\MethodOfMoments\Normal.vb"

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

    '   Total Lines: 187
    '    Code Lines: 150
    ' Comment Lines: 20
    '   Blank Lines: 17
    '     File Size: 6.97 KB


    '     Class Normal
    ' 
    '         Properties: Mean, StDev
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: FindArea, GetCDF, GetInvCDF, GetPDF, TrapazoidalIntegration
    '                   Validate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

'
' * To change this license header, choose License Headers in Project Properties.
' * To change this template file, choose Tools | Templates
' * and open the template in the editor.
' 
Imports System.Runtime.CompilerServices
Imports stdNum = System.Math

Namespace Distributions.MethodOfMoments

    ''' 
    ''' <summary>
    ''' @author Will_and_Sara
    ''' </summary>
    Public Class Normal : Inherits Distributions.ContinuousDistribution

        Public ReadOnly Property Mean As Double
        Public ReadOnly Property StDev As Double

        ''' <summary>
        ''' Creates a standard normal distribution
        ''' </summary>
        Public Sub New()
            Mean = 0
            StDev = 1
        End Sub
        ''' <summary>
        ''' Creates a normal distribution based on the user defined mean and standard deviation </summary>
        ''' <param name="m"> the mean of the distribution </param>
        ''' <param name="sd"> the standard deviation of the distribution </param>
        Public Sub New(m As Double, sd As Double)
            Mean = m
            StDev = sd
        End Sub
        ''' <summary>
        ''' Creates a normal distribution based on input data using the standard method of moments. </summary>
        ''' <param name="data"> an array of double data. </param>
        Public Sub New(data As Double())
            Dim bpm As New MomentFunctions.BasicProductMoments(data)
            Mean = bpm.Mean()
            StDev = bpm.StDev()
            PeriodOfRecord = (bpm.SampleSize())
        End Sub

        Public Overrides Function GetInvCDF(probability As Double) As Double
            Dim i As Integer
            Dim x As Double
            Dim c0 As Double = 2.515517
            Dim c1 As Double = 0.802853
            Dim c2 As Double = 0.010328
            Dim d1 As Double = 1.432788
            Dim d2 As Double = 0.189269
            Dim d3 As Double = 0.001308
            Dim q As Double

            q = probability

            If q = 0.5 Then Return _Mean
            If q <= 0 Then q = 0.000000000000001
            If q >= 1 Then q = 0.999999999999999

            If q < 0.5 Then
                i = -1
            Else
                i = 1
                q = 1 - q
            End If

            Dim t As Double = stdNum.Sqrt(stdNum.Log(1 / stdNum.Pow(q, 2)))
            x = t - (c0 + c1 * t + c2 * (stdNum.Pow(t, 2))) / (1 + d1 * t + d2 * stdNum.Pow(t, 2) + d3 * stdNum.Pow(t, 3))
            x = i * x

            Return (x * _StDev) + _Mean
        End Function

        Private Function TrapazoidalIntegration(y1 As Double, y2 As Double, deltax As Double) As Double
            Dim deltay As Double = 0
            Dim rect As Double = 0
            If y1 > y2 Then
                deltay = y1 - y2
                rect = stdNum.Abs(y2 * deltax)
            Else
                deltay = y2 - y1
                rect = stdNum.Abs(y1 * deltax)
            End If
            Dim tri As Double = (1 \ 2) * (deltax * deltay)
            Return rect + stdNum.Abs(tri)
        End Function

        Private Function FindArea(a As Double, inc As Double, x As Double) As Double
            Dim x1 As Double = GetInvCDF(a)
            Dim x2 As Double = GetInvCDF(a + inc)
            Do While x2 >= x
                x1 = x2
                a += inc
                x2 = GetInvCDF(a + inc)
            Loop
            Dim y1 As Double = GetPDF(x1)
            Dim y2 As Double = GetPDF(x2)
            Dim deltax As Double = stdNum.Abs(x1 - x2)
            Dim area As Double = TrapazoidalIntegration(y1, y2, deltax)
            Dim interpvalue As Double = (x - x1) / (x2 - x1)
            a += area * interpvalue
            Return a
        End Function

        Public Overrides Function GetCDF(value As Double) As Double
            'decide which method i want to use.  errfunction, the method i came up with in vb, or something else.
            If value = _Mean Then Return 0.5
            Dim dist As Double = value - _Mean
            Dim stdevs As Integer = CInt(Fix(stdNum.Floor(stdNum.Abs(dist / _StDev))))
            Dim inc As Double = 1 \ 250
            Dim a As Double = 0.5
            Dim a1 As Double = 0.682689492137 / 2
            Dim a2 As Double = 0.954499736104 / 2
            Dim a3 As Double = 0.997300203937 / 2
            Dim a4 As Double = 0.999936657516 / 2
            Dim a5 As Double = 0.999999426687 / 2
            Dim a6 As Double = 0.999999998027 / 2
            Dim a7 As Double = (a - a6) / 2

            Select Case stdevs
                Case 0
                    If dist < 0 Then a += -a1
                    Return FindArea(a, inc, value)
                Case 1
                    If dist < 0 Then
                        a -= a2
                    Else
                        a += a1
                    End If
                    Return FindArea(a, inc, value)
                Case 2
                    If dist < 0 Then
                        a -= a3
                    Else
                        a += a2
                    End If
                    Return FindArea(a, inc, value)
                Case 3
                    If dist < 0 Then
                        a -= a4
                    Else
                        a += a3
                    End If
                    Return FindArea(a, inc, value)
                Case 4
                    If dist < 0 Then
                        a -= a5
                    Else
                        a += a4
                    End If
                    Return FindArea(a, inc, value)
                Case 5
                    If dist < 0 Then
                        a -= a6
                    Else
                        a += a5
                    End If
                    Return FindArea(a, inc, value)
                Case 6
                    If dist < 0 Then
                        a -= a7
                    Else
                        a += a6
                    End If
                    Return FindArea(a, inc, value)
                Case Else
                    If dist < 0 Then
                        Return 0
                    Else
                        Return 1
                    End If
            End Select
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function GetPDF(value As Double) As Double
            Return (1 / stdNum.Sqrt(2 * stdNum.PI) * stdNum.Pow(_StDev, 2.0)) * stdNum.Exp((-(stdNum.Pow(value - _Mean, 2) / (2 * stdNum.Pow(_StDev, 2)))))
        End Function

        Public Overrides Iterator Function Validate() As IEnumerable(Of Exception)
            If _StDev <= 0 Then Yield New Exception("Standard of Deviation must be greater than 0")
        End Function
    End Class

End Namespace
