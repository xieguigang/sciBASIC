#Region "Microsoft.VisualBasic::dac9f7ec22e7e7e84cf9f7af868c310f, www\Microsoft.VisualBasic.NETProtocol\HTTP\WebCrawling.vb"

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

    '   Total Lines: 173
    '    Code Lines: 138
    ' Comment Lines: 9
    '   Blank Lines: 26
    '     File Size: 6.98 KB


    '     Module WebCrawling
    ' 
    '         Function: __downloadAllLinks, __parsePage, DownloadAllLinks, GetCurrentFolder, GetRootPath
    ' 
    '         Sub: SavePage
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser

Namespace HTTP

    ''' <summary>
    ''' 只会爬取HTML
    ''' </summary>
    Public Module WebCrawling

        Const InvokeJavascript$ = "javascript:void(0);"

        <Extension>
        Public Function GetCurrentFolder(url As String) As String
            Dim len As Integer

            For Each c As SeqValue(Of Char) In url.SeqIterator
                If c.value = "/"c Then
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
        Public Iterator Function DownloadAllLinks(url$,
                                                  Downloads$,
                                                  Optional recursive As Boolean = False,
                                                  Optional ignores$() = Nothing,
                                                  Optional sleeps As Integer = 1000) As IEnumerable(Of String)

            For Each failed$ In __downloadAllLinks(url,
                                                   Downloads,
                                                   recursive,
                                                   New Dictionary(Of String, String),
                                                   If(ignores Is Nothing, {}, ignores),
                                                   sleeps)
                Yield failed
            Next
        End Function

        Private Iterator Function __downloadAllLinks(url$,
                                                     Downloads$,
                                                     recursive As Boolean,
                                                     visited As Dictionary(Of String, String),
                                                     ignoresUrls$(),
                                                     sleeps As Integer) As IEnumerable(Of String)
            Dim current$ = GetCurrentFolder(url)
            Dim root As String = GetRootPath(url)

            If visited.ContainsKey(url) Then
                Call $"Skip visited {url}".Warning
                Return
            Else
                Call visited.Add(url, Nothing)
            End If

            For Each key$ In ignoresUrls
                If InStr(url, key, CompareMethod.Text) = 1 Then
                    Return
                End If
            Next

            Dim page As String = url.GET(doNotRetry404:=True)
            Dim links$() = Regex _
                .Matches(page, "<a .*?href="".+?"".*?>", RegexICSng) _
                .ToArray(AddressOf href)

            Call SavePage(page, url, Downloads)
            Call Threading.Thread.Sleep(sleeps)

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

                Dim skips As Boolean = False

                For Each key$ In ignoresUrls
                    If InStr(url, key, CompareMethod.Text) = 1 Then
                        skips = True
                        Continue For
                    End If
                Next

                If skips Then
                    Continue For
                Else  ' 假若链接都是指向同一个服务器，需要在这里休眠一段时间降低对方服务器的压力
                    Call Threading.Thread.Sleep(sleeps)
                End If

                For Each failed$ In __parsePage(url,
                                                Downloads,
                                                recursive,
                                                visited,
                                                ignoresUrls,
                                                sleeps)
                    Yield failed
                Next
            Next
        End Function

        Private Sub SavePage(page$, url$, Downloads$)
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
        End Sub

        Private Iterator Function __parsePage(url$,
                                              Downloads$,
                                              recursive As Boolean,
                                              visited As Dictionary(Of String, String),
                                              ignoresUrls$(),
                                              sleeps As Integer) As IEnumerable(Of String)
            If recursive Then
                For Each failed$ In __downloadAllLinks(url,
                                                       Downloads,
                                                       recursive,
                                                       visited,
                                                       ignoresUrls,
                                                       sleeps)
                    Yield failed
                Next
            Else
                Dim page As String = url.GET(doNotRetry404:=True)

                If String.IsNullOrEmpty(page) Then
                    Yield url
                Else
                    Call SavePage(page, url, Downloads)
                End If
            End If
        End Function
    End Module
End Namespace
