Imports System.Collections.Generic

'
' * To change this license header, choose License Headers in Project Properties.
' * To change this template file, choose Tools | Templates
' * and open the template in the editor.
' 
Namespace TabularFunctions

	''' 
	''' <summary>
	''' @author Will_and_Sara
	''' </summary>
	Public Interface ISampleWithUncertainty
		Inherits IWriteToXML

		Function GetYFromX(ByVal x As Double, ByVal probability As Double) As Double
		Function GetYValues(ByVal probability As Double) As List(Of Double?)
		Function GetYDistributions() As List(Of Distributions.ContinuousDistribution)
		Function CurveSample(ByVal probability As Double) As ISampleDeterministically
	End Interface

End Namespace