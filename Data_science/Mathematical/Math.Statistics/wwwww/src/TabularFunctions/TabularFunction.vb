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
	Public MustInherit Class TabularFunction
		Public MustOverride Function GetXValues() As List(Of Double?)
		'public abstract ArrayList<Object> GetYValues();
		Public MustOverride Function FunctionType() As FunctionTypeEnum
		Public MustOverride Function Validate() As List(Of TabularFunctionError)
		'WriteToXElement
		'ReadFromXElement
	End Class

End Namespace