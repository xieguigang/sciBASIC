#Region "Microsoft.VisualBasic::124a9054dec3c7bacf520368622b3d75, ..\Win32API\lz32.vb"

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

