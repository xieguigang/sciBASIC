#Region "License Header"
' This Source Code Form is subject to the terms of the Mozilla Public
' License, v. 2.0. If a copy of the MPL was not distributed with this
' file, You can obtain one at https://mozilla.org/MPL/2.0/.
#End Region

Imports System
Imports System.Collections.Generic
Imports System.Text
Imports PrettyPrompt.Consoles
Imports PrettyPrompt.Documents
Imports PrettyPrompt.TextSelection

Namespace ApplicationServices.Terminal

	''' <summary>
	''' Given the text and the syntax highlighting information, render the text into the "cells" of the terminal screen.
	''' </summary>
	Friend Module CellRenderer
		Public Function ApplyColorToCharacters(ByVal highlights As IReadOnlyCollection(Of FormatSpan), ByVal lines As IReadOnlyList(Of WrappedLine), ByVal selection? As SelectionSpan, ByVal selectedTextBackground? As AnsiColor) As Row()
			Dim selectionStart = New ConsoleCoordinate(Integer.MaxValue, Integer.MaxValue) 'invalid
			Dim selectionEnd = New ConsoleCoordinate(Integer.MaxValue, Integer.MaxValue) 'invalid
			Dim selectionValue As T
			If selection.TryGet(selectionValue) Then
				selectionStart = selectionValue.Start
				selectionEnd = selectionValue.End
			End If

			Dim selectionHighlight As Boolean = False

			Dim highlightsLookup = HighlightsGroupingPool.Shared.Get(highlights)
			Dim highlightedRows = New Row(lines.Count - 1) {}
			Dim currentHighlight? As FormatSpan = Nothing
			For lineIndex As Integer = 0 To lines.Count - 1
				Dim line As WrappedLine = lines(lineIndex)
				Dim lineFullWidthCharacterOffset As Integer = 0
				Dim row As New Row(line.Content)
				Dim cellIndex As Integer = 0
				Do While cellIndex < row.Length
					'INSTANT VB NOTE: The variable cell was renamed since it may cause conflicts with calls to static members of the user-defined type with this name:
					Dim cell_Conflict = row(cellIndex)
					If cell_Conflict.IsContinuationOfPreviousCharacter Then
						lineFullWidthCharacterOffset += 1
					End If

					' syntax highlight wrapped lines
					Dim previousLineHighlight As T
					If currentHighlight.TryGet(previousLineHighlight) AndAlso cellIndex = 0 Then
						currentHighlight = HighlightSpan(previousLineHighlight, row, cellIndex, previousLineHighlight.Start - line.StartIndex)
					End If

					' get current syntaxt highlight start
					Dim characterPosition As Integer = line.StartIndex + cellIndex - lineFullWidthCharacterOffset
					Dim lookupHighlight As FormatSpan
					currentHighlight = If(If(currentHighlight, highlightsLookup.TryGetValue(characterPosition, lookupHighlight)), lookupHighlight, Nothing)

					' syntax highlight based on start
					Dim highlight As T
					If currentHighlight.TryGet(highlight) AndAlso highlight.Contains(characterPosition) Then
						currentHighlight = HighlightSpan(highlight, row, cellIndex, cellIndex)
					End If

					' if there's text selected, invert colors to represent the highlight of the selected text.
					If selectionStart.Equals(lineIndex, cellIndex - lineFullWidthCharacterOffset) Then 'start is inclusive
						selectionHighlight = True
					End If
					If selectionEnd.Equals(lineIndex, cellIndex - lineFullWidthCharacterOffset) Then 'end is exclusive
						selectionHighlight = False
					End If
					If selectionHighlight Then
						Dim background As T
						If selectedTextBackground.TryGet(background) Then
							cell_Conflict.TransformBackground(background)
						Else
							cell_Conflict.Formatting = New ConsoleFormat With {.Inverted = True}
						End If
					End If
					cellIndex += 1
				Loop
				highlightedRows(lineIndex) = row
			Next lineIndex
			Return highlightedRows
		End Function

		Private Function HighlightSpan(ByVal currentHighlight As FormatSpan, ByVal row As Row, ByVal cellIndex As Integer, ByVal endPosition As Integer) As FormatSpan?
			Dim highlightedFullWidthOffset = 0
			Dim i As Integer
			For i = cellIndex To Math.Min(endPosition + currentHighlight.Length + highlightedFullWidthOffset, row.Length) - 1
				highlightedFullWidthOffset += row(i).ElementWidth - 1
				row(i).Formatting = currentHighlight.Formatting
			Next i
			If i <> row.Length Then
				Return Nothing
			End If

			Return currentHighlight
		End Function

		''' <summary>
		''' This is just an extra function used by <see cref="Prompt.RenderAnsiOutput"/> that highlights arbitrary text. It's
		''' not used for drawing input during normal functioning of the prompt.
		''' </summary>
		Public Function ApplyColorToCharacters(ByVal highlights As IReadOnlyCollection(Of FormatSpan), ByVal text As String, ByVal textWidth As Integer) As Row()
			Dim wrapped = WordWrapping.WrapEditableCharacters(New StringBuilder(text), 0, textWidth)
			Return ApplyColorToCharacters(highlights, wrapped.WrappedLines, selection:=Nothing, selectedTextBackground:=Nothing)
		End Function

		Private NotInheritable Class HighlightsGroupingPool
			Private ReadOnly pool As New Stack(Of Dictionary(Of Integer, FormatSpan))()

			Public Shared ReadOnly [Shared] As New HighlightsGroupingPool()

			Public Function [Get](ByVal highlights As IReadOnlyCollection(Of FormatSpan)) As Dictionary(Of Integer, FormatSpan)
				'INSTANT VB WARNING: Nullable reference types have no equivalent in VB:
				'ORIGINAL LINE: Dictionary<int, FormatSpan>? result = null;
				Dim result As Dictionary(Of Integer, FormatSpan) = Nothing
				SyncLock pool
					If pool.Count > 0 Then
						result = pool.Pop()
					End If
				End SyncLock
				If TypeOf result Is Nothing Then
					result = New Dictionary(Of Integer, FormatSpan)(highlights.Count)
				Else
					result.EnsureCapacity(highlights.Count)
				End If

				For Each highlight In highlights
					Dim formatSpan As FormatSpan
					If result.TryGetValue(highlight.Start, formatSpan) Then
						If highlight.Length > formatSpan.Length Then
							result(highlight.Start) = highlight
						End If
					Else
						result.Add(highlight.Start, highlight)
					End If
				Next highlight

				Return result
			End Function

			Public Sub Put(ByVal list As Dictionary(Of Integer, FormatSpan))
				list.Clear()
				SyncLock pool
					pool.Push(list)
				End SyncLock
			End Sub
		End Class
	End Module
End Namespace