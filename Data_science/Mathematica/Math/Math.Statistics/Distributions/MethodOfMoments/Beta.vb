#Region "Microsoft.VisualBasic::f9fff18ceaede6c833704740de2f5ac4, ..\sciBASIC#\Data_science\Mathematica\Math\Math.Statistics\Distributions\MethodOfMoments\Beta.vb"

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
Namespace Distributions.MethodOfMoments


	''' 
	''' <summary>
	''' @author Will_and_Sara
	''' </summary>
	Public Class Beta
		Inherits Distributions.ContinuousDistribution

		Private _Alpha As Double
		Private _Beta As Double
		Public Sub New()
			'for relfection
			_Alpha = 0
			_Beta = 0
		End Sub
		Public Sub New( data As Double())
			Dim BPM As New MomentFunctions.BasicProductMoments(data)
			Dim x As Double = BPM.Mean()
			Dim v As Double = BPM.StDev()
			If v < (x * (1 - x)) Then
				_Alpha = x * (((x * (1 - x)) / v) - 1)
				_Beta = (1 - x) * (((x * (1 - x)) / v) - 1)
			Else
				'Beta Fitting Error: variance is greater than mean*(1-mean), this data is not factorable to a beta distribution
			End If
		End Sub
		Public Sub New( Alpha As Double,  Beta As Double)
			_Alpha = Alpha
			_Beta = Beta
		End Sub
		Public Overrides Function GetInvCDF( probability As Double) As Double
			'use bisection since the shape can be bimodal.
			Dim value As Double = 0.5 'midpoint of the beta output range
			Dim testvalue As Double = GetCDF(value)
			Dim inc As Double = 0.5
			Dim n As Integer = 0
			Do
				If testvalue > probability Then
					'incriment a half incriment down
					inc = inc / 2
					value -= inc
					testvalue = GetCDF(value)
				Else
					'incriment a half incriment up
					inc = inc / 2
					value += inc
					testvalue = GetCDF(value)
				End If
				n += 1
			Loop While Math.Abs(testvalue - probability) > 0.000000000000001 Or n <> 100
			Return value
		End Function
		Public Overrides Function GetCDF( value As Double) As Double 'not sure this is right, technically it is the regularized incomplete beta.
            Return SpecialFunctions.RegularizedIncompleteBetaFunction(_Alpha, _Beta, value)
        End Function
		Public Overrides Function GetPDF( value As Double) As Double
            Return (Math.Pow(value, (_Alpha - 1)) * (Math.Pow((1 - value), (_Beta - 1)))) / SpecialFunctions.BetaFunction(_Alpha, _Beta)
        End Function
		Public Overrides Function Validate() As List(Of Distributions.ContinuousDistributionError)
			Dim errors As New List(Of Distributions.ContinuousDistributionError)
			If _Alpha<=0 Then errors.Add(New Distributions.ContinuousDistributionError("Alpha must be greater than 0"))
			If _Beta<=0 Then errors.Add(New Distributions.ContinuousDistributionError("Beta must be greater than 0"))
			If _Alpha<=_Beta Then errors.Add(New Distributions.ContinuousDistributionError("Alpha must be greater than Beta"))
			Return errors
		End Function
	End Class

End Namespace
