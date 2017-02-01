#Region "Microsoft.VisualBasic::03bcb05344830f09c29df8b2320fd1a9, ..\sciBASIC#\Data_science\Mathematical\Math\Scripting\Arithmetic.Expression\TokenIcer.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Language

Namespace Scripting

    Module TokenIcer

        Public Function IsOpenStack(c As Char) As Boolean
            If Not Tokens.ContainsKey(c) Then
                Return False
            End If

            Return Tokens(c) = Mathematical.Scripting.Tokens.OpenStack OrElse
            Tokens(c) = Mathematical.Scripting.Tokens.OpenBracket
        End Function

        Public Function IsCloseStack(c As Char) As Boolean
            If Not Tokens.ContainsKey(c) Then
                Return False
            End If

            Return Tokens(c) = Mathematical.Scripting.Tokens.CloseBracket OrElse
            Tokens(c) = Mathematical.Scripting.Tokens.CloseStack

        End Function

        Public ReadOnly Property Tokens As IReadOnlyDictionary(Of Char, Tokens) =
        New Dictionary(Of Char, Tokens) From {
 _
        {"0"c, Mathematical.Scripting.Tokens.Number},  ' Numbers
        {"1"c, Mathematical.Scripting.Tokens.Number},
        {"2"c, Mathematical.Scripting.Tokens.Number},
        {"3"c, Mathematical.Scripting.Tokens.Number},
        {"4"c, Mathematical.Scripting.Tokens.Number},
        {"5"c, Mathematical.Scripting.Tokens.Number},
        {"6"c, Mathematical.Scripting.Tokens.Number},
        {"7"c, Mathematical.Scripting.Tokens.Number},
        {"8"c, Mathematical.Scripting.Tokens.Number},
        {"9"c, Mathematical.Scripting.Tokens.Number},
        {"."c, Mathematical.Scripting.Tokens.Number},
 _
        {"+"c, Mathematical.Scripting.Tokens.Operator},  ' Operators
        {"-"c, Mathematical.Scripting.Tokens.Operator},
        {"*"c, Mathematical.Scripting.Tokens.Operator},
        {"/"c, Mathematical.Scripting.Tokens.Operator},
        {"!"c, Mathematical.Scripting.Tokens.Operator},
        {"%"c, Mathematical.Scripting.Tokens.Operator},
        {"^"c, Mathematical.Scripting.Tokens.Operator},
 _
        {"["c, Mathematical.Scripting.Tokens.OpenBracket},  ' Brackets
        {"]"c, Mathematical.Scripting.Tokens.OpenBracket},
        {"{"c, Mathematical.Scripting.Tokens.CloseBracket},
        {"}"c, Mathematical.Scripting.Tokens.CloseBracket},
 _
        {"("c, Mathematical.Scripting.Tokens.OpenStack},  ' Stacks 
        {")"c, Mathematical.Scripting.Tokens.CloseStack},
 _
        {" "c, Mathematical.Scripting.Tokens.WhiteSpace},    ' White Space
        {CChar(vbTab), Mathematical.Scripting.Tokens.WhiteSpace},
 _
        {","c, Mathematical.Scripting.Tokens.Delimiter}
    }

        ''' <summary>
        ''' 和VisualBasic的标识符命名规则一样，变量请不要以数字开头，否则会被解析为一个数字从而产生错误的表达式
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        Public Function TryParse(s As String) As List(Of Token(Of Tokens))
            Dim str As CharEnumerator = s.GetEnumerator
            Dim tokens As New List(Of Token(Of Tokens))
            Dim ch As Char
            Dim token As New List(Of Char)
            Dim type As Tokens = Mathematical.Scripting.Tokens.UNDEFINE
            Dim exitb As Boolean = False

            If Not str.MoveNext() Then  ' Empty expression
                Return New List(Of Token(Of Tokens))
            End If

            Do While True
                ch = str.Current
                token += ch

                If TokenIcer.Tokens.ContainsKey(ch) Then
                    type = TokenIcer.Tokens(ch)

                    Select Case type
                        Case Mathematical.Scripting.Tokens.Number
                            exitb = str.__parseDouble(token)
                            tokens += New Token(Of Tokens)(type, New String(token))
                        Case Mathematical.Scripting.Tokens.WhiteSpace ' Ignore white space
                            exitb = str.MoveNext
                        Case Else
                            tokens += New Token(Of Tokens)(type, CStr(ch))
                            exitb = str.MoveNext()
                    End Select
                Else
                    exitb = str.__parseUNDEFINE(token)
                    type = Mathematical.Scripting.Tokens.UNDEFINE
                    tokens += New Token(Of Tokens)(type, New String(token))
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
        <Extension> Private Function __parseDouble(str As CharEnumerator, ByRef token As List(Of Char)) As Boolean
            Do While str.MoveNext
                If Not Tokens.ContainsKey(str.Current) Then
                    Return True
                ElseIf Not Tokens(str.Current) = Mathematical.Scripting.Tokens.Number Then
                    Return True
                Else
                    Call token.Add(str.Current)
                End If
            Loop

            Return False
        End Function

        <Extension> Private Function __parseUNDEFINE(str As CharEnumerator, ByRef token As List(Of Char)) As Boolean
            Do While str.MoveNext
                If Not Tokens.ContainsKey(str.Current) Then
                    Call token.Add(str.Current)
                Else
                    ' If next is operator or white space then exit parser
                    Select Case Tokens(str.Current)
                        Case Mathematical.Scripting.Tokens.WhiteSpace, Mathematical.Scripting.Tokens.Operator
                            Return True
                        Case Mathematical.Scripting.Tokens.OpenBracket, Mathematical.Scripting.Tokens.OpenStack,  ' Probably is a function calls
                         Mathematical.Scripting.Tokens.CloseBracket, Mathematical.Scripting.Tokens.CloseStack,
                         Mathematical.Scripting.Tokens.Delimiter
                            Return True
                        Case Else
                            Call token.Add(str.Current)
                    End Select
                End If
            Loop

            Return False
        End Function
    End Module

    Public Enum Tokens

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
