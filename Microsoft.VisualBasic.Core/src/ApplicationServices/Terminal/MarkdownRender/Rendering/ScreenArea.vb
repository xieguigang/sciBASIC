#Region "License Header"
' This Source Code Form is subject to the terms of the Mozilla Public
' License, v. 2.0. If a copy of the MPL was not distributed with this
' file, You can obtain one at https://mozilla.org/MPL/2.0/.
#End Region

Imports System
Imports PrettyPrompt.Consoles

Private PrettyPrompt As namespace

''' <summary>
''' An area of the screen that's being rendered at a coordinate.
''' This is conceptually a UI pane, rasterized into characters.
''' </summary>
Friend NotOverridable Function ScreenArea(ByVal Start As ConsoleCoordinate, ByVal Rows() As Row, Optional ByVal TruncateToScreenHeight As Boolean = True, Optional ByVal ViewPortStart As Integer = 0) As record
	public static readonly AddressOf ScreenArea Empty = New(ConsoleCoordinate.Zero, Array.Empty(Of Row)())

'INSTANT VB TODO TASK: Local functions are not converted by Instant VB:
'	public int Width
'	{
'		get
'		{
'			Return Rows.Length > 0 ? Rows[0].Length : 0;
'		}
'	}

'INSTANT VB TODO TASK: Local functions are not converted by Instant VB:
'	public void Dispose()
'	{
'		foreach (var row in Rows)
'		{
'			row.Dispose();
'		}
'#If DEBUG Then
'		Array.Clear(Rows, 0, Rows.Length);
'#End If
'	}
End Function