Imports System
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.TestCases
    Public Class NestedProgressBarPerStepProgress
        Implements IProgressBarExample
        Public Function Start(token As CancellationToken) As Task Implements IProgressBarExample.Start
            Dim outerTicks = 10
            Using pbar = New ProgressBar(outerTicks, "outer progress", ConsoleColor.Cyan)
                For i = 0 To outerTicks - 1
                    InnerProgressBars(pbar)
                    pbar.Tick()
                Next
            End Using
            Return Task.FromResult(1)
        End Function

        Private Shared Sub InnerProgressBars(pbar As ProgressBar)
            Dim lInnerProgressBars = Enumerable.Range(0, New Random().Next(2, 6)).[Select](Function(s) pbar.Spawn(New Random().Next(2, 5), $"inner bar {s}")).ToList()

            Dim maxTicks = lInnerProgressBars.Max(Function(p) p.MaxTicks)

            For ii = 0 To maxTicks - 1
                For Each p In lInnerProgressBars
                    InnerInnerProgressBars(p)
                    p.Tick()
                Next


                Thread.Sleep(4)
            Next
            For Each p In lInnerProgressBars
                p.Dispose()
            Next
        End Sub

        Private Shared Sub InnerInnerProgressBars(pbar As ChildProgressBar)
            Dim progressBarOption = New ProgressBarOptions With {
                .ForegroundColor = ConsoleColor.Yellow
            }
            Dim innerProgressBars = Enumerable.Range(0, New Random().Next(1, 3)).[Select](Function(s) pbar.Spawn(New Random().Next(5, 10), $"inner bar {s}", progressBarOption)).ToList()
            If Not innerProgressBars.Any() Then Return

            Dim maxTicks = innerProgressBars.Max(Function(p) p.MaxTicks)

            For ii = 0 To maxTicks - 1
                For Each p In innerProgressBars
                    p.Tick()
                Next
            Next
            For Each p In innerProgressBars
                p.Dispose()
            Next
        End Sub
    End Class
End Namespace
