Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.MIME.Office.Excel.XML._rels
Imports Microsoft.VisualBasic.MIME.Office.Excel.XML.xl
Imports Microsoft.VisualBasic.MIME.Office.Excel.XML.xl.worksheets

Public Class worksheets : Inherits Directory

    ''' <summary>
    ''' Key都是格式``sheet%d``的字符串
    ''' </summary>
    ''' <returns></returns>
    Public Property worksheets As Dictionary(Of String, worksheet)
    Public Property _rels As Dictionary(Of String, rels)

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
        Else
            For Each key In worksheets.Keys
                If key.TextEquals(sheetID) Then
                    sheetID = key
                    Return worksheets(sheetID)
                End If
            Next
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
            .Select(Function(path) (path:=path, path.LoadXml(Of worksheet))) _
            .ToDictionary(Function(page) page.path.BaseName,
                          Function(page) page.Item2)
        _rels = (ls - l - "*.rels" <= (Folder & "/_rels")) _
            .ToDictionary(Function(path) path.BaseName,
                          Function(path) path.LoadXml(Of rels))
    End Sub

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
