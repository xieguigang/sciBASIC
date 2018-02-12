Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.MIME.Office.Excel.XML.xl
Imports Microsoft.VisualBasic.MIME.Office.Excel.XML.xl.worksheets

Namespace Model

    Public Class SheetTable

        Dim strings As sharedStrings
        Dim table As worksheet
        Dim name$

        Public Property Row(i%) As RowObject
            Get

            End Get
            Set(value As RowObject)

            End Set
        End Property

        Public Property Column(i%) As String()
            Get

            End Get
            Set(value As String())

            End Set
        End Property

        Public Property Column(ID$) As String()
            Get

            End Get
            Set(value As String())

            End Set
        End Property

        Default Public Property Cell(point$) As String
            Get

            End Get
            Set(value As String)

            End Set
        End Property

        Default Public Property Cell(X%, Y%) As String
            Get

            End Get
            Set(value As String)

            End Set
        End Property

        Sub New(xlsx As File, sheetName$)
            strings = xlsx.xl.sharedStrings
            name = sheetName

            If xlsx.xl.Exists(worksheet:=sheetName) Then
                table = xlsx.xl.GetWorksheet(sheetName)
            Else
                table = xlsx.AddSheetTable(sheetName)
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return name
        End Function
    End Class
End Namespace
