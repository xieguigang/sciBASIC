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
	Public Class Rayleigh
		Inherits Distributions.ContinuousDistribution

		Private _Sigma As Double
		Public Overridable Function GetSigma() As Double
			Return _Sigma
		End Function
		Public Sub New()
			'for reflection
			_Sigma = 1
		End Sub
		Public Sub New(ByVal sigma As Double)
			_Sigma = sigma
		End Sub
		Public Sub New(ByVal data As Double())
			Dim BPM As New MomentFunctions.BasicProductMoments(data)
			_Sigma = BPM.GetStDev()
			SetPeriodOfRecord(BPM.GetSampleSize())
		End Sub
		Public Overrides Function GetInvCDF(ByVal probability As Double) As Double
			Return _Sigma * Math.Sqrt(-2*Math.Log(probability))
		End Function
		Public Overrides Function GetCDF(ByVal value As Double) As Double
			Return 1-(Math.Exp(-(Math.Pow(value, 2))/(2*(Math.Pow(_Sigma,2)))))
		End Function
		Public Overrides Function GetPDF(ByVal value As Double) As Double
			Return (value/(Math.Pow(_Sigma, 2)))* Math.Exp(-(Math.Pow(value, 2))/(2*(Math.Pow(_Sigma,2))))
		End Function
		Public Overrides Function Validate() As List(Of Distributions.ContinuousDistributionError)
			Dim errs As New List(Of Distributions.ContinuousDistributionError)
			If _Sigma<=0 Then errs.Add(New Distributions.ContinuousDistributionError("Sigma cannot be less than or equal to zero in the Rayleigh distribuiton."))
			Return errs
		End Function
	End Class

End Namespace