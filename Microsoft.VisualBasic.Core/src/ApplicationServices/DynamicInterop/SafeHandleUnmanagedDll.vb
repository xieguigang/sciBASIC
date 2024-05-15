#Region "Microsoft.VisualBasic::ab073e36270b8618d0f79c6cc793c9ac, Microsoft.VisualBasic.Core\src\ApplicationServices\DynamicInterop\SafeHandleUnmanagedDll.vb"

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

    '   Total Lines: 94
    '    Code Lines: 66
    ' Comment Lines: 12
    '   Blank Lines: 16
    '     File Size: 3.54 KB


    '     Class SafeHandleUnmanagedDll
    ' 
    '         Properties: LibraryHandle
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: FreeLibrary, GetFunctionAddress, GetLastError, GetNativeLibraryLoader, ReleaseHandle
    '                   ToString
    ' 
    '         Sub: Dispose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Security.Permissions
Imports Microsoft.Win32.SafeHandles

Namespace ApplicationServices.DynamicInterop

    <SecurityPermission(SecurityAction.Demand, Flags:=SecurityPermissionFlag.UnmanagedCode)>
    Friend NotInheritable Class SafeHandleUnmanagedDll : Inherits SafeHandleZeroOrMinusOneIsInvalid

        ReadOnly libraryLoader As IDynamicLibraryLoader
        ReadOnly unix_dl_open_flag As Integer = UnixLibraryLoader.RTLD_LAZY

        ''' <summary>
        ''' get the library handle which is load via the ``LoadLibrary`` function.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property LibraryHandle As IntPtr
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me.handle
            End Get
        End Property

        Public Sub New(dllName As String, Optional unix_dl_open_flag As Integer = UnixLibraryLoader.RTLD_LAZY)
            MyBase.New(True)

            Me.unix_dl_open_flag = unix_dl_open_flag
            Me.libraryLoader = GetNativeLibraryLoader()
            Me.handle = libraryLoader.LoadLibrary(dllName)
        End Sub

        Private Function GetNativeLibraryLoader() As IDynamicLibraryLoader
            If IsUnix Then
                Return New UnixLibraryLoader() With {.dlopen_flag = unix_dl_open_flag}
            ElseIf Environment.OSVersion.Platform = PlatformID.Win32NT Then
                Return New WindowsLibraryLoader()
            Else
                Throw New NotSupportedException(GetPlatformNotSupportedMsg())
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"[{handle.ToString}] native_library_loader={libraryLoader}"
        End Function

        ''' <summary>
        ''' Frees the native library this objects represents
        ''' </summary>
        ''' <returns>The result of the call to FreeLibrary</returns>
        Protected Overrides Function ReleaseHandle() As Boolean
            Return FreeLibrary()
        End Function

        Private Function FreeLibrary() As Boolean
            Dim freed = False

            If libraryLoader Is Nothing Then
                If Not IsInvalid Then
                    Try
                        Throw New ApplicationException("Warning: unexpected condition of library loader and native handle - some native resources may not be properly disposed of")
                    Finally
                        freed = False
                    End Try
                Else
                    freed = True
                End If

                Return freed
            Else
                Return libraryLoader.FreeLibrary(handle)
            End If
        End Function

        Public Function GetFunctionAddress(lpProcName As String) As IntPtr
            Return libraryLoader.GetFunctionAddress(handle, lpProcName)
        End Function

        ''' <summary>
        ''' Frees the native library this objects represents
        ''' </summary>
        ''' <param name="disposing"></param>
        Protected Overrides Sub Dispose(disposing As Boolean)
            If FreeLibrary() Then
                SetHandleAsInvalid()
            End If

            MyBase.Dispose(disposing)
        End Sub

        Public Function GetLastError() As String
            Return libraryLoader.GetLastError()
        End Function
    End Class
End Namespace
