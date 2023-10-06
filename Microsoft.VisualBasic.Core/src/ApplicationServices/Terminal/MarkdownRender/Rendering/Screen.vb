#Region "License Header"
' This Source Code Form is subject to the terms of the Mozilla Public
' License, v. 2.0. If a copy of the MPL was not distributed with this
' file, You can obtain one at https://mozilla.org/MPL/2.0/.
#End Region

Imports System
Imports System.Diagnostics
Imports System.Linq
Imports PrettyPrompt.Consoles

Namespace ApplicationServices.Terminal

	''' <summary>
	''' Represents characters (TextElements) rendered on a screen.
	''' Used as part of <see cref="IncrementalRendering"/>.
	''' </summary>
	Friend NotInheritable Class Screen
		Implements IDisposable

		Private ReadOnly screenAreas() As ScreenArea

		Public ReadOnly Property Width() As Integer
		Public ReadOnly Property Height() As Integer
		Public ReadOnly Property Cursor() As ConsoleCoordinate
		'INSTANT VB WARNING: Nullable reference types have no equivalent in VB:
		'ORIGINAL LINE: public Cell?[] CellBuffer {get;}
		Public ReadOnly Property CellBuffer() As Cell()
		Public ReadOnly Property MaxIndex() As Integer
		Public ReadOnly Property ViewPortOffset() As Integer

		'INSTANT VB NOTE: The variable width was renamed since Visual Basic does not handle local variables named the same as class members well:
		'INSTANT VB NOTE: The variable height was renamed since Visual Basic does not handle local variables named the same as class members well:
		'INSTANT VB NOTE: The variable cursor was renamed since Visual Basic does not handle local variables named the same as class members well:
		Public Sub New(ByVal width_Conflict As Integer, ByVal height_Conflict As Integer, ByVal cursor_Conflict As ConsoleCoordinate, ParamArray ByVal screenAreas() As ScreenArea)
			Me.screenAreas = screenAreas
			Me.ViewPortOffset = screenAreas.Sum(Function(a) a.ViewPortStart)
			Me.Width = width_Conflict
			Me.Height = screenAreas.Select(Function(area) If(area.TruncateToScreenHeight, Math.Min(height_Conflict, area.Start.Row + area.Rows.Length), area.Start.Row + area.Rows.Length)).DefaultIfEmpty().Max()
			Me.CellBuffer = New Cell((Me.Width * Me.Height) - 1) {}
			Me.MaxIndex = FillCharBuffer(screenAreas)
			Me.Cursor = PositionCursor(Me, cursor_Conflict)
		End Sub

		Private Function FillCharBuffer(ByVal screenAreas() As ScreenArea) As Integer
			'INSTANT VB NOTE: The variable maxIndex was renamed since Visual Basic does not handle local variables named the same as class members well:
			Dim maxIndex_Conflict As Integer = 0
			For Each area In screenAreas
				Dim rowCountToRender As Integer = Math.Min(area.Rows.Length, Height - area.Start.Row)
				For i = 0 To rowCountToRender - 1
					Dim rowPosition = area.Start.Row + i
					Dim row = area.Rows(i)
					Dim position = rowPosition * Width + area.Start.Column
					Dim length = Math.Min(row.Length, CellBuffer.Length - position)
					If length > 0 Then
						For cellIndex As Integer = 0 To row.Length - 1
							row(cellIndex).TruncateToScreenHeight = area.TruncateToScreenHeight
						Next cellIndex
						row.CopyTo(CellBuffer, position, length)
						maxIndex_Conflict = Math.Max(maxIndex_Conflict, position + length)
					End If
				Next i
			Next area
			Return maxIndex_Conflict
		End Function

		''' <summary>
		''' We have our cursor coordinate, but its position represents the position in the input string.
		''' We need to reposition both the row/column of the cursor based on how they'll be rendered to
		''' the console:
		'''
		''' - For the row: we may only display a subset of the rows based on the current console size,
		'''   so we need to adjust the row position based on the viewport start.
		''' - For the column: repositioning is needed in the case where we've rendered CJK characters.
		'''   These are are "full width" characters and take up two characters on screen.
		'''
		''' </summary>
		'INSTANT VB NOTE: The variable cursor was renamed since Visual Basic does not handle local variables named the same as class members well:
		Private Function PositionCursor(ByVal screen As Screen, ByVal cursor_Conflict As ConsoleCoordinate) As ConsoleCoordinate
			If screen.CellBuffer.Length = 0 Then
				Return cursor_Conflict
			End If

			Dim row As Integer = Math.Min(cursor_Conflict.Row, screen.Height - 1)
			Dim column As Integer = Math.Min(cursor_Conflict.Column, screen.Width - 1)
			Dim rowStartIndex As Integer = row * screen.Width
			Dim rowCursorIndex As Integer = rowStartIndex + column
			Dim extraColumnOffset As Integer = 0
			Dim i As Integer = row * screen.Width
			Do While i <= rowCursorIndex + extraColumnOffset
				'INSTANT VB NOTE: The variable cell was renamed since it may cause conflicts with calls to static members of the user-defined type with this name:
				Dim cell_Conflict = screen.CellBuffer(i)
				If TypeOf cell_Conflict Is [not] Nothing AndAlso cell_Conflict.IsContinuationOfPreviousCharacter Then
				Debug.Assert(i > 0)
					Dim previousCell = screen.CellBuffer(i - 1)
					Debug.Assert(TypeOf previousCell?.Text Is [not] Nothing)
					Debug.Assert(previousCell.ElementWidth = 2)

					'e.g. for '界' is previousCell.ElementWidth==2 and previousCell.Text.Length==1
					'e.g. for '😀' is previousCell.ElementWidth==2 and previousCell.Text.Length==2 (which means cursor is already moved by 2 because of Text length)
					extraColumnOffset += previousCell.ElementWidth - previousCell.Text.Length
				End If
				i += 1
			Loop
			Dim newColumn As Integer = column + extraColumnOffset
			Dim newRow As Integer = cursor_Conflict.Row - ViewPortOffset

			Return If(newColumn > screen.Width, New ConsoleCoordinate(newRow + 1, newColumn - screen.Width), New ConsoleCoordinate(newRow, newColumn))
		End Function

		'INSTANT VB NOTE: The variable width was renamed since Visual Basic does not handle local variables named the same as class members well:
		'INSTANT VB NOTE: The variable height was renamed since Visual Basic does not handle local variables named the same as class members well:
		Public Function Resize(ByVal width_Conflict As Integer, ByVal height_Conflict As Integer) As Screen
			Return New(width_Conflict, height_Conflict, Cursor, screenAreas)
		End Function

		Public Sub Dispose() Implements IDisposable.Dispose
			For Each screenArea In screenAreas
				screenArea.Dispose()
			Next screenArea
		End Sub
	End Class
End Namespace