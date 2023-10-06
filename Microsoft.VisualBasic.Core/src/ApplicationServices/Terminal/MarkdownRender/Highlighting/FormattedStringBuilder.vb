#Region "License Header"
' This Source Code Form is subject to the terms of the Mozilla Public
' License, v. 2.0. If a copy of the MPL was not distributed with this
' file, You can obtain one at https://mozilla.org/MPL/2.0/.
#End Region

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics.CodeAnalysis
Imports System.Text
Imports PrettyPrompt.Documents



'INSTANT VB WARNING: VB has no equivalent to the C# readonly struct:
'ORIGINAL LINE: public readonly struct FormattedStringBuilder
Public Structure FormattedStringBuilder
	Private ReadOnly stringBuilder As New StringBuilder()
	Private ReadOnly formatSpans As New List(Of FormatSpan)()

	Public Sub New()
	End Sub 'this line is needed in GitHub CI build, but not needed in VS 17.0.4

	Public ReadOnly Property Length() As Integer
		Get
			Return stringBuilder.Length
		End Get
	End Property
	Public ReadOnly Property IsDefault() As Boolean
		Get
			Return TypeOf stringBuilder Is Nothing
		End Get
	End Property

	Public Function Append(ByVal text As FormattedString) As FormattedStringBuilder
		For Each readonly As In text.FormatSpans
			formatSpans.Add(span.Offset(stringBuilder.Length))
		Next readonly
		stringBuilder.Append(text.Text)
		Return Me
	End Function

	Public Function Append(ByVal character As Char) As FormattedStringBuilder
		stringBuilder.Append(character)
		Return Me
	End Function

'INSTANT VB WARNING: Nullable reference types have no equivalent in VB:
'ORIGINAL LINE: public FormattedStringBuilder Append(string? text)
	Public Function Append(ByVal text As String) As FormattedStringBuilder
		stringBuilder.Append(text)
		Return Me
	End Function

	Public Function Append(ByVal text As ReadOnlySpan(Of Char)) As FormattedStringBuilder
		stringBuilder.Append(text)
		Return Me
	End Function

	Public Function Append(ByVal text As String, ParamArray ByVal formatSpans() As FormatSpan) As FormattedStringBuilder
		Return Append(New FormattedString(text, formatSpans))
	End Function

	Public Function Append(ByVal text As String, ByVal format As ConsoleFormat) As FormattedStringBuilder
		Return Append(text, New FormatSpan(New TextSpan(0, text.Length), format))
	End Function

	Public Sub Clear()
		stringBuilder.Clear()
		formatSpans.Clear()
	End Sub

	Public Function ToFormattedString() As FormattedString
		Return New(stringBuilder.ToString(), formatSpans)
	End Function

	Public Overrides Function ToString() As String
		Return stringBuilder.ToString()
	End Function

'INSTANT VB WARNING: VB has no equivalent to C# 'in' parameters, so they will convert the same as by value parameters:
'ORIGINAL LINE: public static bool operator ==(in FormattedStringBuilder left, in FormattedStringBuilder right)
	Public Shared Operator =(ByVal left As FormattedStringBuilder, ByVal right As FormattedStringBuilder) As Boolean
		Return left.stringBuilder Is right.stringBuilder
	End Operator

'INSTANT VB WARNING: VB has no equivalent to C# 'in' parameters, so they will convert the same as by value parameters:
'ORIGINAL LINE: public static bool operator !=(in FormattedStringBuilder left, in FormattedStringBuilder right)
	Public Shared Operator <>(ByVal left As FormattedStringBuilder, ByVal right As FormattedStringBuilder) As Boolean
		Return Not (left = right)
	End Operator

	Public Overrides Function Equals(<NotNullWhen(True)> Object? ByVal obj As ) As Boolean
		Dim tempVar As Boolean = TypeOf obj Is FormattedStringBuilder
		Dim builder As FormattedStringBuilder = If(tempVar, CType(obj, FormattedStringBuilder), Nothing)
		Return tempVar AndAlso builder = Me
	End Function

	Public Overrides Function GetHashCode() As Integer
		Return stringBuilder.GetHashCode()
	End Function
End Structure