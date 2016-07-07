#Region "Microsoft.VisualBasic::3a11f7a856939a52b44ae59df669550e, ..\VisualBasic_AppFramework\Win32API\Version.vb"

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
