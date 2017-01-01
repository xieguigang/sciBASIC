Imports Microsoft.VisualBasic
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
	Public Class BasicProductMomentsHistogram
		Inherits BasicProductMoments

		Private _Bins As Integer()
		Private _ExpectedMin As Double
		Private _ExpectedMax As Double
		Public Overridable Function Bins() As Integer()
			Return _Bins
		End Function
		Public Sub New(ByVal NumBins As Integer, ByVal Min As Double, ByVal Max As Double)
			MyBase.New()
			_Bins = New Integer(NumBins - 1){}
			_ExpectedMin = Min
			_ExpectedMax = Max
		End Sub
	'    public BasicProductMomentsHistogram(double binwidth){
	'        //need to change the logic to be based on binwidth....
	'        _Bins = new int[0];
	'        
	'    }
		Public Overrides Sub AddObservation(ByVal observation As Double)
			MyBase.AddObservation(observation)
			'histogram logic. Currently this is not designed to be as efficent as possible.  needs work (buffer block copy for instance)
			If observation < _ExpectedMin Then
				Dim binwidth As Double = (_ExpectedMax - _ExpectedMin)/_Bins.Length
				Dim overdist As Integer = CInt(Fix(Math.Ceiling(-(observation-_ExpectedMin)/binwidth)))
				_ExpectedMin = _ExpectedMin - overdist*binwidth
				Dim tmparray As Integer() = New Integer(_Bins.Length + overdist-2){}
				For i As Integer = overdist To _Bins.Length - 1
					tmparray(i) = _Bins(i)
				Next i
				_Bins = tmparray
			ElseIf observation>_ExpectedMax Then
				Dim binwidth As Double = (_ExpectedMax - _ExpectedMin)/_Bins.Length
				Dim overdist As Integer = CInt(Fix(Math.Ceiling((observation-_ExpectedMax)/binwidth)))
				_ExpectedMax = _ExpectedMax + overdist*binwidth
				Dim tmparray As Integer() = New Integer(_Bins.Length + overdist-2){}
				For i As Integer = 0 To _Bins.Length - 1
					tmparray(i) = _Bins(i)
				Next i
				_Bins = tmparray
			End If
			Dim index As Integer = CInt(Fix(Math.Floor(_Bins.Length * (observation-_ExpectedMin)/ (_ExpectedMax-_ExpectedMin))))
			_Bins(index)+=1
		End Sub
	End Class

End Namespace