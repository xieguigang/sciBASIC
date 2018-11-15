#Region "Microsoft.VisualBasic::e2155037614418c1d001a820e64c094c, Microsoft.VisualBasic.Core\ApplicationServices\ZipLib.vb"

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

    '     Module GZip
    ' 
    ' 
    '         Enum Overwrite
    ' 
    '             Always, IfNewer, Never
    ' 
    ' 
    ' 
    '         Enum ArchiveAction
    ' 
    '             [Error], Ignore, Merge, Replace
    ' 
    '  
    ' 
    ' 
    ' 
    '  
    ' 
    '     Function: ExtractToSelfDirectory, IsADirectoryEntry, IsSourceFolderZip
    ' 
    '     Sub: AddToArchive, DirectoryArchive, ExtractToFileInternal, FileArchive, ImprovedExtractToDirectory
    '          ImprovedExtractToFile
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.IO.Compression
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace ApplicationServices

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
        ''' Used to specify what our overwrite policy
        ''' is for files we are extracting.
        ''' </summary>
        Public Enum Overwrite
            Always
            IfNewer
            Never
        End Enum

        ''' <summary>
        ''' Used to identify what we will do if we are
        ''' trying to create a zip file and it already
        ''' exists.
        ''' </summary>
        Public Enum ArchiveAction
            Merge
            Replace
            [Error]
            Ignore
        End Enum

        ''' <summary>
        ''' Unzips the specified file to the given folder in a safe
        ''' manner.  This plans for missing paths and existing files
        ''' and handles them gracefully.
        ''' </summary>
        ''' <param name="sourceArchiveFileName">
        ''' The name of the zip file to be extracted
        ''' </param>
        ''' <param name="destinationDirectoryName">
        ''' The directory to extract the zip file to
        ''' </param>
        ''' <param name="overwriteMethod">
        ''' Specifies how we are going to handle an existing file.
        ''' The default is IfNewer.
        ''' </param>
        ''' 
        <ExportAPI("ExtractToDir", Info:="Unzips the specified file to the given folder in a safe manner. This plans for missing paths and existing files and handles them gracefully.")>
        Public Sub ImprovedExtractToDirectory(<Parameter("Zip", "The name of the zip file to be extracted")> sourceArchiveFileName$,
                                              <Parameter("Dir", "The directory to extract the zip file to")> destinationDirectoryName$,
                                              <Parameter("Overwrite.HowTo", "Specifies how we are going to handle an existing file. The default is IfNewer.")>
                                              Optional overwriteMethod As Overwrite = Overwrite.IfNewer,
                                              Optional extractToFlat As Boolean = False)

            ' Opens the zip file up to be read
            Using archive As ZipArchive = ZipFile.OpenRead(sourceArchiveFileName)

                Dim rootDir$ = Nothing
                Dim isFolderArchive = sourceArchiveFileName.IsSourceFolderZip(folder:=rootDir)
                Dim fullName$

                ' Loops through each file in the zip file
                For Each file As ZipArchiveEntry In archive.Entries
                    If extractToFlat AndAlso isFolderArchive Then
                        fullName = file.FullName.Replace(rootDir, "")
                    Else
                        fullName = file.FullName
                    End If

                    If file.IsADirectoryEntry Then
                        Call Path _
                            .Combine(destinationDirectoryName, fullName) _
                            .MkDIR
                    Else
                        Call file.ExtractToFileInternal(
                            destinationPath:=destinationDirectoryName,
                            overwriteMethod:=overwriteMethod,
                            overridesFullName:=fullName
                        )
                    End If
                Next
            End Using
        End Sub

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
            Using archive As ZipArchive = ZipFile.OpenRead(zip)
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

                Return Not folder Is Nothing
            End Using
        End Function

        Public Function ExtractToSelfDirectory(zip$, Optional overwriteMethod As Overwrite = Overwrite.IfNewer) As String
            Dim Dir As String = FileIO.FileSystem.GetParentPath(zip)
            Dim Name As String = BaseName(zip)
            Dir = Dir & "/" & Name
            Call ImprovedExtractToDirectory(zip, Dir, overwriteMethod)

            Return Dir
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function IsADirectoryEntry(file As ZipArchiveEntry) As Boolean
            Return file.FullName.Last = "/"c OrElse file.FullName.Last = "\"c
        End Function

        <Extension> Private Sub ExtractToFileInternal(file As ZipArchiveEntry, destinationPath$, overwriteMethod As Overwrite, overridesFullName$)
            ' Gets the complete path for the destination file, including any
            ' relative paths that were in the zip file
            Dim destinationFileName As String = Path.Combine(destinationPath, overridesFullName Or file.FullName.AsDefault)

            ' Gets just the new path, minus the file name so we can create the
            ' directory if it does not exist
            Dim destinationFilePath As String = Path.GetDirectoryName(destinationFileName)

            ' Creates the directory (if it doesn't exist) for the new path
            ' 2018-2-2 在原先的代码之中直接使用CreateDirectory，如果目标文件夹存在的话会报错
            ' 在这里使用安全一点的mkdir函数
            Call destinationFilePath.MkDIR(throwEx:=False)

            ' Determines what to do with the file based upon the
            ' method of overwriting chosen
            Select Case overwriteMethod
                Case Overwrite.Always

                    ' Just put the file in and overwrite anything that is found
                    file.ExtractToFile(destinationFileName, True)

                Case Overwrite.IfNewer
                    ' Checks to see if the file exists, and if so, if it should
                    ' be overwritten
                    If Not IO.File.Exists(destinationFileName) OrElse IO.File.GetLastWriteTime(destinationFileName) < file.LastWriteTime Then
                        ' Either the file didn't exist or this file is newer, so
                        ' we will extract it and overwrite any existing file
                        file.ExtractToFile(destinationFileName, True)
                    End If

                Case Overwrite.Never
                    ' Put the file in if it is new but ignores the 
                    ' file if it already exists
                    If Not IO.File.Exists(destinationFileName) Then
                        file.ExtractToFile(destinationFileName)
                    End If

                Case Else
            End Select
        End Sub

        ''' <summary>
        ''' Safely extracts a single file from a zip file
        ''' </summary>
        ''' <param name="file">
        ''' The zip entry we are pulling the file from
        ''' </param>
        ''' <param name="destinationPath">
        ''' The root of where the file is going
        ''' </param>
        ''' <param name="overwriteMethod">
        ''' Specifies how we are going to handle an existing file.
        ''' The default is Overwrite.IfNewer.
        ''' </param>
        ''' 
        <ExportAPI("Extract", Info:="Safely extracts a single file from a zip file.")>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Sub ImprovedExtractToFile(<Parameter("Zip.Entry", "The zip entry we are pulling the file from")>
                                                     file As ZipArchiveEntry,
                                                     destinationPath$,
                                                     Optional overwriteMethod As Overwrite = Overwrite.IfNewer)
            Call file.ExtractToFileInternal(destinationPath, overwriteMethod, Nothing)
        End Sub

        <ExportAPI("File.Zip")>
        Public Sub FileArchive(file$, SaveZip$,
                               Optional action As ArchiveAction = ArchiveAction.Replace,
                               Optional fileOverwrite As Overwrite = Overwrite.IfNewer,
                               Optional compression As CompressionLevel = CompressionLevel.Optimal)

            Call SaveZip.ParentPath.MkDIR
            Call {file}.AddToArchive(SaveZip, action, fileOverwrite, compression)
        End Sub

        <ExportAPI("DIR.Zip")>
        Public Sub DirectoryArchive(DIR$, saveZip$,
                                    Optional action As ArchiveAction = ArchiveAction.Replace,
                                    Optional fileOverwrite As Overwrite = Overwrite.IfNewer,
                                    Optional compression As CompressionLevel = CompressionLevel.Optimal,
                                    Optional flatDirectory As Boolean = False)

            ' 2018-7-28 如果rel是空字符串
            ' 那么再压缩函数之中只会将文件名作为entry，即实现无文件树的效果
            ' 反之会使用相对路径生成文件树，即树状的非flat结构
            Dim rel$ = DIR Or "".When(flatDirectory)

            If Not rel.StringEmpty Then
                rel = rel.GetDirectoryFullPath
            End If

            Call saveZip.ParentPath.MkDIR
            Call (ls - l - r - "*.*" <= DIR) _
                .AddToArchive(
                    archiveFullName:=saveZip,
                    action:=action,
                    fileOverwrite:=fileOverwrite,
                    compression:=compression,
                    relativeDIR:=rel.Replace("/"c, "\"c).Trim("\"c)
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
        <ExportAPI("Zip.Add.Files", Info:="Allows you to add files to an archive, whether the archive already exists or not")>
        <Extension>
        Public Sub AddToArchive(<Parameter("files", "A set of file names that are to be added")> files As IEnumerable(Of String),
                                <Parameter("Zip", "The name of the archive to you want to add your files to")> archiveFullName$,
                                Optional action As ArchiveAction = ArchiveAction.Replace,
                                Optional fileOverwrite As Overwrite = Overwrite.IfNewer,
                                Optional compression As CompressionLevel = CompressionLevel.Optimal,
                                Optional relativeDIR$ = Nothing)

            'Identifies the mode we will be using - the default is Create
            Dim mode As ZipArchiveMode = ZipArchiveMode.Create

            'Determines if the zip file even exists
            Dim archiveExists As Boolean = IO.File.Exists(archiveFullName)

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

            'Opens the zip file in the mode we specified
            Using zipFile As ZipArchive = IO.Compression.ZipFile.Open(archiveFullName, mode)

                'This is a bit of a hack and should be refactored - I am
                'doing a similar foreach loop for both modes, but for Create
                'I am doing very little work while Update gets a lot of
                'code.  This also does not handle any other mode (of
                'which there currently wouldn't be one since we don't
                'use Read here).

                If mode = ZipArchiveMode.Create Then
                    Dim entryName$

                    For Each path As String In files
                        ' Adds the file to the archive
                        If relativeDIR.StringEmpty Then
                            entryName = IO.Path.GetFileName(path)
                        Else
                            entryName = RelativePath(relativeDIR, path, appendParent:=False)
                        End If

                        Call zipFile.CreateEntryFromFile(path, entryName, compression)
                    Next
                Else
                    For Each path As String In files
                        Dim fileInZip = (From f In zipFile.Entries Where f.Name = IO.Path.GetFileName(path)).FirstOrDefault()

                        Select Case fileOverwrite
                            Case Overwrite.Always

                                'Deletes the file if it is found
                                If fileInZip IsNot Nothing Then
                                    fileInZip.Delete()
                                End If

                                'Adds the file to the archive
                                zipFile.CreateEntryFromFile(path, IO.Path.GetFileName(path), compression)

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
                End If
            End Using
        End Sub

        '<Example.Code>     

        'Imports System.Collections.Generic
        'Imports System.IO
        'Imports System.IO.Compression
        'Imports Helpers

        '	'Example 1
        '	Private Shared Sub SimpleZip(dirToZip As String, zipName As String)
        '		ZipFile.CreateFromDirectory(dirToZip, zipName)
        '	End Sub

        '	'Example 2
        '	Private Shared Sub SimpleZip(dirToZip As String, zipName As String, compression As CompressionLevel, includeRoot As Boolean)
        '		ZipFile.CreateFromDirectory(dirToZip, zipName, compression, includeRoot)
        '	End Sub

        '	'Example 3
        '	Private Shared Sub SimpleUnzip(zipName As String, dirToUnzipTo As String)
        '		ZipFile.ExtractToDirectory(zipName, dirToUnzipTo)
        '	End Sub

        '	'Example 4
        '	Private Shared Sub SmarterUnzip(zipName As String, dirToUnzipTo As String)
        '		'This stores the path where the file should be unzipped to,
        '		'including any subfolders that the file was originally in.
        '		Dim fileUnzipFullPath As String

        '		'This is the full name of the destination file including
        '		'the path
        '		Dim fileUnzipFullName As String

        '		'Opens the zip file up to be read
        '		Using archive As ZipArchive = ZipFile.OpenRead(zipName)
        '			'Loops through each file in the zip file
        '			For Each file As ZipArchiveEntry In archive.Entries
        '				'Outputs relevant file information to the console
        '				Console.WriteLine("File Name: {0}", file.Name)
        '				Console.WriteLine("File Size: {0} bytes", file.Length)
        '				Console.WriteLine("Compression Ratio: {0}", (CDbl(file.CompressedLength) / file.Length).ToString("0.0%"))

        '				'Identifies the destination file name and path
        '				fileUnzipFullName = Path.Combine(dirToUnzipTo, file.FullName)

        '				'Extracts the files to the output folder in a safer manner
        '				If Not System.IO.File.Exists(fileUnzipFullName) Then
        '					'Calculates what the new full path for the unzipped file should be
        '					fileUnzipFullPath = Path.GetDirectoryName(fileUnzipFullName)

        '					'Creates the directory (if it doesn't exist) for the new path
        '					Directory.CreateDirectory(fileUnzipFullPath)

        '					'Extracts the file to (potentially new) path
        '					file.ExtractToFile(fileUnzipFullName)
        '				End If
        '			Next
        '		End Using
        '	End Sub

        '	'Example 5
        '	Private Shared Sub ManuallyCreateZipFile(zipName As String)
        '		'Creates a new, blank zip file to work with - the file will be
        '		'finalized when the using statement completes
        '		Using newFile As ZipArchive = ZipFile.Open(zipName, ZipArchiveMode.Create)
        '			'Here are two hard-coded files that we will be adding to the zip
        '			'file.  If you don't have these files in your system, this will
        '			'fail.  Either create them or change the file names.
        '			newFile.CreateEntryFromFile("C:\Temp\File1.txt", "File1.txt")
        '			newFile.CreateEntryFromFile("C:\Temp\File2.txt", "File2.txt", CompressionLevel.Fastest)
        '		End Using
        '	End Sub

        '	'Example 6
        '	Private Shared Sub ManuallyUpdateZipFile(zipName As String)
        '		'Opens the existing file like we opened the new file (just changed
        '		'the ZipArchiveMode to Update
        '		Using modFile As ZipArchive = ZipFile.Open(zipName, ZipArchiveMode.Update)
        '			'Here are two hard-coded files that we will be adding to the zip
        '			'file.  If you don't have these files in your system, this will
        '			'fail.  Either create them or change the file names.  Also, note
        '			'that their names are changed when they are put into the zip file.
        '			modFile.CreateEntryFromFile("C:\Temp\File1.txt", "File10.txt")

        '				'We could also add the code from Example 4 here to read
        '				'the contents of the open zip file as well.
        '			modFile.CreateEntryFromFile("C:\Temp\File2.txt", "File20.txt", CompressionLevel.Fastest)
        '		End Using
        '	End Sub

        '	'Example 7
        '	Private Shared Sub CallingImprovedExtractToDirectory(zipName As String, dirToUnzipTo As String)
        '		'This performs a similar function to Example 3, only now we are doing it
        '		'safely (we won't crash because of predictable and preventable errors). 
        '		'The result is something you don't have to think about - it just works.
        '		Compression.ImprovedExtractToDirectory(zipName, dirToUnzipTo, Compression.Overwrite.IfNewer)
        '	End Sub

        '	'Example 8
        '	Private Shared Sub CallingImprovedExtractToFile(zipName As String, dirToUnzipTo As String)
        '		'Opens the zip file up to be read
        '		Using archive As ZipArchive = ZipFile.OpenRead(zipName)
        '			'Loops through each file in the zip file
        '			For Each file As ZipArchiveEntry In archive.Entries
        '				'Outputs relevant file information to the console
        '				Console.WriteLine("File Name: {0}", file.Name)
        '				Console.WriteLine("File Size: {0} bytes", file.Length)
        '				Console.WriteLine("Compression Ratio: {0}", (CDbl(file.CompressedLength) / file.Length).ToString("0.0%"))

        '				'This is the new call
        '				Compression.ImprovedExtractToFile(file, dirToUnzipTo, Compression.Overwrite.Always)
        '			Next
        '		End Using
        '	End Sub

        '	'Example 9
        '	Private Shared Sub CallingAddToArchive(zipName As String)
        '		'This creates our list of files to be added
        '		Dim filesToArchive As New List(Of String)()

        '		'Here we are adding two hard-coded files to our list
        '		filesToArchive.Add("C:\Temp\File1.txt")
        '		filesToArchive.Add("C:\Temp\File2.txt")

        '		Compression.AddToArchive(zipName, filesToArchive, Compression.ArchiveAction.Replace, Compression.Overwrite.IfNewer, CompressionLevel.Optimal)
        '	End Sub
    End Module
#End If
End Namespace
