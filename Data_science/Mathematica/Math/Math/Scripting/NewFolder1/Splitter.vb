#Region "Microsoft.VisualBasic::4e2baca6454a77836d72934b1819bc16, R#\Interpreter\Syntax\SyntaxTree\Splitter.vb"

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

    '     Module Splitter
    ' 
    '         Function: SplitByTopLevelDelimiter
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Interpreter.SyntaxParser

    Module Splitter

        <Extension>
        Friend Function SplitByTopLevelDelimiter(tokens As IEnumerable(Of Token), delimiter As TokenType, ' <Out> ByRef [error] As Exception,
                                                 Optional includeKeyword As Boolean = False,
                                                 Optional tokenText$ = Nothing) As List(Of Token())
            Dim blocks As New List(Of Token())
            Dim buf As New List(Of Token)
            Dim stack As New Stack(Of Token)
            Dim isDelimiter As Func(Of Token, Boolean)
            Dim tokenVector As Token() = tokens.ToArray

            If tokenVector.Length = 1 Then
                Return blocks + tokenVector
            End If

            If tokenText Is Nothing Then
                isDelimiter = Function(t) t.name = delimiter
            Else
                isDelimiter = Function(t)
                                  Return t.name = delimiter AndAlso t.text = tokenText
                              End Function
            End If

            ' 使用最顶层的comma进行分割
            For Each t As Token In tokenVector
                Dim add As Boolean = True

                If t.name = TokenType.open Then
                    stack.Push(t)
                ElseIf t.name = TokenType.close Then
                    If stack.Count = 0 Then
                        Throw New SyntaxErrorException(tokenVector.JoinBy(" "))
                    Else
                        stack.Pop()
                    End If
                End If

                If isDelimiter(t) OrElse (includeKeyword AndAlso t.name = TokenType.keyword) Then
                    If stack.Count = 0 Then
                        ' 这个是最顶层的分割
                        If buf > 0 Then
                            blocks += buf.PopAll
                        End If

                        blocks += {t}
                        add = False
                    End If
                End If

                If add Then
                    buf += t
                End If
            Next

            If buf > 0 Then
                Return blocks + buf.ToArray
            Else
                Return blocks
            End If
        End Function

    End Module
End Namespace
