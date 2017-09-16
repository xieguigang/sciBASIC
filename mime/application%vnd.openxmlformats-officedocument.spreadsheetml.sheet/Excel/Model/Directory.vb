Imports Microsoft.VisualBasic.MIME.Office.Excel.XML._rels
Imports Microsoft.VisualBasic.MIME.Office.Excel.XML.docProps
Imports Microsoft.VisualBasic.MIME.Office.Excel.XML.xl
Imports Microsoft.VisualBasic.MIME.Office.Excel.XML.xl.worksheets
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Language
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
    ''' Get <see cref="worksheet"/> by name.
    ''' </summary>
    ''' <param name="name$"></param>
    ''' <returns></returns>
    Public Function GetWorksheet(name$) As worksheet
        Dim sheetID$ = workbook.GetSheetIDByName(name)

        If sheetID.StringEmpty Then
            Return Nothing
        Else
            Return worksheets.GetWorksheet(sheetID)
        End If
    End Function

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

    Public Property worksheets As Dictionary(Of String, worksheet)

    Sub New(ROOT$)
        Call MyBase.New(ROOT)
    End Sub

    Public Function HaveWorksheet(sheetID$) As Boolean
        Return worksheets.ContainsKey("sheet" & sheetID)
    End Function

    Public Function GetWorksheet(sheetID$) As worksheet
        With "sheet" & sheetID
            If worksheets.ContainsKey(.ref) Then
                Return worksheets(.ref)
            Else
                Return Nothing
            End If
        End With
    End Function

    Protected Overrides Sub _loadContents()
        worksheets = (ls - l - "*.xml" <= Folder) _
            .ToDictionary(Function(name) name.BaseName,
                          Function(path) path.LoadXml(Of worksheet))
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