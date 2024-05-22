#Region "Microsoft.VisualBasic::fff55d1a8a5b1450313f8aafbeeec6f9, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\XLSX\File.vb"

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

    '   Total Lines: 335
    '    Code Lines: 174 (51.94%)
    ' Comment Lines: 125 (37.31%)
    '    - Xml Docs: 88.00%
    ' 
    '   Blank Lines: 36 (10.75%)
    '     File Size: 14.03 KB


    '     Class File
    ' 
    '         Properties: _rels, ContentTypes, docProps, FilePath, MimeType
    '                     xl
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: _name, AddSheetTable, CreatePackage, FullName, (+2 Overloads) GetTable
    '                   GetWorkdir, GetWorksheet, LoadDataSet, Open, SheetNames
    '                   ToString, (+2 Overloads) WriteSheetTable, WriteXlsx
    ' 
    '         Sub: _loadContents, addInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO.Compression
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Zip
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.FileIO
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Model
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Model.Directory
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.XML.xl.worksheets
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml.OpenXml
Imports csv = Microsoft.VisualBasic.Data.csv.IO.File
Imports OpenXML = Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Model.Xmlns

Namespace XLSX

    ''' <summary>
    ''' XLSX Transitional (Office Open XML), ISO 29500:2008-2016, ECMA-376, Editions 1-5
    ''' 
    ''' > ``*.xlsx`` document file
    ''' 
    ''' Format Description Properties Explanation of format description terms
    '''
    ''' + ID: fdd000398
    ''' + Short name: XLSX/ OOXML_2012
    ''' + Content categories: spreadsheet, office / business
    ''' + Format Category: file-format
    ''' + Other facets: text, structured, symbolic
    ''' + Last significant FDD update: 2022-05-02
    ''' + Draft status: Full
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' The Open Office XML-based spreadsheet format using .xlsx as a file extension 
    ''' has been the default format produced for new documents by versions of Microsoft 
    ''' Excel since Excel 2007. The format was designed to be equivalent to the binary 
    ''' .xls format produced by earlier versions of Microsoft Excel (see MS-XLS). For
    ''' convenience, this format description uses XLSX to identify the corresponding 
    ''' format. The primary content of a XLSX file is marked up in SpreadsheetML, which 
    ''' is specified in parts 1 and 4 of ISO/IEC 29500, Information technology -- Document
    ''' description and processing languages -- Office Open XML File Formats (OOXML). 
    ''' This description focuses on the specification in ISO/IEC 29500:2012 and represents
    ''' the format variant known as "Transitional." Although editions of ISO 29500 were 
    ''' published in 2008, 2011, 2012, and 2016, the specification has had very few 
    ''' changes other than clarifications and corrections to match actual usage in 
    ''' documents since SpreadsheetML was first standardized in ECMA-376, Part 1 in 2006.
    ''' This description can be read as applying to all SpreadsheetML versions published 
    ''' by ECMA International and by ISO/IEC through 2016. See Notes below for more 
    ''' detail on the chronological versions and differences.
    '''
    ''' The XLSX format uses the SpreadsheetML markup language And schema To represent a 
    ''' spreadsheet "document." Conceptually, Using the terminology Of the Spreadsheet 
    ''' ML specification In ISO/IEC 29500-1, the document comprises one Or more worksheets
    ''' In a workbook. A worksheet typically consists Of a rectangular grid Of cells. 
    ''' Each cell can contain a value Or a formula, which will be used To calculate a value,
    ''' With a cached value usually stored pending the Next recalculation. A Single 
    ''' spreadsheet document may serve several purposes: as a container for data values; 
    ''' as program code (based on the formulas in cells) to perform analyses on those 
    ''' values; And as one Or more formatted reports (including charts) of the analyses. 
    ''' Beyond basics, spreadsheet applications have introduced support for more advanced
    ''' features over time. These include mechanisms to extract data dynamically from external
    ''' sources, to support collaborative work, And to perform an increasing number of 
    ''' functions that would have required a database application in the past, such as sorting
    ''' And filtering of entries in a table to display a temporary subset. The markup 
    ''' specification must support both basic And more advanced functionalities in a structure
    ''' that supports the robust performance expected by users.
    ''' </remarks>
    Public Class File : Inherits XlsxDirectoryPart
        Implements IFileReference

        Public Property ContentTypes As ContentTypes
        Public Property _rels As _rels
        Public Property docProps As docProps
        Public Property xl As xl

        Friend ReadOnly modify As New Index(Of String)

        Dim _filePath As [Default](Of String)

        ''' <summary>
        ''' the original file path the reference to this xlsx file
        ''' </summary>
        ''' <returns></returns>
        Public Property FilePath As String Implements IFileReference.FilePath
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return _filePath.value
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Friend Set(value As String)
                _filePath = value
            End Set
        End Property

        ''' <summary>
        ''' get table by sheet name
        ''' </summary>
        ''' <param name="sheetName"></param>
        ''' <returns></returns>
        Default Public Property TableItem(sheetName As String) As csv
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return GetTable(sheetName)
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set
                Call WriteSheetTable(Value, sheetName)
            End Set
        End Property

        Public ReadOnly Property MimeType As ContentType() Implements IFileReference.MimeType
            Get
                ' Microsoft Office - OOXML - Spreadsheet
                ' application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
                ' .xlsx
                ' IANA: OOXML - Spreadsheet
                Return {
                    New ContentType("Microsoft Office - OOXML - Spreadsheet", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ".xlsx") With {
                        .Details = "IANA: OOXML - Spreadsheet"
                    }
                }
            End Get
        End Property

        Sub New()
            Call MyBase.New(Nothing)
        End Sub

        Friend Sub New(workdir As String)
            Call MyBase.New(workdir)
        End Sub

#Region "XlsxDirectoryPart"
        Protected Overrides Function _name() As String
            Return ""
        End Function

        Protected Overrides Sub _loadContents()
            ' do nothing
        End Sub
#End Region

        ''' <summary>
        ''' get all sheet names from current xlsx document
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function SheetNames() As IEnumerable(Of String)
            Return xl.workbook.sheets.Select(Function(s) s.name)
        End Function

        ''' <summary>
        ''' the zip package internal file reference name
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function FullName(name As String) As String
            Return InternalFileName(name)
        End Function

        Public Overrides Function ToString() As String
            Return FilePath
        End Function

        ''' <summary>
        ''' 使用序列化写入数据到xlsx文件之中
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="data">A .net clr object collection</param>
        ''' <param name="sheetName">the name of the sheet table to write data</param>
        ''' <returns></returns>
        Public Function WriteSheetTable(Of T)(data As IEnumerable(Of T), sheetName$) As Boolean
            Dim table As csv = data.ToCsvDoc
            Return WriteSheetTable(table, sheetName)
        End Function

        ''' <summary>
        ''' 如果表名不存在，会追加，否则会直接替换现有的表数据
        ''' </summary>
        ''' <param name="table"></param>
        ''' <param name="sheetName">the name of the sheet table to write data</param>
        ''' <returns></returns>
        Public Function WriteSheetTable(table As csv, sheetName$) As Boolean
            Dim worksheet As worksheet = table.CreateWorksheet(xl.sharedStrings)
            Dim sheetID = xl.workbook.GetSheetIDByName(sheetName)

            If Not sheetID.StringEmpty Then
                ' 进行替换
                xl.worksheets.worksheets(sheetID) = worksheet

                With "worksheet.update"
                    If modify.IndexOf(.ByRef) = -1 Then
                        modify.Add(.ByRef)
                    End If
                End With
            Else
                Call addInternal(sheetName, worksheet)
                Call docProps.addSheetName(sheetName)
                Call docProps.save()
            End If

            Return True
        End Function

        Private Sub addInternal(sheetName$, worksheet As worksheet)
            ' 进行添加
            Dim sheetID = xl.workbook.Add(sheetName)
            xl.worksheets.Add(sheetID, worksheet)
            ContentTypes.Overrides += New Type With {
                .ContentType = OpenXML.worksheet,
                .PartName = $"/xl/worksheets/{sheetID}.xml"
            }

            With "worksheet.add"
                If modify.IndexOf(.ByRef) = -1 Then
                    modify.Add(.ByRef)
                End If
            End With
        End Sub

        ''' <summary>
        ''' Add new worksheet
        ''' </summary>
        ''' <param name="sheetName"></param>
        ''' <returns></returns>
        Public Function AddSheetTable(sheetName As String) As worksheet
            With New csv().CreateWorksheet(xl.sharedStrings)
                Call addInternal(sheetName, .ByRef)
                Return .ByRef
            End With
        End Function

        ''' <summary>
        ''' get the zip file workdir
        ''' </summary>
        ''' <returns></returns>
        Public Function GetWorkdir() As String
            Return folder
        End Function

        ''' <summary>
        ''' 默认是写入原来的文件位置
        ''' </summary>
        ''' <param name="path$"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function WriteXlsx(Optional path$ = Nothing) As Boolean
            ' Save to the user specific path or original source _filePath 
            ' If the path Is Not specific by user
            Return Me.SaveTo(path Or _filePath)
        End Function

        ''' <summary>
        ''' Get worksheet table by sheet name
        ''' </summary>
        ''' <param name="sheetName$"></param>
        ''' <returns></returns>
        Public Function GetTable(sheetName$) As csv
            Dim worksheet As XML.xl.worksheets.worksheet = xl.GetWorksheet(sheetName)

            If worksheet Is Nothing Then
                Return Nothing
            Else
                Return xl.GetTableData(worksheet)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetWorksheet(sheetName$) As XML.xl.worksheets.worksheet
            Return xl.GetWorksheet(sheetName)
        End Function

        ''' <summary>
        ''' Get worksheet table by its index in the workbook.
        ''' (<paramref name="index"/>是以零为底的下标编号)
        ''' </summary>
        ''' <param name="index">ZERO based array index.</param>
        ''' <returns></returns>
        Public Function GetTable(index As Integer) As csv
            Dim worksheet As XML.xl.worksheets.worksheet = xl.GetWorksheetByIndex(index)

            If worksheet Is Nothing Then
                Return Nothing
            Else
                Return xl.GetTableData(worksheet)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function LoadDataSet(Of T As Class)(sheetName$) As T()
            Return GetTable(sheetName).AsDataSource(Of T)
        End Function

        Public Shared Function CreatePackage(tmp$, xlsx$, Optional throwEx As Boolean = True) As Boolean
            Try
                Call ZipLib.DirectoryArchive(tmp, xlsx, ArchiveAction.Replace, Overwrite.Always, CompressionLevel.Fastest)
                Return True
            Catch ex As Exception
                Dim debug$ = New Dictionary(Of String, String) From {
                    {NameOf(tmp), tmp},
                    {NameOf(xlsx), xlsx}
                }.GetJson

                ex = New Exception(debug, ex)

                If throwEx Then
                    Throw ex
                Else
                    Call App.LogException(ex)
                End If

                Return False
            End Try
        End Function

        ''' <summary>
        ''' 读取Excel文件
        ''' </summary>
        ''' <param name="path">the file path of ``*.xlsx`` file target.</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this function allows read table data from a web url
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Open(path As String) As File
            Return IO.CreateReader(xlsx:=path)
        End Function
    End Class
End Namespace
