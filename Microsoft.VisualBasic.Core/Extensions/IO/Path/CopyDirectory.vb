#Region "Microsoft.VisualBasic::b62bde2bf71d64669151efc2987e6471, Microsoft.VisualBasic.Core\Extensions\IO\Path\CopyDirectory.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class CopyDirectoryAction
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CreateDestinationFolderAndReturnNewPath
    ' 
    '         Sub: Copy, CopyFilesToTargetDirectory, CopySubDirectoriesWithFiles
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Language.UnixBash

Namespace FileIO

    Public Class CopyDirectoryAction

        ReadOnly progress As IProgress(Of String)

        Sub New(progress As Progress(Of String))
            Me.progress = progress
        End Sub

        ''' <summary>
        ''' 这个函数会在这个模块内被递归调用
        ''' </summary>
        ''' <param name="src$"></param>
        ''' <param name="destination$"></param>
        Public Sub Copy(src$, destination$, Optional includeSrc As Boolean = True)
            Dim directory As New DirectoryInfo(src)

            If includeSrc Then
                If FileIO.Directory.Exists(Path.Combine(destination, directory.Name)) Then
                    Call $"Directory '{directory.Name}' already exists in '{destination}'".Warning
                End If

                destination = CreateDestinationFolderAndReturnNewPath(src, destination)
            Else
                If FileIO.Directory.Exists(destination) Then
                    Call $"Directory '{destination.DirectoryName}' already exists in '{destination}'".Warning
                End If
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

            ' 2017-12-24
            ' 因为只是复制当前的文件夹的文件，所以在这里就不需要添加-r递归参数了
            For Each path As String In ls - l - "*.*" <= src
                With New FileInfo(path)
                    resultFilePath = IO.Path.Combine(destinationFolder, IO.Path.GetFileName(.Name))
                End With

                Call progress.Report(resultFilePath)
                Call path.FileCopy(resultFilePath)
            Next
        End Sub

        Private Sub CopySubDirectoriesWithFiles(pathToSourceFolder As String, pathToDestinationFolder As String)
            For Each subDirectory As String In IO.Directory.GetDirectories(pathToSourceFolder)
                Copy(subDirectory, pathToDestinationFolder, includeSrc:=True)
            Next
        End Sub
    End Class
End Namespace
