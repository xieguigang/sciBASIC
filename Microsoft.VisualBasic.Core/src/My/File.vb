﻿#Region "Microsoft.VisualBasic::25b92c203a2378a7e45ae2b9c1ed6062, Microsoft.VisualBasic.Core\src\My\File.vb"

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

    '   Total Lines: 127
    '    Code Lines: 82 (64.57%)
    ' Comment Lines: 25 (19.69%)
    '    - Xml Docs: 96.00%
    ' 
    '   Blank Lines: 20 (15.75%)
    '     File Size: 4.15 KB


    '     Module File
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: FileOpened, GetHandle, OpenHandle, OpenTemp, Wait
    ' 
    '         Sub: Close
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash.FileSystem
Imports Microsoft.VisualBasic.Text

Namespace My

    ''' <summary>
    ''' A internal file handler
    ''' </summary>
    Module File

        ReadOnly opendHandles As Dictionary(Of Integer, FileHandle)

        Sub New()
            opendHandles = New Dictionary(Of Integer, FileHandle)
        End Sub

        Friend Function GetHandle(path As Integer) As FileHandle
            If Not opendHandles.ContainsKey(path) Then
                Throw New ObjectNotFoundException($"Path {path} pointer to a null file handle!")
            Else
                Return opendHandles(path)
            End If
        End Function

        ''' <summary>
        ''' 不存在的文件句柄会在这个函数之中被忽略掉
        ''' </summary>
        ''' <param name="file%"></param>
        Public Sub Close(file%)
            SyncLock opendHandles
                With opendHandles
                    If .ContainsKey(file) Then
                        Call .Remove(file)
                    Else
                        ' Do Nothing.
                    End If
                End With
            End SyncLock
        End Sub

        Dim handle As Value(Of Integer) = New Value(Of Integer)(Integer.MinValue)

        ''' <summary>
        ''' Open a file system handle
        ''' </summary>
        ''' <param name="file"></param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        <Extension>
        Public Function OpenHandle(file$, Optional encoding As Encodings = Encodings.UTF8) As Integer
            Dim n As Integer

            If String.IsNullOrEmpty(file) Then
                Throw New NullReferenceException("File handle null pointer!")
            Else
                SyncLock My.File.handle
                    My.File.handle.Value += 1
                    n = My.File.handle
                End SyncLock
            End If

            Dim handle As New FileHandle With {
                .encoding = encoding.CodePage,
                .FileName = file,
                .handle = n
            }

            SyncLock opendHandles
                Call opendHandles.Add(n, handle)
            End SyncLock

            Call FileIO.FileSystem.CreateDirectory(file.ParentPath)

            Return n
        End Function

        Public Function OpenTemp() As Integer
            Return OpenHandle(TempFileSystem.GetAppSysTempFile(App.Process.Id))
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
