Imports System.IO
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX
Imports Microsoft.VisualBasic.MIME.Office.Excel.XLSX.Writer

Public Module write

    Public Sub csv(x As NumericTable, file As String)
        Call x.WriteCsv(file)
    End Sub

    Public Sub tsv(x As NumericTable, file As String)
        Call x.WriteTsv(file)
    End Sub

    Public Sub arff(x As DataFrame, file As String)
        Using s As Stream = file.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
            Call DataFrame.write_arff(x, s)
        End Using
    End Sub

    Public Sub xlsx(x As NumericTable, file As String, sheetName As String)
        Dim df As DataFrame = x.AsDataFrame
        Dim wb As New Workbook(file, sheetName)

        Call wb.WriteSheetTable(df)
        Call wb.Save()
    End Sub

    Public Sub cdf(x As NumericTable, file As String)

    End Sub
End Module
