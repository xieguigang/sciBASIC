#Region "Microsoft.VisualBasic::a4f1c9d2e7354b0c8ab6d1f39e5c7b21, vs_solutions\JavaScript\Runtime\HtmlIO.vb"

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

    '     Class HtmlIO
    '
    '         Properties: Backend, BodyHtml, Fragments, Lines, Messages, Meta
    '                     Theme, Title
    '
    '         Sub: Clear, SaveHtml, WriteLine, WriteMessage
    '
    '         Function: EscapeAttribute, EscapeHtml, LevelClass, LevelLabel
    '                   NormaliseCss, RenderLine, RenderSpans, ToHtml, ToText
    '
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace Runtime

    ''' <summary>
    ''' console sink that renders script output as rich HTML: every line becomes a
    ''' row with a log-level colour bar and the <c>%c</c> styles are emitted as real
    ''' inline CSS, so the result is a styled terminal page.
    ''' <para>
    ''' Two consumption modes are supported:
    ''' <list type="bullet">
    ''' <item><see cref="BodyHtml"/> / <see cref="Fragments"/> — the bare fragment
    ''' stream, meant to be embedded into an existing page.</item>
    ''' <item><see cref="ToHtml"/> / <see cref="SaveHtml"/> — a complete, self
    ''' contained HTML5 document with a dark terminal theme, ready to be opened
    ''' directly in a browser.</item>
    ''' </list>
    ''' </para>
    ''' </summary>
    Public Class HtmlIO : Inherits ScriptIO

        ''' <summary>Document / terminal window title.</summary>
        Public Property Title As String = "LiteJs console"

        ''' <summary>Optional sub-title line (usually the script path).</summary>
        Public Property Meta As String

        ''' <summary>Theme name shown in the status bar.</summary>
        Public Property Theme As String = "midnight-glass"

        ''' <summary>Badge text shown in the top bar.</summary>
        Public Property Backend As String = "html"

        ''' <summary>Rendered <c>div</c> fragment per console message (embedding mode).</summary>
        Public ReadOnly Property Fragments As New List(Of String)

        ''' <summary>Rich records, kept for inspection / assertions.</summary>
        Public ReadOnly Property Messages As New List(Of ConsoleMessage)

        ''' <summary>Plain text form of every message (styling dropped).</summary>
        Public ReadOnly Property Lines As New List(Of String)

        Sub New()
        End Sub

        Sub New(title As String)
            Me.Title = If(String.IsNullOrEmpty(title), "LiteJs console", title)
        End Sub

        Public Overrides Sub WriteLine(text As String)
            Call WriteMessage(ConsoleMessage.FromText("log", text))
        End Sub

        Public Overrides Sub WriteMessage(message As ConsoleMessage)
            If message Is Nothing Then Return

            Messages.Add(message)
            Lines.Add(message.PlainText)
            Fragments.Add(RenderLine(message, Fragments.Count))
        End Sub

        Public Overrides Sub Clear()
            Fragments.Clear()
            Messages.Clear()
            Lines.Clear()
        End Sub

        ''' <summary>All rendered fragments joined by a newline (embedding mode).</summary>
        Public ReadOnly Property BodyHtml As String
            Get
                Return String.Join(System.Environment.NewLine, Fragments)
            End Get
        End Property

        ''' <summary>Whole output as plain text (styling dropped).</summary>
        Public Function ToText() As String
            Return String.Join(System.Environment.NewLine, Lines)
        End Function

        ''' <summary>
        ''' Wrap the rendered fragments into a self-contained HTML5 document (dark
        ''' terminal theme, glassmorphism frame, per-line log level colour bars and
        ''' a CSS-only line-number switch).
        ''' </summary>
        Public Function ToHtml() As String
            Dim sb As New StringBuilder()
            Dim generated = Date.UtcNow.ToString("yyyy-MM-dd HH:mm:ss 'UTC'")
            Dim meta As String = ""

            If Not String.IsNullOrEmpty(Me.Meta) Then
                meta = "<span class=""stat meta"">" & EscapeHtml(Me.Meta) & "</span>"
            End If

            sb.AppendLine("<!DOCTYPE html>")
            sb.AppendLine("<html lang=""zh-CN"">")
            sb.AppendLine("<head>")
            sb.AppendLine("<meta charset=""utf-8"">")
            sb.AppendLine("<meta name=""viewport"" content=""width=device-width, initial-scale=1"">")
            sb.AppendLine("<meta name=""generator"" content=""LiteJs HtmlIO"">")
            sb.AppendLine("<title>" & EscapeHtml(Title) & "</title>")
            sb.AppendLine("<style>")
            sb.AppendLine(ThemeCss)
            sb.AppendLine("</style>")
            sb.AppendLine("</head>")
            sb.AppendLine("<body>")
            sb.AppendLine("<div class=""shell"">")
            sb.AppendLine("<header class=""topbar"">")
            sb.AppendLine("<div class=""lights"" aria-hidden=""true""><i class=""l-red""></i><i class=""l-amber""></i><i class=""l-green""></i></div>")
            sb.AppendLine("<div class=""caption"">")
            sb.AppendLine("<span class=""title"">" & EscapeHtml(Title) & "</span>")
            sb.AppendLine(meta)
            sb.AppendLine("</div>")
            sb.AppendLine("<div class=""meta""><span class=""badge"">" & EscapeHtml(Backend) & "</span>" &
                          "<span class=""stat"">" & Fragments.Count & " 行</span></div>")
            sb.AppendLine("</header>")
            sb.AppendLine("<main class=""screen"">")
            sb.AppendLine("<input type=""checkbox"" id=""gutter-toggle"" class=""toggle"" checked>")
            sb.AppendLine("<pre class=""output"">" & BodyHtml & "</pre>")
            sb.AppendLine("</main>")
            sb.AppendLine("<footer class=""statusbar"">")
            sb.AppendLine("<label class=""switch"" for=""gutter-toggle""><span class=""knob"" aria-hidden=""true""></span>行号</label>")
            sb.AppendLine("<span class=""dim"">输出行数 " & Fragments.Count & "</span>")
            sb.AppendLine("<span class=""dim"">生成于 " & generated & "</span>")
            sb.AppendLine("<span class=""dim"">后端 " & EscapeHtml(Backend) & " · 主题 " & EscapeHtml(Theme) & "</span>")
            sb.AppendLine("</footer>")
            sb.AppendLine("</div>")
            sb.AppendLine("</body>")
            sb.AppendLine("</html>")

            Return sb.ToString()
        End Function

        ''' <summary>Write <see cref="ToHtml"/> to <paramref name="path"/> (UTF-8 by default).</summary>
        Public Sub SaveHtml(path As String, Optional encoding As System.Text.Encoding = Nothing)
            Dim dir = System.IO.Path.GetDirectoryName(path)

            If Not String.IsNullOrEmpty(dir) AndAlso Not System.IO.Directory.Exists(dir) Then
                Call System.IO.Directory.CreateDirectory(dir)
            End If

            Call System.IO.File.WriteAllText(path, ToHtml(), If(encoding, New UTF8Encoding(False)))
        End Sub

        ' ===================== fragment rendering =====================

        Private Function RenderLine(message As ConsoleMessage, index As Integer) As String
            Dim level = If(String.IsNullOrEmpty(message.Level), "log", message.Level)
            Dim delay = System.Math.Min(index, 40)
            Dim sb As New StringBuilder()

            sb.Append("<div class=""line level-")
            sb.Append(EscapeAttribute(LevelClass(level)))
            sb.Append(""" data-level=""")
            sb.Append(EscapeAttribute(level))
            sb.Append(""" style=""--i:")
            sb.Append(delay)
            sb.Append(""">")
            sb.Append("<span class=""bar"" aria-label=""")
            sb.Append(EscapeAttribute(LevelLabel(level)))
            sb.Append(""" role=""img""></span>")
            sb.Append("<span class=""no"">")
            sb.Append(index + 1)
            sb.Append("</span>")
            sb.Append("<code class=""content"">")
            sb.Append(RenderSpans(message))
            sb.Append("</code></div>")

            Return sb.ToString()
        End Function

        Private Function RenderSpans(message As ConsoleMessage) As String
            Dim sb As New StringBuilder()

            For Each span In message.Spans
                Dim text = EscapeHtml(span.Text)

                If String.IsNullOrWhiteSpace(span.Css) Then
                    sb.Append(text)
                Else
                    ' the raw CSS declaration list becomes an inline style attribute
                    sb.Append("<span style=""")
                    sb.Append(EscapeAttribute(NormaliseCss(span.Css)))
                    sb.Append(""">")
                    sb.Append(text)
                    sb.Append("</span>")
                End If
            Next

            Return sb.ToString()
        End Function

        ''' <summary>Strip the characters that could terminate the style attribute.</summary>
        Private Shared Function NormaliseCss(css As String) As String
            Dim sb As New StringBuilder()

            For Each c In css
                If c = ChrW(13) OrElse c = ChrW(10) OrElse c = ChrW(9) Then
                    sb.Append(" "c)
                Else
                    sb.Append(c)
                End If
            Next

            Return sb.ToString().Trim()
        End Function

        Private Shared Function EscapeHtml(value As String) As String
            If String.IsNullOrEmpty(value) Then Return String.Empty

            Dim sb As New StringBuilder(value.Length)

            For Each c In value
                Select Case c
                    Case "&"c : sb.Append("&amp;")
                    Case "<"c : sb.Append("&lt;")
                    Case ">"c : sb.Append("&gt;")
                    Case """"c : sb.Append("&quot;")
                    Case "'"c : sb.Append("&#39;")
                    Case Else : sb.Append(c)
                End Select
            Next

            Return sb.ToString()
        End Function

        Private Shared Function EscapeAttribute(value As String) As String
            Return EscapeHtml(value)
        End Function

        ''' <summary>Accessible label of a log level (colour is never the only cue).</summary>
        Private Shared Function LevelLabel(level As String) As String
            Select Case LCase(level)
                Case "debug" : Return "debug"
                Case "info" : Return "info"
                Case "warn", "warning" : Return "warning"
                Case "error" : Return "error"
                Case "assert" : Return "assertion failed"
                Case "trace" : Return "stack trace"
                Case "group", "groupcollapsed", "groupend" : Return "group"
                Case "table" : Return "table"
                Case "count" : Return "count"
                Case "time" : Return "timer"
                Case Else : Return "log"
            End Select
        End Function

        ''' <summary>CSS class suffix of a log level.</summary>
        Private Shared Function LevelClass(level As String) As String
            Select Case LCase(If(level, ""))
                Case "debug" : Return "debug"
                Case "info" : Return "info"
                Case "warn", "warning" : Return "warn"
                Case "error" : Return "error"
                Case "assert" : Return "assert"
                Case "trace" : Return "trace"
                Case "group", "groupcollapsed", "groupend" : Return "group"
                Case "table" : Return "table"
                Case Else : Return "log"
            End Select
        End Function

        ' ===================== document theme =====================

        ''' <summary>
        ''' Dark "midnight glass" terminal theme: gradient backdrop, glassmorphism
        ''' frame, monospace output area, log level colour bars, 20ms stepped entry
        ''' animation and a reduced-motion fallback.
        ''' </summary>
        Private Shared ReadOnly ThemeCss As String = String.Join(vbCrLf, {
            ":root{",
            "  --bg-0:#0B1120; --bg-1:#111827; --bg-2:#1F2937;",
            "  --text-0:#F8FAFC; --text-1:#E5E7EB; --text-2:#94A3B8;",
            "  --accent-0:#22D3EE; --accent-1:#6366F1; --accent-2:#8B5CF6;",
            "  --ok:#34D399; --warn:#FBBF24; --err:#F87171; --info:#60A5FA;",
            "  --dim:#334155;",
            "  --font:Consolas,'Cascadia Mono','JetBrains Mono',Menlo,Monaco,'Courier New',monospace;",
            "}",
            "*{box-sizing:border-box}",
            "html,body{height:100%}",
            "body{margin:0;padding:28px;font-family:var(--font);color:var(--text-1);",
            "  background:radial-gradient(1200px 600px at 12% -10%,rgba(99,102,241,.28),transparent 60%),",
            "    radial-gradient(900px 520px at 100% 0%,rgba(34,211,238,.16),transparent 55%),",
            "    linear-gradient(160deg,var(--bg-0),var(--bg-1) 45%,var(--bg-0));",
            "  background-attachment:fixed;-webkit-font-smoothing:antialiased}",
            ".shell{max-width:1180px;margin:0 auto;display:flex;flex-direction:column;height:calc(100vh - 56px);",
            "  border:1px solid rgba(148,163,184,.18);border-radius:16px;overflow:hidden;",
            "  background:linear-gradient(180deg,rgba(17,24,39,.82),rgba(11,17,32,.88));",
            "  backdrop-filter:blur(14px);-webkit-backdrop-filter:blur(14px);",
            "  box-shadow:0 30px 80px rgba(2,6,23,.65),0 0 0 1px rgba(34,211,238,.06) inset}",
            ".topbar{display:flex;align-items:center;gap:14px;padding:12px 16px;",
            "  border-bottom:1px solid rgba(148,163,184,.14);",
            "  background:linear-gradient(180deg,rgba(31,41,55,.75),rgba(17,24,39,.45))}",
            ".lights{display:flex;gap:7px;flex:0 0 auto}",
            ".lights i{width:11px;height:11px;border-radius:50%;display:block;box-shadow:0 0 10px rgba(0,0,0,.45)}",
            ".l-red{background:linear-gradient(180deg,#FB7185,#E11D48)}",
            ".l-amber{background:linear-gradient(180deg,#FCD34D,#D97706)}",
            ".l-green{background:linear-gradient(180deg,#6EE7B7,#059669)}",
            ".caption{display:flex;flex-direction:column;gap:2px;min-width:0}",
            ".caption .title{font-size:15px;font-weight:600;color:var(--text-0);letter-spacing:.2px;",
            "  white-space:nowrap;overflow:hidden;text-overflow:ellipsis}",
            ".caption .meta{font-size:12px;color:var(--text-2);white-space:nowrap;overflow:hidden;text-overflow:ellipsis}",
            ".meta{margin-left:auto;display:flex;align-items:center;gap:10px;flex:0 0 auto}",
            ".badge{font-size:11px;letter-spacing:.12em;text-transform:uppercase;color:var(--bg-0);",
            "  background:linear-gradient(120deg,var(--accent-0),var(--accent-1));",
            "  padding:3px 9px;border-radius:999px;font-weight:700;box-shadow:0 0 18px rgba(34,211,238,.35)}",
            ".stat{font-size:12px;color:var(--text-2)}",
            ".screen{position:relative;flex:1;overflow:auto;padding:14px 6px 14px 0}",
            ".toggle{position:absolute;opacity:0;pointer-events:none}",
            ".output{margin:0;padding:0;font-family:var(--font);font-size:13.5px;line-height:1.65;",
            "  white-space:pre-wrap;word-break:break-word;tab-size:4}",
            ".line{display:grid;grid-template-columns:4px 3.2rem 1fr;align-items:start;gap:10px;",
            "  padding:0 12px 0 0;animation:rise .30s ease both;animation-delay:calc(var(--i,0) * 18ms)}",
            ".line:hover{background:linear-gradient(90deg,rgba(34,211,238,.07),transparent 65%)}",
            ".bar{width:4px;height:100%;min-height:1.6em;border-radius:2px;background:var(--dim);opacity:.85}",
            ".no{color:#475569;font-size:12px;text-align:right;user-select:none;padding-top:1px}",
            ".content{font-family:inherit;color:var(--text-1);white-space:pre-wrap}",
            ".level-debug .bar,.level-info .bar{background:var(--info);box-shadow:0 0 12px rgba(96,165,250,.45)}",
            ".level-warn .bar{background:var(--warn);box-shadow:0 0 12px rgba(251,191,36,.45)}",
            ".level-warn .content{color:#FDE68A}",
            ".level-error .bar,.level-assert .bar{background:var(--err);box-shadow:0 0 12px rgba(248,113,113,.45)}",
            ".level-error .content,.level-assert .content{color:#FCA5A5}",
            ".level-trace .bar{background:var(--accent-2);box-shadow:0 0 12px rgba(139,92,246,.45)}",
            ".level-trace .content{color:#C4B5FD}",
            ".level-group .bar{background:var(--accent-0);box-shadow:0 0 12px rgba(34,211,238,.45)}",
            ".level-table .bar{background:var(--ok);box-shadow:0 0 12px rgba(52,211,153,.45)}",
            ".statusbar{display:flex;align-items:center;gap:16px;padding:9px 16px;",
            "  border-top:1px solid rgba(148,163,184,.14);background:rgba(11,17,32,.65);font-size:12px}",
            ".dim{color:var(--text-2)}",
            ".switch{display:inline-flex;align-items:center;gap:8px;cursor:pointer;user-select:none;",
            "  color:var(--text-1);transition:color .18s ease,transform .18s ease}",
            ".switch:hover{color:var(--accent-0)}",
            ".switch .knob{width:30px;height:16px;border-radius:999px;position:relative;",
            "  background:rgba(148,163,184,.25);transition:background .18s ease}",
            ".switch .knob::after{content:'';position:absolute;top:2px;left:2px;width:12px;height:12px;border-radius:50%;",
            "  background:var(--text-2);transition:transform .18s ease,background .18s ease}",
            ".toggle:not(:checked) ~ .output .line{grid-template-columns:4px 1fr}",
            ".toggle:not(:checked) ~ .output .no{display:none}",
            "body:has(.toggle:checked) .switch .knob{background:linear-gradient(120deg,var(--accent-0),var(--accent-1))}",
            "body:has(.toggle:checked) .switch .knob::after{transform:translateX(14px);background:#F8FAFC}",
            ".screen::-webkit-scrollbar{width:10px;height:10px}",
            ".screen::-webkit-scrollbar-track{background:rgba(15,23,42,.4)}",
            ".screen::-webkit-scrollbar-thumb{background:rgba(100,116,139,.45);border-radius:999px;border:2px solid transparent;",
            "  background-clip:content-box}",
            ".screen::-webkit-scrollbar-thumb:hover{background:rgba(148,163,184,.6);background-clip:content-box}",
            "@keyframes rise{from{opacity:0;transform:translateY(5px)}to{opacity:1;transform:none}}",
            "@media (max-width:720px){",
            "  body{padding:12px}.shell{height:calc(100vh - 24px);border-radius:12px}",
            "  .caption .meta,.meta .stat{display:none}",
            "  .line{grid-template-columns:4px 2.4rem 1fr}",
            "}",
            "@media (prefers-reduced-motion:reduce){.line{animation:none}}"
        })

    End Class

End Namespace
