Imports Microsoft.VisualBasic.Scripting.MetaData

<PackageNamespace("mpr.dll")>
Public Module mpr
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    <ExportAPI("WNetGetUniversalName")>
    Public Declare Function WNetGetUniversalName Lib "mpr" Alias "WNetGetUniversalNameA" (lpLocalPath As String, dwInfoLevel As Integer, ByRef lpBuffer As Object, ByRef lpBufferSize As Integer) As Integer
    Public Declare Function WNetGetUser Lib "mpr" Alias "WNetGetUserA" (lpName As String, lpUserName As String, ByRef lpnLength As Integer) As Integer
    'UPGRADE_WARNING: ?? NETRESOURCE ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function WNetOpenEnum Lib "mpr" Alias "WNetOpenEnumA" (dwScope As Integer, dwType As Integer, dwUsage As Integer, ByRef lpNetResource As NETRESOURCE, ByRef lphEnum As Integer) As Integer

End Module
