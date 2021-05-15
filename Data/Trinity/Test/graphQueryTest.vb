

Imports Microsoft.VisualBasic.Data.GraphQuery
Imports Microsoft.VisualBasic.Data.GraphQuery.Language
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.MIME.Markup.HTML

Module graphQueryTest

    Sub Main()

        ' Dim queryTokens = New TokenIcer("E:\GCModeller\src\runtime\sciBASIC#\Data\data\query.ql".ReadAllText).GetTokens.ToArray
        Dim queryText As String = "E:\GCModeller\src\runtime\sciBASIC#\Data\data\query.ql".ReadAllText
        Dim query As Query = QueryParser.GetQuery(queryText)
        Dim engine As New Engine
        Dim doc As HtmlDocument = HtmlDocument.LoadDocument("E:\GCModeller\src\runtime\sciBASIC#\Data\data\query.html")
        Dim data As JsonElement = engine.Execute(doc, query)

        Pause()

    End Sub

End Module


Public Class QueryData


End Class