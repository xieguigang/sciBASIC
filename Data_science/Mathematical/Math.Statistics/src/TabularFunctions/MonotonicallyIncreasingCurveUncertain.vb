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
	Public Class MonotonicallyIncreasingCurveUncertain
		Inherits TabularFunction
		Implements ISampleWithUncertainty

		Private _X As List(Of Double?)
		Private _Y As List(Of Distributions.ContinuousDistribution)
		Public Overrides Function GetXValues() As List(Of Double?)
			Return _X
		End Function
        Public Function GetYDistributions() As List(Of Distributions.ContinuousDistribution) Implements ISampleWithUncertainty.GetYDistributions
            Return _Y
        End Function
        Public Sub New(ByVal Xvalues As List(Of Double?), ByVal Yvalues As List(Of Distributions.ContinuousDistribution))
			_X = Xvalues
			_Y = Yvalues
		End Sub
        'Public Sub New(ByVal ele As org.w3c.dom.Element)
        '	ReadFromXMLElement(ele)
        'End Sub
        Public Function GetYFromX(ByVal x As Double, ByVal probability As Double) As Double Implements ISampleWithUncertainty.GetYFromX
            'determine how to implement a binary search.
            Dim index As Integer = java.util.Collections.binarySearch(_X, x)
            'if index is negative, it should be (-(index)-1);
            If index > 0 Then
                Return _Y(index).GetInvCDF(probability)
            Else
                'interpolate. make sure the index is correctly determined.
                index *= -1
                'check if index is larger than list
                If index >= _Y.Count Then Return _Y(_Y.Count - 1).GetInvCDF(probability)
                Dim delta As Double = _X(index) - _X(index - 1)
                Dim distance As Double = x - _X(index) / delta
                Dim min As Double = _Y(index - 1).GetInvCDF(probability)
                Dim max As Double = _Y(index).GetInvCDF(probability)
                Dim ydelta As Double = max - min 'could use optimization to reduce number of samples.
                Return _Y(index - 1).GetInvCDF(probability) + ydelta * distance
            End If
        End Function
        Public Function GetYValues(ByVal probability As Double) As List(Of Double?) Implements ISampleWithUncertainty.GetYValues
            Dim result As New List(Of Double?)
            For i As Integer = 0 To _Y.Count - 1
                result.Add(_Y(i).GetInvCDF(probability))
            Next i
            Return result
        End Function
        Public Overrides Function FunctionType() As FunctionTypeEnum
			Return FunctionTypeEnum.MonotonicallyIncreasingUncertain
		End Function
		Public Overrides Function Validate() As List(Of TabularFunctionError)
			Dim output As New List(Of TabularFunctionError)
			If _Y.Count >= 1 Then Return output
			Dim DistributionType As String = _Y(0).GetType().Name
			Dim upper As Double
			Dim prevupper As Double = _Y(0).GetInvCDF(.9999999999999)
			Dim lower As Double
			Dim prevlower As Double = _Y(0).GetInvCDF(.0000000000001)
			Dim fifty As Double
			Dim prevfifty As Double = _Y(0).GetInvCDF(.5)
			For i As Integer = 1 To _Y.Count - 1
				lower = _Y(i).GetInvCDF(.0000000000001)
				upper = _Y(i).GetInvCDF(.9999999999999)
				fifty = _Y(i).GetInvCDF(.5)
				If prevlower>lower Then output.Add(New TabularFunctionError("Y is not monotonically increasing for the lower confidence limit.",i,"Y Value lower",DistributionType))
				If prevupper>upper Then output.Add(New TabularFunctionError("Y is not monotonically increasing for the upper confidence limit.",i,"Y Value upper",DistributionType))
				If prevfifty>fifty Then output.Add(New TabularFunctionError("Y is not monotonically increasing for the 50% value.",i,"Y Value",DistributionType))
				prevlower = lower
				prevupper = upper
				prevfifty = fifty
				If _X(i-1)>_X(i) Then output.Add(New TabularFunctionError("X is not monotonically increasing.",i,"X Value",DistributionType))
			Next i
			Return output
		End Function

        Public Function CurveSample(ByVal probability As Double) As ISampleDeterministically Implements ISampleWithUncertainty.CurveSample
            Dim samples As New List(Of Double?)
            For i As Integer = 0 To _Y.Count - 1
                samples.Add(_Y(i).GetInvCDF(probability))
            Next i
            Return New MonotonicallyIncreasingCurve(_X, samples)
        End Function
        '		Public Overrides Sub ReadFromXMLElement(ByVal ele As org.w3c.dom.Element) Implements IWriteToXML.ReadFromXMLElement
        '			_X = New List(Of )
        '			_Y = New List(Of )
        '			Dim Dist As Distributions.ContinuousDistribution
        '			Dim c As Type
        '			Try
        '				If ele.hasAttribute("UncertaintyType") Then
        '					If ele.getAttribute("UncertaintyType").Equals("None") Then
        '						' no distribution type called none, this should be a MonotonicallyIncreasingCurve
        '					Else
        '						c = Type.GetType(ele.getAttribute("UncertaintyType"))
        '						Dist=CType(c.GetConstructor().newInstance(), Distributions.ContinuousDistribution)
        '						Dim flds As Field() = Dist.GetType().DeclaredFields
        '						For i As Integer = 1 To ele.ChildNodes.Length - 1
        '							Dist=CType(c.GetConstructor().newInstance(), Distributions.ContinuousDistribution)
        '							Dim N As org.w3c.dom.Node = ele.ChildNodes.item(i)
        '							If N.hasAttributes() Then
        '								Dim ord As org.w3c.dom.Element = CType(N, org.w3c.dom.Element)
        '								_X.Add(Convert.ToDouble(ord.getAttribute("X")))
        '								For Each f As Field In flds
        '									Select Case f.Type.Name
        '										Case "double"
        '											f.set(Dist,Convert.ToDouble(ord.getAttribute(f.Name)))
        '				'                        case "int":
        '				'                            f.set(Dist,Integer.parseInt(ele.getAttribute(f.getName())));
        '				'                            break;
        '										Case Else
        '											'throw error?
        '									End Select
        '								Next f
        '								_Y.Add(Dist)
        '							End If
        '						Next i
        '					End If
        '				Else
        '					' no distribution type, this should be a MonotonicallyIncreasingCurve
        '				End If

        ''JAVA TO VB CONVERTER TODO TASK: There is no equivalent in VB to Java 'multi-catch' syntax:
        '			Catch ClassNotFoundException Or NoSuchMethodException Or SecurityException Or InstantiationException Or IllegalAccessException Or System.ArgumentException Or InvocationTargetException ex
        '				java.util.logging.Logger.getLogger(GetType(MonotonicallyIncreasingCurveUncertain).Name).log(java.util.logging.Level.SEVERE, Nothing, ex)
        '			End Try
        '		End Sub
        '		Public Overrides Function WriteToXMLElement() As org.w3c.dom.Element Implements IWriteToXML.WriteToXMLElement
        '			Try
        '				Dim d As javax.xml.parsers.DocumentBuilderFactory = javax.xml.parsers.DocumentBuilderFactory.newInstance()
        '				Dim Db As javax.xml.parsers.DocumentBuilder
        '				Db = d.newDocumentBuilder()
        '				Dim doc As org.w3c.dom.Document = Db.newDocument()
        '				Dim ele As org.w3c.dom.Element = doc.createElement(Me.GetType().Name)
        '				ele.setAttribute("UncertaintyType", _Y(0).GetType().Name)
        '				Dim flds As Field() = _Y(0).GetType().DeclaredFields
        '				Dim ord As org.w3c.dom.Element
        '				For i As Integer = 0 To _Y.Count - 1
        '					ord = doc.createElement("Ordinate")
        '					ord.setAttribute("X", String.Format("{0:F5}",_X(i)))
        '					For Each f As Field In flds
        '						Try
        '							Select Case f.Type.Name
        '								Case "double"
        '									ord.setAttribute(f.Name, Convert.ToString(f.getDouble(_Y(i))))
        '								Case Else
        '							End Select
        ''JAVA TO VB CONVERTER TODO TASK: There is no equivalent in VB to Java 'multi-catch' syntax:
        '						Catch System.ArgumentException Or IllegalAccessException ex
        '							java.util.logging.Logger.getLogger(GetType(MonotonicallyIncreasingCurveUncertain).Name).log(java.util.logging.Level.SEVERE, Nothing, ex)
        '						End Try

        '					Next f
        '				Next i
        '				Return ele
        '			Catch ex As javax.xml.parsers.ParserConfigurationException
        '				java.util.logging.Logger.getLogger(GetType(MonotonicallyIncreasingCurveUncertain).Name).log(java.util.logging.Level.SEVERE, Nothing, ex)
        '			End Try
        '			Return Nothing
        '		End Function
    End Class

End Namespace