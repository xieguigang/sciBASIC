#Region "Microsoft.VisualBasic::de4682aabcc785a9e9f19f3ad2b06769, ..\VisualBasic_AppFramework\Win32API\Kernel32.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports Microsoft.VisualBasic.Scripting.MetaData

<PackageNamespace("kernel32.dll",
                  Description:="KERNEL32.DLL exposes to applications most of the Win32 base APIs, such as memory management, 
                  input/output (I/O) operations, process and thread creation, and synchronization functions. Many of these are 
                  implemented within KERNEL32.DLL by calling corresponding functions in the native API, exposed by NTDLL.DLL.",
                  Publisher:="Copyright (C) 2014 Microsoft Corporation",
                  Revision:=10512,
                  Url:="http://SourceForge.net/projects/shoal")>
Public Module Kernel32

    ''' <summary>
    ''' Sleep pauses program execution for a certain amount of time. This is more accurate than using a do-nothing loop, waiting for a certain amount of time to pass. The function does not return a value. 
    ''' </summary>
    ''' <param name="dwMilliseconds">The number of milliseconds to halt program execution for. </param>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Sleep", Info:="Sleep pauses program execution for a certain amount of time. This is more accurate than using a do-nothing loop, waiting for a certain amount of time to pass. The function does not return a value.")>
    Public Declare Sub Sleep Lib "kernel32.dll" ( dwMilliseconds As Long)
    <ExportAPI("GetWindowsDirectoryA")>
    Public Declare Function GetWindowsDirectory Lib "kernel32" Alias "GetWindowsDirectoryA" (lpBuffer As String, nSize As Integer) As Integer
    <ExportAPI("GetSystemDirectoryA")>
    Public Declare Function GetSystemDirectory Lib "kernel32" Alias "GetSystemDirectoryA" (lpBuffer As String, nSize As Integer) As Integer
    <ExportAPI("lstrlenA")>
    Public Declare Function lstrlen Lib "kernel32" Alias "lstrlenA" (lpString As String) As Integer
	
	    Public Declare Function EnumCalendarInfo Lib "kernel32" Alias "EnumCalendarInfoA" (lpCalInfoEnumProc As Integer, Locale As Integer, Calendar As Integer, CalType As Integer) As Integer
    'UPGRADE_WARNING: ?? CURRENCYFMT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetCurrencyFormat Lib "kernel32" Alias "GetCurrencyFormatA" (Locale As Integer, dwFlags As Integer, lpValue As String, ByRef lpFormat As CURRENCYFMT, lpCurrencyStr As String, cchCurrency As Integer) As Integer
    'UPGRADE_WARNING: ?? NUMBERFMT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetNumberFormat Lib "kernel32" Alias "GetNumberFormatA" (Locale As Integer, dwFlags As Integer, lpValue As String, ByRef lpFormat As NUMBERFMT, lpNumberStr As String, cchNumber As Integer) As Integer
    Public Declare Function GetStringTypeEx Lib "kernel32" Alias "GetStringTypeExA" (Locale As Integer, dwInfoType As Integer, lpSrcStr As String, cchSrc As Integer, ByRef lpCharType As Short) As Integer
    Public Declare Function GetStringTypeW Lib "kernel32" (dwInfoType As Integer, lpSrcStr As String, cchSrc As Integer, ByRef lpCharType As Short) As Integer
    Public Declare Function IsDBCSLeadByte Lib "kernel32" (TestChar As Byte) As Integer
    Public Declare Function SetLocaleInfo Lib "kernel32" Alias "SetLocaleInfoA" (Locale As Integer, LCType As Integer, lpLCData As String) As Integer

	    'UPGRADE_WARNING: ?? COMMCONFIG ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CommConfigDialog Lib "kernel32" Alias "CommConfigDialogA" (lpszName As String, hWnd As Integer, ByRef lpCC As COMMCONFIG) As Integer
    Public Declare Function CreateIoCompletionPort Lib "kernel32" (FileHandle As Integer, ExistingCompletionPort As Integer, CompletionKey As Integer, NumberOfConcurrentThreads As Integer) As Integer
    Public Declare Function DisableThreadLibraryCalls Lib "kernel32" (hLibModule As Integer) As Integer
    Public Declare Function EnumResourceLanguages Lib "kernel32" Alias "EnumResourceLanguagesA" (hModule As Integer, lpType As String, lpName As String, lpEnumFunc As Integer, lParam As Integer) As Integer
    Public Declare Function EnumResourceNames Lib "kernel32" Alias "EnumResourceNamesA" (hModule As Integer, lpType As String, lpEnumFunc As Integer, lParam As Integer) As Integer
    Public Declare Function EnumResourceTypes Lib "kernel32" Alias "EnumResourceTypesA" (hModule As Integer, lpEnumFunc As Integer, lParam As Integer) As Integer
    Public Declare Function FreeEnvironmentStrings Lib "kernel32" Alias "FreeEnvironmentStringsA" (lpsz As String) As Integer

    Public Declare Sub FreeLibraryAndExitThread Lib "kernel32" (hLibModule As Integer, dwExitCode As Integer)
    Public Declare Function FreeResource Lib "kernel32" (hResData As Integer) As Integer
    'UPGRADE_WARNING: ?? COMMCONFIG ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetCommConfig Lib "kernel32" (hCommDev As Integer, ByRef lpCC As COMMCONFIG, ByRef lpdwSize As Integer) As Integer
    Public Declare Function GetCompressedFileSize Lib "kernel32" Alias "GetCompressedFileSizeA" (lpFileName As String, ByRef lpFileSizeHigh As Integer) As Integer
    'UPGRADE_WARNING: ?? COMMCONFIG ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetDefaultCommConfig Lib "kernel32" Alias "GetDefaultCommConfigA" (lpszName As String, ByRef lpCC As COMMCONFIG, ByRef lpdwSize As Integer) As Integer
    Public Declare Function GetHandleInformation Lib "kernel32" (hObject As Integer, ByRef lpdwFlags As Integer) As Integer
    Public Declare Function GetProcessHeaps Lib "kernel32" (NumberOfHeaps As Integer, ByRef ProcessHeaps As Integer) As Integer
    Public Declare Function GetProcessWorkingSetSize Lib "kernel32" (hProcess As Integer, ByRef lpMinimumWorkingSetSize As Integer, ByRef lpMaximumWorkingSetSize As Integer) As Integer
    Public Declare Function GetQueuedCompletionStatus Lib "kernel32" (CompletionPort As Integer, ByRef lpNumberOfBytesTransferred As Integer, ByRef lpCompletionKey As Integer, ByRef lpOverlapped As Integer, dwMilliseconds As Integer) As Integer
    Public Declare Function GetSystemTimeAdjustment Lib "kernel32" (ByRef lpTimeAdjustment As Integer, ByRef lpTimeIncrement As Integer, ByRef lpTimeAdjustmentDisabled As Boolean) As Integer

    Public Declare Function GlobalCompact Lib "kernel32" (dwMinFree As Integer) As Integer
    Public Declare Sub GlobalFix Lib "kernel32" (hMem As Integer)
    Public Declare Sub GlobalUnfix Lib "kernel32" (hMem As Integer)
    Public Declare Function GlobalWire Lib "kernel32" (hMem As Integer) As Integer
    Public Declare Function GlobalUnWire Lib "kernel32" (hMem As Integer) As Integer

    Public Declare Function IsBadCodePtr Lib "kernel32" (lpfn As Integer) As Integer
    Public Declare Function LocalCompact Lib "kernel32" (uMinFree As Integer) As Integer
    Public Declare Function LocalShrink Lib "kernel32" (hMem As Integer, cbNewSize As Integer) As Integer
    Public Declare Function MapViewOfFile Lib "kernel32" (hFileMappingObject As Integer, dwDesiredAccess As Integer, dwFileOffsetHigh As Integer, dwFileOffsetLow As Integer, dwNumberOfBytesToMap As Integer) As Integer
    'UPGRADE_WARNING: ?? OVERLAPPED ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function ReadFileEx Lib "kernel32" (hFile As Integer, ByRef lpBuffer As Object, nNumberOfBytesToRead As Integer, ByRef lpOverlapped As OVERLAPPED, lpCompletionRoutine As Integer) As Integer

    'UPGRADE_WARNING: ?? COMMCONFIG ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetCommConfig Lib "kernel32" (hCommDev As Integer, ByRef lpCC As COMMCONFIG, dwSize As Integer) As Integer
    'UPGRADE_WARNING: ?? COMMCONFIG ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetDefaultCommConfig Lib "kernel32" Alias "SetDefaultCommConfigA" (lpszName As String, ByRef lpCC As COMMCONFIG, dwSize As Integer) As Integer
    Public Declare Sub SetFileApisToANSI Lib "kernel32" ()
    Public Declare Function SetHandleInformation Lib "kernel32" (hObject As Integer, dwMask As Integer, dwFlags As Integer) As Integer
    Public Declare Function SetProcessWorkingSetSize Lib "kernel32" (hProcess As Integer, dwMinimumWorkingSetSize As Integer, dwMaximumWorkingSetSize As Integer) As Integer

    Public Declare Function lstrcat Lib "kernel32" Alias "lstrcatA" (lpString1 As String, lpString2 As String) As Integer
    Public Declare Function lstrcpyn Lib "kernel32" Alias "lstrcpynA" (lpString1 As String, lpString2 As String, iMaxLength As Integer) As Integer
    Public Declare Function lstrcpy Lib "kernel32" Alias "lstrcpyA" (lpString1 As String, lpString2 As String) As Integer
    Public Declare Function SetSystemTimeAdjustment Lib "kernel32" (dwTimeAdjustment As Integer, bTimeAdjustmentDisabled As Boolean) As Integer
    Public Declare Function SetThreadAffinityMask Lib "kernel32" (hThread As Integer, dwThreadAffinityMask As Integer) As Integer
    Public Declare Function SetUnhandledExceptionFilter Lib "kernel32" (lpTopLevelExceptionFilter As Integer) As Integer
    'UPGRADE_WARNING: ?? SystemTime ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? SystemTime ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? TIME_ZONE_INFORMATION ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SystemTimeToTzSpecificLocalTime Lib "kernel32" (ByRef lpTimeZoneInformation As TIME_ZONE_INFORMATION, ByRef lpUniversalTime As SystemTime, ByRef lpLocalTime As SystemTime) As Integer
    'UPGRADE_WARNING: ?? OVERLAPPED ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function WriteFileEx Lib "kernel32" (hFile As Integer, ByRef lpBuffer As Object, nNumberOfBytesToWrite As Integer, ByRef lpOverlapped As OVERLAPPED, lpCompletionRoutine As Integer) As Integer

    'UPGRADE_WARNING: ?? SYSTEM_POWER_STATUS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetSystemPowerStatus Lib "kernel32" (ByRef lpSystemPowerStatus As SYSTEM_POWER_STATUS) As Integer
    Public Declare Function SetSystemPowerState Lib "kernel32" (fSuspend As Integer, fForce As Integer) As Integer

    'UPGRADE_WARNING: ?? OSVERSIONINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetVersionEx Lib "kernel32" Alias "GetVersionExA" (lpVersionInformation As OSVERSIONINFO) As Integer
    Public Declare Function ImpersonateLoggedOnUser Lib "kernel32" (hToken As Integer) As Integer
    'UPGRADE_WARNING: ?? PROCESS_INFORMATION ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? STARTUPINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? SECURITY_ATTRIBUTES ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? SECURITY_ATTRIBUTES ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateProcessAsUser Lib "kernel32" Alias "CreateProcessAsUserA" (hToken As Integer, lpApplicationName As String, lpCommandLine As String, lpProcessAttributes As SECURITY_ATTRIBUTES, lpThreadAttributes As SECURITY_ATTRIBUTES, bInheritHandles As Integer, dwCreationFlags As Integer, lpEnvironment As String, lpCurrentDirectory As String, lpStartupInfo As STARTUPINFO, lpProcessInformation As PROCESS_INFORMATION) As Integer
    Public Declare Function GetBinaryType Lib "kernel32" Alias "GetBinaryTypeA" (lpApplicationName As String, ByRef lpBinaryType As Integer) As Integer

    Public Declare Function GetShortPathName Lib "kernel32" (lpszLongPath As String, lpszShortPath As String, cchBuffer As Integer) As Integer
    Public Declare Function GetProcessAffinityMask Lib "kernel32" (hProcess As Integer, ByRef lpProcessAffinityMask As Integer, ByRef SystemAffinityMask As Integer) As Integer

    Public Declare Function HeapLock Lib "kernel32" (hHeap As Integer) As Integer
    Public Declare Function HeapUnlock Lib "kernel32" (hHeap As Integer) As Integer
    '  Define API decoration for direct importing of DLL references.
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function HeapValidate Lib "kernel32" (hHeap As Integer, dwFlags As Integer, ByRef lpMem As Object) As Integer
    Public Declare Function HeapCompact Lib "kernel32" (hHeap As Integer, dwFlags As Integer) As Integer
    Public Declare Function VerLanguageName Lib "kernel32" Alias "VerLanguageNameA" (wLang As Integer, szLang As String, nSize As Integer) As Integer
    Public Declare Function InitAtomTable Lib "kernel32" (nSize As Integer) As Integer
    Public Declare Function AddAtom Lib "kernel32" Alias "AddAtomA" (lpString As String) As Short
    Public Declare Function DeleteAtom Lib "kernel32" (nAtom As Short) As Short
    Public Declare Function FindAtom Lib "kernel32" Alias "FindAtomA" (lpString As String) As Short
    Public Declare Function GetAtomName Lib "kernel32" Alias "GetAtomNameA" (nAtom As Short, lpBuffer As String, nSize As Integer) As Integer
    Public Declare Function GlobalAddAtom Lib "kernel32" Alias "GlobalAddAtomA" (lpString As String) As Short
    Public Declare Function GlobalDeleteAtom Lib "kernel32" (nAtom As Short) As Short
    Public Declare Function GlobalFindAtom Lib "kernel32" Alias "GlobalFindAtomA" (lpString As String) As Short
    Public Declare Function GlobalGetAtomName Lib "kernel32" Alias "GlobalGetAtomNameA" (nAtom As Short, lpBuffer As String, nSize As Integer) As Integer

    ' User Profile Routines
    ' NOTE: The lpKeyName argument for GetProfileString, WriteProfileString,
    '       GetPrivateProfileString, and WritePrivateProfileString can be either
    '       a string or NULL.  This is why the argument is defined as "As Object".
    '          For example, to pass a string specify    "wallpaper"
    '          To pass NULL specify                     0&
    '       You can also pass NULL for the lpString argument for WriteProfileString
    '       and WritePrivateProfileString
    Public Declare Function GetProfileInt Lib "kernel32" Alias "GetProfileIntA" (lpAppName As String, lpKeyName As String, nDefault As Integer) As Integer

    Public Declare Function GetProfileString Lib "kernel32" Alias "GetProfileStringA" (lpAppName As String, lpKeyName As String, lpDefault As String, lpReturnedString As String, nSize As Integer) As Integer

    Public Declare Function WriteProfileString Lib "kernel32" Alias "WriteProfileStringA" (lpszSection As String, lpszKeyName As String, lpszString As String) As Integer

    Public Declare Function GetProfileSection Lib "kernel32" Alias "GetProfileSectionA" (lpAppName As String, lpReturnedString As String, nSize As Integer) As Integer
    Public Declare Function WriteProfileSection Lib "kernel32" Alias "WriteProfileSectionA" (lpAppName As String, lpString As String) As Integer
    Public Declare Function GetPrivateProfileInt Lib "kernel32" Alias "GetPrivateProfileIntA" (lpApplicationName As String, lpKeyName As String, nDefault As Integer, lpFileName As String) As Integer

    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function GetPrivateProfileString Lib "kernel32" Alias "GetPrivateProfileStringA" (lpApplicationName As String, lpKeyName As Object, lpDefault As String, lpReturnedString As String, nSize As Integer, lpFileName As String) As Integer

    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function WritePrivateProfileString Lib "kernel32" Alias "WritePrivateProfileStringA" (lpApplicationName As String, lpKeyName As Object, lpString As Object, lpFileName As String) As Integer

    Public Declare Function GetPrivateProfileSection Lib "kernel32" Alias "GetPrivateProfileSectionA" (lpAppName As String, lpReturnedString As String, nSize As Integer, lpFileName As String) As Integer
    Public Declare Function WritePrivateProfileSection Lib "kernel32" Alias "WritePrivateProfileSectionA" (lpAppName As String, lpString As String, lpFileName As String) As Integer


    Public Declare Function GetTempPath Lib "kernel32" Alias "GetTempPathA" (nBufferLength As Integer, lpBuffer As String) As Integer
    Public Declare Function SetCurrentDirectory Lib "kernel32" Alias "SetCurrentDirectoryA" (lpPathName As String) As Integer
    Public Declare Function GetCurrentDirectory Lib "kernel32" (nBufferLength As Integer, lpBuffer As String) As Integer
    Public Declare Function GetDiskFreeSpace Lib "kernel32" Alias "GetDiskFreeSpaceA" (lpRootPathName As String, ByRef lpSectorsPerCluster As Integer, ByRef lpBytesPerSector As Integer, ByRef lpNumberOfFreeClusters As Integer, ByRef lpTtoalNumberOfClusters As Integer) As Integer
    'UPGRADE_WARNING: ?? SECURITY_ATTRIBUTES ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateDirectory Lib "kernel32" Alias "CreateDirectoryA" (lpPathName As String, ByRef lpSecurityAttributes As SECURITY_ATTRIBUTES) As Integer
    'UPGRADE_WARNING: ?? SECURITY_ATTRIBUTES ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateDirectoryEx Lib "kernel32" Alias "CreateDirectoryExA" (lpTemplateDirectory As String, lpNewDirectory As String, ByRef lpSecurityAttributes As SECURITY_ATTRIBUTES) As Integer
    Public Declare Function RemoveDirectory Lib "kernel32" Alias "RemoveDirectoryA" (lpPathName As String) As Integer
    Public Declare Function GetFullPathName Lib "kernel32" Alias "GetFullPathNameA" (lpFileName As String, nBufferLength As Integer, lpBuffer As String, lpFilePart As String) As Integer
    Public Declare Function SetErrorMode Lib "kernel32" (wMode As Integer) As Integer

    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function ReadProcessMemory Lib "kernel32" (hProcess As Integer, ByRef lpBaseAddress As Object, ByRef lpBuffer As Object, nSize As Integer, ByRef lpNumberOfBytesWritten As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function WriteProcessMemory Lib "kernel32" (hProcess As Integer, ByRef lpBaseAddress As Object, ByRef lpBuffer As Object, nSize As Integer, ByRef lpNumberOfBytesWritten As Integer) As Integer
    'UPGRADE_WARNING: ?? CONTEXT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetThreadContext Lib "kernel32" (hThread As Integer, ByRef lpContext As CONTEXT) As Integer
    'UPGRADE_WARNING: ?? CONTEXT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetThreadContext Lib "kernel32" (hThread As Integer, ByRef lpContext As CONTEXT) As Integer
    Public Declare Function SuspendThread Lib "kernel32" (hThread As Integer) As Integer
    Public Declare Function ResumeThread Lib "kernel32" (hThread As Integer) As Integer

    Public Declare Function FindResource Lib "kernel32" Alias "FindResourceA" (hInstance As Integer, lpName As String, lpType As String) As Integer
    Public Declare Function FindResourceEx Lib "kernel32" Alias "FindResourceExA" (hModule As Integer, lpType As String, lpName As String, wLanguage As Integer) As Integer
    Public Declare Function BeginUpdateResource Lib "kernel32" Alias "BeginUpdateResourceA" (pFileName As String, bDeleteExistingResources As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function UpdateResource Lib "kernel32" Alias "UpdateResourceA" (hUpdate As Integer, lpType As String, lpName As String, wLanguage As Integer, ByRef lpData As Object, cbData As Integer) As Integer
    Public Declare Function EndUpdateResource Lib "kernel32" Alias "EndUpdateResourceA" (hUpdate As Integer, fDiscard As Integer) As Integer
    Public Declare Function LoadResource Lib "kernel32" (hInstance As Integer, hResInfo As Integer) As Integer
    Public Declare Function LockResource Lib "kernel32" (hResData As Integer) As Integer
    Public Declare Function SizeofResource Lib "kernel32" (hInstance As Integer, hResInfo As Integer) As Integer
    Public Declare Function DefineDosDevice Lib "kernel32" Alias "DefineDosDeviceA" (dwFlags As Integer, lpDeviceName As String, lpTargetPath As String) As Integer
    Public Declare Function QueryDosDevice Lib "kernel32" Alias "QueryDosDeviceA" (lpDeviceName As String, lpTargetPath As String, ucchMax As Integer) As Integer
    'UPGRADE_WARNING: ?? SECURITY_ATTRIBUTES ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateFile Lib "kernel32" Alias "CreateFileA" (lpFileName As String, dwDesiredAccess As Integer, dwShareMode As Integer, ByRef lpSecurityAttributes As SECURITY_ATTRIBUTES, dwCreationDisposition As Integer, dwFlagsAndAttributes As Integer, hTemplateFile As Integer) As Integer
    Public Declare Function SetFileAttributes Lib "kernel32" Alias "SetFileAttributesA" (lpFileName As String, dwFileAttributes As Integer) As Integer
    Public Declare Function GetFileAttributes Lib "kernel32" Alias "GetFileAttributesA" (lpFileName As String) As Integer
    Public Declare Function DeleteFile Lib "kernel32" Alias "DeleteFileA" (lpFileName As String) As Integer
    'UPGRADE_WARNING: ?? WIN32_FIND_DATA ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function FindFirstFile Lib "kernel32" Alias "FindFirstFileA" (lpFileName As String, ByRef lpFindFileData As WIN32_FIND_DATA) As Integer
    'UPGRADE_WARNING: ?? WIN32_FIND_DATA ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function FindNextFile Lib "kernel32" Alias "FindNextFileA" (hFindFile As Integer, ByRef lpFindFileData As WIN32_FIND_DATA) As Integer
    Public Declare Function SearchPath Lib "kernel32" Alias "SearchPathA" (lpPath As String, lpFileName As String, lpExtension As String, nBufferLength As Integer, lpBuffer As String, lpFilePart As String) As Integer
    Public Declare Function CopyFile Lib "kernel32" Alias "CopyFileA" (lpExistingFileName As String, lpNewFileName As String, bFailIfExists As Integer) As Integer
    Public Declare Function MoveFile Lib "kernel32" Alias "MoveFileA" (lpExistingFileName As String, lpNewFileName As String) As Integer
    Public Declare Function MoveFileEx Lib "kernel32" Alias "MoveFileExA" (lpExistingFileName As String, lpNewFileName As String, dwFlags As Integer) As Integer


    Public Declare Function GetProcAddress Lib "kernel32" (hModule As Integer, lpProcName As String) As Integer

End Module
