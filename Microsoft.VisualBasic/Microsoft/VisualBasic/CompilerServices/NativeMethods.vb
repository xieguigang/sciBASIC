Imports System
Imports System.Runtime.ConstrainedExecution
Imports System.Runtime.InteropServices
Imports System.Security
Imports System.Text

Namespace Microsoft.VisualBasic.CompilerServices
    <ComVisible(False)> _
    Friend NotInheritable Class NativeMethods
        ' Methods
        Private Sub New()
        End Sub

        <SecurityCritical, DllImport("user32", CharSet:=CharSet.Ansi, SetLastError:=True, ExactSpelling:=True)>
        Friend Shared Function AttachThreadInput(idAttach As Integer, idAttachTo As Integer, fAttach As Integer) As Integer
        End Function

        <SecurityCritical, ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success), DllImport("kernel32", CharSet:=CharSet.Ansi, SetLastError:=True, ExactSpelling:=True)>
        Friend Shared Function CloseHandle(hObject As IntPtr) As Integer
        End Function

        <SecurityCritical, DllImport("Advapi32.dll", CharSet:=CharSet.Unicode, SetLastError:=True)>
        Friend Shared Function ConvertStringSecurityDescriptorToSecurityDescriptor(StringSecurityDescriptor As String, StringSDRevision As UInt32, ByRef SecurityDescriptor As IntPtr, SecurityDescriptorSize As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        <SecurityCritical, DllImport("kernel32", CharSet:=CharSet.Auto)>
        Friend Shared Function CreateProcess(lpApplicationName As String, lpCommandLine As String, lpProcessAttributes As SECURITY_ATTRIBUTES, lpThreadAttributes As SECURITY_ATTRIBUTES, <MarshalAs(UnmanagedType.Bool)> bInheritHandles As Boolean, dwCreationFlags As Integer, lpEnvironment As IntPtr, lpCurrentDirectory As String, lpStartupInfo As STARTUPINFO, lpProcessInformation As PROCESS_INFORMATION) As Integer
        End Function

        <SecurityCritical, DllImport("user32", CharSet:=CharSet.Auto, SetLastError:=True)>
        Friend Shared Function FindWindow(<MarshalAs(UnmanagedType.VBByRefStr)> ByRef lpClassName As String, <MarshalAs(UnmanagedType.VBByRefStr)> ByRef lpWindowName As String) As IntPtr
        End Function

        <SecurityCritical, DllImport("user32", CharSet:=CharSet.Ansi, SetLastError:=True, ExactSpelling:=True)>
        Friend Shared Function GetDesktopWindow() As IntPtr
        End Function

        <SecurityCritical, DllImport("kernel32", CharSet:=CharSet.Auto)>
        Friend Shared Sub GetStartupInfo(<[In], Out> lpStartupInfo As STARTUPINFO)
        End Sub

        <SecurityCritical, DllImport("kernel32", CharSet:=CharSet.Auto)>
        Friend Shared Function GetVolumeInformation(<MarshalAs(UnmanagedType.LPTStr)> lpRootPathName As String, lpVolumeNameBuffer As StringBuilder, nVolumeNameSize As Integer, ByRef lpVolumeSerialNumber As Integer, ByRef lpMaximumComponentLength As Integer, ByRef lpFileSystemFlags As Integer, lpFileSystemNameBuffer As IntPtr, nFileSystemNameSize As Integer) As Integer
        End Function

        <SecurityCritical, DllImport("user32", CharSet:=CharSet.Ansi, SetLastError:=True, ExactSpelling:=True)>
        Friend Shared Function GetWindow(hwnd As IntPtr, wFlag As Integer) As IntPtr
        End Function

        <SecurityCritical, DllImport("user32", CharSet:=CharSet.Auto, SetLastError:=True)>
        Friend Shared Function GetWindowText(hWnd As IntPtr, <Out, MarshalAs(UnmanagedType.LPTStr)> lpString As StringBuilder, nMaxCount As Integer) As Integer
        End Function

        <SecurityCritical, DllImport("Kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
        Friend Shared Sub GlobalMemoryStatus(ByRef lpBuffer As MEMORYSTATUS)
        End Sub

        <SecurityCritical, DllImport("Kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
        Friend Shared Function GlobalMemoryStatusEx(ByRef lpBuffer As MEMORYSTATUSEX) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        <SecurityCritical, DllImport("kernel32", CharSet:=CharSet.Auto, SetLastError:=True)>
        Friend Shared Function MoveFileEx(lpExistingFileName As String, lpNewFileName As String, dwFlags As Integer) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        <SecurityCritical, DllImport("user32", CharSet:=CharSet.Ansi, SetLastError:=True, ExactSpelling:=True)>
        Friend Shared Function SetFocus(hwnd As IntPtr) As IntPtr
        End Function

        <SecurityCritical, DllImport("user32", CharSet:=CharSet.Ansi, SetLastError:=True, ExactSpelling:=True)>
        Friend Shared Function SetForegroundWindow(hwnd As IntPtr) As <MarshalAs(UnmanagedType.Bool)> Boolean
        End Function

        <SecurityCritical, DllImport("shell32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
        Friend Shared Sub SHChangeNotify(wEventId As UInt32, uFlags As UInt32, dwItem1 As IntPtr, dwItem2 As IntPtr)
        End Sub

        <SecurityCritical>
        Friend Shared Function SHFileOperation(ByRef lpFileOp As SHFILEOPSTRUCT) As Integer
            If (IntPtr.Size = 4) Then
                Return NativeMethods.SHFileOperation32(lpFileOp)
            End If
            Dim shfileopstruct As New SHFILEOPSTRUCT64 With {
                .hwnd = lpFileOp.hwnd,
                .wFunc = lpFileOp.wFunc,
                .pFrom = lpFileOp.pFrom,
                .pTo = lpFileOp.pTo,
                .fFlags = lpFileOp.fFlags,
                .fAnyOperationsAborted = lpFileOp.fAnyOperationsAborted,
                .hNameMappings = lpFileOp.hNameMappings,
                .lpszProgressTitle = lpFileOp.lpszProgressTitle
            }
            lpFileOp.fAnyOperationsAborted = shfileopstruct.fAnyOperationsAborted
            Return NativeMethods.SHFileOperation64(shfileopstruct)
        End Function

        <SecurityCritical, DllImport("shell32.dll", EntryPoint:="SHFileOperation", CharSet:=CharSet.Auto, SetLastError:=True)>
        Private Shared Function SHFileOperation32(ByRef lpFileOp As SHFILEOPSTRUCT) As Integer
        End Function

        <SecurityCritical, DllImport("shell32.dll", EntryPoint:="SHFileOperation", CharSet:=CharSet.Auto, SetLastError:=True)>
        Private Shared Function SHFileOperation64(ByRef lpFileOp As SHFILEOPSTRUCT64) As Integer
        End Function

        <SecurityCritical, DllImport("user32", CharSet:=CharSet.Auto, SetLastError:=True)>
        Friend Shared Function WaitForInputIdle(Process As LateInitSafeHandleZeroOrMinusOneIsInvalid, Milliseconds As Integer) As Integer
        End Function

        <SecurityCritical, DllImport("kernel32", CharSet:=CharSet.Ansi, SetLastError:=True, ExactSpelling:=True)>
        Friend Shared Function WaitForSingleObject(hHandle As LateInitSafeHandleZeroOrMinusOneIsInvalid, dwMilliseconds As Integer) As Integer
        End Function


        ' Nested Types
        <StructLayout(LayoutKind.Sequential)> _
        Friend Structure MEMORYSTATUS
            Friend dwLength As UInt32
            Friend dwMemoryLoad As UInt32
            Friend dwTotalPhys As UInt32
            Friend dwAvailPhys As UInt32
            Friend dwTotalPageFile As UInt32
            Friend dwAvailPageFile As UInt32
            Friend dwTotalVirtual As UInt32
            Friend dwAvailVirtual As UInt32
        End Structure

        <StructLayout(LayoutKind.Sequential)> _
        Friend Structure MEMORYSTATUSEX
            Friend dwLength As UInt32
            Friend dwMemoryLoad As UInt32
            Friend ullTotalPhys As UInt64
            Friend ullAvailPhys As UInt64
            Friend ullTotalPageFile As UInt64
            Friend ullAvailPageFile As UInt64
            Friend ullTotalVirtual As UInt64
            Friend ullAvailVirtual As UInt64
            Friend ullAvailExtendedVirtual As UInt64
            Friend Sub Init()
                Me.dwLength = DirectCast(Marshal.SizeOf(GetType(MEMORYSTATUSEX)), UInt32)
            End Sub
        End Structure

        Friend Enum SHChangeEventParameterFlags As UInt32
            ' Fields
            SHCNF_DWORD = 3
        End Enum

        Friend Enum SHChangeEventTypes As UInt32
            ' Fields
            SHCNE_ALLEVENTS = &H7FFFFFFF
            SHCNE_DISKEVENTS = &H2381F
        End Enum

        <Flags> _
        Friend Enum ShFileOperationFlags As UInt16
            ' Fields
            FOF_ALLOWUNDO = &H40
            FOF_CONFIRMMOUSE = 2
            FOF_FILESONLY = &H80
            FOF_MULTIDESTFILES = 1
            FOF_NO_CONNECTED_ELEMENTS = &H2000
            FOF_NOCONFIRMATION = &H10
            FOF_NOCONFIRMMKDIR = &H200
            FOF_NOCOPYSECURITYATTRIBS = &H800
            FOF_NOERRORUI = &H400
            FOF_NORECURSEREPARSE = &H8000
            FOF_NORECURSION = &H1000
            FOF_RENAMEONCOLLISION = 8
            FOF_SILENT = 4
            FOF_SIMPLEPROGRESS = &H100
            FOF_WANTMAPPINGHANDLE = &H20
            FOF_WANTNUKEWARNING = &H4000
        End Enum

        Friend Enum SHFileOperationType As UInt32
            ' Fields
            FO_COPY = 2
            FO_DELETE = 3
            FO_MOVE = 1
            FO_RENAME = 4
        End Enum

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Auto, Pack:=1)> _
        Friend Structure SHFILEOPSTRUCT
            Friend hwnd As IntPtr
            Friend wFunc As UInt32
            <MarshalAs(UnmanagedType.LPTStr)> _
            Friend pFrom As String
            <MarshalAs(UnmanagedType.LPTStr)> _
            Friend pTo As String
            Friend fFlags As UInt16
            Friend fAnyOperationsAborted As Boolean
            Friend hNameMappings As IntPtr
            <MarshalAs(UnmanagedType.LPTStr)> _
            Friend lpszProgressTitle As String
        End Structure

        <StructLayout(LayoutKind.Sequential, CharSet:=CharSet.Auto)> _
        Private Structure SHFILEOPSTRUCT64
            Friend hwnd As IntPtr
            Friend wFunc As UInt32
            <MarshalAs(UnmanagedType.LPTStr)> _
            Friend pFrom As String
            <MarshalAs(UnmanagedType.LPTStr)> _
            Friend pTo As String
            Friend fFlags As UInt16
            Friend fAnyOperationsAborted As Boolean
            Friend hNameMappings As IntPtr
            <MarshalAs(UnmanagedType.LPTStr)> _
            Friend lpszProgressTitle As String
        End Structure
    End Class
End Namespace

