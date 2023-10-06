#Region "License Header"
' This Source Code Form is subject to the terms of the Mozilla Public
' License, v. 2.0. If a copy of the MPL was not distributed with this
' file, You can obtain one at https://mozilla.org/MPL/2.0/.
#End Region

Imports System
Imports PrettyPrompt.Consoles

Namespace ApplicationServices.Terminal

	''' <summary>
	''' An area of the screen that's being rendered at a coordinate.
	''' This is conceptually a UI pane, rasterized into characters.
	''' </summary>
	Friend Structure ScreenArea
		Public Shared ReadOnly Empty As ScreenArea = New ScreenArea(ConsoleCoordinate.Zero, Array.Empty(Of Row)())

		'INSTANT VB TODO TASK: Local functions are not converted by Instant VB:
		Public ReadOnly Property Width As Integer
			Get
				Return If(Rows.Length > 0, Rows(0).Length, 0)
			End Get
		End Property
		'	{
		'		get
		'		{
		'			
		'		}
		'	}


		Dim Start As ConsoleCoordinate
		Dim Rows() As Row
		Dim TruncateToScreenHeight As Boolean
		Dim ViewPortStart As Integer

		Sub New(ByVal Start As ConsoleCoordinate, ByVal Rows() As Row, Optional ByVal TruncateToScreenHeight As Boolean = True, Optional ByVal ViewPortStart As Integer = 0)
			Me.Start = Start
			Me.Rows = Rows
			Me.TruncateToScreenHeight = TruncateToScreenHeight
			Me.ViewPortStart = ViewPortStart
		End Sub

		Public Sub Dispose()
			For Each row In Rows
				row.Dispose()
			Next

			Erase Rows
		End Sub
	End Structure
End Namespace