#Region "Microsoft.VisualBasic::687aa61e4959857b3e82922c6ab65522, vs_solutions\JavaScript\Runtime\ScriptIO.vb"

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

    '     Class ScriptIO
    '
    '         Sub: Clear, WriteLine, WriteMessage
    '
    '     Class LogTextIO
    '
    '         Properties: Lines, Messages
    '
    '         Sub: Clear, WriteLine, WriteMessage
    '
    '     Class ConsoleIO
    '
    '         Sub: Clear, WriteLine, WriteMessage
    '
    '     Class ConsoleSpan
    '
    '         Properties: Css, Text
    '
    '     Class ConsoleMessage
    '
    '         Properties: Level, PlainText, Spans
    '
    '         Function: FromText
    '
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace Runtime

    ''' <summary>
    ''' One styled fragment inside a <see cref="ConsoleMessage"/>: the plain text
    ''' plus the raw CSS declaration string it was styled with (the value consumed
    ''' by a <c>%c</c> format specifier). A <c>Nothing</c> style means the fragment
    ''' inherits the default style of the current log level.
    ''' </summary>
    Public Class ConsoleSpan

        Public Property Text As String
        ''' <summary>Raw CSS declarations, e.g. <c>"color: red; font-weight: bold"</c>.</summary>
        Public Property Css As String

        Sub New()
        End Sub

        Sub New(text As String, Optional css As String = Nothing)
            Me.Text = text
            Me.Css = css
        End Sub

    End Class

    ''' <summary>
    ''' A single console output record: the log level (<c>log</c>, <c>debug</c>,
    ''' <c>info</c>, <c>warn</c>, <c>error</c>, <c>assert</c>, <c>trace</c>, ...)
    ''' plus the styled fragments the line is made of.
    ''' <para>
    ''' Text-only sinks record <see cref="PlainText"/>, so a rich line degrades to
    ''' exactly the output the plain <c>WriteLine</c> path produced before.
    ''' </para>
    ''' </summary>
    Public Class ConsoleMessage

        Public Property Level As String
        Public ReadOnly Property Spans As New List(Of ConsoleSpan)

        Sub New(level As String)
            Me.Level = If(level, "log")
        End Sub

        ''' <summary>Fragment sequence joined without separators (styling dropped).</summary>
        Public ReadOnly Property PlainText As String
            Get
                If Spans.Count = 0 Then Return String.Empty
                If Spans.Count = 1 Then Return If(Spans(0).Text, String.Empty)

                Dim sb As New StringBuilder()

                For Each span In Spans
                    sb.Append(span.Text)
                Next

                Return sb.ToString()
            End Get
        End Property

        ''' <summary>Append a styled fragment (chained helper).</summary>
        Public Function Add(text As String, Optional css As String = Nothing) As ConsoleMessage
            Spans.Add(New ConsoleSpan(text, css))
            Return Me
        End Function

        ''' <summary>A single unstyled fragment carrying <paramref name="level"/>.</summary>
        Public Shared Function FromText(level As String, text As String) As ConsoleMessage
            Return New ConsoleMessage(level).Add(text)
        End Function

        Public Overrides Function ToString() As String
            Return PlainText
        End Function

    End Class

    ''' <summary>
    ''' Sink for <c>console.*</c> output; records the plain text lines for
    ''' test/e2e comparison and, optionally, the rich fragments.
    ''' </summary>
    Public MustInherit Class ScriptIO

        ''' <summary>Write one plain line (kept abstract: every sink must support it).</summary>
        Public MustOverride Sub WriteLine(text As String)

        ''' <summary>
        ''' Write a rich console record (log level + CSS styled fragments). The base
        ''' implementation degrades to the plain text form, so text-only sinks need
        ''' no change.
        ''' </summary>
        Public Overridable Sub WriteMessage(message As ConsoleMessage)
            If message Is Nothing Then Return

            WriteLine(message.PlainText)
        End Sub

        ''' <summary>
        ''' Handle <c>console.clear()</c>: sinks that buffer output drop it, sinks
        ''' attached to a real terminal clear the screen. The default is a no-op.
        ''' </summary>
        Public Overridable Sub Clear()
        End Sub

    End Class

    ''' <summary>
    ''' In-memory sink used by the unit tests: <see cref="Lines"/> keeps the plain
    ''' text of every message (no ANSI escapes, no HTML markup), while
    ''' <see cref="Messages"/> keeps the rich records for style assertions.
    ''' </summary>
    Public Class LogTextIO : Inherits ScriptIO

        Public ReadOnly Property Lines As New List(Of String)
        Public ReadOnly Property Messages As New List(Of ConsoleMessage)

        Public Overrides Sub WriteLine(text As String)
            Lines.Add(text)
        End Sub

        Public Overrides Sub WriteMessage(message As ConsoleMessage)
            If message IsNot Nothing Then Messages.Add(message)

            MyBase.WriteMessage(message)
        End Sub

        Public Overrides Sub Clear()
            Lines.Clear()
            Messages.Clear()
        End Sub

    End Class

    ''' <summary>
    ''' Sink that writes to the .NET console. Rich fragments are rendered through
    ''' <c>CssStyleParser</c> as ANSI escape sequences (SGR), so a <c>%c</c> style
    ''' such as <c>"color: red; font-weight: bold"</c> really colors the output.
    ''' </summary>
    Public Class ConsoleIO : Inherits ScriptIO

        Public Sub New()
        End Sub

        Public Overrides Sub WriteLine(text As String)
            Call System.Console.WriteLine(text)
        End Sub

        Public Overrides Sub WriteMessage(message As ConsoleMessage)
            If message Is Nothing Then Return

            Call System.Console.WriteLine(ConsoleRenderer.Render(message))
        End Sub

        Public Overrides Sub Clear()
            Try
                Call System.Console.Clear()
            Catch ex As System.IO.IOException
                ' stdout is redirected (or no console is attached): nothing to clear
            Catch ex As System.PlatformNotSupportedException
            End Try
        End Sub

    End Class

End Namespace
