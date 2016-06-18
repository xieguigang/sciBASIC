
Imports Microsoft.VisualBasic.Scripting.MetaData

<PackageNamespace("lz32.dll")>
Public Module lz32

    <ExportAPI("CopyLZFile")>
    Public Declare Function CopyLZFile Lib "lz32" (n1 As Integer, n2 As Integer) As Integer
    Public Declare Function LZStart Lib "lz32" () As Integer
    Public Declare Sub LZDone Lib "lz32" ()

    Public Declare Function LZCopy Lib "lz32.dll" (hfSource As Integer, hfDest As Integer) As Integer
    Public Declare Function LZInit Lib "lz32.dll" (hfSrc As Integer) As Integer
    Public Declare Function GetExpandedName Lib "lz32.dll" Alias "GetExpandedNameA" (lpszSource As String, lpszBuffer As String) As Integer
    'UPGRADE_WARNING: ?? OFSTRUCT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function LZOpenFile Lib "lz32.dll" Alias "LZOpenFileA" (lpszFile As String, ByRef lpOf As OFSTRUCT, style As Integer) As Integer
    Public Declare Function LZSeek Lib "lz32.dll" (hfFile As Integer, lOffset As Integer, nOrigin As Integer) As Integer
    Public Declare Function LZRead Lib "lz32.dll" (hfFile As Integer, lpvBuf As String, cbread As Integer) As Integer
    Public Declare Sub LZClose Lib "lz32.dll" (hfFile As Integer)

End Module
