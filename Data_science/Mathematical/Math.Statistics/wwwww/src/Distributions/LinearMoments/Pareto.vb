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
	''' @author Will_and_Sara
	''' </summary>
	Public Class Pareto
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
		Public Sub New(ByVal data As Double())
			Dim LM As New MomentFunctions.LinearMoments(data)
			If LM.GetL2()=0 Then
				_K = (1 - 3 * LM.GetT3()) / (1 + LM.GetT3())
				_Alpha = (1 + _K) * (2 + _K) * LM.GetL2()
				_Xi = LM.GetL1() - (2 + _K) * LM.GetL2()
				SetPeriodOfRecord(LM.GetSampleSize())
			Else
				'coefficient of variation cannot be zero.
			End If
		End Sub
		Public Sub New(ByVal K As Double, ByVal Alpha As Double, ByVal Xi As Double)
			_K = K
			_Alpha = Alpha
			_Xi = Xi
		End Sub
		Public Overrides Function GetInvCDF(ByVal probability As Double) As Double
			If _K <> 0 Then
				Return _Xi + (_Alpha * (1 - Math.Pow(1 - probability,_K)) / _K)
			Else
				Return _Xi - _Alpha * Math.Log(1 - probability)
			End If
		End Function
		Public Overrides Function GetCDF(ByVal value As Double) As Double
			Return 1 - Math.Exp(-Y(value))
		End Function
		Public Overrides Function GetPDF(ByVal value As Double) As Double
			Return (1/_Alpha) * Math.Exp(-(1 - _K) * Y(value))
		End Function
		Public Overridable Function Y(ByVal value As Double) As Double
			If _K <> 0 Then
				Return (1/-_K) * Math.Log(1 - _K * (value - _Xi) / _Alpha)
			Else
				Return (value - _Xi) / _Alpha
			End If
		End Function
		Public Overrides Function Validate() As List(Of Distributions.ContinuousDistributionError)
			Dim errors As New List(Of Distributions.ContinuousDistributionError)
			If _Alpha = 0 Then errors.Add(New Distributions.ContinuousDistributionError("Alpha cannot be zero"))
			Return errors
		End Function
	End Class

End Namespace