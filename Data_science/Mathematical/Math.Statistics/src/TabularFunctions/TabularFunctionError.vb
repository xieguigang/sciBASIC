#Region "Microsoft.VisualBasic::d8ec0c7a139c0debbe66ad44dbfba44f, ..\sciBASIC#\Data_science\Mathematical\Math.Statistics\src\TabularFunctions\TabularFunctionError.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

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
		Public Sub New( message As String,  row As Integer,  ParameterName As String,  DistributionType As String)
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
