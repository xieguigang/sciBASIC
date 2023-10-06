Namespace ApplicationServices.Terminal

	Public Class KeyBindings
		Public ReadOnly Property CommitCompletion() As KeyPressPatterns
		Public ReadOnly Property TriggerCompletionList() As KeyPressPatterns
		Public ReadOnly Property NewLine() As KeyPressPatterns
		Public ReadOnly Property SubmitPrompt() As KeyPressPatterns
		Public ReadOnly Property HistoryPrevious() As KeyPressPatterns
		Public ReadOnly Property HistoryNext() As KeyPressPatterns
		Public ReadOnly Property TriggerOverloadList() As KeyPressPatterns

		'INSTANT VB NOTE: The variable commitCompletion was renamed since Visual Basic does not handle local variables named the same as class members well:
		'INSTANT VB NOTE: The variable triggerCompletionList was renamed since Visual Basic does not handle local variables named the same as class members well:
		'INSTANT VB NOTE: The variable newLine was renamed since Visual Basic does not handle local variables named the same as class members well:
		'INSTANT VB NOTE: The variable submitPrompt was renamed since Visual Basic does not handle local variables named the same as class members well:
		'INSTANT VB NOTE: The variable historyPrevious was renamed since Visual Basic does not handle local variables named the same as class members well:
		'INSTANT VB NOTE: The variable historyNext was renamed since Visual Basic does not handle local variables named the same as class members well:
		'INSTANT VB NOTE: The variable triggerOverloadList was renamed since Visual Basic does not handle local variables named the same as class members well:
		Public Sub New(Optional ByVal commitCompletion_Conflict As KeyPressPatterns = Nothing, Optional ByVal triggerCompletionList_Conflict As KeyPressPatterns = Nothing, Optional ByVal newLine_Conflict As KeyPressPatterns = Nothing, Optional ByVal submitPrompt_Conflict As KeyPressPatterns = Nothing, Optional ByVal historyPrevious_Conflict As KeyPressPatterns = Nothing, Optional ByVal historyNext_Conflict As KeyPressPatterns = Nothing, Optional ByVal triggerOverloadList_Conflict As KeyPressPatterns = Nothing)
			Me.CommitCompletion = [Get](commitCompletion_Conflict, New KeyPressPattern(Enter), New KeyPressPattern(TAB))
			Me.TriggerCompletionList = [Get](triggerCompletionList_Conflict, New KeyPressPattern(Control, Spacebar))
			Me.NewLine = [Get](newLine_Conflict, New KeyPressPattern(Shift, Enter))
			Me.SubmitPrompt = [Get](submitPrompt_Conflict, New KeyPressPattern(Enter), New KeyPressPattern(Control, Enter), New KeyPressPattern(Control Or Alt, Enter))
			Me.HistoryPrevious = [Get](historyPrevious_Conflict, New KeyPressPattern(UpArrow))
			Me.HistoryNext = [Get](historyNext_Conflict, New KeyPressPattern(DownArrow))
			Me.TriggerOverloadList = triggerOverloadList_Conflict
		End Sub

		'INSTANT VB TODO TASK: Local functions are not converted by Instant VB:
		Private Shared Function [Get](patterns As KeyPressPatterns, ParamArray defaultPatterns As KeyPressPattern()) As KeyPressPatterns
			'		{
			Return If(patterns.HasAny, patterns, New KeyPressPatterns(defaultPatterns))
		End Function
	End Class
End Namespace