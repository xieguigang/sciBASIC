#Region "Microsoft.VisualBasic::239dc17c410ef29585c67b238cb7a8e0, Microsoft.VisualBasic.Core\src\ApplicationServices\FileSystem\Fs\IWorkspace.vb"

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

'   Total Lines: 15
'    Code Lines: 5 (33.33%)
' Comment Lines: 7 (46.67%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 3 (20.00%)
'     File Size: 369 B


'     Interface IWorkspace
' 
'         Properties: Workspace
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices

Namespace ApplicationServices

    ''' <summary>
    ''' A abstract reference to a workdir
    ''' </summary>
    Public Interface IWorkspace

        ''' <summary>
        ''' the workdir path of current workspace object
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property Workspace As String

    End Interface

    Public Class FileSystemView : Implements IWorkspace, IFileSystemEnvironment

        Public ReadOnly Property Workspace As String Implements IWorkspace.Workspace
        Public ReadOnly Property [readonly] As Boolean Implements IFileSystemEnvironment.readonly

        ReadOnly fs As IFileSystemEnvironment

        Sub New(fs As IFileSystemEnvironment, subdir As String)
            Me.fs = fs
            Me.Workspace = Strings.Trim(subdir) _
                .Replace("\", "/") _
                .StringReplace("/{2,}", "/") _
                .Trim("/"c)
        End Sub

        Public Sub Close() Implements IFileSystemEnvironment.Close
            Call fs.Close()
        End Sub

        Public Sub Flush() Implements IFileSystemEnvironment.Flush
            Call fs.Flush()
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function OpenFile(path As String, Optional mode As FileMode = FileMode.OpenOrCreate, Optional access As FileAccess = FileAccess.Read) As Stream Implements IFileSystemEnvironment.OpenFile
            Return fs.OpenFile($"{Workspace}/{path}", mode, access)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function DeleteFile(path As String) As Boolean Implements IFileSystemEnvironment.DeleteFile
            Return fs.DeleteFile($"{Workspace}/{path}")
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function FileExists(path As String, Optional ZERO_Nonexists As Boolean = False) As Boolean Implements IFileSystemEnvironment.FileExists
            Return fs.FileExists($"{Workspace}/{path}", ZERO_Nonexists)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function FileSize(path As String) As Long Implements IFileSystemEnvironment.FileSize
            Return fs.FileSize($"{Workspace}/{path}")
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function FileModifyTime(path As String) As Date Implements IFileSystemEnvironment.FileModifyTime
            Return fs.FileModifyTime($"{Workspace}/{path}")
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetFullPath(filename As String) As String Implements IFileSystemEnvironment.GetFullPath
            Return fs.GetFullPath($"{Workspace}/{filename}")
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function WriteText(text As String, path As String) As Boolean Implements IFileSystemEnvironment.WriteText
            Return fs.WriteText(text, $"{Workspace}/{path}")
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ReadAllText(path As String) As String Implements IFileSystemEnvironment.ReadAllText
            Return fs.ReadAllText($"{Workspace}/{path}")
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetFiles() As IEnumerable(Of String) Implements IFileSystemEnvironment.GetFiles
            Return relativeToView(fs.GetFiles(Workspace))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetFiles(subdir As String, ParamArray exts() As String) As IEnumerable(Of String) Implements IFileSystemEnvironment.GetFiles
            Return relativeToView(fs.GetFiles($"{Workspace}/{subdir}", exts))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function EnumerateFiles(subdir As String, ParamArray exts() As String) As IEnumerable(Of String) Implements IFileSystemEnvironment.EnumerateFiles
            Return relativeToView(fs.EnumerateFiles($"{Workspace}/{subdir}", exts))
        End Function

        Private Iterator Function relativeToView(files As IEnumerable(Of String)) As IEnumerable(Of String)
            For Each file As String In files
                Yield "/" & file.Replace(Workspace, "/").Trim
            Next
        End Function
    End Class
End Namespace
