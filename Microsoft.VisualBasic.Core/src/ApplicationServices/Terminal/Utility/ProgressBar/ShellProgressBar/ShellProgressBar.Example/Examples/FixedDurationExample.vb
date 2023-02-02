Imports System
Imports System.Threading
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.Examples
    Public Class FixedDurationExample
        Inherits ExampleBase
        Protected Overrides Function StartAsync() As Task
            Dim options = New ProgressBarOptions With {
    .ForegroundColor = ConsoleColor.Yellow,
    .ForegroundColorDone = ConsoleColor.DarkGreen,
    .BackgroundColor = ConsoleColor.DarkGray,
    .BackgroundCharacter = "▓"c
}
            Dim wait = TimeSpan.FromSeconds(5)
            Using pbar = New FixedDurationBar(wait, "", options)
                Dim t = New Thread(Sub() LongRunningTask(pbar))
                t.Start()

                If Not pbar.CompletedHandle.WaitOne(wait) Then Console.Error.WriteLine($"{NameOf(FixedDurationBar)} did not signal {NameOf(FixedDurationBar.CompletedHandle)} after {wait}")

            End Using

            Return Task.CompletedTask
        End Function

        Private Shared Sub LongRunningTask(bar As FixedDurationBar)
            For i = 0 To 999999
                bar.Message = $"{i} events"
                If bar.IsCompleted Then Exit For
                Thread.Sleep(1)
            Next
        End Sub
    End Class
End Namespace
