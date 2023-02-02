Imports System
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.Examples
    Public Class AlternateFinishedColorExample
        Inherits ExampleBase
        Protected Overrides Async Function StartAsync() As Task
            Dim options = New ProgressBarOptions With {
    .ForegroundColor = ConsoleColor.Yellow,
    .ForegroundColorDone = ConsoleColor.DarkGreen,
    .ForegroundColorError = ConsoleColor.Red,
    .BackgroundColor = ConsoleColor.DarkGray,
    .BackgroundCharacter = "▓"c
}

            Dim pbar = New ProgressBar(100, "100 ticks", options)
            Await Task.Run(Sub()
                               For i = 0 To 9
                                   Call Task.Delay(10).Wait()
                                   pbar.Tick($"Step {i}")
                               Next
                               pbar.WriteErrorLine("The task ran into an issue!")
                               ' OR pbar.ObservedError = true;
                           End Sub)
            pbar.Message = "Indicate the task is done, but the status is not Green."
        End Function


    End Class
End Namespace
