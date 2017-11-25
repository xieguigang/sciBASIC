Imports System.IO
Imports Microsoft.VisualBasic.Language.UnixBash

Namespace FileIO

    Public Class CopyDirectoryAction

        ReadOnly progress As IProgress(Of String)

        Sub New(progress As Progress(Of String))
            Me.progress = progress
        End Sub

        ''' <summary>
        ''' 将源文件夹中的所有内容都复制到目标文件夹之中
        ''' </summary>
        ''' <param name="src$"></param>
        ''' <param name="destination$"></param>
        Public Sub Copy(src$, destination$)
            Dim directory As New DirectoryInfo(src)

            If FileIO.Directory.Exists(Path.Combine(destination, directory.Name)) Then
                Throw New IOException($"Directory '{directory.Name}' already exists in '{destination}'")
            Else
                destination = CreateDestinationFolderAndReturnNewPath(src, destination)
            End If

            Call CopyFilesToTargetDirectory(src, destination)
            Call CopySubDirectoriesWithFiles(src, destination)
        End Sub

        Private Shared Function CreateDestinationFolderAndReturnNewPath(src As String, destinationFolder$) As String
            Dim directory As New DirectoryInfo(src)
            Dim path = IO.Path.Combine(destinationFolder, directory.Name)
            Call path.MkDIR
            Return path
        End Function

        Private Sub CopyFilesToTargetDirectory(src As String, destinationFolder$)
            Dim resultFilePath$

            For Each path As String In ls - l - r - "*.*" <= src
                With New FileInfo(path)
                    resultFilePath = IO.Path.Combine(destinationFolder, IO.Path.GetFileName(.Name))
                End With

                Call progress.Report(resultFilePath)
                Call File.Copy(path, resultFilePath)
            Next
        End Sub

        Private Sub CopySubDirectoriesWithFiles(pathToSourceFolder As String, pathToDestinationFolder As String)
            For Each subDirectory As String In IO.Directory.GetDirectories(pathToSourceFolder)
                Copy(subDirectory, pathToDestinationFolder)
            Next
        End Sub
    End Class
End Namespace