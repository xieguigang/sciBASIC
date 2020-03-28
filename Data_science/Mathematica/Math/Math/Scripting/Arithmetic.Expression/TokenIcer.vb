#Region "Microsoft.VisualBasic::b91799daf377d8e2bd64a23ef670bb56, Data_science\Mathematica\Math\Math\Scripting\Arithmetic.Expression\TokenIcer.vb"

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

    '     Module TokenIcer
    ' 
    '         Properties: Tokens
    ' 
    '         Function: IsCloseStack, IsOpenStack, parseDouble, parseUNDEFINE, TryParse
    ' 
    '     Class MetaToken
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '     Enum ExpressionTokens
    ' 
    '         [Operator], CloseBracket, CloseStack, Delimiter, Number
    '         OpenBracket, OpenStack, UNDEFINE, WhiteSpace
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text

Namespace Scripting

    Module TokenIcer

        Public Function IsOpenStack(c As Char) As Boolean
            If Not Tokens.ContainsKey(c) Then
                Return False
            End If

            Return Tokens(c) = ExpressionTokens.OpenStack OrElse
                Tokens(c) = ExpressionTokens.OpenBracket
        End Function

        Public Function IsCloseStack(c As Char) As Boolean
            If Not Tokens.ContainsKey(c) Then
                Return False
            End If

            Return Tokens(c) = ExpressionTokens.CloseBracket OrElse
                Tokens(c) = ExpressionTokens.CloseStack
        End Function

        Public ReadOnly Property Tokens As IReadOnlyDictionary(Of Char, ExpressionTokens) =
            New Dictionary(Of Char, ExpressionTokens) From {
 _
                {"0"c, ExpressionTokens.Number},  ' Numbers
                {"1"c, ExpressionTokens.Number},
                {"2"c, ExpressionTokens.Number},
                {"3"c, ExpressionTokens.Number},
                {"4"c, ExpressionTokens.Number},
                {"5"c, ExpressionTokens.Number},
                {"6"c, ExpressionTokens.Number},
                {"7"c, ExpressionTokens.Number},
                {"8"c, ExpressionTokens.Number},
                {"9"c, ExpressionTokens.Number},
                {"."c, ExpressionTokens.Number},
 _
                {"+"c, ExpressionTokens.Operator},  ' Operators
                {"-"c, ExpressionTokens.Operator},
                {"*"c, ExpressionTokens.Operator},
                {"/"c, ExpressionTokens.Operator},
                {"!"c, ExpressionTokens.Operator},
                {"%"c, ExpressionTokens.Operator},
                {"^"c, ExpressionTokens.Operator},
 _
                {"["c, ExpressionTokens.OpenBracket},  ' Brackets
                {"]"c, ExpressionTokens.OpenBracket},
                {"{"c, ExpressionTokens.CloseBracket},
                {"}"c, ExpressionTokens.CloseBracket},
 _
                {"("c, ExpressionTokens.OpenStack},  ' Stacks 
                {")"c, ExpressionTokens.CloseStack},
 _
                {" "c, ExpressionTokens.WhiteSpace},    ' White Space
                {ASCII.TAB, ExpressionTokens.WhiteSpace},
 _
                {","c, ExpressionTokens.Delimiter}
            }

        ''' <summary>
        ''' 和VisualBasic的标识符命名规则一样，变量请不要以数字开头，否则会被解析为一个数字从而产生错误的表达式
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        Public Function TryParse(s As String) As List(Of Token(Of ExpressionTokens))
            Dim str As System.CharEnumerator = s.GetEnumerator
            Dim tokens As New List(Of Token(Of ExpressionTokens))
            Dim ch As Char
            Dim token As New List(Of Char)
            Dim type As ExpressionTokens = ExpressionTokens.UNDEFINE
            Dim exitb As Boolean = False

            If Not str.MoveNext() Then  ' Empty expression
                Return New List(Of Token(Of ExpressionTokens))
            End If

            Do While True
                ch = str.Current
                token += ch

                If TokenIcer.Tokens.ContainsKey(ch) Then
                    type = TokenIcer.Tokens(ch)

                    Select Case type
                        Case ExpressionTokens.Number
                            exitb = str.parseDouble(token)
                            tokens += New Token(Of ExpressionTokens)(type, New String(token))
                        Case ExpressionTokens.WhiteSpace ' Ignore white space
                            exitb = str.MoveNext
                        Case Else
                            tokens += New Token(Of ExpressionTokens)(type, CStr(ch))
                            exitb = str.MoveNext()
                    End Select
                Else
                    exitb = str.parseUNDEFINE(token)
                    type = ExpressionTokens.UNDEFINE
                    tokens += New Token(Of ExpressionTokens)(type, New String(token))
                End If

                If Not exitb Then
                    Exit Do
                Else
                    token.Clear()
                End If
            Loop

            Return tokens
        End Function

        ''' <summary>
        ''' 枚举是否已经结束？
        ''' </summary>
        ''' <param name="str"></param>
        ''' <param name="token"></param>
        ''' <returns></returns>
        <Extension> Private Function parseDouble(str As System.CharEnumerator, ByRef token As List(Of Char)) As Boolean
            Do While str.MoveNext
                If Not Tokens.ContainsKey(str.Current) Then
                    Return True
                ElseIf Not Tokens(str.Current) = ExpressionTokens.Number Then
                    Return True
                Else
                    Call token.Add(str.Current)
                End If
            Loop

            Return False
        End Function

        <Extension> Private Function parseUNDEFINE(str As System.CharEnumerator, ByRef token As List(Of Char)) As Boolean
            Do While str.MoveNext
                If Not Tokens.ContainsKey(str.Current) Then
                    Call token.Add(str.Current)
                Else
                    ' If next is operator or white space then exit parser
                    Select Case Tokens(str.Current)
                        Case ExpressionTokens.WhiteSpace, ExpressionTokens.Operator
                            Return True
                        Case ExpressionTokens.OpenBracket, ExpressionTokens.OpenStack,  ' Probably is a function calls
                         ExpressionTokens.CloseBracket, ExpressionTokens.CloseStack,
                         ExpressionTokens.Delimiter
                            Return True
                        Case Else
                            Call token.Add(str.Current)
                    End Select
                End If
            Loop

            Return False
        End Function
    End Module

    Public Class MetaToken : Inherits CodeToken(Of ExpressionTokens)

        Sub New(name As ExpressionTokens, text$)
            Call MyBase.New(name, text)
        End Sub
    End Class

    Public Enum ExpressionTokens

        ''' <summary>
        ''' Function Name, constant, variable
        ''' </summary>
        UNDEFINE
        ''' <summary>
        ''' +-*/!^%
        ''' </summary>
        [Operator]
        ''' <summary>
        ''' <see cref="Double"/>
        ''' </summary>
        Number
        ''' <summary>
        ''' ,
        ''' </summary>
        Delimiter
        ''' <summary>
        ''' [ or {
        ''' </summary>
        OpenBracket
        ''' <summary>
        ''' ] or }
        ''' </summary>
        CloseBracket
        ''' <summary>
        ''' (
        ''' </summary>
        OpenStack
        ''' <summary>
        ''' )
        ''' </summary>
        CloseStack
        ''' <summary>
        ''' Space or Tab
        ''' </summary>
        WhiteSpace
    End Enum
End Namespace
