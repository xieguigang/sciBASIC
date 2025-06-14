#Region "Microsoft.VisualBasic::e2c4eef031f7a0234e523f719a17d063, Microsoft.VisualBasic.Core\src\ApplicationServices\Tools\TarGzFileSystem.vb"

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

    '   Total Lines: 176
    '    Code Lines: 128 (72.73%)
    ' Comment Lines: 9 (5.11%)
    '    - Xml Docs: 44.44%
    ' 
    '   Blank Lines: 39 (22.16%)
    '     File Size: 6.86 KB


    '     Class TarGzFileSystem
    ' 
    '         Properties: [readonly]
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CheckVirtualEntry, DeleteFile, EnumerateFiles, FileExists, FileModifyTime
    '                   FileSize, (+2 Overloads) GetFiles, GetFullPath, OpenFile, ReadAllText
    '                   WriteText
    ' 
    '         Sub: Close, Flush
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Formats.Tar
Imports System.IO
Imports System.IO.Compression
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq

Namespace ApplicationServices

#If NET8_0_OR_GREATER Then

    Public Class TarGzFileSystem : Implements IFileSystemEnvironment

        ReadOnly gz As GZipStream
        ReadOnly tar As TarReader
        ReadOnly tree As New FileSystemTree

        Public ReadOnly Property [readonly] As Boolean Implements IFileSystemEnvironment.readonly
            Get
                Return True
            End Get
        End Property

        Sub New(targz As String)
            Dim file As Stream = targz.Open(FileMode.Open, doClear:=False, [readOnly]:=True)

            gz = New GZipStream(file, CompressionMode.Decompress)
            tar = New TarReader(gz)

            ' load files
            Dim entry As New Value(Of TarEntry)

            Do While (entry = tar.GetNextEntry) IsNot Nothing
                If CheckVirtualEntry(entry) Then
                    Continue Do
                End If

                tree.AddFile("/" & CType(entry, TarEntry).Name).data = CType(entry, TarEntry)
            Loop
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function CheckVirtualEntry(entry As TarEntry) As Boolean
            Return entry.EntryType.HasFlag(TarEntryType.SymbolicLink) OrElse
                entry.EntryType.HasFlag(TarEntryType.HardLink) OrElse
                entry.EntryType.HasFlag(TarEntryType.GlobalExtendedAttributes)
        End Function

        Public Sub Close() Implements IFileSystemEnvironment.Close
            Call tar.Dispose()
            Call gz.Dispose()
        End Sub

        Public Sub Flush() Implements IFileSystemEnvironment.Flush
        End Sub

        Public Function OpenFile(path As String,
                                 Optional mode As FileMode = FileMode.OpenOrCreate,
                                 Optional access As FileAccess = FileAccess.Read) As Stream Implements IFileSystemEnvironment.OpenFile

            Dim entry As FileSystemTree = tree.GetFile(path)

            If entry Is Nothing Then
                Throw New MissingPrimaryKeyException(path)
            End If

            Dim tar As TarEntry = entry.data
            Return tar.DataStream
        End Function

        Public Function DeleteFile(path As String) As Boolean Implements IFileSystemEnvironment.DeleteFile
            Throw New NotSupportedException("Readonly stream!")
        End Function

        Public Function FileExists(path As String, Optional ZERO_Nonexists As Boolean = False) As Boolean Implements IFileSystemEnvironment.FileExists
            Dim entry As FileSystemTree = tree.GetFile(path)

            If entry Is Nothing Then
                Return False
            ElseIf ZERO_Nonexists Then
                Return DirectCast(entry.data, TarEntry).Length > 0
            Else
                Return True
            End If
        End Function

        Public Function FileSize(path As String) As Long Implements IFileSystemEnvironment.FileSize
            Dim entry As FileSystemTree = tree.GetFile(path)

            If entry Is Nothing Then
                Return -1
            Else
                Return DirectCast(entry.data, TarEntry).Length
            End If
        End Function

        Public Function FileModifyTime(path As String) As Date Implements IFileSystemEnvironment.FileModifyTime
            Dim entry As FileSystemTree = tree.GetFile(path)

            If entry Is Nothing Then
                Return Nothing
            Else
                Return DirectCast(entry.data, TarEntry).ModificationTime.LocalDateTime
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="filename">the relative file path of the target file/folder</param>
        ''' <returns></returns>
        Public Function GetFullPath(filename As String) As String Implements IFileSystemEnvironment.GetFullPath
            Dim entry As FileSystemTree = FileSystemTree.GetFile(tree, filename)

            If entry Is Nothing Then
                Return Nothing
            End If

            Return entry.ToString
        End Function

        Public Function WriteText(text As String, path As String) As Boolean Implements IFileSystemEnvironment.WriteText
            Throw New NotSupportedException("Readonly stream!")
        End Function

        Public Function ReadAllText(path As String) As String Implements IFileSystemEnvironment.ReadAllText
            Dim s As Stream = OpenFile(path, FileMode.Open, FileAccess.Read)
            Dim str As New StreamReader(s)

            Return str.ReadToEnd
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetFiles() As IEnumerable(Of String) Implements IFileSystemEnvironment.GetFiles
            Return tree.AsEnumerable
        End Function

        Public Function GetFiles(subdir As String, ParamArray exts() As String) As IEnumerable(Of String) Implements IFileSystemEnvironment.GetFiles
            Dim entry As FileSystemTree = FileSystemTree.GetFile(tree, subdir)

            ' virtual folder is not existsed
            If entry Is Nothing Then
                Return New String() {}
            End If

            With ls - ShellSyntax.wildcards(exts)
                Dim filter As Func(Of String, Boolean) = .MakeFilter
                Dim subset As IEnumerable(Of String) = entry.AsEnumerable.Where(filter)

                Return subset
            End With
        End Function

        Public Function EnumerateFiles(subdir As String, ParamArray exts() As String) As IEnumerable(Of String) Implements IFileSystemEnvironment.EnumerateFiles
            Dim entry As FileSystemTree = FileSystemTree.GetFile(tree, subdir)

            ' virtual folder is not existsed
            If entry Is Nothing Then
                Return New String() {}
            End If

            With ls - ShellSyntax.wildcards(exts)
                Dim filter As Func(Of String, Boolean) = .MakeFilter
                ' only scan current filesystem node
                Dim subset As IEnumerable(Of String) = From file As FileSystemTree
                                                       In entry.Files.Values
                                                       Where filter(file.Name)
                                                       Select file.FullName
                Return subset
            End With
        End Function
    End Class
#End If

End Namespace
