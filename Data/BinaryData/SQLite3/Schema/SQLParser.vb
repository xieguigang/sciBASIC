#Region "Microsoft.VisualBasic::269e6d7fe7f8917e88765ee6fffd309f, Data\BinaryData\SQLite3\Schema\SQLParser.vb"

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

    '   Total Lines: 189
    '    Code Lines: 160
    ' Comment Lines: 3
    '   Blank Lines: 26
    '     File Size: 6.33 KB


    '     Enum TokenTypes
    ' 
    '         [string], close, comma, keyword, length
    '         name, open, type
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class Token
    ' 
    ' 
    ' 
    '     Class SQLParser
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetTokens, populateBufferToken, walkChar
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#If Not NET_48 Then
Imports System.Data
#End If

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Text
Imports Microsoft.VisualBasic.Text.Parser

Namespace ManagedSqlite.Core.SQLSchema

    Public Enum TokenTypes
        name
        type
        keyword
        open
        close
        comma
        length
        [string]
    End Enum

    Public Class Token : Inherits CodeToken(Of TokenTypes)
    End Class

    Public Class SQLParser

        Dim sql As CharPtr
        ''' <summary>
        ''' 主要是针对name进行escape
        ''' </summary>
        Dim escape As Boolean
        Dim escapeChar As Char
        Dim buffer As New List(Of Char)

        Sub New(sql As String)
            Me.sql = sql
        End Sub

        Public Iterator Function GetTokens() As IEnumerable(Of Token)
            Dim token As New Value(Of Token)

            Do While Not sql
                If Not token = walkChar(++sql) Is Nothing Then
                    Yield token

                    If buffer > 0 Then
                        Select Case buffer.PopAll.CharString
                            Case "("
                                Yield New Token With {.name = TokenTypes.open, .text = "("}
                            Case ")"
                                Yield New Token With {.name = TokenTypes.close, .text = ")"}
                            Case ","
                                Yield New Token With {.name = TokenTypes.comma, .text = ","}
                            Case Else
                                Throw New NotImplementedException
                        End Select
                    End If
                End If
            Loop
        End Function

        Private Function walkChar(c As Char) As Token
            If c = """"c OrElse c = "'"c Then
                If escape AndAlso ((c = """"c AndAlso escapeChar = """"c) OrElse (c = "'"c AndAlso escapeChar = "'"c)) Then
                    escape = False

                    If escapeChar = "'"c Then
                        Return New Token With {
                            .name = TokenTypes.string,
                            .text = buffer.PopAll _
                                .CharString _
                                .GetStackValue("'", "'")
                        }
                    Else
                        Return New Token With {
                            .name = TokenTypes.name,
                            .text = buffer.PopAll.CharString
                        }
                    End If
                ElseIf buffer = 0 Then
                    escapeChar = c
                    escape = True
                Else
                    buffer += c
                End If

                Return Nothing
            ElseIf c = "["c Then
                If escape Then
                    buffer += c
                Else
                    escapeChar = c
                    escape = True
                    buffer += "["c
                End If

                Return Nothing
            ElseIf escape Then
                If c = "]"c Then
                    If escape AndAlso escapeChar = "["c Then
                        buffer += "]"c
                        escape = False
                        Return populateBufferToken(Nothing)
                    Else
                        Throw New SyntaxErrorException
                    End If
                Else
                    buffer += c
                End If

                Return Nothing
            End If

            If c = "("c OrElse c = ")"c OrElse c = ","c Then
                If buffer = 0 Then
                    Return New Token With {
                        .text = c.ToString,
                        .name = If(c = "("c, TokenTypes.open, If(c = ")"c, TokenTypes.close, TokenTypes.comma))
                    }
                Else
                    Return populateBufferToken(bufferNext:=c)
                End If
            ElseIf c Like whitespace Then
                Return populateBufferToken(Nothing)
            Else
                buffer += c
            End If

            Return Nothing
        End Function

        ReadOnly whitespace As Index(Of Char) = {" "c, ASCII.TAB, ASCII.CR, ASCII.LF}

        ReadOnly keywords As Index(Of String) = {
            "create", "table", "not", "null", "nocase", "unique", "collate"
        }

        ReadOnly datatype As Index(Of String) = {
            "int", "varchar", "bit", "datetime"
        }

        Private Function populateBufferToken(bufferNext As Char?) As Token
            Dim text = buffer.PopAll.CharString

            If text.Length = 0 Then
                Return Nothing
            End If

            If Not bufferNext Is Nothing Then
                buffer += CChar(bufferNext)
            End If

            If text.ToLower Like keywords Then
                Return New Token With {
                    .text = text,
                    .name = TokenTypes.keyword
                }
            ElseIf text.First = "["c AndAlso text.Last = "]"c Then
                text = text.GetStackValue("[", "]")

                If text.ToLower Like datatype Then
                    Return New Token With {
                        .text = text,
                        .name = TokenTypes.type
                    }
                Else
                    Return New Token With {
                        .text = text,
                        .name = TokenTypes.name
                    }
                End If
            ElseIf text.IsPattern("\d+") Then
                Return New Token With {
                    .name = TokenTypes.length,
                    .text = text
                }
            ElseIf text.IsPattern("[a-z][_a-z0-9]*", RegexICSng) Then
                Return New Token With {
                    .name = TokenTypes.name,
                    .text = text
                }
            End If

            Throw New NotImplementedException(text)
        End Function
    End Class
End Namespace
