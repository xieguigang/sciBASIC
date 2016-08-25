#Region "Microsoft.VisualBasic::84b803ffefbe8fbf684c433b4c321088, ..\visualbasic_App\DocumentFormats\VB_DataFrame\VB_DataFrame\DocumentStream\Tokenizer.vb"

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

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic

Namespace DocumentStream

    ''' <summary>
    ''' RowObject parsers
    ''' </summary>
    Public Module Tokenizer

        ''' <summary>
        ''' A regex expression string that use for split the line text.
        ''' </summary>
        ''' <remarks></remarks>
        Const SplitRegxExpression As String = "[" & vbTab & ",](?=(?:[^""]|""[^""]*"")*$)"

        ''' <summary>
        ''' Parsing the row data from the input string line.(通过正则表达式来解析域)
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        Public Function RegexTokenizer(s As String) As List(Of String)
            If String.IsNullOrEmpty(s) Then
                Return New List(Of String)
            End If

            Dim Row As String() = Regex.Split(s, SplitRegxExpression)
            For i As Integer = 0 To Row.Length - 1
                s = Row(i)

                If Not String.IsNullOrEmpty(s) AndAlso s.Length > 1 Then
                    If s.First = """"c AndAlso s.Last = """"c Then
                        s = Mid(s, 2, s.Length - 2)
                    End If
                End If

                Row(i) = s
            Next

            Return Row.ToList
        End Function

        ''' <summary>
        ''' 通过Chars枚举来解析域
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        Public Function CharsParser(s As String) As List(Of String)
            Dim tokens As New List(Of String)
            Dim temp As New List(Of Char)
            Dim stack As Boolean = False ' 解析器是否是处于由双引号所产生的栈之中？
            Dim preToken As Boolean = False
            Dim deliExit As Boolean = False

            For Each c As Char In s.Replace("""""", """")
                If c = ","c Then
                    If Not stack Then
                        Call tokens.Add(New String(temp.ToArray))
                        Call temp.Clear()
                        deliExit = True
                    Else  '  是以双引号开始的
                        If temp.Count > 0 AndAlso temp.Last = """"c Then ' 但是逗号的前一个符号是双引号，则是结束的标识
                            Call temp.RemoveLast
                            stack = False
                            Call tokens.Add(New String(temp.ToArray))
                            Call temp.Clear()
                            deliExit = True
                        Else
                            If temp.Count = 0 AndAlso stack Then
                                Call tokens.Add("")
                                deliExit = True
                                stack = False
                            Else
                                Call temp.Add(c)
                                deliExit = False
                            End If
                        End If
                    End If
                ElseIf c = """"c Then  ' 必须要在逗号分隔符之前才起作用
                    If temp.Count = 0 Then  ' 这个双引号是在最开始的位置
                        If stack = True Then
                            Call temp.Add(c)
                        Else
                            stack = True
                        End If
                    Else
                        Call temp.Add(c)
                    End If
                    deliExit = False
                Else
                    Call temp.Add(c)
                    deliExit = False
                End If
            Next

            If temp.Count > 0 Then
                If temp.Last = """"c Then
                    Call temp.RemoveLast  ' BUGS fixed for test data:   "Iron ion, (Fe2+)","Iron homeostasis",PM0352,"Iron homeostasis","Fur - Pasteurellales",+,XC_2767,"XC_1988; XC_1989"
                End If
                Call tokens.Add(New String(temp.ToArray))
            Else
                If deliExit Then
                    Call tokens.Add("")
                End If
            End If

            Return tokens
        End Function
    End Module
End Namespace
