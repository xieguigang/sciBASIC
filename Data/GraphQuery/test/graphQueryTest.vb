#Region "Microsoft.VisualBasic::08f34339fd7389e12ca9f1916a70e176, Data\GraphQuery\test\graphQueryTest.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 156
    '    Code Lines: 114 (73.08%)
    ' Comment Lines: 2 (1.28%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 40 (25.64%)
    '     File Size: 4.59 KB


    ' Module graphQueryTest
    ' 
    '     Sub: BookTest, Main, simpleArrayTest, simpleParserTest, SimpleTest
    ' 
    ' Class QueryData
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.GraphQuery
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.MIME.application.json.Javascript

Module graphQueryTest

    Sub Main()
        Call simpleParserTest()

        Call simpleArrayTest()

        Call SimpleTest()

        Call BookTest()

        Pause()
    End Sub

    Sub simpleParserTest()

        Call QueryParser.GetQuery("value css('Value') | css('String, Number, Boolean') [
            text()
        ]")

        Call QueryParser.GetQuery("reaction css('a', '*') [text()]")

    End Sub

    Sub BookTest()

        Dim queryText = "
# https://www.codeproject.com/Articles/1264613/GraphQuery-Powerful-Text-Query-Language-3

graphquery
{
    # parser function pipeline can be 
    # in different line,
    # this will let you write graphquery
    # code in a more graceful style when
    # you needs a lot of pipeline function
    # for parse value data.
    bookID    css('book') 
            | attr('id')

    title     css('title')
    isbn      xpath('//isbn')
    quote     css('quote')
    language  css('title') | attr('lang')

    # another sub query in current graph query
    author css('author') {
        name css('name')
        born css('born')
        dead css('dead')
    }

    # this is a array of type character
    character xpath('//character') [{
        name          css('name')
        born          css('born')
        qualification xpath('qualification')
    }]
}"
        Dim query As Query = QueryParser.GetQuery(queryText)
        Dim engine As New Engine
        Dim doc = <library>
                      <!-- Great book. -->
                      <book id="b0836217462" available="true">
                          <isbn>0836217462</isbn>
                          <title lang="en">Being a Dog Is a Full-Time Job</title>
                          <quote>I'd dog paddle the deepest ocean.</quote>
                          <author id="CMS">
                              <name>Charles M Schulz</name>
                              <born>1922-11-26</born>
                              <dead>2000-02-12</dead>
                          </author>
                          <character id="PP">
                              <name>Peppermint Patty</name>
                              <born>1966-08-22</born>
                              <qualification>bold, brash and tomboyish</qualification>
                          </character>
                          <character id="Snoopy">
                              <name>Snoopy</name>
                              <born>1950-10-04</born>
                              <qualification>extroverted beagle</qualification>
                          </character>
                      </book>
                  </library>
        Dim data As JsonElement = engine.Execute(doc, query)
        Dim json_str As String = data.BuildJsonString(New JSONSerializerOptions With {.indent = True})

        Call Console.WriteLine(json_str)
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
