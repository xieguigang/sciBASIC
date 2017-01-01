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
	Public Class Emperical
		Inherits Distributions.ContinuousDistribution

		Private _CumulativeProbabilities As Double()
		Private _ExceedanceValues As Double()
		Public Overrides Function GetInvCDF(ByVal probability As Double) As Double
			Dim index As Integer = Array.BinarySearch(_CumulativeProbabilities, probability)
			'interpolate or step?
			'check for array out of bounds...
			Return _ExceedanceValues(index)
		End Function
		Public Overrides Function GetCDF(ByVal value As Double) As Double
			Dim index As Integer = Array.BinarySearch(_ExceedanceValues, value)
			'interpolate or step?
			Return _CumulativeProbabilities(index)
		End Function
		Public Overrides Function GetPDF(ByVal value As Double) As Double
			Throw New System.NotSupportedException("Not supported yet.") 'To change body of generated methods, choose Tools | Templates.
		End Function
		Public Overrides Function Validate() As List(Of Distributions.ContinuousDistributionError)
			Dim errors As New List(Of Distributions.ContinuousDistributionError)
			If _CumulativeProbabilities.Length <> _ExceedanceValues.Length Then errors.Add(New Distributions.ContinuousDistributionError("Cumulative Probability values and Emperical Exceedance values are different lengths."))
			Return errors
		End Function

	End Class

End Namespace