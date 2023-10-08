#Region "License Header"
' This Source Code Form is subject to the terms of the Mozilla Public
' License, v. 2.0. If a copy of the MPL was not distributed with this
' file, You can obtain one at https://mozilla.org/MPL/2.0/.
#End Region

Imports System.Collections.Immutable
Imports System.Threading

Namespace ApplicationServices.Terminal

	''' <summary>
	''' A menu item in the Completion Menu Pane.
	''' </summary>
	<DebuggerDisplay("{DisplayText}")>
	Public Class CompletionItem
		Public Delegate Function GetExtendedDescriptionHandler(ByVal cancellationToken As CancellationToken) As Task(Of FormattedString)

		''' <summary>
		''' When the completion item is selected, this text will be inserted into the document at the specified start index.
		''' </summary>
		Public ReadOnly Property ReplacementText() As String

		''' <summary>
		''' This text will be displayed in the completion menu.
		''' </summary>
		Public ReadOnly Property DisplayTextFormatted() As FormattedString

		''' <summary>
		''' This text will be displayed in the completion menu.
		''' </summary>
		Public ReadOnly Property DisplayText() As String
			Get
				Return DisplayTextFormatted.Text!
			End Get
		End Property

		''' <summary>
		''' The text used to determine if the item matches the filter in the list.
		''' </summary>
		Public ReadOnly Property FilterText() As String

		''' <summary>
		''' Rules that modify the set of characters that can be typed to cause the selected item to be committed.
		''' </summary>
		Public ReadOnly Property CommitCharacterRules() As ImmutableArray(Of CharacterSetModificationRule)

		''' <summary>
		''' This task will be executed when the item is selected, to display the extended "tool tip" description to the right of the menu.
		''' </summary>
		Public Function GetExtendedDescriptionAsync(ByVal cancellationToken As CancellationToken) As Task(Of FormattedString)
			Return getExtendedDescription(cancellationToken)
		End Function

		Private ReadOnly getExtendedDescription As GetExtendedDescriptionHandler

		''' <param name="replacementText_Conflict">When the completion item is selected, this text will be inserted into the document at the specified start index.</param>
		''' <param name="displayText_Conflict">This text will be displayed in the completion menu. If not specified, the <paramref name="replacementText"/> value will be used.</param>
		''' <param name="getExtendedDescription">This lazy task will be executed when the item is selected, to display the extended "tool tip" description to the right of the menu.</param>
		''' <param name="filterText_Conflict">The text used to determine if the item matches the filter in the list. If not specified the <paramref name="replacementText"/> value is used.</param>
		''' <param name="commitCharacterRules_Conflict">Rules that modify the set of characters that can be typed to cause the selected item to be committed.</param>
		'INSTANT VB WARNING: Nullable reference types have no equivalent in VB:
		'ORIGINAL LINE: public CompletionItem(string replacementText, FormattedString displayText = default, string? filterText = null, System.Nullable<GetExtendedDescriptionHandler> getExtendedDescription = null, ImmutableArray<CharacterSetModificationRule> commitCharacterRules = default)
		'INSTANT VB NOTE: The variable replacementText was renamed since Visual Basic does not handle local variables named the same as class members well:
		'INSTANT VB NOTE: The variable displayText was renamed since Visual Basic does not handle local variables named the same as class members well:
		'INSTANT VB NOTE: The variable filterText was renamed since Visual Basic does not handle local variables named the same as class members well:
		'INSTANT VB NOTE: The variable commitCharacterRules was renamed since Visual Basic does not handle local variables named the same as class members well:
		Public Sub New(ByVal replacementText_Conflict As String, Optional ByVal displayText_Conflict As FormattedString = Nothing, Optional ByVal filterText_Conflict As String = Nothing, Optional ByVal getExtendedDescription? As GetExtendedDescriptionHandler = Nothing, Optional ByVal commitCharacterRules_Conflict As ImmutableArray(Of CharacterSetModificationRule) = Nothing)
			Me.ReplacementText = replacementText_Conflict
			DisplayTextFormatted = If(displayText_Conflict.IsEmpty, replacementText_Conflict, displayText_Conflict)
			Me.FilterText = If(filterText_Conflict, replacementText_Conflict)
			Me.CommitCharacterRules = If(commitCharacterRules_Conflict.IsDefault, ImmutableArray(Of CharacterSetModificationRule).Empty, commitCharacterRules_Conflict)

			'INSTANT VB WARNING: Nullable reference types have no equivalent in VB:
			'ORIGINAL LINE: Task<FormattedString>? extendedDescriptionTask = null;
			Dim extendedDescriptionTask As Task(Of FormattedString) = Nothing 'will be stored in closure of getExtendedDescription
			Me.getExtendedDescription = Sub(ct) extendedDescriptionTask = Me.getExtendedDescription = Function(ct) If(If(extendedDescriptionTask, getExtendedDescription?.Invoke(ct)), Task.FromResult(FormattedString.Empty))
		End Sub

		''' <summary>
		''' Determines the completion item priority in completion list with respect to currently written text.
		''' Higher numbers represent higher priority. Highest priority item will be on top of the completion list.
		''' </summary>
		''' <param name="text">The user's input text</param>
		''' <param name="caret">The index of the text caret in the input text</param>
		''' <param name="spanToBeReplaced">Span of text that will be replaced by inserted completion item</param>
		''' <returns>
		''' Integer representing priority of the item in completion list. Negative priorities represents
		''' non-matching items.
		''' </returns>
		Public Overridable Function GetCompletionItemPriority(ByVal text As String, ByVal caret As Integer, ByVal spanToBeReplaced As TextSpan) As Integer
			If spanToBeReplaced.IsEmpty Then
				Return 0
			End If

			Dim pattern = text.AsSpan(spanToBeReplaced)

			Dim valueLonger As ReadOnlySpan(Of Char)
			Dim valueShorter As ReadOnlySpan(Of Char)
			If pattern.Length <= FilterText.Length Then
				valueLonger = FilterText
				valueShorter = pattern
			Else
				valueLonger = pattern
				valueShorter = FilterText
			End If

			Dim priority As Integer
			If valueLonger.StartsWith(valueShorter, StringComparison.CurrentCulture) Then
				priority = 4
			ElseIf valueLonger.StartsWith(valueShorter, StringComparison.CurrentCultureIgnoreCase) Then
				priority = 3
			ElseIf valueLonger.Contains(valueShorter, StringComparison.CurrentCulture) Then
				priority = 2
			ElseIf valueLonger.Contains(valueShorter, StringComparison.CurrentCultureIgnoreCase) Then
				priority = 1
			Else
				Return Integer.MinValue 'completely non-matching item
			End If

			If pattern.Length <= FilterText.Length Then
				'matching item
				Return priority
			Else
				'non-matching item, but it's contained in pattern (which is better than completely unmatching)
				Return Integer.MinValue + priority
			End If
		End Function
	End Class
End Namespace