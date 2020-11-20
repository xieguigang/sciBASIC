#Region "Microsoft.VisualBasic::f1309258703d5ac772bb9979e975c5da, Microsoft.VisualBasic.Core\ApplicationServices\DynamicInterop\UnixLibraryLoader.vb"

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

    '     Class UnixLibraryLoader
    ' 
    '         Function: dlclose, dlerror, dlopen, dlsym, FreeLibrary
    '                   GetFunctionAddress, GetLastError, getSearchPaths, InternalLoadLibrary, LoadLibrary
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
    Friend Class UnixLibraryLoader : Implements IDynamicLibraryLoader

        Public Function LoadLibrary(ByVal filename As String) As IntPtr Implements IDynamicLibraryLoader.LoadLibrary
            Return InternalLoadLibrary(filename)
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
        Public Function FreeLibrary(ByVal handle As IntPtr) As Boolean Implements IDynamicLibraryLoader.FreeLibrary
            ' according to the manual page on a Debian box
            ' The function dlclose() returns 0 on success, and nonzero on error.
            Dim status = dlclose(handle)
            Return status = 0
        End Function

        Public Function GetFunctionAddress(ByVal hModule As IntPtr, ByVal lpProcName As String) As IntPtr Implements IDynamicLibraryLoader.GetFunctionAddress
            Return dlsym(hModule, lpProcName)
        End Function

        Friend Shared Function InternalLoadLibrary(ByVal filename As String) As IntPtr
            Const RTLD_LAZY = &H1
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

            Dim result = dlopen(filename, RTLD_LAZY)
            Return result
        End Function

        Private Shared Function getSearchPaths(ByVal pathsEnvVar As String) As List(Of String)
            Dim searchPaths = If(Environment.GetEnvironmentVariable(pathsEnvVar), "").Split(Path.PathSeparator).ToList()
            Return searchPaths
        End Function

        <DllImport("libdl")>
        Private Shared Function dlopen(
        <MarshalAs(UnmanagedType.LPStr)> ByVal filename As String, ByVal flag As Integer) As IntPtr
        End Function

        <DllImport("libdl")>
        Private Shared Function dlerror() As <MarshalAs(UnmanagedType.LPStr)> String
        End Function

        <DllImport("libdl", EntryPoint:="dlclose")>
        <ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
        Private Shared Function dlclose(ByVal hModule As IntPtr) As Integer
        End Function

        <DllImport("libdl", EntryPoint:="dlsym")>
        Private Shared Function dlsym(ByVal hModule As IntPtr,
        <MarshalAs(UnmanagedType.LPStr)> ByVal lpProcName As String) As IntPtr
        End Function
    End Class
End Namespace

