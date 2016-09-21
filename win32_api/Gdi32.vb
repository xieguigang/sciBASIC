#Region "Microsoft.VisualBasic::f17767e09e06662cce0b0fe3fe1a98d4, ..\visualbasic_App\Win32API\Gdi32.vb"

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

Imports System.Drawing
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Win32
Imports Microsoft.VisualBasic.Win32.Win32API

<PackageNamespace("gdi32.dll",
                  Description:="GDI32.DLL exports Graphics Device Interface (GDI) functions that perform primitive drawing functions for 
                  output to video displays and printers. Applications call GDI functions directly to perform low-level drawing, text output, 
                  font management, and similar functions.
                  Initially, GDI supported 16 and 256 color EGA/VGA display cards and monochrome printers. The functionality has expanded 
                  over the years, and now includes support for things like TrueType fonts, alpha channels, and multiple monitors.",
                  Publisher:="Copyright (C) 2014 Microsoft Corporation")>
Public Module Gdi32

    ''' <summary>
    ''' The Escape function allows applications to access capabilities of a particular device not directly available through GDI. Escape calls made by an application are translated and sent to the driver 
    ''' </summary>
    ''' <param name="hdc">Identifies the device context.</param>
    ''' <param name="nEscape">Specifies the escape function to be performed. This parameter must be one of the predefined escape values. Use the ExtEscape function if your application defines a private escape value.</param>
    ''' <param name="nCount">Specifies the number of bytes of data pointed to by the lpvInData parameter.</param>
    ''' <param name="lpInData">Points to the input structure required for the specified escape.</param>
    ''' <param name="lpOutData">
    ''' Points to the structure that receives output from this escape. This parameter should be NULL if no data is returned
    ''' 
    ''' If the function succeeds, the return value is greater than zero, except with the QUERYESCSUPPORT printer escape, which checks for implementation only. If the escape is not implemented, the return value is zero. 
    ''' 
    ''' If the function fails, the return value is an error. To get extended error information, call GetLastError. 
    ''' </param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("Escape", Info:="The Escape function allows applications to access capabilities of a particular device not directly available through GDI. Escape calls made by an application are translated and sent to the driver.")>
    Public Declare Function Escape Lib "gdi32" Alias "Escape" (hdc As Long, nEscape As Long, nCount As Long, lpInData As String, lpOutData As Object) As Long
    <ExportAPI("Polygon")>
    Public Declare Function Polygon Lib "gdi32" (hdc As Integer, lpPoint As Point, nCount As Integer) As Integer
    <ExportAPI("CreateSolidBrush")>
    Public Declare Function CreateSolidBrush Lib "gdi32" (crColor As Integer) As Integer
    <ExportAPI("CreatePatternBrush")>
    Public Declare Function CreatePatternBrush Lib "gdi32" (hBitmap As Integer) As Integer
    <ExportAPI("CreatePen")>
    Public Declare Function CreatePen Lib "gdi32" (nPenStyle As Integer, nWidth As Integer, crColor As Integer) As Integer
    <ExportAPI("CreateCompatibleDC")>
    Public Declare Function CreateCompatibleDC Lib "gdi32" (hdc As Integer) As Integer
    <ExportAPI("DeleteDC")>
    Public Declare Function DeleteDC Lib "gdi32" (hdc As Integer) As Integer
    <ExportAPI("CreateBitmap")>
    Public Declare Function CreateBitmap Lib "gdi32" (nWidth As Integer, nHeight As Integer, nPlanes As Integer, nBitCount As Integer, lpBits As Object) As Integer
    <ExportAPI("CreateCompatibleBitmap")>
    Public Declare Function CreateCompatibleBitmap Lib "gdi32" (hdc As Integer, nWidth As Integer, nHeight As Integer) As Integer
    <ExportAPI("CreateBitmapIndirect")>
    Public Declare Function CreateBitmapIndirect Lib "gdi32" (lpBitmap As C_BITMAP) As Integer
    <ExportAPI("GetObjectA")>
    Public Declare Function GetObject Lib "gdi32" Alias "GetObjectA" (hObject As Integer, nCount As Integer, lpObject As Object) As Integer
    <ExportAPI("DeleteObject")>
    Public Declare Function DeleteObject Lib "gdi32" (hObject As Integer) As Integer
    <ExportAPI("BitBlt")>
    Public Declare Function BitBlt Lib "gdi32" (hDestDC As Integer, x As Integer, y As Integer, nWidth As Integer, nHeight As Integer, hSrcDC As Integer, xSrc As Integer, ySrc As Integer, dwRop As Integer) As Integer


    'UPGRADE_WARNING: ?? PIXELFORMATDESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ChoosePixelFormat Lib "gdi32" (hdc As Integer, ByRef pPixelFormatDescriptor As PIXELFORMATDESCRIPTOR) As Integer
    'UPGRADE_WARNING: ?? BITMAPINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateDIBSection Lib "gdi32" (hdc As Integer, ByRef pBitmapInfo As BITMAPINFO, un As Integer, lplpVoid As Integer, handle As Integer, dw As Integer) As Integer
    'UPGRADE_WARNING: ?? PIXELFORMATDESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function DescribePixelFormat Lib "gdi32" (hdc As Integer, n As Integer, un As Integer, ByRef lpPixelFormatDescriptor As PIXELFORMATDESCRIPTOR) As Integer
    Public Declare Function EndDoc Lib "gdi32" (hdc As Integer) As Integer
    Public Declare Function EnumFonts Lib "gdi32" Alias "EnumFontsA" (hdc As Integer, lpsz As String, lpFontEnumProc As Integer, lParam As Integer) As Integer
    Public Declare Function EnumMetaFile Lib "gdi32" (hdc As Integer, hMetafile As Integer, lpMFEnumProc As Integer, lParam As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function EnumObjects Lib "gdi32" (hdc As Integer, n As Integer, lpGOBJEnumProc As Integer, ByRef lpVoid As Object) As Integer

    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function FixBrushOrgEx Lib "gdi32" (hdc As Integer, n1 As Integer, n2 As Integer, ByRef lpPoint As POINTAPI) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetBrushOrgEx Lib "gdi32" (hdc As Integer, ByRef lpPoint As POINTAPI) As Integer
    Public Declare Function GetCharWidth Lib "gdi32" Alias "GetCharWidthA" (hdc As Integer, un1 As Integer, un2 As Integer, ByRef lpn As Integer) As Integer
    'UPGRADE_WARNING: ?? RGBQUAD ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetDIBColorTable Lib "gdi32" (hdc As Integer, un1 As Integer, un2 As Integer, ByRef pRGBQuad As RGBQUAD) As Integer
    Public Declare Function GetPixelFormat Lib "gdi32" (hdc As Integer) As Integer
    Public Declare Function LineDDA Lib "gdi32" (n1 As Integer, n2 As Integer, n3 As Integer, n4 As Integer, lpLineDDAProc As Integer, lParam As Integer) As Integer

    Public Declare Function SetAbortProcA Lib "gdi32" Alias "SetAbortProc" (hdc As Integer, lpAbortProc As Integer) As Integer
    'UPGRADE_WARNING: ?? RGBQUAD ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetDIBColorTable Lib "gdi32" (hdc As Integer, un1 As Integer, un2 As Integer, ByRef pcRGBQuad As RGBQUAD) As Integer
    'UPGRADE_WARNING: ?? PIXELFORMATDESCRIPTOR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetPixelFormat Lib "gdi32" (hdc As Integer, n As Integer, ByRef pcPixelFormatDescriptor As PIXELFORMATDESCRIPTOR) As Integer
    Public Declare Function SwapBuffers Lib "gdi32" (hdc As Integer) As Integer


    Public Declare Function Ellipse Lib "gdi32" (hdc As Integer, X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer) As Integer

    Public Declare Function EqualRgn Lib "gdi32" (hSrcRgn1 As Integer, hSrcRgn2 As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function Escape Lib "gdi32" (hdc As Integer, nEscape As Integer, nCount As Integer, lpInData As String, ByRef lpOutData As Object) As Integer
    Public Declare Function ExtEscape Lib "gdi32" (hdc As Integer, nEscape As Integer, cbInput As Integer, lpszInData As String, cbOutput As Integer, lpszOutData As String) As Integer
    Public Declare Function DrawEscape Lib "gdi32" (hdc As Integer, nEscape As Integer, cbInput As Integer, lpszInData As String) As Integer
    Public Declare Function ExcludeClipRect Lib "gdi32" (hdc As Integer, X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer) As Integer
    'UPGRADE_WARNING: ?? RgnData ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? xform ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ExtCreateRegion Lib "gdi32" (ByRef lpXform As xform, nCount As Integer, ByRef lpRgnData As RgnData) As Integer
    Public Declare Function ExtFloodFill Lib "gdi32" (hdc As Integer, X As Integer, Y As Integer, crColor As Integer, wFillType As Integer) As Integer
    Public Declare Function FillRgn Lib "gdi32" (hdc As Integer, hRgn As Integer, hBrush As Integer) As Integer
    Public Declare Function FrameRgn Lib "gdi32" (hdc As Integer, hRgn As Integer, hBrush As Integer, nWidth As Integer, nHeight As Integer) As Integer
    Public Declare Function FloodFill Lib "gdi32" (hdc As Integer, X As Integer, Y As Integer, crColor As Integer) As Integer
    Public Declare Function GetROP2 Lib "gdi32" (hdc As Integer) As Integer
    'UPGRADE_WARNING: ?? Size ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetAspectRatioFilterEx Lib "gdi32" (hdc As Integer, ByRef lpAspectRatio As Size) As Integer
    Public Declare Function GetBkColor Lib "gdi32" (hdc As Integer) As Integer
    Public Declare Function GetBkMode Lib "gdi32" (hdc As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function GetBitmapBits Lib "gdi32" (hBitmap As Integer, dwCount As Integer, ByRef lpBits As Object) As Integer
    'UPGRADE_WARNING: ?? Size ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetBitmapDimensionEx Lib "gdi32" (hBitmap As Integer, ByRef lpDimension As Size) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetBoundsRect Lib "gdi32" (hdc As Integer, ByRef lprcBounds As RECT, Flags As Integer) As Integer

    'Public Declare Function GetCharWidth Lib "gdi32" Alias "GetCharWidthA" (hdc As Integer, wFirstChar As Integer, wLastChar As Integer, ByRef lpBuffer As Integer) As Integer
    Public Declare Function GetCharWidth32 Lib "gdi32" Alias "GetCharWidth32A" (hdc As Integer, iFirstChar As Integer, iLastChar As Integer, ByRef lpBuffer As Integer) As Integer
    Public Declare Function GetCharWidthFloat Lib "gdi32" Alias "GetCharWidthFloatA" (hdc As Integer, iFirstChar As Integer, iLastChar As Integer, ByRef pxBuffer As Double) As Integer

    'UPGRADE_WARNING: ?? ABC ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetCharABCWidths Lib "gdi32" Alias "GetCharABCWidthsA" (hdc As Integer, uFirstChar As Integer, uLastChar As Integer, ByRef lpabc As ABC) As Integer
    'UPGRADE_WARNING: ?? ABCFLOAT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetCharABCWidthsFloat Lib "gdi32" Alias "GetCharABCWidthsFloatA" (hdc As Integer, iFirstChar As Integer, iLastChar As Integer, ByRef lpABCF As ABCFLOAT) As Integer

    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetClipBox Lib "gdi32" (hdc As Integer, ByRef lpRect As RECT) As Integer
    Public Declare Function GetClipRgn Lib "gdi32" (hdc As Integer, hRgn As Integer) As Integer
    Public Declare Function GetMetaRgn Lib "gdi32" (hdc As Integer, hRgn As Integer) As Integer
    Public Declare Function GetCurrentObject Lib "gdi32" (hdc As Integer, uObjectType As Integer) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetCurrentPositionEx Lib "gdi32" (hdc As Integer, ByRef lpPoint As POINTAPI) As Integer
    Public Declare Function GetDeviceCaps Lib "gdi32" (hdc As Integer, nIndex As Integer) As Integer
    'UPGRADE_WARNING: ?? BITMAPINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function GetDIBits Lib "gdi32" (aHDC As Integer, hBitmap As Integer, nStartScan As Integer, nNumScans As Integer, ByRef lpBits As Object, ByRef lpBI As BITMAPINFO, wUsage As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function GetFontData Lib "gdi32" Alias "GetFontDataA" (hdc As Integer, dwTable As Integer, dwOffset As Integer, ByRef lpvBuffer As Object, cbData As Integer) As Integer
    'UPGRADE_WARNING: ?? MAT2 ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    'UPGRADE_WARNING: ?? GLYPHMETRICS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetGlyphOutline Lib "gdi32" Alias "GetGlyphOutlineA" (hdc As Integer, uChar As Integer, fuFormat As Integer, ByRef lpgm As GLYPHMETRICS, cbBuffer As Integer, ByRef lpBuffer As Object, ByRef lpmat2 As MAT2) As Integer
    Public Declare Function GetGraphicsMode Lib "gdi32" (hdc As Integer) As Integer
    Public Declare Function GetMapMode Lib "gdi32" (hdc As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function GetMetaFileBitsEx Lib "gdi32" (hMF As Integer, nSize As Integer, ByRef lpvData As Object) As Integer
    Public Declare Function GetMetaFile Lib "gdi32" Alias "GetMetaFileA" (lpFileName As String) As Integer
    Public Declare Function GetNearestColor Lib "gdi32" (hdc As Integer, crColor As Integer) As Integer
    Public Declare Function GetNearestPaletteIndex Lib "gdi32" (hPalette As Integer, crColor As Integer) As Integer
    Public Declare Function GetObjectType Lib "gdi32" (hgdiobj As Integer) As Integer

    'UPGRADE_WARNING: ?? OUTLINETEXTMETRIC ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetOutlineTextMetrics Lib "gdi32" Alias "GetOutlineTextMetricsA" (hdc As Integer, cbData As Integer, ByRef lpotm As OUTLINETEXTMETRIC) As Integer

    'UPGRADE_WARNING: ?? PALETTEENTRY ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetPaletteEntries Lib "gdi32" (hPalette As Integer, wStartIndex As Integer, wNumEntries As Integer, ByRef lpPaletteEntries As PALETTEENTRY) As Integer
    Public Declare Function GetPixel Lib "gdi32" (hdc As Integer, X As Integer, Y As Integer) As Integer
    Public Declare Function GetPolyFillMode Lib "gdi32" (hdc As Integer) As Integer
    'UPGRADE_WARNING: ?? RASTERIZER_STATUS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetRasterizerCaps Lib "gdi32" (ByRef lpraststat As RASTERIZER_STATUS, cb As Integer) As Integer
    'UPGRADE_WARNING: ?? RgnData ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetRegionData Lib "gdi32" Alias "GetRegionDataA" (hRgn As Integer, dwCount As Integer, ByRef lpRgnData As RgnData) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetRgnBox Lib "gdi32" (hRgn As Integer, ByRef lpRect As RECT) As Integer
    Public Declare Function GetStockObject Lib "gdi32" (nIndex As Integer) As Integer
    Public Declare Function GetStretchBltMode Lib "gdi32" (hdc As Integer) As Integer
    'UPGRADE_WARNING: ?? PALETTEENTRY ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetSystemPaletteEntries Lib "gdi32" (hdc As Integer, wStartIndex As Integer, wNumEntries As Integer, ByRef lpPaletteEntries As PALETTEENTRY) As Integer
    Public Declare Function GetSystemPaletteUse Lib "gdi32" (hdc As Integer) As Integer
    Public Declare Function GetTextCharacterExtra Lib "gdi32" Alias "GetTextCharacterExtraA" (hdc As Integer) As Integer
    Public Declare Function GetTextAlign Lib "gdi32" (hdc As Integer) As Integer
    Public Declare Function GetTextColor Lib "gdi32" (hdc As Integer) As Integer

    'UPGRADE_WARNING: ?? Size ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetTextExtentPoint Lib "gdi32" Alias "GetTextExtentPointA" (hdc As Integer, lpszString As String, cbString As Integer, ByRef lpSize As Size) As Integer
    'UPGRADE_WARNING: ?? Size ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetTextExtentPoint32 Lib "gdi32" Alias "GetTextExtentPoint32A" (hdc As Integer, lpsz As String, cbString As Integer, ByRef lpSize As Size) As Integer
    'UPGRADE_WARNING: ?? Size ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetTextExtentExPoint Lib "gdi32" Alias "GetTextExtentExPointA" (hdc As Integer, lpszStr As String, cchString As Integer, nMaxExtent As Integer, ByRef lpnFit As Integer, ByRef alpDx As Integer, ByRef lpSize As Size) As Integer

    'UPGRADE_WARNING: ?? Size ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetViewportExtEx Lib "gdi32" (hdc As Integer, ByRef lpSize As Size) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetViewportOrgEx Lib "gdi32" (hdc As Integer, ByRef lpPoint As POINTAPI) As Integer
    'UPGRADE_WARNING: ?? Size ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetWindowExtEx Lib "gdi32" (hdc As Integer, ByRef lpSize As Size) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetWindowOrgEx Lib "gdi32" (hdc As Integer, ByRef lpPoint As POINTAPI) As Integer

    Public Declare Function IntersectClipRect Lib "gdi32" (hdc As Integer, X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer) As Integer
    Public Declare Function InvertRgn Lib "gdi32" (hdc As Integer, hRgn As Integer) As Integer
    Public Declare Function LineTo Lib "gdi32" (hdc As Integer, X As Integer, Y As Integer) As Integer
    Public Declare Function MaskBlt Lib "gdi32" (hdcDest As Integer, nXDest As Integer, nYDest As Integer, nWidth As Integer, nHeight As Integer, hdcSrc As Integer, nXSrc As Integer, nYSrc As Integer, hbmMask As Integer, xMask As Integer, yMask As Integer, dwRop As Integer) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function PlgBlt Lib "gdi32" (hdcDest As Integer, ByRef lpPoint As POINTAPI, hdcSrc As Integer, nXSrc As Integer, nYSrc As Integer, nWidth As Integer, nHeight As Integer, hbmMask As Integer, xMask As Integer, yMask As Integer) As Integer

    Public Declare Function OffsetClipRgn Lib "gdi32" (hdc As Integer, X As Integer, Y As Integer) As Integer
    Public Declare Function OffsetRgn Lib "gdi32" (hRgn As Integer, X As Integer, Y As Integer) As Integer
    Public Declare Function PatBlt Lib "gdi32" (hdc As Integer, X As Integer, Y As Integer, nWidth As Integer, nHeight As Integer, dwRop As Integer) As Integer
    Public Declare Function Pie Lib "gdi32" (hdc As Integer, X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer, X3 As Integer, Y3 As Integer, X4 As Integer, Y4 As Integer) As Integer
    Public Declare Function PlayMetaFile Lib "gdi32" (hdc As Integer, hMF As Integer) As Integer
    Public Declare Function PaintRgn Lib "gdi32" (hdc As Integer, hRgn As Integer) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function PolyPolygon Lib "gdi32" (hdc As Integer, ByRef lpPoint As POINTAPI, ByRef lpPolyCounts As Integer, nCount As Integer) As Integer
    Public Declare Function PtInRegion Lib "gdi32" (hRgn As Integer, X As Integer, Y As Integer) As Integer
    Public Declare Function PtVisible Lib "gdi32" (hdc As Integer, X As Integer, Y As Integer) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function RectVisible Lib "gdi32" (hdc As Integer, ByRef lpRect As RECT) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function RectInRegion Lib "gdi32" (hRgn As Integer, ByRef lpRect As RECT) As Integer
    Public Declare Function Rectangle Lib "gdi32" (hdc As Integer, X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer) As Integer
    Public Declare Function RestoreDC Lib "gdi32" (hdc As Integer, nSavedDC As Integer) As Integer
    'UPGRADE_WARNING: ?? DEVMODE ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ResetDC Lib "gdi32" Alias "ResetDCA" (hdc As Integer, ByRef lpInitData As DEVMODE) As Integer
    Public Declare Function RealizePalette Lib "gdi32" (hdc As Integer) As Integer
    Public Declare Function RemoveFontResource Lib "gdi32" Alias "RemoveFontResourceA" (lpFileName As String) As Integer
    Public Declare Function RoundRect Lib "gdi32" (hdc As Integer, X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer, X3 As Integer, Y3 As Integer) As Integer
    Public Declare Function ResizePalette Lib "gdi32" (hPalette As Integer, nNumEntries As Integer) As Integer

    Public Declare Function SaveDC Lib "gdi32" (hdc As Integer) As Integer
    Public Declare Function SelectClipRgn Lib "gdi32" (hdc As Integer, hRgn As Integer) As Integer
    Public Declare Function ExtSelectClipRgn Lib "gdi32" (hdc As Integer, hRgn As Integer, fnMode As Integer) As Integer
    Public Declare Function SetMetaRgn Lib "gdi32" (hdc As Integer) As Integer
    Public Declare Function SelectObject Lib "gdi32" (hdc As Integer, hObject As Integer) As Integer
    Public Declare Function SelectPalette Lib "gdi32" (hdc As Integer, hPalette As Integer, bForceBackground As Integer) As Integer
    Public Declare Function SetBkColor Lib "gdi32" (hdc As Integer, crColor As Integer) As Integer
    Public Declare Function SetBkMode Lib "gdi32" (hdc As Integer, nBkMode As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function SetBitmapBits Lib "gdi32" (hBitmap As Integer, dwCount As Integer, ByRef lpBits As Object) As Integer

    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetBoundsRect Lib "gdi32" (hdc As Integer, ByRef lprcBounds As RECT, Flags As Integer) As Integer
    'UPGRADE_WARNING: ?? BITMAPINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function SetDIBits Lib "gdi32" (hdc As Integer, hBitmap As Integer, nStartScan As Integer, nNumScans As Integer, ByRef lpBits As Object, ByRef lpBI As BITMAPINFO, wUsage As Integer) As Integer
    'UPGRADE_WARNING: ?? BITMAPINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function SetDIBitsToDevice Lib "gdi32" (hdc As Integer, X As Integer, Y As Integer, dx As Integer, dy As Integer, SrcX As Integer, SrcY As Integer, Scan As Integer, NumScans As Integer, ByRef Bits As Object, ByRef BitsInfo As BITMAPINFO, wUsage As Integer) As Integer
    Public Declare Function SetMapperFlags Lib "gdi32" (hdc As Integer, dwFlag As Integer) As Integer
    Public Declare Function SetGraphicsMode Lib "gdi32" (hdc As Integer, iMode As Integer) As Integer
    Public Declare Function SetMapMode Lib "gdi32" (hdc As Integer, nMapMode As Integer) As Integer
    Public Declare Function SetMetaFileBitsEx Lib "gdi32" (nSize As Integer, ByRef lpData As Byte) As Integer
    'UPGRADE_WARNING: ?? PALETTEENTRY ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetPaletteEntries Lib "gdi32" (hPalette As Integer, wStartIndex As Integer, wNumEntries As Integer, ByRef lpPaletteEntries As PALETTEENTRY) As Integer
    Public Declare Function SetPixel Lib "gdi32" (hdc As Integer, X As Integer, Y As Integer, crColor As Integer) As Integer
    Public Declare Function SetPixelV Lib "gdi32" (hdc As Integer, X As Integer, Y As Integer, crColor As Integer) As Integer
    Public Declare Function SetPolyFillMode Lib "gdi32" (hdc As Integer, nPolyFillMode As Integer) As Integer
    Public Declare Function StretchBlt Lib "gdi32" (hdc As Integer, X As Integer, Y As Integer, nWidth As Integer, nHeight As Integer, hSrcDC As Integer, xSrc As Integer, ySrc As Integer, nSrcWidth As Integer, nSrcHeight As Integer, dwRop As Integer) As Integer
    Public Declare Function SetRectRgn Lib "gdi32" (hRgn As Integer, X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer) As Integer
    'UPGRADE_WARNING: ?? BITMAPINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function StretchDIBits Lib "gdi32" (hdc As Integer, X As Integer, Y As Integer, dx As Integer, dy As Integer, SrcX As Integer, SrcY As Integer, wSrcWidth As Integer, wSrcHeight As Integer, ByRef lpBits As Object, ByRef lpBitsInfo As BITMAPINFO, wUsage As Integer, dwRop As Integer) As Integer
    Public Declare Function SetROP2 Lib "gdi32" (hdc As Integer, nDrawMode As Integer) As Integer
    Public Declare Function SetStretchBltMode Lib "gdi32" (hdc As Integer, nStretchMode As Integer) As Integer
    Public Declare Function SetSystemPaletteUse Lib "gdi32" (hdc As Integer, wUsage As Integer) As Integer
    Public Declare Function SetTextCharacterExtra Lib "gdi32" Alias "SetTextCharacterExtraA" (hdc As Integer, nCharExtra As Integer) As Integer
    Public Declare Function SetTextColor Lib "gdi32" (hdc As Integer, crColor As Integer) As Integer
    Public Declare Function SetTextAlign Lib "gdi32" (hdc As Integer, wFlags As Integer) As Integer
    Public Declare Function SetTextJustification Lib "gdi32" (hdc As Integer, nBreakExtra As Integer, nBreakCount As Integer) As Integer
    Public Declare Function UpdateColors Lib "gdi32" (hdc As Integer) As Integer



    'UPGRADE_WARNING: ?? METARECORD ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? HANDLETABLE ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function PlayMetaFileRecord Lib "gdi32" (hdc As Integer, ByRef lpHandletable As HANDLETABLE, ByRef lpMetaRecord As METARECORD, nHandles As Integer) As Integer

    Public Declare Function CloseEnhMetaFile Lib "gdi32" (hdc As Integer) As Integer
    Public Declare Function CopyEnhMetaFile Lib "gdi32" Alias "CopyEnhMetaFileA" (hemfSrc As Integer, lpszFile As String) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateEnhMetaFile Lib "gdi32" Alias "CreateEnhMetaFileA" (hdcRef As Integer, lpFileName As String, ByRef lpRect As RECT, lpDescription As String) As Integer
    Public Declare Function DeleteEnhMetaFile Lib "gdi32" (hemf As Integer) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function EnumEnhMetaFile Lib "gdi32" (hdc As Integer, hemf As Integer, lpEnhMetaFunc As Integer, ByRef lpData As Object, ByRef lpRect As RECT) As Integer
    Public Declare Function GetEnhMetaFile Lib "gdi32" Alias "GetEnhMetaFileA" (lpszMetaFile As String) As Integer
    Public Declare Function GetEnhMetaFileBits Lib "gdi32" (hemf As Integer, cbBuffer As Integer, ByRef lpbBuffer As Byte) As Integer
    Public Declare Function GetEnhMetaFileDescription Lib "gdi32" Alias "GetEnhMetaFileDescriptionA" (hemf As Integer, cchBuffer As Integer, lpszDescription As String) As Integer
    'UPGRADE_WARNING: ?? ENHMETAHEADER ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetEnhMetaFileHeader Lib "gdi32" (hemf As Integer, cbBuffer As Integer, ByRef lpemh As ENHMETAHEADER) As Integer
    'UPGRADE_WARNING: ?? PALETTEENTRY ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetEnhMetaFilePaletteEntries Lib "gdi32" (hemf As Integer, cEntries As Integer, ByRef lppe As PALETTEENTRY) As Integer
    Public Declare Function GetWinMetaFileBits Lib "gdi32" (hemf As Integer, cbBuffer As Integer, ByRef lpbBuffer As Byte, fnMapMode As Integer, hdcRef As Integer) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function PlayEnhMetaFile Lib "gdi32" (hdc As Integer, hemf As Integer, ByRef lpRect As RECT) As Integer
    'UPGRADE_WARNING: ?? ENHMETARECORD ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? HANDLETABLE ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function PlayEnhMetaFileRecord Lib "gdi32" (hdc As Integer, ByRef lpHandletable As HANDLETABLE, ByRef lpEnhMetaRecord As ENHMETARECORD, nHandles As Integer) As Integer
    Public Declare Function SetEnhMetaFileBits Lib "gdi32" (cbBuffer As Integer, ByRef lpData As Byte) As Integer
    'UPGRADE_WARNING: ?? METAFILEPICT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetWinMetaFileBits Lib "gdi32" (cbBuffer As Integer, ByRef lpbBuffer As Byte, hdcRef As Integer, ByRef lpmfp As METAFILEPICT) As Integer
    Public Declare Function GdiComment Lib "gdi32" (hdc As Integer, cbSize As Integer, ByRef lpData As Byte) As Integer

    'UPGRADE_WARNING: ?? TEXTMETRIC ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetTextMetrics Lib "gdi32" Alias "GetTextMetricsA" (hdc As Integer, ByRef lpMetrics As TEXTMETRIC) As Integer

    Public Declare Function AngleArc Lib "gdi32" (hdc As Integer, X As Integer, Y As Integer, dwRadius As Integer, eStartAngle As Double, eSweepAngle As Double) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function PolyPolyline Lib "gdi32" (hdc As Integer, ByRef lppt As POINTAPI, ByRef lpdwPolyPoints As Integer, cCount As Integer) As Integer
    'UPGRADE_WARNING: ?? xform ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetWorldTransform Lib "gdi32" (hdc As Integer, ByRef lpXform As xform) As Integer
    'UPGRADE_WARNING: ?? xform ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetWorldTransform Lib "gdi32" (hdc As Integer, ByRef lpXform As xform) As Integer
    'UPGRADE_WARNING: ?? xform ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ModifyWorldTransform Lib "gdi32" (hdc As Integer, ByRef lpXform As xform, iMode As Integer) As Integer
    'UPGRADE_WARNING: ?? xform ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? xform ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? xform ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CombineTransform Lib "gdi32" (ByRef lpxformResult As xform, ByRef lpxform1 As xform, ByRef lpxform2 As xform) As Integer

    'UPGRADE_WARNING: ?? ColorAdjustment ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetColorAdjustment Lib "gdi32" (hdc As Integer, ByRef lpca As ColorAdjustment) As Integer
    'UPGRADE_WARNING: ?? ColorAdjustment ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetColorAdjustment Lib "gdi32" (hdc As Integer, ByRef lpca As ColorAdjustment) As Integer
    Public Declare Function CreateHalftonePalette Lib "gdi32" (hdc As Integer) As Integer



    'UPGRADE_WARNING: ?? DOCINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function StartDoc Lib "gdi32" Alias "StartDocA" (hdc As Integer, ByRef lpdi As DOCINFO) As Integer
    Public Declare Function StartPage Lib "gdi32" (hdc As Integer) As Integer
    Public Declare Function EndPage Lib "gdi32" (hdc As Integer) As Integer
    'Public Declare Function EndDoc Lib "gdi32" (hdc As Integer) As Integer
    Public Declare Function AbortDoc Lib "gdi32" (hdc As Integer) As Integer

    Public Declare Function AbortPath Lib "gdi32" (hdc As Integer) As Integer
    Public Declare Function ArcTo Lib "gdi32" (hdc As Integer, X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer, X3 As Integer, Y3 As Integer, X4 As Integer, Y4 As Integer) As Integer
    Public Declare Function BeginPath Lib "gdi32" (hdc As Integer) As Integer
    Public Declare Function CloseFigure Lib "gdi32" (hdc As Integer) As Integer
    Public Declare Function EndPath Lib "gdi32" (hdc As Integer) As Integer
    Public Declare Function FillPath Lib "gdi32" (hdc As Integer) As Integer
    Public Declare Function FlattenPath Lib "gdi32" (hdc As Integer) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetPath Lib "gdi32" (hdc As Integer, ByRef lpPoint As POINTAPI, ByRef lpTypes As Byte, nSize As Integer) As Integer
    Public Declare Function PathToRegion Lib "gdi32" (hdc As Integer) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function PolyDraw Lib "gdi32" (hdc As Integer, ByRef lppt As POINTAPI, ByRef lpbTypes As Byte, cCount As Integer) As Integer
    Public Declare Function SelectClipPath Lib "gdi32" (hdc As Integer, iMode As Integer) As Integer
    Public Declare Function SetArcDirection Lib "gdi32" (hdc As Integer, ArcDirection As Integer) As Integer
    Public Declare Function SetMiterLimit Lib "gdi32" (hdc As Integer, eNewLimit As Double, ByRef peOldLimit As Double) As Integer
    Public Declare Function StrokeAndFillPath Lib "gdi32" (hdc As Integer) As Integer
    Public Declare Function StrokePath Lib "gdi32" (hdc As Integer) As Integer
    Public Declare Function WidenPath Lib "gdi32" (hdc As Integer) As Integer
    'UPGRADE_WARNING: ?? LOGBRUSH ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ExtCreatePen Lib "gdi32" (dwPenStyle As Integer, dwWidth As Integer, ByRef lplb As LOGBRUSH, dwStyleCount As Integer, ByRef lpStyle As Integer) As Integer
    Public Declare Function GetMiterLimit Lib "gdi32" (hdc As Integer, ByRef peLimit As Double) As Integer
    Public Declare Function GetArcDirection Lib "gdi32" (hdc As Integer) As Integer

    'UPGRADE_NOTE: GetObject ???? GetObject_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function GetObject_Renamed Lib "gdi32" Alias "GetObjectA" (hObject As Integer, nCount As Integer, ByRef lpObject As Object) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function MoveToEx Lib "gdi32" (hdc As Integer, X As Integer, Y As Integer, ByRef lpPoint As POINTAPI) As Integer
    Public Declare Function TextOut Lib "gdi32" Alias "TextOutA" (hdc As Integer, X As Integer, Y As Integer, lpString As String, nCount As Integer) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ExtTextOut Lib "gdi32" Alias "ExtTextOutA" (hdc As Integer, X As Integer, Y As Integer, wOptions As Integer, ByRef lpRect As RECT, lpString As String, nCount As Integer, ByRef lpDX As Integer) As Integer
    'UPGRADE_WARNING: ?? POLYTEXT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function PolyTextOut Lib "gdi32" Alias "PolyTextOutA" (hdc As Integer, ByRef pptxt As POLYTEXT, ByRef cStrings As Integer) As Integer

    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreatePolygonRgn Lib "gdi32" (ByRef lpPoint As POINTAPI, nCount As Integer, nPolyFillMode As Integer) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function DPtoLP Lib "gdi32" (hdc As Integer, ByRef lpPoint As POINTAPI, nCount As Integer) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function LPtoDP Lib "gdi32" (hdc As Integer, ByRef lpPoint As POINTAPI, nCount As Integer) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function Polyline Lib "gdi32" (hdc As Integer, ByRef lpPoint As POINTAPI, nCount As Integer) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function Polygon Lib "gdi32" (hdc As Integer, ByRef lpPoint As POINTAPI, nCount As Integer) As Integer

    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function PolyBezier Lib "gdi32" (hdc As Integer, ByRef lppt As POINTAPI, cPoints As Integer) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function PolyBezierTo Lib "gdi32" (hdc As Integer, ByRef lppt As POINTAPI, cCount As Integer) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function PolylineTo Lib "gdi32" (hdc As Integer, ByRef lppt As POINTAPI, cCount As Integer) As Integer

    'UPGRADE_WARNING: ?? Size ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetViewportExtEx Lib "gdi32" (hdc As Integer, nX As Integer, nY As Integer, ByRef lpSize As Size) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetViewportOrgEx Lib "gdi32" (hdc As Integer, nX As Integer, nY As Integer, ByRef lpPoint As POINTAPI) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetWindowOrgEx Lib "gdi32" (hdc As Integer, nX As Integer, nY As Integer, ByRef lpPoint As POINTAPI) As Integer
    'UPGRADE_WARNING: ?? Size ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetWindowExtEx Lib "gdi32" (hdc As Integer, nX As Integer, nY As Integer, ByRef lpSize As Size) As Integer

    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function OffsetViewportOrgEx Lib "gdi32" (hdc As Integer, nX As Integer, nY As Integer, ByRef lpPoint As POINTAPI) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function OffsetWindowOrgEx Lib "gdi32" (hdc As Integer, nX As Integer, nY As Integer, ByRef lpPoint As POINTAPI) As Integer
    'UPGRADE_WARNING: ?? Size ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ScaleWindowExtEx Lib "gdi32" (hdc As Integer, nXnum As Integer, nXdenom As Integer, nYnum As Integer, nYdenom As Integer, ByRef lpSize As Size) As Integer
    'UPGRADE_WARNING: ?? Size ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ScaleViewportExtEx Lib "gdi32" (hdc As Integer, nXnum As Integer, nXdenom As Integer, nYnum As Integer, nYdenom As Integer, ByRef lpSize As Size) As Integer
    'UPGRADE_WARNING: ?? Size ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetBitmapDimensionEx Lib "gdi32" (hbm As Integer, nX As Integer, nY As Integer, ByRef lpSize As Size) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetBrushOrgEx Lib "gdi32" (hdc As Integer, nXOrg As Integer, nYOrg As Integer, ByRef lppt As POINTAPI) As Integer

    Public Declare Function GetTextFace Lib "gdi32" Alias "GetTextFaceA" (hdc As Integer, nCount As Integer, lpFacename As String) As Integer

    'UPGRADE_WARNING: ?? KERNINGPAIR ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetKerningPairs Lib "gdi32" Alias "GetKerningPairsA" (hdc As Integer, cPairs As Integer, ByRef lpkrnpair As KERNINGPAIR) As Integer

    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetDCOrgEx Lib "gdi32" (hdc As Integer, ByRef lpPoint As POINTAPI) As Integer
    Public Declare Function UnrealizeObject Lib "gdi32" (hObject As Integer) As Integer

    Public Declare Function GdiFlush Lib "gdi32" () As Integer
    Public Declare Function GdiSetBatchLimit Lib "gdi32" (dwLimit As Integer) As Integer
    Public Declare Function GdiGetBatchLimit Lib "gdi32" () As Integer

End Module
