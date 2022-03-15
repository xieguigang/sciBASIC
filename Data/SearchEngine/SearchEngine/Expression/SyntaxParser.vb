#Region "Microsoft.VisualBasic::e53d8d192ec7718ba398e05bd536c1b3, sciBASIC#\Data\SearchEngine\SearchEngine\Expression\SyntaxParser.vb"

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

    '   Total Lines: 240
    '    Code Lines: 197
    ' Comment Lines: 8
    '   Blank Lines: 35
    '     File Size: 8.43 KB


    ' Module SyntaxParser
    ' 
    '     Function: Debug, is_Op2nd, IsOptr, Parser
    '     Enum Tokens
    ' 
    '         AnyTerm, MustTerm, op_AND, op_NOT, op_OR
    '         stackClose, stackOpen
    ' 
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Language

Public Module SyntaxParser

    <Extension>
    Public Function Debug(exp As IEnumerable(Of CodeToken(Of Tokens))) As String
        Dim list As New List(Of String)

        For Each x As CodeToken(Of Tokens) In exp
            If x.name = Tokens.AnyTerm OrElse x.name = Tokens.MustTerm Then
                list += $"[{x.name.ToString}, {x.text}]"
            ElseIf x.name = Tokens.stackClose Then
                list += "("
            ElseIf x.name = Tokens.stackOpen Then
                list += ")"
            ElseIf x.name = Tokens.op_AND Then
                list += "AND"
            ElseIf x.name = Tokens.op_NOT Then
                list += "NOT"
            ElseIf x.name = Tokens.op_OR Then
                list += "OR"
            End If
        Next

        Return String.Join(" ", list.ToArray)
    End Function

    Const AND$ = "AND"
    Const OR$ = "OR"
    Const NOT$ = "NOT"

    Const op$ = [AND] & [OR] & [NOT]
    Const op_start$ = "AON"
    Const op_second$ = "NRO"

    Const stackOpen As Char = "("c
    Const stackClose As Char = ")"c

    ''' <summary>
    ''' 判断当前的栈是否是在提取操作符
    ''' </summary>
    ''' <param name="tmp"></param>
    ''' <returns></returns>
    Public Function IsOptr(tmp As List(Of Char), Optional ByRef type As Tokens = Tokens.AnyTerm) As Boolean
        Select Case tmp.Count
            Case 1
                Return op_start.Contains(tmp.First)
            Case 2
                Return is_Op2nd(a:=tmp(Scan0), b:=tmp(1), type:=type)
            Case 3
                If is_Op2nd(a:=tmp(0), b:=tmp(1), type:=type) Then
                    If type = Tokens.op_OR Then ' OR只有两个字符，在这里出现了第三个字符，则很明显不是
                        Return False
                    Else
                        Dim c As Char = tmp(2)

                        If type = Tokens.op_AND Then
                            Return c = "D"c
                        Else
                            Return c = "T"c
                        End If
                    End If
                End If
            Case Else
                Return False
        End Select

        Return False
    End Function

    Private Function is_Op2nd(a As Char, b As Char, Optional ByRef type As Tokens = Tokens.AnyTerm) As Boolean
        If a = "A"c AndAlso b = "N"c Then 'AND
            type = Tokens.op_AND
            Return True
        End If
        If a = "O"c AndAlso b = "R"c Then 'OR
            type = Tokens.op_OR
            Return True
        End If
        If a = "N"c AndAlso b = "O"c Then 'NOT
            type = Tokens.op_NOT
            Return True
        End If

        Return False
    End Function

    Const Marks As Char = "'"c

    Public Function Parser(query$) As List(Of CodeToken(Of Tokens))
        Dim tklist As New List(Of CodeToken(Of Tokens))
        Dim quotOpen As Boolean
        Dim markOpen As Boolean
        Dim escape As Boolean
        Dim tmp As New List(Of Char)
        Dim getOp As Boolean
        Dim t As New Pointer(Of Char)(query$)

        Do While Not t.EndRead
            Dim c As Char = +t

            If c = ASCII.Quot AndAlso Not markOpen Then
                If escape Then
                    tmp += c
                    escape = False
                Else
                    Dim isQuotClose As Boolean = quotOpen

                    quotOpen = Not quotOpen
                    tmp += c

                    If isQuotClose Then  ' 这里的这个双引号是结束符
                        tklist += New CodeToken(Of Tokens)(Tokens.MustTerm, New String(tmp))
                        tmp.Clear()
                    End If
                End If
            ElseIf c = " "c OrElse c = ASCII.TAB Then
                If Not quotOpen AndAlso Not escape AndAlso Not markOpen Then
                    tklist += New CodeToken(Of Tokens)(Tokens.AnyTerm, New String(tmp))
                    tmp.Clear()
                Else
                    tmp += c
                End If
            Else
                tmp += c

                If quotOpen Then
                    If c = "\"c Then
                        escape = Not escape
                    Else
                        If escape Then
                            escape = Not escape
                        End If
                    End If
                    Continue Do
                End If

                If c = Marks Then
                    If escape Then
                        escape = False
                    Else
                        Dim ismarkClose As Boolean = markOpen

                        markOpen = Not markOpen

                        If ismarkClose Then  ' 这里的这个双引号是结束符
                            tklist += New CodeToken(Of Tokens)(Tokens.AnyTerm, New String(tmp))
                            tmp.Clear()
                        End If
                    End If
                End If

                If markOpen Then
                    If c = "\"c Then
                        escape = Not escape
                    Else
                        If escape Then
                            escape = Not escape
                        End If
                    End If
                    Continue Do
                End If

                If c = stackOpen Then
                    tklist += New CodeToken(Of Tokens)(Tokens.stackOpen, "(")
                    tmp.Clear()
                    Continue Do
                ElseIf c = stackClose Then
                    If tmp.Last = stackClose Then
                        tmp.RemoveLast
                    End If

                    tklist += New CodeToken(Of Tokens)(Tokens.AnyTerm, New String(tmp))
                    tklist += New CodeToken(Of Tokens)(Tokens.stackClose, ")")
                    tmp.Clear()
                    Continue Do
                End If

                If tmp.Count = 1 AndAlso op_start.IndexOf(c) > -1 Then
                    ' 可能是操作符的起始
                    getOp = True
                Else ' 第二个或者第三个
                    If getOp Then
                        Dim type As Tokens

                        If Not IsOptr(tmp, type) Then
                            getOp = False
                        End If

                        If type = Tokens.op_OR Then
                            getOp = False ' 因为提取到的已经是OR了，所以无论如何都要关闭提取

                            If t.Current = " "c Then
                                ' 是OR
                                tklist += New CodeToken(Of Tokens)(Tokens.op_OR, "OR")
                                tmp.Clear()
                                t += 1

                                Continue Do
                            End If
                        ElseIf type = Tokens.op_AND OrElse type = Tokens.op_NOT Then
                            If tmp.Count = 3 Then
                                getOp = False ' 因为提取到3个字符了，所以无论如何都要关闭提取
                            End If

                            If t.Current = " "c Then
                                ' 是AND/NOT
                                tklist += New CodeToken(Of Tokens)(type, type.ToString)
                                tmp.Clear()
                                t += 1

                                Continue Do
                            End If
                        End If
                    End If
                End If
            End If
        Loop

        tklist += New CodeToken(Of Tokens)(Tokens.AnyTerm, New String(tmp))
        tklist = New List(Of CodeToken(Of Tokens))(tklist.Where(Function(x) Not String.IsNullOrEmpty(x.text)))

        Return tklist
    End Function

    Public Enum Tokens
        AnyTerm
        MustTerm
        op_AND
        op_NOT
        op_OR
        stackOpen
        stackClose
    End Enum
End Module
