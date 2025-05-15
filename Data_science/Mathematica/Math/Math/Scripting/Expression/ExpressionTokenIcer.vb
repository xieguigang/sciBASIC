#Region "Microsoft.VisualBasic::fdf91d3b499f7749c57bd15578e1cb04, Data_science\Mathematica\Math\Math\Scripting\Expression\ExpressionTokenIcer.vb"

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

    '   Total Lines: 149
    '    Code Lines: 130 (87.25%)
    ' Comment Lines: 1 (0.67%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 18 (12.08%)
    '     File Size: 5.66 KB


    '     Class ExpressionTokenIcer
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetTokens, populateToken, walkChar
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser

Namespace Scripting.MathExpression

    Public Class ExpressionTokenIcer

        Dim text As CharPtr
        Dim buf As New CharBuffer

        ReadOnly operators As Index(Of Char) = {"+"c, "-"c, "*"c, "/"c, "^"c, "%"c}

        Sub New(text As CharPtr)
            Me.text = text
        End Sub

        Public Iterator Function GetTokens() As IEnumerable(Of MathToken)
            Dim token As New Value(Of MathToken)

            Do While Not text
                If Not (token = walkChar(++text)) Is Nothing Then
                    Yield token

                    If buf = 1 Then
                        If buf(Scan0) Like operators Then
                            Yield New MathToken(MathTokens.Operator, buf.ToString)
                        ElseIf buf(Scan0) = ","c Then
                            Yield New MathToken(MathTokens.Comma, ",")
                        ElseIf buf(Scan0) = "("c Then
                            Yield New MathToken(MathTokens.Open, "(")
                        ElseIf buf(Scan0) = ")"c Then
                            Yield New MathToken(MathTokens.Close, ")")
                        End If

                        buf *= 0
                    End If
                End If
            Loop

            If buf > 0 Then
                Yield populateToken(Nothing)
            End If
        End Function

        Private Function walkChar(c As Char) As MathToken
            Static numbers As Index(Of Char) = {"."c, "0"c, "1"c, "2"c, "3"c, "4"c, "5"c, "6"c, "7"c, "8"c, "9"c, "!"c}
            Static whitespaces As Index(Of Char) = {" "c, ASCII.TAB, ASCII.CR, ASCII.LF}

            If c Like numbers Then
                buf += c
            ElseIf c Like operators Then
                If buf > 0 Then
                    ' number or symbol
                    Return populateToken(cacheNext:=c)
                Else
                    Return New MathToken(MathTokens.Operator, c)
                End If
            ElseIf c Like whitespaces Then
                If buf > 0 Then
                    Return populateToken(cacheNext:=Nothing)
                End If
            ElseIf c = "("c Then
                If buf > 0 Then
                    Return populateToken(cacheNext:=c)
                Else
                    Return New MathToken(MathTokens.Open, "("c)
                End If
            ElseIf c = ")"c Then
                If buf > 0 Then
                    Return populateToken(cacheNext:=c)
                Else
                    Return New MathToken(MathTokens.Close, ")"c)
                End If
            ElseIf c = ";"c OrElse c = ","c Then
                If buf > 0 Then
                    Return populateToken(cacheNext:=c)
                ElseIf c = ";"c Then
                    Return New MathToken(MathTokens.Terminator, ";"c)
                Else
                    Return New MathToken(MathTokens.Comma, ","c)
                End If
            ElseIf c = "<"c Then
                If buf > 0 Then
                    Return populateToken(cacheNext:=c)
                Else
                    buf += c
                End If
            ElseIf c = ">" Then
                If buf = 1 AndAlso buf(Scan0) = "<" Then
                    buf.Clear()
                    Return New MathToken(MathTokens.Operator, "<>")
                Else
                    Return populateToken(cacheNext:=c)
                End If
            Else
                If buf = "<" Then
                    buf.Clear()
                    Return New MathToken(MathTokens.Operator, "<")
                ElseIf buf = ">" Then
                     buf.Clear()
                    Return New MathToken(MathTokens.Operator, ">")
                End If

                buf += c
            End If

            Return Nothing
        End Function

        Private Function populateToken(cacheNext As Char?) As MathToken
            Dim text As String = buf.ToString

            buf *= 0

            If Not cacheNext Is Nothing Then
                buf += cacheNext
            End If
            If text.Length = 0 Then
                Return Nothing
            End If

            If Char.IsLetter(text.First) Then
                If text.TextEquals("not") Then
                    Return New MathToken(MathTokens.UnaryNot, "!")
                ElseIf IsBooleanFactor(text, False) Then
                    Return New MathToken(MathTokens.LogicalLiteral, text)
                Else
                    Return New MathToken(MathTokens.Symbol, text)
                End If
            ElseIf text.IsNumeric Then
                Return New MathToken(MathTokens.Literal, text)
            ElseIf text.Last = "!"c AndAlso Mid(text, 1, text.Length - 1).IsNumeric() Then
                Return New MathToken(MathTokens.Literal, text)
            ElseIf text = "(" Then
                Return New MathToken(MathTokens.Open, "(")
            ElseIf text = ")" Then
                Return New MathToken(MathTokens.Close, ")")
            ElseIf text = ";" Then
                Return New MathToken(MathTokens.Terminator, ";")
            ElseIf text = "<" OrElse text = ">" Then
                Return New MathToken(MathTokens.Operator, text)
            Else
                Throw New NotImplementedException(text)
            End If
        End Function
    End Class
End Namespace
