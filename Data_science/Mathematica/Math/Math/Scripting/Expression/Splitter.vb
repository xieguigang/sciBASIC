#Region "Microsoft.VisualBasic::7c2f3e8a1da36a1c99837e7e58e5bf05, Data_science\Mathematica\Math\Math\Scripting\Expression\Splitter.vb"

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

    '   Total Lines: 59
    '    Code Lines: 46
    ' Comment Lines: 2
    '   Blank Lines: 11
    '     File Size: 1.94 KB


    '     Module Splitter
    ' 
    '         Function: SplitByTopLevelDelimiter
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language

Namespace Scripting.MathExpression

    Module Splitter

        <Extension>
        Friend Function SplitByTopLevelDelimiter(tokens As IEnumerable(Of MathToken), delimiter As MathTokens) As List(Of MathToken())
            Dim blocks As New List(Of MathToken())
            Dim buf As New List(Of MathToken)
            Dim stack As New Stack(Of MathToken)
            Dim tokenVector As MathToken() = tokens.ToArray
            Dim isDelimiter As Func(Of MathToken, Boolean) = Function(t) t.name = delimiter

            If tokenVector.Length = 1 Then
                Return blocks + tokenVector
            End If

            ' 使用最顶层的comma进行分割
            For Each t As MathToken In tokenVector
                Dim add As Boolean = True

                If t.name = MathTokens.Open Then
                    stack.Push(t)
                ElseIf t.name = MathTokens.Close Then
                    If stack.Count = 0 Then
                        Throw New SyntaxErrorException(tokenVector.JoinBy(" "))
                    Else
                        stack.Pop()
                    End If
                End If

                If isDelimiter(t) Then
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
