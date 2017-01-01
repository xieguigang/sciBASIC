Imports System

'
' * To change this license header, choose License Headers in Project Properties.
' * To change this template file, choose Tools | Templates
' * and open the template in the editor.
' 
Namespace MomentFunctions

	''' 
	''' <summary>
	''' @author Will_and_Sara
	''' </summary>
	Public Class ProductMoments
		Private _Min As Double
		Private _Max As Double
		Private _Mean As Double
		Private _StandardDeviation As Double
		'private double _Median;
		Private _Skew As Double
		Private _Kurtosis As Double
		Private _Count As Integer
		Public Sub New(ByVal data As Double())
			_Count = data.Length
			Dim BPM As New BasicProductMoments(data)
			_Min = BPM.GetMin()
			_Max = BPM.GetMax()
			_Mean = BPM.GetMean()
			_StandardDeviation = BPM.GetStDev()
			Dim skewsums As Double = 0
			Dim ksums As Double = 0
			For i As Integer = 0 To data.Length - 1
				skewsums += Math.Pow((data(i)-_Mean),3)
				ksums += Math.Pow(((data(i)-_Mean)/_StandardDeviation),4)
			Next i
			'just alittle more math...
			ksums *= (_Count*(_Count+1))\((_Count-1)*(_Count-2)*(_Count-3))
			_Skew = (_Count*skewsums)/((_Count-1)*(_Count-2)*Math.Pow(_StandardDeviation,3))
			_Kurtosis = ksums - ((3*(Math.Pow(_Count-1, 2)))/((_Count-2)*(_Count-3)))
			'figure out an efficent algorithm for median...
		End Sub
		Public Overridable Function GetSkew() As Double
			Return _Skew
		End Function
		Public Overridable Function GetKurtosis() As Double
			Return _Kurtosis
		End Function
		Public Overridable Function GetMin() As Double
			Return _Min
		End Function
		Public Overridable Function GetMax() As Double
			Return _Max
		End Function
		Public Overridable Function GetMean() As Double
			Return _Mean
		End Function
		Public Overridable Function GetStDev() As Double
			Return _StandardDeviation
		End Function
		Public Overridable Function GetSampleSize() As Integer
			Return _Count
		End Function

	End Class

End Namespace