Imports System
Imports System.Threading

Namespace ApplicationServices.Terminal.ProgressBar.ShellProgressBar
    Public Class IndeterminateChildProgressBar
        Inherits ChildProgressBar
        Private Const MaxTicksForIndeterminate As Integer = 20
        Private _timer As Timer
        Friend Sub New(message As String, scheduleDraw As Action, writeLine As Action(Of String), writeError As Action(Of String), Optional options As ProgressBarOptions = Nothing, Optional growth As Action(Of ProgressBarHeight) = Nothing)
            MyBase.New(MaxTicksForIndeterminate, message, scheduleDraw, writeLine, writeError, options, growth)
            If options Is Nothing Then
                options = New ProgressBarOptions()
            End If

            options.DisableBottomPercentage = True
            _timer = New Timer(Sub(s) OnTimerTick(), Nothing, 500, 500)
        End Sub

        Private _seenTicks As Long = 0

        Protected Sub OnTimerTick()
            Interlocked.Increment(_seenTicks)
            If _seenTicks = MaxTicksForIndeterminate - 1 Then
                Tick(0)
                Interlocked.Exchange(_seenTicks, 0)
            Else
                Tick()
            End If
            DisplayProgress()
        End Sub

        Public Sub Finished()
            _timer.Change(Timeout.Infinite, Timeout.Infinite)
            _timer.Dispose()
            Tick(MaxTicksForIndeterminate)
        End Sub

        Public Overloads Sub Dispose()
            If _timer IsNot Nothing Then _timer.Dispose()
            For Each c In Children
                c.Dispose()
            Next
            OnDone()
        End Sub
    End Class
End Namespace
