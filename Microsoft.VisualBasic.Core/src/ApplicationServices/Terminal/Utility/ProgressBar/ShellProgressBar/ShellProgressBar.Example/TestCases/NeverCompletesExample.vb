Imports System.Threading
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.TestCases
    Public Class NeverCompletesExample
        Implements IProgressBarExample
        Public Function Start(token As CancellationToken) As Task Implements IProgressBarExample.Start
            Dim ticks = 5
            Using pbar = New ProgressBar(ticks, "A console progress bar does not complete")
                pbar.Tick()
                pbar.Tick()
                pbar.Tick()
                pbar.Tick()
            End Using
            Return Task.FromResult(1)
        End Function
    End Class
End Namespace
