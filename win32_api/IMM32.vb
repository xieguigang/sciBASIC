#Region "Microsoft.VisualBasic::ee24877514190ee868ba0aaa6215fd0c, ..\sciBASIC#\win32_api\IMM32.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
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

<PackageNamespace("imm32.dll")>
Public Module IMM32
    '  prototype of IMM API

    <ExportAPI("ImmInstallIME")>
    Public Declare Function ImmInstallIME Lib "imm32.dll" Alias "ImmInstallIMEA" (lpszIMEFileName As String, lpszLayoutText As String) As Integer
    Public Declare Function ImmGetDefaultIMEWnd Lib "imm32.dll" (hWnd As Integer) As Integer
    Public Declare Function ImmGetDescription Lib "imm32.dll" Alias "ImmGetDescriptionA" (hkl As Integer, lpsz As String, uBufLen As Integer) As Integer
    Public Declare Function ImmGetIMEFileName Lib "imm32.dll" Alias "ImmGetIMEFileNameA" (hkl As Integer, lpStr As String, uBufLen As Integer) As Integer
    Public Declare Function ImmGetProperty Lib "imm32.dll" (hkl As Integer, dw As Integer) As Integer
    Public Declare Function ImmIsIME Lib "imm32.dll" (hkl As Integer) As Integer
    Public Declare Function ImmSimulateHotKey Lib "imm32.dll" (hWnd As Integer, dw As Integer) As Integer
    Public Declare Function ImmCreateContext Lib "imm32.dll" () As Integer
    Public Declare Function ImmDestroyContext Lib "imm32.dll" (himc As Integer) As Integer
    Public Declare Function ImmGetContext Lib "imm32.dll" (hWnd As Integer) As Integer
    Public Declare Function ImmReleaseContext Lib "imm32.dll" (hWnd As Integer, himc As Integer) As Integer
    Public Declare Function ImmAssociateContext Lib "imm32.dll" (hWnd As Integer, himc As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function ImmGetCompositionString Lib "imm32.dll" Alias "ImmGetCompositionStringA" (himc As Integer, dw As Integer, ByRef lpv As Object, dw2 As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function ImmSetCompositionString Lib "imm32.dll" Alias "ImmSetCompositionStringA" (himc As Integer, dwIndex As Integer, ByRef lpComp As Object, dw As Integer, ByRef lpRead As Object, dw2 As Integer) As Integer
    Public Declare Function ImmGetCandidateListCount Lib "imm32.dll" Alias "ImmGetCandidateListCountA" (himc As Integer, ByRef lpdwListCount As Integer) As Integer
    'UPGRADE_WARNING: ?? CANDIDATELIST ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ImmGetCandidateList Lib "imm32.dll" Alias "ImmGetCandidateListA" (himc As Integer, deIndex As Integer, ByRef lpCandidateList As CANDIDATELIST, dwBufLen As Integer) As Integer
    Public Declare Function ImmGetGuideLine Lib "imm32.dll" Alias " ImmGetGuideLineA" (himc As Integer, dwIndex As Integer, lpStr As String, dwBufLen As Integer) As Integer
    Public Declare Function ImmGetConversionStatus Lib "imm32.dll" (himc As Integer, ByRef lpdw As Integer, ByRef lpdw2 As Integer) As Integer
    Public Declare Function ImmSetConversionStatus Lib "imm32.dll" (himc As Integer, dw1 As Integer, dw2 As Integer) As Integer
    Public Declare Function ImmGetOpenStatus Lib "imm32.dll" (himc As Integer) As Integer
    Public Declare Function ImmSetOpenStatus Lib "imm32.dll" (himc As Integer, b As Integer) As Integer
    'UPGRADE_WARNING: ?? LOGFONT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ImmGetCompositionFont Lib "imm32.dll" Alias "ImmGetCompositionFontA" (himc As Integer, ByRef lpLogFont As LOGFONT) As Integer
    'UPGRADE_WARNING: ?? LOGFONT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ImmSetCompositionFont Lib "imm32.dll" Alias "ImmSetCompositionFontA" (himc As Integer, ByRef lpLogFont As LOGFONT) As Integer
    Public Declare Function ImmConfigureIME Lib "imm32.dll" (hkl As Integer, hWnd As Integer, dw As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function ImmEscape Lib "imm32.dll" Alias "ImmEscapeA" (hkl As Integer, himc As Integer, un As Integer, ByRef lpv As Object) As Integer
    'UPGRADE_WARNING: ?? CANDIDATELIST ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ImmGetConversionList Lib "imm32.dll" Alias "ImmGetConversionListA" (hkl As Integer, himc As Integer, lpsz As String, ByRef lpCandidateList As CANDIDATELIST, dwBufLen As Integer, uFlag As Integer) As Integer
    Public Declare Function ImmNotifyIME Lib "imm32.dll" (himc As Integer, dwAction As Integer, dwIndex As Integer, dwValue As Integer) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ImmGetStatusWindowPos Lib "imm32.dll" (himc As Integer, ByRef lpPoint As POINTAPI) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ImmSetStatusWindowPos Lib "imm32.dll" (himc As Integer, ByRef lpPoint As POINTAPI) As Integer
    'UPGRADE_WARNING: ?? COMPOSITIONFORM ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ImmGetCompositionWindow Lib "imm32.dll" (himc As Integer, ByRef lpCompositionForm As COMPOSITIONFORM) As Integer
    'UPGRADE_WARNING: ?? COMPOSITIONFORM ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ImmSetCompositionWindow Lib "imm32.dll" (himc As Integer, ByRef lpCompositionForm As COMPOSITIONFORM) As Integer
    'UPGRADE_WARNING: ?? CANDIDATEFORM ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ImmGetCandidateWindow Lib "imm32.dll" (himc As Integer, dw As Integer, ByRef lpCandidateForm As CANDIDATEFORM) As Integer
    'UPGRADE_WARNING: ?? CANDIDATEFORM ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ImmSetCandidateWindow Lib "imm32.dll" (himc As Integer, ByRef lpCandidateForm As CANDIDATEFORM) As Integer
    Public Declare Function ImmIsUIMessage Lib "imm32.dll" Alias "ImmIsUIMessageA" (hWnd As Integer, un As Integer, wParam As Integer, lParam As Integer) As Integer
    Public Declare Function ImmGetVirtualKey Lib "imm32.dll" (hWnd As Integer) As Integer
    Public Declare Function ImmRegisterWord Lib "imm32.dll" Alias "ImmRegisterWordA" (hkl As Integer, lpszReading As String, dw As Integer, lpszRegister As String) As Integer
    Public Declare Function ImmUnregisterWord Lib "imm32.dll" Alias "ImmUnregisterWordA" (hkl As Integer, lpszReading As String, dw As Integer, lpszUnregister As String) As Integer
    'UPGRADE_WARNING: ?? STYLEBUF ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ImmGetRegisterWordStyle Lib "imm32.dll" Alias " ImmGetRegisterWordStyleA" (hkl As Integer, nItem As Integer, ByRef lpStyleBuf As STYLEBUF) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function ImmEnumRegisterWord Lib "imm32.dll" Alias "ImmEnumRegisterWordA" (hkl As Integer, RegisterWordEnumProc As Integer, lpszReading As String, dw As Integer, lpszRegister As String, ByRef lpv As Object) As Integer

End Module
