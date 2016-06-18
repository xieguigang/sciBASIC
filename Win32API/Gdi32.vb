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
    <ExportAPI("Rectangle")>
    Public Declare Function Rectangle Lib "gdi32" (hdc As Integer, X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer) As Integer
    <ExportAPI("Polygon")>
    Public Declare Function Polygon Lib "gdi32" (hdc As Integer, lpPoint As Point, nCount As Integer) As Integer
    <ExportAPI("CreateSolidBrush")>
    Public Declare Function CreateSolidBrush Lib "gdi32" (crColor As Integer) As Integer
    <ExportAPI("CreatePatternBrush")>
    Public Declare Function CreatePatternBrush Lib "gdi32" (hBitmap As Integer) As Integer
    <ExportAPI("CreatePen")>
    Public Declare Function CreatePen Lib "gdi32" (nPenStyle As Integer, nWidth As Integer, crColor As Integer) As Integer
    <ExportAPI("GetPixel")>
    Public Declare Function GetPixel Lib "gdi32" (hdc As Integer, x As Integer, y As Integer) As Integer
    <ExportAPI("SetPixelV")>
    Public Declare Function SetPixelV Lib "gdi32" (hdc As Integer, x As Integer, y As Integer, crColor As Integer) As Integer
    <ExportAPI("CreateCompatibleDC")>
    Public Declare Function CreateCompatibleDC Lib "gdi32" (hdc As Integer) As Integer
    <ExportAPI("DeleteDC")>
    Public Declare Function DeleteDC Lib "gdi32" (hdc As Integer) As Integer
    <ExportAPI("SaveDC")>
    Public Declare Function SaveDC Lib "gdi32" (hdc As Integer) As Integer
    <ExportAPI("RestoreDC")>
    Public Declare Function RestoreDC Lib "gdi32" (hdc As Integer, nSavedDC As Integer) As Integer
    <ExportAPI("CreateBitmap")>
    Public Declare Function CreateBitmap Lib "gdi32" (nWidth As Integer, nHeight As Integer, nPlanes As Integer, nBitCount As Integer, lpBits As Object) As Integer
    <ExportAPI("CreateCompatibleBitmap")>
    Public Declare Function CreateCompatibleBitmap Lib "gdi32" (hdc As Integer, nWidth As Integer, nHeight As Integer) As Integer
    <ExportAPI("CreateBitmapIndirect")>
    Public Declare Function CreateBitmapIndirect Lib "gdi32" (lpBitmap As C_BITMAP) As Integer
    <ExportAPI("GetObjectA")>
    Public Declare Function GetObject Lib "gdi32" Alias "GetObjectA" (hObject As Integer, nCount As Integer, lpObject As Object) As Integer
    <ExportAPI("SelectObject")>
    Public Declare Function SelectObject Lib "gdi32" (hdc As Integer, hObject As Integer) As Integer
    <ExportAPI("DeleteObject")>
    Public Declare Function DeleteObject Lib "gdi32" (hObject As Integer) As Integer
    <ExportAPI("GetMapMode")>
    Public Declare Function GetMapMode Lib "gdi32" (hdc As Integer) As Integer
    <ExportAPI("SetMapMode")>
    Public Declare Function SetMapMode Lib "gdi32" (hdc As Integer, nMapMode As Integer) As Integer
    <ExportAPI("SetBkColor")>
    Public Declare Function SetBkColor Lib "gdi32" (hdc As Integer, crColor As Integer) As Integer
    <ExportAPI("BitBlt")>
    Public Declare Function BitBlt Lib "gdi32" (hDestDC As Integer, x As Integer, y As Integer, nWidth As Integer, nHeight As Integer, hSrcDC As Integer, xSrc As Integer, ySrc As Integer, dwRop As Integer) As Integer
    <ExportAPI("StretchBlt")>
    Public Declare Function StretchBlt Lib "gdi32" (hdc As Integer, x As Integer, y As Integer, nWidth As Integer, nHeight As Integer, hSrcDC As Integer, xSrc As Integer, ySrc As Integer, nSrcWidth As Integer, nSrcHeight As Integer, dwRop As Integer) As Integer
    <ExportAPI("SetTextColor")>
    Public Declare Function SetTextColor Lib "gdi32" (hdc As Integer, crColor As Integer) As Integer
    <ExportAPI("SetBkMode")>
    Public Declare Function SetBkMode Lib "gdi32" (hdc As Integer, nBkMode As Integer) As Integer


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

End Module
