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
	Public Class Exponential
		Inherits Distributions.ContinuousDistribution

		Private _Lambda As Double
		Public Sub New()
			'for reflection
			_Lambda = 1
		End Sub
		Public Sub New(ByVal lambda As Double)
			_Lambda = lambda
		End Sub
		Public Sub New(ByVal data As Double())
			Dim BPM As New MomentFunctions.BasicProductMoments(data)
			_Lambda = 1/BPM.GetMean()
			SetPeriodOfRecord(BPM.GetSampleSize())
		End Sub
		Public Overrides Function GetInvCDF(ByVal probability As Double) As Double
			Return Math.Log(probability)/_Lambda
		End Function
		Public Overrides Function GetCDF(ByVal value As Double) As Double
			Return 1- Math.Exp(-_Lambda*value)
		End Function
		Public Overrides Function GetPDF(ByVal value As Double) As Double
			If value<0 Then
				Return 0
			Else
				Return _Lambda * Math.Exp(-_Lambda*value)
			End If
		End Function
		Public Overrides Function Validate() As List(Of Distributions.ContinuousDistributionError)
			Dim errors As New List(Of Distributions.ContinuousDistributionError)
			If _Lambda<=0 Then errors.Add(New Distributions.ContinuousDistributionError("Lambda must be greater than 0"))
			Return errors
		End Function

	End Class

End Namespace