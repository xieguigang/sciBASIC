#Region "Microsoft.VisualBasic::90f7a030a01f66bebe173399f4302129, Data_science\Mathematica\Math\Math.Statistics\Distributions\LinearMoments\GEV.vb"

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

    '   Total Lines: 99
    '    Code Lines: 80
    ' Comment Lines: 13
    '   Blank Lines: 6
    '     File Size: 4.62 KB


    '     Class GEV
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: GetCDF, GetInvCDF, GetPDF, Validate, Y
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports stdNum = System.Math

'
' * To change this license header, choose License Headers in Project Properties.
' * To change this template file, choose Tools | Templates
' * and open the template in the editor.
' 
Namespace Distributions.LinearMoments


    ''' 
    ''' <summary>
    ''' @author Will_and_Sara and Micheal Wright
    ''' </summary>
    Public Class GEV
        Inherits Distributions.ContinuousDistribution

        Private _K As Double
        Private _Alpha As Double
        Private _Xi As Double
        Public Sub New()
            'for reflection
            _K = 0
            _Alpha = 0
            _Xi = 0
        End Sub
        Public Sub New(data As Double())
            Dim LM As New MomentFunctions.LinearMoments(data)
            PeriodOfRecord = (LM.SampleSize())
            'different formulae finding _k for positive and negative t3 - for very low t3 _k is refined through newton-raphson iteration
            If LM.T3() <= 0 Then 'following works for -0.8 to 0
                _K = (0.2837753 + LM.T3() * (-1.21096399 + LM.T3() * (-2.50728214 + LM.T3() * (-1.13455566 + LM.T3() * -0.07138022)))) / (1 + LM.T3() * (2.06189696 + LM.T3() * (1.31912239 + LM.T3() * 0.25077104)))
                If LM.T3() < -0.8 Then 'use above _k as starting point for newton-raphson iteration to converge to answer
                    If LM.T3() <= -0.97 Then
                        _K = 1 - stdNum.Log(1 + LM.T3()) / stdNum.Log(2) '...unless t3 is below -0.97 in which case start from this formula
                    Else
                        Dim t0 As Double = (LM.T3() + 3) / 2
                        For i As Integer = 1 To 19
                            Dim x2 As Double = stdNum.Pow(2, -_K)
                            Dim x3 As Double = stdNum.Pow(3, -_K)
                            Dim xx2 As Double = 1 - x2
                            Dim xx3 As Double = 1 - x3
                            Dim deriv As Double = (xx2 * x3 * stdNum.Log(3) - xx3 * x2 * stdNum.Log(2)) / stdNum.Pow(xx2, 2)
                            Dim kold As Double = _K
                            _K = _K - (xx3 / xx2 - t0) / deriv
                            If stdNum.Abs(_K - kold) <= _K * 0.000001 Then i = 20
                        Next i
                    End If
                Else
                    'use the above k, without any newton-raphson
                End If 'positive t3 always uses the below k
            Else
                Dim z As Double = 1 - LM.T3()
                _K = (-1 + z * (1.59921491 + z * (-0.48832213 + z * 0.01573152))) / (1 + z * (-0.64363929 + z * 0.08985247))
            End If
            'calculate alpha and xi from k, or if k = 0 calculate them in a different way
            If stdNum.Abs(_K) < 0.000001 Then
                _K = 0
                _Alpha = LM.L2() / stdNum.Log(2)
                _Xi = LM.L1() - _Alpha * 0.57721566 'euler's constant
            Else
                Dim gam As Double = stdNum.Exp(SpecialFunctions.gammaln(1 + _K))
                _Alpha = LM.L2() * _K / (gam * (1 - stdNum.Pow(2, -_K)))
                _Xi = LM.L1() - _Alpha * (1 - gam) / _K
            End If
        End Sub
        Public Sub New(K As Double, Alpha As Double, Xi As Double)
            _K = K
            _Alpha = Alpha
            _Xi = Xi
        End Sub
        Public Overrides Function GetInvCDF(probability As Double) As Double
            If _K <> 0 Then
                Return _Xi + (_Alpha / _K) * ((1 - stdNum.Pow((-stdNum.Log(probability)), _K)))
            Else
                Return _Xi - _Alpha * stdNum.Log(-stdNum.Log(probability))
            End If
        End Function
        Public Overrides Function GetCDF(value As Double) As Double
            Return stdNum.Exp(-stdNum.Exp(-Y(value)))
        End Function
        Public Overrides Function GetPDF(value As Double) As Double
            Return (1 / _Alpha) * stdNum.Exp(-(1 - _K) * (Y(value) - stdNum.Exp(-Y(value))))
        End Function
        Private Function Y(value As Double) As Double
            If _K <> 0 Then
                Return (-1 / _K) * stdNum.Log(1 - _K * (value - _Xi) / _Alpha)
            Else
                Return (value - _Xi) / _Alpha
            End If
        End Function
        Public Overrides Iterator Function Validate() As IEnumerable(Of Exception)
            If _Alpha = 0 Then Yield New Exception("Alpha cannot be zero")
            If _K = 0 Then Yield New Exception("K cannot be zero")

        End Function
    End Class

End Namespace
