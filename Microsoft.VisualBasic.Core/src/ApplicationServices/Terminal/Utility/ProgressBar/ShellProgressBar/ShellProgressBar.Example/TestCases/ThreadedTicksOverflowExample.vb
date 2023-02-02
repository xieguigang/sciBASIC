Imports System
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.TestCases
    Public Class ThreadedTicksOverflowExample
        Implements IProgressBarExample
        Public Function Start(token As CancellationToken) As Task Implements IProgressBarExample.Start
            Dim ticks = 200
            Using pbar = New ProgressBar(ticks / 10, "My operation that ticks to often using threads", ConsoleColor.Cyan)
                Dim threads = Enumerable.Range(0, ticks).[Select](Function(i) New Thread(Sub() pbar.Tick("threaded tick " & i))).ToList()
                For Each thread In threads
                    thread.Start()
                Next
                For Each thread In threads
                    thread.Join()
                Next
            End Using
            Return Task.FromResult(1)
        End Function
    End Class
End Namespace
