

Imports Microsoft.VisualBasic.Data.GraphQuery
Imports Microsoft.VisualBasic.Data.GraphQuery.Language

Module graphQueryTest

    Sub Main()

        ' Dim queryTokens = New TokenIcer("E:\GCModeller\src\runtime\sciBASIC#\Data\data\query.ql".ReadAllText).GetTokens.ToArray
        Dim queryText As String = "E:\GCModeller\src\runtime\sciBASIC#\Data\data\query.ql".ReadAllText
        Dim query As Query = QueryParser.GetQuery(queryText)

        Pause()

    End Sub

End Module


Public Class QueryData


End Class