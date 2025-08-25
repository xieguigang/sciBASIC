Imports Microsoft.VisualBasic.Data.IO.Xpt

Module xptReader

    Sub Main()
        Dim converter As SASXportConverter = New SASXportConverter("/Users/ravi1/Downloads/test.sasxpt")
        converter.Dispose()

        Dim iterator As SASXportFileIterator = New SASXportFileIterator("/grid/data/xpt/test3.sasxpt")
        While iterator.hasNext()
            Dim row As IList(Of String) = iterator.next()
        End While
        Console.WriteLine("Total Rows: " & iterator.RowCount.ToString())
        iterator.Dispose()

        Dim cal As Date = New DateTime()
        cal = New DateTime(1960, 1, 1)
        cal.AddDays(19778)
        Console.WriteLine(cal.ToString())
    End Sub
End Module
