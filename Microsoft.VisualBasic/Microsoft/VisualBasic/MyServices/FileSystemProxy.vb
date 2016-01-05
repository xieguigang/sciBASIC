Imports Microsoft.VisualBasic.FileIO
Imports System
Imports System.Collections.ObjectModel
Imports System.ComponentModel
Imports System.IO
Imports System.Security.Permissions
Imports System.Text

Namespace Microsoft.VisualBasic.MyServices
    <EditorBrowsable(EditorBrowsableState.Never), HostProtection(SecurityAction.LinkDemand, Resources:=HostProtectionResource.ExternalProcessMgmt)> _
    Public Class FileSystemProxy
        ' Methods
        Friend Sub New()
        End Sub

        Public Function CombinePath(baseDirectory As String, relativePath As String) As String
            Return FileSystem.CombinePath(baseDirectory, relativePath)
        End Function

        Public Sub CopyDirectory(sourceDirectoryName As String, destinationDirectoryName As String)
            FileSystem.CopyDirectory(sourceDirectoryName, destinationDirectoryName)
        End Sub

        Public Sub CopyDirectory(sourceDirectoryName As String, destinationDirectoryName As String, showUI As UIOption)
            FileSystem.CopyDirectory(sourceDirectoryName, destinationDirectoryName, showUI)
        End Sub

        Public Sub CopyDirectory(sourceDirectoryName As String, destinationDirectoryName As String, overwrite As Boolean)
            FileSystem.CopyDirectory(sourceDirectoryName, destinationDirectoryName, overwrite)
        End Sub

        Public Sub CopyDirectory(sourceDirectoryName As String, destinationDirectoryName As String, showUI As UIOption, onUserCancel As UICancelOption)
            FileSystem.CopyDirectory(sourceDirectoryName, destinationDirectoryName, showUI, onUserCancel)
        End Sub

        Public Sub CopyFile(sourceFileName As String, destinationFileName As String)
            FileSystem.CopyFile(sourceFileName, destinationFileName)
        End Sub

        Public Sub CopyFile(sourceFileName As String, destinationFileName As String, showUI As UIOption)
            FileSystem.CopyFile(sourceFileName, destinationFileName, showUI)
        End Sub

        Public Sub CopyFile(sourceFileName As String, destinationFileName As String, overwrite As Boolean)
            FileSystem.CopyFile(sourceFileName, destinationFileName, overwrite)
        End Sub

        Public Sub CopyFile(sourceFileName As String, destinationFileName As String, showUI As UIOption, onUserCancel As UICancelOption)
            FileSystem.CopyFile(sourceFileName, destinationFileName, showUI, onUserCancel)
        End Sub

        Public Sub CreateDirectory(directory As String)
            FileSystem.CreateDirectory(directory)
        End Sub

        Public Sub DeleteDirectory(directory As String, onDirectoryNotEmpty As DeleteDirectoryOption)
            FileSystem.DeleteDirectory(directory, onDirectoryNotEmpty)
        End Sub

        Public Sub DeleteDirectory(directory As String, showUI As UIOption, recycle As RecycleOption)
            FileSystem.DeleteDirectory(directory, showUI, recycle)
        End Sub

        Public Sub DeleteDirectory(directory As String, showUI As UIOption, recycle As RecycleOption, onUserCancel As UICancelOption)
            FileSystem.DeleteDirectory(directory, showUI, recycle, onUserCancel)
        End Sub

        Public Sub DeleteFile(file As String)
            FileSystem.DeleteFile(file)
        End Sub

        Public Sub DeleteFile(file As String, showUI As UIOption, recycle As RecycleOption)
            FileSystem.DeleteFile(file, showUI, recycle)
        End Sub

        Public Sub DeleteFile(file As String, showUI As UIOption, recycle As RecycleOption, onUserCancel As UICancelOption)
            FileSystem.DeleteFile(file, showUI, recycle, onUserCancel)
        End Sub

        Public Function DirectoryExists(directory As String) As Boolean
            Return FileSystem.DirectoryExists(directory)
        End Function

        Public Function FileExists(file As String) As Boolean
            Return FileSystem.FileExists(file)
        End Function

        Public Function FindInFiles(directory As String, containsText As String, ignoreCase As Boolean, searchType As SearchOption) As ReadOnlyCollection(Of String)
            Return FileSystem.FindInFiles(directory, containsText, ignoreCase, searchType)
        End Function

        Public Function FindInFiles(directory As String, containsText As String, ignoreCase As Boolean, searchType As SearchOption, ParamArray fileWildcards As String()) As ReadOnlyCollection(Of String)
            Return FileSystem.FindInFiles(directory, containsText, ignoreCase, searchType, fileWildcards)
        End Function

        Public Function GetDirectories(directory As String) As ReadOnlyCollection(Of String)
            Return FileSystem.GetDirectories(directory)
        End Function

        Public Function GetDirectories(directory As String, searchType As SearchOption, ParamArray wildcards As String()) As ReadOnlyCollection(Of String)
            Return FileSystem.GetDirectories(directory, searchType, wildcards)
        End Function

        Public Function GetDirectoryInfo(directory As String) As DirectoryInfo
            Return FileSystem.GetDirectoryInfo(directory)
        End Function

        Public Function GetDriveInfo(drive As String) As DriveInfo
            Return FileSystem.GetDriveInfo(drive)
        End Function

        Public Function GetFileInfo(file As String) As FileInfo
            Return FileSystem.GetFileInfo(file)
        End Function

        Public Function GetFiles(directory As String) As ReadOnlyCollection(Of String)
            Return FileSystem.GetFiles(directory)
        End Function

        Public Function GetFiles(directory As String, searchType As SearchOption, ParamArray wildcards As String()) As ReadOnlyCollection(Of String)
            Return FileSystem.GetFiles(directory, searchType, wildcards)
        End Function

        Public Function GetName(path As String) As String
            Return FileSystem.GetName(path)
        End Function

        Public Function GetParentPath(path As String) As String
            Return FileSystem.GetParentPath(path)
        End Function

        Public Function GetTempFileName() As String
            Return FileSystem.GetTempFileName
        End Function

        Public Sub MoveDirectory(sourceDirectoryName As String, destinationDirectoryName As String)
            FileSystem.MoveDirectory(sourceDirectoryName, destinationDirectoryName)
        End Sub

        Public Sub MoveDirectory(sourceDirectoryName As String, destinationDirectoryName As String, showUI As UIOption)
            FileSystem.MoveDirectory(sourceDirectoryName, destinationDirectoryName, showUI)
        End Sub

        Public Sub MoveDirectory(sourceDirectoryName As String, destinationDirectoryName As String, overwrite As Boolean)
            FileSystem.MoveDirectory(sourceDirectoryName, destinationDirectoryName, overwrite)
        End Sub

        Public Sub MoveDirectory(sourceDirectoryName As String, destinationDirectoryName As String, showUI As UIOption, onUserCancel As UICancelOption)
            FileSystem.MoveDirectory(sourceDirectoryName, destinationDirectoryName, showUI, onUserCancel)
        End Sub

        Public Sub MoveFile(sourceFileName As String, destinationFileName As String)
            FileSystem.MoveFile(sourceFileName, destinationFileName)
        End Sub

        Public Sub MoveFile(sourceFileName As String, destinationFileName As String, showUI As UIOption)
            FileSystem.MoveFile(sourceFileName, destinationFileName, showUI)
        End Sub

        Public Sub MoveFile(sourceFileName As String, destinationFileName As String, overwrite As Boolean)
            FileSystem.MoveFile(sourceFileName, destinationFileName, overwrite)
        End Sub

        Public Sub MoveFile(sourceFileName As String, destinationFileName As String, showUI As UIOption, onUserCancel As UICancelOption)
            FileSystem.MoveFile(sourceFileName, destinationFileName, showUI, onUserCancel)
        End Sub

        Public Function OpenTextFieldParser(file As String) As TextFieldParser
            Return FileSystem.OpenTextFieldParser(file)
        End Function

        Public Function OpenTextFieldParser(file As String, ParamArray fieldWidths As Integer()) As TextFieldParser
            Return FileSystem.OpenTextFieldParser(file, fieldWidths)
        End Function

        Public Function OpenTextFieldParser(file As String, ParamArray delimiters As String()) As TextFieldParser
            Return FileSystem.OpenTextFieldParser(file, delimiters)
        End Function

        Public Function OpenTextFileReader(file As String) As StreamReader
            Return FileSystem.OpenTextFileReader(file)
        End Function

        Public Function OpenTextFileReader(file As String, encoding As Encoding) As StreamReader
            Return FileSystem.OpenTextFileReader(file, encoding)
        End Function

        Public Function OpenTextFileWriter(file As String, append As Boolean) As StreamWriter
            Return FileSystem.OpenTextFileWriter(file, append)
        End Function

        Public Function OpenTextFileWriter(file As String, append As Boolean, encoding As Encoding) As StreamWriter
            Return FileSystem.OpenTextFileWriter(file, append, encoding)
        End Function

        Public Function ReadAllBytes(file As String) As Byte()
            Return FileSystem.ReadAllBytes(file)
        End Function

        Public Function ReadAllText(file As String) As String
            Return FileSystem.ReadAllText(file)
        End Function

        Public Function ReadAllText(file As String, encoding As Encoding) As String
            Return FileSystem.ReadAllText(file, encoding)
        End Function

        Public Sub RenameDirectory(directory As String, newName As String)
            FileSystem.RenameDirectory(directory, newName)
        End Sub

        Public Sub RenameFile(file As String, newName As String)
            FileSystem.RenameFile(file, newName)
        End Sub

        Public Sub WriteAllBytes(file As String, data As Byte(), append As Boolean)
            FileSystem.WriteAllBytes(file, data, append)
        End Sub

        Public Sub WriteAllText(file As String, [text] As String, append As Boolean)
            FileSystem.WriteAllText(file, [text], append)
        End Sub

        Public Sub WriteAllText(file As String, [text] As String, append As Boolean, encoding As Encoding)
            FileSystem.WriteAllText(file, [text], append, encoding)
        End Sub


        ' Properties
        Public Property CurrentDirectory As String
            Get
                Return FileSystem.CurrentDirectory
            End Get
            Set(value As String)
                FileSystem.CurrentDirectory = value
            End Set
        End Property

        Public ReadOnly Property Drives As ReadOnlyCollection(Of DriveInfo)
            Get
                Return FileSystem.Drives
            End Get
        End Property

        Public ReadOnly Property SpecialDirectories As SpecialDirectoriesProxy
            Get
                If (Me.m_SpecialDirectoriesProxy Is Nothing) Then
                    Me.m_SpecialDirectoriesProxy = New SpecialDirectoriesProxy
                End If
                Return Me.m_SpecialDirectoriesProxy
            End Get
        End Property


        ' Fields
        Private m_SpecialDirectoriesProxy As SpecialDirectoriesProxy = Nothing
    End Class
End Namespace

