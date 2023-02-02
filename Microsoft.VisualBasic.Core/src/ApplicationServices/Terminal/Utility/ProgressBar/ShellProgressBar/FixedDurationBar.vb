Imports System
Imports System.Threading
Imports stdNum = System.Math

Namespace ApplicationServices.Terminal.ProgressBar.ShellProgressBar
	Public Class FixedDurationBar
		Inherits ProgressBar
		Private _IsCompleted As Boolean

		Public Property IsCompleted As Boolean
			Get
				Return _IsCompleted
			End Get
			Private Set(value As Boolean)
				_IsCompleted = value
			End Set
		End Property

		Private ReadOnly _completedHandle As ManualResetEvent = New ManualResetEvent(False)
		Public ReadOnly Property CompletedHandle As WaitHandle
			Get
				Return _completedHandle
			End Get
		End Property

		Public Sub New(duration As TimeSpan, message As String, color As ConsoleColor)
			Me.New(duration, message, New ProgressBarOptions With {
				.ForegroundColor = color
			})
		End Sub

		Public Sub New(duration As TimeSpan, message As String, Optional options As ProgressBarOptions = Nothing)
			MyBase.New(CInt(stdNum.Ceiling(duration.TotalSeconds)) * 2, message, options)
			If Not Me.Options.DisplayTimeInRealTime Then
				Throw New ArgumentException($"{NameOf(ProgressBarOptions)}.{NameOf(ProgressBarOptions.DisplayTimeInRealTime)} has to be true for {NameOf(FixedDurationBar)}", NameOf(options))
			End If
		End Sub

		Private _seenTicks As Long = 0
		Protected Overrides Sub OnTimerTick()
			Interlocked.Increment(_seenTicks)
			Tick()
			MyBase.OnTimerTick()
		End Sub

		Protected Overrides Sub OnDone()
			IsCompleted = True
			_completedHandle.Set()
		End Sub
	End Class
End Namespace
