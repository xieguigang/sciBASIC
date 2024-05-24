Imports System
Imports Microsoft.VisualBasic.Data.FullTextSearch
Imports Microsoft.VisualBasic.Linq

Module Program

    ReadOnly demo_repo As String = $"{App.HOME}/demo_data/"

    Sub Main(args As String())
        Call test_create()
        Call test_read()
    End Sub

    Sub test_read()
        Dim index As New FTSEngine(demo_repo)

        For Each hit As SeqValue(Of String) In index.Search("princess  walking")
            Call Console.WriteLine(hit.value)
        Next
    End Sub

    Sub test_create()
        Dim doc_1 = "E:\GCModeller\src\runtime\sciBASIC#\Data\TextRank\Beauty_and_the_Beast.txt".ReadAllLines
        Dim doc_2 = "E:\GCModeller\src\runtime\sciBASIC#\Data\TextRank\Rapunzel.txt".ReadAllLines

        Dim index As New FTSEngine(demo_repo)

        Call index.Indexing(doc_1)
        Call index.Indexing(doc_2)

        'For Each hit In index.Search("baby girl")
        '    Call Console.WriteLine(hit.value)
        'Next

        For Each hit As SeqValue(Of String) In index.Search("princess  walking")
            Call Console.WriteLine(hit.value)
        Next
    End Sub
End Module
