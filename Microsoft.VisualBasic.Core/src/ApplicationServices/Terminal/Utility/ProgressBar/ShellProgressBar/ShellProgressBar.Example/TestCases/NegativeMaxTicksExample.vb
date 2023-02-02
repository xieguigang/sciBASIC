Imports System
Imports System.Threading
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.TestCases
    Public Class NegativeMaxTicksExample
        Implements IProgressBarExample
        Public Function Start(token As CancellationToken) As Task Implements IProgressBarExample.Start
            Dim ticks = -100
            Using pbar = New ProgressBar(ticks, "my operation with negative ticks", ConsoleColor.Cyan)
                For i = 0 To ticks - 1
                    pbar.Tick("step " & i)
                    Thread.Sleep(50)
                Next
            End Using
            Return Task.FromResult(1)
        End Function
    End Class
End Namespace
