#Region "Microsoft.VisualBasic::7f1c0ee6dd529bac07171e32cf03dd14, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\FileIO\LowLevel.vb"

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


    ' Code Statistics:

    '   Total Lines: 1601
    '    Code Lines: 1188 (74.20%)
    ' Comment Lines: 316 (19.74%)
    '    - Xml Docs: 93.35%
    ' 
    '   Blank Lines: 97 (6.06%)
    '     File Size: 86.03 KB


    '     Class LowLevel
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CalculatePaneHeight, CalculatePaneWidth, CreateAppPropertiesDocument, CreateAppString, CreateColsString
    '                   CreateCorePropertiesDocument, CreateCorePropertiesString, CreateMergedCellsString, CreateMruColorsString, CreateRowString
    '                   CreateSharedStringsDocument, CreateSheetProtectionString, CreateStyleBorderString, CreateStyleFillString, CreateStyleFontString
    '                   CreateStyleNumberFormatString, CreateStyleSheetDocument, CreateStyleXfsString, CreateWorkbookDocument, CreateWorksheetPart
    '                   EscapeXmlAttributeChars, EscapeXmlChars, GeneratePasswordHash, GetInternalColumnWidth, GetInternalPaneSplitHeight
    '                   GetInternalPaneSplitWidth, GetInternalRowHeight, GetOADateTimeString, GetOATimeString, GetSortedSheetData
    '                   HasPaneSplitting, NormalizeNewLines
    ' 
    '         Sub: AppendSharedString, AppendXmlTag, AppendXmlToPackagePart, CreatePaneString, CreateRowsString
    '              CreateSheetViewString, CreateWorkbookProtectionString, Save, SaveAsStream, SaveAsStreamInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' 
'  PicoXLSX is a small .NET library to generate XLSX (Microsoft Excel 2007 or newer) files in an easy and native way
'  Copyright Raphael Stoeckli © 2023
'  This library is licensed under the MIT License.
'  You find a copy of the license in project folder or on: http://opensource.org/licenses/MIT
' 

Imports System.Globalization
Imports System.IO
Imports System.IO.Packaging
Imports System.Text
Imports System.Xml
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Writer
Imports stdNum = System.Math

Namespace XLSX.FileIO

    ''' <summary>
    ''' Class for low level handling (XML, formatting, packing)
    ''' </summary>
    Friend Class LowLevel

        ''' <summary>
        ''' Defines the WORKBOOK
        ''' </summary>
        Private Shared WORKBOOK As DocumentPath = New DocumentPath("workbook.xml", "xl/")

        ''' <summary>
        ''' Defines the STYLES
        ''' </summary>
        Private Shared STYLES As DocumentPath = New DocumentPath("styles.xml", "xl/")

        ''' <summary>
        ''' Defines the APP_PROPERTIES
        ''' </summary>
        Private Shared APP_PROPERTIES As DocumentPath = New DocumentPath("app.xml", "docProps/")

        ''' <summary>
        ''' Defines the CORE_PROPERTIES
        ''' </summary>
        Private Shared CORE_PROPERTIES As DocumentPath = New DocumentPath("core.xml", "docProps/")

        ''' <summary>
        ''' Defines the SHARED_STRINGS
        ''' </summary>
        Private Shared SHARED_STRINGS As DocumentPath = New DocumentPath("sharedStrings.xml", "xl/")

        ''' <summary>
        ''' Minimum valid OAdate value (1900-01-01) However, Excel displays this value as 1900-01-00 (day zero)
        ''' </summary>
        Public Const MIN_OADATE_VALUE As Double = 0F

        ''' <summary>
        ''' Maximum valid OAdate value (9999-12-31)
        ''' </summary>
        Public Const MAX_OADATE_VALUE As Double = 2958465.999988426R

        ''' <summary>
        ''' First date that can be displayed by Excel. Real values before this date cannot be processed.
        ''' </summary>
        Public Shared ReadOnly FIRST_ALLOWED_EXCEL_DATE As Date = New DateTime(1900, 1, 1, 0, 0, 0)

        ''' <summary>
        ''' All dates before this date are shifted in Excel by -1.0, since Excel assumes wrongly that the year 1900 is a leap year.<br/>
        ''' See also: <a href="https://docs.microsoft.com/en-us/office/troubleshoot/excel/wrongly-assumes-1900-is-leap-year">
        ''' https://docs.microsoft.com/en-us/office/troubleshoot/excel/wrongly-assumes-1900-is-leap-year</a>
        ''' </summary>
        Public Shared ReadOnly FIRST_VALID_EXCEL_DATE As Date = New DateTime(1900, 3, 1)

        ''' <summary>
        ''' Last date that can be displayed by Excel. Real values after this date cannot be processed.
        ''' </summary>
        Public Shared ReadOnly LAST_ALLOWED_EXCEL_DATE As Date = New DateTime(9999, 12, 31, 23, 59, 59)

        ''' <summary>
        ''' Constant for number conversion. The invariant culture (represents mostly the US numbering scheme) ensures that no culture-specific 
        ''' punctuations are used when converting numbers to strings, This is especially important for OOXML number values
        ''' See also: <a href="https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.invariantculture?view=net-5.0">
        ''' https://docs.microsoft.com/en-us/dotnet/api/system.globalization.cultureinfo.invariantculture?view=net-5.0</a>
        ''' </summary>
        Public Shared ReadOnly INVARIANT_CULTURE As CultureInfo = CultureInfo.InvariantCulture

        ''' <summary>
        ''' Defines the COLUMN_WIDTH_ROUNDING_MODIFIER
        ''' </summary>
        Private Const COLUMN_WIDTH_ROUNDING_MODIFIER As Single = 256.0F

        ''' <summary>
        ''' Defines the SPLIT_WIDTH_MULTIPLIER
        ''' </summary>
        Private Const SPLIT_WIDTH_MULTIPLIER As Single = 12.0F

        ''' <summary>
        ''' Defines the SPLIT_WIDTH_OFFSET
        ''' </summary>
        Private Const SPLIT_WIDTH_OFFSET As Single = 0.5F

        ''' <summary>
        ''' Defines the SPLIT_WIDTH_POINT_MULTIPLIER
        ''' </summary>
        Private Const SPLIT_WIDTH_POINT_MULTIPLIER As Single = 3.0F / 4.0F

        ''' <summary>
        ''' Defines the SPLIT_POINT_DIVIDER
        ''' </summary>
        Private Const SPLIT_POINT_DIVIDER As Single = 20.0F

        ''' <summary>
        ''' Defines the SPLIT_WIDTH_POINT_OFFSET
        ''' </summary>
        Private Const SPLIT_WIDTH_POINT_OFFSET As Single = 390.0F

        ''' <summary>
        ''' Defines the SPLIT_HEIGHT_POINT_OFFSET
        ''' </summary>
        Private Const SPLIT_HEIGHT_POINT_OFFSET As Single = 300.0F

        ''' <summary>
        ''' Defines the ROW_HEIGHT_POINT_MULTIPLIER
        ''' </summary>
        Private Const ROW_HEIGHT_POINT_MULTIPLIER As Single = 1.0F / 3.0F + 1.0F

        ''' <summary>
        ''' Defines the ROOT_MILLIS
        ''' </summary>
        Private Shared ReadOnly ROOT_MILLIS As Double = CDbl(New DateTime(1899, 12, 30, 0, 0, 0).Ticks) / TimeSpan.TicksPerMillisecond

        ''' <summary>
        ''' Defines the culture
        ''' </summary>
        Private ReadOnly culture As CultureInfo

        ''' <summary>
        ''' Defines the workbook
        ''' </summary>
        Private ReadOnly m_workbook As Workbook

        ''' <summary>
        ''' Defines the styles
        ''' </summary>
        Private m_styles As StyleManager

        ''' <summary>
        ''' Defines the sharedStrings
        ''' </summary>
        Private ReadOnly sharedStrings As SortedMap

        ''' <summary>
        ''' Defines the sharedStringsTotalCount
        ''' </summary>
        Private sharedStringsTotalCount As Integer

        ''' <summary>
        ''' Initializes a new instance of the <see cref="LowLevel"/> class
        ''' </summary>
        ''' <param name="workbook">Workbook to process.</param>
        Public Sub New(workbook As Workbook)
            culture = INVARIANT_CULTURE
            m_workbook = workbook
            sharedStrings = New SortedMap()
            sharedStringsTotalCount = 0
        End Sub

        ''' <summary>
        ''' Method to create the app-properties (part of meta data) as raw XML string
        ''' </summary>
        ''' <returns>Raw XML string.</returns>
        Private Function CreateAppPropertiesDocument() As String
            Dim sb As New StringBuilder()
            sb.Append("<Properties xmlns=""http://schemas.openxmlformats.org/officeDocument/2006/extended-properties"" xmlns:vt=""http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes"">")
            sb.Append(CreateAppString())
            sb.Append("</Properties>")
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Method to create the core-properties (part of meta data) as raw XML string
        ''' </summary>
        ''' <returns>Raw XML string.</returns>
        Private Function CreateCorePropertiesDocument() As String
            Dim sb As New StringBuilder()
            sb.Append("<cp:coreProperties xmlns:cp=""http://schemas.openxmlformats.org/package/2006/metadata/core-properties"" xmlns:dc=""http://purl.org/dc/elements/1.1/"" xmlns:dcterms=""http://purl.org/dc/terms/"" xmlns:dcmitype=""http://purl.org/dc/dcmitype/"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">")
            sb.Append(CreateCorePropertiesString())
            sb.Append("</cp:coreProperties>")
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Method to create shared strings as raw XML string
        ''' </summary>
        ''' <returns>Raw XML string.</returns>
        Private Function CreateSharedStringsDocument() As String
            Dim sb As New StringBuilder()
            sb.Append("<sst xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" count=""")
            sb.Append(sharedStringsTotalCount.ToString("G", culture))
            sb.Append(""" uniqueCount=""")
            sb.Append(sharedStrings.Count.ToString("G", culture))
            sb.Append(""">")
            For Each str As String In sharedStrings.Keys
                AppendSharedString(sb, EscapeXmlChars(str))
            Next
            sb.Append("</sst>")
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Method to append shared string values and to handle leading or trailing white spaces
        ''' </summary>
        ''' <param name="sb">StringBuilder instance.</param>
        ''' <param name="value">Escaped string value (not null).</param>
        Private Sub AppendSharedString(sb As StringBuilder, value As String)
            Dim len = value.Length
            sb.Append("<si>")
            If len = 0 Then
                sb.Append("<t></t>")
            Else
                If Char.IsWhiteSpace(value, 0) OrElse Char.IsWhiteSpace(value, len - 1) Then
                    sb.Append("<t xml:space=""preserve"">")
                Else
                    sb.Append("<t>")
                End If
                sb.Append(NormalizeNewLines(value)).Append("</t>")
            End If
            sb.Append("</si>")
        End Sub

        ''' <summary>
        ''' Method to normalize all newlines to CR+LF
        ''' </summary>
        ''' <param name="value">Input value.</param>
        ''' <returns>Normalized value.</returns>
        Private Function NormalizeNewLines(value As String) As String
            If value Is Nothing OrElse Not value.Contains(ChrW(10)) AndAlso Not value.Contains(ChrW(13)) Then
                Return value
            End If
            Dim normalized = value.Replace(CStr(vbCrLf), CStr(vbLf)).Replace(vbCr, vbLf)
            Return normalized.Replace(vbLf, vbCrLf)
        End Function

        ''' <summary>
        ''' Method to create a style sheet as raw XML string
        ''' </summary>
        ''' <returns>Raw XML string.</returns>
        Private Function CreateStyleSheetDocument() As String
            Dim bordersString As String = CreateStyleBorderString()
            Dim fillsString As String = CreateStyleFillString()
            Dim fontsString As String = CreateStyleFontString()
            Dim numberFormatsString As String = CreateStyleNumberFormatString()
            Dim xfsStings As String = CreateStyleXfsString()
            Dim mruColorString As String = CreateMruColorsString()
            Dim fontCount As Integer = m_styles.GetFontStyleNumber()
            Dim fillCount As Integer = m_styles.GetFillStyleNumber()
            Dim styleCount As Integer = m_styles.GetStyleNumber()
            Dim borderCount As Integer = m_styles.GetBorderStyleNumber()
            Dim sb As StringBuilder = New StringBuilder()
            sb.Append("<styleSheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006"" mc:Ignorable=""x14ac"" xmlns:x14ac=""http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac"">")
            Dim numFormatCount As Integer = m_styles.GetNumberFormatStyleNumber()
            If numFormatCount > 0 Then
                sb.Append("<numFmts count=""").Append(numFormatCount.ToString("G", culture)).Append(""">")
                sb.Append(numberFormatsString & "</numFmts>")
            End If
            sb.Append("<fonts x14ac:knownFonts=""1"" count=""").Append(fontCount.ToString("G", culture)).Append(""">")
            sb.Append(fontsString).Append("</fonts>")
            sb.Append("<fills count=""").Append(fillCount.ToString("G", culture)).Append(""">")
            sb.Append(fillsString).Append("</fills>")
            sb.Append("<borders count=""").Append(borderCount.ToString("G", culture)).Append(""">")
            sb.Append(bordersString).Append("</borders>")
            sb.Append("<cellXfs count=""").Append(styleCount.ToString("G", culture)).Append(""">")
            sb.Append(xfsStings).Append("</cellXfs>")
            If Not String.IsNullOrEmpty(mruColorString) Then
                sb.Append("<colors>")
                sb.Append(mruColorString)
                sb.Append("</colors>")
            End If
            sb.Append("</styleSheet>")
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Method to create a workbook as raw XML string
        ''' </summary>
        ''' <returns>Raw XML string.</returns>
        Private Function CreateWorkbookDocument() As String
            If m_workbook.Worksheets.Count = 0 Then
                Throw New RangeException("OutOfRangeException", "The workbook can not be created because no worksheet was defined.")
            End If
            Dim sb As StringBuilder = New StringBuilder()
            sb.Append("<workbook xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" xmlns:r=""http://schemas.openxmlformats.org/officeDocument/2006/relationships"">")
            If m_workbook.SelectedWorksheet > 0 OrElse m_workbook.Hidden Then
                sb.Append("<bookViews><workbookView ")
                If m_workbook.Hidden Then
                    sb.Append("visibility=""hidden""")
                Else
                    sb.Append("activeTab=""").Append(m_workbook.SelectedWorksheet.ToString("G", culture)).Append("""")
                End If
                sb.Append("/></bookViews>")
            End If
            CreateWorkbookProtectionString(sb)
            sb.Append("<sheets>")
            If m_workbook.Worksheets.Count > 0 Then
                For Each item In m_workbook.Worksheets
                    sb.Append("<sheet r:id=""rId").Append(item.SheetID.ToString()).Append(""" sheetId=""").Append(item.SheetID.ToString()).Append(""" name=""").Append(EscapeXmlAttributeChars(item.SheetName)).Append("""")
                    If item.Hidden Then
                        sb.Append(" state=""hidden""")
                    End If
                    sb.Append("/>")
                Next
            Else
                ' Fallback on empty workbook
                sb.Append("<sheet r:id=""rId1"" sheetId=""1"" name=""sheet1""/>")
            End If
            sb.Append("</sheets>")
            sb.Append("</workbook>")
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Method to create the (sub) part of the workbook protection within the workbook XML document
        ''' </summary>
        ''' <param name="sb">reference to the stringbuilder.</param>
        Private Sub CreateWorkbookProtectionString(sb As StringBuilder)
            If m_workbook.UseWorkbookProtection Then
                sb.Append("<workbookProtection")
                If m_workbook.LockWindowsIfProtected Then
                    sb.Append(" lockWindows=""1""")
                End If
                If m_workbook.LockStructureIfProtected Then
                    sb.Append(" lockStructure=""1""")
                End If
                If Not String.IsNullOrEmpty(m_workbook.WorkbookProtectionPassword) Then
                    sb.Append(" workbookPassword=""")
                    sb.Append(m_workbook.WorkbookProtectionPasswordHash)
                    sb.Append("""")
                End If
                sb.Append("/>")
            End If
        End Sub

        ''' <summary>
        ''' Method to create a worksheet part as a raw XML string
        ''' </summary>
        ''' <param name="worksheet">worksheet object to process.</param>
        ''' <returns>Raw XML string.</returns>
        Private Function CreateWorksheetPart(worksheet As Worksheet) As String
            worksheet.RecalculateAutoFilter()
            worksheet.RecalculateColumns()
            Dim sb As StringBuilder = New StringBuilder()
            sb.Append("<worksheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"" xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006"" mc:Ignorable=""x14ac"" xmlns:x14ac=""http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac"">")
            If worksheet.GetLastCellAddress().HasValue AndAlso worksheet.GetFirstCellAddress().HasValue Then
                sb.Append("<dimension ref=""").Append(New Range(worksheet.GetFirstCellAddress().Value, worksheet.GetLastCellAddress().Value)).Append("""/>")
            End If
            If worksheet.SelectedCellRanges.Count > 0 OrElse worksheet.PaneSplitTopHeight IsNot Nothing OrElse worksheet.PaneSplitLeftWidth IsNot Nothing OrElse worksheet.PaneSplitAddress IsNot Nothing OrElse worksheet.Hidden Then
                CreateSheetViewString(worksheet, sb)
            End If
            sb.Append("<sheetFormatPr")
            If Not HasPaneSplitting(worksheet) Then
                ' TODO: Find the right calculation to compensate
                ' baseColWidth when using pane splitting
                sb.Append(" defaultColWidth=""").Append(worksheet.DefaultColumnWidth.ToString("G", culture)).Append("""")
            End If
            sb.Append(" defaultRowHeight=""").Append(worksheet.DefaultRowHeight.ToString("G", culture)).Append(""" baseColWidth=""").Append(worksheet.DefaultColumnWidth.ToString("G", culture)).Append(""" x14ac:dyDescent=""0.25""/>")
            Dim colWidths = CreateColsString(worksheet)
            If Not String.IsNullOrEmpty(colWidths) Then
                sb.Append("<cols>")
                sb.Append(colWidths)
                sb.Append("</cols>")
            End If
            sb.Append("<sheetData>")
            CreateRowsString(worksheet, sb)
            sb.Append("</sheetData>")
            sb.Append(CreateMergedCellsString(worksheet))
            sb.Append(CreateSheetProtectionString(worksheet))

            If worksheet.AutoFilterRange IsNot Nothing Then
                sb.Append("<autoFilter ref=""").Append(worksheet.AutoFilterRange.Value.ToString()).Append("""/>")
            End If

            sb.Append("</worksheet>")
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Checks whether pane splitting is applied in the given worksheet
        ''' </summary>
        ''' <param name="worksheet">.</param>
        ''' <returns>True if applied, otherwise false.</returns>
        Private Function HasPaneSplitting(worksheet As Worksheet) As Boolean
            If worksheet.PaneSplitLeftWidth Is Nothing AndAlso worksheet.PaneSplitTopHeight Is Nothing AndAlso worksheet.PaneSplitAddress Is Nothing Then
                Return False
            End If
            Return True
        End Function

        ''' <summary>
        ''' Method to create the enclosing part of the rows
        ''' </summary>
        ''' <param name="worksheet">worksheet object to process.</param>
        ''' <param name="sb">reference to the stringbuilder.</param>
        Private Sub CreateRowsString(worksheet As Worksheet, sb As StringBuilder)
            Dim cellData = GetSortedSheetData(worksheet)
            Dim line As String
            For Each row In cellData
                line = CreateRowString(row, worksheet)
                sb.Append(line)
            Next
        End Sub

        ''' <summary>
        ''' Method to create the (sub) part of the worksheet view (selected cells and panes) within the worksheet XML document
        ''' </summary>
        ''' <param name="worksheet">worksheet object to process.</param>
        ''' <param name="sb">reference to the stringbuilder.</param>
        Private Sub CreateSheetViewString(worksheet As Worksheet, sb As StringBuilder)
            sb.Append("<sheetViews><sheetView workbookViewId=""0""")
            If m_workbook.SelectedWorksheet = worksheet.SheetID - 1 AndAlso Not worksheet.Hidden Then
                sb.Append(" tabSelected=""1""")
            End If
            sb.Append(">")
            CreatePaneString(worksheet, sb)
            If worksheet.SelectedCells IsNot Nothing Then
                sb.Append("<selection sqref=""")
                For i = 0 To worksheet.SelectedCellRanges.Count - 1
                    sb.Append(worksheet.SelectedCellRanges(i).ToString())
                    If i < worksheet.SelectedCellRanges.Count - 1 Then
                        sb.Append(" ")
                    End If
                Next
                sb.Append(""" activeCell=""")
                sb.Append(worksheet.SelectedCellRanges(0).StartAddress.ToString())
                sb.Append("""/>")
            End If
            sb.Append("</sheetView></sheetViews>")
        End Sub

        ''' <summary>
        ''' Method to create the (sub) part of the pane (splitting and freezing) within the worksheet XML document
        ''' </summary>
        ''' <param name="worksheet">worksheet object to process.</param>
        ''' <param name="sb">reference to the stringbuilder.</param>
        Private Sub CreatePaneString(worksheet As Worksheet, sb As StringBuilder)
            If Not HasPaneSplitting(worksheet) Then
                Return
            End If
            sb.Append("<pane")
            Dim applyXSplit = False
            Dim applyYSplit = False
            If worksheet.PaneSplitAddress IsNot Nothing Then
                Dim freeze = worksheet.FreezeSplitPanes IsNot Nothing AndAlso worksheet.FreezeSplitPanes.Value
                Dim xSplit = worksheet.PaneSplitAddress.Value.Column
                Dim ySplit = worksheet.PaneSplitAddress.Value.Row
                If xSplit > 0 Then
                    If freeze Then
                        sb.Append(" xSplit=""").Append(xSplit.ToString("G", culture)).Append("""")
                    Else
                        sb.Append(" xSplit=""").Append(CalculatePaneWidth(worksheet, xSplit).ToString("G", culture)).Append("""")
                    End If
                    applyXSplit = True
                End If
                If ySplit > 0 Then
                    If freeze Then
                        sb.Append(" ySplit=""").Append(ySplit.ToString("G", culture)).Append("""")
                    Else
                        sb.Append(" ySplit=""").Append(CalculatePaneHeight(worksheet, ySplit).ToString("G", culture)).Append("""")
                    End If
                    applyYSplit = True
                End If
                If freeze AndAlso applyXSplit AndAlso applyYSplit Then
                    sb.Append(" state=""frozenSplit""")
                ElseIf freeze Then
                    sb.Append(" state=""frozen""")
                End If
            Else
                If worksheet.PaneSplitLeftWidth IsNot Nothing Then
                    sb.Append(" xSplit=""").Append(GetInternalPaneSplitWidth(worksheet.PaneSplitLeftWidth.Value).ToString("G", culture)).Append("""")
                    applyXSplit = True
                End If
                If worksheet.PaneSplitTopHeight IsNot Nothing Then
                    sb.Append(" ySplit=""").Append(GetInternalPaneSplitHeight(worksheet.PaneSplitTopHeight.Value).ToString("G", culture)).Append("""")
                    applyYSplit = True
                End If
            End If
            If applyXSplit AndAlso applyYSplit Then
                Select Case worksheet.ActivePane.Value
                    Case WorksheetPane.bottomLeft
                        sb.Append(" activePane=""bottomLeft""")
                    Case WorksheetPane.bottomRight
                        sb.Append(" activePane=""bottomRight""")
                    Case WorksheetPane.topLeft
                        sb.Append(" activePane=""topLeft""")
                    Case WorksheetPane.topRight
                        sb.Append(" activePane=""topRight""")
                End Select
            End If
            Dim topLeftCell As String = worksheet.PaneSplitTopLeftCell.Value.GetAddress()
            sb.Append(" topLeftCell=""").Append(topLeftCell).Append(""" ")
            sb.Append("/>")
            If applyXSplit AndAlso Not applyYSplit Then
                sb.Append("<selection pane=""topRight"" activeCell=""" & topLeftCell & """  sqref=""" & topLeftCell & """ />")
            ElseIf applyYSplit AndAlso Not applyXSplit Then
                sb.Append("<selection pane=""bottomLeft"" activeCell=""" & topLeftCell & """  sqref=""" & topLeftCell & """ />")
            ElseIf applyYSplit AndAlso applyXSplit Then
                sb.Append("<selection activeCell=""" & topLeftCell & """  sqref=""" & topLeftCell & """ />")
            End If
        End Sub

        ''' <summary>
        ''' Method to calculate the pane height, based on the number of rows
        ''' </summary>
        ''' <param name="worksheet">worksheet object to get the row definitions from.</param>
        ''' <param name="numberOfRows">Number of rows from the top to the split position.</param>
        ''' <returns>Internal height from the top of the worksheet to the pane split position.</returns>
        Private Function CalculatePaneHeight(worksheet As Worksheet, numberOfRows As Integer) As Single
            Dim height As Single = 0
            For i = 0 To numberOfRows - 1
                If worksheet.RowHeights.ContainsKey(i) Then
                    height += GetInternalRowHeight(worksheet.RowHeights(i))
                Else
                    height += GetInternalRowHeight(Worksheet.DEFAULT_ROW_HEIGHT)
                End If
            Next
            Return GetInternalPaneSplitHeight(height)
        End Function

        ''' <summary>
        ''' Method to calculate the pane width, based on the number of columns
        ''' </summary>
        ''' <param name="worksheet">worksheet object to get the column definitions from.</param>
        ''' <param name="numberOfColumns">Number of columns from the left to the split position.</param>
        ''' <returns>Internal width from the left of the worksheet to the pane split position.</returns>
        Private Function CalculatePaneWidth(worksheet As Worksheet, numberOfColumns As Integer) As Single
            Dim width As Single = 0
            For i = 0 To numberOfColumns - 1
                If worksheet.Columns.ContainsKey(i) Then
                    width += GetInternalColumnWidth(worksheet.Columns(i).Width)
                Else
                    width += GetInternalColumnWidth(Worksheet.DEFAULT_COLUMN_WIDTH)
                End If
            Next
            ' Add padding of 75 per column
            Return GetInternalPaneSplitWidth(width) + (numberOfColumns - 1) * 0F
        End Function

        ''' <summary>
        ''' Method to save the workbook
        ''' </summary>
        Public Sub Save()
            Using fs As FileStream = New FileStream(m_workbook.Filename, FileMode.Create)
                Call SaveAsStream(fs)
            End Using
        End Sub

        ''' <summary>
        ''' Method to save the workbook asynchronous
        ''' </summary>
        ''' <returns>Async Task.</returns>
        Public Async Function SaveAsync() As Task
            Await Task.Run(Sub() Save())
        End Function

        ''' <summary>
        ''' Method to save the workbook as stream
        ''' </summary>
        ''' <param name="stream">Writable stream as target.</param>
        ''' <param name="leaveOpen">Optional parameter to keep the stream open after writing (used for MemoryStreams; default is false).</param>
        Public Sub SaveAsStream(stream As Stream, Optional leaveOpen As Boolean = False)
            m_workbook.ResolveMergedCells()
            m_styles = StyleManager.GetManagedStyles(m_workbook) ' After this point, styles must not be changed anymore

            Using p = Package.Open(stream, FileMode.Create)
                Call SaveAsStreamInternal(p)

                p.Flush()
                p.Close()

                If Not leaveOpen Then
                    stream.Close()
                End If
            End Using
        End Sub

        Private Sub SaveAsStreamInternal(p As Package)
            Dim workbookUri As Uri = New Uri(WORKBOOK.GetFullPath(), UriKind.Relative)
            Dim stylesheetUri As Uri = New Uri(STYLES.GetFullPath(), UriKind.Relative)
            Dim appPropertiesUri As Uri = New Uri(APP_PROPERTIES.GetFullPath(), UriKind.Relative)
            Dim corePropertiesUri As Uri = New Uri(CORE_PROPERTIES.GetFullPath(), UriKind.Relative)
            Dim sharedStringsUri As Uri = New Uri(SHARED_STRINGS.GetFullPath(), UriKind.Relative)
            Dim sheetPath As DocumentPath
            Dim sheetURIs As New List(Of Uri)()
            Dim pp = p.CreatePart(workbookUri, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml", CompressionOption.Normal)
            p.CreateRelationship(pp.Uri, TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument", "rId1")
            p.CreateRelationship(corePropertiesUri, TargetMode.Internal, "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties", "rId2") '!
            p.CreateRelationship(appPropertiesUri, TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties", "rId3") '!

            AppendXmlToPackagePart(CreateWorkbookDocument(), pp)
            Dim idCounter As Integer
            If m_workbook.Worksheets.Count > 0 Then
                idCounter = m_workbook.Worksheets.Count + 1
            Else
                '  Fallback on empty workbook
                idCounter = 2
            End If
            pp.CreateRelationship(stylesheetUri, TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles", "rId" & idCounter.ToString())
            pp.CreateRelationship(sharedStringsUri, TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings", "rId" & (idCounter + 1).ToString())

            If m_workbook.Worksheets.Count > 0 Then
                For Each item As Worksheet In m_workbook.Worksheets
                    sheetPath = New DocumentPath("sheet" & item.SheetID.ToString() & ".xml", "xl/worksheets")
                    sheetURIs.Add(New Uri(sheetPath.GetFullPath(), UriKind.Relative))
                    pp.CreateRelationship(sheetURIs(sheetURIs.Count - 1), TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet", "rId" & item.SheetID.ToString())
                Next
            Else
                '  Fallback on empty workbook
                sheetPath = New DocumentPath("sheet1.xml", "xl/worksheets")
                sheetURIs.Add(New Uri(sheetPath.GetFullPath(), UriKind.Relative))
                pp.CreateRelationship(sheetURIs(sheetURIs.Count - 1), TargetMode.Internal, "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet", "rId1")
            End If

            pp = p.CreatePart(stylesheetUri, "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml", CompressionOption.Normal)
            AppendXmlToPackagePart(CreateStyleSheetDocument(), pp)

            Dim i = 0
            If m_workbook.Worksheets.Count > 0 Then
                For Each item In m_workbook.Worksheets
                    pp = p.CreatePart(sheetURIs(i), "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml", CompressionOption.Normal)
                    i += 1
                    AppendXmlToPackagePart(CreateWorksheetPart(item), pp)
                Next
            Else
                pp = p.CreatePart(sheetURIs(i), "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml", CompressionOption.Normal)
                i += 1
                AppendXmlToPackagePart(CreateWorksheetPart(New Worksheet("sheet1")), pp)
            End If
            pp = p.CreatePart(sharedStringsUri, "application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml", CompressionOption.Normal)
            AppendXmlToPackagePart(CreateSharedStringsDocument(), pp)

            If m_workbook.WorkbookMetadata IsNot Nothing Then
                pp = p.CreatePart(appPropertiesUri, "application/vnd.openxmlformats-officedocument.extended-properties+xml", CompressionOption.Normal)
                AppendXmlToPackagePart(CreateAppPropertiesDocument(), pp)
                pp = p.CreatePart(corePropertiesUri, "application/vnd.openxmlformats-package.core-properties+xml", CompressionOption.Normal)
                AppendXmlToPackagePart(CreateCorePropertiesDocument(), pp)
            End If
        End Sub

        ''' <summary>
        ''' Method to save the workbook as stream asynchronous
        ''' </summary>
        ''' <param name="stream">Writable stream as target.</param>
        ''' <param name="leaveOpen">Optional parameter to keep the stream open after writing (used for MemoryStreams; default is false).</param>
        ''' <returns>Async Task.</returns>
        Public Async Function SaveAsStreamAsync(stream As Stream, Optional leaveOpen As Boolean = False) As Task
            Await Task.Run(Sub() SaveAsStream(stream, leaveOpen))
        End Function

        ''' <summary>
        ''' Method to append a simple XML tag with an enclosed value to the passed StringBuilder
        ''' </summary>
        ''' <param name="sb">StringBuilder to append.</param>
        ''' <param name="value">Value of the XML element.</param>
        ''' <param name="tagName">Tag name of the XML element.</param>
        ''' <param name="nameSpace">Optional XML name space. Can be empty or null.</param>
        Private Sub AppendXmlTag(sb As StringBuilder, value As String, tagName As String, [nameSpace] As String)
            If String.IsNullOrEmpty(value) Then
                Return
            End If
            If sb Is Nothing OrElse String.IsNullOrEmpty(tagName) Then
                Return
            End If
            Dim hasNoNs = String.IsNullOrEmpty([nameSpace])
            sb.Append("<"c)
            If Not hasNoNs Then
                sb.Append([nameSpace])
                sb.Append(":"c)
            End If
            sb.Append(tagName).Append(">")
            sb.Append(EscapeXmlChars(value))
            sb.Append("</")
            If Not hasNoNs Then
                sb.Append([nameSpace])
                sb.Append(":"c)
            End If
            sb.Append(tagName)
            sb.Append(">"c)
        End Sub

        ''' <summary>
        ''' Writes raw XML strings into the passed Package Part
        ''' </summary>
        ''' <param name="doc">document as raw XML string.</param>
        ''' <param name="pp">Package part to append the XML data.</param>
        Private Sub AppendXmlToPackagePart(doc As String, pp As PackagePart)
            Using ms As MemoryStream = New MemoryStream() ' Write workbook.xml
                If Not ms.CanWrite Then
                    Return
                End If
                Using writer = XmlWriter.Create(ms)
                    writer.WriteProcessingInstruction("xml", "version=""1.0"" encoding=""UTF-8"" standalone=""yes""")
                    writer.WriteRaw(doc)
                    writer.Flush()
                    ms.Position = 0
                    ms.CopyTo(pp.GetStream())
                    ms.Flush()
                End Using
            End Using
        End Sub

        ''' <summary>
        ''' Method to create the XML string for the app-properties document
        ''' </summary>
        ''' <returns>String with formatted XML data.</returns>
        Private Function CreateAppString() As String
            If m_workbook.WorkbookMetadata Is Nothing Then
                Return String.Empty
            End If
            Dim md = m_workbook.WorkbookMetadata
            Dim sb As StringBuilder = New StringBuilder()
            AppendXmlTag(sb, "0", "TotalTime", Nothing)
            AppendXmlTag(sb, md.Application, "Application", Nothing)
            AppendXmlTag(sb, "0", "DocSecurity", Nothing)
            AppendXmlTag(sb, "false", "ScaleCrop", Nothing)
            AppendXmlTag(sb, md.Manager, "Manager", Nothing)
            AppendXmlTag(sb, md.Company, "Company", Nothing)
            AppendXmlTag(sb, "false", "LinksUpToDate", Nothing)
            AppendXmlTag(sb, "false", "SharedDoc", Nothing)
            AppendXmlTag(sb, md.HyperlinkBase, "HyperlinkBase", Nothing)
            AppendXmlTag(sb, "false", "HyperlinksChanged", Nothing)
            AppendXmlTag(sb, md.ApplicationVersion, "AppVersion", Nothing)
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Method to create the columns as XML string. This is used to define the width of columns
        ''' </summary>
        ''' <param name="worksheet">Worksheet to process.</param>
        ''' <returns>String with formatted XML data.</returns>
        Private Function CreateColsString(worksheet As Worksheet) As String
            If worksheet.Columns.Count > 0 Then
                Dim col As String
                Dim hidden = ""
                Dim sb As StringBuilder = New StringBuilder()
                For Each column In worksheet.Columns
                    If column.Value.Width = worksheet.DefaultColumnWidth AndAlso Not column.Value.IsHidden Then
                        Continue For
                    End If
                    If worksheet.Columns.ContainsKey(column.Key) AndAlso worksheet.Columns(column.Key).IsHidden Then
                        hidden = " hidden=""1"""
                    End If
                    col = (column.Key + 1).ToString("G", culture) ' Add 1 for Address
                    Dim width = GetInternalColumnWidth(column.Value.Width)
                    sb.Append("<col customWidth=""1"" width=""").Append(width.ToString("G", culture)).Append(""" max=""").Append(col).Append(""" min=""").Append(col).Append("""").Append(hidden).Append("/>")
                Next
                Dim value As String = sb.ToString()
                If value.Length > 0 Then
                    Return value
                End If
                Return String.Empty
            End If
            Return String.Empty
        End Function

        ''' <summary>
        ''' Method to create the XML string for the core-properties document
        ''' </summary>
        ''' <returns>String with formatted XML data.</returns>
        Private Function CreateCorePropertiesString() As String
            If m_workbook.WorkbookMetadata Is Nothing Then
                Return String.Empty
            End If
            Dim md = m_workbook.WorkbookMetadata
            Dim sb As StringBuilder = New StringBuilder()
            AppendXmlTag(sb, md.Title, "title", "dc")
            AppendXmlTag(sb, md.Subject, "subject", "dc")
            AppendXmlTag(sb, md.Creator, "creator", "dc")
            AppendXmlTag(sb, md.Creator, "lastModifiedBy", "cp")
            AppendXmlTag(sb, md.Keywords, "keywords", "cp")
            AppendXmlTag(sb, md.Description, "description", "dc")
            Dim time = Date.Now.ToString("yyyy-MM-ddThh:mm:ssZ", culture)
            sb.Append("<dcterms:created xsi:type=""dcterms:W3CDTF"">").Append(time).Append("</dcterms:created>")
            sb.Append("<dcterms:modified xsi:type=""dcterms:W3CDTF"">").Append(time).Append("</dcterms:modified>")

            AppendXmlTag(sb, md.Category, "category", "cp")
            AppendXmlTag(sb, md.ContentStatus, "contentStatus", "cp")

            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Method to create the merged cells string of the passed worksheet
        ''' </summary>
        ''' <param name="sheet">Worksheet to process.</param>
        ''' <returns>Formatted string with merged cell ranges.</returns>
        Private Function CreateMergedCellsString(sheet As Worksheet) As String
            If sheet.MergedCells.Count < 1 Then
                Return String.Empty
            End If
            Dim sb As StringBuilder = New StringBuilder()
            sb.Append("<mergeCells count=""").Append(sheet.MergedCells.Count.ToString("G", culture)).Append(""">")
            For Each item In sheet.MergedCells
                sb.Append("<mergeCell ref=""").Append(item.Value.ToString()).Append("""/>")
            Next
            sb.Append("</mergeCells>")
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Method to create a row string
        ''' </summary>
        ''' <param name="dynamicRow">Dynamic row with List of cells, heights and hidden states.</param>
        ''' <param name="worksheet">Worksheet to process.</param>
        ''' <returns>Formatted row string.</returns>
        Private Function CreateRowString(dynamicRow As DynamicRow, worksheet As Worksheet) As String
            Dim rowNumber = dynamicRow.RowNumber
            Dim height = ""
            Dim hidden = ""
            If worksheet.RowHeights.ContainsKey(rowNumber) AndAlso worksheet.RowHeights(rowNumber) <> worksheet.DefaultRowHeight Then
                height = " x14ac:dyDescent=""0.25"" customHeight=""1"" ht=""" & GetInternalRowHeight(worksheet.RowHeights(rowNumber)).ToString("G", culture) & """"
            End If
            If worksheet.HiddenRows.ContainsKey(rowNumber) AndAlso worksheet.HiddenRows(rowNumber) Then
                hidden = " hidden=""1"""
            End If
            Dim sb As StringBuilder = New StringBuilder()
            sb.Append("<row r=""").Append((rowNumber + 1).ToString()).Append("""").Append(height).Append(hidden).Append(">")
            Dim typeAttribute As String
            Dim styleDef = ""
            Dim typeDef = ""
            Dim valueDef = ""
            Dim boolValue As Boolean

            Dim col = 0
            For Each item In dynamicRow.CellDefinitions
                typeDef = " "
                If item.CellStyle IsNot Nothing Then
                    styleDef = " s=""" & item.CellStyle.InternalID.Value.ToString("G", culture) & """ "
                Else
                    styleDef = ""
                End If
                item.ResolveCellType() ' Recalculate the type (for handling DEFAULT)
                If item.DataType = CellType.BOOL Then
                    typeAttribute = "b"
                    typeDef = " t=""" & typeAttribute & """ "
                    boolValue = CBool(item.Value)
                    If boolValue Then
                        valueDef = "1"
                    Else
                        valueDef = "0"
                    End If
                    ' Number casting
                ElseIf item.DataType = CellType.NUMBER Then
                    typeAttribute = "n"
                    typeDef = " t=""" & typeAttribute & """ "
                    Dim t As Type = item.Value.GetType()

                    If t Is GetType(Byte) Then
                        valueDef = CByte(item.Value).ToString("G", culture)
                    ElseIf t Is GetType(SByte) Then
                        valueDef = CSByte(item.Value).ToString("G", culture)
                    ElseIf t Is GetType(Decimal) Then
                        valueDef = CDec(item.Value).ToString("G", culture)
                    ElseIf t Is GetType(Double) Then
                        valueDef = CDbl(item.Value).ToString("G", culture)
                    ElseIf t Is GetType(Single) Then
                        valueDef = CSng(item.Value).ToString("G", culture)
                    ElseIf t Is GetType(Integer) Then
                        valueDef = CInt(item.Value).ToString("G", culture)
                    ElseIf t Is GetType(UInteger) Then
                        valueDef = CUInt(item.Value).ToString("G", culture)
                    ElseIf t Is GetType(Long) Then
                        valueDef = CLng(item.Value).ToString("G", culture)
                    ElseIf t Is GetType(ULong) Then
                        valueDef = CULng(item.Value).ToString("G", culture)
                    ElseIf t Is GetType(Short) Then
                        valueDef = CShort(item.Value).ToString("G", culture)
                    ElseIf t Is GetType(UShort) Then
                        valueDef = CUShort(item.Value).ToString("G", culture)
                    End If
                    ' Date parsing
                ElseIf item.DataType = CellType.DATE Then
                    typeAttribute = "d"
                    Dim [date] As Date = item.Value
                    valueDef = GetOADateTimeString([date])
                    ' Time parsing
                ElseIf item.DataType = CellType.TIME Then
                    typeAttribute = "d"
                    ' TODO: 'd' is probably an outdated attribute (to be checked for dates and times)
                    Dim time As TimeSpan = item.Value
                    valueDef = GetOATimeString(time)
                Else
                    If item.Value Is Nothing Then
                        typeAttribute = Nothing
                        valueDef = Nothing ' Handle sharedStrings
                    Else
                        If item.DataType = CellType.FORMULA Then
                            typeAttribute = "str"
                            valueDef = item.Value.ToString() ' Handle sharedStrings
                        Else
                            If item.DataType = CellType.FORMULA Then
                                typeAttribute = "str"
                                valueDef = item.Value.ToString()
                            Else
                                typeAttribute = "s"
                                valueDef = sharedStrings.Add(item.Value.ToString(), sharedStrings.Count.ToString("G", culture))
                                sharedStringsTotalCount += 1
                            End If
                        End If
                    End If
                    typeDef = " t=""" & typeAttribute & """ "
                End If
                If item.DataType <> CellType.EMPTY Then
                    sb.Append("<c r=""").Append(item.CellAddress).Append("""").Append(typeDef).Append(styleDef).Append(">")
                    If item.DataType = CellType.FORMULA Then
                        sb.Append("<f>").Append(EscapeXmlChars(item.Value.ToString())).Append("</f>")
                    Else
                        sb.Append("<v>").Append(EscapeXmlChars(valueDef)).Append("</v>")
                    End If
                    sb.Append("</c>")
                ElseIf Equals(valueDef, Nothing) OrElse item.DataType = CellType.EMPTY Then ' Empty cell
                    sb.Append("<c r=""").Append(item.CellAddress).Append("""").Append(styleDef).Append("/>") ' All other, unexpected cases
                Else
                    sb.Append("<c r=""").Append(item.CellAddress).Append("""").Append(typeDef).Append(styleDef).Append("/>")
                End If
                col += 1
            Next
            sb.Append("</row>")
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Method to create the protection string of the passed worksheet
        ''' </summary>
        ''' <param name="sheet">Worksheet to process.</param>
        ''' <returns>Formatted string with protection statement of the worksheet.</returns>
        Private Function CreateSheetProtectionString(sheet As Worksheet) As String
            If Not sheet.UseSheetProtection Then
                Return String.Empty
            End If
            Dim actualLockingValues As Dictionary(Of SheetProtectionValue, Integer) = New Dictionary(Of SheetProtectionValue, Integer)()
            If sheet.SheetProtectionValues.Count = 0 Then
                actualLockingValues.Add(SheetProtectionValue.selectLockedCells, 1)
                actualLockingValues.Add(SheetProtectionValue.selectUnlockedCells, 1)
            End If
            If Not sheet.SheetProtectionValues.Contains(SheetProtectionValue.objects) Then
                actualLockingValues.Add(SheetProtectionValue.objects, 1)
            End If
            If Not sheet.SheetProtectionValues.Contains(SheetProtectionValue.scenarios) Then
                actualLockingValues.Add(SheetProtectionValue.scenarios, 1)
            End If
            If Not sheet.SheetProtectionValues.Contains(SheetProtectionValue.selectLockedCells) AndAlso Not actualLockingValues.ContainsKey(SheetProtectionValue.selectLockedCells) Then
                actualLockingValues.Add(SheetProtectionValue.selectLockedCells, 1)
            End If
            If (Not sheet.SheetProtectionValues.Contains(SheetProtectionValue.selectUnlockedCells) OrElse Not sheet.SheetProtectionValues.Contains(SheetProtectionValue.selectLockedCells)) AndAlso Not actualLockingValues.ContainsKey(SheetProtectionValue.selectUnlockedCells) Then
                actualLockingValues.Add(SheetProtectionValue.selectUnlockedCells, 1)
            End If
            If sheet.SheetProtectionValues.Contains(SheetProtectionValue.formatCells) Then
                actualLockingValues.Add(SheetProtectionValue.formatCells, 0)
            End If
            If sheet.SheetProtectionValues.Contains(SheetProtectionValue.formatColumns) Then
                actualLockingValues.Add(SheetProtectionValue.formatColumns, 0)
            End If
            If sheet.SheetProtectionValues.Contains(SheetProtectionValue.formatRows) Then
                actualLockingValues.Add(SheetProtectionValue.formatRows, 0)
            End If
            If sheet.SheetProtectionValues.Contains(SheetProtectionValue.insertColumns) Then
                actualLockingValues.Add(SheetProtectionValue.insertColumns, 0)
            End If
            If sheet.SheetProtectionValues.Contains(SheetProtectionValue.insertRows) Then
                actualLockingValues.Add(SheetProtectionValue.insertRows, 0)
            End If
            If sheet.SheetProtectionValues.Contains(SheetProtectionValue.insertHyperlinks) Then
                actualLockingValues.Add(SheetProtectionValue.insertHyperlinks, 0)
            End If
            If sheet.SheetProtectionValues.Contains(SheetProtectionValue.deleteColumns) Then
                actualLockingValues.Add(SheetProtectionValue.deleteColumns, 0)
            End If
            If sheet.SheetProtectionValues.Contains(SheetProtectionValue.deleteRows) Then
                actualLockingValues.Add(SheetProtectionValue.deleteRows, 0)
            End If
            If sheet.SheetProtectionValues.Contains(SheetProtectionValue.sort) Then
                actualLockingValues.Add(SheetProtectionValue.sort, 0)
            End If
            If sheet.SheetProtectionValues.Contains(SheetProtectionValue.autoFilter) Then
                actualLockingValues.Add(SheetProtectionValue.autoFilter, 0)
            End If
            If sheet.SheetProtectionValues.Contains(SheetProtectionValue.pivotTables) Then
                actualLockingValues.Add(SheetProtectionValue.pivotTables, 0)
            End If
            Dim sb As StringBuilder = New StringBuilder()
            sb.Append("<sheetProtection")
            Dim temp As String
            For Each item In actualLockingValues
                Try
                    ' Note! If the enum names differs from the OOXML definitions,
                    ' this method will cause invalid OOXML entries
                    temp = [Enum].GetName(GetType(SheetProtectionValue), item.Key)
                    sb.Append(" ").Append(temp).Append("=""").Append(item.Value.ToString("G", culture)).Append("""")
                Catch
                    ' no-op
                End Try
            Next
            If Not String.IsNullOrEmpty(sheet.SheetProtectionPassword) Then
                Dim hash = GeneratePasswordHash(sheet.SheetProtectionPassword)
                sb.Append(" password=""").Append(hash).Append("""")
            End If
            sb.Append(" sheet=""1""/>")
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Method to create the XML string for the border part of the style sheet document
        ''' </summary>
        ''' <returns>String with formatted XML data.</returns>
        Private Function CreateStyleBorderString() As String
            Dim borderStyles As Style.Border() = m_styles.GetBorders()
            Dim sb As StringBuilder = New StringBuilder()
            For Each item In borderStyles
                If item.DiagonalDown AndAlso Not item.DiagonalUp Then
                    sb.Append("<border diagonalDown=""1"">")
                ElseIf Not item.DiagonalDown AndAlso item.DiagonalUp Then
                    sb.Append("<border diagonalUp=""1"">")
                ElseIf item.DiagonalDown AndAlso item.DiagonalUp Then
                    sb.Append("<border diagonalDown=""1"" diagonalUp=""1"">")
                Else
                    sb.Append("<border>")
                End If

                If item.LeftStyle <> Style.Border.StyleValue.none Then
                    sb.Append("<left style=""" & Style.Border.GetStyleName(item.LeftStyle) & """>")
                    If Not String.IsNullOrEmpty(item.LeftColor) Then
                        sb.Append("<color rgb=""").Append(item.LeftColor).Append("""/>")
                    Else
                        sb.Append("<color auto=""1""/>")
                    End If
                    sb.Append("</left>")
                Else
                    sb.Append("<left/>")
                End If
                If item.RightStyle <> Style.Border.StyleValue.none Then
                    sb.Append("<right style=""").Append(Style.Border.GetStyleName(item.RightStyle)).Append(""">")
                    If Not String.IsNullOrEmpty(item.RightColor) Then
                        sb.Append("<color rgb=""").Append(item.RightColor).Append("""/>")
                    Else
                        sb.Append("<color auto=""1""/>")
                    End If
                    sb.Append("</right>")
                Else
                    sb.Append("<right/>")
                End If
                If item.TopStyle <> Style.Border.StyleValue.none Then
                    sb.Append("<top style=""").Append(Style.Border.GetStyleName(item.TopStyle)).Append(""">")
                    If Not String.IsNullOrEmpty(item.TopColor) Then
                        sb.Append("<color rgb=""").Append(item.TopColor).Append("""/>")
                    Else
                        sb.Append("<color auto=""1""/>")
                    End If
                    sb.Append("</top>")
                Else
                    sb.Append("<top/>")
                End If
                If item.BottomStyle <> Style.Border.StyleValue.none Then
                    sb.Append("<bottom style=""").Append(Style.Border.GetStyleName(item.BottomStyle)).Append(""">")
                    If Not String.IsNullOrEmpty(item.BottomColor) Then
                        sb.Append("<color rgb=""").Append(item.BottomColor).Append("""/>")
                    Else
                        sb.Append("<color auto=""1""/>")
                    End If
                    sb.Append("</bottom>")
                Else
                    sb.Append("<bottom/>")
                End If
                If item.DiagonalStyle <> Style.Border.StyleValue.none Then
                    sb.Append("<diagonal style=""").Append(Style.Border.GetStyleName(item.DiagonalStyle)).Append(""">")
                    If Not String.IsNullOrEmpty(item.DiagonalColor) Then
                        sb.Append("<color rgb=""").Append(item.DiagonalColor).Append("""/>")
                    Else
                        sb.Append("<color auto=""1""/>")
                    End If
                    sb.Append("</diagonal>")
                Else
                    sb.Append("<diagonal/>")
                End If

                sb.Append("</border>")
            Next
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Method to create the XML string for the font part of the style sheet document
        ''' </summary>
        ''' <returns>String with formatted XML data.</returns>
        Private Function CreateStyleFontString() As String
            Dim fontStyles As Style.Font() = m_styles.GetFonts()
            Dim sb As StringBuilder = New StringBuilder()
            For Each item In fontStyles
                sb.Append("<font>")
                If item.Bold Then
                    sb.Append("<b/>")
                End If
                If item.Italic Then
                    sb.Append("<i/>")
                End If
                If item.Strike Then
                    sb.Append("<strike/>")
                End If
                If item.Underline <> Style.Font.UnderlineValue.none Then
                    If item.Underline = Style.Font.UnderlineValue.u_double Then
                        sb.Append("<u val=""double""/>")
                    ElseIf item.Underline = Style.Font.UnderlineValue.singleAccounting Then
                        sb.Append(" val=""singleAccounting""/>")
                    ElseIf item.Underline = Style.Font.UnderlineValue.doubleAccounting Then
                        sb.Append(" val=""doubleAccounting""/>")
                    Else
                        sb.Append("<u/>")
                    End If
                End If
                If item.VerticalAlign = Style.Font.VerticalAlignValue.subscript Then
                    sb.Append("<vertAlign val=""subscript""/>")
                ElseIf item.VerticalAlign = Style.Font.VerticalAlignValue.superscript Then
                    sb.Append("<vertAlign val=""superscript""/>")
                End If
                sb.Append("<sz val=""").Append(item.Size.ToString("G", culture)).Append("""/>")
                If String.IsNullOrEmpty(item.ColorValue) Then
                    sb.Append("<color theme=""").Append(item.ColorTheme.ToString("G", culture)).Append("""/>")
                Else
                    sb.Append("<color rgb=""").Append(item.ColorValue).Append("""/>")
                End If
                sb.Append("<name val=""").Append(item.Name).Append("""/>")
                sb.Append("<family val=""").Append(item.Family).Append("""/>")
                If item.Scheme <> Style.Font.SchemeValue.none Then
                    If item.Scheme = Style.Font.SchemeValue.major Then
                        sb.Append("<scheme val=""major""/>")
                    ElseIf item.Scheme = Style.Font.SchemeValue.minor Then
                        sb.Append("<scheme val=""minor""/>")
                    End If
                End If
                If Not String.IsNullOrEmpty(item.Charset) Then
                    sb.Append("<charset val=""").Append(item.Charset).Append("""/>")
                End If
                sb.Append("</font>")
            Next
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Method to create the XML string for the fill part of the style sheet document
        ''' </summary>
        ''' <returns>String with formatted XML data.</returns>
        Private Function CreateStyleFillString() As String
            Dim fillStyles As Style.Fill() = m_styles.GetFills()
            Dim sb As StringBuilder = New StringBuilder()
            For Each item In fillStyles
                sb.Append("<fill>")
                sb.Append("<patternFill patternType=""").Append(Style.Fill.GetPatternName(item.PatternFill)).Append("""")
                If item.PatternFill = Style.Fill.PatternValue.solid Then
                    sb.Append(">")
                    sb.Append("<fgColor rgb=""").Append(item.ForegroundColor).Append("""/>")
                    sb.Append("<bgColor indexed=""").Append(item.IndexedColor.ToString("G", culture)).Append("""/>")
                    sb.Append("</patternFill>")
                ElseIf item.PatternFill = Style.Fill.PatternValue.mediumGray OrElse item.PatternFill = Style.Fill.PatternValue.lightGray OrElse item.PatternFill = Style.Fill.PatternValue.gray0625 OrElse item.PatternFill = Style.Fill.PatternValue.darkGray Then
                    sb.Append(">")
                    sb.Append("<fgColor rgb=""").Append(item.ForegroundColor).Append("""/>")
                    If Not String.IsNullOrEmpty(item.BackgroundColor) Then
                        sb.Append("<bgColor rgb=""").Append(item.BackgroundColor).Append("""/>")
                    End If
                    sb.Append("</patternFill>")
                Else
                    sb.Append("/>")
                End If
                sb.Append("</fill>")
            Next
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Method to create the XML string for the number format part of the style sheet document
        ''' </summary>
        ''' <returns>String with formatted XML data.</returns>
        Private Function CreateStyleNumberFormatString() As String
            Dim numberFormatStyles As Style.NumberFormat() = m_styles.GetNumberFormats()
            Dim sb As StringBuilder = New StringBuilder()
            For Each item In numberFormatStyles
                If item.IsCustomFormat Then
                    If String.IsNullOrEmpty(item.CustomFormatCode) Then
                        Throw New FormatException("The number format style component with the ID " & item.CustomFormatID.ToString("G", culture) & " cannot be null or empty")
                    End If
                    ' OOXML: Escaping according to Chp.18.8.31
                    ' TODO: v4> Add a custom format builder as plugin 
                    sb.Append("<numFmt formatCode=""").Append(EscapeXmlAttributeChars(item.CustomFormatCode)).Append(""" numFmtId=""").Append(item.CustomFormatID.ToString("G", culture)).Append("""/>")
                End If
            Next
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Method to create the XML string for the XF part of the style sheet document
        ''' </summary>
        ''' <returns>String with formatted XML data.</returns>
        Private Function CreateStyleXfsString() As String
            Dim styleItems As Style() = m_styles.GetStyles()
            Dim sb As StringBuilder = New StringBuilder()
            Dim sb2 As StringBuilder = New StringBuilder()
            Dim alignmentString, protectionString As String
            Dim formatNumber, textRotation As Integer
            For Each style In styleItems
                textRotation = style.CurrentCellXf.CalculateInternalRotation()
                alignmentString = String.Empty
                protectionString = String.Empty
                If style.CurrentCellXf.HorizontalAlign <> Style.CellXf.HorizontalAlignValue.none OrElse style.CurrentCellXf.VerticalAlign <> Style.CellXf.VerticalAlignValue.none OrElse style.CurrentCellXf.Alignment <> Style.CellXf.TextBreakValue.none OrElse textRotation <> 0 Then
                    sb2.Clear()
                    sb2.Append("<alignment")
                    If style.CurrentCellXf.HorizontalAlign <> Style.CellXf.HorizontalAlignValue.none Then
                        sb2.Append(" horizontal=""")
                        If style.CurrentCellXf.HorizontalAlign = Style.CellXf.HorizontalAlignValue.center Then
                            sb2.Append("center")
                        ElseIf style.CurrentCellXf.HorizontalAlign = Style.CellXf.HorizontalAlignValue.right Then
                            sb2.Append("right")
                        ElseIf style.CurrentCellXf.HorizontalAlign = Style.CellXf.HorizontalAlignValue.centerContinuous Then
                            sb2.Append("centerContinuous")
                        ElseIf style.CurrentCellXf.HorizontalAlign = Style.CellXf.HorizontalAlignValue.distributed Then
                            sb2.Append("distributed")
                        ElseIf style.CurrentCellXf.HorizontalAlign = Style.CellXf.HorizontalAlignValue.fill Then
                            sb2.Append("fill")
                        ElseIf style.CurrentCellXf.HorizontalAlign = Style.CellXf.HorizontalAlignValue.general Then
                            sb2.Append("general")
                        ElseIf style.CurrentCellXf.HorizontalAlign = Style.CellXf.HorizontalAlignValue.justify Then
                            sb2.Append("justify")
                        Else
                            sb2.Append("left")
                        End If
                        sb2.Append("""")
                    End If
                    If style.CurrentCellXf.VerticalAlign <> Style.CellXf.VerticalAlignValue.none Then
                        sb2.Append(" vertical=""")
                        If style.CurrentCellXf.VerticalAlign = Style.CellXf.VerticalAlignValue.center Then
                            sb2.Append("center")
                        ElseIf style.CurrentCellXf.VerticalAlign = Style.CellXf.VerticalAlignValue.distributed Then
                            sb2.Append("distributed")
                        ElseIf style.CurrentCellXf.VerticalAlign = Style.CellXf.VerticalAlignValue.justify Then
                            sb2.Append("justify")
                        ElseIf style.CurrentCellXf.VerticalAlign = Style.CellXf.VerticalAlignValue.top Then
                            sb2.Append("top")
                        Else
                            sb2.Append("bottom")
                        End If
                        sb2.Append("""")
                    End If
                    If style.CurrentCellXf.Indent > 0 AndAlso (style.CurrentCellXf.HorizontalAlign = Style.CellXf.HorizontalAlignValue.left OrElse style.CurrentCellXf.HorizontalAlign = Style.CellXf.HorizontalAlignValue.right OrElse style.CurrentCellXf.HorizontalAlign = Style.CellXf.HorizontalAlignValue.distributed) Then
                        sb2.Append(" indent=""")
                        sb2.Append(style.CurrentCellXf.Indent.ToString("G", culture))
                        sb2.Append("""")
                    End If

                    If style.CurrentCellXf.Alignment <> Style.CellXf.TextBreakValue.none Then
                        If style.CurrentCellXf.Alignment = Style.CellXf.TextBreakValue.shrinkToFit Then
                            sb2.Append(" shrinkToFit=""1")
                        Else
                            sb2.Append(" wrapText=""1")
                        End If
                        sb2.Append("""")
                    End If
                    If textRotation <> 0 Then
                        sb2.Append(" textRotation=""")
                        sb2.Append(textRotation.ToString("G", culture))
                        sb2.Append("""")
                    End If
                    sb2.Append("/>") ' </xf>
                    alignmentString = sb2.ToString()
                End If

                If style.CurrentCellXf.Hidden OrElse style.CurrentCellXf.Locked Then
                    If style.CurrentCellXf.Hidden AndAlso style.CurrentCellXf.Locked Then
                        protectionString = "<protection locked=""1"" hidden=""1""/>"
                    ElseIf style.CurrentCellXf.Hidden AndAlso Not style.CurrentCellXf.Locked Then
                        protectionString = "<protection hidden=""1"" locked=""0""/>"
                    Else
                        protectionString = "<protection hidden=""0"" locked=""1""/>"
                    End If
                End If

                sb.Append("<xf numFmtId=""")
                If style.CurrentNumberFormat.IsCustomFormat Then
                    sb.Append(style.CurrentNumberFormat.CustomFormatID.ToString("G", culture))
                Else
                    formatNumber = style.CurrentNumberFormat.Number
                    sb.Append(formatNumber.ToString("G", culture))
                End If
                sb.Append(""" borderId=""").Append(style.CurrentBorder.InternalID.Value.ToString("G", culture))
                sb.Append(""" fillId=""").Append(style.CurrentFill.InternalID.Value.ToString("G", culture))
                sb.Append(""" fontId=""").Append(style.CurrentFont.InternalID.Value.ToString("G", culture))
                If Not style.CurrentFont.IsDefaultFont Then
                    sb.Append(""" applyFont=""1")
                End If
                If style.CurrentFill.PatternFill <> Style.Fill.PatternValue.none Then
                    sb.Append(""" applyFill=""1")
                End If
                If Not style.CurrentBorder.IsEmpty() Then
                    sb.Append(""" applyBorder=""1")
                End If
                If Not Equals(alignmentString, String.Empty) OrElse style.CurrentCellXf.ForceApplyAlignment Then
                    sb.Append(""" applyAlignment=""1")
                End If
                If Not Equals(protectionString, String.Empty) Then
                    sb.Append(""" applyProtection=""1")
                End If
                If style.CurrentNumberFormat.Number <> Style.NumberFormat.FormatNumber.none Then
                    sb.Append(""" applyNumberFormat=""1""")
                Else
                    sb.Append("""")
                End If
                If Not Equals(alignmentString, String.Empty) OrElse Not Equals(protectionString, String.Empty) Then
                    sb.Append(">")
                    sb.Append(alignmentString)
                    sb.Append(protectionString)
                    sb.Append("</xf>")
                Else
                    sb.Append("/>")
                End If
            Next
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Method to create the XML string for the color-MRU part of the style sheet document (recent colors)
        ''' </summary>
        ''' <returns>String with formatted XML data.</returns>
        Private Function CreateMruColorsString() As String
            Dim fonts As Style.Font() = m_styles.GetFonts()
            Dim fills As Style.Fill() = m_styles.GetFills()
            Dim sb As StringBuilder = New StringBuilder()
            Dim tempColors As List(Of String) = New List(Of String)()
            For Each item In fonts
                If String.IsNullOrEmpty(item.ColorValue) Then
                    Continue For
                End If
                If Equals(item.ColorValue, Style.Fill.DEFAULT_COLOR) Then
                    Continue For
                End If
                If Not tempColors.Contains(item.ColorValue) Then
                    tempColors.Add(item.ColorValue)
                End If
            Next
            For Each item In fills
                If Not String.IsNullOrEmpty(item.BackgroundColor) AndAlso Not Equals(item.BackgroundColor, Style.Fill.DEFAULT_COLOR) AndAlso Not tempColors.Contains(item.BackgroundColor) Then
                    tempColors.Add(item.BackgroundColor)
                End If
                If Not String.IsNullOrEmpty(item.ForegroundColor) AndAlso Not Equals(item.ForegroundColor, Style.Fill.DEFAULT_COLOR) AndAlso Not tempColors.Contains(item.ForegroundColor) Then
                    tempColors.Add(item.ForegroundColor)
                End If
            Next
            If tempColors.Count > 0 Then
                sb.Append("<mruColors>")
                For Each item In tempColors
                    sb.Append("<color rgb=""").Append(item).Append("""/>")
                Next
                sb.Append("</mruColors>")
                Return sb.ToString()
            End If
            Return String.Empty
        End Function

        ''' <summary>
        ''' Method to sort the cells of a worksheet as preparation for the XML document
        ''' </summary>
        ''' <param name="sheet">Worksheet to process.</param>
        ''' <returns>Sorted list of dynamic rows that are either defined by cells or row widths / hidden states. The list is sorted by row numbers (zero-based).</returns>
        Private Function GetSortedSheetData(sheet As Worksheet) As List(Of DynamicRow)
            Dim temp As List(Of Cell) = New List(Of Cell)()
            For Each item In sheet.Cells
                temp.Add(item.Value)
            Next
            temp.Sort()
            Dim row As DynamicRow = New DynamicRow()
            Dim rows As Dictionary(Of Integer, DynamicRow) = New Dictionary(Of Integer, DynamicRow)()
            Dim rowNumber As Integer
            If temp.Count > 0 Then
                rowNumber = temp(0).RowNumber
                row.RowNumber = rowNumber
                For Each cell In temp
                    If cell.RowNumber <> rowNumber Then
                        rows.Add(rowNumber, row)
                        row = New DynamicRow()
                        row.RowNumber = cell.RowNumber
                        rowNumber = cell.RowNumber
                    End If
                    row.CellDefinitions.Add(cell)
                Next
                If row.CellDefinitions.Count > 0 Then
                    rows.Add(rowNumber, row)
                End If
            End If
            For Each rowHeight In sheet.RowHeights
                If Not rows.ContainsKey(rowHeight.Key) Then
                    row = New DynamicRow()
                    row.RowNumber = rowHeight.Key
                    rows.Add(rowHeight.Key, row)
                End If
            Next
            For Each hiddenRow In sheet.HiddenRows
                If Not rows.ContainsKey(hiddenRow.Key) Then
                    row = New DynamicRow()
                    row.RowNumber = hiddenRow.Key
                    rows.Add(hiddenRow.Key, row)
                End If
            Next
            Dim output As List(Of DynamicRow) = rows.Values.ToList()
            output.Sort(Function(r1, r2) r1.RowNumber.CompareTo(r2.RowNumber)) ' Lambda sort
            Return output
        End Function

        ''' <summary>
        ''' Method to escape XML characters between two XML tags
        ''' </summary>
        ''' <param name="input">Input string to process.</param>
        ''' <returns>Escaped string.</returns>
        Public Shared Function EscapeXmlChars(input As String) As String
            If Equals(input, Nothing) Then
                Return ""
            End If
            Dim len = input.Length
            Dim illegalCharacters As List(Of Integer) = New List(Of Integer)(len)
            Dim characterTypes As List(Of Byte) = New List(Of Byte)(len)
            Dim i As Integer
            For i = 0 To len - 1
                Dim ch As Integer = AscW(input(i))

                If ch < &H9 OrElse ch > &HA AndAlso ch < &HD OrElse ch > &HD AndAlso ch < &H20 OrElse ch > &HD7FF AndAlso ch < &HE000 OrElse ch > &HFFFD Then
                    illegalCharacters.Add(i)
                    characterTypes.Add(0)
                    Continue For
                End If ' Note: XML specs allow characters up to 0x10FFFF. However, the C# char range is only up to 0xFFFF; Higher values are neglected here 
                If ch = &H3C Then ' <
                    illegalCharacters.Add(i)
                    characterTypes.Add(1)
                ElseIf ch = &H3E Then ' >
                    illegalCharacters.Add(i)
                    characterTypes.Add(2)
                ElseIf ch = &H26 Then ' &
                    illegalCharacters.Add(i)
                    characterTypes.Add(3)
                End If
            Next
            If illegalCharacters.Count = 0 Then
                Return input
            End If

            Dim sb As StringBuilder = New StringBuilder(len)
            Dim lastIndex = 0
            len = illegalCharacters.Count
            For i = 0 To len - 1
                sb.Append(input.Substring(lastIndex, illegalCharacters(i) - lastIndex))
                If characterTypes(i) = 0 Then
                    sb.Append(" "c) ' Whitespace as fall back on illegal character
                ElseIf characterTypes(i) = 1 Then ' replace <
                    sb.Append("&lt;")
                ElseIf characterTypes(i) = 2 Then ' replace >
                    sb.Append("&gt;")
                ElseIf characterTypes(i) = 3 Then ' replace &
                    sb.Append("&amp;")
                End If
                lastIndex = illegalCharacters(i) + 1
            Next
            sb.Append(input.Substring(lastIndex))
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Method to escape XML characters in an XML attribute
        ''' </summary>
        ''' <param name="input">Input string to process.</param>
        ''' <returns>Escaped string.</returns>
        Public Shared Function EscapeXmlAttributeChars(input As String) As String
            input = EscapeXmlChars(input) ' Sanitize string from illegal characters beside quotes
            input = input.Replace("""", "&quot;")
            Return input
        End Function

        ''' <summary>
        ''' Method to generate an Excel internal password hash to protect workbooks or worksheets<br></br>This method is derived from the c++ implementation by Kohei Yoshida (<a href="http://kohei.us/2008/01/18/excel-sheet-protection-password-hash/">http://kohei.us/2008/01/18/excel-sheet-protection-password-hash/</a>)
        ''' </summary>
        ''' <param name="password">Password string in UTF-8 to encrypt.</param>
        ''' <returns>16 bit hash as hex string.</returns>
        Public Shared Function GeneratePasswordHash(password As String) As String
            If String.IsNullOrEmpty(password) Then
                Return String.Empty
            End If
            Dim passwordLength = password.Length
            Dim passwordHash = 0
            Dim character As Integer

            Const N As Integer = Asc("N"c)
            Const K As Integer = Asc("K"c)

            For i = passwordLength To 1 Step -1
                character = AscW(password(i - 1))
                passwordHash = passwordHash >> 14 And &H1 Or passwordHash << 1 And &H7FFF
                passwordHash = passwordHash Xor character
            Next
            passwordHash = passwordHash >> 14 And &H1 Or passwordHash << 1 And &H7FFF
            passwordHash = passwordHash Xor &H8000 Or N << 8 Or K
            passwordHash = passwordHash Xor passwordLength
            Return passwordHash.ToString("X")
        End Function

        ''' <summary>
        ''' Method to convert a date or date and time into the internal Excel time format (OAdate)
        ''' </summary>
        ''' <param name="date">Date to process.</param>
        ''' <returns>Date or date and time as number.</returns>
        Public Shared Function GetOADateTimeString([date] As Date) As String
            If [date] < FIRST_ALLOWED_EXCEL_DATE OrElse [date] > LAST_ALLOWED_EXCEL_DATE Then
                Throw New FormatException("The date is not in a valid range for Excel. Dates before 1900-01-01 or after 9999-12-31 are not allowed.")
            End If
            Dim dateValue = [date]
            If [date] < FIRST_VALID_EXCEL_DATE Then
                dateValue = [date].AddDays(-1) ' Fix of the leap-year-1900-error
            End If
            Dim currentMillis = dateValue.Ticks / TimeSpan.TicksPerMillisecond
            Dim d = (dateValue.Second + dateValue.Minute * 60 + dateValue.Hour * 3600) / 86400.0R + stdNum.Floor((currentMillis - ROOT_MILLIS) / 86400000.0R)
            Return d.ToString("G", INVARIANT_CULTURE)
        End Function

        ''' <summary>
        ''' Method to convert a time into the internal Excel time format (OAdate without days)
        ''' </summary>
        ''' <param name="time">Time to process. The date component of the timespan is neglected.</param>
        ''' <returns>Time as number.</returns>
        Public Shared Function GetOATimeString(time As TimeSpan) As String
            Dim seconds = time.Seconds + time.Minutes * 60 + time.Hours * 3600
            Dim d = seconds / 86400.0R
            Return d.ToString("G", INVARIANT_CULTURE)
        End Function

        ''' <summary>
        ''' Calculates the internal width of a column in characters. This width is used only in the XML documents of worksheets and is usually not exposed to the (Excel) end user
        ''' </summary>
        ''' <param name="columnWidth">Target column width (displayed in Excel).</param>
        ''' <param name="maxDigitWidth">Maximum digit with of the default font (default is 7.0 for Calibri, size 11).</param>
        ''' <param name="textPadding">Text padding of the default font (default is 5.0 for Calibri, size 11).</param>
        ''' <returns>The internal column width in characters, used in worksheet XML documents.</returns>
        Public Shared Function GetInternalColumnWidth(columnWidth As Single, Optional maxDigitWidth As Single = 7.0F, Optional textPadding As Single = 5.0F) As Single
            If columnWidth < Worksheet.MIN_COLUMN_WIDTH OrElse columnWidth > Worksheet.MAX_COLUMN_WIDTH Then
                Throw New FormatException("The column width " & columnWidth.ToString() & " is not valid. The valid range is between " & Worksheet.MIN_COLUMN_WIDTH.ToString() & " and " & Worksheet.MAX_COLUMN_WIDTH.ToString())
            End If
            If columnWidth <= 0F OrElse maxDigitWidth <= 0F Then
                Return 0F
            ElseIf columnWidth <= 1.0F Then
                Return CSng(stdNum.Floor(columnWidth * (maxDigitWidth + textPadding) / maxDigitWidth * COLUMN_WIDTH_ROUNDING_MODIFIER)) / COLUMN_WIDTH_ROUNDING_MODIFIER
            Else
                Return CSng(stdNum.Floor((columnWidth * maxDigitWidth + textPadding) / maxDigitWidth * COLUMN_WIDTH_ROUNDING_MODIFIER)) / COLUMN_WIDTH_ROUNDING_MODIFIER
            End If
        End Function

        ''' <summary>
        ''' Calculates the internal height of a row. This height is used only in the XML documents of worksheets and is usually not exposed to the (Excel) end user
        ''' </summary>
        ''' <param name="rowHeight">Target row height (displayed in Excel).</param>
        ''' <returns>The internal row height which snaps to the nearest pixel.</returns>
        Public Shared Function GetInternalRowHeight(rowHeight As Single) As Single
            If rowHeight < Worksheet.MIN_ROW_HEIGHT OrElse rowHeight > Worksheet.MAX_ROW_HEIGHT Then
                Throw New FormatException("The row height " & rowHeight.ToString() & " is not valid. The valid range is between " & Worksheet.MIN_ROW_HEIGHT.ToString() & " and " & Worksheet.MAX_ROW_HEIGHT.ToString())
            End If
            If rowHeight <= 0F Then
                Return 0F
            End If
            Dim heightInPixel = stdNum.Round(rowHeight * ROW_HEIGHT_POINT_MULTIPLIER)
            Return CSng(heightInPixel) / ROW_HEIGHT_POINT_MULTIPLIER
        End Function

        ''' <summary>
        ''' Calculates the internal width of a split pane in a worksheet. This width is used only in the XML documents of worksheets and is not exposed to the (Excel) end user
        ''' </summary>
        ''' <param name="width">Target column(s) width (one or more columns, displayed in Excel).</param>
        ''' <param name="maxDigitWidth">Maximum digit with of the default font (default is 7.0 for Calibri, size 11).</param>
        ''' <param name="textPadding">Text padding of the default font (default is 5.0 for Calibri, size 11).</param>
        ''' <returns>The internal pane width, used in worksheet XML documents in case of worksheet splitting.</returns>
        Public Shared Function GetInternalPaneSplitWidth(width As Single, Optional maxDigitWidth As Single = 7.0F, Optional textPadding As Single = 5.0F) As Single
            Dim pixels As Single
            If width < 0 Then
                width = 0
            End If
            If width <= 1.0F Then
                pixels = CSng(stdNum.Floor(width / SPLIT_WIDTH_MULTIPLIER + SPLIT_WIDTH_OFFSET))
            Else
                pixels = CSng(stdNum.Floor(width * maxDigitWidth + SPLIT_WIDTH_OFFSET)) + textPadding
            End If
            Dim points = pixels * SPLIT_WIDTH_POINT_MULTIPLIER
            Return points * SPLIT_POINT_DIVIDER + SPLIT_WIDTH_POINT_OFFSET
        End Function

        ''' <summary>
        ''' Calculates the internal height of a split pane in a worksheet. This height is used only in the XML documents of worksheets and is not exposed to the (Excel) user
        ''' </summary>
        ''' <param name="height">Target row(s) height (one or more rows, displayed in Excel).</param>
        ''' <returns>The internal pane height, used in worksheet XML documents in case of worksheet splitting.</returns>
        Public Shared Function GetInternalPaneSplitHeight(height As Single) As Single
            If height < 0 Then
                height = 0F
            End If
            Return stdNum.Floor(SPLIT_POINT_DIVIDER * height + SPLIT_HEIGHT_POINT_OFFSET)
        End Function

    End Class
End Namespace
