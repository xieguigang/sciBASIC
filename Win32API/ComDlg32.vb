Imports Microsoft.VisualBasic.Scripting.MetaData

<[PackageNamespace]("comdlg32.dll", Publisher:="Copyright (C) 2014 Microsoft Corporation")>
Public Module ComDlg32

    'UPGRADE_WARNING: ?? PageSetupDlg ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function PageSetupDlg_Renamed Lib "comdlg32.dll" Alias "PageSetupDlgA" (ByRef pPagesetupdlg As PageSetupDlg) As Integer
    'UPGRADE_WARNING: ?? PrintDlg ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function PrintDlg_Renamed Lib "comdlg32.dll" Alias "PrintDlgA" (ByRef pPrintdlg As PrintDlg) As Integer
    'UPGRADE_WARNING: ?? ChooseFont ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ChooseFont_Renamed Lib "comdlg32.dll" Alias "ChooseFontA" (ByRef pChoosefont As ChooseFont) As Integer
    'UPGRADE_WARNING: ?? FINDREPLACE ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function FindText Lib "comdlg32.dll" Alias "FindTextA " (ByRef pFindreplace As FINDREPLACE) As Integer

    'UPGRADE_WARNING: ?? FINDREPLACE ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ReplaceText Lib "comdlg32.dll" Alias "ReplaceTextA" (ByRef pFindreplace As FINDREPLACE) As Integer
    'UPGRADE_WARNING: ?? ChooseColor ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ChooseColor_Renamed Lib "comdlg32.dll" Alias "ChooseColorA" (ByRef pChoosecolor As ChooseColor) As Integer
    'UPGRADE_WARNING: ?? OPENFILENAME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetOpenFileName Lib "comdlg32.dll" Alias "GetOpenFileNameA" (ByRef pOpenfilename As OPENFILENAME) As Integer

    'UPGRADE_WARNING: ?? OPENFILENAME ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function GetSaveFileName Lib "comdlg32.dll" Alias "GetSaveFileNameA" (ByRef pOpenfilename As OPENFILENAME) As Integer

    Public Declare Function GetFileTitle Lib "comdlg32.dll" Alias "GetFileTitleA" (lpszFile As String, lpszTitle As String, cbBuf As Short) As Short


    ''' <summary>
    ''' CommDlgExtendedError returns the error code from the last common dialog box function. This function does not return error codes for any other API function; for that, use GetLastError instead. 
    ''' </summary>
    ''' <returns>
    ''' The function's return value is undefined if the last common
    ''' dialog function call was successful. If an error with a common dialog function did occur, the return value is exactly one of the
    ''' following common dialog error flags: 
    ''' </returns>
    ''' <remarks></remarks>
    ''' 
    <ExportAPI("CommDlgExtendedError")>
    Public Declare Function CommDlgExtendedError Lib "comdlg32.dll" () As Long

    ''' <summary>
    ''' The function could not open the dialog box. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const CDERR_DIALOGFAILURE = &HFFFF
    ''' <summary>
    ''' The function failed to find the desired resource. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const CDERR_FINDRESFAILURE = &H6
    ''' <summary>
    ''' The error involved a general common dialog box property. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const CDERR_GENERALCODES = &H0
    ''' <summary>
    ''' The function failed during initialization (probably insufficient memory). 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const CDERR_INITIALIZATION = &H2
    ''' <summary>
    ''' The function failed to load the desired resource. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const CDERR_LOADRESFAILURE = &H7
    ''' <summary>
    ''' The function failed to load the desired string. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const CDERR_LOADSTRFAILURE = &H5
    ''' <summary>
    ''' The function failed to lock the desired resource. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const CDERR_LOCKRESFAILURE = &H8
    ''' <summary>
    ''' The function failed to allocate sufficient memory. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const CDERR_MEMALLOCFAILURE = &H9
    ''' <summary>
    ''' The function failed to lock the desired memory. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const CDERR_MEMLOCKFAILURE = &HA
    ''' <summary>
    ''' The function was not provided with a valid instance handle (if one was required). 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const CDERR_NOHINSTANCE = &H4
    ''' <summary>
    ''' The function was not provided with a valid hook function handle (if one was required). 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const CDERR_NOHOOK = &HB
    ''' <summary>
    ''' The function was not provided with a valid template (if one was required). 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const CDERR_NOTEMPLATE = &H3
    ''' <summary>
    ''' The function failed to successfully register a window message. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const CDERR_REGISTERMSGFAIL = &HC
    ''' <summary>
    ''' The function was provided with an invalid structure size. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const CDERR_STRUCTSIZE = &H1
    ''' <summary>
    ''' The error involved the Choose Font common dialog box. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const CFERR_CHOOSEFONTCODES = &H2000
    ''' <summary>
    ''' The function was provided with a maximum font size value smaller than the provided minimum font size. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const CFERR_MAXLESSTHANMIN = &H2002
    ''' <summary>
    ''' The function could not find any existing fonts. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const CFERR_NOFONTS = &H2001
    ''' <summary>
    ''' The function was provided with a filename buffer which was too small. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const FNERR_BUFFERTOOSMALL = &H3003
    ''' <summary>
    ''' The error involved the Open File or Save File common dialog box. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const FNERR_FILENAMECODES = &H3000
    ''' <summary>
    ''' The function was provided with or received an invalid filename. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const FNERR_INVALIDFILENAME = &H3002
    ''' <summary>
    ''' The function had insufficient memory to subclass the list box. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const FNERR_SUBCLASSFAILURE = &H3001
    ''' <summary>
    ''' The function was provided with an invalid buffer. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const FRERR_BUFFERLENGTHZERO = &H4001
    ''' <summary>
    ''' The error involved the Find or Replace common dialog box. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const FRERR_FINDREPLACECODES = &H4000
    ''' <summary>
    ''' The function failed to create an information context. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const PDERR_CREATEICFAILURE = &H100A
    ''' <summary>
    ''' The function was told that the information provided described the default printer, but the default printer's actual settings were
    ''' different. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const PDERR_DEFAULTDIFFERENT = &H100C
    ''' <summary>
    ''' The data in the two data structures describe different printers (i.e., they hold conflicting information). 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const PDERR_DNDMMISMATCH = &H1009
    ''' <summary>
    ''' The printer driver failed to initialize the DEVMODE structure. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const PDERR_GETDEVMODEFAIL = &H1005
    ''' <summary>
    ''' The function failed during initialization. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const PDERR_INITFAILURE = &H1006
    ''' <summary>
    ''' The function failed to load the desired device driver. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const PDERR_LOADDRVFAILURE = &H1004
    ''' <summary>
    ''' The function could not find a default printer. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const PDERR_NODEFAULTPRN = &H1008
    ''' <summary>
    ''' The function could not find any printers. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const PDERR_NODEVICES = &H1007
    ''' <summary>
    ''' The function failed to parse the printer-related strings in WIN.INI. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const PDERR_PARSEFAILURE = &H1002
    ''' <summary>
    ''' The error involved the Print common dialog box. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const PDERR_PRINTERCODES = &H1000
    ''' <summary>
    ''' The function could not find information in WIN.INI about the requested printer. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const PDERR_PRINTERNOTFOUND = &H100B
    ''' <summary>
    ''' The handles to the data structures provided were nonzero even though the function was asked to return information about
    ''' the default printer. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const PDERR_RETDEFFAILURE = &H1003
    ''' <summary>
    ''' The function failed to load the desired resources. 
    ''' </summary>
    ''' <remarks></remarks>
    <ImportsConstant> Public Const PDERR_SETUPFAILURE = &H1001

End Module
