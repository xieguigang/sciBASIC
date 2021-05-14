#Region "Microsoft.VisualBasic::a8249973c8caf43e37764fb9bbd8e1f2, vs_solutions\dev\ApplicationServices\Win32\Win32API.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:

    '     Structure RECT
    ' 
    '         Function: ToString
    ' 
    '     Structure C_BITMAP
    ' 
    '         Function: ToString
    ' 
    '     Module Win32API
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Win32

    Public Structure RECT
        Dim Left As Integer
        Dim Top As Integer
        Dim right As Integer
        Dim bottom As Integer

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        Public Shared Narrowing Operator CType(obj As RECT) As Rectangle
            Dim pt As New Point(obj.Left, obj.Top)
            Dim sz As New Size(obj.right - obj.Left, obj.bottom - obj.Top)
            Return New Rectangle(pt, sz)
        End Operator

        Public Shared Narrowing Operator CType(obj As RECT) As RectangleF
            Dim pt As New PointF(obj.Left, obj.Top)
            Dim sz As New SizeF(obj.right - obj.Left, obj.bottom - obj.Top)
            Return New RectangleF(pt, sz)
        End Operator
    End Structure

    Public Structure C_BITMAP '14 bytes
        Dim bmType As Integer
        Dim bmWidth As Integer
        Dim bmHeight As Integer
        Dim bmWidthBytes As Integer
        Dim bmPlanes As Integer
        Dim bmBitsPixel As Integer
        Dim bmBits As Integer

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure

    Public Module Win32API

        ' 通用
        Declare Function SendMessage Lib "user32" Alias "SendMessageA" (hwnd As Integer, wMsg As Integer, wParam As Integer, lParam As Object) As Integer

        Public Const WM_GETFONT = &H31
        '
        Declare Function SetCapture Lib "user32" (hwnd As Integer) As Integer
        Declare Function ReleaseCapture Lib "user32" () As Integer
        '
        Declare Function GetWindowRect Lib "user32" (hwnd As Integer, lpRect As RECT) As Integer
        Declare Function GetCursorPos Lib "user32" (lpPoint As Point) As Integer
        Declare Function SetCursorPos Lib "user32" (x As Integer, y As Integer) As Integer
        '

        ' 模块: modRunFile
        '
        Declare Function GetWindowsDirectory Lib "kernel32" Alias "GetWindowsDirectoryA" (lpBuffer As String, nSize As Integer) As Integer
        Declare Function GetSystemDirectory Lib "kernel32" Alias "GetSystemDirectoryA" (lpBuffer As String, nSize As Integer) As Integer
        Declare Function ShellExecute Lib "shell32.dll" Alias "ShellExecuteA" (hwnd As Integer, lpOperation As String, lpFile As String, lpParameters As String, lpDirectory As String, nShowCmd As Integer) As Integer
        '

        ' 模块: modDrawEdge
        Declare Function DrawEdge Lib "user32" (hdc As Integer, qrc As RECT, edge As Integer, grfFlags As Integer) As Boolean

        Public Const BDR_RAISEDOUTER = &H1
        Public Const BDR_SUNKENOUTER = &H2
        Public Const BDR_RAISEDINNER = &H4
        Public Const BDR_SUNKENINNER = &H8

        Public Const BDR_OUTER = &H3
        Public Const BDR_INNER = &HC
        Public Const BDR_RAISED = &H5
        Public Const BDR_SUNKEN = &HA

        Public Const EDGE_RAISED = (BDR_RAISEDOUTER Or BDR_RAISEDINNER)
        Public Const EDGE_SUNKEN = (BDR_SUNKENOUTER Or BDR_SUNKENINNER)
        Public Const EDGE_ETCHED = (BDR_SUNKENOUTER Or BDR_RAISEDINNER)
        Public Const EDGE_BUMP = (BDR_RAISEDOUTER Or BDR_SUNKENINNER)

        Public Const BF_LEFT = &H1
        Public Const BF_TOP = &H2
        Public Const BF_RIGHT = &H4
        Public Const BF_BOTTOM = &H8

        Public Const BF_TOPLEFT = (BF_TOP Or BF_LEFT)
        Public Const BF_TOPRIGHT = (BF_TOP Or BF_RIGHT)
        Public Const BF_BOTTOMLEFT = (BF_BOTTOM Or BF_LEFT)
        Public Const BF_BOTTOMRIGHT = (BF_BOTTOM Or BF_RIGHT)
        Public Const BF_RECT = (BF_LEFT Or BF_TOP Or BF_RIGHT Or BF_BOTTOM)

        Public Const BF_DIAGONAL = &H10
        Public Const BF_DIAGONAL_ENDTOPRIGHT = (BF_DIAGONAL Or BF_TOP Or BF_RIGHT)
        Public Const BF_DIAGONAL_ENDTOPLEFT = (BF_DIAGONAL Or BF_TOP Or BF_LEFT)
        Public Const BF_DIAGONAL_ENDBOTTOMLEFT = (BF_DIAGONAL Or BF_BOTTOM Or BF_LEFT)
        Public Const BF_DIAGONAL_ENDBOTTOMRIGHT = (BF_DIAGONAL Or BF_BOTTOM Or BF_RIGHT)

        Public Const BF_MIDDLE = &H800
        Public Const BF_SOFT = &H1000
        Public Const BF_ADJUST = &H2000
        Public Const BF_FLAT = &H4000
        Public Const BF_MONO = &H8000
        '

        ' 模块: modDrawText
        '
        Declare Function DrawText Lib "user32" Alias "DrawTextA" (hdc As Integer, lpStr As String, nCount As Integer, lpRect As RECT, wFormat As Integer) As Integer
        Declare Function SetTextColor Lib "gdi32" (hdc As Integer, crColor As Integer) As Integer
        Declare Function OffsetRect Lib "user32" (lpRect As RECT, x As Integer, y As Integer) As Integer
        Declare Function lstrlen Lib "kernel32" Alias "lstrlenA" (lpString As String) As Integer
        Declare Function SetBkMode Lib "gdi32" (hdc As Integer, nBkMode As Integer) As Integer

        Public Const DT_TOP = &H0
        Public Const DT_LEFT = &H0
        Public Const DT_CENTER = &H1
        Public Const DT_RIGHT = &H2
        Public Const DT_VCENTER = &H4
        Public Const DT_BOTTOM = &H8
        Public Const DT_WORDBREAK = &H10
        Public Const DT_SINGLELINE = &H20
        Public Const DT_EXPANDTABS = &H40
        Public Const DT_TABSTOP = &H80
        Public Const DT_NOCLIP = &H100
        Public Const DT_EXTERNALLEADING = &H200
        Public Const DT_CALCRECT = &H400
        Public Const DT_NOPREFIX = &H800
        Public Const DT_INTERNAL = &H1000

        Public Const OPAQUE = 2
        Public Const TRANSPARENT = 1
        '

        ' 模块: modDrawBitmap
        '
        Declare Function GetPixel Lib "gdi32" (hdc As Integer, x As Integer, y As Integer) As Integer
        Declare Function SetPixelV Lib "gdi32" (hdc As Integer, x As Integer, y As Integer, crColor As Integer) As Integer
        '
        Declare Function GetDC Lib "user32" (hwnd As Integer) As Integer
        Declare Function CreateCompatibleDC Lib "gdi32" (hdc As Integer) As Integer
        Declare Function DeleteDC Lib "gdi32" (hdc As Integer) As Integer
        Declare Function ReleaseDC Lib "user32" (hwnd As Integer, hdc As Integer) As Integer
        Declare Function SaveDC Lib "gdi32" (hdc As Integer) As Integer
        Declare Function RestoreDC Lib "gdi32" (hdc As Integer, nSavedDC As Integer) As Integer
        '
        Declare Function CreateBitmap Lib "gdi32" (nWidth As Integer, nHeight As Integer, nPlanes As Integer, nBitCount As Integer, lpBits As Object) As Integer
        Declare Function CreateCompatibleBitmap Lib "gdi32" (hdc As Integer, nWidth As Integer, nHeight As Integer) As Integer
        Declare Function CreateBitmapIndirect Lib "gdi32" (lpBitmap As C_BITMAP) As Integer
        '
        Declare Function GetObjectA Lib "gdi32" Alias "GetObjectA" (hObject As Integer, nCount As Integer, lpObject As Object) As Integer
        Declare Function SelectObject Lib "gdi32" (hdc As Integer, hObject As Integer) As Integer
        Declare Function DeleteObject Lib "gdi32" (hObject As Integer) As Integer
        '
        Declare Function GetMapMode Lib "gdi32" (hdc As Integer) As Integer
        Declare Function SetMapMode Lib "gdi32" (hdc As Integer, nMapMode As Integer) As Integer
        '
        Declare Function SetBkColor Lib "gdi32" (hdc As Integer, crColor As Integer) As Integer
        '
        Declare Function BitBlt Lib "gdi32" (hDestDC As Integer, x As Integer, y As Integer, nWidth As Integer, nHeight As Integer, hSrcDC As Integer, xSrc As Integer, ySrc As Integer, dwRop As Integer) As Integer
        Declare Function StretchBlt Lib "gdi32" (hdc As Integer, x As Integer, y As Integer, nWidth As Integer, nHeight As Integer, hSrcDC As Integer, xSrc As Integer, ySrc As Integer, nSrcWidth As Integer, nSrcHeight As Integer, dwRop As Integer) As Integer

        Public Const BLACKNESS = &H42           ' (DWORD) dest = BLACK
        Public Const DSTINVERT = &H550009       ' (DWORD) dest = (NOT dest)
        Public Const MERGECOPY = &HC000CA       ' (DWORD) dest = (source AND pattern)
        Public Const MERGEPAINT = &HBB0226      ' (DWORD) dest = (NOT source) OR dest
        Public Const NOTSRCCOPY = &H330008      ' (DWORD) dest = (NOT source)
        Public Const NOTSRCERASE = &H1100A6     ' (DWORD) dest = (NOT src) AND (NOT dest)
        Public Const PATCOPY = &HF00021         ' (DWORD) dest = pattern
        Public Const PATINVERT = &H5A0049       ' (DWORD) dest = pattern XOR dest
        Public Const PATPAINT = &HFB0A09        ' (DWORD) dest = DPSnoo
        Public Const SRCAND = &H8800C6          ' (DWORD) dest = source AND dest
        Public Const SRCCOPY = &HCC0020         ' (DWORD) dest = source
        Public Const SRCERASE = &H440328        ' (DWORD) dest = source AND (NOT dest )
        Public Const SRCINVERT = &H660046       ' (DWORD) dest = source XOR dest
        Public Const SRCPAINT = &HEE0086        ' (DWORD) dest = source OR dest
        Public Const WHITENESS = &HFF0062       ' (DWORD) dest = WHITE
        '
        Declare Function CreateSolidBrush Lib "gdi32" (crColor As Integer) As Integer
        Declare Function CreatePatternBrush Lib "gdi32" (hBitmap As Integer) As Integer
        Declare Function CreatePen Lib "gdi32" (nPenStyle As Integer, nWidth As Integer, crColor As Integer) As Integer

        Public Const PS_SOLID = 0
        Public Const PS_DASH = 1                    '  -------
        Public Const PS_DOT = 2                     '  .......
        Public Const PS_DASHDOT = 3                 '  _._._._
        Public Const PS_DASHDOTDOT = 4              '  _.._.._
        Public Const PS_NULL = 5
        Public Const PS_INSIDEFRAME = 6
        '
        Declare Function FillRect Lib "user32" (hdc As Integer, lpRect As RECT, hBrush As Integer) As Integer
        '
        Declare Function DrawState Lib "user32" Alias "DrawStateA" (hdc As Integer, hBrush As Integer, lpDrawStateProc As Integer, lParam As Integer, wParam As Integer, n1 As Integer, n2 As Integer, n3 As Integer, n4 As Integer, un As Integer) As Integer

        Public Const DSS_DISABLED = &H20
        Public Const DSS_MONO = &H80
        Public Const DSS_NORMAL = &H0
        Public Const DSS_RIGHT = &H8000
        Public Const DSS_UNION = &H10
        Public Const DST_BITMAP = &H4
        Public Const DST_COMPLEX = &H0
        Public Const DST_ICON = &H3
        Public Const DST_PREFIXTEXT = &H2
        Public Const DST_TEXT = &H1
        '
        Declare Function Polygon Lib "gdi32" (hdc As Integer, lpPoint As Point, nCount As Integer) As Integer
        '
        Declare Function GetWindowDC Lib "user32" (hwnd As Integer) As Integer
        Declare Function IntersectRect Lib "user32" (lpDestRect As RECT, lpSrc1Rect As RECT, lpSrc2Rect As RECT) As Integer
        Declare Function SubtractRect Lib "user32" (lprcDst As RECT, lprcSrc1 As RECT, lprcSrc2 As RECT) As Integer
        Declare Function UnionRect Lib "user32" (lpDestRect As RECT, lpSrc1Rect As RECT, lpSrc2Rect As RECT) As Integer
        Declare Function IsRectEmpty Lib "user32" (lpRect As RECT) As Integer
        Declare Function SetRect Lib "user32" (lpRect As RECT, X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer) As Integer
        Declare Function Rectangle Lib "gdi32" (hdc As Integer, X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer) As Integer
        Declare Function EqualRect Lib "user32" (lpRect1 As RECT, lpRect2 As RECT) As Integer
    End Module
End Namespace
