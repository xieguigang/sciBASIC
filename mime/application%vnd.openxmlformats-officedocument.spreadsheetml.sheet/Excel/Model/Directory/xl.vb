Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.Office.Excel.XML.xl
Imports Microsoft.VisualBasic.MIME.Office.Excel.XML.xl.worksheets
Imports csv = Microsoft.VisualBasic.Data.csv.IO.File

Public Class xl : Inherits Directory

    Public Property workbook As workbook
    Public Property styles As styles
    Public Property sharedStrings As sharedStrings
    Public Property worksheets As worksheets
    Public Property _rels As _rels

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

        ' rId to sheetName by using rels file
        If sheetID.StringEmpty Then
            Return Nothing
        Else
            Dim key$ = _rels.workbook.Target(sheetID).Target.BaseName
            Return worksheets.GetWorksheet(key)
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
        _rels = New _rels(Folder)
    End Sub

    Protected Overrides Function _name() As String
        Return NameOf(xl)
    End Function
End Class
