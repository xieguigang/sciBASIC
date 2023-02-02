Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.Examples
    Public Class IntegrationWithIProgressExample
        Implements IProgressBarExample
        Public Function Start(token As CancellationToken) As Task Implements IProgressBarExample.Start
            Dim files = Enumerable.Range(1, 10).[Select](Function(e) New FileInfo($"Data{e:D2}.csv")).ToList()
            Using pbar = New ProgressBar(files.Count, "A console progress that integrates with IProgress<T>")
                ProcessFiles(files, pbar.AsProgress(Of FileInfo)(Function(e) $"Processed {e.Name}"))
            End Using
            Return Task.FromResult(1)
        End Function

        Public Shared Sub ProcessFiles(files As IEnumerable(Of FileInfo), progress As IProgress(Of FileInfo))
            For Each file In files
                DoWork(file)
                progress?.Report(file)
            Next
        End Sub

        Private Shared Sub DoWork(file As FileInfo)
            Thread.Sleep(200)
        End Sub
    End Class
End Namespace
