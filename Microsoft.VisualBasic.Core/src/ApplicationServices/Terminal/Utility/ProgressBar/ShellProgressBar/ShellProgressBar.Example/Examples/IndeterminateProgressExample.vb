Imports System
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.Examples
    Public Class IndeterminateProgressExample
        Inherits ExampleBase
        Protected Overrides Function StartAsync() As Task
            Dim options = New ProgressBarOptions With {
    .ForegroundColor = ConsoleColor.Yellow,
    .ForegroundColorDone = ConsoleColor.DarkGreen,
    .BackgroundColor = ConsoleColor.DarkGray,
    .BackgroundCharacter = "▓"c
}

            Using pbar = New IndeterminateProgressBar("Indeterminate", options)
                Call Task.Run(Sub()
                                  For i = 0 To 999
                                      pbar.Message = $"The progress is beating to its own drum (indeterminate) {i}"
                                      Call Task.Delay(10).Wait()
                                  Next
                              End Sub).Wait()
                pbar.Finished()
                pbar.Message = "Finished! Moving on to the next in 5 seconds."
            End Using

            Return Task.CompletedTask
        End Function
    End Class
End Namespace
