Imports Microsoft.VisualBasic.Data.Framework
Imports Microsoft.VisualBasic.Text
Imports xlsxFile = Microsoft.VisualBasic.MIME.Office.Excel.XLSX.File

Public Module read

    Public Function csv(file As String) As DataFrame
        Return DataFrame.read_csv(file)
    End Function

    Public Function tsv(file As String) As DataFrame
        Return DataFrame.read_csv(file, delimiter:=ASCII.TAB)
    End Function

    Public Function arff(file As String) As DataFrame
        Return DataFrame.read_arff(file)
    End Function

    Public Function xlsx(file As String, sheet As String) As DataFrame
        Dim pkg As xlsxFile = xlsxFile.Open(file)
        Dim tbl = pkg.GetTable(sheetName:=sheet)
        Dim df As DataFrame = tbl.as_dataframe

        Return df
    End Function

    Public Function xlsx(file As String, sheet As Integer) As DataFrame
        Dim pkg As xlsxFile = xlsxFile.Open(file)
        Dim tbl = pkg.GetTable(index:=sheet)
        Dim df As DataFrame = tbl.as_dataframe

        Return df
    End Function

End Module
