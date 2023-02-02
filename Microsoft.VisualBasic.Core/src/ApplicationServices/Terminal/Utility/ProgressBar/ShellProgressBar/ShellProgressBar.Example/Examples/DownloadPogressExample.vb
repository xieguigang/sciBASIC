Imports System
Imports System.Linq
Imports System.Net
Imports System.Threading
Imports System.Threading.Tasks

Namespace ShellProgressBar.Example.Examples
    Public Class DownloadProgressExample
        Inherits ExampleBase
        Protected Overrides Function StartAsync() As Task
            Dim files = New String() {"https://github.com/Mpdreamz/shellprogressbar/archive/4.3.0.zip", "https://github.com/Mpdreamz/shellprogressbar/archive/4.2.0.zip", "https://github.com/Mpdreamz/shellprogressbar/archive/4.1.0.zip", "https://github.com/Mpdreamz/shellprogressbar/archive/4.0.0.zip"}
            Dim childOptions = New ProgressBarOptions With {
    .ForegroundColor = ConsoleColor.Yellow,
    .ProgressCharacter = "▓"c
}
            Dim pbar = New ProgressBar(files.Length, "downloading")
            For Each fileI In files.[Select](Function(f, i) (f, i))
                Dim file = fileI.Item1
                Dim i = fileI.Item2
                Dim data As Byte() = Nothing
                Dim child = pbar.Spawn(100, "page: " & i, childOptions)
                Try
                    Dim client = New WebClient()
                    AddHandler client.DownloadProgressChanged, Sub(o, args) child.Tick(args.ProgressPercentage)
                    AddHandler client.DownloadDataCompleted, Sub(o, args) data = args.Result
                    client.DownloadDataAsync(New Uri(file))
                    While client.IsBusy
                        Thread.Sleep(1)
                    End While

                    pbar.Tick()
                Catch [error] As WebException
                    pbar.WriteLine([error].Message)
                End Try
            Next

            Return Task.CompletedTask
        End Function
    End Class
End Namespace
