#Region "License Header"
' This Source Code Form is subject to the terms of the Mozilla Public
' License, v. 2.0. If a copy of the MPL was not distributed with this
' file, You can obtain one at https://mozilla.org/MPL/2.0/.
#End Region

Imports System
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks



Friend Class SyntaxHighlighter
	Private ReadOnly promptCallbacks As IPromptCallbacks
	Private ReadOnly hasUserOptedOutFromColor As Boolean

	' quick and dirty caching, mainly to handle cases where the user enters control
	' characters (e.g. arrow keys, intellisense) that don't actually change the highlighted input
	Private previousInput As String
	Private previousOutput As IReadOnlyCollection(Of FormatSpan)

	Public Sub New(ByVal promptCallbacks As IPromptCallbacks, ByVal hasUserOptedOutFromColor As Boolean)
		Me.promptCallbacks = promptCallbacks
		Me.hasUserOptedOutFromColor = hasUserOptedOutFromColor
		Me.previousInput = String.Empty
		Me.previousOutput = Array.Empty(Of FormatSpan)()
	End Sub

	Public Async Function HighlightAsync(ByVal input As String, ByVal cancellationToken As CancellationToken) As Task(Of IReadOnlyCollection(Of FormatSpan))
		If hasUserOptedOutFromColor Then
			Return Array.Empty(Of FormatSpan)()
		End If

		If input.Equals(previousInput) Then
			Return previousOutput
		End If

		Dim highlights = Await promptCallbacks.HighlightCallbackAsync(input, cancellationToken).ConfigureAwait(False)
		previousInput = input
		previousOutput = highlights
		Return highlights
	End Function
End Class