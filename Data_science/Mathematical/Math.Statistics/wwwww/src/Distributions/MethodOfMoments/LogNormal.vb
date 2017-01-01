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
	Public Class LogNormal
		Inherits Distributions.ContinuousDistribution

		Private _Mean As Double
		Private _StDev As Double
		Public Sub New()
			'for reflection
			_Mean = 0
			_StDev = 1
		End Sub
		Public Sub New(ByVal mean As Double, ByVal stdev As Double, ByVal samplesize As Integer)
			_Mean = mean
			_StDev = stdev
			SetPeriodOfRecord(samplesize)
		End Sub
		''' <summary>
		''' This takes an input array of sample data, calculates the log base 10 of the data, then calculates the mean and standard deviation of the log data. </summary>
		''' <param name="data"> the sampled data (in linear space) </param>
		Public Sub New(ByVal data As Double())
			For i As Integer = 0 To data.Length - 1
				data(i) = Math.Log10(data(i))
			Next i
			Dim BPM As New MomentFunctions.BasicProductMoments(data)
			_Mean = BPM.GetMean()
			_StDev = BPM.GetStDev()
			SetPeriodOfRecord(BPM.GetSampleSize())
		End Sub
		Public Overrides Function GetInvCDF(ByVal probability As Double) As Double
			Dim z As New Normal(_Mean,_StDev)
			Return Math.Pow(10,z.GetInvCDF(probability))
		End Function
		Public Overrides Function GetCDF(ByVal value As Double) As Double
			Dim n As New Distributions.MethodOfMoments.Normal(_Mean,_StDev)
			Return n.GetCDF(Math.Log10(value))
		End Function
		Public Overrides Function GetPDF(ByVal value As Double) As Double
			Dim n As New Distributions.MethodOfMoments.Normal(_Mean,_StDev)
			Return n.GetPDF(Math.Log10(value))
		End Function
		Public Overridable Function Bullentin17BConfidenceLimit(ByVal probability As Double, ByVal alphaValue As Double) As Double
			Dim sn As New Normal(0,1)
			Dim k As Double = sn.GetInvCDF(probability)
			Dim z As Double = sn.GetInvCDF(alphaValue)
			Dim zSquared As Double = Math.Pow(z, 2)
			Dim kSquared As Double = Math.Pow(k, 2)
			Dim Avalue As Double = (1-(zSquared)/2\(GetPeriodOfRecord()-1))
			Dim Bvalue As Double = (kSquared) - ((zSquared)/GetPeriodOfRecord())
			Dim RootValue As Double = Math.Sqrt(kSquared-(Avalue*Bvalue))
			If alphaValue>.5 Then
				Return Math.Pow(10,_Mean + _StDev*(k + RootValue)/Avalue)
			Else
				Return Math.Pow(10,_Mean + _StDev*(k - RootValue)/Avalue)
			End If
		End Function
		Public Overrides Function Validate() As List(Of Distributions.ContinuousDistributionError)
			Dim errors As New List(Of Distributions.ContinuousDistributionError)
			If _StDev<=0 Then errors.Add(New Distributions.ContinuousDistributionError("Standard of Deviation must be greater than 0"))
			Return errors
		End Function
	End Class

End Namespace