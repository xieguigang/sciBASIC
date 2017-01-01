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
	Public Class Gamma
		Inherits Distributions.ContinuousDistribution

		Private _Alpha As Double
		Private _Beta As Double
		Public Sub New()
			'for reflection
			_Alpha = 0
			_Beta = 0
		End Sub
		Public Sub New(ByVal data As Double())
			'http://www.itl.nist.gov/div898/handbook/eda/section3/eda366b.htm
			Dim BPM As New MomentFunctions.BasicProductMoments(data)
			_Alpha = Math.Pow((BPM.GetMean() / BPM.GetStDev()),2)
			_Beta = 1 / (BPM.GetStDev() / BPM.GetMean())
			SetPeriodOfRecord(BPM.GetSampleSize())
		End Sub
		Public Sub New(ByVal Alpha As Double, ByVal Beta As Double)
			_Alpha = Alpha
			_Beta = Beta
		End Sub
		Public Overrides Function GetInvCDF(ByVal probability As Double) As Double
			Dim xn As Double = _Alpha/_Beta
			Dim testvalue As Double = GetCDF(xn)
			Dim i As Integer = 0
			Do
				xn = xn - ((testvalue - probability)/GetPDF(xn))
				testvalue = GetCDF(xn)
				i+=1
			Loop While Math.Abs(testvalue - probability)<=0.00000000000001 Or i = 100
			Return xn
		End Function
		Public Overrides Function GetCDF(ByVal value As Double) As Double
			Return SpecialFunctions.SpecialFunctions.IncompleteGamma(_Alpha, _Beta*value)/Math.Exp(SpecialFunctions.SpecialFunctions.gammaln(_Alpha))
		End Function
		Public Overrides Function GetPDF(ByVal value As Double) As Double
			Return (((Math.Pow(_Beta, _Alpha))*((Math.Pow(value,_Alpha-1))*Math.Exp(-_Beta*value))/Math.Exp(SpecialFunctions.SpecialFunctions.gammaln(_Alpha))))
		End Function
		Public Overrides Function Validate() As List(Of Distributions.ContinuousDistributionError)
			Dim errors As New List(Of Distributions.ContinuousDistributionError)
			If _Beta<=0 Then errors.Add(New Distributions.ContinuousDistributionError("Beta must be greater than 0"))
			Return errors
		End Function
	End Class

End Namespace