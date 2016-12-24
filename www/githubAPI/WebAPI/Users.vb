Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Webservices.Github.Class
Imports Microsoft.VisualBasic.Language

Namespace WebAPI

    Public Module Users

        Public Const Github$ = "https://github.com"

        <Extension> Public Function Followers(username As String) As User()
            Dim url As String = Github & "/{0}?page={1}&tab=followers"
            Return ParserIterator(url, username)
        End Function

        Private Function ParserIterator(url$, username$) As User()
            Dim out As New List(Of User)
            Dim i As int = 1
            Dim [get] As New Value(Of User())

            Do While Not ([get] = ParserInternal(username, ++i, url)).IsNullOrEmpty
                out += (+[get])
            Loop

            Return out
        End Function

        Private Function ParserInternal(user$, page%, url$) As User()
            Dim html$

            Const Splitter$ = "<div class=""js-repo-filter position-relative"">"

            url = String.Format(url, user, page)
            html = url.GET
            html = Strings.Split(html, Splitter).Last

        End Function

        <Extension> Public Function Following(username As String) As User()
            Dim url As String = Github & "/{0}?page={1}&tab=following"
            Return ParserIterator(url, username)
        End Function
    End Module
End Namespace