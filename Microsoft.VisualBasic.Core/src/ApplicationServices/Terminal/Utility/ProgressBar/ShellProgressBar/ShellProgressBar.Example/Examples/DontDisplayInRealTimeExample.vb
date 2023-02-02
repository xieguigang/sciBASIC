Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.Examples
    Public Class DontDisplayInRealTimeExample
        Inherits ExampleBase
        Protected Overrides Function StartAsync() As Task
            Const totalTicks = 5
            Dim options = New ProgressBarOptions With {
    .DisplayTimeInRealTime = False
}
            Using pbar = New ProgressBar(totalTicks, "only draw progress on tick", options)
                TickToCompletion(pbar, totalTicks, sleep:=1750)
            End Using

            Return Task.CompletedTask
        End Function
    End Class
End Namespace
