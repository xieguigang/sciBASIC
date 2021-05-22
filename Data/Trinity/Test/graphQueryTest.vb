Imports Microsoft.VisualBasic.Data.GraphQuery
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript
Imports Microsoft.VisualBasic.MIME.Html.Document

Module graphQueryTest

    Sub Main()
        Call complextest()

        Call simpleArrayTest()

        Call SimpleTest()

        Call BookTest()

        Pause()

    End Sub

    Sub complextest()
        Dim queryText As String = "E:\GCModeller\src\repository\graphquery\kegg\kegg_table.graphquery".ReadAllText
        Dim query As Query = QueryParser.GetQuery(queryText)
        Dim engine As New Engine
        Dim doc As HtmlDocument = HtmlDocument.LoadDocument("E:\GCModeller\src\repository\graphquery\kegg\gene.html")
        Dim data As JsonElement = engine.Execute(doc, query)

        Call Console.WriteLine(data.BuildJsonString(New JSONSerializerOptions With {.indent = True}))

        Pause()
    End Sub

    Sub BookTest()
        ' Dim queryTokens = New TokenIcer("E:\GCModeller\src\runtime\sciBASIC#\Data\data\query.ql".ReadAllText).GetTokens.ToArray
        Dim queryText As String = "E:\GCModeller\src\runtime\sciBASIC#\Data\data\query.ql".ReadAllText
        Dim query As Query = QueryParser.GetQuery(queryText)
        Dim engine As New Engine
        Dim doc As HtmlDocument = HtmlDocument.LoadDocument("E:\GCModeller\src\runtime\sciBASIC#\Data\data\query.html")
        Dim data As JsonElement = engine.Execute(doc, query)

        Call Console.WriteLine(data.BuildJsonString(New JSONSerializerOptions With {.indent = True}))
    End Sub

    Sub SimpleTest()

        Dim document = <div>
                           <a href="1.html">anchor 1</a>
                           <a href="2.html">anchor 2</a>
                           <a href="3.html">anchor 3</a>
                       </div>
        Dim query As Query = QueryParser.GetQuery("

            a css('a') [{
                title  text() | trim() 
                url    attr('href') 
            }]

        ")

        Dim data As JsonElement = New Engine().Execute(document, query)
        Dim json As String = data.BuildJsonString(indent:=True)

        Call Console.WriteLine(json)

        '  Pause()
    End Sub

    Sub simpleArrayTest()

        Dim document = <html>

                           <body>
                               <a href="01.html">Page 1</a>
                               <a href="02.html">Page 2</a>
                               <a href="03.html">Page 3</a>
                           </body>

                       </html>

        Dim query As Query = QueryParser.GetQuery("
            
            graphquery { 
            
                anchor css('a') [ 
                    text() 
                ] 
            }

        ")

        Dim data As JsonElement = New Engine().Execute(document, query)
        Dim json As String = data.BuildJsonString()

        Call Console.WriteLine(json)

        '  Pause()
    End Sub

End Module


Public Class QueryData


End Class