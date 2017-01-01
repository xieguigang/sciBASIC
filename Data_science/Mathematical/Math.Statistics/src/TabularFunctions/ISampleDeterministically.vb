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
	Public Interface ISampleDeterministically
		Inherits IWriteToXML

		Function GetYFromX(ByVal x As Double) As Double
		Function GetYValues() As List(Of Double?)
	End Interface

End Namespace