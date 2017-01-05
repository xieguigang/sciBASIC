#Region "Microsoft.VisualBasic::d6539a4c62df42bc8bfeb7ec0081f10d, ..\sciBASIC#\Data_science\Mathematical\Math.Statistics\src\Distributions\LinearMoments\GEV.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System
Imports System.Collections.Generic

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
		Public Sub New( data As Double())
			Dim LM As New MomentFunctions.LinearMoments(data)
			SetPeriodOfRecord(LM.GetSampleSize())
			'different formulae finding _k for positive and negative t3 - for very low t3 _k is refined through newton-raphson iteration
			If LM.GetT3() <= 0 Then 'following works for -0.8 to 0
				_K = (0.2837753 + LM.GetT3() * (-1.21096399 + LM.GetT3() * (-2.50728214 + LM.GetT3() * (-1.13455566 + LM.GetT3() * -0.07138022)))) / (1 + LM.GetT3() * (2.06189696 + LM.GetT3() * (1.31912239 + LM.GetT3() * 0.25077104)))
				If LM.GetT3() < -0.8 Then 'use above _k as starting point for newton-raphson iteration to converge to answer
					If LM.GetT3() <= -0.97 Then
						_K = 1 - Math.Log(1 + LM.GetT3()) / Math.Log(2) '...unless t3 is below -0.97 in which case start from this formula
					Else
					Dim t0 As Double = (LM.GetT3() + 3) / 2
						For i As Integer = 1 To 19
							Dim x2 As Double = Math.Pow(2, -_K)
							Dim x3 As Double = Math.Pow(3, -_K)
							Dim xx2 As Double = 1 - x2
							Dim xx3 As Double = 1 - x3
							Dim deriv As Double = (xx2 * x3 * Math.Log(3) - xx3 * x2 * Math.Log(2)) / Math.Pow(xx2, 2)
							Dim kold As Double = _K
							_K = _K - (xx3 / xx2 - t0) / deriv
							If Math.Abs(_K - kold) <= _K * 0.000001 Then i = 20
						Next i
					End If
				Else
					'use the above k, without any newton-raphson
				End If 'positive t3 always uses the below k
			Else
				Dim z As Double = 1 - LM.GetT3()
				_K = (-1 + z * (1.59921491 + z * (-0.48832213 + z * 0.01573152))) / (1 + z * (-0.64363929 + z * 0.08985247))
			End If
			'calculate alpha and xi from k, or if k = 0 calculate them in a different way
			If Math.Abs(_K) < 0.000001 Then
				_K = 0
				_Alpha = LM.GetL2() / Math.Log(2)
				_Xi = LM.GetL1() - _Alpha * 0.57721566 'euler's constant
			Else
				Dim gam As Double = Math.Exp(SpecialFunctions.SpecialFunctions.gammaln(1 + _K))
				_Alpha = LM.GetL2() * _K / (gam * (1 - Math.Pow(2,-_K)))
				_Xi = LM.GetL1() - _Alpha * (1 - gam) / _K
			End If
		End Sub
		Public Sub New( K As Double,  Alpha As Double,  Xi As Double)
			_K = K
			_Alpha = Alpha
			_Xi = Xi
		End Sub
		Public Overrides Function GetInvCDF( probability As Double) As Double
			If _K <> 0 Then
				Return _Xi + (_Alpha / _K) * ((1 - Math.Pow((-Math.Log(probability)),_K)))
			Else
				Return _Xi - _Alpha * Math.Log(-Math.Log(probability))
			End If
		End Function
		Public Overrides Function GetCDF( value As Double) As Double
			Return Math.Exp(-Math.Exp(-Y(value)))
		End Function
		Public Overrides Function GetPDF( value As Double) As Double
			Return (1/_Alpha) * Math.Exp(-(1 - _K) * (Y(value) - Math.Exp(-Y(value))))
		End Function
		Private Function Y( value As Double) As Double
			If _K <> 0 Then
				Return (-1/_K) * Math.Log(1 - _K * (value - _Xi) / _Alpha)
			Else
				Return (value - _Xi) / _Alpha
			End If
		End Function
		Public Overrides Function Validate() As List(Of Distributions.ContinuousDistributionError)
			Dim errors As New List(Of Distributions.ContinuousDistributionError)
			If _Alpha = 0 Then errors.Add(New Distributions.ContinuousDistributionError("Alpha cannot be zero"))
			If _K = 0 Then errors.Add(New Distributions.ContinuousDistributionError("K cannot be zero"))
			Return errors
		End Function
	End Class

End Namespace
