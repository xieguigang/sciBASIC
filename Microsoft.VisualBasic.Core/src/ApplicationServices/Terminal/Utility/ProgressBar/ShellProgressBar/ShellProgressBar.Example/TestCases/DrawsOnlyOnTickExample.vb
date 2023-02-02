Imports System.Threading
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.TestCases
    Public Class DrawsOnlyOnTickExample
        Implements IProgressBarExample
        Public Function Start(token As CancellationToken) As Task Implements IProgressBarExample.Start
            Dim ticks = 5
            Dim updateOnTicksOnlyOptions = New ProgressBarOptions With {
                .DisplayTimeInRealTime = False
            }
            Using pbar = New ProgressBar(ticks, "only update time on ticks", updateOnTicksOnlyOptions)
                For i = 0 To ticks - 1
                    pbar.Tick("only update time on ticks, current: " & i)
                    Thread.Sleep(1750)
                Next
            End Using
            Return Task.FromResult(1)
        End Function
    End Class
End Namespace
