Imports Microsoft.VisualBasic.Data.csv.IO

Module fileParser2

    Sub Main()

        ' Call multipleLineRowtest1()

        Dim df As DataFrame = DataFrame.Load("E:\GCModeller\src\runtime\sciBASIC#\Data\DataFrame\test\Food.csv", simpleRowIterators:=False)

        Pause()
    End Sub

    Sub multipleLineRowtest1()
        Dim reader As New RowIterator("E:\GCModeller\src\runtime\sciBASIC#\Data\DataFrame\test\single_row.csv".OpenReadonly)
        Dim r As RowObject() = reader.GetRows.ToArray

        Pause()
    End Sub

End Module
