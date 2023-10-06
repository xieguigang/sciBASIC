#Region "License Header"
' This Source Code Form is subject to the terms of the Mozilla Public
' License, v. 2.0. If a copy of the MPL was not distributed with this
' file, You can obtain one at https://mozilla.org/MPL/2.0/.
#End Region

Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports PrettyPrompt.Consoles
Imports PrettyPrompt.Highlighting
Imports PrettyPrompt.Panes
Imports PrettyPrompt.Rendering
Imports System.ConsoleKey
Imports System.ConsoleModifiers
Imports PrettyPrompt.Consoles.AnsiEscapeCodes

Namespace ApplicationServices.Terminal

	''' <summary>
	''' Given our panes, actually draw them to the screen.
	''' This class mostly deals with generating Cells, which the <see cref="IncrementalRendering"/> class then processes
	''' to generate the minimal set of ANSI escape sequences to write to the screen.
	''' </summary>
	Friend Class Renderer
		Implements IDisposable

		Private ReadOnly console As IConsole
		'INSTANT VB NOTE: The variable boxDrawing was renamed since it may cause conflicts with calls to static members of the user-defined type with this name:
		Private ReadOnly boxDrawing_Conflict As BoxDrawing
		Private ReadOnly configuration As PromptConfiguration

		Private previouslyRenderedScreen As New Screen(0, 0, ConsoleCoordinate.Zero)
		Private wasTextSelectedDuringPreviousRender As Boolean

		Public Sub New(ByVal console As IConsole, ByVal configuration As PromptConfiguration)
			Me.console = console
			Me.boxDrawing_Conflict = New BoxDrawing(configuration)
			Me.configuration = configuration
		End Sub

		Public Sub RenderPrompt(ByVal codePane As CodePane)
			' write some newlines to ensure we have enough room to render the completion pane.
			Dim newLinesCount = codePane.EmptySpaceAtBottomOfWindowHeight
			console.Write(New String(ControlChars.Lf, newLinesCount) + GetMoveCursorUp(newLinesCount) + GetMoveCursorToColumn(1) + Reset)
			console.Write(configuration.Prompt)
		End Sub

		'INSTANT VB WARNING: Nullable reference types have no equivalent in VB:
		'ORIGINAL LINE: public void RenderOutput(PromptResult? result, CodePane codePane, OverloadPane overloadPane, CompletionPane completionPane, IReadOnlyCollection<FormatSpan> highlights, KeyPress key)
		'INSTANT VB NOTE: The parameter overloadPane was renamed since it may cause conflicts with calls to static members of the user-defined type with this name:
		Public Sub RenderOutput(ByVal result As PromptResult, ByVal codePane As CodePane, ByVal overloadPane_Conflict As OverloadPane, ByVal completionPane As CompletionPane, ByVal highlights As IReadOnlyCollection(Of FormatSpan), ByVal key As KeyPress)
			If TypeOf result Is [not] Nothing Then
			Dim redraw As Boolean = False
				If wasTextSelectedDuringPreviousRender AndAlso TypeOf codePane.Selection Is Nothing Then
					redraw = True
				End If

				If completionPane.IsOpen Then
					completionPane.IsOpen = False
					redraw = True
				End If

				'https://github.com/waf/PrettyPrompt/issues/239
				If overloadPane_Conflict.IsOpen Then
					overloadPane_Conflict.IsOpen = False
					redraw = True
				End If

				If redraw Then
					Redraw()
				End If

				console.Write(GetMoveCursorDown(codePane.WordWrappedLines.Count - codePane.Cursor.Row - 1) + GetMoveCursorToColumn(1) & vbLf & ClearToEndOfScreen, hideCursor:=True)
			Else
				If TypeOf key.ObjectPattern Is (Control, L) Then
					previouslyRenderedScreen = New Screen(0, 0, ConsoleCoordinate.Zero)
					console.Clear() ' for some reason, using escape codes (ClearEntireScreen and MoveCursorToPosition) leaves
					' CursorTop in an old (cached?) state. Using Console.Clear() works around this.
					RenderPrompt(codePane)
					codePane.MeasureConsole() ' our code pane will have more room to render, it now renders at the top of the screen.
				End If

				Redraw()
			End If

			wasTextSelectedDuringPreviousRender = codePane.Selection.HasValue

			'INSTANT VB TODO TASK: Local functions are not converted by Instant VB:
			'		void Redraw()
			'		{
			'			' convert our "view models" into characters, contained in screen areas
			'			var codeWidget = BuildCodeScreenArea(codePane, highlights);
			'			var completionWidgets = BuildCompletionScreenAreas(codePane, overloadPane, completionPane, codeAreaWidth: codePane.CodeAreaWidth);
			'
			'			' ansi escape sequence row/column values are 1-indexed.
			'			var ansiCoordinate = New ConsoleCoordinate(row: 1 + codePane.TopCoordinate, column: 1 + configuration.Prompt.Length);
			'
			'			' draw screen areas to screen representation.
			'			' later screen areas can overlap earlier screen areas.
			'			var screen = New Screen(codePane.CodeAreaWidth, codePane.CodeAreaHeight, codePane.Cursor, screenAreas: New[] { codeWidget }.Concat(completionWidgets).ToArray());
			'
			'			if (DidCodeAreaResize(previouslyRenderedScreen, screen))
			'			{
			'				previouslyRenderedScreen = previouslyRenderedScreen.Resize(screen.Width, screen.Height);
			'			}
			'
			'			' calculate the diff between the previous screen and the
			'			' screen to be drawn, and output that diff.
			'			IncrementalRendering.CalculateDiffAndWriteToConsole(screen, previouslyRenderedScreen, ansiCoordinate, console);
			'			previouslyRenderedScreen.Dispose();
			'			previouslyRenderedScreen = screen;
			'		}
		End Sub

		Private Shared Function DidCodeAreaResize(ByVal previousScreen As Screen, ByVal currentScreen As Screen) As Boolean
			Return previousScreen IsNot Nothing AndAlso previousScreen?.Width <> currentScreen.Width
		End Function

		Private Function BuildCodeScreenArea(ByVal codePane As CodePane, ByVal highlights As IReadOnlyCollection(Of FormatSpan)) As ScreenArea
			Dim highlightedLines = CellRenderer.ApplyColorToCharacters(highlights, codePane.WordWrappedLines, codePane.Selection, configuration.SelectedTextBackground)

			' if we've filled up the full line, add a new line at the end so we can render our cursor on this new line.
			If highlightedLines(^ 1).Length > 0 AndAlso (highlightedLines(^ 1).Length >= codePane.CodeAreaWidth OrElse highlightedLines(^ 1)(^ 1)?.Text = vbLf) Then
				Array.Resize(highlightedLines, highlightedLines.Length + 1)
				highlightedLines(^ 1) = New Row(0)
			End If

'INSTANT VB TODO TASK: VB has no equivalent to the C# deconstruction assignments:
		(highlightedLines, Integer bufferStart) = TrimLinesToViewPortSize(codePane, highlightedLines)

		Dim codeWidget = New ScreenArea(ConsoleCoordinate.Zero, highlightedLines, TruncateToScreenHeight:=False, ViewPortStart:=bufferStart)
			Return codeWidget
		End Function

		''' <summary>
		''' If there are too many lines of code to show in the current console window, return a subset
		''' of the lines that fit in the console window ("viewport") along with the index of the line
		''' where the viewport starts.
		''' The lines returned will always contain the line that contains the cursor.
		''' </summary>
		Private Function TrimLinesToViewPortSize(ByVal codePane As CodePane, ByVal highlightedLines() As Row) As (rowsInViewPort() As Row, viewPortStart As Integer)
			Const BlankBufferLines As Integer = 2
			If highlightedLines.Length <= console.WindowHeight - BlankBufferLines Then
				Return (highlightedLines, 0)
			End If

			Dim height As Integer = console.WindowHeight - BlankBufferLines
			Dim bufferStart As Integer = codePane.Cursor.Row - height \ 2
			Dim bufferEnd As Integer = bufferStart + height
			If bufferStart < 0 Then
				bufferStart = 0
				bufferEnd = bufferStart + height
			ElseIf bufferEnd > highlightedLines.Length Then
				bufferStart -= (bufferEnd - highlightedLines.Length)
				bufferEnd = highlightedLines.Length
			End If

			Return (highlightedLines(bufferStart..bufferEnd), bufferStart)
		End Function

		'INSTANT VB NOTE: The parameter overloadPane was renamed since it may cause conflicts with calls to static members of the user-defined type with this name:
		Private Function BuildCompletionScreenAreas(ByVal codePane As CodePane, ByVal overloadPane_Conflict As OverloadPane, ByVal completionPane As CompletionPane, ByVal codeAreaWidth As Integer) As ScreenArea()
			'  _  <-- cursor location
			'  ┌──────────────┬─────────────────────────────┐
			'  │ completion 1 │ documentation box with some |
			'  │ completion 2 │ docs that may wrap.         |
			'  │ completion 3 ├─────────────────────────────┘
			'  └──────────────┘

			Dim filteredView = completionPane.FilteredView
			Dim completionStart = codePane.GetHelperPanesStartPosition()
			Dim overloadArea As ScreenArea
			If overloadPane_Conflict.IsOpen Then
				overloadArea = BuildOverloadArea(overloadPane_Conflict, completionStart)
				completionStart = completionStart.Offset(overloadArea.Rows.Length - 1, 0)
			Else
				overloadArea = ScreenArea.Empty
			End If

			If Not completionPane.IsOpen OrElse filteredView.IsEmpty Then
				If overloadPane_Conflict.IsOpen Then
					Return {overloadArea}
				Else
					Return Array.Empty(Of ScreenArea)()
				End If
			End If

			Dim completionArea = BuildCompletionArea(completionPane, codeAreaWidth, completionStart)

			Dim documentationStart = New ConsoleCoordinate(completionStart.Row, completionStart.Column + completionArea.Width - 1)
			Dim documentationArea = BuildDocumentationArea(completionPane, documentationStart)

			boxDrawing_Conflict.Connect(overloadArea.Rows, completionArea.Rows, documentationArea.Rows)

			Return {overloadArea, completionArea, documentationArea}
		End Function

		'INSTANT VB NOTE: The parameter overloadPane was renamed since it may cause conflicts with calls to static members of the user-defined type with this name:
		Private Function BuildOverloadArea(ByVal overloadPane_Conflict As OverloadPane, ByVal position As ConsoleCoordinate) As ScreenArea
			Dim rows = boxDrawing_Conflict.BuildFromLines(overloadPane_Conflict.SelectedItem, configuration:=configuration, background:=configuration.CompletionItemDescriptionPaneBackground)
			Return New ScreenArea(position, rows)
		End Function

		Private Function BuildCompletionArea(ByVal completionPane As CompletionPane, ByVal codeAreaWidth As Integer, ByVal position As ConsoleCoordinate) As ScreenArea
			Dim rows = boxDrawing_Conflict.BuildFromItemList(items:=completionPane.FilteredView.VisibleItems.Select(Function(c) c.DisplayTextFormatted), configuration:=configuration, maxWidth:=codeAreaWidth - position.Column, selectedLineIndex:=completionPane.FilteredView.SelectedIndexInVisibleItems)
			Return New ScreenArea(position, rows)
		End Function

		Private Function BuildDocumentationArea(ByVal completionPane As CompletionPane, ByVal position As ConsoleCoordinate) As ScreenArea
			Dim documentation = completionPane.SelectedItemDocumentation
			If documentation.Count = 0 Then
				Return ScreenArea.Empty
			End If

			Dim rows = boxDrawing_Conflict.BuildFromLines(documentation, configuration:=configuration, background:=configuration.CompletionItemDescriptionPaneBackground)
			Return New ScreenArea(position, rows)
		End Function

		Public Sub Dispose() Implements IDisposable.Dispose
			previouslyRenderedScreen?.Dispose()
		End Sub
	End Class
End Namespace