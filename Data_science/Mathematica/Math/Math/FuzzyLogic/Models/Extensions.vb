#Region "Microsoft.VisualBasic::5ecc5093444f1fedbdb38edec82d2183, Data_science\Mathematica\Math\Math\FuzzyLogic\Models\Extensions.vb"

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

    '   Total Lines: 186
    '    Code Lines: 158
    ' Comment Lines: 1
    '   Blank Lines: 27
    '     File Size: 7.03 KB


    '     Module Extensions
    ' 
    '         Function: __parseComparer, __parseOperator, __parseUNDEFINE, (+2 Overloads) IsWhiteSpace, TryParse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Language

Namespace Logical.FuzzyLogic.Models

    Module Extensions

        ReadOnly tokens As New Dictionary(Of String, Tokens) From {
 _
            {"and", Models.Tokens.Operator},
            {"or", Models.Tokens.Operator},
            {"not", Models.Tokens.Operator},
            {"xor", Models.Tokens.Operator},
            {"nor", Models.Tokens.Operator},
            {"nand", Models.Tokens.Operator},
            {"is", Models.Tokens.Operator},
            {"<<", Models.Tokens.Comparer},
            {"<", Models.Tokens.Comparer},
            {"<=", Models.Tokens.Comparer},
            {">", Models.Tokens.Comparer},
            {"=>", Models.Tokens.Comparer},
            {">>", Models.Tokens.Comparer},
            {"~=", Models.Tokens.Comparer},
            {"=", Models.Tokens.Comparer},
            {"<>", Models.Tokens.Comparer},
            {vbTab, Models.Tokens.WhiteSpace},
            {" ", Models.Tokens.WhiteSpace},
            {"(", Models.Tokens.OpenStack},
            {")", Models.Tokens.CloseStack},
            {"[", Models.Tokens.OpenStack},
            {"]", Models.Tokens.CloseStack},
            {"{", Models.Tokens.OpenStack},
            {"}", Models.Tokens.CloseStack}
        }

        Const STACKS As String = "()[]{}"
        Const OPERATORS As String = "AndOrNotTxXorRNorNandDISis"
        Const COMPARERS As String = "<<=>>~"

        Public Function TryParse(s As String) As List(Of LogicalToken)
            Dim str As CharEnumerator = s.GetEnumerator
            Dim tokens As New List(Of LogicalToken)
            Dim ch As Char
            Dim token As New List(Of Char)
            Dim type As Tokens = Models.Tokens.UNDEFINE
            Dim exitb As Boolean = False

            If Not str.MoveNext() Then  ' Empty expression
                Return New List(Of LogicalToken)
            End If

            Do While True
                ch = str.Current

                If STACKS.IndexOf(ch) > -1 Then
                    Dim st As String = CStr(ch)
                    tokens += New LogicalToken(Extensions.tokens(st), st)
                    If Not str.MoveNext Then
                        Exit Do
                    Else
                        Continue Do
                    End If
                ElseIf ch.IsWhiteSpace Then
                    If Not str.MoveNext Then
                        Exit Do
                    Else
                        Continue Do
                    End If
                End If

                token += ch
CONTINUTES:
                If Operators.IndexOf(ch) > -1 Then
                    Call __parseOperator(str, ch, token)
                    type = Models.Tokens.Operator

                ElseIf COMPARERS.IndexOf(ch) > -1 Then
                    Call __parseComparer(str, ch, token)
                    type = Models.Tokens.Comparer

                Else
                    exitb = str.__parseUNDEFINE(token)
                    type = Models.Tokens.UNDEFINE
                    tokens += New LogicalToken(type, New String(token))
                End If

                If type <> Models.Tokens.UNDEFINE Then
                    Dim st As String = New String(token).ToLower

                    If Extensions.tokens.ContainsKey(st) Then
                        type = Extensions.tokens(st)

                        If ch.IsWhiteSpace AndAlso type = Models.Tokens.Operator Then
                            tokens += New LogicalToken(type, st)
                        ElseIf type = Models.Tokens.Comparer Then
                            tokens += New LogicalToken(type, st)
                            Call token.Clear()
                            token += ch
                            GoTo CONTINUTES
                        Else
                            GoTo UNDEFINE
                        End If
                    Else
UNDEFINE:               If Not ch.IsWhiteSpace Then
                            token += ch  ' 不是空格，则继续解析
                            exitb = str.__parseUNDEFINE(token)
                        Else
                            '是一个空格，会被用来作为分隔符，到此为止了
                            exitb = True
                        End If

                        type = Models.Tokens.UNDEFINE
                        tokens += New LogicalToken(type, New String(token))
                    End If
                End If

                If Not exitb Then
                    Exit Do
                Else
                    token.Clear()
                End If
            Loop

            Return tokens.Removes(AddressOf IsWhiteSpace)
        End Function

        <Extension> Public Function IsWhiteSpace(x As LogicalToken) As Boolean
            Return x.name = Models.Tokens.WhiteSpace OrElse (x.name = Models.Tokens.UNDEFINE AndAlso String.IsNullOrWhiteSpace(x.text))
        End Function

        <Extension> Public Function IsWhiteSpace(ch As Char) As Boolean
            Dim s As String = CStr(ch)

            If Not tokens.ContainsKey(s) Then
                Return False
            Else
                Return tokens(s) = Models.Tokens.WhiteSpace
            End If
        End Function

        <Extension> Private Function __parseUNDEFINE(str As CharEnumerator, ByRef token As List(Of Char)) As Boolean
            Do While str.MoveNext
                If str.Current.IsWhiteSpace Then
                    Return True
                End If
                If COMPARERS.IndexOf(str.Current) = -1 AndAlso
                    STACKS.IndexOf(str.Current) = -1 Then

                    Call token.Add(str.Current)
                Else
                    Return True
                End If
            Loop

            Return False
        End Function

        <Extension> Private Function __parseOperator(str As CharEnumerator, ByRef last As Char, ByRef token As List(Of Char)) As Boolean
            Do While str.MoveNext
                If OPERATORS.IndexOf(str.Current) = -1 Then
                    last = str.Current
                    Return True
                Else
                    Call token.Add(str.Current)
                End If
            Loop

            Return False
        End Function

        <Extension> Private Function __parseComparer(str As CharEnumerator, ByRef last As Char, ByRef token As List(Of Char)) As Boolean
            Do While str.MoveNext
                If COMPARERS.IndexOf(str.Current) = -1 Then
                    last = str.Current
                    Return True
                Else
                    Call token.Add(str.Current)
                End If
            Loop

            Return False
        End Function
    End Module
End Namespace
