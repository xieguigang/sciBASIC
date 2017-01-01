Imports System

'
' * To change this license header, choose License Headers in Project Properties.
' * To change this template file, choose Tools | Templates
' * and open the template in the editor.
' 
Namespace Distributions

	''' 
	''' <summary>
	''' @author Will_and_Sara
	''' </summary>
	Public Class ExampleMonteCarlo
		Public Shared Sub Main(ByVal args As String())
			MonteCarlo()
		End Sub
		Public Shared Sub MonteCarlo()
			'this is a very trivial example of creating a monte carlo using a
			'standard normal distribution
			Dim SN As New Distributions.MethodOfMoments.Normal
			Dim output As Double() = New Double(999){}
			Dim r As New Random
			For i As Integer = 0 To output.Length - 1
				output(i) =SN.GetInvCDF(r.NextDouble())
			Next i
			'output now contains 1000 random normally distributed values.

			'to evaluate the mean and standard deviation of the output
			'you can use Basic Product Moment Stats
			Dim BPM As New MomentFunctions.BasicProductMoments(output)
			Console.WriteLine("Mean: " & BPM.GetMean())
			Console.WriteLine("StDev:" & BPM.GetStDev())
			Console.WriteLine("Sample Size: " & BPM.GetSampleSize())
			Console.WriteLine("Minimum: " & BPM.GetMin())
			Console.WriteLine("Maximum: " & BPM.GetMax())
		End Sub
	End Class

End Namespace