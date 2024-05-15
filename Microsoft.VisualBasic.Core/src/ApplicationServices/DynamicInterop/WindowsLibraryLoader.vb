#Region "Microsoft.VisualBasic::23c59d3ec87c022b996364f84daccc49, Microsoft.VisualBasic.Core\src\ApplicationServices\DynamicInterop\WindowsLibraryLoader.vb"

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

    '   Total Lines: 88
    '    Code Lines: 58
    ' Comment Lines: 13
    '   Blank Lines: 17
    '     File Size: 3.55 KB


    '     Class WindowsLibraryLoader
    ' 
    '         Function: FreeLibrary, GetFunctionAddress, GetLastError, GetShortPath, LoadLibrary
    '                   ToString
    ' 
    '     Module Win32
    ' 
    '         Function: FreeLibrary, GetProcAddress, GetShortPathName, LoadLibrary
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Runtime.ConstrainedExecution
Imports System.Runtime.InteropServices
Imports System.Security.Permissions
Imports System.Text

Namespace ApplicationServices.DynamicInterop

    <SecurityPermission(SecurityAction.Demand, Flags:=SecurityPermissionFlag.UnmanagedCode)>
    Public Class WindowsLibraryLoader : Implements IDynamicLibraryLoader

        Public Function LoadLibrary(filename As String) As IntPtr Implements IDynamicLibraryLoader.LoadLibrary
            'new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            Dim handle = Win32.LoadLibrary(filename)

            If handle = IntPtr.Zero Then
                Dim ex As New Win32Exception(Marshal.GetLastWin32Error())
                Dim [error] = ex.Message

                App.LogException(ex)
                VBDebugger.EchoLine([error])
            End If

            Return handle
        End Function

        Public Overrides Function ToString() As String
            Return "WIN32<kernel32.dll>"
        End Function

        Public Function GetLastError() As String Implements IDynamicLibraryLoader.GetLastError
            ' see for instance http://blogs.msdn.com/b/shawnfa/archive/2004/09/10/227995.aspx 
            ' and http://blogs.msdn.com/b/adam_nathan/archive/2003/04/25/56643.aspx
            ' TODO: does this work as expected with Mono+Windows stack?
            Return New Win32Exception().Message
        End Function

        Public Function FreeLibrary(handle As IntPtr) As Boolean Implements IDynamicLibraryLoader.FreeLibrary
            Return Win32.FreeLibrary(handle)
        End Function

        ''' <summary>
        ''' GetProcAddress
        ''' </summary>
        ''' <param name="hModule"></param>
        ''' <param name="lpProcName"></param>
        ''' <returns></returns>
        Public Function GetFunctionAddress(hModule As IntPtr, lpProcName As String) As IntPtr Implements IDynamicLibraryLoader.GetFunctionAddress
            Return GetProcAddress(hModule, lpProcName)
        End Function

        Public Shared Function GetShortPath(path As String) As String
            Dim shortPath = New StringBuilder(MaxPathLength)
            GetShortPathName(path, shortPath, MaxPathLength)
            Return shortPath.ToString()
        End Function
    End Class

    Friend Module Win32

        <DllImport("kernel32.dll", SetLastError:=True)>
        Public Function LoadLibrary(
        <MarshalAs(UnmanagedType.LPStr)> lpFileName As String) As IntPtr
        End Function

        <DllImport("kernel32.dll")>
        <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        Public Function FreeLibrary(hModule As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        <DllImport("kernel32.dll")>
        Public Function GetProcAddress(hModule As IntPtr,
        <MarshalAs(UnmanagedType.LPStr)> lpProcName As String) As IntPtr
        End Function

        ''' <summary>
        ''' MaxPath is 248. 
        ''' </summary>
        Public Const MaxPathLength As Integer = 248
        Public Const MaxFileName As Integer = 260

        <DllImport("kernel32.dll", CharSet:=CharSet.Auto)>
        Public Function GetShortPathName(
        <MarshalAs(UnmanagedType.LPTStr)> path As String,
        <MarshalAs(UnmanagedType.LPTStr)> shortPath As StringBuilder, shortPathLength As Integer) As Integer
        End Function
    End Module
End Namespace
