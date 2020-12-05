Imports Microsoft.VisualBasic.Linq
Imports test.Tester

Module test2

    Sub Main()
        Dim data As New List(Of LabelledVector)

        For Each row In "E:\GCModeller\src\runtime\sciBASIC#\Data_science\DataMining\data\umap\data.csv".IterateAllLines.SeqIterator
            data.Add(New LabelledVector With {.UID = row.i, .Vector = row.value.Split(","c).Select(Function(str) CSng(Val(str))).ToArray})
        Next

        Call Tester.Program.RunTest(data.ToArray)

        Pause()
    End Sub
End Module
