
Imports Microsoft.VisualBasic.Scripting.MetaData

<PackageNamespace("advapi32.dll")>
Public Module advapi32
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    <ExportAPI("IsTextUnicode")>
    Public Declare Function IsTextUnicode Lib "advapi32" (ByRef lpBuffer As Object, cb As Integer, ByRef lpi As Integer) As Integer
    Public Declare Function NotifyChangeEventLog Lib "advapi32" (hEventLog As Integer, hEvent As Integer) As Integer
    'UPGRADE_WARNING: ?? PRIVILEGE_SET ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function ObjectOpenAuditAlarm Lib "advapi32" Alias "ObjectOpenAuditAlarmA" (SubsystemName As String, ByRef HandleId As Object, ObjectTypeName As String, ObjectName As String, ByRef pSecurityDescriptor As SECURITY_DESCRIPTOR, ClientToken As Integer, DesiredAccess As Integer, GrantedAccess As Integer, ByRef Privileges As PRIVILEGE_SET, ObjectCreation As Boolean, AccessGranted As Boolean, ByRef GenerateOnClose As Boolean) As Integer
    Public Declare Function SetThreadToken Lib "advapi32" (ByRef Thread As Integer, Token As Integer) As Integer

    Public Declare Function SetServiceBits Lib "advapi32" (hServiceStatus As Integer, dwServiceBits As Integer, bSetBitsOn As Boolean, bUpdateImmediately As Boolean) As Integer
    Public Declare Function LogonUser Lib "ADVAPI32.DLL" Alias "LogonUserA" (lpszUsername As String, lpszDomain As String, lpszPassword As String, dwLogonType As Integer, dwLogonProvider As Integer, ByRef phToken As Integer) As Integer
    '  Prototype for the Service Control Handler Function

    ' /////////////////////////////////////////////////////////////////////////
    '  API Function Prototypes
    ' /////////////////////////////////////////////////////////////////////////

    Public Declare Function ChangeServiceConfig Lib "advapi32.dll" Alias "ChangeServiceConfigA" (hService As Integer, dwServiceType As Integer, dwStartType As Integer, dwErrorControl As Integer, lpBinaryPathName As String, lpLoadOrderGroup As String, ByRef lpdwTagId As Integer, lpDependencies As String, lpServiceStartName As String, lpPassword As String, lpDisplayName As String) As Integer
    Public Declare Function CloseServiceHandle Lib "advapi32.dll" (hSCObject As Integer) As Integer
    'UPGRADE_WARNING: ?? SERVICE_STATUS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ControlService Lib "advapi32.dll" (hService As Integer, dwControl As Integer, ByRef lpServiceStatus As SERVICE_STATUS) As Integer
    Public Declare Function CreateService Lib "advapi32.dll" Alias "CreateServiceA" (hSCManager As Integer, lpServiceName As String, lpDisplayName As String, dwDesiredAccess As Integer, dwServiceType As Integer, dwStartType As Integer, dwErrorControl As Integer, lpBinaryPathName As String, lpLoadOrderGroup As String, ByRef lpdwTagId As Integer, lpDependencies As String, lp As String, lpPassword As String) As Integer
    Public Declare Function DeleteService Lib "advapi32.dll" (hService As Integer) As Integer
    'UPGRADE_WARNING: ?? ENUM_SERVICE_STATUS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function EnumDependentServices Lib "advapi32.dll" Alias "EnumDependentServicesA" (hService As Integer, dwServiceState As Integer, ByRef lpServices As ENUM_SERVICE_STATUS, cbBufSize As Integer, ByRef pcbBytesNeeded As Integer, ByRef lpServicesReturned As Integer) As Integer
    'UPGRADE_WARNING: ?? ENUM_SERVICE_STATUS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function EnumServicesStatus Lib "advapi32.dll" Alias "EnumServicesStatusA" (hSCManager As Integer, dwServiceType As Integer, dwServiceState As Integer, ByRef lpServices As ENUM_SERVICE_STATUS, cbBufSize As Integer, ByRef pcbBytesNeeded As Integer, ByRef lpServicesReturned As Integer, ByRef lpResumeHandle As Integer) As Integer
    Public Declare Function GetServiceKeyName Lib "advapi32.dll" Alias "GetServiceKeyNameA" (hSCManager As Integer, lpDisplayName As String, lpServiceName As String, ByRef lpcchBuffer As Integer) As Integer
    Public Declare Function GetServiceDisplayName Lib "advapi32.dll" Alias "GetServiceDisplayNameA" (hSCManager As Integer, lpServiceName As String, lpDisplayName As String, ByRef lpcchBuffer As Integer) As Integer
    Public Declare Function LockServiceDatabase Lib "advapi32.dll" (hSCManager As Integer) As Integer
    Public Declare Function NotifyBootConfigStatus Lib "advapi32.dll" (BootAcceptable As Integer) As Integer
    Public Declare Function OpenSCManager Lib "advapi32.dll" Alias "OpenSCManagerA" (lpMachineName As String, lpDatabaseName As String, dwDesiredAccess As Integer) As Integer
    Public Declare Function OpenService Lib "advapi32.dll" Alias "OpenServiceA" (hSCManager As Integer, lpServiceName As String, dwDesiredAccess As Integer) As Integer
    'UPGRADE_WARNING: ?? QUERY_SERVICE_CONFIG ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function QueryServiceConfig Lib "advapi32.dll" Alias "QueryServiceConfigA" (hService As Integer, ByRef lpServiceConfig As QUERY_SERVICE_CONFIG, cbBufSize As Integer, ByRef pcbBytesNeeded As Integer) As Integer
    'UPGRADE_WARNING: ?? QUERY_SERVICE_LOCK_STATUS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function QueryServiceLockStatus Lib "advapi32.dll" Alias "QueryServiceLockStatusA" (hSCManager As Integer, ByRef lpLockStatus As QUERY_SERVICE_LOCK_STATUS, cbBufSize As Integer, ByRef pcbBytesNeeded As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function QueryServiceObjectSecurity Lib "advapi32.dll" (hService As Integer, dwSecurityInformation As Integer, ByRef lpSecurityDescriptor As Object, cbBufSize As Integer, ByRef pcbBytesNeeded As Integer) As Integer
    'UPGRADE_WARNING: ?? SERVICE_STATUS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function QueryServiceStatus Lib "advapi32.dll" (hService As Integer, ByRef lpServiceStatus As SERVICE_STATUS) As Integer
    Public Declare Function RegisterServiceCtrlHandler Lib "advapi32.dll" Alias "RegisterServiceCtrlHandlerA" (lpServiceName As String, lpHandlerProc As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function SetServiceObjectSecurity Lib "advapi32.dll" (hService As Integer, dwSecurityInformation As Integer, ByRef lpSecurityDescriptor As Object) As Integer
    'UPGRADE_WARNING: ?? SERVICE_STATUS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetServiceStatus Lib "advapi32.dll" (hServiceStatus As Integer, ByRef lpServiceStatus As SERVICE_STATUS) As Integer
    'UPGRADE_WARNING: ?? SERVICE_TABLE_ENTRY ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function StartServiceCtrlDispatcher Lib "advapi32.dll" Alias "StartServiceCtrlDispatcherA" (ByRef lpServiceStartTable As SERVICE_TABLE_ENTRY) As Integer
    Public Declare Function StartService Lib "advapi32.dll" Alias "StartServiceA" (hService As Integer, dwNumServiceArgs As Integer, lpServiceArgVectors As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function UnlockServiceDatabase Lib "advapi32.dll" (ByRef ScLock As Object) As Integer

    ' Registry API prototypes

    Public Declare Function RegCloseKey Lib "advapi32.dll" (hKey As Integer) As Integer
    Public Declare Function RegConnectRegistry Lib "advapi32.dll" Alias "RegConnectRegistryA" (lpMachineName As String, hKey As Integer, ByRef phkResult As Integer) As Integer
    Public Declare Function RegCreateKey Lib "advapi32.dll" Alias "RegCreateKeyA" (hKey As Integer, lpSubKey As String, ByRef phkResult As Integer) As Integer
    'UPGRADE_WARNING: ?? SECURITY_ATTRIBUTES ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function RegCreateKeyEx Lib "advapi32.dll" Alias "RegCreateKeyExA" (hKey As Integer, lpSubKey As String, Reserved As Integer, lpClass As String, dwOptions As Integer, samDesired As Integer, ByRef lpSecurityAttributes As SECURITY_ATTRIBUTES, ByRef phkResult As Integer, ByRef lpdwDisposition As Integer) As Integer
    Public Declare Function RegDeleteKey Lib "advapi32.dll" Alias "RegDeleteKeyA" (hKey As Integer, lpSubKey As String) As Integer
    Public Declare Function RegDeleteValue Lib "advapi32.dll" Alias "RegDeleteValueA" (hKey As Integer, lpValueName As String) As Integer
    Public Declare Function RegEnumKey Lib "advapi32.dll" Alias "RegEnumKeyA" (hKey As Integer, dwIndex As Integer, lpName As String, cbName As Integer) As Integer
    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function RegEnumKeyEx Lib "advapi32.dll" Alias "RegEnumKeyExA" (hKey As Integer, dwIndex As Integer, lpName As String, ByRef lpcbName As Integer, ByRef lpReserved As Integer, lpClass As String, ByRef lpcbClass As Integer, ByRef lpftLastWriteTime As FILETIME) As Integer
    Public Declare Function RegEnumValue Lib "advapi32.dll" Alias "RegEnumValueA" (hKey As Integer, dwIndex As Integer, lpValueName As String, ByRef lpcbValueName As Integer, ByRef lpReserved As Integer, ByRef lpType As Integer, ByRef lpData As Byte, ByRef lpcbData As Integer) As Integer
    Public Declare Function RegFlushKey Lib "advapi32.dll" (hKey As Integer) As Integer
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function RegGetKeySecurity Lib "advapi32.dll" (hKey As Integer, SecurityInformation As Integer, ByRef pSecurityDescriptor As SECURITY_DESCRIPTOR, ByRef lpcbSecurityDescriptor As Integer) As Integer
    Public Declare Function RegLoadKey Lib "advapi32.dll" Alias "RegLoadKeyA" (hKey As Integer, lpSubKey As String, lpFile As String) As Integer
    Public Declare Function RegNotifyChangeKeyValue Lib "advapi32.dll" (hKey As Integer, bWatchSubtree As Integer, dwNotifyFilter As Integer, hEvent As Integer, fAsynchronus As Integer) As Integer
    Public Declare Function RegOpenKey Lib "advapi32.dll" Alias "RegOpenKeyA" (hKey As Integer, lpSubKey As String, ByRef phkResult As Integer) As Integer
    Public Declare Function RegOpenKeyEx Lib "advapi32.dll" Alias "RegOpenKeyExA" (hKey As Integer, lpSubKey As String, ulOptions As Integer, samDesired As Integer, ByRef phkResult As Integer) As Integer
    'UPGRADE_WARNING: ?? FILETIME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function RegQueryInfoKey Lib "advapi32.dll" Alias "RegQueryInfoKeyA" (hKey As Integer, lpClass As String, ByRef lpcbClass As Integer, ByRef lpReserved As Integer, ByRef lpcSubKeys As Integer, ByRef lpcbMaxSubKeyLen As Integer, ByRef lpcbMaxClassLen As Integer, ByRef lpcValues As Integer, ByRef lpcbMaxValueNameLen As Integer, ByRef lpcbMaxValueLen As Integer, ByRef lpcbSecurityDescriptor As Integer, ByRef lpftLastWriteTime As FILETIME) As Integer
    Public Declare Function RegQueryValue Lib "advapi32.dll" Alias "RegQueryValueA" (hKey As Integer, lpSubKey As String, lpValue As String, ByRef lpcbValue As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function RegQueryValueEx Lib "advapi32.dll" Alias "RegQueryValueExA" (hKey As Integer, lpValueName As String, lpReserved As Integer, ByRef lpType As Integer, ByRef lpData As Object, ByRef lpcbData As Integer) As Integer ' Note that if you Public Declare the lpData parameter as String, you must pass it By Value.
    Public Declare Function RegReplaceKey Lib "advapi32.dll" Alias "RegReplaceKeyA" (hKey As Integer, lpSubKey As String, lpNewFile As String, lpOldFile As String) As Integer
    Public Declare Function RegRestoreKey Lib "advapi32.dll" Alias "RegRestoreKeyA" (hKey As Integer, lpFile As String, dwFlags As Integer) As Integer
    'UPGRADE_WARNING: ?? SECURITY_ATTRIBUTES ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function RegSaveKey Lib "advapi32.dll" Alias "RegSaveKeyA" (hKey As Integer, lpFile As String, ByRef lpSecurityAttributes As SECURITY_ATTRIBUTES) As Integer
    'UPGRADE_WARNING: ?? SECURITY_DESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function RegSetKeySecurity Lib "advapi32.dll" (hKey As Integer, SecurityInformation As Integer, ByRef pSecurityDescriptor As SECURITY_DESCRIPTOR) As Integer
    Public Declare Function RegSetValue Lib "advapi32.dll" Alias "RegSetValueA" (hKey As Integer, lpSubKey As String, dwType As Integer, lpData As String, cbData As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function RegSetValueEx Lib "advapi32.dll" Alias "RegSetValueExA" (hKey As Integer, lpValueName As String, Reserved As Integer, dwType As Integer, ByRef lpData As Object, cbData As Integer) As Integer ' Note that if you Public Declare the lpData parameter as String, you must pass it By Value.
    Public Declare Function RegUnLoadKey Lib "advapi32.dll" Alias "RegUnLoadKeyA" (hKey As Integer, lpSubKey As String) As Integer
    Public Declare Function InitiateSystemShutdown Lib "advapi32.dll" Alias "InitiateSystemShutdownA" (lpMachineName As String, lpMessage As String, dwTimeout As Integer, bForceAppsClosed As Integer, bRebootAfterShutdown As Integer) As Integer
    Public Declare Function AbortSystemShutdown Lib "advapi32.dll" Alias "AbortSystemShutdownA" (lpMachineName As String) As Integer

End Module
