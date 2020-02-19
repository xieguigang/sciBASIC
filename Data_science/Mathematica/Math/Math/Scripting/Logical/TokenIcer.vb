#Region "Microsoft.VisualBasic::8985d540665b9a980926f17e8b944bf9, Data_science\Mathematica\Math\Math\Scripting\Logical\TokenIcer.vb"

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

    '     Enum Tokens
    ' 
    '         [Operator], CloseStack, Comparer, OpenStack, UNDEFINE
    '         WhiteSpace
    ' 
    '  
    ' 
    ' 
    ' 
    '     Module TokenIcer
    ' 
    '         Properties: Tokens
    ' 
    '         Function: __parseComparer, __parseOperator, __parseUNDEFINE, (+2 Overloads) IsWhiteSpace, Split
    '                   TryParse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Language

Namespace Scripting.Logical

    Public Enum Tokens
        UNDEFINE
        ''' <summary>
        ''' And Or Not Xor Nor Nand
        ''' </summary>
        [Operator]
        ''' <summary>
        ''' &lt;&lt;, &lt;, &lt;=, >, =>, >>, ~=, =, &lt;>
        ''' </summary>
        Comparer
        ''' <summary>
        ''' Space or VbTab
        ''' </summary>
        WhiteSpace
        OpenStack
        CloseStack
    End Enum

    Public Class LogicalToken : Inherits CodeToken(Of Tokens)

        Sub New(name As Tokens, text$)
            Call MyBase.New(name, text)
        End Sub
    End Class

    Public Module TokenIcer

        Public ReadOnly Property Tokens As IReadOnlyDictionary(Of String, Tokens) =
            New Dictionary(Of String, Tokens) From {
 _
            {"and", Logical.Tokens.Operator},
            {"or", Logical.Tokens.Operator},
            {"not", Logical.Tokens.Operator},
            {"xor", Logical.Tokens.Operator},
            {"nor", Logical.Tokens.Operator},
            {"nand", Logical.Tokens.Operator},
            {"is", Logical.Tokens.Operator},
            {"<<", Logical.Tokens.Comparer},
            {"<", Logical.Tokens.Comparer},
            {"<=", Logical.Tokens.Comparer},
            {">", Logical.Tokens.Comparer},
            {"=>", Logical.Tokens.Comparer},
            {">>", Logical.Tokens.Comparer},
            {"~=", Logical.Tokens.Comparer},
            {"=", Logical.Tokens.Comparer},
            {"<>", Logical.Tokens.Comparer},
            {vbTab, Logical.Tokens.WhiteSpace},
            {" ", Logical.Tokens.WhiteSpace},
            {"(", Logical.Tokens.OpenStack},
            {")", Logical.Tokens.CloseStack},
            {"[", Logical.Tokens.OpenStack},
            {"]", Logical.Tokens.CloseStack},
            {"{", Logical.Tokens.OpenStack},
            {"}", Logical.Tokens.CloseStack}
        }

        Const OPERATORS As String = "AndOrNotTxXorRNorNandDISis"
        Const COMPARERS As String = "<<=>>~"
        Const STACKS As String = "()[]{}"

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

        Public Function TryParse(s As String) As List(Of LogicalToken)
            Dim str As CharEnumerator = s.GetEnumerator
            Dim tokens As New List(Of LogicalToken)
            Dim ch As Char
            Dim token As New List(Of Char)
            Dim type As Tokens = Logical.Tokens.UNDEFINE
            Dim exitb As Boolean = False

            If Not str.MoveNext() Then  ' Empty expression
                Return New List(Of LogicalToken)
            End If

            Do While True
                ch = str.Current

                If STACKS.IndexOf(ch) > -1 Then
                    Dim st As String = CStr(ch)
                    tokens += New LogicalToken(TokenIcer.Tokens(st), st)
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
                If OPERATORS.IndexOf(ch) > -1 Then
                    Call __parseOperator(str, ch, token)
                    type = Logical.Tokens.Operator

                ElseIf COMPARERS.IndexOf(ch) > -1 Then
                    Call __parseComparer(str, ch, token)
                    type = Logical.Tokens.Comparer

                Else
                    exitb = str.__parseUNDEFINE(token)
                    type = Logical.Tokens.UNDEFINE
                    tokens += New LogicalToken(type, New String(token))
                End If

                If type <> Logical.Tokens.UNDEFINE Then
                    Dim st As String = New String(token).ToLower

                    If TokenIcer.Tokens.ContainsKey(st) Then
                        type = TokenIcer.Tokens(st)

                        If ch.IsWhiteSpace AndAlso type = Logical.Tokens.Operator Then
                            tokens += New LogicalToken(type, st)
                        ElseIf type = Logical.Tokens.Comparer Then
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

                        type = Logical.Tokens.UNDEFINE
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

        <Extension> Public Function Split(source As IEnumerable(Of LogicalToken)) As List(Of MetaExpression(Of LogicalToken(), LogicalToken))
            Dim lst As New List(Of MetaExpression(Of LogicalToken(), LogicalToken))
            Dim left As New List(Of LogicalToken)

            For Each x As LogicalToken In source
                If x.name <> Logical.Tokens.UNDEFINE Then
                    lst += New MetaExpression(Of LogicalToken(), LogicalToken) With {
                        .LEFT = left,
                        .Operator = x
                    }
                    Call left.Clear()
                Else
                    left += x
                End If
            Next

            If left.Count > 0 Then
                lst += New MetaExpression(Of LogicalToken(), LogicalToken)(left)
            End If

            Return lst
        End Function

        <Extension> Public Function IsWhiteSpace(x As LogicalToken) As Boolean
            Return x.name = Logical.Tokens.WhiteSpace OrElse (x.name = Logical.Tokens.UNDEFINE AndAlso String.IsNullOrWhiteSpace(x.text))
        End Function

        <Extension> Public Function IsWhiteSpace(ch As Char) As Boolean
            Dim s As String = CStr(ch)

            If Not Tokens.ContainsKey(s) Then
                Return False
            Else
                Return Tokens(s) = Logical.Tokens.WhiteSpace
            End If
        End Function
    End Module
End Namespace
