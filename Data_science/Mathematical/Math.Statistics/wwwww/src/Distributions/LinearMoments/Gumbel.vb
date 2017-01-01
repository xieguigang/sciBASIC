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
	Public Class Gumbel
		Inherits Distributions.ContinuousDistribution

		Private _Alpha As Double
		Private _Xi As Double
		Public Sub New()
			'for reflection
			_Alpha = 0
			_Xi = 0
		End Sub
		Public Sub New(ByVal data As Double())
			Dim LM As New MomentFunctions.LinearMoments(data)
			_Alpha = LM.GetL2() / Math.Log(2)
			_Xi = LM.GetL1() - 0.57721566490153287 * _Alpha
			SetPeriodOfRecord(LM.GetSampleSize())
		End Sub
		Public Sub New(ByVal Alpha As Double, ByVal Xi As Double)
			_Alpha = Alpha
			_Xi = Xi
		End Sub
		Public Overrides Function GetInvCDF(ByVal probability As Double) As Double
			Return _Xi - _Alpha * Math.Log(-Math.Log(probability))
		End Function
		Public Overrides Function GetCDF(ByVal value As Double) As Double
			Return Math.Exp(-Math.Exp(-(value - _Xi) / _Alpha))
		End Function
		Public Overrides Function GetPDF(ByVal value As Double) As Double
			Return (1/_Alpha) * Math.Exp(-(value - _Xi) / _Alpha) * Math.Exp(-Math.Exp(-(value - _Xi) / _Alpha))
		End Function
		Public Overrides Function Validate() As List(Of Distributions.ContinuousDistributionError)
			Dim errors As New List(Of Distributions.ContinuousDistributionError)
			If _Alpha = 0 Then errors.Add(New Distributions.ContinuousDistributionError("Alpha cannot be zero"))
			Return errors
		End Function
	End Class

End Namespace