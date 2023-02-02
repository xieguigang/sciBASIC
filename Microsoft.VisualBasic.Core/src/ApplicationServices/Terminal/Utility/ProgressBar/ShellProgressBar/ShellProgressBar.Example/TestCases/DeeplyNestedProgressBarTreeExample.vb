Imports System
Imports System.Runtime.InteropServices
Imports System.Threading
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.TestCases
    Public Class DeeplyNestedProgressBarTreeExample
        Implements IProgressBarExample
        Public Function Start(token As CancellationToken) As Task Implements IProgressBarExample.Start
            Dim random = New Random()

            Dim numberOfSteps = 7

            Dim overProgressOptions = New ProgressBarOptions With {
    .DenseProgressBar = True,
    .ProgressCharacter = "─"c,
    .BackgroundColor = ConsoleColor.DarkGray,
    .EnableTaskBarProgress = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
}

            Using pbar = New ProgressBar(numberOfSteps, "overall progress", overProgressOptions)
                Dim stepBarOptions = New ProgressBarOptions With {
    .DenseProgressBar = True,
    .ForegroundColor = ConsoleColor.Cyan,
    .ForegroundColorDone = ConsoleColor.DarkGreen,
    .ProgressCharacter = "─"c,
    .BackgroundColor = ConsoleColor.DarkGray,
    .CollapseWhenFinished = True
}
                Call Parallel.For(0, numberOfSteps, Sub(i)
                                                        Dim workBarOptions = New ProgressBarOptions With {
                                        .DenseProgressBar = True,
                                        .ForegroundColor = ConsoleColor.Yellow,
                                        .ProgressCharacter = "─"c,
                                        .BackgroundColor = ConsoleColor.DarkGray
                                    }
                                                        Dim childSteps = random.Next(1, 5)
														Using childProgress As ChildProgressBar = pbar.Spawn(childSteps, $"step {i} progress", stepBarOptions)
															Call Parallel.For(0, childSteps, Sub(ci)
																								 Dim childTicks = random.Next(50, 250)
																								 Using innerChildProgress = childProgress.Spawn(childTicks, $"step {i}::{ci} progress", workBarOptions)
																									 For r = 0 To childTicks - 1
																										 innerChildProgress.Tick()
																										 Program.BusyWait(50)
																									 Next
																								 End Using
																								 childProgress.Tick()
																							 End Sub)
														End Using

														pbar.Tick()
                                                    End Sub)
            End Using
            Return Task.FromResult(1)
        End Function
    End Class
End Namespace
