#Region "License Header"
' This Source Code Form is subject to the terms of the Mozilla Public
' License, v. 2.0. If a copy of the MPL was not distributed with this
' file, You can obtain one at https://mozilla.org/MPL/2.0/.
#End Region

Imports Microsoft.VisualBasic
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Linq
Imports PrettyPrompt.Highlighting
Namespace ApplicationServices.Terminal


	''' <summary>
	''' Represents a single cell in the console, with any associate formatting.
	'''
	''' https://en.wikipedia.org/wiki/Halfwidth_and_fullwidth_forms
	''' A character can be full-width (e.g. CJK: Chinese, Japanese, Korean) in
	''' which case it will take up two characters on the console, so we represent
	''' it as two consecutive cells. The first cell will have <see cref="ElementWidth"/> of 2.
	''' the trailing cell will have <see cref="IsContinuationOfPreviousCharacter"/> set to true.
	''' </summary>
	'
	' Do not change to struct without benchmarking. With some work it's possible, but I tried and performace was much worse.
	' This because we are making copies of lists of cells and they are smaller when they are reference types.
	' Pooling of cells is currently better.
	Friend NotInheritable Class Cell
		Public Shared ReadOnly SharedPool As New Pool()

		'INSTANT VB WARNING: Nullable reference types have no equivalent in VB:
		'ORIGINAL LINE: private string? text;
		'INSTANT VB NOTE: The field text was renamed since Visual Basic does not allow fields to have the same name as other class members:
		Private text_Conflict As String
		'INSTANT VB NOTE: The field isContinuationOfPreviousCharacter was renamed since Visual Basic does not allow fields to have the same name as other class members:
		Private isContinuationOfPreviousCharacter_Conflict As Boolean
		'INSTANT VB NOTE: The field elementWidth was renamed since Visual Basic does not allow fields to have the same name as other class members:
		Private elementWidth_Conflict As Integer

		Public ReadOnly Property Text() As String
			Get
				Return text_Conflict
			End Get
		End Property
		Public ReadOnly Property IsContinuationOfPreviousCharacter() As Boolean
			Get
				Return isContinuationOfPreviousCharacter_Conflict
			End Get
		End Property
		Public ReadOnly Property ElementWidth() As Integer
			Get
				Return elementWidth_Conflict
			End Get
		End Property

		Public Formatting As ConsoleFormat
		Public TruncateToScreenHeight As Boolean

		Private isPoolable As Boolean

		Private Sub New(ByVal isPoolable As Boolean)
			Me.isPoolable = isPoolable
		End Sub

		'INSTANT VB WARNING: Nullable reference types have no equivalent in VB:
		'ORIGINAL LINE: private void Initialize(string? text, in ConsoleFormat formatting, int elementWidth, bool isContinuationOfPreviousCharacter)
		'INSTANT VB NOTE: The variable text was renamed since Visual Basic does not handle local variables named the same as class members well:
		'INSTANT VB WARNING: VB has no equivalent to C# 'in' parameters, so they will convert the same as by value parameters:
		'INSTANT VB NOTE: The variable formatting was renamed since Visual Basic does not handle local variables named the same as class members well:
		'INSTANT VB NOTE: The variable elementWidth was renamed since Visual Basic does not handle local variables named the same as class members well:
		'INSTANT VB NOTE: The variable isContinuationOfPreviousCharacter was renamed since Visual Basic does not handle local variables named the same as class members well:
		Private Sub Initialize(ByVal text_Conflict As String, ByVal formatting_Conflict As ConsoleFormat, ByVal elementWidth_Conflict As Integer, ByVal isContinuationOfPreviousCharacter_Conflict As Boolean)
			Me.text_Conflict = text_Conflict
			Me.Formatting = formatting_Conflict

			' full-width handling properties
			Me.isContinuationOfPreviousCharacter_Conflict = isContinuationOfPreviousCharacter_Conflict
			Me.elementWidth_Conflict = elementWidth_Conflict
		End Sub

		'INSTANT VB NOTE: The parameter formattedString was renamed since it may cause conflicts with calls to static members of the user-defined type with this name:
		Public Shared Sub AddTo(ByVal cells As List(Of Cell), ByVal formattedString_Conflict As FormattedString)
			' note, this method is fairly hot, please profile when making changes to it.
			' don't use: foreach (var (element, formatting) in formattedString.EnumerateTextElements())
			'            manual enumeration and using by-ref values is faster
			Dim enumerator = formattedString_Conflict.EnumerateTextElements()
			Do While enumerator.MoveNext()
				'INSTANT VB TODO TASK: 'ref locals' are not converted by Instant VB:
				'ORIGINAL LINE: ref readonly var elem = ref enumerator.GetCurrentByRef();
				Dim elem = enumerator.GetCurrentByRef()
				Dim elementWidth As Object
				Dim elementText = StringCache.Shared.Get(elem.Element, elementWidth_Conflict)
				cells.Add(SharedPool.Get(elementText, elem.Formatting, elementWidth_Conflict))
				For i As Integer = 1 To elementWidth_Conflict - 1
					cells.Add(SharedPool.Get(Nothing, elem.Formatting, isContinuationOfPreviousCharacter_Conflict:=True))
				Next i
			Loop

			Debug.Assert(cells.AsEnumerable().Count(Function(c) c.text = vbLf) <= 1) 'otherwise it should be splitted into multiple rows
		End Sub

		'INSTANT VB WARNING: VB has no equivalent to C# 'in' parameters, so they will convert the same as by value parameters:
		'ORIGINAL LINE: public static Cell CreateSingleNonpoolableCell(char character, in ConsoleFormat formatting)
		'INSTANT VB NOTE: The variable formatting was renamed since Visual Basic does not handle local variables named the same as class members well:
		Public Shared Function CreateSingleNonpoolableCell(ByVal character As Char, ByVal formatting_Conflict As ConsoleFormat) As Cell
			Dim list = ListPool(Of Cell).Shared.Get(1)
			AddTo(list, New FormattedString(character.ToString(), formatting_Conflict))
			'INSTANT VB NOTE: The variable cell was renamed since it may cause conflicts with calls to static members of the user-defined type with this name:
			Dim cell_Conflict = list.Single()
			ListPool(Of Cell).Shared.Put(list)
			cell_Conflict.isPoolable = False
			Return cell_Conflict
		End Function

		'INSTANT VB WARNING: Nullable reference types have no equivalent in VB:
		'ORIGINAL LINE: public static bool Equals(Cell? left, Cell? right)
		Public Shared Function Equals(ByVal left As Cell, ByVal right As Cell) As Boolean
			'this is hot from IncrementalRendering.CalculateDiff, so we want to use custom optimized Equals
			If Not ReferenceEquals(left, right) Then
				If left IsNot Nothing Then
					Return left.Equals(right)
				End If
				Return False
			End If
			Return True
		End Function

		'INSTANT VB WARNING: Nullable reference types have no equivalent in VB:
		'ORIGINAL LINE: public bool Equals(Cell? other)
		Public Function Equals(ByVal other As Cell) As Boolean
			'this is hot from IncrementalRendering.CalculateDiff, so we want to use custom optimized Equals
			Return other IsNot Nothing AndAlso text_Conflict = other.text_Conflict AndAlso isContinuationOfPreviousCharacter_Conflict = other.isContinuationOfPreviousCharacter_Conflict AndAlso Formatting.Equals(other.Formatting) AndAlso TruncateToScreenHeight = other.TruncateToScreenHeight
		End Function

		Private Function GetDebuggerDisplay() As String
			Return text_Conflict & " " & Formatting.ToString()
		End Function

		Friend Class Pool
			Private ReadOnly pool As New Stack(Of Cell)()

			'INSTANT VB WARNING: Nullable reference types have no equivalent in VB:
			'ORIGINAL LINE: public Cell @Get(string? text, in ConsoleFormat formatting, int elementWidth = 1, bool isContinuationOfPreviousCharacter = false)
			'INSTANT VB WARNING: VB has no equivalent to C# 'in' parameters, so they will convert the same as by value parameters:
			Public Function [Get](ByVal text As String, ByVal formatting As ConsoleFormat, Optional ByVal elementWidth As Integer = 1, Optional ByVal isContinuationOfPreviousCharacter As Boolean = False) As Cell
				'INSTANT VB WARNING: Nullable reference types have no equivalent in VB:
				'ORIGINAL LINE: Cell? result = null;
				Dim result As Cell = Nothing
				SyncLock pool
					If pool.Count > 0 Then
						result = pool.Pop()
					End If
				End SyncLock
				result = If(result, New Cell(isPoolable:=True))
				result.Initialize(text, formatting, elementWidth, isContinuationOfPreviousCharacter)
				Return result
			End Function

			Public Sub Put(ByVal value As Cell)
				If value.isPoolable Then
					SyncLock pool
						pool.Push(value)
					End SyncLock
				End If
			End Sub
		End Class
	End Class
End Namespace