#Region "License Header"
' This Source Code Form is subject to the terms of the Mozilla Public
' License, v. 2.0. If a copy of the MPL was not distributed with this
' file, You can obtain one at https://mozilla.org/MPL/2.0/.
#End Region

Imports System
Imports System.Buffers
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Diagnostics.CodeAnalysis
Imports System.Globalization
Imports System.Linq
Imports System.Runtime.CompilerServices
Imports PrettyPrompt.Rendering

Namespace ApplicationServices.Terminal

	''' <summary>
	''' Represents text with associated non-overlapping formating spans.
	''' </summary>
	'INSTANT VB WARNING: VB has no equivalent to the C# readonly struct:
	'ORIGINAL LINE: public readonly struct FormattedString : IEquatable<FormattedString>
	Public Structure FormattedString
		Implements IEquatable(Of FormattedString)

		Public Shared ReadOnly Property Empty() As FormattedString
			Get
				Return String.Empty
			End Get
		End Property

		'INSTANT VB WARNING: Nullable reference types have no equivalent in VB:
		'ORIGINAL LINE: public string? Text {get;}
		Public ReadOnly Property Text() As String
		'INSTANT VB NOTE: The field formatSpans was renamed since Visual Basic does not allow fields to have the same name as other class members:
		Private ReadOnly formatSpans_Conflict() As FormatSpan

		Public ReadOnly Property FormatSpans() As ReadOnlySpan(Of FormatSpan)
			Get
				Return If(formatSpans_Conflict, Array.Empty(Of FormatSpan)())
			End Get
		End Property
		Public ReadOnly Property Length() As Integer
			Get
				Return If(Text?.Length, 0)
			End Get
		End Property

		Private ReadOnly Property TextOrEmpty() As String
			Get
				Return If(Text, "")
			End Get
		End Property
		Private ReadOnly Property FormatSpansOrEmpty() As FormatSpan()
			Get
				Return If(formatSpans_Conflict, Array.Empty(Of FormatSpan)())
			End Get
		End Property

		<MemberNotNullWhen(False, NameOf(Text))>
		Public IsEmpty As Function(Boolean) Length = 0

		'INSTANT VB WARNING: Nullable reference types have no equivalent in VB:
		'ORIGINAL LINE: public FormattedString(string? text, System.Nullable<IEnumerable<FormatSpan>> formatSpans)
		'INSTANT VB NOTE: The variable text was renamed since Visual Basic does not handle local variables named the same as class members well:
		'INSTANT VB NOTE: The variable formatSpans was renamed since Visual Basic does not handle local variables named the same as class members well:
		Public Sub New(ByVal text_Conflict As String, ByVal formatSpans_Conflict As IEnumerable(Of FormatSpan))
			Me.New(text_Conflict, formatSpans_Conflict?.ToArray())
		End Sub

		'INSTANT VB WARNING: Nullable reference types have no equivalent in VB:
		'ORIGINAL LINE: public FormattedString(string? text)
		'INSTANT VB NOTE: The variable text was renamed since Visual Basic does not handle local variables named the same as class members well:
		Public Sub New(ByVal text_Conflict As String)
			Me.Text = text_Conflict
			formatSpans_Conflict = Array.Empty(Of FormatSpan)()
		End Sub

		'INSTANT VB WARNING: Nullable reference types have no equivalent in VB:
		'ORIGINAL LINE: public FormattedString(string? text, params FormatSpan[]? formatSpans)
		'INSTANT VB NOTE: The variable text was renamed since Visual Basic does not handle local variables named the same as class members well:
		'INSTANT VB NOTE: The variable formatSpans was renamed since Visual Basic does not handle local variables named the same as class members well:
		Public Sub New(ByVal text_Conflict As String, ParamArray FormatSpan()? ByVal formatSpans_Conflict As )
			Me.Text = text_Conflict
			If TypeOf formatSpans_Conflict Is Nothing Then
				Me.formatSpans_Conflict = Array.Empty(Of FormatSpan)()
			Else
				Select Case formatSpans_Conflict.Length
					Case 0
						Me.formatSpans_Conflict = formatSpans_Conflict
					Case 1
						Me.formatSpans_Conflict = If(formatSpans_Conflict(0).Length > 0, formatSpans_Conflict, Array.Empty(Of FormatSpan)())
					Case Else
						'slow path
						Me.formatSpans_Conflict = formatSpans_Conflict!.Where(Function(s) s.Length > 0).OrderBy(Function(s) s.Start).ToArray()
						CheckFormatSpans()
				End Select
			End If
		End Sub

		'INSTANT VB WARNING: Nullable reference types have no equivalent in VB:
		'ORIGINAL LINE: public FormattedString(string? text, List<FormatSpan> formatSpans)
		'INSTANT VB NOTE: The variable text was renamed since Visual Basic does not handle local variables named the same as class members well:
		'INSTANT VB NOTE: The variable formatSpans was renamed since Visual Basic does not handle local variables named the same as class members well:
		Public Sub New(ByVal text_Conflict As String, ByVal formatSpans_Conflict As List(Of FormatSpan))
			Me.Text = text_Conflict
			Select Case formatSpans_Conflict.Count
				Case 0
					Me.formatSpans_Conflict = Array.Empty(Of FormatSpan)()
				Case 1
					Me.formatSpans_Conflict = If(formatSpans_Conflict(0).Length > 0, formatSpans_Conflict.ToArray(), Array.Empty(Of FormatSpan)())
				Case Else
					'slow path
					Me = New FormattedString(text_Conflict, formatSpans_Conflict.ToArray())
			End Select
		End Sub

		'INSTANT VB WARNING: Nullable reference types have no equivalent in VB:
		'ORIGINAL LINE: public FormattedString(string? text, in ConsoleFormat formatting)
		'INSTANT VB NOTE: The variable text was renamed since Visual Basic does not handle local variables named the same as class members well:
		'INSTANT VB WARNING: VB has no equivalent to C# 'in' parameters, so they will convert the same as by value parameters:
		Public Sub New(ByVal text_Conflict As String, ByVal formatting As ConsoleFormat)
			Me.New(text_Conflict, If((If(text_Conflict?.Length, 0)) = 0, Array.Empty(Of FormatSpan)(), {New FormatSpan(0, text_Conflict!.Length, formatting)}))
		End Sub

		'INSTANT VB NOTE: The variable text was renamed since Visual Basic does not handle local variables named the same as class members well:
		Public Shared Widening Operator CType(String? ByVal text_Conflict As ) As FormattedString
			Return New(text_Conflict)
		End Operator

		Public Shared Operator +(ByVal left As FormattedString, ByVal right As FormattedString) As FormattedString
			Dim resultText = left.TextOrEmpty & right.TextOrEmpty

			Dim leftFormatSpans = left.FormatSpansOrEmpty
			Dim rightFormatSpans = right.FormatSpansOrEmpty
			Dim resultFormatSpansCount = leftFormatSpans.Length + rightFormatSpans.Length
			Dim resultFormatSpans() As FormatSpan
			If resultFormatSpansCount > 0 Then
				resultFormatSpans = New FormatSpan(resultFormatSpansCount - 1) {}
				leftFormatSpans.AsSpan().CopyTo(resultFormatSpans)
				For i As Integer = 0 To rightFormatSpans.Length - 1
					resultFormatSpans(leftFormatSpans.Length + i) = rightFormatSpans(i).Offset(left.TextOrEmpty.Length)
				Next i
			Else
				resultFormatSpans = Array.Empty(Of FormatSpan)()
			End If

			Return New FormattedString(resultText, resultFormatSpans)
		End Operator

		Public Function GetUnicodeWidth() As Integer
			Return UnicodeWidth.GetWidth(Text)
		End Function

		''' <summary>
		''' Removes all leading and trailing white-space characters from the current string.
		''' </summary>
		Public Function Trim() As FormattedString
			If TypeOf Text Is Nothing Then
				Return Empty
			End If
			If FormatSpansOrEmpty.Length = 0 Then
				Return Text.Trim()
			End If

			Dim trimedCharsFromLeft = Text.Length - Text.AsSpan().TrimStart().Length
			If trimedCharsFromLeft = Text.Length Then
				Return Empty
			End If

			Dim trimedCharsFromRight = Text.Length - Text.AsSpan().TrimEnd().Length
			Return Substring(trimedCharsFromLeft, Text.Length - trimedCharsFromLeft - trimedCharsFromRight)
		End Function

		''' <summary>
		''' Retrieves a substring from this instance. The substring starts at a specified character position and has a specified length.
		''' </summary>
		'INSTANT VB NOTE: The variable length was renamed since Visual Basic does not handle local variables named the same as class members well:
		Public Function Substring(ByVal startIndex As Integer, ByVal length_Conflict As Integer) As FormattedString
			'formal argument validation will be done in Text.Substring(...)
			Debug.Assert(startIndex >= 0 AndAlso startIndex <= Me.Length)
			Debug.Assert(length_Conflict >= 0 AndAlso length_Conflict - startIndex <= Me.Length)

			If TypeOf Text Is Nothing OrElse length_Conflict = 0 Then
				Return Empty
			End If
			If length_Conflict - startIndex = Me.Length Then
				Return Me
			End If

			'INSTANT VB NOTE: The local variable substring was renamed since Visual Basic will not allow local variables with the same name as their enclosing function or property:
			Dim substring_Conflict = Text.Substring(startIndex, length_Conflict)
			If FormatSpansOrEmpty.Length = 0 Then
				Return substring_Conflict
			End If

			Dim resultFormatSpans = ListPool(Of FormatSpan).Shared.Get(formatSpans_Conflict.Length)
			'INSTANT VB NOTE: The variable formatSpan was renamed since it may cause conflicts with calls to static members of the user-defined type with this name:
			For Each formatSpan_Conflict In formatSpans_Conflict
				Dim newSpan As T
				If formatSpan_Conflict.Overlap(startIndex, length_Conflict).TryGet(newSpan) Then
					resultFormatSpans.Add(newSpan.Offset(-startIndex))
				End If
			Next formatSpan_Conflict

			Dim result = New FormattedString(substring_Conflict, resultFormatSpans)
			ListPool(Of FormatSpan).Shared.Put(resultFormatSpans)
			Return result
		End Function

		''' <summary>
		''' Returns a new string in which all occurrences of a specified string in the current instance are replaced with another specified string.
		''' </summary>
		Public Function Replace(ByVal oldValue As String, ByVal newValue As String) As FormattedString
			If TypeOf Text Is Nothing Then
				Return Empty
			End If
			If FormatSpansOrEmpty.Length = 0 Then
				Return Text.Replace(oldValue, newValue)
			End If

			'INSTANT VB NOTE: The variable text was renamed since Visual Basic does not handle local variables named the same as class members well:
			Dim text_Conflict = Text.AsSpan()
			Dim currentOffsetInPartialyReplacedText As Integer = 0

			Dim sb = StringBuilderPool.Shared.Get(oldValue.Length)
			'INSTANT VB NOTE: The variable formatSpans was renamed since Visual Basic does not handle local variables named the same as class members well:
			Dim formatSpans_Conflict = Me.formatSpans_Conflict.ToArray()
			Dim formatIndex As Integer = 0
			Do
				Dim replaceIndex = text_Conflict.IndexOf(oldValue)
				If replaceIndex < 0 Then
					Exit Do
				End If

				sb.Append(text_Conflict.Slice(0, replaceIndex))
				sb.Append(newValue)

				Dim replaceIndexInPartialyReplacedText = replaceIndex + currentOffsetInPartialyReplacedText
				For i As Integer = formatIndex To formatSpans_Conflict.Length - 1
					'INSTANT VB TODO TASK: 'ref locals' are not converted by Instant VB:
					'ORIGINAL LINE: ref var formatSpan = ref formatSpans[i];
					'INSTANT VB NOTE: The variable formatSpan was renamed since it may cause conflicts with calls to static members of the user-defined type with this name:
					Dim formatSpan_Conflict = formatSpans_Conflict(i)
					If replaceIndexInPartialyReplacedText >= formatSpan_Conflict.End Then
						'replace happens after current span, so we don't care about it anymore
						Debug.Assert(i = formatIndex)
						formatIndex += 1
					ElseIf formatSpan_Conflict.OverlapsWith(start:=replaceIndexInPartialyReplacedText, length:=oldValue.Length) Then
						'replace overlaps with current span
						If replaceIndexInPartialyReplacedText >= formatSpan_Conflict.Start AndAlso oldValue.Length >= formatSpan_Conflict.Length Then
							'replace happens inside current span, so we just make the span shorter
							formatSpan_Conflict = formatSpan_Conflict.WithLength(formatSpan_Conflict.Length - oldValue.Length + newValue.Length)
						Else
							'complex ovelap - we cannot decide what to do - throw the span away
							formatSpan_Conflict = FormatSpan.Empty
						End If
					Else
						'replace happens before current span, so we just need to translate the whole span
						formatSpan_Conflict = formatSpan_Conflict.Offset(newValue.Length - oldValue.Length)
					End If
				Next i

				text_Conflict = text_Conflict.Slice(replaceIndex + oldValue.Length)
				currentOffsetInPartialyReplacedText += replaceIndex + newValue.Length
			Loop

			sb.Append(text_Conflict)

			Dim resultText = sb.ToString()
			StringBuilderPool.Shared.Put(sb)
			Return New FormattedString(resultText, formatSpans_Conflict)
		End Function

		''' <summary>
		''' Splits a string into substrings based on the provided character separator.
		''' </summary>
		Public Iterator Function Split(ByVal separator As Char) As IEnumerable(Of FormattedString)
			'INSTANT VB NOTE: The variable text was renamed since Visual Basic does not handle local variables named the same as class members well:
			Dim text_Conflict As String = Text
			If TypeOf text_Conflict Is Nothing Then
				Return
			End If

			If FormatSpansOrEmpty.Length = 0 Then
				For Each part In text_Conflict.Split(separator)
					Yield part
				Next part
			Else
				Dim partStart As Integer = 0
				Dim formattingList = ListPool(Of FormatSpan).Shared.Get(formatSpans_Conflict.Length)
				Dim usedFormattingCount As Integer = 0
				Dim previousFormattingCharsUsed As Integer = 0
				Do While partStart < text_Conflict.Length
					Dim partLength As Integer = 0
					Dim i As Integer = partStart
					Do While i < text_Conflict.Length
						If text_Conflict.Chars(i) = separator Then
							Exit Do
						End If
						i += 1
						partLength += 1
					Loop

					GenerateFormattingsForPart(partStart, partLength, partSeparatorLength:=1, usedFormattingCount, previousFormattingCharsUsed, formattingList)

					Yield New FormattedString(text_Conflict.AsSpan(partStart, partLength).ToString(), formattingList)
					partStart += partLength + 1 '+1 to skip separator
				Loop
				ListPool(Of FormatSpan).Shared.Put(formattingList)
			End If
		End Function

		Public Iterator Function SplitIntoChunks(ByVal chunkSize As Integer) As IEnumerable(Of FormattedString)
			If chunkSize < 1 Then
				Throw New ArgumentOutOfRangeException(NameOf(chunkSize), "has to be >= 1")
			End If

			'INSTANT VB NOTE: The variable text was renamed since Visual Basic does not handle local variables named the same as class members well:
			Dim text_Conflict As String = Text
			If TypeOf text_Conflict Is Nothing Then
				Return
			End If

			Dim stringWidth = UnicodeWidth.GetWidth(Text)
			If stringWidth <= chunkSize Then
				Yield Me
				Return
			End If

			Dim partStart As Integer = 0
			Dim formattingList = ListPool(Of FormatSpan).Shared.Get(formatSpans_Conflict.Length)
			Dim usedFormattingCount As Integer = 0
			Dim previousFormattingCharsUsed As Integer = 0
			Do While partStart < text_Conflict.Length
				Dim partLength As Integer = 0
				Dim i As Integer = partStart
				Dim partWidth As Integer = 0
				Do While i < text_Conflict.Length
					Dim cWidth = UnicodeWidth.GetWidth(text_Conflict.Chars(i))
					partWidth += cWidth
					If partWidth > chunkSize Then
						Exit Do
					End If
					i += 1
					partLength += 1
				Loop

				GenerateFormattingsForPart(partStart, partLength, partSeparatorLength:=0, usedFormattingCount, previousFormattingCharsUsed, formattingList)

				Yield New FormattedString(text_Conflict.AsSpan(partStart, partLength).ToString(), formattingList)
				partStart += partLength
			Loop
			ListPool(Of FormatSpan).Shared.Put(formattingList)
		End Function

		Private Sub GenerateFormattingsForPart(ByVal partStart As Integer, ByVal partLength As Integer, ByVal partSeparatorLength As Integer, ByRef usedFormattingCount As Integer, ByRef previousFormattingCharsUsed As Integer, ByVal formattingList As List(Of FormatSpan))
			formattingList.Clear()

			Dim partEnd = partStart + partLength
			For i As Integer = usedFormattingCount To formatSpans_Conflict.Length - 1
				'INSTANT VB TODO TASK: 'ref locals' are not converted by Instant VB:
				'ORIGINAL LINE: ref readonly var formatting = ref formatSpans[i];
				Dim formatting = formatSpans_Conflict(i)
				If formatting.Start >= partEnd Then
					'no more formattings for this part
					Exit For
				End If

				Debug.Assert(previousFormattingCharsUsed < formatting.Length)

				If formatting.End <= partStart Then
					'formatting ended before this part
					previousFormattingCharsUsed = 0
					usedFormattingCount += 1
					Continue For
				End If

				Dim offset = -Math.Min(formatting.Start, partStart)
				Dim newFormatting As T
				Dim hasValue = formatting.Offset(offset).WithLength(formatting.Length - previousFormattingCharsUsed).Overlap(0, partLength).TryGet(newFormatting)

				Debug.Assert(hasValue, "formatting has to overlap due to prior conditions")
				formattingList.Add(newFormatting)

				If formatting.End <= partEnd + partSeparatorLength Then
					'formatting cannot affect next part
					previousFormattingCharsUsed = 0
					usedFormattingCount += 1
				Else
					previousFormattingCharsUsed += newFormatting.Length + partSeparatorLength
				End If
			Next i
		End Sub

		Public Function EnumerateTextElements() As TextElementsEnumerator
			Return New(TextOrEmpty, formatSpans_Conflict)
		End Function

		Private Sub CheckFormatSpans()
			Dim textLen = Length
			If textLen = 0 Then
				If formatSpans_Conflict.Length <> 0 Then
					Throw New ArgumentException("There is no text to be formatted.", NameOf(formatSpans_Conflict))
				End If
			Else
				Dim i As Integer = 0
				Do While i < formatSpans_Conflict.Length
					'INSTANT VB TODO TASK: 'ref locals' are not converted by Instant VB:
					'ORIGINAL LINE: ref readonly var span = ref formatSpans[i];
					Dim span = formatSpans_Conflict(i)
					If span.Start >= textLen Then
						Throw New ArgumentException("Span start cannot be larger than text length.", NameOf(formatSpans_Conflict))
					End If
					If span.Start + span.Length > textLen Then
						Throw New ArgumentException("Span end cannot be outside of text.", NameOf(formatSpans_Conflict))
					End If

					If i > 0 Then
						'INSTANT VB TODO TASK: 'ref locals' are not converted by Instant VB:
						'ORIGINAL LINE: ref readonly var previousSpan = ref formatSpans[i - 1];
						Dim previousSpan = formatSpans_Conflict(i - 1)
						If span.Start < previousSpan.End Then
							Throw New ArgumentException("Spans cannot overlap.", NameOf(formatSpans_Conflict))
						End If
					End If
					i += 1
				Loop
			End If
		End Sub

		Public Overloads Function Equals(ByVal other As FormattedString) As Boolean Implements IEquatable(Of FormattedString).Equals
			If Text <> other.Text Then
				Return False
			End If
			'INSTANT VB NOTE: The variable formatSpans was renamed since Visual Basic does not handle local variables named the same as class members well:
			Dim formatSpans_Conflict As ReadOnlySpan(Of FormatSpan) = FormatSpans
			Dim otherFormatSpans = other.FormatSpans
			If formatSpans_Conflict.Length <> otherFormatSpans.Length Then
				Return False
			End If
			For i As Integer = 0 To formatSpans_Conflict.Length - 1
				If Not formatSpans_Conflict(i).Equals(otherFormatSpans(i)) Then
					Return False
				End If
			Next i
			Return True
		End Function

		Public Overrides Function Equals(Object? ByVal obj As ) As Boolean
			Dim tempVar As Boolean = TypeOf obj Is FormattedString
			Dim other As FormattedString = If(tempVar, CType(obj, FormattedString), Nothing)
			Return tempVar AndAlso Equals(other)
		End Function
		Public Overrides Function GetHashCode() As Integer
			Return String.GetHashCode(Text)
		End Function
		Public Overrides String? Function ToString() As
		Return Text
		End Function

		Public Shared Operator =(ByVal left As FormattedString, ByVal right As FormattedString) As Boolean
			Return left.Equals(right)
		End Operator
		Public Shared Operator <>(ByVal left As FormattedString, ByVal right As FormattedString) As Boolean
			Return Not (left = right)
		End Operator

		'INSTANT VB WARNING: VB has no equivalent to the C# ref struct:
		'ORIGINAL LINE: public ref struct TextElementsEnumerator
		Public Structure TextElementsEnumerator
			Private elementsEnumerator As TextElementEnumeratorFast
			Private ReadOnly formatSpans() As FormatSpan
			Private textIndex As Integer
			Private formatIndex As Integer
			'INSTANT VB NOTE: The field current was renamed since Visual Basic does not allow fields to have the same name as other class members:
			Private current_Conflict As Result

			Public Sub New(ByVal text As String, ByVal formatSpans() As FormatSpan)
				elementsEnumerator = New TextElementEnumeratorFast(text)
				Me.formatSpans = formatSpans
				textIndex = 0
				formatIndex = 0
				current_Conflict = Nothing
			End Sub

			Public ReadOnly Property Current() As Result
				Get
					Return current_Conflict
				End Get
			End Property

			'INSTANT VB TODO TASK: 'ref return' methods are not converted by Instant VB:
			'		internal static ref readonly Result GetCurrentByRef(in TextElementsEnumerator enumerator)
			'		{
			'			Return ref enumerator.current;
			'		}

			Public Function MoveNext() As Boolean
				If Not elementsEnumerator.MoveNext() Then
					Return False
				End If

				Dim element = elementsEnumerator.Current

				'this method is hot so we need to be little bit hardcore
				Dim formatSpans = Me.formatSpans 'local copy to remove double bound checking
				'INSTANT VB TODO TASK: 'ref locals' are not converted by Instant VB:
				'ORIGINAL LINE: ref var span = ref(uint)formatIndex < (uint)formatSpans.Length ? ref formatSpans[formatIndex] : ref Unsafe.NullRef<FormatSpan>();
				Dim span = If(ref(Of UInteger)formatIndex < CUInt(formatSpans.Length), formatSpans(formatIndex), Unsafe.NullRef(Of FormatSpan)())

			If Not Unsafe.IsNullRef(span) Then
					If span.Contains(textIndex) Then
						current_Conflict.Formatting = span.Formatting 'write directly to current to avoid double copy
					Else
						current_Conflict.Formatting = ConsoleFormat.None 'write directly to current to avoid double copy
					End If
				Else
					current_Conflict.Formatting = ConsoleFormat.None 'write directly to current to avoid double copy
				End If

				current_Conflict.Element = element

				textIndex += element.Length
				If Not Unsafe.IsNullRef(span) AndAlso textIndex >= span.End Then
					formatIndex += 1
				End If

				Return True
			End Function

			Public Function GetEnumerator() As TextElementsEnumerator
				Return Me
			End Function

			'INSTANT VB WARNING: VB has no equivalent to the C# ref struct:
			'ORIGINAL LINE: public ref struct Result
			Public Structure Result
				Public Element As ReadOnlySpan(Of Char)
				Public Formatting As ConsoleFormat

				'INSTANT VB NOTE: The variable element was renamed since Visual Basic does not handle local variables named the same as class members well:
				'INSTANT VB NOTE: The variable formatting was renamed since Visual Basic does not handle local variables named the same as class members well:
				Public Sub New(ByVal element_Conflict As ReadOnlySpan(Of Char), ByVal formatting_Conflict As ConsoleFormat)
					Me.Element = element_Conflict
					Me.Formatting = formatting_Conflict
				End Sub

				'INSTANT VB NOTE: The variable element was renamed since Visual Basic does not handle local variables named the same as class members well:
				'INSTANT VB NOTE: The variable formatting was renamed since Visual Basic does not handle local variables named the same as class members well:
				Public Sub Deconstruct(<System.Runtime.InteropServices.Out()> ByRef element_Conflict As ReadOnlySpan(Of Char), <System.Runtime.InteropServices.Out()> ByRef formatting_Conflict As ConsoleFormat)
					element_Conflict = Me.Element
					formatting_Conflict = Me.Formatting
				End Sub
			End Structure

			Private Structure TextElementEnumeratorFast
				Private ReadOnly text As String
				Private i As Integer
				Private elementLength As Integer

				Public Sub New(ByVal text As String)
					Me.text = text
					i = 0
					elementLength = 0
				End Sub

				Public ReadOnly Property Current() As ReadOnlySpan(Of Char)
					Get
						Return text.AsSpan(i, elementLength)
					End Get
				End Property

				Public Function MoveNext() As Boolean
					i += elementLength
					If i < text.Length Then
						elementLength = StringInfo.GetNextTextElementLength(text, i)
						Return True
					End If
					Return False
				End Function
			End Structure
		End Structure
	End Structure

	Public Module TextElementsEnumeratorX
		'INSTANT VB TODO TASK: 'ref return' methods are not converted by Instant VB:
		'	public static ref readonly FormattedString.TextElementsEnumerator.Result GetCurrentByRef(in Me FormattedString.TextElementsEnumerator enumerator)
		'	{
		'		Return ref FormattedString.TextElementsEnumerator.GetCurrentByRef(in enumerator);
		'	}
	End Module
End Namespace