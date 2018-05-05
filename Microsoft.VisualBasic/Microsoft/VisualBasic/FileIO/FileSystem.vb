Imports Microsoft.VisualBasic.CompilerServices
Imports System
Imports System.Collections
Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.ComponentModel
Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Security
Imports System.Security.Permissions
Imports System.Text

Namespace Microsoft.VisualBasic.FileIO
    <HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)>
    Public Class FileSystem
        ' Methods
        Private Shared Sub AddToStringCollection(ByVal StrCollection As Collection(Of String), ByVal StrArray As String())
            If (Not StrArray Is Nothing) Then
                Dim str As String
                For Each str In StrArray
                    If Not StrCollection.Contains(str) Then
                        StrCollection.Add(str)
                    End If
                Next
            End If
        End Sub

        Friend Shared Sub CheckFilePathTrailingSeparator(ByVal path As String, ByVal paramName As String)
            If (path = "") Then
                Throw ExceptionUtils.GetArgumentNullException(paramName)
            End If
            If (path.EndsWith(Conversions.ToString(path.DirectorySeparatorChar), StringComparison.Ordinal) Or path.EndsWith(Conversions.ToString(path.AltDirectorySeparatorChar), StringComparison.Ordinal)) Then
                Throw ExceptionUtils.GetArgumentExceptionWithArgName(paramName, "IO_FilePathException", New String(0 - 1) {})
            End If
        End Sub

        Public Shared Function CombinePath(ByVal baseDirectory As String, ByVal relativePath As String) As String
            If (baseDirectory = "") Then
                Dim placeHolders As String() = New String() {"baseDirectory"}
                Throw ExceptionUtils.GetArgumentNullException("baseDirectory", "General_ArgumentEmptyOrNothing_Name", placeHolders)
            End If
            If (relativePath = "") Then
                Return baseDirectory
            End If
            baseDirectory = Path.GetFullPath(baseDirectory)
            Return FileSystem.NormalizePath(Path.Combine(baseDirectory, relativePath))
        End Function

        Public Shared Sub CopyDirectory(ByVal sourceDirectoryName As String, ByVal destinationDirectoryName As String)
            FileSystem.CopyOrMoveDirectory(CopyOrMove.Copy, sourceDirectoryName, destinationDirectoryName, False, UIOptionInternal.NoUI, UICancelOption.ThrowException)
        End Sub

        Public Shared Sub CopyDirectory(ByVal sourceDirectoryName As String, ByVal destinationDirectoryName As String, ByVal showUI As UIOption)
            FileSystem.CopyOrMoveDirectory(CopyOrMove.Copy, sourceDirectoryName, destinationDirectoryName, False, FileSystem.ToUIOptionInternal(showUI), UICancelOption.ThrowException)
        End Sub

        Public Shared Sub CopyDirectory(ByVal sourceDirectoryName As String, ByVal destinationDirectoryName As String, ByVal overwrite As Boolean)
            FileSystem.CopyOrMoveDirectory(CopyOrMove.Copy, sourceDirectoryName, destinationDirectoryName, overwrite, UIOptionInternal.NoUI, UICancelOption.ThrowException)
        End Sub

        Public Shared Sub CopyDirectory(ByVal sourceDirectoryName As String, ByVal destinationDirectoryName As String, ByVal showUI As UIOption, ByVal onUserCancel As UICancelOption)
            FileSystem.CopyOrMoveDirectory(CopyOrMove.Copy, sourceDirectoryName, destinationDirectoryName, False, FileSystem.ToUIOptionInternal(showUI), onUserCancel)
        End Sub

        Public Shared Sub CopyFile(ByVal sourceFileName As String, ByVal destinationFileName As String)
            FileSystem.CopyOrMoveFile(CopyOrMove.Copy, sourceFileName, destinationFileName, False, UIOptionInternal.NoUI, UICancelOption.ThrowException)
        End Sub

        Public Shared Sub CopyFile(ByVal sourceFileName As String, ByVal destinationFileName As String, ByVal showUI As UIOption)
            FileSystem.CopyOrMoveFile(CopyOrMove.Copy, sourceFileName, destinationFileName, False, FileSystem.ToUIOptionInternal(showUI), UICancelOption.ThrowException)
        End Sub

        Public Shared Sub CopyFile(ByVal sourceFileName As String, ByVal destinationFileName As String, ByVal overwrite As Boolean)
            FileSystem.CopyOrMoveFile(CopyOrMove.Copy, sourceFileName, destinationFileName, overwrite, UIOptionInternal.NoUI, UICancelOption.ThrowException)
        End Sub

        Public Shared Sub CopyFile(ByVal sourceFileName As String, ByVal destinationFileName As String, ByVal showUI As UIOption, ByVal onUserCancel As UICancelOption)
            FileSystem.CopyOrMoveFile(CopyOrMove.Copy, sourceFileName, destinationFileName, False, FileSystem.ToUIOptionInternal(showUI), onUserCancel)
        End Sub

        <SecuritySafeCritical>
        Private Shared Sub CopyOrMoveDirectory(ByVal operation As CopyOrMove, ByVal sourceDirectoryName As String, ByVal destinationDirectoryName As String, ByVal overwrite As Boolean, ByVal showUI As UIOptionInternal, ByVal onUserCancel As UICancelOption)
            FileSystem.VerifyUICancelOption("onUserCancel", onUserCancel)
            Dim fullDirectoryPath As String = FileSystem.NormalizePath(sourceDirectoryName)
            Dim str2 As String = FileSystem.NormalizePath(destinationDirectoryName)
            Dim read As FileIOPermissionAccess = FileIOPermissionAccess.Read
            If (operation = CopyOrMove.Move) Then
                read = (read Or FileIOPermissionAccess.Write)
            End If
            FileSystem.DemandDirectoryPermission(fullDirectoryPath, read)
            FileSystem.DemandDirectoryPermission(str2, (FileIOPermissionAccess.Write Or FileIOPermissionAccess.Read))
            FileSystem.ThrowIfDevicePath(fullDirectoryPath)
            FileSystem.ThrowIfDevicePath(str2)
            If Not Directory.Exists(fullDirectoryPath) Then
                Dim placeHolders As String() = New String() {sourceDirectoryName}
                Throw ExceptionUtils.GetDirectoryNotFoundException("IO_DirectoryNotFound_Path", placeHolders)
            End If
            If FileSystem.IsRoot(fullDirectoryPath) Then
                Dim placeHolders As String() = New String() {sourceDirectoryName}
                Throw ExceptionUtils.GetIOException("IO_DirectoryIsRoot_Path", placeHolders)
            End If
            If File.Exists(str2) Then
                Dim placeHolders As String() = New String() {destinationDirectoryName}
                Throw ExceptionUtils.GetIOException("IO_FileExists_Path", placeHolders)
            End If
            If str2.Equals(fullDirectoryPath, StringComparison.OrdinalIgnoreCase) Then
                Throw ExceptionUtils.GetIOException("IO_SourceEqualsTargetDirectory", New String(0 - 1) {})
            End If
            If (((str2.Length > fullDirectoryPath.Length) AndAlso str2.Substring(0, fullDirectoryPath.Length).Equals(fullDirectoryPath, StringComparison.OrdinalIgnoreCase)) AndAlso (str2.Chars(fullDirectoryPath.Length) = Path.DirectorySeparatorChar)) Then
                Throw ExceptionUtils.GetInvalidOperationException("IO_CyclicOperation", New String(0 - 1) {})
            End If
            If ((showUI <> UIOptionInternal.NoUI) AndAlso Environment.UserInteractive) Then
                FileSystem.ShellCopyOrMove(operation, FileOrDirectory.Directory, fullDirectoryPath, str2, showUI, onUserCancel)
            Else
                FileSystem.FxCopyOrMoveDirectory(operation, fullDirectoryPath, str2, overwrite)
            End If
        End Sub

        Private Shared Sub CopyOrMoveDirectoryNode(ByVal Operation As CopyOrMove, ByVal SourceDirectoryNode As DirectoryNode, ByVal Overwrite As Boolean, ByVal Exceptions As ListDictionary)
            Try
                If Not Directory.Exists(SourceDirectoryNode.TargetPath) Then
                    Directory.CreateDirectory(SourceDirectoryNode.TargetPath)
                End If
            Catch exception As Exception
                If ((Not TypeOf exception Is IOException AndAlso Not TypeOf exception Is UnauthorizedAccessException) AndAlso ((Not TypeOf exception Is DirectoryNotFoundException AndAlso Not TypeOf exception Is NotSupportedException) AndAlso Not TypeOf exception Is SecurityException)) Then
                    Throw
                End If
                Exceptions.Add(SourceDirectoryNode.Path, exception.Message)
                Return
            End Try
            If Not Directory.Exists(SourceDirectoryNode.TargetPath) Then
                Dim placeHolders As String() = New String() {SourceDirectoryNode.TargetPath}
                Exceptions.Add(SourceDirectoryNode.TargetPath, ExceptionUtils.GetDirectoryNotFoundException("IO_DirectoryNotFound_Path", placeHolders))
            Else
                Dim str As String
                For Each str In Directory.GetFiles(SourceDirectoryNode.Path)
                    Try
                        FileSystem.CopyOrMoveFile(Operation, str, Path.Combine(SourceDirectoryNode.TargetPath, Path.GetFileName(str)), Overwrite, UIOptionInternal.NoUI, UICancelOption.ThrowException)
                    Catch exception2 As Exception
                        If ((Not TypeOf exception2 Is IOException AndAlso Not TypeOf exception2 Is UnauthorizedAccessException) AndAlso (Not TypeOf exception2 Is SecurityException AndAlso Not TypeOf exception2 Is NotSupportedException)) Then
                            Throw
                        End If
                        Exceptions.Add(str, exception2.Message)
                    End Try
                Next
                Dim node As DirectoryNode
                For Each node In SourceDirectoryNode.SubDirs
                    FileSystem.CopyOrMoveDirectoryNode(Operation, node, Overwrite, Exceptions)
                Next
                If (Operation = CopyOrMove.Move) Then
                    Try
                        Directory.Delete(SourceDirectoryNode.Path, False)
                    Catch exception3 As Exception
                        If ((Not TypeOf exception3 Is IOException AndAlso Not TypeOf exception3 Is UnauthorizedAccessException) AndAlso (Not TypeOf exception3 Is SecurityException AndAlso Not TypeOf exception3 Is DirectoryNotFoundException)) Then
                            Throw
                        End If
                        Exceptions.Add(SourceDirectoryNode.Path, exception3.Message)
                    End Try
                End If
            End If
        End Sub

        <SecuritySafeCritical>
        Private Shared Sub CopyOrMoveFile(ByVal operation As CopyOrMove, ByVal sourceFileName As String, ByVal destinationFileName As String, ByVal overwrite As Boolean, ByVal showUI As UIOptionInternal, ByVal onUserCancel As UICancelOption)
            FileSystem.VerifyUICancelOption("onUserCancel", onUserCancel)
            Dim path As String = FileSystem.NormalizeFilePath(sourceFileName, "sourceFileName")
            Dim str2 As String = FileSystem.NormalizeFilePath(destinationFileName, "destinationFileName")
            Dim read As FileIOPermissionAccess = FileIOPermissionAccess.Read
            If (operation = CopyOrMove.Move) Then
                read = (read Or FileIOPermissionAccess.Write)
            End If
            New FileIOPermission(read, path).Demand
            New FileIOPermission(FileIOPermissionAccess.Write, str2).Demand
            FileSystem.ThrowIfDevicePath(path)
            FileSystem.ThrowIfDevicePath(str2)
            If Not File.Exists(path) Then
                Dim placeHolders As String() = New String() {sourceFileName}
                Throw ExceptionUtils.GetFileNotFoundException(sourceFileName, "IO_FileNotFound_Path", placeHolders)
            End If
            If Directory.Exists(str2) Then
                Dim placeHolders As String() = New String() {destinationFileName}
                Throw ExceptionUtils.GetIOException("IO_DirectoryExists_Path", placeHolders)
            End If
            Directory.CreateDirectory(FileSystem.GetParentPath(str2))
            If ((showUI <> UIOptionInternal.NoUI) AndAlso Environment.UserInteractive) Then
                FileSystem.ShellCopyOrMove(operation, FileOrDirectory.File, path, str2, showUI, onUserCancel)
            ElseIf ((operation = CopyOrMove.Copy) OrElse path.Equals(str2, StringComparison.OrdinalIgnoreCase)) Then
                File.Copy(path, str2, overwrite)
            ElseIf overwrite Then
                If (Environment.OSVersion.Platform = PlatformID.Win32NT) Then
                    New SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert
                    Try
                        If Not NativeMethods.MoveFileEx(path, str2, 11) Then
                            FileSystem.ThrowWinIOError(Marshal.GetLastWin32Error)
                        End If
                        Return
                    Catch exception1 As Exception
                        Throw
                    Finally
                        CodeAccessPermission.RevertAssert()
                    End Try
                End If
                File.Delete(str2)
                File.Move(path, str2)
            Else
                File.Move(path, str2)
            End If
        End Sub

        Public Shared Sub CreateDirectory(ByVal directory As String)
            directory = Path.GetFullPath(directory)
            If File.Exists(directory) Then
                Dim placeHolders As String() = New String() {directory}
                Throw ExceptionUtils.GetIOException("IO_FileExists_Path", placeHolders)
            End If
            directory.CreateDirectory(directory)
        End Sub

        Public Shared Sub DeleteDirectory(ByVal directory As String, ByVal onDirectoryNotEmpty As DeleteDirectoryOption)
            FileSystem.DeleteDirectoryInternal(directory, onDirectoryNotEmpty, UIOptionInternal.NoUI, RecycleOption.DeletePermanently, UICancelOption.ThrowException)
        End Sub

        Public Shared Sub DeleteDirectory(ByVal directory As String, ByVal showUI As UIOption, ByVal recycle As RecycleOption)
            FileSystem.DeleteDirectoryInternal(directory, DeleteDirectoryOption.DeleteAllContents, FileSystem.ToUIOptionInternal(showUI), recycle, UICancelOption.ThrowException)
        End Sub

        Public Shared Sub DeleteDirectory(ByVal directory As String, ByVal showUI As UIOption, ByVal recycle As RecycleOption, ByVal onUserCancel As UICancelOption)
            FileSystem.DeleteDirectoryInternal(directory, DeleteDirectoryOption.DeleteAllContents, FileSystem.ToUIOptionInternal(showUI), recycle, onUserCancel)
        End Sub

        <SecuritySafeCritical>
        Private Shared Sub DeleteDirectoryInternal(ByVal directory As String, ByVal onDirectoryNotEmpty As DeleteDirectoryOption, ByVal showUI As UIOptionInternal, ByVal recycle As RecycleOption, ByVal onUserCancel As UICancelOption)
            FileSystem.VerifyDeleteDirectoryOption("onDirectoryNotEmpty", onDirectoryNotEmpty)
            FileSystem.VerifyRecycleOption("recycle", recycle)
            FileSystem.VerifyUICancelOption("onUserCancel", onUserCancel)
            Dim fullPath As String = Path.GetFullPath(directory)
            FileSystem.DemandDirectoryPermission(fullPath, FileIOPermissionAccess.Write)
            FileSystem.ThrowIfDevicePath(fullPath)
            If Not directory.Exists(fullPath) Then
                Dim placeHolders As String() = New String() {directory}
                Throw ExceptionUtils.GetDirectoryNotFoundException("IO_DirectoryNotFound_Path", placeHolders)
            End If
            If FileSystem.IsRoot(fullPath) Then
                Dim placeHolders As String() = New String() {directory}
                Throw ExceptionUtils.GetIOException("IO_DirectoryIsRoot_Path", placeHolders)
            End If
            If ((showUI <> UIOptionInternal.NoUI) AndAlso Environment.UserInteractive) Then
                FileSystem.ShellDelete(fullPath, showUI, recycle, onUserCancel, FileOrDirectory.Directory)
            Else
                directory.Delete(fullPath, (onDirectoryNotEmpty = DeleteDirectoryOption.DeleteAllContents))
            End If
        End Sub

        Public Shared Sub DeleteFile(ByVal file As String)
            FileSystem.DeleteFileInternal(file, UIOptionInternal.NoUI, RecycleOption.DeletePermanently, UICancelOption.ThrowException)
        End Sub

        Public Shared Sub DeleteFile(ByVal file As String, ByVal showUI As UIOption, ByVal recycle As RecycleOption)
            FileSystem.DeleteFileInternal(file, FileSystem.ToUIOptionInternal(showUI), recycle, UICancelOption.ThrowException)
        End Sub

        Public Shared Sub DeleteFile(ByVal file As String, ByVal showUI As UIOption, ByVal recycle As RecycleOption, ByVal onUserCancel As UICancelOption)
            FileSystem.DeleteFileInternal(file, FileSystem.ToUIOptionInternal(showUI), recycle, onUserCancel)
        End Sub

        <SecuritySafeCritical>
        Private Shared Sub DeleteFileInternal(ByVal file As String, ByVal showUI As UIOptionInternal, ByVal recycle As RecycleOption, ByVal onUserCancel As UICancelOption)
            FileSystem.VerifyRecycleOption("recycle", recycle)
            FileSystem.VerifyUICancelOption("onUserCancel", onUserCancel)
            Dim path As String = FileSystem.NormalizeFilePath(file, "file")
            New FileIOPermission(FileIOPermissionAccess.Write, path).Demand
            FileSystem.ThrowIfDevicePath(path)
            If Not file.Exists(path) Then
                Dim placeHolders As String() = New String() {file}
                Throw ExceptionUtils.GetFileNotFoundException(file, "IO_FileNotFound_Path", placeHolders)
            End If
            If ((showUI <> UIOptionInternal.NoUI) AndAlso Environment.UserInteractive) Then
                FileSystem.ShellDelete(path, showUI, recycle, onUserCancel, FileOrDirectory.File)
            Else
                file.Delete(path)
            End If
        End Sub

        <SecuritySafeCritical>
        Private Shared Sub DemandDirectoryPermission(ByVal fullDirectoryPath As String, ByVal access As FileIOPermissionAccess)
            If Not (fullDirectoryPath.EndsWith(Conversions.ToString(Path.DirectorySeparatorChar), StringComparison.Ordinal) Or fullDirectoryPath.EndsWith(Conversions.ToString(Path.AltDirectorySeparatorChar), StringComparison.Ordinal)) Then
                fullDirectoryPath = (fullDirectoryPath & Conversions.ToString(Path.DirectorySeparatorChar))
            End If
            New FileIOPermission(access, fullDirectoryPath).Demand
        End Sub

        Public Shared Function DirectoryExists(ByVal directory As String) As Boolean
            Return directory.Exists(directory)
        End Function

        Private Shared Sub EnsurePathNotExist(ByVal Path As String)
            If File.Exists(Path) Then
                Dim placeHolders As String() = New String() {Path}
                Throw ExceptionUtils.GetIOException("IO_FileExists_Path", placeHolders)
            End If
            If Directory.Exists(Path) Then
                Dim placeHolders As String() = New String() {Path}
                Throw ExceptionUtils.GetIOException("IO_DirectoryExists_Path", placeHolders)
            End If
        End Sub

        Private Shared Function FileContainsText(ByVal FilePath As String, ByVal [Text] As String, ByVal IgnoreCase As Boolean) As Boolean
            Dim flag As Boolean
            Dim num As Integer = &H400
            Dim stream As FileStream = Nothing
            Try
                stream = New FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
                Dim currentEncoding As Encoding = Encoding.Default
                Dim array As Byte() = New Byte(((num - 1) + 1) - 1) {}
                Dim count As Integer = 0
                count = stream.Read(array, 0, array.Length)
                If (count > 0) Then
                    Dim stream2 As New MemoryStream(array, 0, count)
                    Dim reader1 As New StreamReader(stream2, currentEncoding, True)
                    reader1.ReadLine()
                    currentEncoding = reader1.CurrentEncoding
                End If
                Dim num3 As Integer = Math.Max(currentEncoding.GetMaxByteCount([Text].Length), num)
                Dim helper As New TextSearchHelper(currentEncoding, [Text], IgnoreCase)
                If (num3 > num) Then
                    array = DirectCast(Utils.CopyArray(array, New Byte(((num3 - 1) + 1) - 1) {}), Byte())
                    Dim num4 As Integer = stream.Read(array, count, (array.Length - count))
                    count = (count + num4)
                End If
                Do
                    If ((count > 0) AndAlso helper.IsTextFound(array, count)) Then
                        Return True
                    End If
                    count = stream.Read(array, 0, array.Length)
                Loop While (count > 0)
                flag = False
            Catch exception As Exception
                If Not (((TypeOf exception Is IOException Or TypeOf exception Is NotSupportedException) Or TypeOf exception Is SecurityException) Or TypeOf exception Is UnauthorizedAccessException) Then
                    Throw
                End If
                flag = False
            Finally
                If (Not stream Is Nothing) Then
                    stream.Close()
                End If
            End Try
            Return flag
        End Function

        Public Shared Function FileExists(ByVal file As String) As Boolean
            If (Not String.IsNullOrEmpty(file) AndAlso (file.EndsWith(Conversions.ToString(Path.DirectorySeparatorChar), StringComparison.OrdinalIgnoreCase) Or file.EndsWith(Conversions.ToString(Path.AltDirectorySeparatorChar), StringComparison.OrdinalIgnoreCase))) Then
                Return False
            End If
            Return file.Exists(file)
        End Function

        Private Shared Function FindFilesOrDirectories(ByVal FileOrDirectory As FileOrDirectory, ByVal directory As String, ByVal searchType As SearchOption, ByVal wildcards As String()) As ReadOnlyCollection(Of String)
            Dim results As New Collection(Of String)
            FileSystem.FindFilesOrDirectories(FileOrDirectory, directory, searchType, wildcards, results)
            Return New ReadOnlyCollection(Of String)(results)
        End Function

        Private Shared Sub FindFilesOrDirectories(ByVal FileOrDirectory As FileOrDirectory, ByVal directory As String, ByVal searchType As SearchOption, ByVal wildcards As String(), ByVal Results As Collection(Of String))
            FileSystem.VerifySearchOption("searchType", searchType)
            directory = FileSystem.NormalizePath(directory)
            If (Not wildcards Is Nothing) Then
                Dim strArray As String() = wildcards
                Dim i As Integer
                For i = 0 To strArray.Length - 1
                    If (strArray(i).TrimEnd(New Char(0 - 1) {}) = "") Then
                        Throw ExceptionUtils.GetArgumentNullException("wildcards", "IO_GetFiles_NullPattern", New String(0 - 1) {})
                    End If
                Next i
            End If
            If ((wildcards Is Nothing) OrElse (wildcards.Length = 0)) Then
                FileSystem.AddToStringCollection(Results, FileSystem.FindPaths(FileOrDirectory, directory, Nothing))
            Else
                Dim str As String
                For Each str In wildcards
                    FileSystem.AddToStringCollection(Results, FileSystem.FindPaths(FileOrDirectory, directory, str))
                Next
            End If
            If (searchType = SearchOption.SearchAllSubDirectories) Then
                Dim str2 As String
                For Each str2 In directory.GetDirectories(directory)
                    FileSystem.FindFilesOrDirectories(FileOrDirectory, str2, searchType, wildcards, Results)
                Next
            End If
        End Sub

        Public Shared Function FindInFiles(ByVal directory As String, ByVal containsText As String, ByVal ignoreCase As Boolean, ByVal searchType As SearchOption) As ReadOnlyCollection(Of String)
            Return FileSystem.FindInFiles(directory, containsText, ignoreCase, searchType, Nothing)
        End Function

        Public Shared Function FindInFiles(ByVal directory As String, ByVal containsText As String, ByVal ignoreCase As Boolean, ByVal searchType As SearchOption, ByVal ParamArray fileWildcards As String()) As ReadOnlyCollection(Of String)
            Dim onlys2 As ReadOnlyCollection(Of String) = FileSystem.FindFilesOrDirectories(FileOrDirectory.File, directory, searchType, fileWildcards)
            If (containsText <> "") Then
                Dim list As New Collection(Of String)
                Dim str As String
                For Each str In onlys2
                    If FileSystem.FileContainsText(str, containsText, ignoreCase) Then
                        list.Add(str)
                    End If
                Next
                Return New ReadOnlyCollection(Of String)(list)
            End If
            Return onlys2
        End Function

        Private Shared Function FindPaths(ByVal FileOrDirectory As FileOrDirectory, ByVal directory As String, ByVal wildCard As String) As String()
            If (FileOrDirectory = FileOrDirectory.Directory) Then
                If (wildCard = "") Then
                    Return directory.GetDirectories(directory)
                End If
                Return directory.GetDirectories(directory, wildCard)
            End If
            If (wildCard = "") Then
                Return directory.GetFiles(directory)
            End If
            Return directory.GetFiles(directory, wildCard)
        End Function

        Private Shared Sub FxCopyOrMoveDirectory(ByVal operation As CopyOrMove, ByVal sourceDirectoryPath As String, ByVal targetDirectoryPath As String, ByVal overwrite As Boolean)
            If (((operation = CopyOrMove.Move) And Not Directory.Exists(targetDirectoryPath)) And FileSystem.IsOnSameDrive(sourceDirectoryPath, targetDirectoryPath)) Then
                Directory.CreateDirectory(FileSystem.GetParentPath(targetDirectoryPath))
                Try
                    Directory.Move(sourceDirectoryPath, targetDirectoryPath)
                    Return
                Catch exception As IOException
                Catch exception2 As UnauthorizedAccessException
                End Try
            End If
            Directory.CreateDirectory(targetDirectoryPath)
            Dim sourceDirectoryNode As New DirectoryNode(sourceDirectoryPath, targetDirectoryPath)
            Dim exceptions As New ListDictionary
            FileSystem.CopyOrMoveDirectoryNode(operation, sourceDirectoryNode, overwrite, exceptions)
            If (exceptions.Count > 0) Then
                Dim exception3 As New IOException(Utils.GetResourceString("IO_CopyMoveRecursive"))
                Dim enumerator As IDictionaryEnumerator = exceptions.GetEnumerator
                Do While enumerator.MoveNext
                    Dim current As DictionaryEntry
                    If (Not enumerator.Current Is Nothing) Then
                        current = DirectCast(enumerator.Current, DictionaryEntry)
                    Else
                        current = New DictionaryEntry
                    End If
                    exception3.Data.Add(current.Key, current.Value)
                Loop
                Throw exception3
            End If
        End Sub

        Public Shared Function GetDirectories(ByVal directory As String) As ReadOnlyCollection(Of String)
            Return FileSystem.FindFilesOrDirectories(FileOrDirectory.Directory, directory, SearchOption.SearchTopLevelOnly, Nothing)
        End Function

        Public Shared Function GetDirectories(ByVal directory As String, ByVal searchType As SearchOption, ByVal ParamArray wildcards As String()) As ReadOnlyCollection(Of String)
            Return FileSystem.FindFilesOrDirectories(FileOrDirectory.Directory, directory, searchType, wildcards)
        End Function

        Public Shared Function GetDirectoryInfo(ByVal directory As String) As DirectoryInfo
            Return New DirectoryInfo(directory)
        End Function

        Public Shared Function GetDriveInfo(ByVal drive As String) As DriveInfo
            Return New DriveInfo(drive)
        End Function

        Public Shared Function GetFileInfo(ByVal file As String) As FileInfo
            file = FileSystem.NormalizeFilePath(file, "file")
            Return New FileInfo(file)
        End Function

        Public Shared Function GetFiles(ByVal directory As String) As ReadOnlyCollection(Of String)
            Return FileSystem.FindFilesOrDirectories(FileOrDirectory.File, directory, SearchOption.SearchTopLevelOnly, Nothing)
        End Function

        Public Shared Function GetFiles(ByVal directory As String, ByVal searchType As SearchOption, ByVal ParamArray wildcards As String()) As ReadOnlyCollection(Of String)
            Return FileSystem.FindFilesOrDirectories(FileOrDirectory.File, directory, searchType, wildcards)
        End Function

        Private Shared Function GetFullPathFromNewName(ByVal Path As String, ByVal NewName As String, ByVal ArgumentName As String) As String
            If (NewName.IndexOfAny(FileSystem.m_SeparatorChars) >= 0) Then
                Dim placeHolders As String() = New String() {ArgumentName, NewName}
                Throw ExceptionUtils.GetArgumentExceptionWithArgName(ArgumentName, "IO_ArgumentIsPath_Name_Path", placeHolders)
            End If
            Dim path As String = FileSystem.RemoveEndingSeparator(path.GetFullPath(path.Combine(path, NewName)))
            If Not FileSystem.GetParentPath(path).Equals(path, StringComparison.OrdinalIgnoreCase) Then
                Dim placeHolders As String() = New String() {ArgumentName, NewName}
                Throw ExceptionUtils.GetArgumentExceptionWithArgName(ArgumentName, "IO_ArgumentIsPath_Name_Path", placeHolders)
            End If
            Return path
        End Function

        Private Shared Function GetLongPath(ByVal FullPath As String) As String
            Try
                If Not FileSystem.IsRoot(FullPath) Then
                    Dim info As New DirectoryInfo(FileSystem.GetParentPath(FullPath))
                    If File.Exists(FullPath) Then
                        Return info.GetFiles(Path.GetFileName(FullPath))(0).FullName
                    End If
                    If Directory.Exists(FullPath) Then
                        Return info.GetDirectories(Path.GetFileName(FullPath))(0).FullName
                    End If
                End If
                Return FullPath
            Catch exception As Exception
                If (((Not TypeOf exception Is ArgumentException AndAlso Not TypeOf exception Is ArgumentNullException) AndAlso (Not TypeOf exception Is PathTooLongException AndAlso Not TypeOf exception Is NotSupportedException)) AndAlso ((Not TypeOf exception Is DirectoryNotFoundException AndAlso Not TypeOf exception Is SecurityException) AndAlso Not TypeOf exception Is UnauthorizedAccessException)) Then
                    Throw
                End If
                Return FullPath
            End Try
        End Function

        Public Shared Function GetName(ByVal path As String) As String
            Return path.GetFileName(path)
        End Function

        Private Shared Function GetOperationFlags(ByVal ShowUI As UIOptionInternal) As ShFileOperationFlags
            Dim flags As ShFileOperationFlags = (ShFileOperationFlags.FOF_NO_CONNECTED_ELEMENTS Or ShFileOperationFlags.FOF_NOCONFIRMMKDIR)
            If (ShowUI = UIOptionInternal.OnlyErrorDialogs) Then
                flags = (flags Or (ShFileOperationFlags.FOF_NOCONFIRMATION Or ShFileOperationFlags.FOF_SILENT))
            End If
            Return flags
        End Function

        Public Shared Function GetParentPath(ByVal path As String) As String
            path.GetFullPath(path)
            If FileSystem.IsRoot(path) Then
                Dim placeHolders As String() = New String() {path}
                Throw ExceptionUtils.GetArgumentExceptionWithArgName("path", "IO_GetParentPathIsRoot_Path", placeHolders)
            End If
            Dim trimChars As Char() = New Char() {path.DirectorySeparatorChar, path.AltDirectorySeparatorChar}
            Return path.GetDirectoryName(path.TrimEnd(trimChars))
        End Function

        <SecurityCritical>
        Private Shared Function GetShellOperationInfo(ByVal OperationType As SHFileOperationType, ByVal OperationFlags As ShFileOperationFlags, ByVal SourcePath As String, ByVal Optional TargetPath As String = Nothing) As SHFILEOPSTRUCT
            Dim sourcePaths As String() = New String() {SourcePath}
            Return FileSystem.GetShellOperationInfo(OperationType, OperationFlags, sourcePaths, TargetPath)
        End Function

        <SecurityCritical>
        Private Shared Function GetShellOperationInfo(ByVal OperationType As SHFileOperationType, ByVal OperationFlags As ShFileOperationFlags, ByVal SourcePaths As String(), ByVal Optional TargetPath As String = Nothing) As SHFILEOPSTRUCT
            Dim shfileopstruct As SHFILEOPSTRUCT
            shfileopstruct.wFunc = DirectCast(OperationType, UInt32)
            shfileopstruct.fFlags = CUShort(OperationFlags)
            shfileopstruct.pFrom = FileSystem.GetShellPath(SourcePaths)
            If (TargetPath Is Nothing) Then
                shfileopstruct.pTo = Nothing
            Else
                shfileopstruct.pTo = FileSystem.GetShellPath(TargetPath)
            End If
            shfileopstruct.hNameMappings = IntPtr.Zero
            Try
                shfileopstruct.hwnd = Process.GetCurrentProcess.MainWindowHandle
            Catch exception As Exception
                If ((Not TypeOf exception Is SecurityException AndAlso Not TypeOf exception Is InvalidOperationException) AndAlso Not TypeOf exception Is NotSupportedException) Then
                    Throw
                End If
                shfileopstruct.hwnd = IntPtr.Zero
            End Try
            shfileopstruct.lpszProgressTitle = String.Empty
            Return shfileopstruct
        End Function

        Private Shared Function GetShellPath(ByVal FullPath As String) As String
            Dim fullPaths As String() = New String() {FullPath}
            Return FileSystem.GetShellPath(fullPaths)
        End Function

        Private Shared Function GetShellPath(ByVal FullPaths As String()) As String
            Dim builder As New StringBuilder
            Dim str As String
            For Each str In FullPaths
                builder.Append((str & ChrW(0)))
            Next
            Return builder.ToString
        End Function

        Public Shared Function GetTempFileName() As String
            Return Path.GetTempFileName
        End Function

        Private Shared Function IsOnSameDrive(ByVal Path1 As String, ByVal Path2 As String) As Boolean
            Dim trimChars As Char() = New Char() {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar}
            Path1 = Path1.TrimEnd(trimChars)
            Dim chArray2 As Char() = New Char() {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar}
            Path2 = Path2.TrimEnd(chArray2)
            Return (String.Compare(Path.GetPathRoot(Path1), Path.GetPathRoot(Path2), StringComparison.OrdinalIgnoreCase) = 0)
        End Function

        Private Shared Function IsRoot(ByVal Path As String) As Boolean
            If Not Path.IsPathRooted(Path) Then
                Return False
            End If
            Dim trimChars As Char() = New Char() {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar}
            Path = Path.TrimEnd(trimChars)
            Return (String.Compare(Path, Path.GetPathRoot(Path), StringComparison.OrdinalIgnoreCase) = 0)
        End Function

        Public Shared Sub MoveDirectory(ByVal sourceDirectoryName As String, ByVal destinationDirectoryName As String)
            FileSystem.CopyOrMoveDirectory(CopyOrMove.Move, sourceDirectoryName, destinationDirectoryName, False, UIOptionInternal.NoUI, UICancelOption.ThrowException)
        End Sub

        Public Shared Sub MoveDirectory(ByVal sourceDirectoryName As String, ByVal destinationDirectoryName As String, ByVal showUI As UIOption)
            FileSystem.CopyOrMoveDirectory(CopyOrMove.Move, sourceDirectoryName, destinationDirectoryName, False, FileSystem.ToUIOptionInternal(showUI), UICancelOption.ThrowException)
        End Sub

        Public Shared Sub MoveDirectory(ByVal sourceDirectoryName As String, ByVal destinationDirectoryName As String, ByVal overwrite As Boolean)
            FileSystem.CopyOrMoveDirectory(CopyOrMove.Move, sourceDirectoryName, destinationDirectoryName, overwrite, UIOptionInternal.NoUI, UICancelOption.ThrowException)
        End Sub

        Public Shared Sub MoveDirectory(ByVal sourceDirectoryName As String, ByVal destinationDirectoryName As String, ByVal showUI As UIOption, ByVal onUserCancel As UICancelOption)
            FileSystem.CopyOrMoveDirectory(CopyOrMove.Move, sourceDirectoryName, destinationDirectoryName, False, FileSystem.ToUIOptionInternal(showUI), onUserCancel)
        End Sub

        Public Shared Sub MoveFile(ByVal sourceFileName As String, ByVal destinationFileName As String)
            FileSystem.CopyOrMoveFile(CopyOrMove.Move, sourceFileName, destinationFileName, False, UIOptionInternal.NoUI, UICancelOption.ThrowException)
        End Sub

        Public Shared Sub MoveFile(ByVal sourceFileName As String, ByVal destinationFileName As String, ByVal showUI As UIOption)
            FileSystem.CopyOrMoveFile(CopyOrMove.Move, sourceFileName, destinationFileName, False, FileSystem.ToUIOptionInternal(showUI), UICancelOption.ThrowException)
        End Sub

        Public Shared Sub MoveFile(ByVal sourceFileName As String, ByVal destinationFileName As String, ByVal overwrite As Boolean)
            FileSystem.CopyOrMoveFile(CopyOrMove.Move, sourceFileName, destinationFileName, overwrite, UIOptionInternal.NoUI, UICancelOption.ThrowException)
        End Sub

        Public Shared Sub MoveFile(ByVal sourceFileName As String, ByVal destinationFileName As String, ByVal showUI As UIOption, ByVal onUserCancel As UICancelOption)
            FileSystem.CopyOrMoveFile(CopyOrMove.Move, sourceFileName, destinationFileName, False, FileSystem.ToUIOptionInternal(showUI), onUserCancel)
        End Sub

        Friend Shared Function NormalizeFilePath(ByVal Path As String, ByVal ParamName As String) As String
            FileSystem.CheckFilePathTrailingSeparator(Path, ParamName)
            Return FileSystem.NormalizePath(Path)
        End Function

        Friend Shared Function NormalizePath(ByVal Path As String) As String
            Return FileSystem.GetLongPath(FileSystem.RemoveEndingSeparator(Path.GetFullPath(Path)))
        End Function

        Public Shared Function OpenTextFieldParser(ByVal file As String) As TextFieldParser
            Return New TextFieldParser(file)
        End Function

        Public Shared Function OpenTextFieldParser(ByVal file As String, ByVal ParamArray fieldWidths As Integer()) As TextFieldParser
            Dim parser1 As New TextFieldParser(file)
            parser1.SetFieldWidths(fieldWidths)
            parser1.TextFieldType = FieldType.FixedWidth
            Return parser1
        End Function

        Public Shared Function OpenTextFieldParser(ByVal file As String, ByVal ParamArray delimiters As String()) As TextFieldParser
            Dim parser1 As New TextFieldParser(file)
            parser1.SetDelimiters(delimiters)
            parser1.TextFieldType = FieldType.Delimited
            Return parser1
        End Function

        Public Shared Function OpenTextFileReader(ByVal file As String) As StreamReader
            Return FileSystem.OpenTextFileReader(file, Encoding.UTF8)
        End Function

        Public Shared Function OpenTextFileReader(ByVal file As String, ByVal encoding As Encoding) As StreamReader
            file = FileSystem.NormalizeFilePath(file, "file")
            Return New StreamReader(file, encoding, True)
        End Function

        Public Shared Function OpenTextFileWriter(ByVal file As String, ByVal append As Boolean) As StreamWriter
            Return FileSystem.OpenTextFileWriter(file, append, Encoding.UTF8)
        End Function

        Public Shared Function OpenTextFileWriter(ByVal file As String, ByVal append As Boolean, ByVal encoding As Encoding) As StreamWriter
            file = FileSystem.NormalizeFilePath(file, "file")
            Return New StreamWriter(file, append, encoding)
        End Function

        Public Shared Function ReadAllBytes(ByVal file As String) As Byte()
            Return file.ReadAllBytes(file)
        End Function

        Public Shared Function ReadAllText(ByVal file As String) As String
            Return file.ReadAllText(file)
        End Function

        Public Shared Function ReadAllText(ByVal file As String, ByVal encoding As Encoding) As String
            Return file.ReadAllText(file, encoding)
        End Function

        Private Shared Function RemoveEndingSeparator(ByVal Path As String) As String
            If (Path.IsPathRooted(Path) AndAlso Path.Equals(Path.GetPathRoot(Path), StringComparison.OrdinalIgnoreCase)) Then
                Return Path
            End If
            Dim trimChars As Char() = New Char() {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar}
            Return Path.TrimEnd(trimChars)
        End Function

        Public Shared Sub RenameDirectory(ByVal directory As String, ByVal newName As String)
            directory = path.GetFullPath(directory)
            FileSystem.ThrowIfDevicePath(directory)
            If FileSystem.IsRoot(directory) Then
                Dim placeHolders As String() = New String() {directory}
                Throw ExceptionUtils.GetIOException("IO_DirectoryIsRoot_Path", placeHolders)
            End If
            If Not directory.Exists(directory) Then
                Dim placeHolders As String() = New String() {directory}
                Throw ExceptionUtils.GetDirectoryNotFoundException("IO_DirectoryNotFound_Path", placeHolders)
            End If
            If (newName = "") Then
                Dim placeHolders As String() = New String() {"newName"}
                Throw ExceptionUtils.GetArgumentNullException("newName", "General_ArgumentEmptyOrNothing_Name", placeHolders)
            End If
            Dim path As String = FileSystem.GetFullPathFromNewName(FileSystem.GetParentPath(directory), newName, "newName")
            FileSystem.EnsurePathNotExist(path)
            directory.Move(directory, path)
        End Sub

        Public Shared Sub RenameFile(ByVal file As String, ByVal newName As String)
            file = FileSystem.NormalizeFilePath(file, "file")
            FileSystem.ThrowIfDevicePath(file)
            If Not file.Exists(file) Then
                Dim placeHolders As String() = New String() {file}
                Throw ExceptionUtils.GetFileNotFoundException(file, "IO_FileNotFound_Path", placeHolders)
            End If
            If (newName = "") Then
                Dim placeHolders As String() = New String() {"newName"}
                Throw ExceptionUtils.GetArgumentNullException("newName", "General_ArgumentEmptyOrNothing_Name", placeHolders)
            End If
            Dim path As String = FileSystem.GetFullPathFromNewName(FileSystem.GetParentPath(file), newName, "newName")
            FileSystem.EnsurePathNotExist(path)
            file.Move(file, path)
        End Sub

        <SecurityCritical>
        Private Shared Sub ShellCopyOrMove(ByVal Operation As CopyOrMove, ByVal TargetType As FileOrDirectory, ByVal FullSourcePath As String, ByVal FullTargetPath As String, ByVal ShowUI As UIOptionInternal, ByVal OnUserCancel As UICancelOption)
            Dim type As SHFileOperationType
            If (Operation = CopyOrMove.Copy) Then
                type = SHFileOperationType.FO_COPY
            Else
                type = SHFileOperationType.FO_MOVE
            End If
            Dim operationFlags As ShFileOperationFlags = FileSystem.GetOperationFlags(ShowUI)
            Dim fullSource As String = FullSourcePath
            If (TargetType = FileOrDirectory.Directory) Then
                If Directory.Exists(FullTargetPath) Then
                    fullSource = Path.Combine(FullSourcePath, "*")
                Else
                    Directory.CreateDirectory(FileSystem.GetParentPath(FullTargetPath))
                End If
            End If
            FileSystem.ShellFileOperation(type, operationFlags, fullSource, FullTargetPath, OnUserCancel, TargetType)
            If ((((Operation = CopyOrMove.Move) And (TargetType = FileOrDirectory.Directory)) AndAlso Directory.Exists(FullSourcePath)) AndAlso ((Directory.GetDirectories(FullSourcePath).Length = 0) AndAlso (Directory.GetFiles(FullSourcePath).Length = 0))) Then
                Directory.Delete(FullSourcePath, False)
            End If
        End Sub

        <SecurityCritical>
        Private Shared Sub ShellDelete(ByVal FullPath As String, ByVal ShowUI As UIOptionInternal, ByVal recycle As RecycleOption, ByVal OnUserCancel As UICancelOption, ByVal FileOrDirectory As FileOrDirectory)
            Dim operationFlags As ShFileOperationFlags = FileSystem.GetOperationFlags(ShowUI)
            If (recycle = RecycleOption.SendToRecycleBin) Then
                operationFlags = (operationFlags Or ShFileOperationFlags.FOF_ALLOWUNDO)
            End If
            FileSystem.ShellFileOperation(SHFileOperationType.FO_DELETE, operationFlags, FullPath, Nothing, OnUserCancel, FileOrDirectory)
        End Sub

        <SecurityCritical, HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt, UI:=True)>
        Private Shared Sub ShellFileOperation(ByVal OperationType As SHFileOperationType, ByVal OperationFlags As ShFileOperationFlags, ByVal FullSource As String, ByVal FullTarget As String, ByVal OnUserCancel As UICancelOption, ByVal FileOrDirectory As FileOrDirectory)
            Dim num As Integer
            Call New UIPermission(UIPermissionWindow.SafeSubWindows).Demand()
            Dim noAccess As FileIOPermissionAccess = FileIOPermissionAccess.NoAccess
            If (OperationType = SHFileOperationType.FO_COPY) Then
                noAccess = FileIOPermissionAccess.Read
            ElseIf (OperationType = SHFileOperationType.FO_MOVE) Then
                noAccess = (FileIOPermissionAccess.Write Or FileIOPermissionAccess.Read)
            ElseIf (OperationType = SHFileOperationType.FO_DELETE) Then
                noAccess = FileIOPermissionAccess.Write
            End If
            Dim fullDirectoryPath As String = FullSource
            If (((OperationType = SHFileOperationType.FO_COPY) OrElse (OperationType = SHFileOperationType.FO_MOVE)) AndAlso fullDirectoryPath.EndsWith("*", StringComparison.Ordinal)) Then
                Dim trimChars As Char() = New Char() {"*"c}
                fullDirectoryPath = FileSystem.RemoveEndingSeparator(FullSource.TrimEnd(trimChars))
            End If
            If (FileOrDirectory = FileOrDirectory.Directory) Then
                FileSystem.DemandDirectoryPermission(fullDirectoryPath, noAccess)
            Else
                New FileIOPermission(noAccess, fullDirectoryPath).Demand
            End If
            If (OperationType <> SHFileOperationType.FO_DELETE) Then
                If (FileOrDirectory = FileOrDirectory.Directory) Then
                    FileSystem.DemandDirectoryPermission(FullTarget, FileIOPermissionAccess.Write)
                Else
                    New FileIOPermission(FileIOPermissionAccess.Write, FullTarget).Demand
                End If
            End If
            Dim lpFileOp As SHFILEOPSTRUCT = FileSystem.GetShellOperationInfo(OperationType, OperationFlags, FullSource, FullTarget)
            New SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Assert
            Try
                num = NativeMethods.SHFileOperation(lpFileOp)
                NativeMethods.SHChangeNotify(&H2381F, 3, IntPtr.Zero, IntPtr.Zero)
            Catch exception1 As Exception
                Throw
            Finally
                CodeAccessPermission.RevertAssert()
            End Try
            If lpFileOp.fAnyOperationsAborted Then
                If (OnUserCancel = UICancelOption.ThrowException) Then
                    Throw New OperationCanceledException
                End If
            ElseIf (num <> 0) Then
                FileSystem.ThrowWinIOError(num)
            End If
        End Sub

        Private Shared Sub ThrowIfDevicePath(ByVal path As String)
            If path.StartsWith("\\.\", StringComparison.Ordinal) Then
                Throw ExceptionUtils.GetArgumentExceptionWithArgName("path", "IO_DevicePath", New String(0 - 1) {})
            End If
        End Sub

        <SecurityCritical>
        Private Shared Sub ThrowWinIOError(ByVal errorCode As Integer)
            Select Case errorCode
                Case 2
                    Throw New FileNotFoundException
                Case 3
                    Throw New DirectoryNotFoundException
                Case 5
                    Throw New UnauthorizedAccessException
                Case 15
                    Throw New DriveNotFoundException
                Case &HCE
                    Throw New PathTooLongException
                Case &H3E3, &H4C7
                    Throw New OperationCanceledException
            End Select
            Throw New IOException(New Win32Exception(errorCode).Message, Marshal.GetHRForLastWin32Error)
        End Sub

        Private Shared Function ToUIOptionInternal(ByVal showUI As UIOption) As UIOptionInternal
            Dim option As UIOption = showUI
            If ([option] <> UIOption.OnlyErrorDialogs) Then
                If ([option] <> UIOption.AllDialogs) Then
                    Throw New InvalidEnumArgumentException("showUI", CInt(showUI), GetType(UIOption))
                End If
                Return UIOptionInternal.AllDialogs
            End If
            Return UIOptionInternal.OnlyErrorDialogs
        End Function

        Private Shared Sub VerifyDeleteDirectoryOption(ByVal argName As String, ByVal argValue As DeleteDirectoryOption)
            If ((argValue <> DeleteDirectoryOption.DeleteAllContents) AndAlso (argValue <> DeleteDirectoryOption.ThrowIfDirectoryNonEmpty)) Then
                Throw New InvalidEnumArgumentException(argName, CInt(argValue), GetType(DeleteDirectoryOption))
            End If
        End Sub

        Private Shared Sub VerifyRecycleOption(ByVal argName As String, ByVal argValue As RecycleOption)
            If ((argValue <> RecycleOption.DeletePermanently) AndAlso (argValue <> RecycleOption.SendToRecycleBin)) Then
                Throw New InvalidEnumArgumentException(argName, CInt(argValue), GetType(RecycleOption))
            End If
        End Sub

        Private Shared Sub VerifySearchOption(ByVal argName As String, ByVal argValue As SearchOption)
            If ((argValue <> SearchOption.SearchAllSubDirectories) AndAlso (argValue <> SearchOption.SearchTopLevelOnly)) Then
                Throw New InvalidEnumArgumentException(argName, CInt(argValue), GetType(SearchOption))
            End If
        End Sub

        Private Shared Sub VerifyUICancelOption(ByVal argName As String, ByVal argValue As UICancelOption)
            If ((argValue <> UICancelOption.DoNothing) AndAlso (argValue <> UICancelOption.ThrowException)) Then
                Throw New InvalidEnumArgumentException(argName, CInt(argValue), GetType(UICancelOption))
            End If
        End Sub

        Public Shared Sub WriteAllBytes(ByVal file As String, ByVal data As Byte(), ByVal append As Boolean)
            FileSystem.CheckFilePathTrailingSeparator(file, "file")
            Dim stream As FileStream = Nothing
            Try
                Dim create As FileMode
                If append Then
                    create = FileMode.Append
                Else
                    create = FileMode.Create
                End If
                stream = New FileStream(file, create, FileAccess.Write, FileShare.Read)
                stream.Write(data, 0, data.Length)
            Finally
                If (Not stream Is Nothing) Then
                    stream.Close()
                End If
            End Try
        End Sub

        Public Shared Sub WriteAllText(ByVal file As String, ByVal [text] As String, ByVal append As Boolean)
            FileSystem.WriteAllText(file, [text], append, Encoding.UTF8)
        End Sub

        Public Shared Sub WriteAllText(ByVal file As String, ByVal [text] As String, ByVal append As Boolean, ByVal encoding As Encoding)
            FileSystem.CheckFilePathTrailingSeparator(file, "file")
            Dim writer As StreamWriter = Nothing
            Try
                If (append AndAlso file.Exists(file)) Then
                    Dim reader As StreamReader = Nothing
                    Try
                        reader = New StreamReader(file, encoding, True)
                        Dim buffer As Char() = New Char(10 - 1) {}
                        reader.Read(buffer, 0, 10)
                        encoding = reader.CurrentEncoding
                    Catch exception As IOException
                    Finally
                        If (Not reader Is Nothing) Then
                            reader.Close()
                        End If
                    End Try
                End If
                writer = New StreamWriter(file, append, encoding)
                writer.Write([text])
            Finally
                If (Not writer Is Nothing) Then
                    writer.Close()
                End If
            End Try
        End Sub


        ' Properties
        Public Shared ReadOnly Property Drives As ReadOnlyCollection(Of DriveInfo)
            Get
                Dim list As New Collection(Of DriveInfo)
                Dim info As DriveInfo
                For Each info In DriveInfo.GetDrives
                    list.Add(info)
                Next
                Return New ReadOnlyCollection(Of DriveInfo)(list)
            End Get
        End Property

        Public Shared Property CurrentDirectory As String
            Get
                Return FileSystem.NormalizePath(Directory.GetCurrentDirectory)
            End Get
            Set(ByVal value As String)
                Directory.SetCurrentDirectory(value)
            End Set
        End Property


        ' Fields
        Private Const m_SHELL_OPERATION_FLAGS_BASE As ShFileOperationFlags = (ShFileOperationFlags.FOF_NO_CONNECTED_ELEMENTS Or ShFileOperationFlags.FOF_NOCONFIRMMKDIR)
        Private Const m_SHELL_OPERATION_FLAGS_HIDE_UI As ShFileOperationFlags = (ShFileOperationFlags.FOF_NOCONFIRMATION Or ShFileOperationFlags.FOF_SILENT)
        Private Const m_MOVEFILEEX_FLAGS As Integer = 11
        Private Shared ReadOnly m_SeparatorChars As Char() = New Char() {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar}

        ' Nested Types
        Private Enum CopyOrMove
            ' Fields
            Copy = 0
            Move = 1
        End Enum

        Private Class DirectoryNode
            ' Methods
            Friend Sub New(ByVal DirectoryPath As String, ByVal TargetDirectoryPath As String)
                Me.m_Path = DirectoryPath
                Me.m_TargetPath = TargetDirectoryPath
                Me.m_SubDirs = New Collection(Of DirectoryNode)
                Dim str As String
                For Each str In Directory.GetDirectories(Me.m_Path)
                    Dim targetDirectoryPath As String = Path.Combine(Me.m_TargetPath, Path.GetFileName(str))
                    Me.m_SubDirs.Add(New DirectoryNode(str, targetDirectoryPath))
                Next
            End Sub


            ' Properties
            Friend ReadOnly Property Path As String
                Get
                    Return Me.m_Path
                End Get
            End Property

            Friend ReadOnly Property TargetPath As String
                Get
                    Return Me.m_TargetPath
                End Get
            End Property

            Friend ReadOnly Property SubDirs As Collection(Of DirectoryNode)
                Get
                    Return Me.m_SubDirs
                End Get
            End Property


            ' Fields
            Private m_Path As String
            Private m_TargetPath As String
            Private m_SubDirs As Collection(Of DirectoryNode)
        End Class

        Private Enum FileOrDirectory
            ' Fields
            File = 0
            Directory = 1
        End Enum

        Private Class TextSearchHelper
            ' Methods
            Private Sub New()
                Me.m_PreviousCharBuffer = New Char(0 - 1) {}
                Me.m_CheckPreamble = True
            End Sub

            Friend Sub New(ByVal Encoding As Encoding, ByVal [Text] As String, ByVal IgnoreCase As Boolean)
                Me.m_PreviousCharBuffer = New Char(0 - 1) {}
                Me.m_CheckPreamble = True
                Me.m_Decoder = Encoding.GetDecoder
                Me.m_Preamble = Encoding.GetPreamble
                Me.m_IgnoreCase = IgnoreCase
                If Me.m_IgnoreCase Then
                    Me.m_SearchText = [Text].ToUpper(CultureInfo.CurrentCulture)
                Else
                    Me.m_SearchText = [Text]
                End If
            End Sub

            Private Shared Function BytesMatch(ByVal BigBuffer As Byte(), ByVal SmallBuffer As Byte()) As Boolean
                If ((BigBuffer.Length < SmallBuffer.Length) Or (SmallBuffer.Length = 0)) Then
                    Return False
                End If
                Dim num As Integer = (SmallBuffer.Length - 1)
                Dim i As Integer = 0
                Do While (i <= num)
                    If (BigBuffer(i) <> SmallBuffer(i)) Then
                        Return False
                    End If
                    i += 1
                Loop
                Return True
            End Function

            Friend Function IsTextFound(ByVal ByteBuffer As Byte(), ByVal Count As Integer) As Boolean
                Dim index As Integer = 0
                If Me.m_CheckPreamble Then
                    If TextSearchHelper.BytesMatch(ByteBuffer, Me.m_Preamble) Then
                        index = Me.m_Preamble.Length
                        Count = (Count - Me.m_Preamble.Length)
                    End If
                    Me.m_CheckPreamble = False
                    If (Count <= 0) Then
                        Return False
                    End If
                End If
                Dim num2 As Integer = Me.m_Decoder.GetCharCount(ByteBuffer, index, Count)
                Dim destinationArray As Char() = New Char((((Me.m_PreviousCharBuffer.Length + num2) - 1) + 1) - 1) {}
                Array.Copy(Me.m_PreviousCharBuffer, 0, destinationArray, 0, Me.m_PreviousCharBuffer.Length)
                Me.m_Decoder.GetChars(ByteBuffer, index, Count, destinationArray, Me.m_PreviousCharBuffer.Length)
                If (destinationArray.Length > Me.m_SearchText.Length) Then
                    If (Me.m_PreviousCharBuffer.Length <> Me.m_SearchText.Length) Then
                        Me.m_PreviousCharBuffer = New Char(((Me.m_SearchText.Length - 1) + 1) - 1) {}
                    End If
                    Array.Copy(destinationArray, (destinationArray.Length - Me.m_SearchText.Length), Me.m_PreviousCharBuffer, 0, Me.m_SearchText.Length)
                Else
                    Me.m_PreviousCharBuffer = destinationArray
                End If
                If Me.m_IgnoreCase Then
                    Return New String(destinationArray).ToUpper(CultureInfo.CurrentCulture).Contains(Me.m_SearchText)
                End If
                Return New String(destinationArray).Contains(Me.m_SearchText)
            End Function


            ' Fields
            Private m_SearchText As String
            Private m_IgnoreCase As Boolean
            Private m_Decoder As Decoder
            Private m_PreviousCharBuffer As Char()
            Private m_CheckPreamble As Boolean
            Private m_Preamble As Byte()
        End Class

        Private Enum UIOptionInternal
            ' Fields
            OnlyErrorDialogs = 2
            AllDialogs = 3
            NoUI = 4
        End Enum
    End Class
End Namespace

