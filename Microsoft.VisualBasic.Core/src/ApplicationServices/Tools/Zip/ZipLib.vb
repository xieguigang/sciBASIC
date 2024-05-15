#Region "Microsoft.VisualBasic::34fba8a07a7025d9d94f7c1cfedb9723, Microsoft.VisualBasic.Core\src\ApplicationServices\Tools\Zip\ZipLib.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 356
    '    Code Lines: 188
    ' Comment Lines: 119
    '   Blank Lines: 49
    '     File Size: 15.47 KB


    '     Module ZipLib
    ' 
    '         Function: CheckValidZipFile, IsADirectoryEntry, (+2 Overloads) IsSourceFolderZip
    ' 
    '         Sub: AddToArchive, AppendZip, DeleteItems, DirectoryArchive, FileArchive
    '              WriteFiles
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.IO.Compression
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace ApplicationServices.Zip

#If NET_40 = 0 Then

    ''' <summary>
    ''' Creating Zip Files Easily in .NET 4.5
    ''' Tim Corey, 11 May 2012
    ''' 
    ''' http://www.codeproject.com/Articles/381661/Creating-Zip-Files-Easily-in-NET
    ''' </summary>
    ''' <remarks></remarks>
    ''' 
    <Package("IO.ZIP", Description:="Creating Zip Files Easily in .NET 4.6",
         Publisher:="Tim Corey",
         Url:="http://www.codeproject.com/Articles/381661/Creating-Zip-Files-Easily-in-NET")>
    Public Module ZipLib

        ''' <summary>
        ''' Check if the given <paramref name="zip"/> file contains any data. 
        ''' </summary>
        ''' <param name="zip"></param>
        ''' <returns></returns>
        Public Function CheckValidZipFile(zip As String) As Boolean
            Dim result As Boolean = False

            If zip.FileExists(True) Then
                Try
                    Using archive As ZipArchive = ZipFile.OpenRead(zip)
                        result = archive.Entries.Count > 0
                    End Using
                Catch ex As Exception
                    result = False
                End Try
            End If

            Return result
        End Function

        ''' <summary>
        ''' 判断目标zip文件是否是直接将文件夹进行压缩的
        ''' 如果是直接将文件夹压缩的，那么肯定会在每一个entry的起始存在一个共同的文件夹名
        ''' 例如：
        ''' 
        ''' ```
        ''' 95-1.D/
        ''' 95-1.D/AcqData/
        ''' 95-1.D/AcqData/Contents.xml
        ''' 95-1.D/AcqData/Devices.xml
        ''' ```
        ''' </summary>
        ''' <param name="zip"></param>
        ''' <returns></returns>
        <Extension>
        Public Function IsSourceFolderZip(zip$, Optional ByRef folder$ = Nothing) As Boolean
            Using file As Stream = zip.Open(FileMode.Open, doClear:=False, [readOnly]:=True)
                Return file.IsSourceFolderZip(folder)
            End Using
        End Function

        ''' <summary>
        ''' 判断目标zip文件是否是直接将文件夹进行压缩的
        ''' 如果是直接将文件夹压缩的，那么肯定会在每一个entry的起始存在一个共同的文件夹名
        ''' 例如：
        ''' 
        ''' ```
        ''' 95-1.D/
        ''' 95-1.D/AcqData/
        ''' 95-1.D/AcqData/Contents.xml
        ''' 95-1.D/AcqData/Devices.xml
        ''' ```
        ''' </summary>
        ''' <param name="zip"></param>
        ''' <returns></returns>
        <Extension>
        Public Function IsSourceFolderZip(zip As Stream,
                                          Optional ByRef folder$ = Nothing,
                                          Optional reset As Boolean = False) As Boolean

            Using archive As ZipArchive = New ZipArchive(zip, ZipArchiveMode.Read)
                Dim fileNames = archive.Entries _
                    .Where(Function(e) Not e.IsADirectoryEntry) _
                    .Select(Function(file) file.FullName) _
                    .ToArray
                Dim rootDir = archive.Entries _
                    .Where(Function(e) e.IsADirectoryEntry) _
                    .Select(Function(d) d.FullName) _
                    .OrderBy(Function(d) d.Length) _
                    .FirstOrDefault

                If rootDir.StringEmpty OrElse rootDir = "/" OrElse rootDir = "\" Then
                    ' 没有root文件夹，说明不是
                    Return False
                End If

                If fileNames.All(Function(path) path.StartsWith(rootDir)) Then
                    folder = rootDir
                Else
                    folder = Nothing
                End If

                If reset Then
                    Call zip.Seek(Scan0, SeekOrigin.Begin)
                End If

                Return Not folder Is Nothing
            End Using
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function IsADirectoryEntry(file As ZipArchiveEntry) As Boolean
            Return file.FullName.Last = "/"c OrElse file.FullName.Last = "\"c
        End Function

        <ExportAPI("File.Zip")>
        Public Sub FileArchive(file$, SaveZip$,
                               Optional action As ArchiveAction = ArchiveAction.Replace,
                               Optional fileOverwrite As Overwrite = Overwrite.IfNewer,
                               Optional compression As CompressionLevel = CompressionLevel.Optimal)

            Call SaveZip.ParentPath.MakeDir
            Call {file}.AddToArchive(SaveZip, action, fileOverwrite, compression)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="directory$"></param>
        ''' <param name="saveZip$"></param>
        ''' <param name="action"></param>
        ''' <param name="fileOverwrite"></param>
        ''' <param name="compression"></param>
        ''' <param name="flatDirectory">
        ''' 当这个参数为FALSE的时候，zip文件之中会保留有原来的文件夹的树形结构，
        ''' 反之，则zip文件之中不会存在任何文件夹结构，所有的文件都会被保存在zip文件里面的根目录之中
        ''' 
        ''' 这个参数默认为False，即保留有原来的文件夹树形结构
        ''' </param>
        <ExportAPI("DIR.Zip")>
        Public Sub DirectoryArchive(directory$, saveZip$,
                                    Optional action As ArchiveAction = ArchiveAction.Replace,
                                    Optional fileOverwrite As Overwrite = Overwrite.IfNewer,
                                    Optional compression As CompressionLevel = CompressionLevel.Optimal,
                                    Optional flatDirectory As Boolean = False)

            ' 2018-7-28 如果rel是空字符串
            ' 那么再压缩函数之中只会将文件名作为entry，即实现无文件树的效果
            ' 反之会使用相对路径生成文件树，即树状的非flat结构
            Dim rel$ = directory Or "".When(flatDirectory)

            If Not rel.StringEmpty Then
                rel = rel.GetDirectoryFullPath
            End If

            Call saveZip.ParentPath.MakeDir
            Call (ls - l - r - "*.*" <= directory) _
                .AddToArchive(
                    archiveFullName:=saveZip,
                    action:=action,
                    fileOverwrite:=fileOverwrite,
                    compression:=compression,
                    relativeDir:=rel.Replace("/"c, "\"c).Trim("\"c)
                 )
        End Sub

        ''' <summary>
        ''' Allows you to add files to an archive, whether the archive
        ''' already exists or not
        ''' </summary>
        ''' <param name="archiveFullName">
        ''' The name of the archive to you want to add your files to
        ''' </param>
        ''' <param name="files">
        ''' A set of file names that are to be added
        ''' </param>
        ''' <param name="action">
        ''' Specifies how we are going to handle an existing archive
        ''' </param>
        ''' <param name="compression">
        ''' Specifies what type of compression to use - defaults to Optimal
        ''' </param>
        ''' 
        <Extension>
        Public Sub AddToArchive(<Parameter("files", "A set of file names that are to be added")> files As IEnumerable(Of String),
                                <Parameter("Zip", "The name of the archive to you want to add your files to")> archiveFullName$,
                                Optional action As ArchiveAction = ArchiveAction.Replace,
                                Optional fileOverwrite As Overwrite = Overwrite.IfNewer,
                                Optional compression As CompressionLevel = CompressionLevel.Optimal,
                                Optional relativeDir$ = Nothing)

            'Identifies the mode we will be using - the default is Create
            Dim mode As ZipArchiveMode = ZipArchiveMode.Create
            'Determines if the zip file even exists
            Dim archiveExists As Boolean = IO.File.Exists(archiveFullName)

            Call archiveFullName.ParentPath.MakeDir

            'Figures out what to do based upon our specified overwrite method
            Select Case action
                Case ArchiveAction.Merge
                    'Sets the mode to update if the file exists, otherwise
                    'the default of Create is fine
                    If archiveExists Then
                        mode = ZipArchiveMode.Update
                    End If

                Case ArchiveAction.Replace
                    'Deletes the file if it exists.  Either way, the default
                    'mode of Create is fine
                    If archiveExists Then
                        IO.File.Delete(archiveFullName)
                    End If

                Case ArchiveAction.[Error]
                    'Throws an error if the file exists
                    If archiveExists Then
                        Throw New IOException($"The zip file {archiveFullName.ToFileURL.CLIPath} already exists.")
                    End If

                Case ArchiveAction.Ignore
                    'Closes the method silently and does nothing
                    If archiveExists Then
                        Return
                    End If

                Case Else

            End Select

            ' Opens the zip file in the mode we specified
            Using zipFile As ZipArchive = IO.Compression.ZipFile.Open(archiveFullName, mode)
                Call zipFile.WriteFiles(files.ToArray, mode, fileOverwrite, compression, relativeDir)
            End Using
        End Sub

        ''' <summary>
        ''' add files into the zip file archive object
        ''' </summary>
        ''' <param name="zipFile"></param>
        ''' <param name="files">
        ''' a file path list to save into the zip package file, should be full path!
        ''' </param>
        ''' <param name="mode"></param>
        ''' <param name="fileOverwrite"></param>
        ''' <param name="compression"></param>
        ''' <param name="relativeDir"></param>
        ''' <param name="parent$"></param>
        <Extension>
        Public Sub WriteFiles(zipFile As ZipArchive, files As String(), mode As ZipArchiveMode,
                              Optional fileOverwrite As Overwrite = Overwrite.IfNewer,
                              Optional compression As CompressionLevel = CompressionLevel.Optimal,
                              Optional relativeDir$ = Nothing,
                              Optional parent$ = "")
            Dim entryName$
            Dim appendParent As Boolean = Not parent.StringEmpty

            'This is a bit of a hack and should be refactored - I am
            'doing a similar foreach loop for both modes, but for Create
            'I am doing very little work while Update gets a lot of
            'code.  This also does not handle any other mode (of
            'which there currently wouldn't be one since we don't
            'use Read here).

            If mode = ZipArchiveMode.Create Then
                For Each path As String In files
                    ' Adds the file to the archive
                    If relativeDir.StringEmpty Then
                        entryName = IO.Path.GetFileName(path)
                    Else
                        entryName = RelativePath(relativeDir, path, appendParent:=False, fixZipPath:=True)
                    End If

                    If appendParent Then
                        entryName = $"{parent}/{entryName}"
                    End If

                    Call zipFile.CreateEntryFromFile(path, entryName, compression)
                Next
            Else
                Call zipFile.AppendZip(files, fileOverwrite, compression)
            End If
        End Sub

        <Extension>
        Public Sub DeleteItems(zip As ZipArchive, itemNames As Index(Of String))
            For Each file As ZipArchiveEntry In zip.Entries.ToArray
                If file.Name Like itemNames Then
                    Call file.Delete()
                End If
            Next
        End Sub

        <Extension>
        Private Sub AppendZip(zipFile As ZipArchive, files As IEnumerable(Of String), fileOverwrite As Overwrite, compression As CompressionLevel)
            For Each path As String In files
                Dim pathFileName = IO.Path.GetFileName(path)
                Dim fileInZip = (From f In zipFile.Entries Where f.Name = pathFileName).FirstOrDefault()

                Select Case fileOverwrite
                    Case Overwrite.Always

                        'Deletes the file if it is found
                        If fileInZip IsNot Nothing Then
                            fileInZip.Delete()
                        End If

                        'Adds the file to the archive
                        zipFile.CreateEntryFromFile(path, pathFileName, compression)

                    Case Overwrite.IfNewer

                        'This is a bit trickier - we only delete the file if it is
                        'newer, but if it is newer or if the file isn't already in
                        'the zip file, we will write it to the zip file

                        If fileInZip IsNot Nothing Then

                            'Deletes the file only if it is older than our file.
                            'Note that the file will be ignored if the existing file
                            'in the archive is newer.
                            If fileInZip.LastWriteTime < IO.File.GetLastWriteTime(path) Then
                                fileInZip.Delete()

                                'Adds the file to the archive
                                zipFile.CreateEntryFromFile(path, IO.Path.GetFileName(path), compression)
                            End If
                        Else
                            'The file wasn't already in the zip file so add it to the archive
                            zipFile.CreateEntryFromFile(path, IO.Path.GetFileName(path), compression)
                        End If

                    Case Overwrite.Never

                        'Don't do anything - this is a decision that you need to
                        'consider, however, since this will mean that no file will
                        'be writte.  You could write a second copy to the zip with
                        'the same name (not sure that is wise, however).

                    Case Else

                End Select
            Next
        End Sub
    End Module
#End If
End Namespace
