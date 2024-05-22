#Region "Microsoft.VisualBasic::94e9d675d53833b395d86af2ab3aba1d, Microsoft.VisualBasic.Core\src\ApplicationServices\DynamicInterop\UnixLibraryLoader.vb"

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

    '   Total Lines: 89
    '    Code Lines: 52 (58.43%)
    ' Comment Lines: 21 (23.60%)
    '    - Xml Docs: 42.86%
    ' 
    '   Blank Lines: 16 (17.98%)
    '     File Size: 4.09 KB


    '     Class UnixLibraryLoader
    ' 
    '         Function: dlclose, dlerror, dlopen, dlsym, FreeLibrary
    '                   GetFunctionAddress, GetLastError, getSearchPaths, InternalLoadLibrary, LoadLibrary
    '                   ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.ConstrainedExecution
Imports System.Runtime.InteropServices
Imports System.Security.Permissions

Namespace ApplicationServices.DynamicInterop

    <SecurityPermission(SecurityAction.Demand, Flags:=SecurityPermissionFlag.UnmanagedCode)>
    Public Class UnixLibraryLoader : Implements IDynamicLibraryLoader

        Public Const RTLD_LAZY = &H1
        Public Const RTLD_NOW As Integer = &H2
        Public Const RTLD_GLOBAL As Integer = &H100

        Public dlopen_flag As Integer = RTLD_LAZY

        Public Function LoadLibrary(filename As String) As IntPtr Implements IDynamicLibraryLoader.LoadLibrary
            Return InternalLoadLibrary(filename, dlopen_flag)
        End Function

        ''' <summary>
        ''' Gets the last error. NOTE: according to http://tldp.org/HOWTO/Program-Library-HOWTO/dl-libraries.html, returns NULL if called more than once after dlopen.
        ''' </summary>
        ''' <returns>The last error.</returns>
        Public Function GetLastError() As String Implements IDynamicLibraryLoader.GetLastError
            Return dlerror()
        End Function

        ''' <summary>
        ''' Unloads a library
        ''' </summary>
        ''' <param name="handle">The pointer resulting from loading the library</param>
        ''' <returns>True if the function dlclose returned 0</returns>
        Public Function FreeLibrary(handle As IntPtr) As Boolean Implements IDynamicLibraryLoader.FreeLibrary
            ' according to the manual page on a Debian box
            ' The function dlclose() returns 0 on success, and nonzero on error.
            Dim status = dlclose(handle)
            Return status = 0
        End Function

        Public Function GetFunctionAddress(hModule As IntPtr, lpProcName As String) As IntPtr Implements IDynamicLibraryLoader.GetFunctionAddress
            Return dlsym(hModule, lpProcName)
        End Function

        Friend Shared Function InternalLoadLibrary(filename As String, dlopen_flag As Integer) As IntPtr
            '            if (filename.StartsWith ("/")) {
            '                return dlopen (filename, RTLD_LAZY);
            '            }
            '            var searchPaths = getSearchPaths ("LD_LIBRARY_PATH");
            '            searchPaths.AddRange (getSearchPaths ("PATH"));
            '            var dll = searchPaths.Select (directory => Path.Combine (directory, filename)).FirstOrDefault (File.Exists);
            '            if (dll == null) {
            '                throw new DllNotFoundException ("Could not find the file: " + filename + " on the search path.  Checked these directories:\n "
            '                + String.Join ("\n", searchPaths));
            '            }

            Dim result = dlopen(filename, dlopen_flag)
            Return result
        End Function

        Private Shared Function getSearchPaths(pathsEnvVar As String) As List(Of String)
            Dim searchPaths = If(Environment.GetEnvironmentVariable(pathsEnvVar), "").Split(Path.PathSeparator).ToList()
            Return searchPaths
        End Function

        Public Overrides Function ToString() As String
            Return $"UNIX<libdl.so>"
        End Function

        <DllImport("libdl")>
        Private Shared Function dlopen(
        <MarshalAs(UnmanagedType.LPStr)> filename As String, flag As Integer) As IntPtr
        End Function

        <DllImport("libdl")>
        Private Shared Function dlerror() As <MarshalAs(UnmanagedType.LPStr)> String
        End Function

        <DllImport("libdl", EntryPoint:="dlclose")>
        <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        Private Shared Function dlclose(hModule As IntPtr) As Integer
        End Function

        <DllImport("libdl", EntryPoint:="dlsym")>
        Private Shared Function dlsym(hModule As IntPtr,
        <MarshalAs(UnmanagedType.LPStr)> lpProcName As String) As IntPtr
        End Function
    End Class
End Namespace
