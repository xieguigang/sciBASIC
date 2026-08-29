#Region "Microsoft.VisualBasic::452ba945d5c79f7acd97fd4d565c827e, Microsoft.VisualBasic.Core\src\ApplicationServices\Terminal\MarkdownRender\ANSI\AnsiEscapeCodes.vb"

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

    '   Total Lines: 166
    '    Code Lines: 100 (60.24%)
    ' Comment Lines: 41 (24.70%)
    '    - Xml Docs: 80.49%
    ' 
    '   Blank Lines: 25 (15.06%)
    '     File Size: 6.85 KB


    '     Module AnsiEscapeCodes
    ' 
    '         Function: GetMoveCursorDown, GetMoveCursorLeft, GetMoveCursorRight, GetMoveCursorToColumn, GetMoveCursorUp
    '                   ToAnsiEscapeSequence, ToAnsiEscapeSequenceSlow
    ' 
    '         Sub: AppendAnsiEscapeSequence, AppendMoveCursorDown, AppendMoveCursorLeft, AppendMoveCursorRight, AppendMoveCursorToColumn
    '              AppendMoveCursorUp, MoveCursor
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace ApplicationServices.Terminal

    ''' <summary>
    ''' https://en.wikipedia.org/wiki/ANSI_escape_code
    ''' 
    ''' ANSI escape sequences are a standard for in-band signaling to control 
    ''' cursor location, color, font styling, and other options on video text 
    ''' terminals and terminal emulators. Certain sequences of bytes, most 
    ''' starting with an ASCII escape character and a bracket character, are
    ''' embedded into text. The terminal interprets these sequences as commands,
    ''' rather than text to display verbatim.
    ''' 
    ''' ANSI sequences were introduced In the 1970S To replace vendor-specific 
    ''' sequences And became widespread In the computer equipment market by 
    ''' the early 1980S. They are used In development, scientific, commercial 
    ''' text-based applications As well As bulletin board systems To offer 
    ''' standardized functionality.
    ''' 
    ''' Although hardware text terminals have become increasingly rare In the 
    ''' 21St century, the relevance Of the ANSI standard persists because a great
    ''' majority Of terminal emulators And command consoles interpret at least
    ''' a portion Of the ANSI standard.
    ''' </summary>
    Public Module AnsiEscapeCodes

        ''' <summary>
        ''' ANSI 转义字符
        ''' </summary>
        Public Const Escape As String = ChrW(&H1B)

        Private Const EscapeChar As Char = ChrW(&H1B)
        Private Const ResetForegroundColor As String = "39"
        Private Const ResetBackgroundColor As String = "49"
        Private Const ResetChar As Char = "0"c
        Private Const Bold As Char = "1"c
        Private Const Underline As Char = "4"c
        Private Const Reverse As String = "7"
        ''' <summary>
        ''' SGR 9: the strike-through / crossed-out text style, which is required
        ''' by the markdown ``~~deleted~~`` span rendering.
        ''' </summary>
        Private Const Strikeout As Char = "9"c
        Public ReadOnly ClearLine As String = $"{Escape}[0K"
        Public ReadOnly ClearToEndOfScreen As String = $"{Escape}[0J"
        Public ReadOnly ClearEntireScreen As String = $"{Escape}[2J"
        Public ReadOnly Reset As String = $"{Escape}[{ResetChar}m"

        ''' <param name="index">Index starts at 1.</param>
        Public Function GetMoveCursorToColumn(index As Integer) As String
            Return $"{Escape}[{index}G"
        End Function

        Public Function GetMoveCursorUp(count As Integer) As String
            Return If(count = 0, "", $"{Escape}[{count}A")
        End Function

        Public Function GetMoveCursorDown(count As Integer) As String
            Return If(count = 0, "", $"{Escape}[{count}B")
        End Function

        Public Function GetMoveCursorRight(count As Integer) As String
            Return If(count = 0, "", $"{Escape}[{count}C")
        End Function

        Public Function GetMoveCursorLeft(count As Integer) As String
            Return If(count = 0, "", $"{Escape}[{count}D")
        End Function

        ''' <param name="sb">Target StringBUilder.</param>
        ''' <param name="index">Index starts at 1.</param>
        Public Sub AppendMoveCursorToColumn(sb As StringBuilder, index As Integer)
            MoveCursor(sb, index, "G"c)
        End Sub

        Public Sub AppendMoveCursorUp(sb As StringBuilder, count As Integer)
            MoveCursor(sb, count, "A"c)
        End Sub

        Public Sub AppendMoveCursorDown(sb As StringBuilder, count As Integer)
            MoveCursor(sb, count, "B"c)
        End Sub

        Public Sub AppendMoveCursorRight(sb As StringBuilder, count As Integer)
            MoveCursor(sb, count, "C"c)
        End Sub

        Public Sub AppendMoveCursorLeft(sb As StringBuilder, count As Integer)
            MoveCursor(sb, count, "D"c)
        End Sub

        Private Sub MoveCursor(sb As StringBuilder, count As Integer, direction As Char)
            If count > 0 Then
                sb.Append(EscapeChar)
                sb.Append("["c)
                sb.Append(count)
                sb.Append(direction)
            End If
        End Sub

        Public Function ToAnsiEscapeSequence(colorCode As String) As String
            Return $"{Escape}[{colorCode}m"
        End Function

        ''' <summary>
        ''' convert the console print style as the ANSI escape sequence string
        ''' </summary>
        ''' <param name="formatting"></param>
        ''' <returns></returns>
        Public Function ToAnsiEscapeSequenceSlow(formatting As ConsoleFormat) As String
            Dim sb = New StringBuilder()
            AppendAnsiEscapeSequence(sb, formatting)
            Return sb.ToString()
        End Function

        Public Sub AppendAnsiEscapeSequence(stringBuilder As StringBuilder, formatting As ConsoleFormat)
            stringBuilder.Append(EscapeChar)
            stringBuilder.Append("["c)
            If formatting.Inverted Then
                ' swaps the foreground and background, so the explicitly assigned
                ' foreground/background colors should be dropped and the terminal
                ' default colors are restored at first.
                stringBuilder.Append(ResetForegroundColor)
                stringBuilder.Append(";"c)
                stringBuilder.Append(ResetBackgroundColor)
            Else
                stringBuilder.Append(ResetChar)

                If formatting.ForegroundCode IsNot Nothing Then
                    stringBuilder.Append(";"c)
                    stringBuilder.Append(formatting.ForegroundCode)
                End If

                If formatting.BackgroundCode IsNot Nothing Then
                    stringBuilder.Append(";"c)
                    stringBuilder.Append(formatting.BackgroundCode)
                End If
            End If

            ' the text decoration switches are independent from the color codes,
            ' so that they should always be evaluated, even if the style is inverted.
            If formatting.Inverted Then
                stringBuilder.Append(";"c)
                stringBuilder.Append(Reverse)
            End If

            If formatting.Bold Then
                stringBuilder.Append(";"c)
                stringBuilder.Append(Bold)
            End If

            If formatting.Underline Then
                stringBuilder.Append(";"c)
                stringBuilder.Append(Underline)
            End If

            If formatting.Strikeout Then
                stringBuilder.Append(";"c)
                stringBuilder.Append(Strikeout)
            End If

            stringBuilder.Append("m"c)
        End Sub
    End Module
End Namespace
