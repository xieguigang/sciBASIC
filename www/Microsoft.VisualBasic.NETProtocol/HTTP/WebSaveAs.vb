#Region "Microsoft.VisualBasic::15ca7773e0f046ab2ff0c7fe64012b57, www\Microsoft.VisualBasic.NETProtocol\HTTP\WebSaveAs.vb"

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

    '   Total Lines: 84
    '    Code Lines: 50 (59.52%)
    ' Comment Lines: 17 (20.24%)
    '    - Xml Docs: 88.24%
    ' 
    '   Blank Lines: 17 (20.24%)
    '     File Size: 2.84 KB


    '     Module WebSaveAs
    ' 
    '         Function: SaveAs
    ' 
    '         Sub: SaveFile
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Text.Parser.HtmlParser

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
                .ToArray(AddressOf Href)

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
