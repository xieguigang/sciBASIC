

Imports Microsoft.VisualBasic.Data.GraphQuery
Imports Microsoft.VisualBasic.Data.GraphQuery.Language

Module graphQueryTest

    Sub Main()

        Dim queryTokens = New TokenIcer("E:\GCModeller\src\runtime\sciBASIC#\Data\data\query.ql".ReadAllText).GetTokens.ToArray

        Pause()

    End Sub

End Module


Public Class Query


End Class