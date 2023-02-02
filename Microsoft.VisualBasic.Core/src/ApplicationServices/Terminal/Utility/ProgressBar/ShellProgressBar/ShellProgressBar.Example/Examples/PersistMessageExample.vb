Imports System
Imports System.Threading
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.Examples
    Public Class PersistMessageExample
        Inherits ExampleBase
        Protected Overrides Function StartAsync() As Task
            Dim options = New ProgressBarOptions With {
    .ForegroundColor = ConsoleColor.Yellow,
    .ForegroundColorDone = ConsoleColor.DarkGreen,
    .ForegroundColorError = ConsoleColor.Red,
    .BackgroundColor = ConsoleColor.DarkGray,
    .BackgroundCharacter = "▓"c,
    .WriteQueuedMessage = Function(o)
                              Dim writer = If(o.Error, Console.Error, Console.Out)
                              Dim c = If(o.Error, ConsoleColor.DarkRed, ConsoleColor.Blue)
                              If o.Line.StartsWith("Report 500") Then
                                  Console.ForegroundColor = ConsoleColor.Yellow
                                  writer.WriteLine("Add an extra message, because why not")

                                  Console.ForegroundColor = c
                                  writer.WriteLine(o.Line)
                                  Return 2 'signal to the progressbar we wrote two messages
                              End If
                              Console.ForegroundColor = c
                              writer.WriteLine(o.Line)
                              Return 1
                          End Function
}
            Dim wait = TimeSpan.FromSeconds(6)
            Dim pbar = New FixedDurationBar(wait, "", options)
            Dim t = New Thread(Sub() LongRunningTask(pbar))
            t.Start()

            If Not pbar.CompletedHandle.WaitOne(wait.Subtract(TimeSpan.FromSeconds(.5))) Then
                pbar.WriteErrorLine($"{NameOf(FixedDurationBar)} did not signal {NameOf(FixedDurationBar.CompletedHandle)} after {wait}")
                pbar.Dispose()
            End If
            Return Task.CompletedTask
        End Function

        Private Shared Sub LongRunningTask(bar As FixedDurationBar)
            For i = 0 To 999999
                bar.Message = $"{i} events"
                If bar.IsCompleted OrElse bar.ObservedError Then Exit For
                If i Mod 500 = 0 Then bar.WriteLine($"Report {i} to console above the progressbar")
                Thread.Sleep(1)
            Next
        End Sub
    End Class
End Namespace
