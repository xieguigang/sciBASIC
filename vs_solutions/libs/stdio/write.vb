Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Framework
Imports xlsxFile = Microsoft.VisualBasic.MIME.Office.Excel.XLSX.File

Public Module write

    Public Sub csv(x As NumericTable, file As String)
        Call x.WriteCsv(file)
    End Sub

    Public Sub tsv(x As NumericTable, file As String)
        Call x.WriteTsv(file)
    End Sub

    Public Sub xlsx(x As NumericTable, file As String, sheetName As String)
        Dim df As DataFrame = x.AsDataFrame

    End Sub
End Module
