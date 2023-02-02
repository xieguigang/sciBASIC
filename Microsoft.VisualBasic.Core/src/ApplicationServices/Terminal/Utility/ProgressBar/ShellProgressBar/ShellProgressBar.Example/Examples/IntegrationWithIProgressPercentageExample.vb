Imports System
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.Examples
    Public Class IntegrationWithIProgressPercentageExample
        Implements IProgressBarExample
        Public Function Start(token As CancellationToken) As Task Implements IProgressBarExample.Start
            Using pbar = New ProgressBar(100, "A console progress that integrates with IProgress<float>")
                Call ProcessFiles(pbar.AsProgress(Of Single)())
            End Using
            Return Task.FromResult(1)
        End Function

        Public Shared Sub ProcessFiles(progress As IProgress(Of Single))
            Dim files = Enumerable.Range(1, 10).[Select](Function(e) New FileInfo($"Data{e:D2}.csv")).ToList()
            Dim i = 0
            For Each file In files
                DoWork(file)
                progress?.Report(Interlocked.Increment(i) / CSng(files.Count))
            Next
        End Sub

        Private Shared Sub DoWork(file As FileInfo)
            Thread.Sleep(200)
        End Sub
    End Class
End Namespace
