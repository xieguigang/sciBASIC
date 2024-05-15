#Region "Microsoft.VisualBasic::d6e139e5cc910968c2a57fa04718d35c, Data\GraphQuery\Language\TokenIcer.vb"

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

    '   Total Lines: 90
    '    Code Lines: 78
    ' Comment Lines: 1
    '   Blank Lines: 11
    '     File Size: 3.32 KB


    '     Class TokenIcer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: popOutToken, walkChar
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser

Namespace Language

    Public Class TokenIcer : Inherits SyntaxTokenlizer(Of Tokens, Token)

        ReadOnly escape As New Escaping

        Public Sub New(text As [Variant](Of String, CharPtr))
            MyBase.New(text)
        End Sub

        Protected Overrides Function walkChar(c As Char) As Token
            If escape.string Then
                If c <> escape.quot Then
                    buffer += c
                    Return Nothing
                Else
                    escape.string = False
                    escape.quot = Nothing
                    Return New Token(Tokens.text, buffer.PopAllChars)
                End If
            ElseIf escape.comment Then
                If c <> ASCII.CR AndAlso c <> ASCII.LF Then
                    buffer += c
                    Return Nothing
                Else
                    Dim t As New Token(Tokens.comment, buffer.PopAllChars)
                    escape.comment = False
                    buffer += ASCII.LF
                    Return t
                End If
            Else
                Select Case c
                    Case "{"c, "("c, "["c, "}"c, "]"c, ")"c, "|"c, ","c
                        Dim t As Token = popOutToken()
                        buffer += c
                        Return t
                    Case " "c, ASCII.TAB
                        Return popOutToken()
                    Case ASCII.CR, ASCII.LF, ";"c
                        Dim t = popOutToken()
                        buffer += ASCII.LF
                        Return t
                    Case """"c, "'"c
                        Dim t As Token = popOutToken()
                        escape.string = True
                        escape.quot = c
                        Return t
                    Case "#"c
                        Dim t As Token = popOutToken()
                        escape.comment = True
                        Return t
                    Case Else
                        Dim t As Token = Nothing

                        If buffer = 1 AndAlso (buffer Like {"|", "(", ",", "["}) Then
                            t = popOutToken()
                        End If

                        buffer += c

                        Return t
                End Select
            End If
        End Function

        Protected Overrides Function popOutToken() As Token
            If buffer = 0 Then
                Return Nothing
            End If

            Dim text As New String(buffer.PopAllChars)

            Select Case text
                Case "{", "[", "(" : Return New Token(Tokens.open, text)
                Case "}", "]", ")" : Return New Token(Tokens.close, text)
                Case "|" : Return New Token(Tokens.pipeline, text)
                Case "," : Return New Token(Tokens.comma, ",")
                Case ASCII.LF
                    ' Return New Token(Tokens.terminator, ";")
                    Return Nothing
                Case Else
                    Return New Token(Tokens.symbol, text.Trim(ASCII.LF, ASCII.CR))
            End Select
        End Function
    End Class
End Namespace
