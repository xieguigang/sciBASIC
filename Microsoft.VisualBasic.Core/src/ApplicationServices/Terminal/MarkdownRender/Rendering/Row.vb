#Region "License Header"
' This Source Code Form is subject to the terms of the Mozilla Public
' License, v. 2.0. If a copy of the MPL was not distributed with this
' file, You can obtain one at https://mozilla.org/MPL/2.0/.
#End Region

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Linq
Imports PrettyPrompt.Highlighting

Private PrettyPrompt As namespace

''' <summary>
''' A row of cells. Just here for the readability of method signatures.
''' </summary>
<DebuggerDisplay("Row: {" & NameOf(GetDebuggerDisplay) & "()}")>
Friend Class Row
	Implements IDisposable

	Private ReadOnly cells As List(Of Cell)
	Private disposed As Boolean

	Public ReadOnly Property Length() As Integer
		Get
			Return cells.Count
		End Get
	End Property
	Default Public ReadOnly Property Item(ByVal index As Integer) As Cell
		Get
			Return cells(index)
		End Get
	End Property

	Public Sub New(ByVal capacity As Integer)
		cells = ListPool(Of Cell).Shared.Get(capacity)
	End Sub

	Public Sub New(ByVal text As Char, ByVal formatting As ConsoleFormat)
		Me.New(New FormattedString(text.ToString(), formatting))
	End Sub

	Public Sub New(ByVal text As String)
		Me.New(New FormattedString(text))
	End Sub

	Public Sub New(ByVal text As String, ByVal formatting As ConsoleFormat)
		Me.New(New FormattedString(text, formatting))
	End Sub

'INSTANT VB NOTE: The parameter formattedString was renamed since it may cause conflicts with calls to static members of the user-defined type with this name:
	Public Sub New(ByVal formattedString_Conflict As FormattedString)
		Me.New(capacity:= 6 * formattedString_Conflict.Length \ 5)
		Cell.AddTo(cells, formattedString_Conflict)
	End Sub

	Public Sub Dispose() Implements IDisposable.Dispose
		If Not disposed Then
'INSTANT VB NOTE: The variable cell was renamed since it may cause conflicts with calls to static members of the user-defined type with this name:
			For Each cell_Conflict In cells
				Cell.SharedPool.Put(cell_Conflict)
			Next cell_Conflict
			ListPool(Of Cell).Shared.Put(cells)
			disposed = True
		End If
	End Sub

	Public Sub Add(ByVal text As String)
		Add(New FormattedString(text))
	End Sub

'INSTANT VB WARNING: VB has no equivalent to C# 'in' parameters, so they will convert the same as by value parameters:
'ORIGINAL LINE: public void Add(string text, in ConsoleFormat formatting)
	Public Sub Add(ByVal text As String, ByVal formatting As ConsoleFormat)
		Add(New FormattedString(text, formatting))
	End Sub

'INSTANT VB NOTE: The parameter formattedString was renamed since it may cause conflicts with calls to static members of the user-defined type with this name:
	Public Sub Add(ByVal formattedString_Conflict As FormattedString)
		Cell.AddTo(cells, formattedString_Conflict)
	End Sub

'INSTANT VB NOTE: The parameter cell was renamed since it may cause conflicts with calls to static members of the user-defined type with this name:
	Public Sub Add(ByVal cell_Conflict As Cell)
		cells.Add(cell_Conflict)
	End Sub

'INSTANT VB NOTE: The parameter cell was renamed since it may cause conflicts with calls to static members of the user-defined type with this name:
	Public Sub Replace(ByVal index As Integer, ByVal cell_Conflict As Cell)
		Cell.SharedPool.Put(cells(index))
		cells(index) = cell_Conflict
	End Sub

	Public Sub CopyTo(Cell? ByVal cells() As , ByVal targetPosition As Integer, ByVal count As Integer)
		Me.cells.CopyTo(0, cells!, targetPosition, count)
	End Sub

	Private Function GetDebuggerDisplay() As String
		Return String.Join("", cells.Select(Function(c) c.Text))
	End Function
End Class