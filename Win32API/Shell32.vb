Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

<[PackageNamespace]("shell32.dll",
                    Description:="",
                    Url:="",
                    Publisher:="Copyright (C) 2014 Microsoft Corporation")>
Public Module Shell32

    <ExportAPI("ShellExecuteA")>
    Public Declare Function ShellExecute Lib "shell32.dll" Alias "ShellExecuteA" (hwnd As Integer, lpOperation As String, lpFile As String, lpParameters As String, lpDirectory As String, nShowCmd As Integer) As Integer

    ''' <summary>
    ''' UPGRADE_ISSUE: ???????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="82EBB1AE-1FCB-4FEF-9E6C-8736A316F8A7"”
    ''' </summary>
    ''' <param name="lpCmdLine"></param>
    ''' <param name="pNumArgs"></param>
    ''' <returns></returns>
    ''' 
    <ExportAPI("CommandLineToArgv")>
    Public Declare Function CommandLineToArgv Lib "shell32" Alias "CommandLineToArgvW" (lpCmdLine As String, ByRef pNumArgs As Short) As Integer
    'UPGRADE_WARNING: ?? SHFILEINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    <ExportAPI("SHGetFileInfo")>
    Public Declare Function SHGetFileInfo Lib "shell32.dll" Alias " SHGetFileInfoA" (pszPath As String, dwFileAttributes As Integer, ByRef psfi As SHFILEINFO, cbFileInfo As Integer, uFlags As Integer) As Integer
    <ExportAPI("SHGetNewLinkInfo")>
    Public Declare Function SHGetNewLinkInfo Lib "shell32.dll" Alias "SHGetNewLinkInfoA" (pszLinkto As String, pszDir As String, pszName As String, ByRef pfMustCopy As Integer, uFlags As Integer) As Integer
    'UPGRADE_WARNING: ?? NOTIFYICONDATA ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    <ExportAPI("Shell_NotifyIcon")>
    Public Declare Function Shell_NotifyIcon Lib "shell32.dll" Alias " Shell_NotifyIconA" (dwMessage As Integer, ByRef lpData As NOTIFYICONDATA) As Integer
    'UPGRADE_NOTE: error ???? error_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
    <ExportAPI("WinExecError")>
    Public Declare Sub WinExecError Lib "shell32.dll" Alias "WinExecErrorA" (hWnd As Integer, error_Renamed As Integer, lpstrFileName As String, lpstrTitle As String)

    'UPGRADE_WARNING: ?? SHFILEOPSTRUCT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    <ExportAPI("SHFileOperation")>
    Public Declare Function SHFileOperation Lib "shell32.dll" Alias " SHFileOperationA" (ByRef lpFileOp As SHFILEOPSTRUCT) As Integer
    <ExportAPI("SHFreeNameMappings")>
    Public Declare Sub SHFreeNameMappings Lib "shell32.dll" (hNameMappings As Integer)
    <ExportAPI("ExtractIconEx")>
    Public Declare Function ExtractIconEx Lib "shell32.dll" Alias "ExtractIconExA" (lpszFile As String, nIconIndex As Integer, ByRef phiconLarge As Integer, ByRef phiconSmall As Integer, nIcons As Integer) As Integer
    'UPGRADE_WARNING: ?? APPBARDATA ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    <ExportAPI("SHAppBarMessage")>
    Public Declare Function SHAppBarMessage Lib "shell32.dll" (dwMessage As Integer, ByRef pData As APPBARDATA) As Integer

    ' //  EndAppBar
    <ExportAPI("DoEnvironmentSubst")>
    Public Declare Function DoEnvironmentSubst Lib "shell32.dll" Alias "DoEnvironmentSubstA" (szString As String, cbString As Integer) As Integer
    <ExportAPI("FindEnvironmentString")>
    Public Declare Function FindEnvironmentString Lib "shell32.dll" Alias "FindEnvironmentStringA" (szEnvVar As String) As String
    <ExportAPI("DragQueryFile")>
    Public Declare Function DragQueryFile Lib "shell32.dll" Alias "DragQueryFileA" (hDrop As Integer, UINT As Integer, lpStr As String, ch As Integer) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    <ExportAPI("DragQueryPoint")>
    Public Declare Function DragQueryPoint Lib "shell32.dll" (hDrop As Integer, ByRef lpPoint As POINTAPI) As Integer
    <ExportAPI("DragFinish")>
    Public Declare Sub DragFinish Lib "shell32.dll" (hDrop As Integer)
    <ExportAPI("DragAcceptFiles")>
    Public Declare Sub DragAcceptFiles Lib "shell32.dll" (hWnd As Integer, fAccept As Integer)
    <ExportAPI("FindExecutable")>
    Public Declare Function FindExecutable Lib "shell32.dll" Alias "FindExecutableA" (lpFile As String, lpDirectory As String, lpResult As String) As Integer
    <ExportAPI("ShellAbout")>
    Public Declare Function ShellAbout Lib "shell32.dll" Alias "ShellAboutA" (hWnd As Integer, szApp As String, szOtherStuff As String, hIcon As Integer) As Integer
    <ExportAPI("DuplicateIcon")>
    Public Declare Function DuplicateIcon Lib "shell32.dll" (hInst As Integer, hIcon As Integer) As Integer
    <ExportAPI("ExtractAssociatedIcon")>
    Public Declare Function ExtractAssociatedIcon Lib "shell32.dll" Alias "ExtractAssociateIconA" (hInst As Integer, lpIconPath As String, ByRef lpiIcon As Integer) As Integer
    <ExportAPI("ExtractIcon")>
    Public Declare Function ExtractIcon Lib "shell32.dll" Alias "ExtractIconA" (hInst As Integer, lpszExeFileName As String, nIconIndex As Integer) As Integer

End Module
