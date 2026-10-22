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

    ''' <summary>
    ''' 论文学术风格主题：中文论文版式（宋体正文 + 黑体章标题 + 首行缩进两字符 + 三线表）。
    ''' </summary>
    ''' <remarks>
    ''' 版式取自 <c>ThesisFormatting</c>（docx 论文主题），保持同一份内容在 word 与
    ''' html 下的一致性：章标题三号黑体居中、节标题四号宋体加粗、正文小四宋体首行缩进
    ''' 两字符、表格使用无竖线的三线表、引用块不倾斜无底纹、代码块五号等宽无底纹。
    ''' 打印时章标题自动另起一页。
    ''' </remarks>
    ''' <returns></returns>
    Public Shared ReadOnly Property Thesis As MarkdownStyle
        Get
            Return _thesis
        End Get
    End Property

    Private Shared ReadOnly _github As MarkdownStyle = Create(GitHubCss(), "github", "markdown-body")
    Private Shared ReadOnly _dark As MarkdownStyle = Create(DarkCss(), "dark", "markdown-body")
    Private Shared ReadOnly _thesis As MarkdownStyle = Create(ThesisCss(), "thesis", "markdown-body")

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

    ''' <summary>
    ''' 论文学术风格样式：中文论文版式（三号黑体章标题、四号宋体节标题、小四宋体正文
    ''' 首行缩进两字符、三线表、无底纹代码块），与 <c>ThesisFormatting</c> 的规范一致。
    ''' </summary>
    ''' <returns></returns>
    Private Shared Function ThesisCss() As String
        Dim lines As String() = {
            ".markdown-body {",
            "  box-sizing: border-box;",
            "  max-width: 800px;",
            "  margin: 0 auto;",
            "  padding: 48px 56px;",
            "  font-family: ""Times New Roman"", SimSun, ""宋体"", ""Songti SC"", serif;",
            "  font-size: 12pt;",
            "  line-height: 1.7;",
            "  color: #000000;",
            "  background-color: #ffffff;",
            "  text-align: justify;",
            "  word-wrap: break-word;",
            "  -webkit-text-size-adjust: 100%;",
            "}",
            ".markdown-body > *:first-child { margin-top: 0 !important; }",
            ".markdown-body > *:last-child { margin-bottom: 0 !important; }",
            "/* 章标题：三号黑体，居中，段前段后各空一行 */",
            ".markdown-body h1 {",
            "  margin: 2.4em 0 1.2em;",
            "  padding: 0;",
            "  font-family: SimHei, ""黑体"", ""Heiti SC"", sans-serif;",
            "  font-size: 16pt;",
            "  font-weight: bold;",
            "  line-height: 1.5;",
            "  text-align: center;",
            "  text-indent: 0;",
            "  border: 0;",
            "  break-after: avoid;",
            "}",
            "/* 节标题：四号宋体加粗，左对齐 */",
            ".markdown-body h2 {",
            "  margin: 1.4em 0 .7em;",
            "  padding: 0;",
            "  font-family: ""Times New Roman"", SimSun, ""宋体"", serif;",
            "  font-size: 14pt;",
            "  font-weight: bold;",
            "  line-height: 1.5;",
            "  text-align: left;",
            "  text-indent: 0;",
            "  border: 0;",
            "  break-after: avoid;",
            "}",
            "/* 小节标题：小四号宋体加粗，左对齐 */",
            ".markdown-body h3 {",
            "  margin: 1.2em 0 .6em;",
            "  padding: 0;",
            "  font-size: 12pt;",
            "  font-weight: bold;",
            "  text-align: left;",
            "  text-indent: 0;",
            "  border: 0;",
            "  break-after: avoid;",
            "}",
            ".markdown-body h4, .markdown-body h5, .markdown-body h6 {",
            "  margin: 1em 0 .5em;",
            "  padding: 0;",
            "  font-size: 12pt;",
            "  font-weight: bold;",
            "  text-align: left;",
            "  text-indent: 0;",
            "  border: 0;",
            "  break-after: avoid;",
            "}",
            ".markdown-body h5, .markdown-body h6 { font-size: 10.5pt; }",
            "/* 正文：小四号宋体，首行缩进两字符，两端对齐 */",
            ".markdown-body p {",
            "  margin: 0 0 .35em;",
            "  text-indent: 2em;",
            "  text-align: justify;",
            "}",
            ".markdown-body li > p, .markdown-body td p, .markdown-body th p { text-indent: 0; }",
            ".markdown-body a { color: #000000; text-decoration: underline; }",
            ".markdown-body strong { font-weight: bold; }",
            ".markdown-body ul, .markdown-body ol { margin: 0 0 .6em; padding-left: 2.4em; }",
            ".markdown-body li { margin: .15em 0; text-align: justify; }",
            "/* 引用块：小四号宋体，首行缩进两字符，不倾斜、无底纹、无左边线 */",
            ".markdown-body blockquote {",
            "  margin: 0 0 .6em;",
            "  padding: 0;",
            "  border: 0;",
            "  font-size: 12pt;",
            "  font-style: normal;",
            "  color: #000000;",
            "  text-indent: 2em;",
            "}",
            ".markdown-body blockquote > :first-child { margin-top: 0; }",
            ".markdown-body blockquote > :last-child { margin-bottom: 0; }",
            ".markdown-body hr {",
            "  width: 30%;",
            "  margin: 2em auto;",
            "  padding: 0;",
            "  height: 0;",
            "  background: none;",
            "  border: 0;",
            "  border-top: 1px solid #000000;",
            "}",
            "/* 插图：居中，图题五号居中 */",
            ".markdown-body img {",
            "  display: block;",
            "  max-width: 100%;",
            "  margin: 1em auto .4em;",
            "  background: none;",
            "}",
            "/* 代码：五号等宽，无底纹 */",
            ".markdown-body code, .markdown-body tt {",
            "  padding: 0;",
            "  font-family: Consolas, ""Courier New"", ""Times New Roman"", monospace;",
            "  font-size: 10.5pt;",
            "  background-color: transparent;",
            "  border: 0;",
            "}",
            ".markdown-body pre {",
            "  margin: 0 0 .8em;",
            "  padding: .6em .8em;",
            "  font-size: 10.5pt;",
            "  line-height: 1.4;",
            "  overflow: auto;",
            "  background-color: #ffffff;",
            "  border: 1px solid #c8c8c8;",
            "  border-radius: 0;",
            "  text-indent: 0;",
            "}",
            ".markdown-body pre code {",
            "  padding: 0;",
            "  font-size: 100%;",
            "  white-space: pre;",
            "  background-color: transparent;",
            "  border: 0;",
            "}",
            "/* 三线表：上下粗线 + 表头下细线，无竖线、无斑马纹 */",
            ".markdown-body table {",
            "  display: table;",
            "  width: auto;",
            "  max-width: 100%;",
            "  margin: .8em auto;",
            "  border: 0;",
            "  border-top: 1.5pt solid #000000;",
            "  border-bottom: 1.5pt solid #000000;",
            "  border-collapse: collapse;",
            "  border-spacing: 0;",
            "  font-family: ""Times New Roman"", SimSun, ""宋体"", serif;",
            "  font-size: 10.5pt;",
            "  overflow: visible;",
            "}",
            ".markdown-body table thead th {",
            "  padding: .35em .9em;",
            "  font-weight: bold;",
            "  text-align: center;",
            "  background-color: transparent;",
            "  border: 0;",
            "  border-bottom: 1pt solid #000000;",
            "}",
            ".markdown-body table tbody td {",
            "  padding: .3em .9em;",
            "  background-color: transparent;",
            "  border: 0;",
            "}",
            ".markdown-body table tr {",
            "  background-color: transparent;",
            "  border: 0;",
            "}",
            ".markdown-body table caption {",
            "  caption-side: top;",
            "  padding-bottom: .4em;",
            "  font-size: 10.5pt;",
            "}",
            ".markdown-body .task-list { list-style: none; padding-left: 1.4em; }",
            ".markdown-body input[type=""checkbox""] { margin-right: .4em; vertical-align: middle; }",
            "/* 打印：章标题另起一页，去掉屏幕预览的内边距 */",
            "@media print {",
            "  .markdown-body { max-width: none; padding: 0; }",
            "  .markdown-body h1 { break-before: page; }",
            "  .markdown-body h1:first-of-type { break-before: auto; }",
            "}"
        }

        Return String.Join(vbLf, lines)
    End Function

End Class
