Imports Microsoft.VisualBasic.Data.Trinity

Module indexTest

    Sub Main()
        Dim index As New WordSimilarityIndex(Of String)

        For Each item As String In 5000.SeqRandom.Select(Function(l) RandomASCIIString(20, True))
            If Not index.HaveKey(item) Then
                Call index.AddTerm(item, item)
            End If
        Next

        Dim result = index.FindMatches("Aaaaaaaaaaaaaaaaaaaa").ToArray

        Pause()
    End Sub
End Module
