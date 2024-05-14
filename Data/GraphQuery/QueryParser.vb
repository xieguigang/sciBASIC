#Region "Microsoft.VisualBasic::d7bf451e555fde2a0de4088afc743672, Data\GraphQuery\QueryParser.vb"

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

    '   Total Lines: 138
    '    Code Lines: 121
    ' Comment Lines: 1
    '   Blank Lines: 16
    '     File Size: 5.19 KB


    ' Class QueryParser
    ' 
    '     Function: GetParser, GetQuery, GetQueryTokens
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.GraphQuery.Language
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language

Public Class QueryParser

    Public Shared Function GetQuery(text As String) As Query
        Dim tokenList As Token() = New TokenIcer(text) _
            .GetTokens _
            .Where(Function(tk) Not tk.name = Tokens.comment) _
            .ToArray
        Dim i As Pointer(Of QueryToken) = GetQueryTokens(tokenList).ToArray
        Dim t As Value(Of QueryToken) = ++i
        Dim query As Query
        Dim queryStack As New Stack(Of Query)
        Dim pipeNext As Boolean = False

        If CType(t, QueryToken).name = Tokens.symbol Then
            query = New Query With {.name = CType(t, QueryToken).text}
        Else
            query = New Query With {.name = "n/a"}
        End If

        Call queryStack.Push(query)

        Do While i
            Select Case (t = ++i).name
                Case Tokens.open
                    Select Case CType(t, QueryToken).text
                        Case "{"
                            queryStack.Push(query)
                        Case "["
                            query.isArray = True
                        Case Else
                            Throw New SyntaxErrorException
                    End Select
                Case Tokens.close
                    Select Case CType(t, QueryToken).text
                        Case "}"
                            queryStack.Pop()
                        Case Else
                            ' do nothing 
                    End Select
                Case Tokens.symbol
                    query = New Query With {.name = CType(t, QueryToken).text}
                    queryStack.Peek.Add(query)

                    pipeNext = False
                Case Tokens.NA
                    If pipeNext Then
                        If query.isTextArray Then
                            query.members(Scan0).parser &= CType(t, QueryToken).func
                        Else
                            query.parser &= CType(t, QueryToken).func
                        End If
                    ElseIf query.isArray Then
                        If query.members.IsNullOrEmpty Then
                            query.members = {
                                New Query With {
                                    .parser = CType(t, QueryToken).func,
                                    .name = "@array"
                                }
                            }
                        Else
                            Throw New SyntaxErrorException
                        End If
                    ElseIf Not query.parser Is Nothing Then
                        Throw New SyntaxErrorException
                    Else
                        query.parser = CType(t, QueryToken).func
                    End If

                    pipeNext = False
                Case Tokens.pipeline
                    pipeNext = True
                Case Else
                    Throw New NotImplementedException
            End Select
        Loop

        query = queryStack.First

        Return query
    End Function

    Private Shared Iterator Function GetQueryTokens(tokenList As Token()) As IEnumerable(Of QueryToken)
        Dim buf As New List(Of Token)

        For Each t As Token In tokenList
            If t.name = Tokens.symbol Then
                If buf = 1 Then
                    Yield New QueryToken With {.token = buf(Scan0)}
                    buf.Clear()
                End If

                buf += t
            ElseIf t.name = Tokens.open AndAlso t.text = "(" Then
                buf += t
            ElseIf t.name = Tokens.text OrElse t.name = Tokens.comma Then
                buf += t
            ElseIf t.name = Tokens.close AndAlso t.text = ")" Then
                buf += t
                Yield New QueryToken With {.func = GetParser(buf.PopAll)}
            Else
                If buf > 0 Then
                    Yield New QueryToken With {.token = buf.PopAll()(Scan0)}
                End If

                Yield New QueryToken With {.token = t}
            End If
        Next

        If buf > 0 Then
            Yield New QueryToken With {.func = GetParser(buf.PopAll)}
        End If
    End Function

    Private Shared Function GetParser(buf As Token()) As Parser
        Dim name As Token = buf(Scan0)
        Dim args = buf _
            .Skip(2) _
            .Take(buf.Length - 3) _
            .Split(Function(t) t.name = Tokens.comma) _
            .ToArray
        Dim parameters = args.Select(Function(a) a(Scan0).text).ToArray

        Select Case name.text
            Case "css"
                Return New CSSSelector(name.text, parameters)
            Case "attr"
                Return New AttributeSelector(name.text, parameters)
            Case "xpath"
                Return New XPathSelector(name.text, parameters)
            Case Else
                Return New FunctionParser(name.text, parameters)
        End Select
    End Function
End Class
