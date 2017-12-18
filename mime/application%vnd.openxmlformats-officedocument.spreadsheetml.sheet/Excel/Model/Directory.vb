#Region "Microsoft.VisualBasic::a2dff7a0e7cb3eb1f47dee0981c9dc1c, ..\sciBASIC#\mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\Model\Directory.vb"

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

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.MIME.Office.Excel.XML._rels
Imports Microsoft.VisualBasic.MIME.Office.Excel.XML.docProps
Imports Microsoft.VisualBasic.MIME.Office.Excel.XML.xl
Imports Microsoft.VisualBasic.MIME.Office.Excel.XML.xl.worksheets
Imports csv = Microsoft.VisualBasic.Data.csv.IO.File

Public Class _rels : Inherits Directory

    Sub New(ROOT$)
        Call MyBase.New(ROOT)
    End Sub

    Public Property rels As rels

    Protected Overrides Sub _loadContents()
        rels = (Folder & "/.rels").LoadXml(Of rels)
    End Sub

    Protected Overrides Function _name() As String
        Return NameOf(Excel._rels)
    End Function
End Class

Public Class docProps : Inherits Directory

    Public Property core As core
    Public Property app As XML.docProps.app
    Public Property custom As custom

    Sub New(ROOT$)
        Call MyBase.New(ROOT)
    End Sub

    Protected Overrides Sub _loadContents()
        core = (Folder & "/core.xml").LoadXml(Of core)
        custom = (Folder & "/custom.xml").LoadXml(Of custom)
        app = (Folder & "/app.xml").LoadXml(Of XML.docProps.app)
    End Sub

    Protected Overrides Function _name() As String
        Return NameOf(docProps)
    End Function
End Class

Public Class xl : Inherits Directory

    Public Property workbook As workbook
    Public Property styles As styles
    Public Property sharedStrings As sharedStrings
    Public Property worksheets As worksheets

    Sub New(ROOT$)
        Call MyBase.New(ROOT)
    End Sub

    ''' <summary>
    ''' 使用表名称来判断目标工作簿是否存在？
    ''' </summary>
    ''' <param name="worksheet$"></param>
    ''' <returns></returns>
    Public Function Exists(worksheet$) As Boolean
        Return Not workbook _
            .GetSheetIDByName(worksheet) _
            .StringEmpty
    End Function

    ''' <summary>
    ''' Get <see cref="worksheet"/> by name.
    ''' </summary>
    ''' <param name="name$">如果表名称不存在的话，则这个函数是会返回一个空值的</param>
    ''' <returns></returns>
    Public Function GetWorksheet(name$) As worksheet
        Dim sheetID$ = workbook.GetSheetIDByName(name)

        If sheetID.StringEmpty Then
            Return Nothing
        Else
            Return worksheets.GetWorksheet(sheetID)
        End If
    End Function

    ''' <summary>
    ''' 因为这个是直接通过编号来查找的，所以应该不会存在不存在名称的问题
    ''' 直接返回就好了
    ''' </summary>
    ''' <param name="index"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetWorksheetByIndex(index As Integer) As worksheet
        Return worksheets.GetWorksheet(sheetID:=workbook.GetSheetIDByIndex(index))
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetTableData(worksheet As worksheet) As csv
        Return worksheet.ToTableFrame(sharedStrings)
    End Function

    Protected Overrides Sub _loadContents()
        sharedStrings = (Folder & "/sharedStrings.xml").LoadXml(Of sharedStrings)
        workbook = (Folder & "/workbook.xml").LoadXml(Of workbook)
        worksheets = New worksheets(Folder)
    End Sub

    Protected Overrides Function _name() As String
        Return NameOf(xl)
    End Function
End Class

Public Class worksheets : Inherits Directory

    ''' <summary>
    ''' Key都是格式``sheet%d``的字符串
    ''' </summary>
    ''' <returns></returns>
    Public Property worksheets As Dictionary(Of String, worksheet)

    Sub New(ROOT$)
        Call MyBase.New(ROOT)
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function HaveWorksheet(sheetID$) As Boolean
        Return worksheets.ContainsKey(sheetID) OrElse
               worksheets.ContainsKey("sheet" & sheetID)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub Add(sheetID As String, worksheet As worksheet)
        Call worksheets.Add(sheetID, worksheet)
    End Sub

    Public Function GetWorksheet(sheetID$) As worksheet
        If worksheets.ContainsKey(sheetID) Then
            Return worksheets(sheetID)
        End If

        With "sheet" & sheetID
            If worksheets.ContainsKey(.ref) Then
                Return worksheets(.ref)
            Else
                Return Nothing
            End If
        End With
    End Function

    Protected Overrides Sub _loadContents()
        ' 2017-12-18 发现有时候会出现sheetID不一致的情况，这种情况可能会出现于用户手动的从Excel电子表格文件之中删除了前面的几个表
        ' 所以在这里不可以直接使用文件名来作为sheet的编号名称
        ' r:id是一致的
        worksheets = (ls - l - "*.xml" <= Folder) _
            .Select(Function(path) (path, path.LoadXml(Of worksheet))) _
            .ToDictionary(Function(page) getID(page),
                          Function(page) page.Item2)
    End Sub

    Private Shared Function getID(page As (path$, page As worksheet)) As String
        Dim sheet = page.page

        If Not sheet.pageSetup Is Nothing Then
            Return sheet.pageSetup.id
        Else
            Return page.path.BaseName
        End If
    End Function

    Public Sub Save()
        Dim path$

        For Each sheet In worksheets
            path = $"{Folder}/{sheet.Key}.xml"
            sheet.Value _
                .ToXML _
                .SaveTo(path, Encoding.UTF8)
        Next
    End Sub

    Protected Overrides Function _name() As String
        Return NameOf(worksheets)
    End Function
End Class

Public MustInherit Class Directory

    Public ReadOnly Property Folder As String

    Sub New(ROOT$)
        Folder = $"{ROOT}/{_name()}"
        Call _loadContents()
    End Sub

    Protected MustOverride Function _name() As String
    Protected MustOverride Sub _loadContents()

    Public Overrides Function ToString() As String
        Return Folder
    End Function
End Class
