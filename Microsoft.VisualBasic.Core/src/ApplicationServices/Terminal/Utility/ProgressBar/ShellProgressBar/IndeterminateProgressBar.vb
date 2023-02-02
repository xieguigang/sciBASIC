Imports System
Imports System.Threading

Namespace ApplicationServices.Terminal.ProgressBar.ShellProgressBar
    Public Class IndeterminateProgressBar
        Inherits ProgressBar
        Private Const MaxTicksForIndeterminate As Integer = 20

        Public Sub New(message As String, color As ConsoleColor)
            Me.New(message, New ProgressBarOptions With {
    .ForegroundColor = color
})
        End Sub

        Public Sub New(message As String, Optional options As ProgressBarOptions = Nothing)
            MyBase.New(MaxTicksForIndeterminate, message, options)
            If options Is Nothing Then
                options = New ProgressBarOptions()
            End If

            options.DisableBottomPercentage = True
            options.DisplayTimeInRealTime = True

            If Not Me.Options.DisplayTimeInRealTime Then Throw New ArgumentException($"{NameOf(ProgressBarOptions)}.{NameOf(ProgressBarOptions.DisplayTimeInRealTime)} has to be true for {NameOf(FixedDurationBar)}", NameOf(options))
        End Sub

        Private _seenTicks As Long = 0

        Protected Overrides Sub OnTimerTick()
            Interlocked.Increment(_seenTicks)
            If _seenTicks = MaxTicksForIndeterminate - 1 Then
                Tick(0)
                Interlocked.Exchange(_seenTicks, 0)
            Else
                Tick()
            End If

            MyBase.OnTimerTick()
        End Sub

        Public Sub Finished()
            Tick(MaxTicksForIndeterminate)
        End Sub
    End Class
End Namespace
