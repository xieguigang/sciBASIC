Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions

Namespace Html

    ''' <summary>
    ''' HTML 文本处理相关的辅助函数
    ''' </summary>
    Public Module HtmlText

        ''' <summary>
        ''' 空白字符折叠用的正则(包含不换行空格)
        ''' </summary>
        Private ReadOnly blanks As New Regex("[\s ]+", RegexOptions.Compiled)

        ''' <summary>
        ''' 将一个节点之内的内容抽取为纯文本
        ''' </summary>
        <Extension>
        Public Function ToPlainText(node As HtmlNode, Optional preserveBlocks As Boolean = False) As String
            If node Is Nothing Then
                Return ""
            End If

            Return node.GetText(preserveBlocks)
        End Function

        ''' <summary>
        ''' 将文本之中的连续空白字符折叠为单个空格
        ''' </summary>
        <Extension>
        Public Function CollapseWhiteSpace(text As String) As String
            Return Normalize(text, preserveBlocks:=False)
        End Function

        ''' <summary>
        ''' 文本归一化：实体解码 + 空白折叠
        ''' </summary>
        ''' <param name="text">原始文本</param>
        ''' <param name="preserveBlocks">
        ''' 为 <c>True</c> 的时候保留换行符，按行做两端空白的裁剪；
        ''' 为 <c>False</c> 的时候所有的空白都会被折叠为单个空格，返回单行文本。
        ''' </param>
        Friend Function Normalize(text As String, preserveBlocks As Boolean) As String
            If String.IsNullOrEmpty(text) Then
                Return ""
            End If

            text = HtmlEntity.Decode(text)

            If preserveBlocks Then
                Dim lines As New List(Of String)()

                For Each line As String In text.Split(New String() {vbCrLf, vbLf, vbCr}, StringSplitOptions.None)
                    line = blanks.Replace(line.Replace(vbTab, " "), " ").Trim()

                    If line.Length > 0 Then
                        Call lines.Add(line)
                    End If
                Next

                Return String.Join(vbLf, lines)
            End If

            Return blanks.Replace(text, " ").Trim()
        End Function

        ''' <summary>
        ''' 判断一个链接地址是否为有效的、可以安全访问的链接
        ''' </summary>
        ''' <remarks>
        ''' 会过滤掉空链接以及 ``javascript:void(0);`` 这类脚本伪链接。
        ''' </remarks>
        Public Function IsSafeUrl(url As String) As Boolean
            If String.IsNullOrWhiteSpace(url) Then
                Return False
            End If

            url = url.Trim()

            If url.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) Then
                Return False
            End If
            If url.StartsWith("void(", StringComparison.OrdinalIgnoreCase) Then
                Return False
            End If
            If url = "#" OrElse url.StartsWith("#") Then
                Return False
            End If
            If url.StartsWith("about:blank", StringComparison.OrdinalIgnoreCase) Then
                Return False
            End If
            If url.StartsWith("data:", StringComparison.OrdinalIgnoreCase) Then
                Return False
            End If

            Return True
        End Function

        ''' <summary>
        ''' 将一个可能是相对路径的链接地址转换为绝对地址
        ''' </summary>
        ''' <param name="href">原始的链接地址，允许包含 HTML 字符实体</param>
        ''' <param name="baseUrl">基准地址，用于补全相对路径</param>
        ''' <returns>
        ''' 绝对地址；如果 <paramref name="href"/> 是一个不安全的伪链接，则返回空字符串；
        ''' 如果 <paramref name="baseUrl"/> 为空并且 <paramref name="href"/> 本身就是一个相对路径，
        ''' 则原样返回 <paramref name="href"/>。
        ''' </returns>
        Public Function AbsoluteUrl(href As String, baseUrl As String) As String
            If String.IsNullOrWhiteSpace(href) Then
                Return ""
            End If

            href = HtmlEntity.Decode(href.Trim())

            If Not IsSafeUrl(href) Then
                Return ""
            End If

            Try
                Dim u As New Uri(href, UriKind.RelativeOrAbsolute)

                If u.IsAbsoluteUri Then
                    Return u.AbsoluteUri
                End If

                If String.IsNullOrWhiteSpace(baseUrl) Then
                    Return href
                End If

                Return New Uri(New Uri(baseUrl, UriKind.Absolute), href).AbsoluteUri
            Catch ex As Exception
                Return href
            End Try
        End Function

        ''' <summary>
        ''' 将一段 HTML 文本之中的所有的相对链接补全为绝对链接
        ''' </summary>
        ''' <param name="html">HTML 文本</param>
        ''' <param name="baseUrl">基准地址</param>
        <Extension>
        Public Function FixRelativeUrls(html As String, baseUrl As String) As String
            If String.IsNullOrEmpty(html) OrElse String.IsNullOrWhiteSpace(baseUrl) Then
                Return html
            End If

            Dim base As Uri

            Try
                base = New Uri(baseUrl, UriKind.Absolute)
            Catch ex As Exception
                Return html
            End Try

            Return Regex.Replace(html, "(?is)(href|src)\s*=\s*(""|')([^""']+)(""|')",
                Function(m As Match)
                    Dim org As String = m.Value
                    Dim link As String = HtmlEntity.Decode(m.Groups(3).Value.Trim())

                    If Not IsSafeUrl(link) Then
                        Return org
                    End If

                    Try
                        Dim full As String = New Uri(base, link).AbsoluteUri
                        Return $"{m.Groups(1).Value}={m.Groups(2).Value}{full}{m.Groups(4).Value}"
                    Catch ex As Exception
                        Return org
                    End Try
                End Function)
        End Function

        ''' <summary>
        ''' 使用多个分隔符之中的任意一个来切分文本，自动的去掉空白项
        ''' </summary>
        ''' <param name="text">待切分的文本</param>
        ''' <param name="separators">分隔符列表，例如中文的 ``·`` 或者英文的 ``|``</param>
        <Extension>
        Public Function SplitBy(text As String, ParamArray separators As String()) As String()
            If String.IsNullOrWhiteSpace(text) Then
                Return New String() {}
            End If
            If separators Is Nothing OrElse separators.Length = 0 Then
                Return New String() {text.Trim()}
            End If

            Dim parts As New List(Of String)() From {text}

            For Each sep As String In separators
                Dim buffer As New List(Of String)()

                For Each part As String In parts
                    Call buffer.AddRange(part.Split(New String() {sep}, StringSplitOptions.None))
                Next

                parts = buffer
            Next

            Return parts _
                .Select(Function(s) s.Trim()) _
                .Where(Function(s) s.Length > 0) _
                .ToArray()
        End Function

        ''' <summary>
        ''' 去掉文本两端的成对引号
        ''' </summary>
        <Extension>
        Public Function TrimQuote(text As String) As String
            If String.IsNullOrEmpty(text) Then
                Return ""
            End If

            Return text.Trim().Trim(""""c, "'"c, ChrW(&H201C), ChrW(&H201D), ChrW(&H2018), ChrW(&H2019))
        End Function
    End Module

End Namespace
