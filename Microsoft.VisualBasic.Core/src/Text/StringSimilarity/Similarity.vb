#Region "Microsoft.VisualBasic::280246689c0256c23d13e0e564e00db2, Microsoft.VisualBasic.Core\src\Text\StringSimilarity\Similarity.vb"

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

    '   Total Lines: 270
    '    Code Lines: 170 (62.96%)
    ' Comment Lines: 63 (23.33%)
    '    - Xml Docs: 92.06%
    ' 
    '   Blank Lines: 37 (13.70%)
    '     File Size: 11.73 KB


    '     Delegate Function
    ' 
    ' 
    '     Module Evaluations
    ' 
    '         Function: Evaluate, tokenEquals, tokenEqualsIgnoreCase
    '         Delegate Function
    ' 
    '             Function: (+4 Overloads) IsOrdered, LevenshteinEvaluate, LevenshteinOrder, StringSelection, (+3 Overloads) TokenOrders
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming.Levenshtein
Imports Microsoft.VisualBasic.ComponentModel.DataStructures.GenericLambda(Of String)
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports std = System.Math

Namespace Text.Similarity

    ''' <summary>
    ''' Summary description for StringMatcher.
    ''' </summary>
    ''' 
    Public Delegate Function ISimilarity(s1 As String, s2 As String) As Double

    ''' <summary>
    ''' text string similarity evaluation module
    ''' </summary>
    Public Module Evaluations

        ReadOnly ignoreCase As New [Default](Of IEquals)(AddressOf tokenEqualsIgnoreCase)

        ''' <summary>
        ''' 两个字符串之间是通过单词的排布的相似度来比较相似度的
        ''' </summary>
        ''' <param name="s1"></param>
        ''' <param name="s2"></param>
        ''' <param name="ignoreCase"></param>
        ''' <param name="cost"></param>
        ''' <param name="dist"></param>
        ''' <returns>
        ''' A similarity score in value between [0,1]
        ''' </returns>
        Public Function Evaluate(s1$, s2$,
                                 Optional ignoreCase As Boolean = True,
                                 Optional cost# = 0.7,
                                 Optional ByRef dist As DistResult = Nothing,
                                 Optional strlen_diff As Boolean = True) As Double

            If String.Equals(s1, s2, If(ignoreCase, StringComparison.OrdinalIgnoreCase, StringComparison.Ordinal)) Then
                Return 1
            End If

            Dim tokenEquals As IEquals = New IEquals(AddressOf Evaluations.tokenEquals) Or Evaluations.ignoreCase.When(ignoreCase)
            Dim len1 = s1.Length
            Dim len2 = s2.Length
            Dim diff As Double = If(strlen_diff, std.Min(len1, len2) / std.Max(len1, len2), 1)

            dist = LevenshteinDistance.ComputeDistance(
                s1.Split,
                s2.Split,
                tokenEquals,
                Function(s) s.FirstOrDefault,
                cost)

            If dist Is Nothing Then
                Return 0
            Else
                Return dist.MatchSimilarity * diff
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
                                            Optional ByRef dist As DistResult = Nothing,
                                            Optional strlen_diff As Boolean = True) As Double
            If ignoreCase Then
                s1 = s1.ToLower
                s2 = s2.ToLower
            End If

            If s1 = s2 Then
                ' 假若是大小写不敏感的，由于前面已经被转换为小写了，
                ' 所以这里直接进行比较
                Return 1
            End If

            dist = LevenshteinDistance.ComputeDistance(s1, s2, cost)

            If dist Is Nothing Then
                Return 0
            Else
                Dim len1 = s1.Length
                Dim len2 = s2.Length
                Dim diff As Double = If(strlen_diff, std.Min(len1, len2) / std.Max(len1, len2), 1)

                Return dist.MatchSimilarity * diff
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

        ''' <summary>
        ''' Make text similariy sort in desc order
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="collection"></param>
        ''' <param name="query"></param>
        ''' <param name="cutoff">
        ''' this text similarity cutoff will filter out some content from the result.
        ''' </param>
        ''' <param name="strlen_diff"></param>
        ''' <returns>
        ''' a sort collection result by the text similarity measurement. this result
        ''' collection may has elements number less than the input <paramref name="collection"/> 
        ''' elements number.
        ''' </returns>
        ''' <remarks>
        ''' this function will removes the item inside the collection
        ''' which its text similarity is smaller than the cutoff of given 
        ''' <paramref name="query"/> text.
        ''' </remarks>
        <Extension>
        Public Function LevenshteinOrder(Of T)(collection As IEnumerable(Of T), query As String, getData As Func(Of T, IEnumerable(Of String)),
                                               Optional ignoreCase As Boolean = True,
                                               Optional cutoff As Double = 0.6,
                                               Optional cost As Double = 0.7,
                                               Optional strlen_diff As Boolean = True) As IEnumerable(Of T)
            Return collection.AsParallel _
                .Select(Function(a)
                            Dim top As Double = Double.MinValue

                            For Each si As String In getData(a)
                                If si Is Nothing Then
                                    Continue For
                                End If

                                Dim score As Double = LevenshteinEvaluate(
                                    query, si,
                                    ignoreCase:=ignoreCase,
                                    cost:=cost,
                                    strlen_diff:=strlen_diff)

                                If score > top Then
                                    top = score
                                End If
                            Next

                            If top > cutoff Then
                                Return (top, a)
                            Else
                                Return (0, a)
                            End If
                        End Function) _
                .Where(Function(a) a.Item1 > 0) _
                .OrderByDescending(Function(a) a.Item1) _
                .Select(Function(a)
                            Return a.Item2
                        End Function)
        End Function

        ''' <summary>
        ''' Get most similar string from the given <paramref name="collection"/>
        ''' </summary>
        ''' <param name="query">the text for the similarity measurement</param>
        ''' <param name="collection">a given string collection for compares with the 
        ''' given <paramref name="query"/> text string.</param>
        ''' <param name="cutoff"></param>
        ''' <param name="ignoreCase"></param>
        ''' <param name="tokenBased"></param>
        ''' <returns></returns>
        Public Function StringSelection(query As String, collection As IEnumerable(Of String),
                                        Optional cutoff# = 0.6,
                                        Optional ignoreCase As Boolean = True,
                                        Optional tokenBased As Boolean = False) As String
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
