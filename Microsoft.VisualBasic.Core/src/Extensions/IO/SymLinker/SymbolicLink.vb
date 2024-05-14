#Region "Microsoft.VisualBasic::3657912b74e191d47455cc60f5792cc7, Microsoft.VisualBasic.Core\src\Extensions\IO\SymLinker\SymbolicLink.vb"

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

    '   Total Lines: 138
    '    Code Lines: 98
    ' Comment Lines: 11
    '   Blank Lines: 29
    '     File Size: 5.92 KB


    '     Module SymbolicLink
    ' 
    '         Function: CreateFile, CreateSymbolicLink, DeviceIoControl, Exists, getFileHandle
    '                   GetTarget
    ' 
    '         Sub: CreateDirectoryLink, CreateFileLink
    '         Structure SymbolicLinkReparseData
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.Win32.SafeHandles
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text

Namespace FileIO.SymLinker

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>https://github.com/amd989/Symlinker</remarks>
    Public Module SymbolicLink

        Private Const genericReadAccess As UInteger = &H80000000UI

        Private Const fileFlagsForOpenReparsePointAndBackupSemantics As UInteger = &H2200000

        Private Const ioctlCommandGetReparsePoint As Integer = &H900A8

        Private Const openExisting As UInteger = &H3

        Private Const pathNotAReparsePointError As UInteger = &H80071126UI

        ''' <summary>
        ''' Read, Write, Delete
        ''' </summary>
        Private Const shareModeAll As UInteger = &H7

        ''' <summary>
        ''' 
        ''' </summary>
        Private Const symLinkTag As UInteger = &HA000000CUI

        Private Const targetIsAFile As Integer = 0

        Private Const targetIsADirectory As Integer = 1

        <DllImport("kernel32.dll", SetLastError:=True)>
        Private Function CreateFile(lpFileName As String, dwDesiredAccess As UInteger, dwShareMode As UInteger, lpSecurityAttributes As IntPtr, dwCreationDisposition As UInteger, dwFlagsAndAttributes As UInteger,
            hTemplateFile As IntPtr) As SafeFileHandle
        End Function

        <DllImport("kernel32.dll", SetLastError:=True)>
        Private Function CreateSymbolicLink(lpSymlinkFileName As String, lpTargetFileName As String, dwFlags As Integer) As Boolean
        End Function

        <DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
        Private Function DeviceIoControl(hDevice As IntPtr, dwIoControlCode As UInteger, lpInBuffer As IntPtr, nInBufferSize As Integer, lpOutBuffer As IntPtr, nOutBufferSize As Integer,
            ByRef lpBytesReturned As Integer, lpOverlapped As IntPtr) As Boolean
        End Function

        <StructLayout(LayoutKind.Sequential)>
        Public Structure SymbolicLinkReparseData
            ' Not certain about this!
            Private Const maxUnicodePathLength As Integer = 260 * 2

            Public ReparseTag As UInteger
            Public ReparseDataLength As UShort
            Public Reserved As UShort
            Public SubstituteNameOffset As UShort
            Public SubstituteNameLength As UShort
            Public PrintNameOffset As UShort
            Public PrintNameLength As UShort
            Public Flags As UInteger

            <MarshalAs(UnmanagedType.ByValArray, SizeConst:=maxUnicodePathLength)>
            Public PathBuffer As Byte()
        End Structure

        Public Sub CreateDirectoryLink(linkPath As String, targetPath As String)
            If Not CreateSymbolicLink(linkPath, targetPath, targetIsADirectory) OrElse Marshal.GetLastWin32Error() <> 0 Then
                Try
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error())
                Catch exception As COMException
                    Throw New IOException(exception.Message, exception)
                End Try
            End If
        End Sub

        Public Sub CreateFileLink(linkPath As String, targetPath As String)
            If Not CreateSymbolicLink(linkPath, targetPath, targetIsAFile) Then
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error())
            End If
        End Sub

        Public Function Exists(path As String) As Boolean
            If Not Directory.Exists(path) AndAlso Not File.Exists(path) Then
                Return False
            End If
            Dim target As String = GetTarget(path)
            Return target IsNot Nothing
        End Function

        Private Function getFileHandle(path As String) As SafeFileHandle
            Return CreateFile(path, genericReadAccess, shareModeAll, IntPtr.Zero, openExisting, fileFlagsForOpenReparsePointAndBackupSemantics,
                IntPtr.Zero)
        End Function

        Public Function GetTarget(path As String) As String
            Dim reparseDataBuffer As SymbolicLinkReparseData

            Using fileHandle As SafeFileHandle = getFileHandle(path)
                If fileHandle.IsInvalid Then
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error())
                End If

                Dim outBufferSize As Integer = Marshal.SizeOf(GetType(SymbolicLinkReparseData))
                Dim outBuffer As IntPtr = IntPtr.Zero
                Try
                    outBuffer = Marshal.AllocHGlobal(outBufferSize)
                    Dim bytesReturned As Integer
                    Dim success As Boolean = DeviceIoControl(fileHandle.DangerousGetHandle(), ioctlCommandGetReparsePoint, IntPtr.Zero, 0, outBuffer, outBufferSize,
                        bytesReturned, IntPtr.Zero)

                    fileHandle.Close()

                    If Not success Then
                        If CUInt(Marshal.GetHRForLastWin32Error()) = pathNotAReparsePointError Then
                            Return Nothing
                        End If
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error())
                    End If

                    reparseDataBuffer = CType(Marshal.PtrToStructure(outBuffer, GetType(SymbolicLinkReparseData)), SymbolicLinkReparseData)
                Finally
                    Marshal.FreeHGlobal(outBuffer)
                End Try
            End Using
            If reparseDataBuffer.ReparseTag <> symLinkTag Then
                Return Nothing
            End If

            Dim target As String = Encoding.Unicode.GetString(reparseDataBuffer.PathBuffer, reparseDataBuffer.PrintNameOffset, reparseDataBuffer.PrintNameLength)

            Return target
        End Function
    End Module
End Namespace
