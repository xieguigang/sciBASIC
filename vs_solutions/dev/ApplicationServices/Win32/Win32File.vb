#Region "Microsoft.VisualBasic::5fabdcf92fe163a9b8eff4cd5bd91ce1, sciBASIC#\vs_solutions\dev\ApplicationServices\Win32\Win32File.vb"

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

    '   Total Lines: 283
    '    Code Lines: 182
    ' Comment Lines: 56
    '   Blank Lines: 45
    '     File Size: 11.03 KB


    '     Module Win32File
    ' 
    '         Function: CreateFileW, GetAccess, GetMode, GetShare, (+3 Overloads) Open
    '                   OpenRead, OpenWrite, ReadAllText, SetFilePointer
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports System.Text
Imports Microsoft.Win32.SafeHandles
Imports System.Runtime.InteropServices
Imports System.Runtime.InteropServices.Marshal
Imports System.IO

Namespace Win32

    ''' <summary>
    ''' .NET 2.0 Workaround for PathTooLongException
    ''' </summary>
    ''' <remarks>
    ''' http://www.codeproject.com/Articles/22013/NET-Workaround-for-PathTooLongException
    ''' </remarks>
    Public Module Win32File

        ''' <summary>
        ''' Error
        ''' </summary>
        Const ERROR_ALREADY_EXISTS As Integer = 183
        ' seek location
        Const FILE_BEGIN As UInteger = &H0
        Const FILE_CURRENT As UInteger = &H1
        Const FILE_END As UInteger = &H2


        ' access
        Const GENERIC_READ As UInteger = &H80000000UI
        Const GENERIC_WRITE As UInteger = &H40000000
        Const GENERIC_EXECUTE As UInteger = &H20000000
        Const GENERIC_ALL As UInteger = &H10000000

        Const FILE_APPEND_DATA As UInteger = &H4

        ' attribute
        Const FILE_ATTRIBUTE_NORMAL As UInteger = &H80

        ' share
        Const FILE_SHARE_DELETE As UInteger = &H4
        Const FILE_SHARE_READ As UInteger = &H1
        Const FILE_SHARE_WRITE As UInteger = &H2

        'mode
        Const CREATE_NEW As UInteger = 1
        Const CREATE_ALWAYS As UInteger = 2
        Const OPEN_EXISTING As UInteger = 3
        Const OPEN_ALWAYS As UInteger = 4
        Const TRUNCATE_EXISTING As UInteger = 5


        <DllImport("kernel32.dll", SetLastError:=True, CharSet:=CharSet.Unicode)>
        Private Function CreateFileW(lpFileName As String, dwDesiredAccess As UInteger, dwShareMode As UInteger, lpSecurityAttributes As IntPtr, dwCreationDisposition As UInteger, dwFlagsAndAttributes As UInteger, hTemplateFile As IntPtr) As SafeFileHandle
        End Function

        <DllImport("kernel32.dll", SetLastError:=True, CharSet:=CharSet.Unicode)>
        Private Function SetFilePointer(hFile As SafeFileHandle, lDistanceToMove As Long, lpDistanceToMoveHigh As IntPtr, dwMoveMethod As UInteger) As UInteger
        End Function

#Region "GetMode"

        ''' <summary>
        ''' Converts the filemode constant to win32 constant
        ''' </summary>
        ''' <param name="mode"></param>
        ''' <returns></returns>
        Private Function GetMode(mode As FileMode) As UInteger
            Dim umode As UInteger = 0
            Select Case mode
                Case FileMode.CreateNew
                    umode = CREATE_NEW

                Case FileMode.Create
                    umode = CREATE_ALWAYS

                Case FileMode.Append
                    umode = OPEN_ALWAYS

                Case FileMode.Open
                    umode = OPEN_EXISTING

                Case FileMode.OpenOrCreate
                    umode = OPEN_ALWAYS

                Case FileMode.Truncate
                    umode = TRUNCATE_EXISTING

            End Select
            Return umode
        End Function
#End Region

        Public Function ReadAllText(path As String, encoding As System.Text.Encoding) As String
            On Error Resume Next

            Dim FileStream = Win32File.OpenRead(path)
            Dim ChunkBuffer As Byte() = New Byte(FileStream.Length - 1) {}
            Call FileStream.Read(ChunkBuffer, 0, ChunkBuffer.Length)
            Return encoding.GetString(ChunkBuffer)
        End Function

#Region "GetAccess"

        ''' <summary>
        ''' Converts the FileAccess constant to win32 constant
        ''' </summary>
        ''' <param name="access"></param>
        ''' <returns></returns>
        Private Function GetAccess(access As FileAccess) As UInteger
            Dim uaccess As UInteger = 0
            Select Case access
                Case FileAccess.Read
                    uaccess = GENERIC_READ

                Case FileAccess.ReadWrite
                    uaccess = GENERIC_READ Or GENERIC_WRITE

                Case FileAccess.Write
                    uaccess = GENERIC_WRITE

            End Select
            Return uaccess
        End Function
#End Region

#Region "GetShare"

        ''' <summary>
        ''' Converts the FileShare constant to win32 constant
        ''' </summary>
        ''' <param name="share"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Function GetShare(share As FileShare) As UInteger
            Dim ushare As UInteger = 0
            Select Case share
                Case FileShare.Read
                    ushare = FILE_SHARE_READ

                Case FileShare.ReadWrite
                    ushare = FILE_SHARE_READ Or FILE_SHARE_WRITE

                Case FileShare.Write
                    ushare = FILE_SHARE_WRITE

                Case FileShare.Delete
                    ushare = FILE_SHARE_DELETE

                Case FileShare.None
                    ushare = 0

            End Select
            Return ushare
        End Function
#End Region

        Const ushare As UInteger = 0

        Public Function Open(filepath As String, mode As FileMode) As FileStream
            'opened in the specified mode and path, with read/write access and not shared
            Dim fs As FileStream = Nothing
            Dim umode As UInteger = GetMode(mode)
            Dim uaccess As UInteger = GENERIC_READ Or GENERIC_WRITE

            'not shared
            If mode = FileMode.Append Then
                uaccess = FILE_APPEND_DATA
            End If
            ' If file path is disk file path then prepend it with \\?\
            ' if file path is UNC prepend it with \\?\UNC\ and remove \\ prefix in unc path.
            If filepath.StartsWith("\\") Then
                filepath = "\\?\UNC\" & filepath.Substring(2, filepath.Length - 2)
            Else
                filepath = "\\?\" & filepath
            End If
            Dim sh As SafeFileHandle = CreateFileW(filepath, uaccess, ushare, IntPtr.Zero, umode, FILE_ATTRIBUTE_NORMAL,
            IntPtr.Zero)
            Dim iError As Integer = GetLastWin32Error()
            If (iError > 0 AndAlso Not (mode = FileMode.Append AndAlso iError = ERROR_ALREADY_EXISTS)) OrElse sh.IsInvalid Then
                Throw New Exception("Error opening file Win32 Error:" & iError)
            Else
                fs = New FileStream(sh, FileAccess.ReadWrite)
            End If

            ' if opened in append mode
            If mode = FileMode.Append Then
                If Not sh.IsInvalid Then
                    SetFilePointer(sh, 0, IntPtr.Zero, FILE_END)
                End If
            End If

            Return fs
        End Function

        Public Function Open(filepath As String, mode As FileMode, access As FileAccess) As FileStream
            'opened in the specified mode and access and not shared
            Dim fs As FileStream = Nothing
            Dim umode As UInteger = GetMode(mode)
            Dim uaccess As UInteger = GetAccess(access)

            'not shared
            If mode = FileMode.Append Then
                uaccess = FILE_APPEND_DATA
            End If
            ' If file path is disk file path then prepend it with \\?\
            ' if file path is UNC prepend it with \\?\UNC\ and remove \\ prefix in unc path.
            If filepath.StartsWith("\\") Then
                filepath = "\\?\UNC\" & filepath.Substring(2, filepath.Length - 2)
            Else
                filepath = "\\?\" & filepath
            End If
            Dim sh As SafeFileHandle = CreateFileW(filepath, uaccess, ushare, IntPtr.Zero, umode, FILE_ATTRIBUTE_NORMAL,
            IntPtr.Zero)
            Dim iError As Integer = GetLastWin32Error()
            If (iError > 0 AndAlso Not (mode = FileMode.Append AndAlso iError <> ERROR_ALREADY_EXISTS)) OrElse sh.IsInvalid Then
                Throw New Exception("Error opening file Win32 Error:" & iError)
            Else
                fs = New FileStream(sh, access)
            End If
            ' if opened in append mode
            If mode = FileMode.Append Then
                If Not sh.IsInvalid Then
                    SetFilePointer(sh, 0, IntPtr.Zero, FILE_END)
                End If
            End If
            Return fs

        End Function

        Public Function Open(filepath As String, mode As FileMode, access As FileAccess, share As FileShare) As FileStream
            'opened in the specified mode , access and  share
            Dim fs As FileStream = Nothing
            Dim umode As UInteger = GetMode(mode)
            Dim uaccess As UInteger = GetAccess(access)
            Dim ushare As UInteger = GetShare(share)
            If mode = FileMode.Append Then
                uaccess = FILE_APPEND_DATA
            End If
            ' If file path is disk file path then prepend it with \\?\
            ' if file path is UNC prepend it with \\?\UNC\ and remove \\ prefix in unc path.
            If filepath.StartsWith("\\") Then
                filepath = "\\?\UNC\" & filepath.Substring(2, filepath.Length - 2)
            Else
                filepath = "\\?\" & filepath
            End If
            Dim sh As SafeFileHandle = CreateFileW(filepath, uaccess, ushare, IntPtr.Zero, umode, FILE_ATTRIBUTE_NORMAL,
            IntPtr.Zero)
            Dim iError As Integer = GetLastWin32Error()
            If (iError > 0 AndAlso Not (mode = FileMode.Append AndAlso iError <> ERROR_ALREADY_EXISTS)) OrElse sh.IsInvalid Then
                Throw New Exception("Error opening file Win32 Error:" & iError)
            Else
                fs = New FileStream(sh, access)
            End If
            ' if opened in append mode
            If mode = FileMode.Append Then
                If Not sh.IsInvalid Then
                    SetFilePointer(sh, 0, IntPtr.Zero, FILE_END)
                End If
            End If
            Return fs
        End Function

        ''' <summary>
        ''' Open readonly file mode open(String, FileMode.Open, FileAccess.Read, FileShare.Read)
        ''' </summary>
        ''' <param name="filepath"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function OpenRead(filepath As String) As FileStream
            Return Open(filepath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read)
        End Function

        ''' <summary>
        ''' open writable open(String, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None).
        ''' </summary>
        ''' <param name="filepath"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function OpenWrite(filepath As String) As FileStream
            Return Open(filepath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None)
        End Function
    End Module
End Namespace
