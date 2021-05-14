#Region "Microsoft.VisualBasic::15a14075c62b3a7bcfa84df211f7d7da, Microsoft.VisualBasic.Core\src\Text\StringSimilarity\Similarity.vb"

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

    '     Delegate Function
    ' 
    ' 
    '     Module Evaluations
    ' 
    '         Function: Evaluate, tokenEquals, tokenEqualsIgnoreCase
    '         Delegate Function
    ' 
    '             Function: (+4 Overloads) IsOrdered, LevenshteinEvaluate, StringSelection, (+3 Overloads) TokenOrders
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming.Levenshtein
Imports Microsoft.VisualBasic.GenericLambda(Of String)
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default

Namespace Text.Similarity

    ''' <summary>
    ''' Summary description for StringMatcher.
    ''' </summary>
    ''' 
    Public Delegate Function ISimilarity(s1 As String, s2 As String) As Double

    Public Module Evaluations

        ReadOnly ignoreCase As New [Default](Of IEquals)(AddressOf tokenEqualsIgnoreCase)

        ''' <summary>
        ''' 两个字符串之间是通过单词的排布的相似度来比较相似度的
        ''' </summary>
        ''' <param name="s1"></param>
        ''' <param name="s2"></param>
        ''' <param name="ignoreCase"></param>
        ''' <param name="cost#"></param>
        ''' <param name="dist"></param>
        ''' <returns></returns>
        Public Function Evaluate(s1$, s2$,
                                 Optional ignoreCase As Boolean = True,
                                 Optional cost# = 0.7,
                                 Optional ByRef dist As DistResult = Nothing) As Double

            If String.Equals(s1, s2, If(ignoreCase, StringComparison.OrdinalIgnoreCase, StringComparison.Ordinal)) Then
                Return 1
            End If

            Dim tokenEquals As IEquals = New IEquals(AddressOf Evaluations.tokenEquals) Or Evaluations.ignoreCase.When(ignoreCase)

            dist = LevenshteinDistance.ComputeDistance(
                s1.Split,
                s2.Split,
                tokenEquals,
                Function(s) s.FirstOrDefault,
                cost)

            If dist Is Nothing Then
                Return 0
            Else
                Return dist.MatchSimilarity
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function tokenEquals(w1$, w2$) As Boolean
            Return w1$ = w2$
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function tokenEqualsIgnoreCase(w1$, w2$) As Boolean
            Return String.Equals(w1, w2, StringComparison.OrdinalIgnoreCase)
        End Function

        Public Delegate Function IEvaluate(s1$, s2$, ignoreCase As Boolean, cost#, ByRef dist As DistResult) As Double

        ''' <summary>
        ''' 计算字符串，这个是直接通过计算字符而非像<see cref="Evaluate"/>方法之中计算单词的
        ''' </summary>
        ''' <param name="s1$"></param>
        ''' <param name="s2$"></param>
        ''' <param name="ignoreCase"></param>
        ''' <param name="cost#"></param>
        ''' <param name="dist"></param>
        ''' <returns></returns>
        Public Function LevenshteinEvaluate(s1$, s2$,
                                            Optional ignoreCase As Boolean = True,
                                            Optional cost# = 0.7,
                                            Optional ByRef dist As DistResult = Nothing) As Double
            If ignoreCase Then
                s1 = s1.ToLower
                s2 = s2.ToLower
            End If

            If s1 = s2 Then ' 假若是大小写不敏感的，由于前面已经被转换为小写了，所以这里直接进行比较
                Return 1
            End If

            dist = LevenshteinDistance.ComputeDistance(s1, s2, cost)

            If dist Is Nothing Then
                Return 0
            Else
                Return dist.MatchSimilarity
            End If
        End Function

        ''' <summary>
        ''' 以s1为准则，将s2进行比较，返回s2之中的单词在s1之中的排列顺序
        ''' </summary>
        ''' <param name="s1"></param>
        ''' <param name="s2"></param>
        ''' <returns>序列之中的-1表示s2之中的单词在s1之中不存在</returns>
        Public Function TokenOrders(s1 As String, s2 As String, Optional caseSensitive As Boolean = False) As Integer()
            Dim t1$() = s1.Split
            Return t1$.TokenOrders(s2, caseSensitive)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function TokenOrders(s1$(), s2$, Optional caseSensitive As Boolean = False) As Integer()
            Return TokenOrders(s1, s2.Split.Distinct, caseSensitive) ' 假若有重复的字符串出现，则肯定不会有顺序排布的结果，将重复的去掉
        End Function

        <Extension>
        Public Function TokenOrders(s1$(), s2 As IEnumerable(Of String), Optional caseSensitive As Boolean = False, Optional fuzzy As Boolean = True) As Integer()
            Dim orders As New List(Of Integer)

            For Each t$ In s2
                orders += s1.Located(t$, caseSensitive, fuzzy)
            Next

            Return orders
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function IsOrdered(s1$(), s2$, Optional caseSensitive As Boolean = False) As Boolean
            Return s1.IsOrdered(s2.Split, caseSensitive)
        End Function

        ''' <summary>
        ''' 查看<paramref name="s2"/>之中的字符串的顺序是否是在<paramref name="s1"/>之中按顺序排序的
        ''' </summary>
        ''' <param name="s1$"></param>
        ''' <param name="s2$"></param>
        ''' <param name="caseSensitive"></param>
        ''' <returns></returns>
        <Extension>
        Public Function IsOrdered(s1$(), s2$(), Optional caseSensitive As Boolean = False, Optional fuzzy As Boolean = True) As Boolean
            Dim orders%() = s1.TokenOrders(s2, caseSensitive, fuzzy)
            orders = orders.Where(Function(x) x <> -1).ToArray

            If orders.Length = 0 Then  ' 这里是完全比对不上的情况，则肯定是False
                Return False
            End If

            ' 还有一个比对上的怎么办？？？
            If orders.SequenceEqual(orders.OrderBy(Function(x) x)) Then
                ' 假若排序前和排序后的元素仍然每一个元素都相等，则是说明s2是和s1的排序是一样的
                Return True
            Else
                Return False
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function IsOrdered(s1$, s2$, Optional caseSensitive As Boolean = False) As Boolean
            Return s1.Split.IsOrdered(s2$, caseSensitive)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function IsOrdered(s1$, s2$(), Optional caseSensitive As Boolean = False, Optional fuzzy As Boolean = True) As Boolean
            Return s1.Split.IsOrdered(s2$, caseSensitive, fuzzy)
        End Function

        Public Function StringSelection(query As String, collection As IEnumerable(Of String), Optional cutoff# = 0.6, Optional ignoreCase As Boolean = True, Optional tokenBased As Boolean = False) As String
            Dim compare As IEvaluate

            If tokenBased Then
                compare = AddressOf Evaluate
            Else
                compare = AddressOf LevenshteinEvaluate
            End If

            Dim LQuery = From s As String
                         In collection.AsParallel
                         Let score As Double = compare(query, s, ignoreCase, 0.7, Nothing)
                         Where score >= cutoff
                         Select s,
                             score
                         Order By score Descending
            Dim result = LQuery.FirstOrDefault

            If result Is Nothing Then
                Return Nothing
            Else
                Return result.s
            End If
        End Function
    End Module
End Namespace
