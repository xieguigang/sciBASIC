
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
    <ExportAPI("SetCapture")>
    Public Declare Function SetCapture Lib "user32" (hwnd As Integer) As Integer
    <ExportAPI("ReleaseCapture")>
    Public Declare Function ReleaseCapture Lib "user32" () As Integer
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

End Module
