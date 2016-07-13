#Region "Microsoft.VisualBasic::f70e4043ecf3fddcadc51d02809860c6, ..\VisualBasic_AppFramework\Win32API\User32.vb"

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

<[PackageNamespace]("user32.dll",
                    Description:="USER32.DLL implements the Windows USER component that creates and manipulates the standard 
                    elements of the Windows user interface, such as the desktop, windows, and menus. It thus enables programs to 
                    implement a graphical user interface (GUI) that matches the Windows look and feel. Programs call functions from 
                    Windows USER to perform operations such as creating and managing windows, receiving window messages 
                    (which are mostly user input such as mouse and keyboard events, but also notifications from the operating system), 
                    displaying text in a window, and displaying message boxes.
                    Many of the functions in USER32.DLL call upon GDI functions exported by GDI32.DLL to do the actual rendering of the 
                    various elements of the user interface. Some types of programs will also call GDI functions directly to perform lower-level 
                    drawing operations within a window previously created via USER32 functions.",
                    Publisher:="Copyright (C) 2014 Microsoft Corporation",
                    Url:="")>
Public Module User32

    <ExportAPI("SetWindowsHookExA")>
    Public Declare Function SetWindowsHookEx Lib "user32" Alias "SetWindowsHookExA" (idHook As Integer, lpfn As Integer, hmod As Integer, dwThreadId As Integer) As Integer
    <ExportAPI("SendMessageA")>
    Public Declare Function SendMessage Lib "user32" Alias "SendMessageA" (hWnd As Integer, wMsg As Integer, wParam As Integer, lParam As Object) As Integer
    <ExportAPI("GetWindowRect")>
    Public Declare Function GetWindowRect Lib "user32" (hwnd As Integer, lpRect As RECT) As Integer
    <ExportAPI("GetCursorPos")>
    Public Declare Function GetCursorPos Lib "user32" (lpPoint As System.Drawing.Point) As Integer
    <ExportAPI("SetCursorPos")>
    Public Declare Function SetCursorPos Lib "user32" (x As Integer, y As Integer) As Integer
    <ExportAPI("DrawEdge")>
    Public Declare Function DrawEdge Lib "user32" (hdc As Integer, qrc As RECT, edge As Integer, grfFlags As Integer) As Boolean
    <ExportAPI("OffsetRect")>
    Public Declare Function OffsetRect Lib "user32" (lpRect As RECT, x As Integer, y As Integer) As Integer
    <ExportAPI("DrawTextA")>
    Public Declare Function DrawText Lib "user32" Alias "DrawTextA" (hdc As Integer, lpStr As String, nCount As Integer, lpRect As RECT, wFormat As Integer) As Integer
    <ExportAPI("GetDC")>
    Public Declare Function GetDC Lib "user32" (hwnd As Integer) As Integer
    <ExportAPI("ReleaseDC")>
    Public Declare Function ReleaseDC Lib "user32" (hwnd As Integer, hdc As Integer) As Integer
    <ExportAPI("FillRect")>
    Public Declare Function FillRect Lib "user32" (hdc As Integer, lpRect As RECT, hBrush As Integer) As Integer
    <ExportAPI("DrawStateA")>
    Public Declare Function DrawState Lib "user32" Alias "DrawStateA" (hdc As Integer, hBrush As Integer, lpDrawStateProc As Integer, lParam As Integer, wParam As Integer, n1 As Integer, n2 As Integer, n3 As Integer, n4 As Integer, un As Integer) As Integer
    <ExportAPI("GetWindowDC")>
    Public Declare Function GetWindowDC Lib "user32" (hwnd As Integer) As Integer
    <ExportAPI("IntersectRect")>
    Public Declare Function IntersectRect Lib "user32" (lpDestRect As RECT, lpSrc1Rect As RECT, lpSrc2Rect As RECT) As Integer
    <ExportAPI("SubtractRect")>
    Public Declare Function SubtractRect Lib "user32" (lprcDst As RECT, lprcSrc1 As RECT, lprcSrc2 As RECT) As Integer
    <ExportAPI("UnionRect")>
    Public Declare Function UnionRect Lib "user32" (lpDestRect As RECT, lpSrc1Rect As RECT, lpSrc2Rect As RECT) As Integer
    <ExportAPI("IsRectEmpty")>
    Public Declare Function IsRectEmpty Lib "user32" (lpRect As RECT) As Integer
    <ExportAPI("SetRect")>
    Public Declare Function SetRect Lib "user32" (lpRect As RECT, X1 As Integer, Y1 As Integer, X2 As Integer, Y2 As Integer) As Integer
    <ExportAPI("EqualRect")>
    Public Declare Function EqualRect Lib "user32" (lpRect1 As RECT, lpRect2 As RECT) As Integer

	    'UPGRADE_WARNING: ?? TPMPARAMS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function TrackPopupMenuEx Lib "user32" (hMenu As Integer, un As Integer, n1 As Integer, n2 As Integer, hWnd As Integer, ByRef lpTPMParams As TPMPARAMS) As Integer
    Public Declare Function UnhookWindowsHook Lib "user32" (nCode As Integer, pfnFilterProc As Integer) As Integer
    Public Declare Function VkKeyScanEx Lib "user32" Alias "VkKeyScanExA" (ch As Byte, dwhkl As Integer) As Short

	    'UPGRADE_WARNING: ?? WNDCLASSEX ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function RegisterClassEx Lib "user32" Alias "RegisterClassExA" (ByRef pcWndClassEx As WNDCLASSEX) As Short
    Public Declare Function SetMenuContextHelpId Lib "user32" (hMenu As Integer, dw As Integer) As Integer
    Public Declare Function SetMenuDefaultItem Lib "user32" (hMenu As Integer, uItem As Integer, fByPos As Integer) As Integer
    'UPGRADE_WARNING: ?? MENUITEMINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetMenuItemInfo Lib "user32" Alias "SetMenuItemInfoA" (hMenu As Integer, un As Integer, bool As Boolean, ByRef lpcMenuItemInfo As MENUITEMINFO) As Integer
    Public Declare Function SetMessageExtraInfo Lib "user32" (lParam As Integer) As Integer
    Public Declare Function SetMessageQueue Lib "user32" (cMessagesMax As Integer) As Integer
    Public Declare Function SetProcessWindowStation Lib "user32" (hWinSta As Integer) As Integer
    'UPGRADE_WARNING: ?? SCROLLINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function SetScrollInfo Lib "user32" (hWnd As Integer, n As Integer, ByRef lpcScrollInfo As SCROLLINFO, bool As Boolean) As Integer
    Public Declare Function SetSystemCursor Lib "user32" (hcur As Integer, id As Integer) As Integer
    Public Declare Function SetThreadDesktop Lib "user32" (hDesktop As Integer) As Integer
    Public Declare Function SetTimer Lib "user32" (hWnd As Integer, nIDEvent As Integer, uElapse As Integer, lpTimerFunc As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function SetUserObjectInformation Lib "user32" Alias "SetUserObjectInformationA" (hObj As Integer, nIndex As Integer, ByRef pvInfo As Object, nLength As Integer) As Integer
    Public Declare Function SetWindowContextHelpId Lib "user32" (hWnd As Integer, dw As Integer) As Integer
    Public Declare Function SetWindowRgn Lib "user32" (hWnd As Integer, hRgn As Integer, bRedraw As Boolean) As Integer
    Public Declare Function SetWindowsHook Lib "user32" Alias "SetWindowsHookA" (nFilterType As Integer, pfnFilterProc As Integer) As Integer
    Public Declare Function ShowWindowAsync Lib "user32" (hWnd As Integer, nCmdShow As Integer) As Integer
    Public Declare Function SwitchDesktop Lib "user32" (hDesktop As Integer) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function TileWindows Lib "user32" (hwndParent As Integer, wHow As Integer, ByRef lpRect As RECT, cKids As Integer, ByRef lpKids As Integer) As Short
    Public Declare Function ToAsciiEx Lib "user32" (uVirtKey As Integer, uScanCode As Integer, ByRef lpKeyState As Byte, ByRef lpChar As Short, uFlags As Integer, dwhkl As Integer) As Integer
	
	    'UPGRADE_WARNING: ?? MSGBOXPARAMS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function MessageBoxIndirect Lib "user32" Alias "MessageBoxIndirectA" (ByRef lpMsgBoxParams As MSGBOXPARAMS) As Integer
    Public Declare Function OpenDesktop Lib "user32" Alias "OpenDesktopA" (lpszDesktop As String, dwFlags As Integer, fInherit As Boolean, dwDesiredAccess As Integer) As Integer
    Public Declare Function OpenInputDesktop Lib "user32" (dwFlags As Integer, fInherit As Boolean, dwDesiredAccess As Integer) As Integer
    Public Declare Function OpenWindowStation Lib "user32" Alias "OpenWindowStationA" (lpszWinSta As String, fInherit As Boolean, dwDesiredAccess As Integer) As Integer
    Public Declare Function PaintDesktop Lib "user32" (hdc As Integer) As Integer

	    'UPGRADE_WARNING: ?? SCROLLINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetScrollInfo Lib "user32" (hWnd As Integer, n As Integer, ByRef lpScrollInfo As SCROLLINFO) As Integer
    Public Declare Function GetSysColorBrush Lib "user32" (nIndex As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function GetUserObjectInformation Lib "user32" Alias "GetUserObjectInformationA" (hObj As Integer, nIndex As Integer, ByRef pvInfo As Object, nLength As Integer, ByRef lpnLengthNeeded As Integer) As Integer
    Public Declare Function GetWindowContextHelpId Lib "user32" (hWnd As Integer) As Integer
    Public Declare Function GetWindowRgn Lib "user32" (hWnd As Integer, hRgn As Integer) As Integer
    Public Declare Function GrayString Lib "user32" Alias "GrayStringA" (hdc As Integer, hBrush As Integer, lpOutputFunc As Integer, lpData As Integer, nCount As Integer, X As Integer, Y As Integer, nWidth As Integer, nHeight As Integer) As Integer
    'UPGRADE_WARNING: ?? MENUITEMINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function InsertMenuItem Lib "user32" Alias "InsertMenuItemA" (hMenu As Integer, un As Integer, bool As Boolean, lpcMenuItemInfo As MENUITEMINFO) As Integer
    Public Declare Function LoadCursorFromFile Lib "user32" Alias "LoadCursorFromFileA" (lpFileName As String) As Integer
    Public Declare Function LoadImage Lib "user32" Alias "LoadImageA" (hInst As Integer, lpsz As String, un1 As Integer, n1 As Integer, n2 As Integer, un2 As Integer) As Integer
    Public Declare Function LookupIconIdFromDirectoryEx Lib "user32" (ByRef presbits As Byte, fIcon As Boolean, cxDesired As Integer, cyDesired As Integer, Flags As Integer) As Integer
    Public Declare Function MapVirtualKeyEx Lib "user32" Alias "MapVirtualKeyExA" (uCode As Integer, uMapType As Integer, dwhkl As Integer) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function MenuItemFromPoint Lib "user32" (hWnd As Integer, hMenu As Integer, ptScreen As POINTAPI) As Integer
	
    'UPGRADE_WARNING: ?? MENUITEMINFO ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetMenuItemInfo Lib "user32" Alias "GetMenuItemInfoA" (hMenu As Integer, un As Integer, b As Boolean, ByRef lpMenuItemInfo As MENUITEMINFO) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetMenuItemRect Lib "user32" (hWnd As Integer, hMenu As Integer, uItem As Integer, ByRef lprcItem As RECT) As Integer

	    'UPGRADE_WARNING: ?? DRAWTEXTPARAMS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function DrawTextEx Lib "user32" Alias "DrawTextExA" (hdc As Integer, lpsz As String, n As Integer, ByRef lpRect As RECT, un As Integer, ByRef lpDrawTextParams As DRAWTEXTPARAMS) As Integer
    Public Declare Function EnumChildWindows Lib "user32" (hwndParent As Integer, lpEnumFunc As Integer, lParam As Integer) As Integer
    Public Declare Function EnumDesktops Lib "user32" Alias "EnumDesktopsA" (hWinSta As Integer, lpEnumFunc As Integer, lParam As Integer) As Integer
    Public Declare Function EnumDesktopWindows Lib "user32" (hDesktop As Integer, lpfn As Integer, lParam As Integer) As Integer
    Public Declare Function EnumPropsEx Lib "user32" Alias "EnumPropsExA" (hWnd As Integer, lpEnumFunc As Integer, lParam As Integer) As Integer
    Public Declare Function EnumProps Lib "user32" Alias "EnumPropsA" (hWnd As Integer, lpEnumFunc As Integer) As Integer
    Public Declare Function EnumThreadWindows Lib "user32" (dwThreadId As Integer, lpfn As Integer, lParam As Integer) As Integer
    Public Declare Function EnumWindowStations Lib "user32" Alias "EnumWindowStationsA" (lpEnumFunc As Integer, lParam As Integer) As Integer
    Public Declare Function FindWindowEx Lib "user32" Alias "FindWindowExA" (hWnd1 As Integer, hWnd2 As Integer, lpsz1 As String, lpsz2 As String) As Integer

    Public Declare Function GetKeyboardLayoutList Lib "user32" (nBuff As Integer, ByRef lpList As Integer) As Integer
    Public Declare Function GetKeyboardLayout Lib "user32" (dwLayout As Integer) As Integer
    Public Declare Function GetMenuContextHelpId Lib "user32" (hMenu As Integer) As Integer
    Public Declare Function GetMenuDefaultItem Lib "user32" (hMenu As Integer, fByPos As Integer, gmdiFlags As Integer) As Integer

	    Public Declare Function BroadcastSystemMessage Lib "user32" (dw As Integer, ByRef pdw As Integer, un As Integer, wParam As Integer, lParam As Integer) As Integer
    Public Declare Function CallWindowProc Lib "user32" Alias "CallWindowProcA" (lpPrevWndFunc As Integer, hWnd As Integer, Msg As Integer, wParam As Integer, lParam As Integer) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CascadeWindows Lib "user32" (hwndParent As Integer, wHow As Integer, lpRect As RECT, cKids As Integer, ByRef lpKids As Integer) As Short
    Public Declare Function ChangeMenu Lib "user32" Alias "ChangeMenuA" (hMenu As Integer, cmd As Integer, lpszNewItem As String, cmdInsert As Integer, Flags As Integer) As Integer
    Public Declare Function CheckMenuRadioItem Lib "user32" (hMenu As Integer, un1 As Integer, un2 As Integer, un3 As Integer, un4 As Integer) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ChildWindowFromPoint Lib "user32" (hwndParent As Integer, pt As POINTAPI) As Integer
    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ChildWindowFromPointEx Lib "user32" (hWnd As Integer, pt As POINTAPI, un As Integer) As Integer
    Public Declare Function CloseDesktop Lib "user32" (hDesktop As Integer) As Integer
    Public Declare Function CloseWindowStation Lib "user32" (hWinSta As Integer) As Integer

    Public Declare Function CopyImage Lib "user32" (handle As Integer, un1 As Integer, n1 As Integer, n2 As Integer, un2 As Integer) As Integer
    'UPGRADE_WARNING: ?? SECURITY_ATTRIBUTES ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? DEVMODE ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateDesktop Lib "user32" Alias "CreateDesktopA" (lpszDesktop As String, lpszDevice As String, ByRef pDevmode As DEVMODE, dwFlags As Integer, dwDesiredAccess As Integer, ByRef lpsa As SECURITY_ATTRIBUTES) As Integer
    'UPGRADE_WARNING: ?? DLGTEMPLATE ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateDialogIndirectParam Lib "user32" Alias "CreateDialogIndirectParamA" (hInstance As Integer, ByRef lpTemplate As DLGTEMPLATE, hwndParent As Integer, lpDialogFunc As Integer, dwInitParam As Integer) As Integer
    Public Declare Function CreateDialogParam Lib "user32" Alias "CreateDialogParamA" (hInstance As Integer, lpName As String, hwndParent As Integer, lpDialogFunc As Integer, lParamInit As Integer) As Integer
    Public Declare Function CreateIconFromResource Lib "user32" (ByRef presbits As Byte, dwResSize As Integer, fIcon As Boolean, dwVer As Integer) As Integer
    'UPGRADE_WARNING: ?? DLGTEMPLATE ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function DialogBoxIndirectParam Lib "user32" Alias "DialogBoxIndirectParamA" (hInstance As Integer, ByRef hDialogTemplate As DLGTEMPLATE, hwndParent As Integer, lpDialogFunc As Integer, dwInitParam As Integer) As Integer

    'UPGRADE_WARNING: ?? POINTAPI ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function DragDetect Lib "user32" (hWnd As Integer, pt As POINTAPI) As Integer
    Public Declare Function DragObject Lib "user32" (hWnd1 As Integer, hWnd2 As Integer, un As Integer, dw As Integer, hCursor As Integer) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function DrawAnimatedRects Lib "user32" (hWnd As Integer, idAni As Integer, ByRef lprcFrom As RECT, ByRef lprcTo As RECT) As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function DrawCaption Lib "user32" (hWnd As Integer, hdc As Integer, ByRef pcRect As RECT, un As Integer) As Integer

    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function DrawFrameControl Lib "user32" (hdc As Integer, ByRef lpRect As RECT, un1 As Integer, un2 As Integer) As Integer
    Public Declare Function DrawIconEx Lib "user32" (hdc As Integer, xLeft As Integer, yTop As Integer, hIcon As Integer, cxWidth As Integer, cyWidth As Integer, istepIfAniCur As Integer, hbrFlickerFreeDraw As Integer, diFlags As Integer) As Integer

    Public Declare Function DdeInitialize Lib "user32" Alias "DdeInitializeA" (ByRef pidInst As Integer, pfnCallback As Integer, afCmd As Integer, ulRes As Integer) As Short

    Public Declare Sub SetDebugErrorLevel Lib "user32" (dwLevel As Integer)


    Public Declare Function EndDialog Lib "user32" (hDlg As Integer, nResult As Integer) As Integer
    Public Declare Function GetDlgItem Lib "user32" (hDlg As Integer, nIDDlgItem As Integer) As Integer
    Public Declare Function SetDlgItemInt Lib "user32" (hDlg As Integer, nIDDlgItem As Integer, wValue As Integer, bSigned As Integer) As Integer
    Public Declare Function GetDlgItemInt Lib "user32" (hDlg As Integer, nIDDlgItem As Integer, lpTranslated As Integer, bSigned As Integer) As Integer
    Public Declare Function SetDlgItemText Lib "user32" Alias "SetDlgItemTextA" (hDlg As Integer, nIDDlgItem As Integer, lpString As String) As Integer
    Public Declare Function GetDlgItemText Lib "user32" Alias "GetDlgItemTextA" (hDlg As Integer, nIDDlgItem As Integer, lpString As String, nMaxCount As Integer) As Integer
    Public Declare Function CheckDlgButton Lib "user32" Alias "CheckDLGButtonA" (hDlg As Integer, nIDButton As Integer, wCheck As Integer) As Integer
    Public Declare Function CheckRadioButton Lib "user32" Alias "CheckRadioButtonA" (hDlg As Integer, nIDFirstButton As Integer, nIDLastButton As Integer, nIDCheckButton As Integer) As Integer
    Public Declare Function IsDlgButtonChecked Lib "user32" (hDlg As Integer, nIDButton As Integer) As Integer
    Public Declare Function SendDlgItemMessage Lib "user32" Alias "SendDlgItemMessageA" (hDlg As Integer, nIDDlgItem As Integer, wMsg As Integer, wParam As Integer, lParam As Integer) As Integer
    Public Declare Function GetNextDlgGroupItem Lib "user32" (hDlg As Integer, hCtl As Integer, bPrevious As Integer) As Integer
    Public Declare Function GetNextDlgTabItem Lib "user32" (hDlg As Integer, hCtl As Integer, bPrevious As Integer) As Integer
    Public Declare Function GetDlgCtrlID Lib "user32" (hWnd As Integer) As Integer
    Public Declare Function GetDialogBaseUnits Lib "user32" () As Integer
    Public Declare Function DefDlgProc Lib "user32" Alias "DefDlgProcA" (hDlg As Integer, wMsg As Integer, wParam As Integer, lParam As Integer) As Integer

    Public Const DLGWINDOWEXTRA As Short = 30 '  Window extra bytes needed for private dialog classes

    'UPGRADE_WARNING: ?? Msg ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CallMsgFilter Lib "user32" Alias "CallMsgFilterA" (ByRef lpMsg As Msg, nCode As Integer) As Integer

    ' Clipboard Manager Functions
    Public Declare Function OpenClipboard Lib "user32" (hWnd As Integer) As Integer
    Public Declare Function CloseClipboard Lib "user32" () As Integer
    Public Declare Function GetClipboardOwner Lib "user32" () As Integer
    Public Declare Function SetClipboardViewer Lib "user32" (hWnd As Integer) As Integer
    Public Declare Function GetClipboardViewer Lib "user32" () As Integer
    Public Declare Function ChangeClipboardChain Lib "user32" (hWnd As Integer, hWndNext As Integer) As Integer
    Public Declare Function SetClipboardData Lib "user32" Alias "SetClipboardDataA" (wFormat As Integer, hMem As Integer) As Integer
    Public Declare Function GetClipboardData Lib "user32" Alias "GetClipboardDataA" (wFormat As Integer) As Integer
    Public Declare Function RegisterClipboardFormat Lib "user32" Alias "RegisterClipboardFormatA" (lpString As String) As Integer
    Public Declare Function CountClipboardFormats Lib "user32" () As Integer
    Public Declare Function EnumClipboardFormats Lib "user32" (wFormat As Integer) As Integer
    Public Declare Function GetClipboardFormatName Lib "user32" Alias "GetClipboardFormatNameA" (wFormat As Integer, lpString As String, nMaxCount As Integer) As Integer
    Public Declare Function EmptyClipboard Lib "user32" () As Integer
    Public Declare Function IsClipboardFormatAvailable Lib "user32" (wFormat As Integer) As Integer
    Public Declare Function GetPriorityClipboardFormat Lib "user32" (ByRef lpPriorityList As Integer, nCount As Integer) As Integer
    Public Declare Function GetOpenClipboardWindow Lib "user32" () As Integer
    Public Declare Function CharToOem Lib "user32" Alias "CharToOemA" (lpszSrc As String, lpszDst As String) As Integer
    Public Declare Function OemToChar Lib "user32" Alias "OemToCharA" (lpszSrc As String, lpszDst As String) As Integer
    Public Declare Function CharToOemBuff Lib "user32" Alias "CharToOemBuffA" (lpszSrc As String, lpszDst As String, cchDstLength As Integer) As Integer
    Public Declare Function OemToCharBuff Lib "user32" Alias "OemToCharBuffA" (lpszSrc As String, lpszDst As String, cchDstLength As Integer) As Integer
    Public Declare Function CharUpper Lib "user32" Alias "CharUpperA" (lpsz As String) As String
    Public Declare Function CharUpperBuff Lib "user32" Alias "CharUpperBuffA" (lpsz As String, cchLength As Integer) As Integer
    Public Declare Function CharLower Lib "user32" Alias "CharLowerA" (lpsz As String) As String
    Public Declare Function CharLowerBuff Lib "user32" Alias "CharLowerBuffA" (lpsz As String, cchLength As Integer) As Integer
    Public Declare Function CharNext Lib "user32" Alias "CharNextA" (lpsz As String) As String
    Public Declare Function CharPrev Lib "user32" Alias "CharPrevA" (lpszStart As String, lpszCurrent As String) As String

    ' Language dependent Routines
    'UPGRADE_NOTE: cChar ???? cChar_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
    Public Declare Function IsCharAlpha Lib "user32" Alias "IsCharAlphaA" (cChar_Renamed As Byte) As Integer
    'UPGRADE_NOTE: cChar ???? cChar_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
    Public Declare Function IsCharAlphaNumeric Lib "user32" Alias "IsCharAlphaNumericA" (cChar_Renamed As Byte) As Integer
    'UPGRADE_NOTE: cChar ???? cChar_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
    Public Declare Function IsCharUpper Lib "user32" Alias "IsCharUpperA" (cChar_Renamed As Byte) As Integer
    'UPGRADE_NOTE: cChar ???? cChar_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
    Public Declare Function IsCharLower Lib "user32" Alias "IsCharLowerA" (cChar_Renamed As Byte) As Integer

    Public Declare Function SetFocus Lib "user32" (hWnd As Integer) As Integer
    Public Declare Function GetFocus Lib "user32" () As Integer
    Public Declare Function GetActiveWindow Lib "user32" () As Integer

    ' Keyboard Information Routines
    Public Declare Function GetKBCodePage Lib "user32" () As Integer
    Public Declare Function GetKeyState Lib "user32" (nVirtKey As Integer) As Short
    Public Declare Function GetAsyncKeyState Lib "user32" (vKey As Integer) As Short
    Public Declare Function GetKeyboardState Lib "user32" (ByRef pbKeyState As Byte) As Integer
    Public Declare Function SetKeyboardState Lib "user32" (ByRef lppbKeyState As Byte) As Integer
    Public Declare Function GetKeyboardType Lib "user32" (nTypeFlag As Integer) As Integer
    Public Declare Function GetKeyNameText Lib "user32" Alias "GetKeyNameTextA" (lParam As Integer, lpBuffer As String, nSize As Integer) As Integer

    Public Declare Function ToAscii Lib "user32" (uVirtKey As Integer, uScanCode As Integer, ByRef lpbKeyState As Byte, ByRef lpwTransKey As Integer, fuState As Integer) As Integer
    Public Declare Function ToUnicode Lib "user32" (wVirtKey As Integer, wScanCode As Integer, ByRef lpKeyState As Byte, pwszBuff As String, cchBuff As Integer, wFlags As Integer) As Integer

    Public Declare Function OemKeyScan Lib "user32" (wOemChar As Integer) As Integer
    'UPGRADE_NOTE: cChar ???? cChar_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
    Public Declare Function VkKeyScan Lib "user32" Alias "VkKeyScanA" (cChar_Renamed As Byte) As Short



    Public Declare Sub mouse_event Lib "user32" (dwFlags As Integer, dx As Integer, dy As Integer, cButtons As Integer, dwExtraInfo As Integer)
    Public Declare Function MapVirtualKey Lib "user32" Alias "MapVirtualKeyA" (wCode As Integer, wMapType As Integer) As Integer

    Public Declare Function GetInputState Lib "user32" () As Integer
    Public Declare Function GetQueueStatus Lib "user32" (fuFlags As Integer) As Integer
    Public Declare Function GetCapture Lib "user32" () As Integer
    Public Declare Function SetCapture Lib "user32" (hWnd As Integer) As Integer
    Public Declare Function ReleaseCapture Lib "user32" () As Integer

    Public Declare Function MsgWaitForMultipleObjects Lib "user32" (nCount As Integer, ByRef pHandles As Integer, fWaitAll As Integer, dwMilliseconds As Integer, dwWakeMask As Integer) As Integer


    ' Windows Functions
    Public Declare Function KillTimer Lib "user32" (hWnd As Integer, nIDEvent As Integer) As Integer

    Public Declare Function IsWindowUnicode Lib "user32" (hWnd As Integer) As Integer

    Public Declare Function EnableWindow Lib "user32" (hWnd As Integer, fEnable As Integer) As Integer
    Public Declare Function IsWindowEnabled Lib "user32" (hWnd As Integer) As Integer

    Public Declare Function LoadAccelerators Lib "user32" Alias "LoadAcceleratorsA" (hInstance As Integer, lpTableName As String) As Integer
    'UPGRADE_WARNING: ?? ACCEL ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CreateAcceleratorTable Lib "user32" Alias "CreateAcceleratorTableA" (ByRef lpaccl As ACCEL, cEntries As Integer) As Integer
    Public Declare Function DestroyAcceleratorTable Lib "user32" (haccel As Integer) As Integer
    'UPGRADE_WARNING: ?? ACCEL ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function CopyAcceleratorTable Lib "user32" Alias "CopyAcceleratorTableA" (hAccelSrc As Integer, ByRef lpAccelDst As ACCEL, cAccelEntries As Integer) As Integer
    'UPGRADE_WARNING: ?? Msg ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function TranslateAccelerator Lib "user32" Alias "TranslateAcceleratorA" (hWnd As Integer, hAccTable As Integer, ByRef lpMsg As Msg) As Integer


    Public Declare Function GetSystemMetrics Lib "user32" (nIndex As Integer) As Integer

    Public Declare Function LoadMenu Lib "user32" Alias "LoadMenuA" (hInstance As Integer, lpString As String) As Integer
    Public Declare Function LoadMenuIndirect Lib "user32" Alias "LoadMenuIndirectA" (lpMenuTemplate As Integer) As Integer
    Public Declare Function GetMenu Lib "user32" (hWnd As Integer) As Integer
    Public Declare Function SetMenu Lib "user32" (hWnd As Integer, hMenu As Integer) As Integer
    Public Declare Function HiliteMenuItem Lib "user32" (hWnd As Integer, hMenu As Integer, wIDHiliteItem As Integer, wHilite As Integer) As Integer
    Public Declare Function GetMenuString Lib "user32" Alias "GetMenuStringA" (hMenu As Integer, wIDItem As Integer, lpString As String, nMaxCount As Integer, wFlag As Integer) As Integer
    Public Declare Function GetMenuState Lib "user32" (hMenu As Integer, wID As Integer, wFlags As Integer) As Integer
    Public Declare Function DrawMenuBar Lib "user32" (hWnd As Integer) As Integer
    Public Declare Function GetSystemMenu Lib "user32" (hWnd As Integer, bRevert As Integer) As Integer
    Public Declare Function CreateMenu Lib "user32" () As Integer
    Public Declare Function CreatePopupMenu Lib "user32" () As Integer
    Public Declare Function DestroyMenu Lib "user32" (hMenu As Integer) As Integer
    Public Declare Function CheckMenuItem Lib "user32" (hMenu As Integer, wIDCheckItem As Integer, wCheck As Integer) As Integer
    Public Declare Function EnableMenuItem Lib "user32" (hMenu As Integer, wIDEnableItem As Integer, wEnable As Integer) As Integer
    Public Declare Function GetSubMenu Lib "user32" (hMenu As Integer, nPos As Integer) As Integer
    Public Declare Function GetMenuItemID Lib "user32" (hMenu As Integer, nPos As Integer) As Integer
    Public Declare Function GetMenuItemCount Lib "user32" (hMenu As Integer) As Integer

    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function InsertMenu Lib "user32" Alias "InsertMenuA" (hMenu As Integer, nPosition As Integer, wFlags As Integer, wIDNewItem As Integer, lpNewItem As Object) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function AppendMenu Lib "user32" Alias "AppendMenuA" (hMenu As Integer, wFlags As Integer, wIDNewItem As Integer, lpNewItem As Object) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function ModifyMenu Lib "user32" Alias "ModifyMenuA" (hMenu As Integer, nPosition As Integer, wFlags As Integer, wIDNewItem As Integer, lpString As Object) As Integer
    Public Declare Function RemoveMenu Lib "user32" (hMenu As Integer, nPosition As Integer, wFlags As Integer) As Integer
    Public Declare Function DeleteMenu Lib "user32" (hMenu As Integer, nPosition As Integer, wFlags As Integer) As Integer
    Public Declare Function SetMenuItemBitmaps Lib "user32" (hMenu As Integer, nPosition As Integer, wFlags As Integer, hBitmapUnchecked As Integer, hBitmapChecked As Integer) As Integer
    Public Declare Function GetMenuCheckMarkDimensions Lib "user32" () As Integer
    'UPGRADE_WARNING: ?? RECT ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function TrackPopupMenu Lib "user32" (hMenu As Integer, wFlags As Integer, X As Integer, Y As Integer, nReserved As Integer, hWnd As Integer, ByRef lprc As RECT) As Integer

End Module
