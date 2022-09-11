#Region "Microsoft.VisualBasic::b16c9df48eece994c3a79710f0a591e0, sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\Model\File.vb"

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

    '   Total Lines: 229
    '    Code Lines: 147
    ' Comment Lines: 54
    '   Blank Lines: 28
    '     File Size: 7.83 KB


    ' Class File
    ' 
    '     Properties: _rels, ContentTypes, docProps, FilePath, MimeType
    '                 xl
    ' 
    '     Function: AddSheetTable, CreatePackage, (+2 Overloads) GetTable, GetWorksheet, LoadDataSet
    '               Open, SheetNames, ToString, (+2 Overloads) WriteSheetTable, WriteXlsx
    ' 
    '     Sub: addInternal
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
Imports Microsoft.VisualBasic.MIME.Office.Excel.Model
Imports Microsoft.VisualBasic.MIME.Office.Excel.Model.Directory
Imports Microsoft.VisualBasic.MIME.Office.Excel.XML.xl.worksheets
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml.OpenXml
Imports csv = Microsoft.VisualBasic.Data.csv.IO.File
Imports OpenXML = Microsoft.VisualBasic.MIME.Office.Excel.Model.Xmlns

''' <summary>
''' ``*.xlsx`` document file
''' </summary>
Public Class File : Implements IFileReference

    Public Property ContentTypes As ContentTypes
    Public Property _rels As _rels
    Public Property docProps As docProps
    Public Property xl As xl

    Friend ReadOnly modify As New Index(Of String)
    Friend ROOT$

    Dim _filePath As [Default](Of String)

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

    Public ReadOnly Property MimeType As Net.Protocols.ContentTypes.ContentType() Implements IFileReference.MimeType
        Get
            Throw New NotImplementedException()
        End Get
    End Property

    ''' <summary>
    ''' get all sheet names from current xlsx document
    ''' </summary>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function SheetNames() As IEnumerable(Of String)
        Return xl _
            .workbook _
            .sheets _
            .Select(Function(s) s.name)
    End Function

    Public Overrides Function ToString() As String
        Return FilePath
    End Function

    ''' <summary>
    ''' 使用序列化写入数据到xlsx文件之中
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="data"></param>
    ''' <param name="sheetName$"></param>
    ''' <returns></returns>
    Public Function WriteSheetTable(Of T)(data As IEnumerable(Of T), sheetName$) As Boolean
        Dim table As csv = data.ToCsvDoc
        Return WriteSheetTable(table, sheetName)
    End Function

    ''' <summary>
    ''' 如果表名不存在，会追加，否则会直接替换现有的表数据
    ''' </summary>
    ''' <param name="table"></param>
    ''' <param name="sheetName$"></param>
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
