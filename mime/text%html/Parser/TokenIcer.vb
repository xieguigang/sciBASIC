#Region "Microsoft.VisualBasic::cfc743deb7f7ea28f7cfd735fdf4f4e9, mime\text%html\Parser\TokenIcer.vb"

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

    '   Total Lines: 127
    '    Code Lines: 107
    ' Comment Lines: 0
    '   Blank Lines: 20
    '     File Size: 4.43 KB


    '     Class TokenIcer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetTokens, MeasureToken, WalkChar
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser

Namespace Language

    Public Class TokenIcer

        Dim buf As New CharBuffer
        Dim text As CharPtr
        Dim escape As New Escaping
        Dim lastToken As Token

        Sub New(document As CharPtr)
            Me.text = document
        End Sub

        Public Iterator Function GetTokens() As IEnumerable(Of Token)
            Do While text
                For Each t As Token In WalkChar(++text)
                    If Not t Is Nothing Then
                        lastToken = t
                        Yield t
                    End If
                Next
            Loop

            If buf > 0 Then
                Dim t As Token = MeasureToken(New String(buf.PopAllChars))

                If Not t Is Nothing Then
                    Yield t
                End If
            End If
        End Function

        Private Iterator Function WalkChar(c As Char) As IEnumerable(Of Token)
            If escape.string Then
                If c = escape.quote Then
                    Yield New Token(HtmlTokens.text, buf.PopAllChars)

                    escape.string = False
                    escape.quote = Nothing
                Else
                    buf += c
                End If
            ElseIf escape.scriptOpen Then
                If c = "<"c Then
                    If escape.checkScriptEnd = 0 Then
                        escape.checkScriptEnd += c
                    End If
                ElseIf c = "/"c Then
                    If escape.checkScriptEnd = "<" Then
                        escape.checkScriptEnd *= 0
                        escape.scriptOpen = False

                        If buf > 0 Then
                            Yield MeasureToken(New String(buf.PopAllChars))
                            Yield New Token(HtmlTokens.openTag, "<")
                            Yield New Token(HtmlTokens.splash, "/")
                        End If
                    End If
                Else
                    escape.checkScriptEnd.Clear()
                    buf += c
                End If
            ElseIf c = "<"c Then
                If buf > 0 Then
                    Yield MeasureToken(New String(buf.PopAllChars))
                End If

                escape.tagOpen = True

                Yield New Token(HtmlTokens.openTag, c)
            ElseIf c = "/"c Then
                If buf > 0 Then
                    Yield MeasureToken(New String(buf.PopAllChars))
                End If

                Yield New Token(HtmlTokens.splash, c)
            ElseIf c = ">"c Then
                If buf > 0 Then
                    Yield MeasureToken(New String(buf.PopAllChars))
                End If

                escape.tagOpen = False

                Yield New Token(HtmlTokens.closeTag, c)
            ElseIf escape.tagOpen AndAlso (c = """"c OrElse c = "'"c) Then
                If buf > 0 Then
                    Yield MeasureToken(New String(buf.PopAllChars))
                End If

                escape.string = True
                escape.quote = c
            ElseIf escape.tagOpen AndAlso (c = " "c OrElse c = ASCII.TAB) Then
                If buf > 0 Then
                    Yield MeasureToken(New String(buf.PopAllChars))
                End If
            ElseIf c = "="c Then
                If buf > 0 Then
                    Yield MeasureToken(New String(buf.PopAllChars))
                End If

                Yield New Token(HtmlTokens.equalsSymbol, "=")
            Else
                buf += c
            End If
        End Function

        Private Function MeasureToken(text As String) As Token
            If lastToken IsNot Nothing AndAlso lastToken = (HtmlTokens.openTag, "<") AndAlso text.TextEquals("script") Then
                escape.scriptOpen = True
            End If

            If text = "=" Then
                Return New Token(HtmlTokens.equalsSymbol, "=")
            ElseIf text = vbCr OrElse text = vbLf OrElse text = vbCrLf Then
                Return Nothing
            ElseIf text.Trim(" "c, ASCII.CR, ASCII.LF) = "" Then
                Return Nothing
            Else
                Return New Token(HtmlTokens.text, text)
            End If
        End Function

    End Class
End Namespace
