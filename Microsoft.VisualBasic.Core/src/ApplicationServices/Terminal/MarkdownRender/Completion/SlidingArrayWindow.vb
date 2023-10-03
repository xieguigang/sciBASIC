#Region "License Header"
' This Source Code Form is subject to the terms of the Mozilla Public
' License, v. 2.0. If a copy of the MPL was not distributed with this
' file, You can obtain one at https://mozilla.org/MPL/2.0/.
#End Region

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports PrettyPrompt.Documents

Private PrettyPrompt As namespace

''' <summary>
''' Datastructure that provides a window over a segment of an array, similar to <see cref="ArraySegment{T}"/>, but
''' also has a concept of the window "sliding" to always keep a selected index in view. This datastructure powers
''' the auto-complete menu, and the window slides to provide the scrolling of the menu.
''' </summary>
Friend NotInheritable Class SlidingArrayWindow
	Private ReadOnly itemsOriginal As New List(Of CompletionItem)()
	Private ReadOnly windowBuffer As Integer
'INSTANT VB NOTE: The field visibleItems was renamed since Visual Basic does not allow fields to have the same name as other class members:
	Private ReadOnly visibleItems_Conflict As New List(Of CompletionItem)()
	Private itemsSorted As New List(Of (Item As CompletionItem, IsMatching As Boolean))()
	Private windowLength As Integer
	Private windowStart As Integer

	Private selectedIndex? As Integer
	Private Async Function SetSelectedIndex(ByVal value? As Integer, ByVal cancellationToken As CancellationToken) As Task
		selectedIndex = value
		Dim selectedItemChanged = SelectedItemChangedEvent
		If selectedItemChanged IsNot Nothing Then
			Await selectedItemChanged(SelectedItem, cancellationToken).ConfigureAwait(False)
		End If
	End Function

	Public Sub New(Optional ByVal windowBuffer As Integer = 3)
		Me.windowBuffer = windowBuffer
	End Sub

	Public Integer? ReadOnly Property SelectedIndexInAllItems() As
		Get
			Return selectedIndex
		End Get
	End Property
	Public Integer? ReadOnly Property SelectedIndexInVisibleItems() As
		Get
			Return selectedIndex - windowStart
		End Get
	End Property
	Public CompletionItem? ReadOnly Property SelectedItem() As
		Get
			Return If(Not IsEmpty AndAlso selectedIndex.HasValue, itemsSorted(selectedIndex.Value).Item, Nothing)
		End Get
	End Property
	Public ReadOnly Property AllItemsCount() As Integer
		Get
			Return itemsSorted.Count
		End Get
	End Property
	Public ReadOnly Property VisibleItemsCount() As Integer
		Get
			Return visibleItems_Conflict.Count
		End Get
	End Property
	Public ReadOnly Property IsEmpty() As Boolean
		Get
			Return AllItemsCount = 0
		End Get
	End Property
	Public ReadOnly Property VisibleItems() As IReadOnlyList(Of CompletionItem)
		Get
			Return visibleItems_Conflict
		End Get
	End Property

'INSTANT VB WARNING: Nullable reference types have no equivalent in VB:
'ORIGINAL LINE: public event Func<System.Nullable<CompletionItem>, CancellationToken, Task>? SelectedItemChanged;
	Public Event SelectedItemChanged As Func(Of CompletionItem, CancellationToken, Task)

	Public Async Function UpdateItems(ByVal items As IEnumerable(Of CompletionItem), ByVal documentText As String, ByVal documentCaret As Integer, ByVal spanToReplace As TextSpan, ByVal windowLength As Integer, ByVal cancellationToken As CancellationToken) As Task
		Me.windowLength = windowLength
		Me.itemsOriginal.Clear()
		Me.itemsOriginal.AddRange(items)

		Await Match(documentText, documentCaret, spanToReplace, cancellationToken).ConfigureAwait(False)
		Await ResetSelectedIndex(cancellationToken).ConfigureAwait(False)
		UpdateVisibleItems()
	End Function

	Public Async Function Match(ByVal documentText As String, ByVal caret As Integer, ByVal spanToBeReplaced As TextSpan, ByVal cancellationToken As CancellationToken) As Task
		'this could be done more efficiently if we would have in-place stable List<T> sort implementation
		itemsSorted = itemsOriginal.Select(Function(i) (Item:= i, Priority:= i.GetCompletionItemPriority(documentText, caret, spanToBeReplaced))).OrderByDescending(Function(t) t.Priority).Select(Function(t) (t.Item, t.Priority >= 0)).ToList()

		UpdateVisibleItems()
		Await ResetSelectedIndex(cancellationToken).ConfigureAwait(False)
	End Function

	Public Async Function IncrementSelectedIndex(ByVal cancellationToken As CancellationToken) As Task
		If Not selectedIndex.HasValue Then
			Await SetSelectedIndex(0, cancellationToken).ConfigureAwait(False)
			Return
		End If

		If selectedIndex.Equals(AllItemsCount - 1) Then
			Return
		End If

		Await SetSelectedIndex(selectedIndex + 1, cancellationToken).ConfigureAwait(False)

		If selectedIndex + windowBuffer >= windowStart + windowLength AndAlso windowStart + windowLength < AllItemsCount Then
			windowStart += 1
			UpdateVisibleItems()
		End If
	End Function

	Public Async Function DecrementSelectedIndex(ByVal cancellationToken As CancellationToken) As Task
		If Not selectedIndex.HasValue Then
			Await SetSelectedIndex(0, cancellationToken).ConfigureAwait(False)
			Return
		End If

		If selectedIndex.Equals(0) Then
			Return
		End If

		Await SetSelectedIndex(selectedIndex - 1, cancellationToken).ConfigureAwait(False)

		If selectedIndex - windowBuffer < windowStart AndAlso windowStart > 0 Then
			windowStart -= 1
			UpdateVisibleItems()
		End If
	End Function

	Public Async Function Clear(ByVal cancellationToken As CancellationToken) As Task
		itemsOriginal.Clear()
		itemsSorted.Clear()
		windowLength = 0
		Await ResetSelectedIndex(cancellationToken).ConfigureAwait(False)
		UpdateVisibleItems()
	End Function

	Private Async Function ResetSelectedIndex(ByVal cancellationToken As CancellationToken) As Task
		Await SetSelectedIndex(If(IsEmpty, Nothing, (If(itemsSorted(0).IsMatching, 0, Nothing))), cancellationToken).ConfigureAwait(False)
		windowStart = 0
	End Function

	Private Sub UpdateVisibleItems()
		visibleItems_Conflict.Clear()
		Dim count = Math.Min(windowLength, AllItemsCount)
		Dim i As Integer = windowStart
		Do While i < windowStart + count
			visibleItems_Conflict.Add(itemsSorted(i).Item)
			i += 1
		Loop
	End Sub
End Class
