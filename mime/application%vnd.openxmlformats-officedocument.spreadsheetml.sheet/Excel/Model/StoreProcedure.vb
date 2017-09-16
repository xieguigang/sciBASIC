Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.Excel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Office.Excel.XML.xl
Imports Microsoft.VisualBasic.MIME.Office.Excel.XML.xl.worksheets
Imports csv = Microsoft.VisualBasic.Data.csv.IO.File

''' <summary>
''' 读取工作表的数据或者生成工作表数据的过程
''' </summary>
Public Module StoreProcedure

    <Extension>
    Public Function ToTableFrame(worksheet As worksheet, strings As sharedStrings) As csv
        Dim getValues As IEnumerable(Of RowObject) =
            worksheet _
                .sheetData _
                .rows _
                .Select(Function(r) r.ToRowData(strings))
        Dim csv As New csv(getValues)
        Return csv
    End Function

    <Extension>
    Private Function ToRowData(row As row, strings As sharedStrings) As RowObject
        Dim csv As New RowObject
        Dim string$
        Dim colIndex%
        Dim cellIndex As Point

        For Each col As c In row.columns
            cellIndex = Coordinates.Index(col.r)
            colIndex = cellIndex.Y ' 因为这里都是同一行的数据，所以只取列下标即可

            With col.sharedStringsRef
                If .ref = -1 Then
                    [string] = col.v
                Else
                    [string] = strings.strings(.ref).t
                End If
            End With

            csv(colIndex - 1) = [string]
        Next

        Return csv
    End Function
End Module
