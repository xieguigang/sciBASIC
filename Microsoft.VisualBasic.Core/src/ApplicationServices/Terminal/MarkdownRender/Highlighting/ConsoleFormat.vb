#Region "License Header"
' This Source Code Form is subject to the terms of the Mozilla Public
' License, v. 2.0. If a copy of the MPL was not distributed with this
' file, You can obtain one at https://mozilla.org/MPL/2.0/.
#End Region



'INSTANT VB WARNING: VB has no equivalent to the C# readonly struct:
'ORIGINAL LINE: public readonly record struct ConsoleFormat(System.Nullable<AnsiColor> Foreground = null, System.Nullable<AnsiColor> Background = null, bool Bold = false, bool Underline = false, bool Inverted = false)
'INSTANT VB TODO TASK: The following line contains an assignment within expression that was not extracted by Instant VB:
Public Structure ConsoleFormat
	Public Shared ReadOnly Property None() As ConsoleFormat
		Get
			Return Nothing
		End Get
	End Property

	Public ReadOnly Property ForegroundCode() As String
		Get
			Return Foreground?.GetCode(AnsiColor.Type.Foreground)
		End Get
	End Property
	Public ReadOnly Property BackgroundCode() As String
		Get
			Return Background?.GetCode(AnsiColor.Type.Background)
		End Get
	End Property

	ReadOnly Foreground As AnsiColor?
	ReadOnly Background As AnsiColor?
	ReadOnly Bold As Boolean
	ReadOnly Underline As Boolean
	ReadOnly Inverted As Boolean

	Sub New(Optional Foreground As AnsiColor = Nothing, Optional Background As AnsiColor = Nothing, Optional Bold As Boolean = False, Optional Underline As Boolean = False, Optional Inverted As Boolean = False)
		Me.Foreground = Foreground
		Me.Background = Background
		Me.Bold = Bold
		Me.Underline = Underline
		Me.Inverted = Inverted
	End Sub

	'INSTANT VB TODO TASK: Instant VB has no equivalent to readonly methods:
	'ORIGINAL LINE: public readonly bool Equals(in ConsoleFormat other)
	'INSTANT VB WARNING: VB has no equivalent to C# 'in' parameters, so they will convert the same as by value parameters:
	Public Overloads Function Equals(ByVal other As ConsoleFormat) As Boolean
		'this is hot from IncrementalRendering.CalculateDiff, so we want to use custom Equals where 'other' is by-ref
		Return Foreground = other.Foreground AndAlso Background = other.Background AndAlso Bold = other.Bold AndAlso Underline = other.Underline AndAlso Inverted = other.Inverted
	End Function

	Public ReadOnly Property IsDefault() As Boolean
		Get
			Return Not Foreground.HasValue AndAlso Not Background.HasValue AndAlso Not Bold AndAlso Not Underline AndAlso Not Inverted
		End Get
	End Property
End Structure