Imports System

Namespace ApplicationServices.Terminal.ProgressBar.ShellProgressBar
	Friend Class Progress(Of T)
		Implements IProgress(Of T), IDisposable
		Private ReadOnly _progressBar As WeakReference(Of IProgressBar)
		Private ReadOnly _message As Func(Of T, String)
		Private ReadOnly _percentage As Func(Of T, Double?)

		Public Sub New(progressBar As IProgressBar, message As Func(Of T, String), percentage As Func(Of T, Double?))
			_progressBar = New WeakReference(Of IProgressBar)(progressBar)
			_message = message
			_percentage = If(percentage, Function(value) If(CType(CObj(value), Double?), CType(CObj(value), Single?)))
		End Sub

		Public Sub Report(value As T) Implements IProgress(Of T).Report
			Dim progressBar As IProgressBar = Nothing
			If Not _progressBar.TryGetTarget(progressBar) Then Return

			Dim message = _message?.Invoke(value)
			Dim percentage = _percentage(value)
			If percentage.HasValue Then
				progressBar.Tick(percentage * progressBar.MaxTicks, message)
			Else
				progressBar.Tick(message)
			End If
		End Sub

		Public Sub Dispose() Implements IDisposable.Dispose
			Dim progressBar As IProgressBar = Nothing

			If _progressBar.TryGetTarget(progressBar) Then
				progressBar.Dispose()
			End If
		End Sub
	End Class
End Namespace
