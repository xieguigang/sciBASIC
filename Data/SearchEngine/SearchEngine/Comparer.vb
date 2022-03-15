#Region "Microsoft.VisualBasic::1d8022d685200f14f00ab72d6ff092fb, sciBASIC#\Data\SearchEngine\SearchEngine\Comparer.vb"

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

    '   Total Lines: 67
    '    Code Lines: 40
    ' Comment Lines: 18
    '   Blank Lines: 9
    '     File Size: 2.14 KB


    ' Module IComparer
    ' 
    '     Function: Evaluate, FindAll, Match, MatchAll
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Text

Public Module IComparer

    <Extension>
    Public Function Evaluate(query As String, x As Object) As Boolean
        Dim type As Type = x.GetType

        If type.Equals(GetType(String)) Then
            Return Build(query$) _
                .Evaluate(New IObject(GetType(Text)),
                          New Text With {
                            .Text = DirectCast(x, String)
                          })
        Else
            Return Build(query$).Evaluate(New IObject(type), x)
        End If
    End Function

    ''' <summary>
    ''' Does the text data can be matched by the query expression?
    ''' </summary>
    ''' <param name="text$"></param>
    ''' <param name="query$"></param>
    ''' <returns></returns>
    <Extension> Public Function Match(text$, query$) As Boolean
        Return Build(query).Evaluate(
            New IObject(GetType(Text)),
            New Text With {
                .Text = text$
            })
    End Function

    ''' <summary>
    ''' All of the world tokens in the input <paramref name="query"/> should match in one of the fileds in target object.
    ''' </summary>
    ''' <param name="query$"></param>
    ''' <param name="x"></param>
    ''' <returns></returns>
    <Extension>
    Public Function FindAll(query$, x As Object) As Boolean
        Dim tokens$() = query.Split(" "c, ASCII.TAB)
        Dim exp$ = query.JoinBy(" AND ")

        Return exp.Evaluate(x)
    End Function

    ''' <summary>
    ''' All of the world tokens in the input <paramref name="query"/> should match in any fields in target object.
    ''' </summary>
    ''' <param name="query$"></param>
    ''' <param name="x"></param>
    ''' <returns></returns>
    <Extension>
    Public Function MatchAll(query$, x As Object) As Boolean
        Dim tokens$() = query.Split(" "c, ASCII.TAB)

        For Each t$ In tokens
            If Not t$.Evaluate(x) Then
                Return False
            End If
        Next

        Return True
    End Function
End Module
