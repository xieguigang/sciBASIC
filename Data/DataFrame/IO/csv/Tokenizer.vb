#Region "Microsoft.VisualBasic::fcaa2d92376c02eeb590423fbe9af323, ..\sciBASIC#\Data\DataFrame\IO\csv\Tokenizer.vb"

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

Option Explicit On
Option Strict Off

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Text

Namespace IO

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

            Return Row.AsList
        End Function

        ''' <summary>
        ''' 通过Chars枚举来解析域，分隔符默认为逗号
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        Public Function CharsParser(s$, Optional delimiter As Char = ","c) As List(Of String)
            Dim tokens As New List(Of String)  ' row data 
            Dim temp As New List(Of Char)
            Dim openEscaping As Boolean = False ' 解析器是否是处于由双引号所产生的栈之中？
            Dim buffer As New Pointer(Of Char)(s)

            Do While Not buffer.EndRead
                Dim c As Char = +buffer

                If openEscaping Then

                    If c = ASCII.Quot Then
                        If temp.StartEscaping Then
                            Call temp.RemoveLast
                            Call temp.Add(c)
                        Else
                            ' 查看下一个字符是否为分隔符
                            Dim peek = buffer.Current  ' 因为前面的 Dim c As Char = +buffer 已经位移了，所以在这里直接取当前的字符

                            If peek = delimiter OrElse buffer.EndRead Then
                                ' 下一个字符为分隔符，则结束这个token
                                tokens += New String(temp)
                                temp *= 0
                                buffer += 1  ' 跳过下一个分隔符，因为已经在这里判断过了
                                openEscaping = False
                            Else
                                ' 不是，则继续添加
                                temp += c
                            End If
                        End If
                    Else
                        ' 由于双引号而产生的转义                   
                        temp += c
                    End If
                Else
                    If temp.Count = 0 AndAlso c = ASCII.Quot Then
                        ' token的第一个字符串为双引号，则开始转义
                        openEscaping = True
                    Else
                        If c = delimiter Then
                            tokens += New String(temp)
                            temp *= 0
                        Else
                            temp += c
                        End If
                    End If
                End If
            Loop

            If temp.Count > 0 Then
                tokens += New String(temp)
            End If

            Return tokens
        End Function

        ''' <summary>
        ''' 是否等于``,,,,,,,,,``
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        <Extension>
        Public Function IsEmptyRow(s As String, del As Char) As Boolean
            Dim l As Integer = Len(s)

            If l = 0 Then
                Return True
            End If

            For Each c As Char In s
                If c = del Then
                    l -= 1
                End If
            Next

            Return l = 0 ' 长度为零说明整个字符串都是分隔符，即为空行
        End Function
    End Module
End Namespace
