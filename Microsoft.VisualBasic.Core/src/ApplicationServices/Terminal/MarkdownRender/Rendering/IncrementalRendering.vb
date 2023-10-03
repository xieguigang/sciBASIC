#Region "License Header"
' This Source Code Form is subject to the terms of the Mozilla Public
' License, v. 2.0. If a copy of the MPL was not distributed with this
' file, You can obtain one at https://mozilla.org/MPL/2.0/.
#End Region

Imports Microsoft.VisualBasic
Imports System
Imports System.Text
Imports PrettyPrompt.Consoles
Imports PrettyPrompt.Highlighting
Imports PrettyPrompt.Consoles.AnsiEscapeCodes

Private PrettyPrompt As namespace

Friend Module IncrementalRendering
	''' <summary>
	''' Given a new screen and the previously rendered screen,
	''' returns the minimum required ansi escape sequences to
	''' render the new screen.
	''' 
	''' In the simple case, where the user typed a single character, we should only return that character (e.g. the returned string will be of length 1).
	''' A more complicated case, like finishing a word that triggers syntax highlighting, we should redraw just that word in the new color.
	''' An even more complicated case, like opening the autocomplete menu, should draw the autocomplete menu, and return the cursor to the correct position.
	''' </summary>
	Public Sub CalculateDiffAndWriteToConsole(ByVal currentScreen As Screen, ByVal previousScreen As Screen, ByVal ansiCoordinate As ConsoleCoordinate, ByVal console As IConsole)
		Dim diff = CalculateDiffInternal(currentScreen, previousScreen, ansiCoordinate)
		Dim hideCursor = diff.Length > 64 'rough heuristic
		console.Write(diff, hideCursor)
		StringBuilderPool.Shared.Put(diff)
	End Sub

	''' <summary>
	''' Given a new screen and the previously rendered screen,
	''' returns the minimum required ansi escape sequences to
	''' render the new screen.
	''' 
	''' In the simple case, where the user typed a single character, we should only return that character (e.g. the returned string will be of length 1).
	''' A more complicated case, like finishing a word that triggers syntax highlighting, we should redraw just that word in the new color.
	''' An even more complicated case, like opening the autocomplete menu, should draw the autocomplete menu, and return the cursor to the correct position.
	''' 
	''' This method needs to allocate string for result. If you want to just write result to console use <see cref="CalculateDiffAndWriteToConsole(Screen, Screen, ConsoleCoordinate, IConsole)"/> instead.
	''' </summary>
	Public Function CalculateDiff(ByVal currentScreen As Screen, ByVal previousScreen As Screen, ByVal ansiCoordinate As ConsoleCoordinate) As String
		Dim diff = CalculateDiffInternal(currentScreen, previousScreen, ansiCoordinate)
		Dim result = diff.ToString()
		StringBuilderPool.Shared.Put(diff)
		Return result
	End Function

	Private Function CalculateDiffInternal(ByVal currentScreen As Screen, ByVal previousScreen As Screen, ByVal ansiCoordinate As ConsoleCoordinate) As StringBuilder
		' if there are multiple characters with the same formatting, don't output formatting
		' instructions per character; instead output one instruction at the beginning for all
		' characters that share the same formatting.
		Dim currentFormatRun = ConsoleFormat.None
		Dim previousCoordinate = New ConsoleCoordinate(row:= ansiCoordinate.Row + previousScreen.Cursor.Row, column:= ansiCoordinate.Column + previousScreen.Cursor.Column)

		Dim currentBuffer = currentScreen.CellBuffer
		Dim previousBuffer = previousScreen.CellBuffer
		Dim maxIndex = Math.Max(currentBuffer.Length, previousBuffer.Length)
		Dim diff = StringBuilderPool.Shared.Get(maxIndex)
		For i As Integer = 0 To maxIndex - 1
			Dim currentCell = If(i < currentBuffer.Length, currentBuffer(i), Nothing)
			If TypeOf currentCell Is [not] Nothing AndAlso currentCell.IsContinuationOfPreviousCharacter Then
				Continue For
			End If

			Dim previousCell = If(i < previousBuffer.Length, previousBuffer(i), Nothing)
			If Cell.Equals(currentCell, previousCell) Then
				Continue For
			End If

'INSTANT VB TODO TASK: VB has no equivalent to C# deconstruction declarations:
			var(rowOffset, columnOffset) = Math.DivRem(i, currentScreen.Width)
			Dim cellCoordinate = ansiCoordinate.Offset(rowOffset, columnOffset)

			MoveCursorIfRequired(diff, previousCoordinate, cellCoordinate)
			previousCoordinate = cellCoordinate

			' handle when we're erasing characters/formatting from the previously rendered screen.
			If TypeOf currentCell Is Nothing OrElse currentCell.Formatting.IsDefault Then
				If Not currentFormatRun.IsDefault Then
					diff.Append(Reset)
					currentFormatRun = ConsoleFormat.None
				End If

				If TypeOf currentCell?.Text Is Nothing OrElse currentCell.Text = vbLf Then
					diff.Append(" "c)
					UpdateCoordinateFromCursorMove(previousScreen, ansiCoordinate, diff, previousCoordinate, currentCell)

					If TypeOf currentCell Is Nothing Then
						Continue For
					End If
				End If
			End If

			' write out current character, with any formatting
			If Not currentCell.Formatting.Equals(in currentFormatRun) Then
				' text selection is implemented by inverting colors. Reset inverted colors if required.
				If Not currentFormatRun.IsDefault AndAlso currentCell.Formatting.Inverted <> currentFormatRun.Inverted Then
					diff.Append(Reset)
				End If
				AppendAnsiEscapeSequence(diff, currentCell.Formatting)
				diff.Append(currentCell.Text)
				currentFormatRun = currentCell.Formatting
			Else
				diff.Append(currentCell.Text)
			End If

			' writing to the console will automatically move the cursor.
			' update our internal tracking so we calculate the least
			' amount of movement required for the next character.
			If currentCell.Text = vbLf Then
				UpdateCoordinateFromNewLine(previousCoordinate)
			Else
				UpdateCoordinateFromCursorMove(currentScreen, ansiCoordinate, diff, previousCoordinate, currentCell)
			End If
		Next i

		If Not currentFormatRun.IsDefault Then
			diff.Append(Reset)
		End If

		' all done rendering, update the cursor position if we need to. If we rendered the
		' autocomplete menu, or if the cursor is manually positioned in the middle of
		' the text, the cursor won't be in the correct position.
		MoveCursorIfRequired(diff, fromCoordinate:= previousCoordinate, toCoordinate:= New ConsoleCoordinate(currentScreen.Cursor.Row + ansiCoordinate.Row, currentScreen.Cursor.Column + ansiCoordinate.Column))

		Return diff
	End Function

'INSTANT VB WARNING: Nullable reference types have no equivalent in VB:
'ORIGINAL LINE: private static void UpdateCoordinateFromCursorMove(Screen currentScreen, ConsoleCoordinate ansiCoordinate, StringBuilder diff, ref ConsoleCoordinate previousCoordinate, Cell? currentCell)
	Private Sub UpdateCoordinateFromCursorMove(ByVal currentScreen As Screen, ByVal ansiCoordinate As ConsoleCoordinate, ByVal diff As StringBuilder, ByRef previousCoordinate As ConsoleCoordinate, ByVal currentCell As Cell)
		Dim characterWidth = If(TypeOf currentCell Is Nothing, 1, currentCell.ElementWidth)
		' if we hit the edge of the screen, wrap
		Dim hitRightEdgeOfScreen As Boolean = previousCoordinate.Column + characterWidth = currentScreen.Width + ansiCoordinate.Column
		If hitRightEdgeOfScreen Then
			If TypeOf currentCell Is [not] Nothing AndAlso Not currentCell.TruncateToScreenHeight Then
				diff.Append(ControlChars.Lf)
				UpdateCoordinateFromNewLine(previousCoordinate)
				If characterWidth = 2 Then
					previousCoordinate = previousCoordinate.MoveRight()
				End If
			End If
		Else
			previousCoordinate = previousCoordinate.MoveRight()
			If characterWidth = 2 Then
				previousCoordinate = previousCoordinate.MoveRight()
			End If
		End If
	End Sub

	Private Sub UpdateCoordinateFromNewLine(ByRef previousCoordinate As ConsoleCoordinate)
		' for simplicity, we standardize all newlines to "\n" regardless of platform. However, that complicates our diff,
		' because "\n" on windows _only_ moves one line down, it does not change the column. Handle that here.
		previousCoordinate = previousCoordinate.MoveDown()
		If Not OperatingSystem.IsWindows() Then
			previousCoordinate = previousCoordinate.WithColumn(1)
		End If
	End Sub

	Private Sub MoveCursorIfRequired(ByVal diff As StringBuilder, ByVal fromCoordinate As ConsoleCoordinate, ByVal toCoordinate As ConsoleCoordinate)
		' we only ever move the cursor relative to its current position.
		' this is because ansi escape sequences know nothing about the current scroll in the window,
		' they only operate on the current viewport. If we move to absolute positions, the display
		' is garbled if the user scrolls the window and then types.

		If fromCoordinate.Row <> toCoordinate.Row Then
			If fromCoordinate.Row < toCoordinate.Row Then
				AppendMoveCursorDown(diff, toCoordinate.Row - fromCoordinate.Row)
			Else
				AppendMoveCursorUp(diff, fromCoordinate.Row - toCoordinate.Row)
			End If
		End If
		If fromCoordinate.Column <> toCoordinate.Column Then
			If fromCoordinate.Column < toCoordinate.Column Then
				AppendMoveCursorRight(diff, toCoordinate.Column - fromCoordinate.Column)
			Else
				AppendMoveCursorLeft(diff, fromCoordinate.Column - toCoordinate.Column)
			End If
		End If
	End Sub
End Module
