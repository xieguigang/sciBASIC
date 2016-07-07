#Region "Microsoft.VisualBasic::96d23013a7fa589ebf1513b3a94e9d4f, ..\VisualBasic_AppFramework\Win32API\winspool_drv.vb"

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

<PackageNamespace("winspool.drv")>
Public Module winspool_drv

    'UPGRADE_WARNING: ?? DEVMODE ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function DeviceCapabilities Lib "winspool.drv" Alias "DeviceCapabilitiesA" (lpDeviceName As String, lpPort As String, iIndex As Integer, lpOutput As String, ByRef lpDevMode As DEVMODE) As Integer

    Public Declare Function DeletePrinter Lib "winspool.drv" (hPrinter As Integer) As Integer

    Public Declare Function FindClosePrinterChangeNotification Lib "winspool.drv" (hChange As Integer) As Integer
    Public Declare Function FindFirstPrinterChangeNotification Lib "winspool.drv" (hPrinter As Integer, fdwFlags As Integer, fdwOptions As Integer, pPrinterNotifyOptions As String) As Integer
    Public Declare Function FindNextPrinterChangeNotification Lib "winspool.drv" (hChange As Integer, ByRef pdwChange As Integer, pvReserved As String, ppPrinterNotifyInfo As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function GetPrinter Lib "winspool.drv" Alias "GetPrinterA" (hPrinter As Integer, Level As Integer, ByRef pPrinter As Object, cbBuf As Integer, ByRef pcbNeeded As Integer) As Integer
    'UPGRADE_NOTE: Command ???? Command_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
    Public Declare Function SetPrinter Lib "winspool.drv" Alias "SetPrinterA" (hPrinter As Integer, Level As Integer, ByRef pPrinter As Byte, Command_Renamed As Integer) As Integer
    Public Declare Function ClosePrinter Lib "winspool.drv" (hPrinter As Integer) As Integer
    Public Declare Function AddForm Lib "winspool.drv" Alias "AddFormA" (hPrinter As Integer, Level As Integer, ByRef pForm As Byte) As Integer
    Public Declare Function DeleteForm Lib "winspool.drv" Alias "DeleteFormA" (hPrinter As Integer, pFormName As String) As Integer
    Public Declare Function GetForm Lib "winspool.drv" Alias "GetFormA" (hPrinter As Integer, pFormName As String, Level As Integer, ByRef pForm As Byte, cbBuf As Integer, ByRef pcbNeeded As Integer) As Integer
    Public Declare Function SetForm Lib "winspool.drv" Alias "SetFormA" (hPrinter As Integer, pFormName As String, Level As Integer, ByRef pForm As Byte) As Integer
    Public Declare Function EnumForms Lib "winspool.drv" Alias "EnumFormsA" (hPrinter As Integer, Level As Integer, ByRef pForm As Byte, cbBuf As Integer, ByRef pcbNeeded As Integer, ByRef pcReturned As Integer) As Integer

    Public Declare Function EnumMonitors Lib "winspool.drv" Alias "EnumMonitorsA" (pName As String, Level As Integer, ByRef pMonitors As Byte, cbBuf As Integer, ByRef pcbNeeded As Integer, ByRef pcReturned As Integer) As Integer
    Public Declare Function AddMonitor Lib "winspool.drv" Alias "AddMonitorA" (pName As String, Level As Integer, ByRef pMonitors As Byte) As Integer
    Public Declare Function DeleteMonitor Lib "winspool.drv" Alias "DeleteMonitorA" (pName As String, pEnvironment As String, pMonitorName As String) As Integer

    Public Declare Function EnumPorts Lib "winspool.drv" Alias "EnumPortsA" (pName As String, Level As Integer, lpbPorts As Integer, cbBuf As Integer, ByRef pcbNeeded As Integer, ByRef pcReturned As Integer) As Integer
    Public Declare Function AddPort Lib "winspool.drv" Alias "AddPortA" (pName As String, hWnd As Integer, pMonitorName As String) As Integer
    Public Declare Function ConfigurePort Lib "winspool.drv" Alias "ConfigurePortA" (pName As String, hWnd As Integer, pPortName As String) As Integer
    Public Declare Function DeletePort Lib "winspool.drv" Alias "DeletePortA" (pName As String, hWnd As Integer, pPortName As String) As Integer

    Public Declare Function AddPrinterConnection Lib "winspool.drv" Alias "AddPrinterConnectionA" (pName As String) As Integer

    Public Declare Function DeletePrinterConnection Lib "winspool.drv" Alias "DeletePrinterConnectionA" (pName As String) As Integer
    Public Declare Function ConnectToPrinterDlg Lib "winspool.drv" (hWnd As Integer, Flags As Integer) As Integer

    Public Declare Function AddPrintProvidor Lib "winspool.drv" Alias "AddPrintProvidorA" (pName As String, Level As Integer, ByRef pProvidorInfo As Byte) As Integer
    Public Declare Function DeletePrintProvidor Lib "winspool.drv" Alias "DeletePrintProvidorA" (pName As String, pEnvironment As String, pPrintProvidorName As String) As Integer

    'UPGRADE_NOTE: error ???? error_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
    Public Declare Function PrinterMessageBox Lib "winspool.drv" Alias "PrinterMessageBoxA" (hPrinter As Integer, error_Renamed As Integer, hWnd As Integer, pText As String, pCaption As String, dwType As Integer) As Integer
    'UPGRADE_WARNING: ?? PRINTER_DEFAULTS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function OpenPrinter Lib "winspool.drv" Alias "OpenPrinterA" (pPrinterName As String, ByRef phPrinter As Integer, ByRef pDefault As PRINTER_DEFAULTS) As Integer
    'UPGRADE_WARNING: ?? PRINTER_DEFAULTS ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function ResetPrinter Lib "winspool.drv" Alias "ResetPrinterA" (hPrinter As Integer, ByRef pDefault As PRINTER_DEFAULTS) As Integer
    'UPGRADE_NOTE: Command ???? Command_Renamed? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="A9E4979A-37FA-4718-9994-97DD76ED70A7"”
    Public Declare Function SetJob Lib "winspool.drv" Alias "SetJobA" (hPrinter As Integer, JobId As Integer, Level As Integer, ByRef pJob As Byte, Command_Renamed As Integer) As Integer
    Public Declare Function GetJob Lib "winspool.drv" Alias "GetJobA" (hPrinter As Integer, JobId As Integer, Level As Integer, ByRef pJob As Byte, cdBuf As Integer, ByRef pcbNeeded As Integer) As Integer
    Public Declare Function EnumJobs Lib "winspool.drv" Alias "EnumJobsA" (hPrinter As Integer, FirstJob As Integer, NoJobs As Integer, Level As Integer, ByRef pJob As Byte, cdBuf As Integer, ByRef pcbNeeded As Integer, ByRef pcReturned As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function AddPrinter Lib "winspool.drv" Alias "AddPrinterA" (pName As String, Level As Integer, ByRef pPrinter As Object) As Integer

    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function AddPrinterDriver Lib "winspool.drv" Alias "AddPrinterDriverA" (pName As String, Level As Integer, ByRef pDriverInfo As Object) As Integer

    Public Declare Function EnumPrinterDrivers Lib "winspool.drv" Alias "EnumPrinterDriversA" (pName As String, pEnvironment As String, Level As Integer, ByRef pDriverInfo As Byte, cdBuf As Integer, ByRef pcbNeeded As Integer, ByRef pcRetruned As Integer) As Integer
    Public Declare Function GetPrinterDriver Lib "winspool.drv" Alias "GetPrinterDriverA" (hPrinter As Integer, pEnvironment As String, Level As Integer, ByRef pDriverInfo As Byte, cdBuf As Integer, ByRef pcbNeeded As Integer) As Integer
    Public Declare Function GetPrinterDriverDirectory Lib "winspool.drv" Alias "GetPrinterDriverDirectoryA" (pName As String, pEnvironment As String, Level As Integer, ByRef pDriverDirectory As Byte, cdBuf As Integer, ByRef pcbNeeded As Integer) As Integer
    Public Declare Function DeletePrinterDriver Lib "winspool.drv" Alias "DeletePrinterDriverA" (pName As String, pEnvironment As String, pDriverName As String) As Integer

    Public Declare Function AddPrintProcessor Lib "winspool.drv" Alias "AddPrintProcessorA" (pName As String, pEnvironment As String, pPathName As String, pPrintProcessorName As String) As Integer
    Public Declare Function EnumPrintProcessors Lib "winspool.drv" Alias "EnumPrintProcessorsA" (pName As String, pEnvironment As String, Level As Integer, ByRef pPrintProcessorInfo As Byte, cdBuf As Integer, ByRef pcbNeeded As Integer, ByRef pcReturned As Integer) As Integer
    Public Declare Function GetPrintProcessorDirectory Lib "winspool.drv" Alias "GetPrintProcessorDirectoryA" (pName As String, pEnvironment As String, Level As Integer, pPrintProcessorInfo As String, cdBuf As Integer, ByRef pcbNeeded As Integer) As Integer
    Public Declare Function EnumPrintProcessorDatatypes Lib "winspool.drv" Alias "EnumPrintProcessorDatatypesA" (pName As String, pPrintProcessorName As String, Level As Integer, ByRef pDatatypes As Byte, cdBuf As Integer, ByRef pcbNeeded As Integer, ByRef pcRetruned As Integer) As Integer
    Public Declare Function DeletePrintProcessor Lib "winspool.drv" Alias "DeletePrintProcessorA" (pName As String, pEnvironment As String, pPrintProcessorName As String) As Integer

    Public Declare Function StartDocPrinter Lib "winspool.drv" Alias "StartDocPrinterA" (hPrinter As Integer, Level As Integer, ByRef pDocInfo As Byte) As Integer
    Public Declare Function StartPagePrinter Lib "winspool.drv" (hPrinter As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function WritePrinter Lib "winspool.drv" (hPrinter As Integer, ByRef pBuf As Object, cdBuf As Integer, ByRef pcWritten As Integer) As Integer
    Public Declare Function EndPagePrinter Lib "winspool.drv" (hPrinter As Integer) As Integer
    Public Declare Function AbortPrinter Lib "winspool.drv" (hPrinter As Integer) As Integer
    'UPGRADE_ISSUE: ?????????“As Object”? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="FAE78A8D-8978-4FD4-8208-5B7324A8F795"”
    Public Declare Function ReadPrinter Lib "winspool.drv" (hPrinter As Integer, ByRef pBuf As Object, cdBuf As Integer, ByRef pNoBytesRead As Integer) As Integer
    Public Declare Function EndDocPrinter Lib "winspool.drv" (hPrinter As Integer) As Integer

    Public Declare Function AddJob Lib "winspool.drv" Alias "AddJobA" (hPrinter As Integer, Level As Integer, ByRef pData As Byte, cdBuf As Integer, ByRef pcbNeeded As Integer) As Integer
    Public Declare Function ScheduleJob Lib "winspool.drv" (hPrinter As Integer, JobId As Integer) As Integer
    Public Declare Function PrinterProperties Lib "winspool.drv" (hWnd As Integer, hPrinter As Integer) As Integer
    'UPGRADE_WARNING: ?? DEVMODE ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? DEVMODE ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function DocumentProperties Lib "winspool.drv" Alias "DocumentPropertiesA" (hWnd As Integer, hPrinter As Integer, pDeviceName As String, ByRef pDevModeOutput As DEVMODE, ByRef pDevModeInput As DEVMODE, fMode As Integer) As Integer
    'UPGRADE_WARNING: ?? DEVMODE ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    'UPGRADE_WARNING: ?? DEVMODE ????????????? Public Declare ????????? ?????????:“ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?keyword="C429C3A5-5D47-4CD9-8F51-74A1616405DC"”
    Public Declare Function AdvancedDocumentProperties Lib "winspool.drv" Alias "AdvancedDocumentPropertiesA" (hWnd As Integer, hPrinter As Integer, pDeviceName As String, ByRef pDevModeOutput As DEVMODE, ByRef pDevModeInput As DEVMODE) As Integer

    Public Declare Function GetPrinterData Lib "winspool.drv" Alias "GetPrinterDataA" (hPrinter As Integer, pValueName As String, ByRef pType As Integer, ByRef pData As Byte, nSize As Integer, ByRef pcbNeeded As Integer) As Integer
    Public Declare Function SetPrinterData Lib "winspool.drv" Alias "SetPrinterDataA" (hPrinter As Integer, pValueName As String, dwType As Integer, ByRef pData As Byte, cbData As Integer) As Integer
    Public Declare Function WaitForPrinterChange Lib "winspool.drv" (hPrinter As Integer, Flags As Integer) As Integer
    Public Declare Function EnumPrinters Lib "winspool.drv" Alias "EnumPrintersA" (Flags As Integer, name As String, Level As Integer, ByRef pPrinterEnum As Byte, cdBuf As Integer, ByRef pcbNeeded As Integer, ByRef pcReturned As Integer) As Integer

    Public Declare Function EnumPrinterPropertySheets Lib "winspool.drv" (ByRef hPrinter As Integer, ByRef hWnd As Integer, ByRef lpfnAdd As Integer, lParam As Integer) As Integer

End Module
