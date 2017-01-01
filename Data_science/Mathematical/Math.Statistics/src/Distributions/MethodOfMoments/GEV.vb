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
	Public Class GEV
		Inherits Distributions.ContinuousDistribution

		Private _Mu As Double 'https://en.wikipedia.org/wiki/Generalized_extreme_value_distribution
		Private _Sigma As Double
		Private _Xi As Double
		Public Sub New(ByVal mu As Double, ByVal sigma As Double, ByVal xi As Double)
			_Mu = mu
			_Sigma = sigma
			_Xi = xi
		End Sub
		Public Sub New(ByVal data As Double())
			'not implemented
		End Sub
		Public Overrides Function GetInvCDF(ByVal probability As Double) As Double
			If probability<=0 Then
				If _Xi>0 Then
					Return _Mu-_Sigma/_Xi
				Else
					Return Double.NegativeInfinity
				End If
			ElseIf probability>=1 Then
				If _Xi<0 Then
					Return _Mu-_Sigma/_Xi
				Else
					Return Double.PositiveInfinity
				End If
			End If
			Return Tinv(probability)
		End Function
		Public Overrides Function GetCDF(ByVal value As Double) As Double
			If _Xi>0 AndAlso value <= _Mu-_Sigma/_Xi Then Return 0
			If _Xi<0 AndAlso value >= _Mu-_Sigma/_Xi Then Return 1
			Return Math.Exp(-T(value))
		End Function

		Public Overrides Function GetPDF(ByVal value As Double) As Double
			If _Xi>0 AndAlso value <= _Mu-_Sigma/_Xi Then Return 0
			If _Xi<0 AndAlso value >= _Mu-_Sigma/_Xi Then Return 0
			Dim tx As Double = T(value)
			Return (1/_Sigma)*Math.Pow(tx, _Xi+1)*Math.Exp(-tx)
		End Function
		Private Function T(ByVal x As Double) As Double
			If _Xi<>0 Then
				Return Math.Pow((1+((x-_Mu)/_Sigma)*_Xi),-1/_Xi)
			Else
				Return Math.Exp(-(x-_Mu)/_Sigma)
			End If
		End Function
		Private Function Tinv(ByVal probability As Double) As Double
			If _Xi<>0 Then
				Return _Mu-_Sigma*(Math.Pow(Math.Log(1/probability), _Xi)-1)/_Xi
			Else
				Return _Mu-_Sigma*Math.Log(Math.Log(1/probability))
			End If
		End Function
		Public Overrides Function Validate() As List(Of Distributions.ContinuousDistributionError)
			Dim errors As New List(Of Distributions.ContinuousDistributionError)
			If _Xi<=0 Then errors.Add(New Distributions.ContinuousDistributionError("Xi must be greater than 0"))
			If _Sigma<=0 Then errors.Add(New Distributions.ContinuousDistributionError("Sigma must be greater than 0"))
			Return errors
		End Function
	End Class

End Namespace