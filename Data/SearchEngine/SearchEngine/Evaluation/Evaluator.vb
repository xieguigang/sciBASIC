#Region "Microsoft.VisualBasic::fb940cabe15e5bbbad8359175d857d2e, sciBASIC#\Data\SearchEngine\SearchEngine\Evaluation\Evaluator.vb"

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

    '   Total Lines: 137
    '    Code Lines: 103
    ' Comment Lines: 15
    '   Blank Lines: 19
    '     File Size: 5.53 KB


    ' Module Evaluator
    ' 
    '     Function: CompileMustSearch, CompileNormalSearch, ContainsAny, IsAnyExpression, IsMustExpression
    '               MustxContains
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming.Levenshtein
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text

''' <summary>
''' 进行字符串计算的具体过程
''' </summary>
Public Module Evaluator

    ReadOnly __symbolsNoWildcards As Char() =
        ASCII.Symbols _
        .Where(Function(x) x <> "*"c AndAlso x <> "%"c) _
        .Join(" "c, ASCII.TAB) _
        .ToArray

    ReadOnly __allASCIISymbols As Char() =
        __symbolsNoWildcards _
        .Join("*"c, "%"c) _
        .ToArray

    ''' <summary>
    ''' 大小写敏感，在使用之前要先用tolower或者toupper
    ''' </summary>
    ''' <param name="term$"></param>
    ''' <param name="searchIn$"></param>
    ''' <returns></returns>
    Public Function ContainsAny(term$,
                                searchIn$,
                                Optional allowInStr As Boolean = True,
                                Optional caseSensitive As Boolean = True) As Boolean
        Return term$.CompileNormalSearch(allowInStr, caseSensitive)(searchIn$)
    End Function

    <Extension>
    Public Function CompileNormalSearch(term$,
                                        Optional allowInStr As Boolean = True,
                                        Optional caseSensitive As Boolean = True) As Func(Of String, Boolean)
        If term.First = "#"c Then
            Dim regexp As New Regex(Mid(term$, 2), RegexICSng)
            Return Function(searchIn$) regexp.Match(searchIn$).Success
        End If

        If term.First = "~"c Then  ' Levenshtein match
            Dim exp$ = Mid(term.ToLower, 2)
            Dim t1$() = exp.Split(__allASCIISymbols)
            Dim query%() = exp.Select(AddressOf AscW).ToArray

            If t1$.Length = 1 Then ' 只有一个单词，则做匹配的时候需要一个单词一个单词的进行匹配
                Return Function(searchIn$)
                           For Each t$ In searchIn$.Split(" "c)
                               Dim dist As DistResult = ComputeDistance(
                                   query%, t$.ToLower, )

                               If (Not dist Is Nothing) AndAlso dist.MatchSimilarity >= 0.8 Then
                                   Return True
                               End If
                           Next

                           Return False
                       End Function
            Else ' 为一整个句子，则直接计算整个句子
                Return Function(searchIn$)
                           Dim dist As DistResult = ComputeDistance(
                               query%, searchIn$.ToLower, )

                           If dist Is Nothing OrElse dist.MatchSimilarity < 0.8 Then
                               Return False
                           Else
                               Return True
                           End If
                       End Function
            End If
        Else
            Dim t1$() = term.Split(__symbolsNoWildcards)  ' term
            Return Function(searchIn$)
                       Dim t2$() = searchIn.Split(__allASCIISymbols) ' 目标

                       For Each t$ In t1$
                           If t2.Located(t$, caseSensitive, fuzzy:=allowInStr) <> -1 Then
                               Return True
                           ElseIf t2.WildcardsLocated(t$, caseSensitive) <> -1 Then
                               Return True
                           End If
                       Next

                       Return False
                   End Function
        End If
    End Function

    ''' <summary>
    ''' 假若是一个单词，则要整个单词都相等才行，假若为组合词，则直接匹配
    ''' </summary>
    ''' <param name="term$"></param>
    ''' <param name="searchIn$"></param>
    ''' <returns></returns>
    Public Function MustxContains(term$, searchIn$, Optional caseSensitive As Boolean = True) As Boolean
        Return term$.CompileMustSearch(caseSensitive)(searchIn$)
    End Function

    <Extension>
    Public Function CompileMustSearch(term$, Optional caseSensitive As Boolean = True) As Func(Of String, Boolean)
        Dim t1$() = term.Split(__allASCIISymbols)

        If t1$.Length = 1 Then ' 必须要整个单词都被匹配上
            Return Function(searchIn$)
                       Dim t2$() = searchIn.Split(__allASCIISymbols) ' 目标

                       For Each t$ In t1$
                           If t2.Located(t$, caseSensitive, False) <> -1 Then
                               Return True
                           End If
                       Next

                       Return False
                   End Function
        Else ' 对于含有多个tokens的查询，则直接比较
            Dim compare As CompareMethod = If(
                caseSensitive,
                CompareMethod.Binary,
                CompareMethod.Text)
            Return Function(searchIn$) InStr(searchIn, term$, compare) > 0
        End If
    End Function

    <Extension>
    Public Function IsMustExpression(s$) As Boolean
        Return s.First = """"c AndAlso s.Last = """"c
    End Function

    <Extension>
    Public Function IsAnyExpression(s$) As Boolean
        Return s.First = "'"c AndAlso s.Last = "'"c
    End Function
End Module
