Imports System
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.Net.WebClient

Module Program

    <STAThread>
    Async Sub Main(args As String())
        Await Download(App.CommandLine)
    End Sub

    ' url/files.txt [--filename <save_filename>] [--download_to <directory_path>]
    Private Async Function Download(args As CommandLine) As Task
        Dim target As String = args.Name
        Dim targetList As String()

        If target.FileExists Then
            Dim downloads As String = args("--download_to")

            ' is local file of target download file list
            targetList = target.ReadAllLines

            For Each url As String In targetList
                Await New Axel().Download(url, $"{downloads}/{url.FileName}")
            Next
        Else
            Dim filename As String = args("--filename")

            If filename.StringEmpty() Then
                filename = $"./{target.FileName}"
            End If

            Await New Axel().Download(target, filename)
        End If
    End Function
End Module
