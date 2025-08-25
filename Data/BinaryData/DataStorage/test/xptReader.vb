Imports Microsoft.VisualBasic.Data.IO.Xpt
Imports Microsoft.VisualBasic.Serialization.JSON

Module xptReader

    Const testfile = "G:\pixelArtist\src\framework\Data\data\ALQ_H.xpt"
    Const test2222 = "G:\pixelArtist\src\framework\Data\data\test.xpt"

    Sub Main()
        Dim converter As SASXportConverter = New SASXportConverter(testfile)
        converter.Dispose()

        Dim iterator As SASXportFileIterator = New SASXportFileIterator(test2222)
        While iterator.hasNext()
            Dim row As IList(Of String) = iterator.next()
            Call Console.WriteLine(row.GetJson)
        End While
        Console.WriteLine("Total Rows: " & iterator.RowCount.ToString())
        iterator.Dispose()

        Dim cal As Date = New DateTime()
        cal = New DateTime(1960, 1, 1)
        cal.AddDays(19778)
        Console.WriteLine(cal.ToString())
    End Sub
End Module
