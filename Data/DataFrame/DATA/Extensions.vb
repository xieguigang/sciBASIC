Imports System.IO
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.csv.IO

Namespace DATA

    <HideModuleName>
    Public Module Extensions

        Public Sub ProjectLargeDataFrame(targetFile$, columns As IEnumerable(Of String), output As TextWriter)
            Dim headers As Index(Of String) = Tokenizer.CharsParser(targetFile.ReadFirstLine)
            Dim index As Integer() = headers.GetOrdinal(columns)
            Dim row As RowObject

            row = New RowObject(index.Select(Function(i) headers(i)))
            output.WriteLine(row.AsLine)

            For Each line As String In targetFile.IterateAllLines.Skip(1)
                row = Tokenizer.CharsParser(line)
                row = row.Takes(index)
                output.WriteLine(row.AsLine)
            Next

            Call output.Flush()
        End Sub
    End Module
End Namespace