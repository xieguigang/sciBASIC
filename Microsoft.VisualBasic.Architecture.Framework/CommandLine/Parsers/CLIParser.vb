#Region "Microsoft.VisualBasic::3326d884e789a1978e94e98989a253b4, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\CommandLine\Parsers\CLIParser.vb"

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
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Text

Namespace CommandLine.Parsers

    ''' <summary>
    ''' 命令行单词解析器
    ''' </summary>
    Public Module CLIParser

        ''' <summary>
        ''' 非正则表达式命令行解析引擎
        ''' </summary>
        ''' <param name="CLI$"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' + 双引号表示一个完整的token
        ''' + 空格为分隔符
        ''' </remarks>
        <Extension> Public Function GetTokens(CLI$) As String()
            Dim buffer As New Pointer(Of Char)(CLI)
            Dim tokens As New List(Of String)
            Dim tmp As New List(Of Char)
            Dim c As Char
            Dim quotOpen As Boolean = False

            Do While Not buffer.EndRead
                c = (+buffer)

                If quotOpen Then

                    ' 双引号是结束符，但是可以使用\"进行转义
                    If c <> ASCII.Quot Then
                        tmp += c
                    Else
                        If tmp.StartEscaping Then
                            tmp.RemoveLast
                            tmp += c
                        Else
                            ' 结束
                            tokens += tmp.CharString
                            tmp *= 0
                            quotOpen = False

                        End If
                    End If

                Else
                    If c = ASCII.Quot AndAlso tmp = 0 Then
                        quotOpen = True
                    ElseIf c = " "c Then
                        ' 分隔符
                        If tmp <> 0 Then
                            tokens += tmp.CharString
                            tmp *= 0
                        End If
                    Else
                        tmp += c
                    End If
                End If
            Loop

            If tmp <> 0 Then
                tokens += New String(tmp)
            End If

            Return tokens
        End Function
    End Module
End Namespace
