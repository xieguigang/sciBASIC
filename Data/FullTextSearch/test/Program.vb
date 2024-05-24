Imports System
Imports Microsoft.VisualBasic.Data.FullTextSearch

Module Program

    Sub Main(args As String())
        Dim doc = "E:\GCModeller\src\runtime\sciBASIC#\Data\TextRank\Beauty_and_the_Beast.txt".ReadAllLines
        Dim index As New InvertedIndex

        Call index.Add(doc)

        For Each hit In index.Search("baby girl")
            Call Console.WriteLine(hit.value)
        Next
    End Sub
End Module
