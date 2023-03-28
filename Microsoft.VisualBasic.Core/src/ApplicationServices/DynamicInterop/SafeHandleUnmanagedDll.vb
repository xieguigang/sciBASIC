#Region "Microsoft.VisualBasic::24b50cb3dd4b1122cc2e8799aa3279e6, sciBASIC#\Microsoft.VisualBasic.Core\src\ApplicationServices\DynamicInterop\SafeHandleUnmanagedDll.vb"

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

    '   Total Lines: 76
    '    Code Lines: 54
    ' Comment Lines: 8
    '   Blank Lines: 14
    '     File Size: 2.70 KB


    '     Class SafeHandleUnmanagedDll
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: FreeLibrary, GetFunctionAddress, GetLastError, ReleaseHandle
    ' 
    '         Sub: Dispose
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Security.Permissions
Imports Microsoft.Win32.SafeHandles

Namespace ApplicationServices.DynamicInterop

    <SecurityPermission(SecurityAction.Demand, Flags:=SecurityPermissionFlag.UnmanagedCode)>
    Friend NotInheritable Class SafeHandleUnmanagedDll
        Inherits SafeHandleZeroOrMinusOneIsInvalid

        Private ReadOnly libraryLoader As IDynamicLibraryLoader

        Public Sub New(dllName As String)
            MyBase.New(True)

            Me.libraryLoader = GetNativeLibraryLoader()
            Me.handle = libraryLoader.LoadLibrary(dllName)
        End Sub

        Private Shared Function GetNativeLibraryLoader() As IDynamicLibraryLoader
            If IsUnix Then
                Return New UnixLibraryLoader()
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
