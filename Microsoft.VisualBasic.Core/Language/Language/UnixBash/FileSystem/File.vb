#Region "Microsoft.VisualBasic::adeed41ba1eaafc5aead39fac5f18d11, Microsoft.VisualBasic.Core\Language\Language\UnixBash\FileSystem\File.vb"

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

    '     Class File
    ' 
    '         Function: Save
    '         Operators: <, >, >>
    ' 
    '     Structure FileHandle
    ' 
    '         Properties: IsDirectory, IsFile, IsHTTP
    ' 
    '         Function: ToString
    ' 
    '     Module FileHandles
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: __getHandle, FileOpened, OpenHandle, OpenTemp, Wait
    ' 
    '         Sub: Close
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Threading
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Namespace Language.UnixBash.FileSystem

    ''' <summary>
    ''' Asbtract file IO model
    ''' </summary>
    Public MustInherit Class File : Inherits BaseClass
        Implements ISaveHandle

        Public Function Save(Optional Path As String = "", Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return Save(Path, encoding.CodePage)
        End Function

        Public MustOverride Function Save(Optional Path As String = "", Optional encoding As Encoding = Nothing) As Boolean Implements ISaveHandle.Save

        Public Shared Operator >(file As File, path As String) As Boolean
            Return file.Save(path, Encodings.UTF8)
        End Operator

        Public Shared Operator <(file As File, path As String) As Boolean
            Throw New NotImplementedException
        End Operator

        Public Shared Operator >>(file As File, path As Integer) As Boolean
            Dim handle As FileHandle = __getHandle(path)
            Return file.Save(handle.FileName, handle.encoding)
        End Operator
    End Class

    ''' <summary>
    ''' 文件系统对象的句柄
    ''' </summary>
    Public Structure FileHandle
        Dim FileName As String
        Dim handle As Integer
        Dim encoding As Encoding

        ''' <summary>
        ''' Determined that is this filename is a network location.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsHTTP As Boolean
            Get
                Return FileName.isURL
            End Get
        End Property

        Public ReadOnly Property IsFile As Boolean
            Get
                Return FileName.FileExists
            End Get
        End Property

        Public ReadOnly Property IsDirectory As Boolean
            Get
                Return FileName.DirectoryExists
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure

    Public Module FileHandles

        ReadOnly ___opendHandles As Dictionary(Of Integer, FileHandle)

        Sub New()
            ___opendHandles = New Dictionary(Of Integer, FileHandle)
        End Sub

        Friend Function __getHandle(path As Integer) As FileHandle
            If Not FileHandles.___opendHandles.ContainsKey(path) Then
                Throw New ObjectNotFoundException($"Path {path} pointer to a null file handle!")
            Else
                Return FileHandles.___opendHandles(path)
            End If
        End Function

        ''' <summary>
        ''' 不存在的文件句柄会在这个函数之中被忽略掉
        ''' </summary>
        ''' <param name="file%"></param>
        Public Sub Close(file%)
            SyncLock ___opendHandles
                With ___opendHandles
                    If .ContainsKey(file) Then
                        Call .Remove(file)
                    Else
                        ' Do Nothing.
                    End If
                End With
            End SyncLock
        End Sub

        Dim __handle As Value(Of Integer) = New Value(Of Integer)(Integer.MinValue)

        ''' <summary>
        ''' Open a file system handle
        ''' </summary>
        ''' <param name="file"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        <Extension> Public Function OpenHandle(file$, Optional encoding As Encodings = Encodings.UTF8) As Integer
            If String.IsNullOrEmpty(file) Then
                Throw New NullReferenceException("File handle null pointer!")
            End If

            SyncLock ___opendHandles
                SyncLock __handle
                    __handle.value += 1

                    Dim handle As New FileHandle With {
                        .encoding = encoding.CodePage,
                        .FileName = file,
                        .handle = __handle.value
                    }

                    Call ___opendHandles.Add(__handle.value, handle)
                    Call FileIO.FileSystem.CreateDirectory(file.ParentPath)

                    Return __handle.value
                End SyncLock
            End SyncLock
        End Function

        Public Function OpenTemp() As Integer
            Return OpenHandle(App.GetAppSysTempFile(App.Process.Id))
        End Function

        ''' <summary>
        ''' Is this file opened
        ''' </summary>
        ''' <param name="filename"></param>
        ''' <returns></returns>
        <Extension> Public Function FileOpened(filename As String) As Boolean
            If Not filename.FileExists Then
                Return False
            Else
                Try
                    Using file As New IO.FileStream(filename, IO.FileMode.OpenOrCreate)
                    End Using

                    Return False
                Catch ex As Exception
                    Return True
                Finally
                End Try
            End If
        End Function

        ''' <summary>
        ''' 等待文件句柄的关闭
        ''' </summary>
        ''' <param name="file$"></param>
        ''' <param name="timeout">等待的时间长度，默认为100s，单位为毫秒</param>
        ''' <returns></returns>
        Public Function Wait(file$, Optional timeout& = 1000 * 100) As Boolean
            Dim sw As Stopwatch = Stopwatch.StartNew

            Do While file.FileOpened
                Call Thread.Sleep(1)

                If sw.ElapsedMilliseconds >= timeout Then
                    Return False
                End If
            Loop

            Return True
        End Function
    End Module
End Namespace
