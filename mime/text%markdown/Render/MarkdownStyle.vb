''' <summary>
''' markdown html 主题样式：一段 CSS 文本 + 承载样式的 body class。
''' </summary>
''' <remarks>
''' 该对象只描述样式，不含任何渲染逻辑，通过
''' <c>New MarkdownRender(New HtmlRender(MarkdownStyle.GitHub))</c> 交给
''' <see cref="HtmlRender"/> 使用；<see cref="HtmlRender.Document"/> 会把
''' CSS 直接写进输出的 html 片段（&lt;style&gt; 块），因此不需要额外的样式文件。
''' </remarks>
Public Class MarkdownStyle

    ''' <summary>主题名称（仅作标识）。</summary>
    Public Property Name As String = "default"

    ''' <summary>承载主题样式的 body class，默认 <c>markdown-body</c>。</summary>
    Public Property BodyClass As String = "markdown-body"

    ''' <summary>主题 CSS 文本（全部选择器以 <see cref="BodyClass"/> 为根）。</summary>
    Public Property Css As String = ""

    ''' <summary>复制一份样式对象。</summary>
    ''' <returns></returns>
    Public Function Clone() As MarkdownStyle
        Return New MarkdownStyle With {
            .Name = Name,
            .BodyClass = BodyClass,
            .Css = Css
        }
    End Function

    ''' <summary>
    ''' 自定义样式工厂。
    ''' </summary>
    ''' <param name="css">CSS 文本。</param>
    ''' <param name="name">主题名称。</param>
    ''' <param name="bodyClass">承载样式的 body class。</param>
    ''' <returns></returns>
    Public Shared Function Create(css As String,
                                  Optional name As String = "custom",
                                  Optional bodyClass As String = "markdown-body") As MarkdownStyle
        Return New MarkdownStyle With {
            .Name = name,
            .BodyClass = bodyClass,
            .Css = css
        }
    End Function

    ''' <summary>
    ''' GitHub 风格的浅色主题（推荐用于编辑器预览）。
    ''' </summary>
    ''' <returns></returns>
    Public Shared ReadOnly Property GitHub As MarkdownStyle
        Get
            Return _github
        End Get
    End Property

    ''' <summary>
    ''' 深色主题。
    ''' </summary>
    ''' <returns></returns>
    Public Shared ReadOnly Property Dark As MarkdownStyle
        Get
            Return _dark
        End Get
    End Property

    Private Shared ReadOnly _github As MarkdownStyle = Create(GitHubCss(), "github", "markdown-body")
    Private Shared ReadOnly _dark As MarkdownStyle = Create(DarkCss(), "dark", "markdown-body")

    Private Shared Function GitHubCss() As String
        Dim lines As String() = {
            ".markdown-body {",
            "  box-sizing: border-box;",
            "  max-width: 920px;",
            "  margin: 0 auto;",
            "  padding: 32px 24px 96px;",
            "  font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", ""Microsoft YaHei"", ""Noto Sans SC"", Helvetica, Arial, sans-serif;",
            "  font-size: 16px;",
            "  line-height: 1.75;",
            "  color: #1f2328;",
            "  background-color: #ffffff;",
            "  word-wrap: break-word;",
            "  -webkit-text-size-adjust: 100%;",
            "}",
            ".markdown-body > *:first-child { margin-top: 0 !important; }",
            ".markdown-body > *:last-child { margin-bottom: 0 !important; }",
            ".markdown-body h1, .markdown-body h2, .markdown-body h3,",
            ".markdown-body h4, .markdown-body h5, .markdown-body h6 {",
            "  margin: 24px 0 16px;",
            "  font-weight: 600;",
            "  line-height: 1.25;",
            "  scroll-margin-top: 16px;",
            "}",
            ".markdown-body h1 { font-size: 2em; padding-bottom: .3em; border-bottom: 1px solid #d8dee4; }",
            ".markdown-body h2 { font-size: 1.5em; padding-bottom: .3em; border-bottom: 1px solid #d8dee4; }",
            ".markdown-body h3 { font-size: 1.25em; }",
            ".markdown-body h4 { font-size: 1em; }",
            ".markdown-body h5 { font-size: .875em; }",
            ".markdown-body h6 { font-size: .85em; color: #59636e; }",
            ".markdown-body p, .markdown-body blockquote, .markdown-body ul,",
            ".markdown-body ol, .markdown-body dl, .markdown-body table, .markdown-body pre {",
            "  margin: 0 0 16px;",
            "}",
            ".markdown-body a { color: #0969da; text-decoration: none; }",
            ".markdown-body a:hover { text-decoration: underline; }",
            ".markdown-body strong { font-weight: 600; }",
            ".markdown-body ul, .markdown-body ol { padding-left: 2em; }",
            ".markdown-body li + li { margin-top: .25em; }",
            ".markdown-body blockquote {",
            "  padding: 0 1em;",
            "  color: #59636e;",
            "  border-left: .25em solid #d0d7de;",
            "}",
            ".markdown-body blockquote > :first-child { margin-top: 0; }",
            ".markdown-body blockquote > :last-child { margin-bottom: 0; }",
            ".markdown-body hr {",
            "  height: .25em;",
            "  padding: 0;",
            "  margin: 24px 0;",
            "  background-color: #d0d7de;",
            "  border: 0;",
            "}",
            ".markdown-body img { max-width: 100%; box-sizing: content-box; background-color: #ffffff; }",
            ".markdown-body code, .markdown-body tt {",
            "  padding: .2em .4em;",
            "  font-size: 85%;",
            "  white-space: break-spaces;",
            "  background-color: #eff1f3;",
            "  border-radius: 6px;",
            "  font-family: ui-monospace, SFMono-Regular, ""Cascadia Mono"", Consolas, ""Liberation Mono"", monospace;",
            "}",
            ".markdown-body pre {",
            "  padding: 16px;",
            "  overflow: auto;",
            "  font-size: 85%;",
            "  line-height: 1.45;",
            "  background-color: #f6f8fa;",
            "  border-radius: 6px;",
            "}",
            ".markdown-body pre code {",
            "  padding: 0;",
            "  margin: 0;",
            "  font-size: 100%;",
            "  white-space: pre;",
            "  background-color: transparent;",
            "  border: 0;",
            "}",
            ".markdown-body table {",
            "  border-spacing: 0;",
            "  border-collapse: collapse;",
            "  display: block;",
            "  width: max-content;",
            "  max-width: 100%;",
            "  overflow: auto;",
            "}",
            ".markdown-body table th { font-weight: 600; }",
            ".markdown-body table th, .markdown-body table td {",
            "  padding: 6px 13px;",
            "  border: 1px solid #d0d7de;",
            "}",
            ".markdown-body table tr { background-color: #ffffff; border-top: 1px solid #d8dee4; }",
            ".markdown-body table tr:nth-child(2n) { background-color: #f6f8fa; }",
            ".markdown-body .task-list { list-style: none; padding-left: 1.4em; }",
            ".markdown-body input[type=""checkbox""] { margin-right: .4em; vertical-align: middle; }"
        }

        Return String.Join(vbLf, lines)
    End Function

    Private Shared Function DarkCss() As String
        Dim lines As String() = {
            ".markdown-body {",
            "  box-sizing: border-box;",
            "  max-width: 920px;",
            "  margin: 0 auto;",
            "  padding: 32px 24px 96px;",
            "  font-family: -apple-system, BlinkMacSystemFont, ""Segoe UI"", ""Microsoft YaHei"", ""Noto Sans SC"", Helvetica, Arial, sans-serif;",
            "  font-size: 16px;",
            "  line-height: 1.75;",
            "  color: #e6edf3;",
            "  background-color: #0d1117;",
            "  word-wrap: break-word;",
            "  -webkit-text-size-adjust: 100%;",
            "}",
            ".markdown-body > *:first-child { margin-top: 0 !important; }",
            ".markdown-body > *:last-child { margin-bottom: 0 !important; }",
            ".markdown-body h1, .markdown-body h2, .markdown-body h3,",
            ".markdown-body h4, .markdown-body h5, .markdown-body h6 {",
            "  margin: 24px 0 16px;",
            "  font-weight: 600;",
            "  line-height: 1.25;",
            "  scroll-margin-top: 16px;",
            "}",
            ".markdown-body h1 { font-size: 2em; padding-bottom: .3em; border-bottom: 1px solid #30363d; }",
            ".markdown-body h2 { font-size: 1.5em; padding-bottom: .3em; border-bottom: 1px solid #30363d; }",
            ".markdown-body h3 { font-size: 1.25em; }",
            ".markdown-body h4 { font-size: 1em; }",
            ".markdown-body h5 { font-size: .875em; }",
            ".markdown-body h6 { font-size: .85em; color: #9198a1; }",
            ".markdown-body p, .markdown-body blockquote, .markdown-body ul,",
            ".markdown-body ol, .markdown-body dl, .markdown-body table, .markdown-body pre {",
            "  margin: 0 0 16px;",
            "}",
            ".markdown-body a { color: #4493f8; text-decoration: none; }",
            ".markdown-body a:hover { text-decoration: underline; }",
            ".markdown-body strong { font-weight: 600; }",
            ".markdown-body ul, .markdown-body ol { padding-left: 2em; }",
            ".markdown-body li + li { margin-top: .25em; }",
            ".markdown-body blockquote {",
            "  padding: 0 1em;",
            "  color: #9198a1;",
            "  border-left: .25em solid #30363d;",
            "}",
            ".markdown-body hr { height: .25em; padding: 0; margin: 24px 0; background-color: #30363d; border: 0; }",
            ".markdown-body img { max-width: 100%; box-sizing: content-box; background-color: #0d1117; }",
            ".markdown-body code, .markdown-body tt {",
            "  padding: .2em .4em;",
            "  font-size: 85%;",
            "  white-space: break-spaces;",
            "  background-color: #161b22;",
            "  border-radius: 6px;",
            "  font-family: ui-monospace, SFMono-Regular, ""Cascadia Mono"", Consolas, ""Liberation Mono"", monospace;",
            "}",
            ".markdown-body pre {",
            "  padding: 16px;",
            "  overflow: auto;",
            "  font-size: 85%;",
            "  line-height: 1.45;",
            "  background-color: #161b22;",
            "  border-radius: 6px;",
            "}",
            ".markdown-body pre code {",
            "  padding: 0;",
            "  margin: 0;",
            "  font-size: 100%;",
            "  white-space: pre;",
            "  background-color: transparent;",
            "  border: 0;",
            "}",
            ".markdown-body table {",
            "  border-spacing: 0;",
            "  border-collapse: collapse;",
            "  display: block;",
            "  width: max-content;",
            "  max-width: 100%;",
            "  overflow: auto;",
            "}",
            ".markdown-body table th { font-weight: 600; }",
            ".markdown-body table th, .markdown-body table td { padding: 6px 13px; border: 1px solid #30363d; }",
            ".markdown-body table tr { background-color: #0d1117; border-top: 1px solid #30363d; }",
            ".markdown-body table tr:nth-child(2n) { background-color: #151b23; }",
            ".markdown-body .task-list { list-style: none; padding-left: 1.4em; }",
            ".markdown-body input[type=""checkbox""] { margin-right: .4em; vertical-align: middle; }"
        }

        Return String.Join(vbLf, lines)
    End Function

End Class
