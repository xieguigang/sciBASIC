Imports System.Threading
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.TestCases
    Public Class NeverTicksExample
        Implements IProgressBarExample
        Public Function Start(token As CancellationToken) As Task Implements IProgressBarExample.Start
            Dim ticks = 10
            Using pbar = New ProgressBar(ticks, "A console progress bar that never ticks")
            End Using
            Return Task.FromResult(1)
        End Function
    End Class
End Namespace
