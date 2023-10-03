#Region "License Header"
' This Source Code Form is subject to the terms of the Mozilla Public
' License, v. 2.0. If a copy of the MPL was not distributed with this
' file, You can obtain one at https://mozilla.org/MPL/2.0/.
#End Region

Imports System
Imports System.Threading
Imports PrettyPrompt.Consoles

Private PrettyPrompt As namespace

Friend NotInheritable Class CancellationManager
	Private ReadOnly console As IConsole
'INSTANT VB WARNING: Nullable reference types have no equivalent in VB:
'ORIGINAL LINE: private PromptResult? execution;
	Private execution As PromptResult

	Public Sub New(ByVal console As IConsole)
		Me.console = console
		AddHandler console.CancelKeyPress, AddressOf SignalCancellationToLastResult
	End Sub

	Friend Sub CaptureControlC()
		Me.console.CaptureControlC = True
	End Sub

	Friend Sub AllowControlCToCancelResult(ByVal result As PromptResult)
		Me.execution = result
		Me.execution.CancellationTokenSource = New CancellationTokenSource()
		Me.console.CaptureControlC = False
	End Sub

'INSTANT VB WARNING: Nullable reference types have no equivalent in VB:
'ORIGINAL LINE: private void SignalCancellationToLastResult(object? sender, ConsoleCancelEventArgs e)
	Private Sub SignalCancellationToLastResult(ByVal sender As Object, ByVal e As ConsoleCancelEventArgs)
		e.Cancel = True
		Me.execution?.CancellationTokenSource?.Cancel()
	End Sub
End Class
