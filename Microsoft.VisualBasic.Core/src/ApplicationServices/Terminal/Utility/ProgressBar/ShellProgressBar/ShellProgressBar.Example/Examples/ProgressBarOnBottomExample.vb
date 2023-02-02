Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.Examples
    Public Class ProgressBarOnBottomExample
        Inherits ExampleBase
        Protected Overrides Function StartAsync() As Task
            Const totalTicks = 10
            Dim options = New ProgressBarOptions With {
    .ProgressCharacter = "─"c,
    .ProgressBarOnBottom = True
}
            Dim pbar = New ProgressBar(totalTicks, "progress bar is on the bottom now", options)
            TickToCompletion(pbar, totalTicks, sleep:=500)

            Return Task.CompletedTask
        End Function
    End Class
End Namespace
