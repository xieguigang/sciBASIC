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
	Public Class Uniform
		Inherits Distributions.ContinuousDistribution

		Private _Min As Double
		Private _Max As Double
		Public Overridable Function GetMin() As Double
			Return _Min
		End Function
		Public Overridable Function GetMax() As Double
			Return _Max
		End Function
		Public Sub New()
			'for reflection
			_Min = 0
			_Max = 0
		End Sub
		Public Sub New(ByVal min As Double, ByVal max As Double)
			_Min = min
			_Max = max
		End Sub
		Public Sub New(ByVal data As Double())
			Dim BPM As New MomentFunctions.BasicProductMoments(data)
			_Min = BPM.GetMin()
			_Max = BPM.GetMax()
			SetPeriodOfRecord(BPM.GetSampleSize())
			'alternative method
			'double dist = java.lang.Math.sqrt(3*BPM.GetSampleVariance());
			'_Min = BPM.GetMean() - dist;
			'_Max = BPM.GetMean() + dist;
		End Sub
		Public Overrides Function GetInvCDF(ByVal probability As Double) As Double
			Return _Min + ((_Max - _Min)* probability)
		End Function
		Public Overrides Function GetCDF(ByVal value As Double) As Double
			If value<_Min Then
				Return 0
			ElseIf value <=_Max Then
				Return (value - _Min)/(_Min - _Max)
			Else
				Return 1
			End If
		End Function
		Public Overrides Function GetPDF(ByVal value As Double) As Double
			If value < _Min Then
				Return 0
			ElseIf value <= _Max Then
				Return 1/(_Max-_Min)
			Else
				Return 0
			End If
		End Function
		Public Overrides Function Validate() As List(Of Distributions.ContinuousDistributionError)
			Dim errs As New List(Of Distributions.ContinuousDistributionError)
			If _Min>_Max Then errs.Add(New Distributions.ContinuousDistributionError("The min cannot be greater than the max in the uniform distribuiton."))
			Return errs
		End Function
	End Class

End Namespace