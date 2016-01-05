'Public Class Excel

'    ''' <summary>
'    ''' 一个Excel文档
'    ''' </summary>
'    ''' <remarks></remarks>
'    Dim XlsxDocument As Global.DocumentFormat.OpenXml.Packaging.SpreadsheetDocument
'    Dim _Path As String

'    Public Overrides Function ToString() As String
'        Return _Path
'    End Function

'    ''' <summary>
'    ''' Adds a new worksheet to the workbook
'    ''' </summary>
'    ''' <param name="name">Name of the worksheet</param>
'    ''' <returns>True if succesful</returns>
'    Public Function AddWorksheet(name As String) As Boolean
'        Dim sheets As Global.DocumentFormat.OpenXml.Spreadsheet.Sheets = XlsxDocument.WorkbookPart.Workbook.GetFirstChild(Of Global.DocumentFormat.OpenXml.Spreadsheet.Sheets)()
'        Dim worksheetPart As Global.DocumentFormat.OpenXml.Packaging.WorksheetPart

'        ' Add the worksheetpart
'        worksheetPart = XlsxDocument.WorkbookPart.AddNewPart(Of Global.DocumentFormat.OpenXml.Packaging.WorksheetPart)()
'        worksheetPart.Worksheet = New Global.DocumentFormat.OpenXml.Spreadsheet.Worksheet(New Global.DocumentFormat.OpenXml.Spreadsheet.SheetData())
'        worksheetPart.Worksheet.Save()

'        ' Add the sheet and make relation to workbook
'        Dim sheet As Global.DocumentFormat.OpenXml.Spreadsheet.Sheet = New Global.DocumentFormat.OpenXml.Spreadsheet.Sheet With {
'            .Id = XlsxDocument.WorkbookPart.GetIdOfPart(worksheetPart),
'            .SheetId = (XlsxDocument.WorkbookPart.Workbook.Sheets.Count() + 1),
'            .Name = name}

'        Call sheets.Append(sheet)

'        Return True
'    End Function

'    Public Function Save(Optional FilePath As String = "") As Boolean
'        If String.IsNullOrEmpty(FilePath) Then
'            ' Workbook
'            Call XlsxDocument.WorkbookPart.Workbook.Save()
'            ' Shared string table
'            Call XlsxDocument.WorkbookPart.SharedStringTablePart.SharedStringTable.Save()
'            ' Stylesheet
'            Call XlsxDocument.WorkbookPart.WorkbookStylesPart.Stylesheet.Save()
'        Else '另存为
'            Call FileIO.FileSystem.CopyFile(Me._Path, FilePath)
'            Dim Excel As Excel = Excel.Open(FilePath)
'            Excel.XlsxDocument = Me.XlsxDocument
'            Call Excel.Save()

'            Me.XlsxDocument = Excel.XlsxDocument
'            Me._Path = FilePath
'        End If

'        Return True
'    End Function

'    Public Shared Function Open(Path As String) As Excel
'        Return New Excel With {.XlsxDocument = Global.DocumentFormat.OpenXml.Packaging.SpreadsheetDocument.Open(Path, isEditable:=True), ._Path = Path}
'    End Function

'    ''' <summary>
'    ''' 创建一个新的Excel文件
'    ''' </summary>
'    ''' <param name="Path"></param>
'    ''' <returns></returns>
'    ''' <remarks></remarks>
'    Public Shared Function CreateNew(Path As String) As Excel
'        Dim sharedStringTablePart As Global.DocumentFormat.OpenXml.Packaging.SharedStringTablePart
'        Dim workbookStylesPart As Global.DocumentFormat.OpenXml.Packaging.WorkbookStylesPart

'        ' Create the Excel workbook
'        Dim spreadSheet As Global.DocumentFormat.OpenXml.Packaging.SpreadsheetDocument =
'            Global.DocumentFormat.OpenXml.Packaging.SpreadsheetDocument.Create(Path, Global.DocumentFormat.OpenXml.SpreadsheetDocumentType.Workbook, False)

'        ' Create the parts and the corresponding objects

'        ' Workbook
'        spreadSheet.AddWorkbookPart()
'        spreadSheet.WorkbookPart.Workbook = New Global.DocumentFormat.OpenXml.Spreadsheet.Workbook()
'        spreadSheet.WorkbookPart.Workbook.Save()

'        ' Shared string table
'        sharedStringTablePart = spreadSheet.WorkbookPart.AddNewPart(Of Global.DocumentFormat.OpenXml.Packaging.SharedStringTablePart)()
'        sharedStringTablePart.SharedStringTable = New Global.DocumentFormat.OpenXml.Spreadsheet.SharedStringTable()
'        sharedStringTablePart.SharedStringTable.Save()

'        ' Sheets collection
'        spreadSheet.WorkbookPart.Workbook.Sheets = New Global.DocumentFormat.OpenXml.Spreadsheet.Sheets()
'        spreadSheet.WorkbookPart.Workbook.Save()

'        ' Stylesheet
'        workbookStylesPart = spreadSheet.WorkbookPart.AddNewPart(Of Global.DocumentFormat.OpenXml.Packaging.WorkbookStylesPart)()
'        workbookStylesPart.Stylesheet = New Global.DocumentFormat.OpenXml.Spreadsheet.Stylesheet()
'        workbookStylesPart.Stylesheet.Save()

'        Return New Excel With {.XlsxDocument = spreadSheet, ._Path = Path}
'    End Function
'End Class
