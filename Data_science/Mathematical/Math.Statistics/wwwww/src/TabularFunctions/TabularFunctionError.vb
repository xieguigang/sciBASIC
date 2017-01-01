Imports Microsoft.VisualBasic
Imports System.Text

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
	Public Class TabularFunctionError
		Private ReadOnly _ErrorMessage As String
		Private ReadOnly _Row As Integer
		Private ReadOnly _ParameterName As String
		Private ReadOnly _DistributionType As String
		Public Sub New(ByVal message As String, ByVal row As Integer, ByVal ParameterName As String, ByVal DistributionType As String)
			_ErrorMessage = message
			_Row = row
			_ParameterName = ParameterName
			_DistributionType = DistributionType
		End Sub
		Public Overridable Function GetErrorMessage() As String
			Return _ErrorMessage
		End Function
		Public Overridable Function GetRow() As Integer
			Return _Row
		End Function
		Public Overridable Function GetParameterName() As String
			Return _ParameterName
		End Function
		Public Overridable Function GetDistributionType() As String
			Return _DistributionType
		End Function
		Public Overridable Function GetFormattedError() As String
			Dim SB As New StringBuilder
			SB.Append("An error occured on row ")
			SB.Append(_Row)
			SB.Append(", for the parameter ")
			SB.Append(_ParameterName)
			SB.Append(" in a distribution of type ")
			SB.Append(_DistributionType)
			SB.Append(". The Error was: ")
			SB.Append(_ErrorMessage)
			SB.Append(vbLf)
			Return SB.ToString()
		End Function
	End Class

End Namespace