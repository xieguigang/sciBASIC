#Region "License Header"
' This Source Code Form is subject to the terms of the Mozilla Public
' License, v. 2.0. If a copy of the MPL was not distributed with this
' file, You can obtain one at https://mozilla.org/MPL/2.0/.
#End Region

Imports System
Imports PrettyPrompt.Documents



'INSTANT VB WARNING: VB has no equivalent to the C# readonly struct:
'ORIGINAL LINE: public readonly record struct FormatSpan
Public record Structure FormatSpan
	Public Shared ReadOnly Empty As New FormatSpan(0, 0, ConsoleFormat.None)

	Public ReadOnly Span As TextSpan
	Public ReadOnly Formatting As ConsoleFormat

	''' <summary>
	''' Start point of the span.
	''' </summary>
	Public ReadOnly Property Start() As Integer
		Get
			Return Span.Start
		End Get
	End Property

	''' <summary>
	''' Length of the span.
	''' </summary>
	Public ReadOnly Property Length() As Integer
		Get
			Return Span.Length
		End Get
	End Property

	''' <summary>
	''' End of the span.
	''' </summary>
	Public ReadOnly Property End() As Integer
		Get
			Return Span.End
		End Get
	End Property

	''' <summary>
	''' Determines whether or not the span is empty.
	''' </summary>
	Public ReadOnly Property IsEmpty() As Boolean
		Get
			Return Span.IsEmpty
		End Get
	End Property

'INSTANT VB NOTE: The variable span was renamed since Visual Basic does not handle local variables named the same as class members well:
'INSTANT VB NOTE: The variable formatting was renamed since Visual Basic does not handle local variables named the same as class members well:
	Public Sub New(ByVal span_Conflict As TextSpan, ByVal formatting_Conflict As ConsoleFormat)
		Me.Span = span_Conflict
		Me.Formatting = formatting_Conflict
	End Sub

'INSTANT VB NOTE: The variable start was renamed since Visual Basic does not handle local variables named the same as class members well:
'INSTANT VB NOTE: The variable length was renamed since Visual Basic does not handle local variables named the same as class members well:
'INSTANT VB NOTE: The variable formatting was renamed since Visual Basic does not handle local variables named the same as class members well:
	Public Sub New(ByVal start_Conflict As Integer, ByVal length_Conflict As Integer, ByVal formatting_Conflict As ConsoleFormat)
		Me.New(New TextSpan(start_Conflict, length_Conflict), formatting_Conflict)
	End Sub

'INSTANT VB NOTE: The variable start was renamed since Visual Basic does not handle local variables named the same as class members well:
'INSTANT VB NOTE: The variable length was renamed since Visual Basic does not handle local variables named the same as class members well:
	Public Sub New(ByVal start_Conflict As Integer, ByVal length_Conflict As Integer, ByVal foregroundColor As AnsiColor)
		Me.New(start_Conflict, length_Conflict, New ConsoleFormat(Foreground:= foregroundColor))
	End Sub

'INSTANT VB NOTE: The variable start was renamed since Visual Basic does not handle local variables named the same as class members well:
'INSTANT VB NOTE: The variable end was renamed since Visual Basic does not handle local variables named the same as class members well:
'INSTANT VB NOTE: The variable formatting was renamed since Visual Basic does not handle local variables named the same as class members well:
	Public Shared Function FromBounds(ByVal start_Conflict As Integer, ByVal end_Conflict As Integer, ByVal formatting_Conflict As ConsoleFormat) As FormatSpan
		Return New(TextSpan.FromBounds(start_Conflict, [end]), formatting_Conflict)
	End Function

	''' <summary>
	''' Determines whether the position lies within the span.
	''' </summary>
	Public Function Contains(ByVal index As Integer) As Boolean
		Return index >= Start AndAlso index < Start + Length
	End Function

	''' <summary>
	''' Determines whether span falls completely within this span.
	''' </summary>
'INSTANT VB NOTE: The variable span was renamed since Visual Basic does not handle local variables named the same as class members well:
	Public Function Contains(ByVal span_Conflict As TextSpan) As Boolean
		Return span_Conflict.Start >= Start AndAlso span_Conflict.End <= [End]
	End Function

	''' <summary>
	''' Determines whether span overlaps this span. Two spans are considered to overlap if they have positions in common and neither is empty. Empty spans do not overlap with any other span.
	''' </summary>
'INSTANT VB NOTE: The variable span was renamed since Visual Basic does not handle local variables named the same as class members well:
	Public Function OverlapsWith(ByVal span_Conflict As TextSpan) As Boolean
		Return OverlapsWith(span_Conflict.Start, span_Conflict.Length)
	End Function

	''' <summary>
	''' Determines whether span overlaps this span. Two spans are considered to overlap if they have positions in common and neither is empty. Empty spans do not overlap with any other span.
	''' </summary>
'INSTANT VB NOTE: The variable start was renamed since Visual Basic does not handle local variables named the same as class members well:
'INSTANT VB NOTE: The variable length was renamed since Visual Basic does not handle local variables named the same as class members well:
	Public Function OverlapsWith(ByVal start_Conflict As Integer, ByVal length_Conflict As Integer) As Boolean
		Return Math.Max(Me.Start, start_Conflict) < Math.Min([End], start_Conflict + length_Conflict)
	End Function

	''' <summary>
	''' Returns the overlap with the given span, or null if there is no overlap.
	''' </summary>
'INSTANT VB NOTE: The variable start was renamed since Visual Basic does not handle local variables named the same as class members well:
'INSTANT VB NOTE: The variable length was renamed since Visual Basic does not handle local variables named the same as class members well:
	Public Function Overlap(ByVal start_Conflict As Integer, ByVal length_Conflict As Integer) As FormatSpan?
		Dim resultStart As Integer = Math.Max(Me.Start, start_Conflict)
		Dim resultEnd As Integer = Math.Min([End], start_Conflict + length_Conflict)
		If resultStart < resultEnd Then
			Return FromBounds(resultStart, resultEnd, Formatting)
		End If
		Return Nothing
	End Function

	''' <summary>
	'''  Determines whether span intersects this span. Two spans are considered to intersect
	'''  if they have positions in common or the end of one span coincides with the start
	'''  of the other span.
	''' </summary>
'INSTANT VB NOTE: The variable span was renamed since Visual Basic does not handle local variables named the same as class members well:
	Public Function IntersectsWith(ByVal span_Conflict As TextSpan) As Boolean
		Return span_Conflict.Start <= [End] AndAlso span_Conflict.End >= Start
	End Function

	''' <summary>
	''' Determines whether position intersects this span. A position is considered to
	''' intersect if it is between the start and end positions(inclusive) of this span.
	''' </summary>
	Public Function IntersectsWith(ByVal position As Integer) As Boolean
		Return CUInt(position - Start) <= CUInt(Length)
	End Function

	''' <summary>
	''' Returns the intersection with the given span, or null if there is no intersection.
	''' </summary>
'INSTANT VB NOTE: The variable span was renamed since Visual Basic does not handle local variables named the same as class members well:
	Public Function Intersection(ByVal span_Conflict As TextSpan) As FormatSpan?
		Dim resultStart As Integer = Math.Max(Start, span_Conflict.Start)
		Dim resultEnd As Integer = Math.Min([End], span_Conflict.End)
		If resultStart <= resultEnd Then
			Return FromBounds(resultStart, resultEnd, Formatting)
		End If
		Return Nothing
	End Function

	''' <summary>
	''' Creates new span translated by some offset.
	''' </summary>
'INSTANT VB NOTE: The parameter offset was renamed since Visual Basic will not allow parameters with the same name as their enclosing function or property:
	Public Function Offset(ByVal offset_Conflict As Integer) As FormatSpan
		Return New(Start + offset_Conflict, Length, Formatting)
	End Function

	''' <summary>
	''' Creates new span with new length.
	''' </summary>
'INSTANT VB NOTE: The variable length was renamed since Visual Basic does not handle local variables named the same as class members well:
	Public Function WithLength(ByVal length_Conflict As Integer) As FormatSpan
		Return New(Start, length_Conflict, Formatting)
	End Function

	Public Overrides Function ToString() As String
		Return $"[{Start}..{[End]})"
	End Function

'INSTANT VB WARNING: VB has no equivalent to C# 'in' parameters, so they will convert the same as by value parameters:
'ORIGINAL LINE: public bool Equals(in FormatSpan other)
	Public Function Equals(ByVal other As FormatSpan) As Boolean
		'struct is big so we use custom by-ref equals
		Return Span = other.Span AndAlso Formatting.Equals(in other.Formatting)
	End Function
End Structure