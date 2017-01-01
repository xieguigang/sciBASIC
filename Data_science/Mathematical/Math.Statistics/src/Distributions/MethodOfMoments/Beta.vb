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
		Public Sub New(ByVal data As Double())
			Dim BPM As New MomentFunctions.BasicProductMoments(data)
			Dim x As Double = BPM.GetMean()
			Dim v As Double = BPM.GetStDev()
			If v < (x * (1 - x)) Then
				_Alpha = x * (((x * (1 - x)) / v) - 1)
				_Beta = (1 - x) * (((x * (1 - x)) / v) - 1)
			Else
				'Beta Fitting Error: variance is greater than mean*(1-mean), this data is not factorable to a beta distribution
			End If
		End Sub
		Public Sub New(ByVal Alpha As Double, ByVal Beta As Double)
			_Alpha = Alpha
			_Beta = Beta
		End Sub
		Public Overrides Function GetInvCDF(ByVal probability As Double) As Double
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
		Public Overrides Function GetCDF(ByVal value As Double) As Double 'not sure this is right, technically it is the regularized incomplete beta.
			Return SpecialFunctions.SpecialFunctions.RegularizedIncompleteBetaFunction(_Alpha, _Beta, value)
		End Function
		Public Overrides Function GetPDF(ByVal value As Double) As Double
			Return (Math.Pow(value,(_Alpha - 1)) * (Math.Pow((1 - value), (_Beta - 1)))) / SpecialFunctions.SpecialFunctions.BetaFunction(_Alpha, _Beta)
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