Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.MIME.Markup.HTML

Namespace HTTP

    ''' <summary>
    ''' 只会保存当前页面的所有内容到指定的文件夹之中，只会保存js，css图像等其他非链接的对象
    ''' </summary>
    Public Module WebSaveAs

        ''' <summary>
        ''' Example as:
        ''' 
        ''' ```html
        ''' &lt;link rel="stylesheet" href="/vendor/source-code-pro/styles.css">
        ''' ```
        ''' </summary>
        Const CSSLink As String = "<link .*?href=""[^""]+"".*?>"

        ''' <summary>
        ''' 这个函数只支持静态内容的抓取
        ''' </summary>
        ''' <param name="url$"></param>
        ''' <param name="DIR$"></param>
        ''' <returns></returns>
        Public Function SaveAs(url$, DIR$) As Boolean
            Dim html As String = url.GET
            Dim files$() = Regex _
                .Matches(html, "src=""[^""]+""", RegexICSng) _
                .ToArray(Function(s) s.GetStackValue("""", """"))
            Dim currentDIR = WebCrawling.GetCurrentFolder(url)
            Dim root As String = GetRootPath(url)

            Call SaveFile(url, DIR)

            Dim csslinks$() = Regex _
                .Matches(html, CSSLink, RegexICSng) _
                .ToArray(AddressOf href)

            For Each file As String In files.Join(csslinks).OrderByDescending(Function(s) s.Length)
                If InStr(file, "http://", CompareMethod.Text) = 1 OrElse
                    InStr(file, "https://", CompareMethod.Text) = 1 OrElse
                    InStr(file, "//") = 1 OrElse
                    InStr(file, "ftp://") = 1 Then

                    Continue For
                End If

                ' 还需要进行替换

                If file.First = "/"c Then  ' 根目录
                    url = root & file
                Else
                    If InStr(file, "./") = 1 Then
                        file = Mid(file, 3)
                    End If

                    url = currentDIR & file
                End If

                Call SaveFile(url, DIR)
            Next

            Return True
        End Function

        Private Sub SaveFile(url$, Downloads$)
            Dim path$, tokens$()
            Dim rawLink = url

            url = Strings.Split(url, "://").Last
            tokens = url.Split("/"c)
            url = tokens.Take(tokens.Length - 1).JoinBy("/")
            url = url & "/" & tokens.Last.Replace("*", "+").Replace("?", "+").Replace(":", "-")
            path = Downloads & "/" & url

            If path.Last = "/"c Then
                path &= "index.html"
            End If

            Call rawLink.DownloadFile(path)
        End Sub
    End Module
End Namespace