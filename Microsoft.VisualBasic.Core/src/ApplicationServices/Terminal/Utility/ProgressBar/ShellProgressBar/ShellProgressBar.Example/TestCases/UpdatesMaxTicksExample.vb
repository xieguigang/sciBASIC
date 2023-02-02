Imports System
Imports System.Threading
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.TestCases
    Public Class UpdatesMaxTicksExample
        Implements IProgressBarExample
        Public Function Start(token As CancellationToken) As Task Implements IProgressBarExample.Start
            Dim ticks = 10
            Using pbar = New ProgressBar(ticks, "My operation that updates maxTicks", ConsoleColor.Cyan)
                Dim sleep = 1000
                For i = 0 To ticks - 1
                    pbar.Tick("Updating maximum ticks " & i)
                    If i = 5 Then
                        ticks = 120
                        pbar.MaxTicks = ticks
                        sleep = 50
                    End If
                    Thread.Sleep(sleep)
                Next
            End Using
            Return Task.FromResult(1)
        End Function
    End Class
End Namespace
