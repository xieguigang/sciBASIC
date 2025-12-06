Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.Net.WebClient

<CLI>
Module Program

    <STAThread>
    Public Function Main(args As String()) As Integer
        Return GetType(Program).RunCLI(App.Command, executeFile:=AddressOf Download)
    End Function

    ' url/files.txt [--filename <save_filename>] [--download_to <directory_path>]
    Private Function Download(target As String, args As CommandLine) As Integer
        If target.FileExists Then
            Dim downloads As String = args("--download_to")

            ' is local file of target download file list
            For Each url As String In target.ReadAllLines
                Call New Axel().Download(url, $"{downloads}/{url.FileName}").Wait()
            Next
        Else
            Dim filename As String = args("--filename")

            If filename.StringEmpty() Then
                filename = $"./{target.FileName}"
            End If

            Call New Axel().Download(target, filename).Wait()
        End If

        Return 0
    End Function
End Module
