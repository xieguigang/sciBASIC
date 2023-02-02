Imports System
Imports System.Threading
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.Examples
    Public Class EstimatedDurationExample
        Inherits ExampleBase
        Protected Overrides Function StartAsync() As Task
            Const totalTicks = 10
            Dim options = New ProgressBarOptions With {
    .ProgressCharacter = "─"c,
    .ShowEstimatedDuration = True
}
            Using pbar = New ProgressBar(totalTicks, "you can set the estimated duration too", options)
                pbar.EstimatedDuration = TimeSpan.FromMilliseconds(totalTicks * 500)

                Dim initialMessage = pbar.Message
                For i = 0 To totalTicks - 1
                    pbar.Message = $"Start {i + 1} of {totalTicks}: {initialMessage}"
                    Thread.Sleep(500)

                    ' Simulate changing estimated durations while progress increases
                    Dim estimatedDuration = TimeSpan.FromMilliseconds(500 * totalTicks) + TimeSpan.FromMilliseconds(300 * i)
                    pbar.Tick(estimatedDuration, $"End {i + 1} of {totalTicks}: {initialMessage}")
                Next
            End Using

            Return Task.CompletedTask
        End Function
    End Class
End Namespace
