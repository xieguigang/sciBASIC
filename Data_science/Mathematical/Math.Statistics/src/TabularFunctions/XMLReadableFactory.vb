''
'' * To change this license header, choose License Headers in Project Properties.
'' * To change this template file, choose Tools | Templates
'' * and open the template in the editor.
'' 
'Namespace TabularFunctions


'	''' 
'	''' <summary>
'	''' @author Will_and_Sara
'	''' </summary>
'	Public Class XMLReadableFactory
'		Public Shared Function ReadXMLElement(ByVal ele As org.w3c.dom.Element) As ISampleWithUncertainty
'			Select Case ele.TagName
'				Case "MonotonicallyIncreasingCurveUncertain"
'					Return New MonotonicallyIncreasingCurveUncertain(ele)
'				Case "MonotonicCurveUSingle" 'Statistics.dll
'					Return New MonotonicallyIncreasingCurveUncertain(ele)
'				Case "MonotonicallyIncreasingCurve"
'					Return New MonotonicallyIncreasingCurve(ele)
'				Case "MonotonicCurveSingle" 'Statistics.dll
'					Return New MonotonicallyIncreasingCurve(ele)
'				Case Else
'					Return Nothing
'			End Select
'		End Function
'	End Class

'End Namespace