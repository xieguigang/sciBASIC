Imports System.Formats.Tar
Imports System.IO
Imports System.IO.Compression
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
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

        Public Function GetFullPath(filename As String) As String Implements IFileSystemEnvironment.GetFullPath
            Dim entry As FileSystemTree = tree.GetFile(filename)

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
    End Class
#End If

End Namespace