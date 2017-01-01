Imports System
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
	Public Class MonotonicallyIncreasingCurve
		Inherits TabularFunction
		Implements ISampleWithUncertainty, ISampleDeterministically

		Private _X As List(Of Double?)
		Private _Y As List(Of Double?)
		Public Overrides Function GetXValues() As List(Of Double?)
			Return _X
		End Function
        Public Function GetYValues() As List(Of Double?) Implements ISampleDeterministically.GetYValues
            Return _Y
        End Function
        Public Sub New(ByVal Xvalues As List(Of Double?), ByVal Yvalues As List(Of Double?))
			_X = Xvalues
			_Y = Yvalues
		End Sub
        'Public Sub New(ByVal ele As org.w3c.dom.Element)
        '	ReadFromXMLElement(ele)
        'End Sub
        Public Overrides Function FunctionType() As FunctionTypeEnum
			Return FunctionTypeEnum.MonotonicallyIncreasing
		End Function
        Public Function GetYFromX(ByVal x As Double) As Double Implements ISampleDeterministically.GetYFromX
            'determine how to implement a binary search.
            Dim index As Integer = java.util.Collections.binarySearch(_X, x)
            'if index is negative, it should be (-(index)-1);
            If index > 0 Then
                Return _Y(index)
            Else
                'interpolate. make sure the index is correctly determined.
                index *= -1
                If index >= _Y.Count Then Return _Y(_Y.Count - 1)
                Dim delta As Double = _X(index) - _X(index - 1)
                Dim distance As Double = x - _X(index) / delta
                Dim ydelta As Double = _Y(index) - _Y(index - 1)
                Return _Y(index - 1) + ydelta * distance
            End If
        End Function
        Public Overrides Function Validate() As List(Of TabularFunctionError)
			Dim output As New List(Of TabularFunctionError)
			If _Y.Count >= 1 Then Return output
			For i As Integer = 1 To _Y.Count - 1
				If _Y(i-1)>_Y(i) Then output.Add(New TabularFunctionError("Y is not monotonically increasing.",i,"Y Value","None"))
				If _X(i-1)>_X(i) Then output.Add(New TabularFunctionError("X is not monotonically increasing.",i,"X Value","None"))
			Next i
			Return output
		End Function
        Public Function GetYFromX(ByVal x As Double, ByVal probability As Double) As Double Implements ISampleWithUncertainty.GetYFromX
            'Basic functionality will return GetYFromX(double x).  MonotonicallyIncreasingCurveUncertain will override this method.
            Return GetYFromX(x)
        End Function
        Public Function GetYValues(ByVal probability As Double) As List(Of Double?) Implements ISampleWithUncertainty.GetYValues
            'Basic functionality will return _Y.  MonotonicallyIncreasingCurveUncertain will override this method.
            Return _Y
        End Function
        Public Function GetYDistributions() As List(Of Distributions.ContinuousDistribution) Implements ISampleWithUncertainty.GetYDistributions
            Dim output As New List(Of Distributions.ContinuousDistribution)
            For i As Integer = 0 To _Y.Count - 1
                output.Add(New Distributions.MethodOfMoments.Uniform(_Y(i), _Y(i)))
            Next i
            Return output
        End Function
        Public Function CurveSample(ByVal probability As Double) As ISampleDeterministically Implements ISampleWithUncertainty.CurveSample
            Return Me 'should implement a clone function
        End Function
        'Public Overrides Sub ReadFromXMLElement(ByVal ele As org.w3c.dom.Element) Implements IWriteToXML.ReadFromXMLElement
        '	_X = New List(Of )
        '	_Y = New List(Of )
        '	For i As Integer = 0 To ele.ChildNodes.Length - 1
        '		Dim N As org.w3c.dom.Node = ele.ChildNodes.item(i)
        '		If N.hasAttributes() Then
        '			Dim ord As org.w3c.dom.Element = CType(N, org.w3c.dom.Element)
        '			If ord.hasAttribute("X") Then
        '				_X.Add(Convert.ToDouble(ord.getAttribute("X")))
        '			ElseIf ord.hasAttribute("_X") Then
        '				_X.Add(Convert.ToDouble(ord.getAttribute("_X")))
        '			Else
        '				'no x attribute?
        '			End If
        '			If ord.hasAttribute("Y") Then
        '				_Y.Add(Convert.ToDouble(ord.getAttribute("Y")))
        '			ElseIf ord.hasAttribute("_Y") Then
        '				_Y.Add(Convert.ToDouble(ord.getAttribute("_Y")))
        '			ElseIf ord.hasAttribute("_value") Then
        '				_Y.Add(Convert.ToDouble(ord.getAttribute("_value")))
        '			Else
        '				'no Y attribute?
        '			End If
        '		End If
        '	Next i
        'End Sub

        'Public Overrides Function WriteToXMLElement() As org.w3c.dom.Element Implements IWriteToXML.WriteToXMLElement
        '	Try
        '		Dim d As javax.xml.parsers.DocumentBuilderFactory = javax.xml.parsers.DocumentBuilderFactory.newInstance()
        '		Dim Db As javax.xml.parsers.DocumentBuilder
        '		Db = d.newDocumentBuilder()
        '		Dim doc As org.w3c.dom.Document = Db.newDocument()
        '		Dim ele As org.w3c.dom.Element = doc.createElement(Me.GetType().Name)
        '		Dim ord As org.w3c.dom.Element
        '		For i As Integer = 0 To _Y.Count - 1
        '			ord = doc.createElement("Ordinate")
        '			ord.setAttribute("X", String.Format("{0:F5}",_X(i)))
        '			ord.setAttribute("Y", String.Format("{0:F5}",_Y(i)))

        '		Next i
        '		Return ele
        '	Catch ex As javax.xml.parsers.ParserConfigurationException
        '		java.util.logging.Logger.getLogger(GetType(MonotonicallyIncreasingCurveUncertain).Name).log(java.util.logging.Level.SEVERE, Nothing, ex)
        '	End Try
        '	Return Nothing
        'End Function
    End Class

End Namespace