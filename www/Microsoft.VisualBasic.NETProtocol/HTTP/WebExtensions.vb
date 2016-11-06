Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML

Namespace HTTP

    Public Module WebExtensions

        Const InvokeJavascript$ = "javascript:void(0);"

        <Extension>
        Public Function GetCurrentFolder(url As String) As String
            Dim len As Integer

            For Each c As SeqValue(Of Char) In url.SeqIterator
                If c.obj = "/"c Then
                    len = c.i
                End If
            Next

            Dim folder As String = Mid(url, 1, len + 1)
            Return folder
        End Function

        Public Function GetRootPath(url$) As String
            Dim protocol$ = Regex.Match(url, ".+?://").Value
            Dim root$ = Strings.Split(url, "://").Last.GetTagValue("/").Name
            root = protocol & root
            Return root
        End Function

        ''' <summary>
        ''' Downloads all links on the target web page and save the files to directory specific by <paramref name="Downloads"/>
        ''' </summary>
        ''' <param name="url$"></param>
        ''' <param name="Downloads$">Directory for save the download contents.</param>
        ''' <returns>返回失败的页面的url</returns>
        <Extension>
        Public Iterator Function DownloadAllLinks(url$, Downloads$, Optional recursive As Boolean = False) As IEnumerable(Of String)
            For Each failed$ In __downloadAllLinks(url, Downloads, recursive, New Dictionary(Of String, String))
                Yield failed
            Next
        End Function

        Private Iterator Function __downloadAllLinks(url$, Downloads$, recursive As Boolean, visited As Dictionary(Of String, String)) As IEnumerable(Of String)
            Dim current$ = GetCurrentFolder(url)
            Dim root As String = GetRootPath(url)

            If visited.ContainsKey(url) Then
                Return
            Else
                Call visited.Add(url, Nothing)
            End If

            Dim page As String = url.GET(DoNotRetry404:=True)
            Dim links$() = Regex _
                .Matches(page, "<a .*?href="".+?"".*?>", RegexICSng) _
                .ToArray(AddressOf href)

            For Each link$ In links.Where(Function(s) Not s.TextEquals(InvokeJavascript))
                If link.IsFullURL OrElse InStr(link, "//") = 1 Then
                    url = link
                Else
                    If link.First = "/"c Then  ' 根目录
                        url = root & link
                    Else
                        If InStr(link, "./") = 1 Then
                            link = Mid(link, 3)
                        End If

                        url = current & link
                    End If
                End If

                For Each failed$ In __parsePage(url, Downloads, recursive, visited)
                    Yield failed
                Next
            Next
        End Function

        Private Iterator Function __parsePage(url$, Downloads$, recursive As Boolean, visited As Dictionary(Of String, String)) As IEnumerable(Of String)
            If recursive Then
                For Each failed$ In DownloadAllLinks(url, Downloads, recursive)
                    Yield failed
                Next
            Else
                Dim page As String = url.GET(DoNotRetry404:=True)

                If String.IsNullOrEmpty(page) Then
                    Yield url
                Else
                    Dim path$, tokens$()

                    url = Strings.Split(url, "://").Last
                    tokens = url.Split("/"c)
                    url = tokens.Take(tokens.Length - 1).JoinBy("/")
                    url = url & "/" & tokens.Last.NormalizePathString
                    path = Downloads & "/" & url

                    If path.Last = "/"c Then
                        path &= "index.html"
                    End If

                    Call page.SaveTo(path, Encoding.UTF8)
                End If
            End If
        End Function
    End Module
End Namespace