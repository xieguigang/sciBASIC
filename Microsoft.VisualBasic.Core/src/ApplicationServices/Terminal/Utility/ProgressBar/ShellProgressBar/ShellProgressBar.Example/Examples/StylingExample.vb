Imports System
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.Examples
    Public Class StylingExample
        Inherits ExampleBase
        Protected Overrides Function StartAsync() As Task
            Const totalTicks = 10
            Dim options = New ProgressBarOptions With {
    .ForegroundColor = ConsoleColor.Yellow,
    .ForegroundColorDone = ConsoleColor.DarkGreen,
    .BackgroundColor = ConsoleColor.DarkGray,
    .BackgroundCharacter = "▓"c
}
            Dim pbar = New ProgressBar(totalTicks, "showing off styling", options)
            TickToCompletion(pbar, totalTicks, sleep:=500, Sub(i)
                                                               If i > 5 Then pbar.ForegroundColor = ConsoleColor.Red
                                                           End Sub)

            Return Task.CompletedTask
        End Function
    End Class
End Namespace
