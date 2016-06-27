
Imports Microsoft.VisualBasic.Scripting.MetaData

<PackageNamespace("version.dll")>
Public Module Version
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    <ExportAPI("VerQueryValue")>
    Public Declare Function VerQueryValue Lib "version.dll" (ByRef pBlock As Object, lpSubBlock As String, lplpBuffer As Integer, ByRef puLen As Integer) As Integer

    '  ----- Function prototypes -----

    Public Declare Function VerFindFile Lib "version.dll" Alias "VerFindFileA" (uFlags As Integer, szFileName As String, szWinDir As String, szAppDir As String, szCurDir As String, ByRef lpuCurDirLen As Integer, szDestDir As String, ByRef lpuDestDirLen As Integer) As Integer
    Public Declare Function VerInstallFile Lib "version.dll" Alias " VerInstallFileA" (uFlags As Integer, szSrcFileName As String, szDestFileName As String, szSrcDir As String, szDestDir As String, szCurDir As String, szTmpFile As String, ByRef lpuTmpFileLen As Integer) As Integer

    '  Returns size of version info in Bytes
    Public Declare Function GetFileVersionInfoSize Lib "version.dll" Alias "GetFileVersionInfoSizeA" (lptstrFilename As String, ByRef lpdwHandle As Integer) As Integer

    '  Read version info into buffer
    ' /* Length of buffer for info *
    ' /* Information from GetFileVersionSize *
    ' /* Filename of version stamped file *
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function GetFileVersionInfo Lib "version.dll" Alias "GetFileVersionInfoA" (lptstrFilename As String, dwHandle As Integer, dwLen As Integer, ByRef lpData As Object) As Integer

End Module
