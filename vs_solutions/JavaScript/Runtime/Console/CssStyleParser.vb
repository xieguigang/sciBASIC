#Region "Microsoft.VisualBasic::c8b2f1a4039d47e58c6ba7d1e0f45a92, vs_solutions\JavaScript\Runtime\CssStyleParser.vb"

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

    '     Module CssStyleParser
    '
    '         Function: FirstColorToken, IsBoldWeight, Parse, ParseColor, ParseInverted, ParseRgb
    '
    '     Module ConsoleRenderer
    '
    '         Function: LevelFormat, Render
    '
    ' /********************************************************************************/

#End Region

Imports System.Globalization
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Terminal

Namespace Runtime

    ''' <summary>
    ''' Parses the CSS declaration list accepted by the <c>%c</c> console format
    ''' specifier (e.g. <c>"color: red; font-weight: bold"</c>) into an ANSI
    ''' <see cref="ConsoleFormat"/>.
    ''' <para>
    ''' Supported declarations: <c>color</c> / <c>background(-color)</c> (named CSS
    ''' colours, <c>#RGB</c>, <c>#RRGGBB</c>, <c>rgb()</c> / <c>rgba()</c>),
    ''' <c>font-weight: bold</c>, <c>text-decoration: underline | line-through</c>
    ''' and <c>filter: invert(...)</c>. Declarations the terminal cannot express
    ''' (padding, border, font-size, ...) are ignored here — the HTML backend still
    ''' emits them verbatim.
    ''' </para>
    ''' <para>
    ''' Returns <c>Nothing</c> when nothing is expressible, so an unstyled line emits
    ''' no escape sequences at all (byte-identical to the plain text output).
    ''' </para>
    ''' </summary>
    Public Module CssStyleParser

        Public Function Parse(css As String) As ConsoleFormat
            If String.IsNullOrWhiteSpace(css) Then Return Nothing

            Dim fg As AnsiColor? = Nothing
            Dim bg As AnsiColor? = Nothing
            Dim bold As Boolean = False
            Dim underline As Boolean = False
            Dim strikeout As Boolean = False
            Dim inverted As Boolean = False

            For Each declaration In css.Split(";"c)
                Dim split = declaration.IndexOf(":"c)

                If split <= 0 Then Continue For

                Dim name = declaration.Substring(0, split).Trim().ToLowerInvariant()
                Dim value = declaration.Substring(split + 1).Trim()

                If value.Length = 0 Then Continue For

                Select Case name
                    Case "color", "foreground", "foreground-color"
                        Dim color = ParseColor(value)

                        If color.HasValue Then fg = color
                    Case "background", "background-color"
                        Dim color = ParseColor(FirstColorToken(value))

                        If color.HasValue Then bg = color
                    Case "font-weight"
                        Dim weight = value.ToLowerInvariant()

                        If weight = "bold" OrElse weight = "bolder" OrElse IsBoldWeight(weight) Then bold = True
                    Case "text-decoration", "text-decoration-line", "text-decoration-style"
                        Dim decoration = value.Replace(" ", "").ToLowerInvariant()

                        If decoration.Contains("underline") Then underline = True
                        If decoration.Contains("line-through") Then strikeout = True
                    Case "filter", "-webkit-filter"
                        If ParseInverted(value) Then inverted = True
                End Select
            Next

            If Not (fg.HasValue OrElse bg.HasValue OrElse bold OrElse underline OrElse strikeout OrElse inverted) Then
                Return Nothing
            End If

            Return New ConsoleFormat With {
                .Foreground = fg,
                .Background = bg,
                .Bold = bold,
                .Underline = underline,
                .Strikeout = strikeout,
                .Inverted = inverted
            }
        End Function

        Private Function IsBoldWeight(weight As String) As Boolean
            Dim w As Double

            If Double.TryParse(weight, NumberStyles.Float, CultureInfo.InvariantCulture, w) Then
                Return w >= 600
            End If

            Return False
        End Function

        ''' <summary>Extract the <c>invert(1)</c> / <c>invert(100%)</c> flag of a filter list.</summary>
        Private Function ParseInverted(filter As String) As Boolean
            Dim compact = filter.Replace(" ", "").ToLowerInvariant()
            Dim start = compact.IndexOf("invert(")

            If start < 0 Then Return False

            Dim [end] = compact.IndexOf(")"c, start)

            If [end] < 0 Then Return False

            Dim amount = compact.Substring(start + "invert(".Length, [end] - start - "invert(".Length)
            Dim value As Double

            If amount.EndsWith("%") Then
                If Double.TryParse(amount.TrimEnd("%"c), NumberStyles.Float, CultureInfo.InvariantCulture, value) Then
                    Return value >= 50
                End If

                Return False
            End If

            If Double.TryParse(amount, NumberStyles.Float, CultureInfo.InvariantCulture, value) Then
                Return value >= 0.5
            End If

            Return False
        End Function

        ''' <summary>
        ''' The first whitespace separated token that resolves as a colour, so a
        ''' shorthand such as <c>background: url(x.png) #1F2937</c> still works.
        ''' </summary>
        Private Function FirstColorToken(value As String) As String
            Dim tokens = value.Split(" "c)

            If tokens.Length <= 1 Then Return value

            For Each token In tokens
                If ParseColor(token).HasValue Then Return token
            Next

            Return tokens(0)
        End Function

        ''' <summary>
        ''' CSS colour → <see cref="AnsiColor"/>; <c>Nothing</c> means "not a colour"
        ''' (inherit / initial / transparent / unknown name / unsupported syntax).
        ''' </summary>
        Public Function ParseColor(value As String) As AnsiColor?
            If String.IsNullOrWhiteSpace(value) Then Return Nothing

            Dim text = value.Trim()

            Select Case text.ToLowerInvariant()
                Case "inherit", "initial", "unset", "revert", "currentcolor", "transparent", "none", "auto"
                    Return Nothing
            End Select

            ' hsl() is not expressible in ansi without extra math: ignore it
            If text.ToLowerInvariant().StartsWith("hsl") Then Return Nothing

            If text.StartsWith("#"c) AndAlso text.Length = 4 Then
                ' #RGB shorthand → #RRGGBB
                Dim r = text(1), g = text(2), b = text(3)
                text = $"#{r}{r}{g}{g}{b}{b}"
            End If

            ' the builtin parser knows the 16 ansi names and #RRGGBB
            Dim ansi As AnsiColor = Nothing

            If AnsiColor.TryParse(text, ansi) Then Return ansi

            ' rgb() / rgba(): an alpha of zero is treated as "no colour set"
            Dim parsed = ParseRgb(text)

            If parsed.HasValue Then Return parsed

            ' CSS named colours beyond the 16 ansi ones (Orange, Purple, ...)
            Dim named = System.Drawing.Color.FromName(text)

            If named.IsEmpty OrElse named.A = 0 Then Return Nothing

            Return AnsiColor.FromRgbColor(named)
        End Function

        Private Function ParseRgb(value As String) As AnsiColor?
            Dim text = value.Trim().ToLowerInvariant()

            If Not (text.StartsWith("rgb(") OrElse text.StartsWith("rgba(")) Then Return Nothing

            Dim open = text.IndexOf("("c)
            Dim close = text.LastIndexOf(")"c)

            If close <= open Then Return Nothing

            Dim parts = text.Substring(open + 1, close - open - 1).Split(","c)
            Dim channels(2) As Byte
            Dim alpha As Double = 1.0

            If parts.Length < 3 Then Return Nothing

            For i = 0 To 2
                Dim channel As Double

                If Not TryParseChannel(parts(i), channel) Then Return Nothing

                channels(i) = CByte(System.Math.Max(0, System.Math.Min(255, System.Math.Round(channel))))
            Next

            If parts.Length >= 4 Then
                Dim a = parts(3).Trim()

                If a.EndsWith("%") Then
                    Dim percent As Double
                    If Double.TryParse(a.TrimEnd("%"c), NumberStyles.Float, CultureInfo.InvariantCulture, percent) Then alpha = percent / 100.0
                Else
                    Dim numeric As Double
                    If Double.TryParse(a, NumberStyles.Float, CultureInfo.InvariantCulture, numeric) Then alpha = numeric
                End If
            End If

            If alpha <= 0 Then Return Nothing

            Return AnsiColor.Rgb(channels(0), channels(1), channels(2))
        End Function

        Private Function TryParseChannel(token As String, ByRef channel As Double) As Boolean
            Dim text = token.Trim()

            If text.EndsWith("%") Then
                Dim percent As Double

                If Double.TryParse(text.TrimEnd("%"c), NumberStyles.Float, CultureInfo.InvariantCulture, percent) Then
                    channel = percent * 255.0 / 100.0
                    Return True
                End If

                Return False
            End If

            Return Double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, channel)
        End Function

    End Module

    ''' <summary>
    ''' Renders a rich <see cref="ConsoleMessage"/> as ANSI escaped text: the log
    ''' level provides the base style (<c>warn</c> → yellow, <c>error</c>/<c>assert</c>
    ''' → red, <c>trace</c> → magenta, ...) and each fragment's <c>%c</c> CSS is
    ''' merged on top of it.
    ''' </summary>
    Public Module ConsoleRenderer

        Public Function Render(message As ConsoleMessage) As String
            If message Is Nothing Then Return String.Empty
            If message.Spans.Count = 0 Then Return String.Empty

            Dim parent = LevelFormat(message.Level)
            Dim sb As New StringBuilder()

            For Each span In message.Spans
                Dim child As ConsoleFormat = Nothing

                If Not String.IsNullOrWhiteSpace(span.Css) Then child = CssStyleParser.Parse(span.Css)

                AppendRun(sb, span.Text, ConsoleFormat.Combine(parent, child))
            Next

            Return sb.ToString()
        End Function

        Private Sub AppendRun(sb As StringBuilder, text As String, format As ConsoleFormat)
            If String.IsNullOrEmpty(text) Then Return

            If format Is Nothing OrElse format.IsDefault Then
                sb.Append(text)
                Return
            End If

            sb.Append(AnsiEscapeCodes.ToAnsiEscapeSequenceSlow(format))
            sb.Append(text)
            sb.Append(AnsiEscapeCodes.Reset)
        End Sub

        ''' <summary>Base style of a log level; <c>Nothing</c> keeps the terminal default.</summary>
        Public Function LevelFormat(level As String) As ConsoleFormat
            Select Case LCase(If(level, ""))
                Case "warn", "warning"
                    Return New ConsoleFormat With {.Foreground = AnsiColor.BrightYellow}
                Case "error", "assert"
                    Return New ConsoleFormat With {.Foreground = AnsiColor.BrightRed}
                Case "debug", "info"
                    Return New ConsoleFormat With {.Foreground = AnsiColor.BrightBlue}
                Case "trace"
                    Return New ConsoleFormat With {.Foreground = AnsiColor.BrightMagenta}
                Case "group", "groupcollapsed", "groupend"
                    Return New ConsoleFormat With {.Foreground = AnsiColor.BrightCyan}
                Case Else
                    Return Nothing
            End Select
        End Function

    End Module

End Namespace
