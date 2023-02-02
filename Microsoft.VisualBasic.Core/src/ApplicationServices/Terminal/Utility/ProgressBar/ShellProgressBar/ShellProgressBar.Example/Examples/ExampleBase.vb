Imports System
Imports System.Threading
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.Examples
    Public MustInherit Class ExampleBase
        Implements IProgressBarExample
        Private Property RequestToQuit As Boolean

        Protected Sub TickToCompletion(pbar As IProgressBar, ticks As Integer, Optional sleep As Integer = 1750, Optional childAction As Action(Of Integer) = Nothing)
            Dim initialMessage = pbar.Message
            Dim i = 0

            While i < ticks AndAlso Not RequestToQuit
                pbar.Message = $"Start {i + 1} of {ticks} {Console.CursorTop}/{Console.WindowHeight}: {initialMessage}"
                childAction?.Invoke(i)
                Thread.Sleep(sleep)
                pbar.Tick($"End {i + 1} of {ticks} {Console.CursorTop}/{Console.WindowHeight}: {initialMessage}")
                i += 1
            End While
        End Sub

        Public Async Function Start(token As CancellationToken) As Task Implements IProgressBarExample.Start
            RequestToQuit = False
            token.Register(Sub() RequestToQuit = True)

            Await StartAsync()
        End Function

        Protected MustOverride Function StartAsync() As Task
    End Class
End Namespace
