Imports System
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.Examples
    Public Class IndeterminateChildrenNoCollapseExample
        Inherits ExampleBase
        Protected Overrides Async Function StartAsync() As Task
            Const totalChildren = 2
            Dim random As Random = New Random()
            Dim options = New ProgressBarOptions With {
    .ForegroundColor = ConsoleColor.Yellow,
    .BackgroundColor = ConsoleColor.DarkGray,
    .ProgressCharacter = "─"c
}
            Dim childOptions = New ProgressBarOptions With {
    .ForegroundColor = ConsoleColor.Green,
    .BackgroundColor = ConsoleColor.DarkGray,
    .ProgressCharacter = "─"c,
    .CollapseWhenFinished = False
}
            Using pbar = New ProgressBar(totalChildren, "main progressbar", options)
                For i = 1 To totalChildren
                    pbar.Message = $"Start {i} of {totalChildren}: main progressbar"
                    Using child = pbar.SpawnIndeterminate("child action " & i, childOptions)
                        Await Task.Delay(1000 * random.Next(5, 15))
                        child.Finished()
                    End Using
                    pbar.Tick()
                Next
            End Using
        End Function
    End Class
End Namespace
