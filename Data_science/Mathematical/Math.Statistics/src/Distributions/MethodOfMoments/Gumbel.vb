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
	Public Class Gumbel
		Inherits Distributions.ContinuousDistribution

		Private _Mu As Double
		Private _Beta As Double
		Public Sub New()
			_Mu = 0
			_Beta = 0
		End Sub
		Public Sub New(ByVal mu As Double, ByVal beta As Double)
			_Mu = mu
			_Beta = beta
		End Sub
		Public Sub New(ByVal data As Double())
			Dim BPM As New MomentFunctions.BasicProductMoments(data)
			_Beta = Math.PI/(BPM.GetStDev()*Math.Sqrt(6))
			_Mu = BPM.GetMean()-_Beta*0.57721566490153287
			SetPeriodOfRecord(BPM.GetSampleSize())
		End Sub
		Public Overrides Function GetInvCDF(ByVal probability As Double) As Double
			Return (_Mu-(_Beta*(Math.Log(Math.Log(probability)))))
		End Function
		Public Overrides Function GetCDF(ByVal value As Double) As Double
			Return Math.Exp(-Math.Exp(-(value-_Mu)/_Beta))
		End Function
		Public Overrides Function GetPDF(ByVal value As Double) As Double
			Dim z As Double = (value-_Mu)/_Beta
			Return (1/_Beta)*Math.Exp(-(z+Math.Exp(-z)))
		End Function
		Public Overrides Function Validate() As List(Of Distributions.ContinuousDistributionError)
			Dim errors As New List(Of Distributions.ContinuousDistributionError)
			If _Beta<=0 Then errors.Add(New Distributions.ContinuousDistributionError("Beta must be greater than 0"))
			Return errors
		End Function
	End Class

End Namespace