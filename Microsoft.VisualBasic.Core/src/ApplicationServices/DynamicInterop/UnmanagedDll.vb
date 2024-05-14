#Region "Microsoft.VisualBasic::bf25cd6bf0af9a8c7b231fbb21e7ff96, Microsoft.VisualBasic.Core\src\ApplicationServices\DynamicInterop\UnmanagedDll.vb"

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

    '   Total Lines: 341
    '    Code Lines: 157
    ' Comment Lines: 140
    '   Blank Lines: 44
    '     File Size: 16.71 KB


    '     Class UnmanagedDll
    ' 
    '         Properties: FileName, LibraryHandle
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: checkedGetSymbolHandle, CheckHandleInvalid, createLdLibPathMsg, DangerousGetHandle, GetAnsiString
    '                   GetByte, (+3 Overloads) GetFunction, GetFunctionAddress, GetInt32, GetInt64
    '                   GetIntPtr, ToString
    ' 
    '         Sub: (+2 Overloads) Dispose, ReportLoadLibError, ThrowFailedLibraryLoad, WriteByte, WriteInt32
    '              WriteInt64, WriteIntPtr
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Concurrent
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.ComponentModel.Collection

Namespace ApplicationServices.DynamicInterop

    ''' <summary>
    ''' A proxy for unmanaged dynamic link library (DLL).
    ''' </summary>
    Public Class UnmanagedDll : Inherits MarshalByRefObject
        Implements IDisposable

        Dim handle As SafeHandleUnmanagedDll

        ''' <summary>
        ''' Gets the Dll file name used for this native Dll wrapper.
        ''' </summary>
        Public Property FileName As String

        ''' <summary>
        ''' get the library handle which is load via the ``LoadLibrary`` function.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property LibraryHandle As IntPtr
            Get
                Return handle.LibraryHandle
            End Get
        End Property

        ''' <summary>
        ''' Creates a proxy for the specified dll.
        ''' </summary>
        ''' <param name="dllName">The DLL's name.</param>
        Public Sub New(dllName As String, Optional unix_dl_open_flag As Integer = UnixLibraryLoader.RTLD_LAZY)
            If dllName Is Nothing Then
                Throw New ArgumentNullException("dllName", "The name of the library to load is a null reference")
            ElseIf dllName.StringEmpty Then
                Throw New ArgumentException("The name of the library to load is an empty string", "dllName")
            Else
                handle = New SafeHandleUnmanagedDll(dllName, unix_dl_open_flag)
            End If

            If handle.IsInvalid Then
                Dim nativeError = handle.GetLastError()

                ' Retrieve the last error as soon as possible, 
                ' to limit the risk of another call to the dynamic
                ' loader overriding the error message;
                ReportLoadLibError(dllName, nativeError)
            End If

            FileName = dllName
        End Sub

        ''' <summary>
        ''' Gets whether the current handle is equal to the invalid handle
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' public override bool IsInvalid
        ''' </remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CheckHandleInvalid() As Boolean
            Return handle.LibraryHandle = IntPtr.Zero
        End Function

        Public Overrides Function ToString() As String
            Return $"{handle} {FileName}"
        End Function

        Private Sub ReportLoadLibError(dllName As String, nativeError As String)
            Dim dllFullName As String = dllName.GetFullPath

            If (File.Exists(dllFullName)) Then
                ThrowFailedLibraryLoad(dllFullName, nativeError)
            Else

                ' This below assumes that the PATH environment variable Is what Is relied on
                ' TODO: check whether there Is more to it: http://msdn.microsoft.com/en-us/library/ms682586.aspx

                ' Also some pointers to relevant information if we want to check whether the attempt to load 
                ' was made on a 32 Or 64 bit library
                ' For Windows:
                ' http//stackoverflow.com/questions/1345632/determine-if-an-executable-Or-library-Is-32-Or-64-bits-on-windows
                ' http://www.neowin.net/forum/topic/732648-check-if-exe-Is-x64/?p=590544108#entry590544108
                ' Linux, And perhaps MacOS; the 'file' command seems the way to go.
                ' http://stackoverflow.com/questions/5665228/in-linux-determine-if-a-a-library-archive-32-bit-Or-64-bit

                dllFullName = FindFullPath(dllName, {}).JoinBy("; ")
                ThrowFailedLibraryLoad(dllFullName, nativeError)
            End If

        End Sub

        <Obsolete("This message is likely to be too distribution specific", True)>
        Private Function createLdLibPathMsg() As String
            If Not IsUnix Then Return Nothing
            'var sampleldLibPaths = "/usr/local/lib/R/lib:/usr/local/lib:/usr/lib/jvm/java-7-openjdk-amd64/jre/lib/amd64/server";
            Dim ldLibPathEnv = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH")
            Dim msg = Environment.NewLine & Environment.NewLine

            If String.IsNullOrEmpty(ldLibPathEnv) Then
                msg = msg & "The environment variable LD_LIBRARY_PATH is not set."
            Else
                msg = msg & String.Format("The environment variable LD_LIBRARY_PATH is set to {0}.", ldLibPathEnv)
            End If

            msg = msg & " For some Unix-like operating systems you may need to set or modify the variable LD_LIBRARY_PATH BEFORE launching the application."
            'msg = msg + " You can get the value as set by the R console application for your system, with the statement Sys.getenv('LD_LIBRARY_PATH'). For instance from your shell prompt:";
            'msg = msg + Environment.NewLine;
            'msg = msg + "Rscript -e \"Sys.getenv('LD_LIBRARY_PATH')\"";
            'msg = msg + Environment.NewLine;
            'msg = msg + "export LD_LIBRARY_PATH=/usr/the/paths/you/just/got/from/Rscript";
            msg = msg & Environment.NewLine & Environment.NewLine
            Return msg
        End Function

        Private Sub ThrowFailedLibraryLoad(dllFullName As String, nativeError As String)
            Dim strMsg = String.Format("This {0}-bit process failed to load the library {1}",
                                       If(Environment.Is64BitProcess, "64", "32"),
                                       dllFullName)

            If Not String.IsNullOrEmpty(nativeError) Then
                strMsg = strMsg & String.Format(". Native error message is '{0}'", nativeError)
            Else
                strMsg = strMsg & ". No further error message from the dynamic library loader"
            End If

            '            var ldLibPathMsg = createLdLibPathMsg();
            '            if (!string.IsNullOrEmpty(ldLibPathMsg))
            '                strMsg = strMsg + string.Format(". {0}", ldLibPathMsg);
            Throw New ArgumentException(strMsg)
        End Sub

        Private delegateFunctionPointers As New ConcurrentDictionary(Of String, Object)()

        ''' <summary>
        ''' Creates the delegate function for the specified function defined in the DLL.
        ''' </summary>
        ''' <typeparam name="TDelegate">The type of delegate. The name of the native 
        ''' function is assumed to be the same as the delegate type name.</typeparam>
        ''' <returns>The delegate.</returns>
        Public Function GetFunction(Of TDelegate As Class)() As TDelegate
            Return GetFunction(Of TDelegate)(GetType(TDelegate).Name)
        End Function

        ''' <summary>
        ''' Creates the delegate function for the specified function defined in the DLL.
        ''' </summary>
        ''' <typeparam name="TDelegate">The type of delegate.</typeparam>
        ''' <param name="entryPoint">The name of the function exported by the DLL</param>
        ''' <returns>The delegate.</returns>
        Public Function GetFunction(Of TDelegate As Class)(entryPoint As String) As TDelegate
            If String.IsNullOrEmpty(entryPoint) Then
                Throw New ArgumentNullException("entryPoint", "Native function name cannot be null or empty")
            ElseIf Not GetType(TDelegate).IsSubclassOf(GetType([Delegate])) Then
                Throw New InvalidCastException()
            End If

            SyncLock Me
                Return delegateFunctionPointers.ComputeIfAbsent(
                    key:=entryPoint,
                    lazyValue:=Function(func)
                                   Return TryCast(GetFunction(entryPoint, GetType(TDelegate)), TDelegate)
                               End Function)
            End SyncLock
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="entryPoint"></param>
        ''' <param name="type">
        ''' should be a delegate type
        ''' </param>
        ''' <returns></returns>
        Public Function GetFunction(entryPoint As String, type As Type) As Object
            Dim [function] = GetFunctionAddress(entryPoint)

            If [function] = IntPtr.Zero Then
                Throw New EntryPointNotFoundException($"Function {entryPoint} not found in native library {FileName}")
            Else
                Return Marshal.GetDelegateForFunctionPointer([function], type)
            End If
        End Function

        ''' <summary>
        ''' Gets the address of a native function entry point.
        ''' </summary>
        ''' <returns>The function address.</returns>
        ''' <param name="lpProcName">name of the function in the native library</param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetFunctionAddress(lpProcName As String) As IntPtr
            Return handle.GetFunctionAddress(lpProcName)
        End Function

        ''' <summary>
        ''' Gets the handle of the specified entry.
        ''' </summary>
        ''' <param name="entryPoint">The name of function.</param>
        ''' <returns>The handle.</returns>
        Public Function DangerousGetHandle(entryPoint As String) As IntPtr
            If String.IsNullOrEmpty(entryPoint) Then
                Throw New ArgumentNullException("The entry point cannot be null or an empty string", "entryPoint")
            End If

            Return GetFunctionAddress(entryPoint)
        End Function

        ''' <summary>
        ''' Frees the native library this objects represents
        ''' </summary>
        ''' <param name="disposing"></param>
        Protected Overridable Sub Dispose(disposing As Boolean)
            handle.Dispose()
        End Sub

        ''' <summary>
        ''' Dispose of this library.
        ''' </summary>
        ''' <remarks>Call <see cref="Dispose()"/> when you are finished using the <see cref="DynamicInterop.UnmanagedDll"/>. The
        ''' <see cref="Dispose()"/> method leaves the <see cref="DynamicInterop.UnmanagedDll"/> in an unusable state.
        ''' After calling <see cref="Dispose()"/>, you must release all references to the
        ''' <see cref="DynamicInterop.UnmanagedDll"/> so the garbage collector can reclaim the memory that the
        ''' <see cref="DynamicInterop.UnmanagedDll"/> was occupying.</remarks>
        Public Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
        End Sub

        Private Function checkedGetSymbolHandle(symbolName As String) As IntPtr
            Dim addr = DangerousGetHandle(symbolName)
            If IntPtr.Zero = addr Then
                Throw New ArgumentException(String.Format("Could not retrieve a pointer for the symbol '{0}' in file '{1}'", symbolName, FileName))
            End If
            Return addr
        End Function

        ''' <summary>
        ''' Writes an int32 value to the address of a symbol in the library. 
        ''' </summary>
        ''' <param name="symbolName">Symbol name.</param>
        ''' <param name="value">Value.</param>
        ''' <remarks>Throws an <exception cref="System.ArgumentException">ArgumentException</exception> if the symbol is not exported by the library</remarks>
        Public Sub WriteInt32(symbolName As String, value As Integer)
            Dim addr = checkedGetSymbolHandle(symbolName)
            Marshal.WriteInt32(addr, value)
        End Sub

        ''' <summary>
        ''' Reads an int32 value from the address of a symbol in the library. 
        ''' </summary>
        ''' <returns>The value for this symbol, read as an int32</returns>
        ''' <param name="symbolName">Symbol name.</param>
        ''' <remarks>Throws an <exception cref="System.ArgumentException">ArgumentException</exception> if the symbol is not exported by the library</remarks>
        Public Function GetInt32(symbolName As String) As Integer
            Dim addr = checkedGetSymbolHandle(symbolName)
            Return Marshal.ReadInt32(addr)
        End Function

        ''' <summary>
        ''' Writes an int64 value to the address of a symbol in the library. 
        ''' </summary>
        ''' <param name="symbolName">Symbol name.</param>
        ''' <param name="value">Value.</param>
        ''' <remarks>Throws an <exception cref="System.ArgumentException">ArgumentException</exception> if the symbol is not exported by the library</remarks>
        Public Sub WriteInt64(symbolName As String, value As Long)
            Dim addr = checkedGetSymbolHandle(symbolName)
            Marshal.WriteInt64(addr, value)
        End Sub

        ''' <summary>
        ''' Reads an int64 value from the address of a symbol in the library. 
        ''' </summary>
        ''' <returns>The value for this symbol, read as an int64</returns>
        ''' <param name="symbolName">Symbol name.</param>
        ''' <remarks>Throws an <exception cref="System.ArgumentException">ArgumentException</exception> if the symbol is not exported by the library</remarks>
        Public Function GetInt64(symbolName As String) As Long
            Dim addr = checkedGetSymbolHandle(symbolName)
            Return Marshal.ReadInt64(addr)
        End Function

        ''' <summary>
        ''' Writes an IntPtr value to the address of a symbol in the library. 
        ''' </summary>
        ''' <param name="symbolName">Symbol name.</param>
        ''' <param name="value">Value.</param>
        ''' <remarks>Throws an <exception cref="System.ArgumentException">ArgumentException</exception> if the symbol is not exported by the library</remarks>
        Public Sub WriteIntPtr(symbolName As String, value As IntPtr)
            Dim addr = checkedGetSymbolHandle(symbolName)
            Marshal.WriteIntPtr(addr, value)
        End Sub

        ''' <summary>
        ''' Reads an IntPtr value from the address of a symbol in the library. 
        ''' </summary>
        ''' <returns>The value for this symbol, read as an IntPtr</returns>
        ''' <param name="symbolName">Symbol name.</param>
        ''' <remarks>Throws an <exception cref="System.ArgumentException">ArgumentException</exception> if the symbol is not exported by the library</remarks>
        Public Function GetIntPtr(symbolName As String) As IntPtr
            Dim addr = checkedGetSymbolHandle(symbolName)
            Return Marshal.ReadIntPtr(addr)
        End Function

        ''' <summary>
        ''' Writes a Byte value to the address of a symbol in the library. 
        ''' </summary>
        ''' <param name="symbolName">Symbol name.</param>
        ''' <param name="value">Value.</param>
        ''' <remarks>Throws an <exception cref="System.ArgumentException">ArgumentException</exception> if the symbol is not exported by the library</remarks>
        Public Sub WriteByte(symbolName As String, value As Byte)
            Dim addr = checkedGetSymbolHandle(symbolName)
            Marshal.WriteByte(addr, value)
        End Sub

        ''' <summary>
        ''' Reads a byte value from the address of a symbol in the library. 
        ''' </summary>
        ''' <returns>The value for this symbol, read as a byte</returns>
        ''' <param name="symbolName">Symbol name.</param>
        ''' <remarks>Throws an <exception cref="System.ArgumentException">ArgumentException</exception> if the symbol is not exported by the library</remarks>
        Public Function GetByte(symbolName As String) As Byte
            Dim addr = checkedGetSymbolHandle(symbolName)
            Return Marshal.ReadByte(addr)
        End Function

        ''' <summary>
        ''' Reads a string value from the address of a symbol in the library. 
        ''' </summary>
        ''' <returns>The value for this symbol, read as an ANSI string</returns>
        ''' <param name="symbolName">Symbol name.</param>
        ''' <remarks>Throws an <exception cref="System.ArgumentException">ArgumentException</exception> if the symbol is not exported by the library</remarks>
        Public Function GetAnsiString(symbolName As String) As String
            Dim addr = checkedGetSymbolHandle(symbolName)
            Return Marshal.PtrToStringAnsi(addr)
        End Function
    End Class
End Namespace
