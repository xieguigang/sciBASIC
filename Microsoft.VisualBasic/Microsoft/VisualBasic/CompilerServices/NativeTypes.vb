Imports Microsoft.Win32.SafeHandles
Imports System
Imports System.ComponentModel
Imports System.Runtime.ConstrainedExecution
Imports System.Runtime.InteropServices
Imports System.Security

Namespace Microsoft.VisualBasic.CompilerServices
    <EditorBrowsable(EditorBrowsableState.Never)> _
    Friend NotInheritable Class NativeTypes
        ' Methods
        Private Sub New()
        End Sub


        ' Fields
        Friend Const ERROR_ACCESS_DENIED As Integer = 5
        Friend Const ERROR_ALREADY_EXISTS As Integer = &HB7
        Friend Const ERROR_CANCELLED As Integer = &H4C7
        Friend Const ERROR_FILE_EXISTS As Integer = 80
        Friend Const ERROR_FILE_NOT_FOUND As Integer = 2
        Friend Const ERROR_FILENAME_EXCED_RANGE As Integer = &HCE
        Friend Const ERROR_INVALID_DRIVE As Integer = 15
        Friend Const ERROR_INVALID_PARAMETER As Integer = &H57
        Friend Const ERROR_OPERATION_ABORTED As Integer = &H3E3
        Friend Const ERROR_PATH_NOT_FOUND As Integer = 3
        Friend Const ERROR_SHARING_VIOLATION As Integer = &H20
        Friend Const GW_CHILD As Integer = 5
        Friend Const GW_HWNDFIRST As Integer = 0
        Friend Const GW_HWNDLAST As Integer = 1
        Friend Const GW_HWNDNEXT As Integer = 2
        Friend Const GW_HWNDPREV As Integer = 3
        Friend Const GW_MAX As Integer = 5
        Friend Const GW_OWNER As Integer = 4
        Friend Shared ReadOnly INVALID_HANDLE As IntPtr = New IntPtr(-1)
        Friend Const LCMAP_FULLWIDTH As Integer = &H800000
        Friend Const LCMAP_HALFWIDTH As Integer = &H400000
        Friend Const LCMAP_HIRAGANA As Integer = &H100000
        Friend Const LCMAP_KATAKANA As Integer = &H200000
        Friend Const LCMAP_LOWERCASE As Integer = &H100
        Friend Const LCMAP_SIMPLIFIED_CHINESE As Integer = &H2000000
        Friend Const LCMAP_TRADITIONAL_CHINESE As Integer = &H4000000
        Friend Const LCMAP_UPPERCASE As Integer = &H200
        Friend Const NORMAL_PRIORITY_CLASS As Integer = &H20
        Friend Const STARTF_USESHOWWINDOW As Integer = 1

        ' Nested Types
        <SecurityCritical, SuppressUnmanagedCodeSecurity> _
        Friend NotInheritable Class LateInitSafeHandleZeroOrMinusOneIsInvalid
            Inherits SafeHandleZeroOrMinusOneIsInvalid
            ' Methods
            <SecurityCritical> _
            Friend Sub New()
                MyBase.New(True)
            End Sub

            <SecurityCritical>
            Friend Sub InitialSetHandle(h As IntPtr)
                MyBase.SetHandle(h)
            End Sub

            <SecurityCritical>
            Protected Overrides Function ReleaseHandle() As Boolean
                Return (NativeMethods.CloseHandle(MyBase.handle) > 0)
            End Function

        End Class

        <Flags>
        Friend Enum MoveFileExFlags
            ' Fields
            MOVEFILE_COPY_ALLOWED = 2
            MOVEFILE_DELAY_UNTIL_REBOOT = 4
            MOVEFILE_REPLACE_EXISTING = 1
            MOVEFILE_WRITE_THROUGH = 8
        End Enum

        <StructLayout(LayoutKind.Sequential), SecurityCritical, SuppressUnmanagedCodeSecurity>
        Friend NotInheritable Class PROCESS_INFORMATION
            Public hProcess As IntPtr = IntPtr.Zero
            Public hThread As IntPtr = IntPtr.Zero
            Public dwProcessId As Integer
            Public dwThreadId As Integer
            Friend Sub New()
            End Sub
        End Class

        <StructLayout(LayoutKind.Sequential)>
        Friend NotInheritable Class SECURITY_ATTRIBUTES
            Implements IDisposable
            Public nLength As Integer = Marshal.SizeOf(GetType(SECURITY_ATTRIBUTES))
            Public lpSecurityDescriptor As IntPtr
            Public bInheritHandle As Boolean
            Friend Sub New()
            End Sub

            <SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)>
            Public Sub Dispose() Implements IDisposable.Dispose
                If (Me.lpSecurityDescriptor <> IntPtr.Zero) Then
                    UnsafeNativeMethods.LocalFree(Me.lpSecurityDescriptor)
                    Me.lpSecurityDescriptor = IntPtr.Zero
                End If
                GC.SuppressFinalize(Me)
            End Sub

            Protected Overrides Sub Finalize()
                Me.Dispose()
                MyBase.Finalize()
            End Sub
        End Class

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Auto), SecurityCritical, SuppressUnmanagedCodeSecurity>
        Friend NotInheritable Class STARTUPINFO
            Implements IDisposable
            Public cb As Integer
            Public lpReserved As IntPtr = IntPtr.Zero
            Public lpDesktop As IntPtr = IntPtr.Zero
            Public lpTitle As IntPtr = IntPtr.Zero
            Public dwX As Integer
            Public dwY As Integer
            Public dwXSize As Integer
            Public dwYSize As Integer
            Public dwXCountChars As Integer
            Public dwYCountChars As Integer
            Public dwFillAttribute As Integer
            Public dwFlags As Integer
            Public wShowWindow As Short
            Public cbReserved2 As Short
            Public lpReserved2 As IntPtr = IntPtr.Zero
            Public hStdInput As IntPtr = IntPtr.Zero
            Public hStdOutput As IntPtr = IntPtr.Zero
            Public hStdError As IntPtr = IntPtr.Zero
            Private m_HasBeenDisposed As Boolean
            Friend Sub New()
            End Sub

            <SecuritySafeCritical>
            Protected Overrides Sub Finalize()
                Me.Dispose(False)
            End Sub

            <SecurityCritical>
            Private Sub Dispose(disposing As Boolean)
                If (Not Me.m_HasBeenDisposed AndAlso disposing) Then
                    Me.m_HasBeenDisposed = True
                    If ((Me.dwFlags And &H100) <> 0) Then
                        If ((Me.hStdInput <> IntPtr.Zero) AndAlso (Me.hStdInput <> NativeTypes.INVALID_HANDLE)) Then
                            NativeMethods.CloseHandle(Me.hStdInput)
                            Me.hStdInput = NativeTypes.INVALID_HANDLE
                        End If
                        If ((Me.hStdOutput <> IntPtr.Zero) AndAlso (Me.hStdOutput <> NativeTypes.INVALID_HANDLE)) Then
                            NativeMethods.CloseHandle(Me.hStdOutput)
                            Me.hStdOutput = NativeTypes.INVALID_HANDLE
                        End If
                        If ((Me.hStdError <> IntPtr.Zero) AndAlso (Me.hStdError <> NativeTypes.INVALID_HANDLE)) Then
                            NativeMethods.CloseHandle(Me.hStdError)
                            Me.hStdError = NativeTypes.INVALID_HANDLE
                        End If
                    End If
                End If
            End Sub

            <SecuritySafeCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)> _
            Friend Sub Dispose() Implements IDisposable.Dispose
                Me.Dispose(True)
                GC.SuppressFinalize(Me)
            End Sub
        End Class

        <StructLayout(LayoutKind.Sequential)> _
        Friend NotInheritable Class SystemTime
            Public wYear As Short
            Public wMonth As Short
            Public wDayOfWeek As Short
            Public wDay As Short
            Public wHour As Short
            Public wMinute As Short
            Public wSecond As Short
            Public wMilliseconds As Short
            Friend Sub New()
            End Sub
        End Class
    End Class
End Namespace

