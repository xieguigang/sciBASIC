Imports System
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.Examples
    Public Class MessageBeforeAndAfterExample
        Inherits ExampleBase
        Protected Overrides Function StartAsync() As Task
            Console.WriteLine("This should not be overwritten")
            Const totalTicks = 10
            Dim options = New ProgressBarOptions With {
    .ForegroundColor = ConsoleColor.Yellow,
    .ForegroundColorDone = ConsoleColor.DarkGreen,
    .BackgroundColor = ConsoleColor.DarkGray,
    .BackgroundCharacter = "▓"c
}
            Using pbar = New ProgressBar(totalTicks, "showing off styling", options)
                TickToCompletion(pbar, totalTicks, sleep:=500, Sub(i) pbar.WriteErrorLine($"This should appear above:{i}"))
            End Using

            Console.WriteLine("This should not be overwritten either afterwards")
            Return Task.CompletedTask
        End Function
    End Class
End Namespace
