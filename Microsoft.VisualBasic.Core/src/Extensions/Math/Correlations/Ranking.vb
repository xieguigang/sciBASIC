#Region "Microsoft.VisualBasic::bf59c1640715c234a303c0c09e5d6247, Microsoft.VisualBasic.Core\src\Extensions\Math\Correlations\Ranking.vb"

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

    '   Total Lines: 333
    '    Code Lines: 153 (45.95%)
    ' Comment Lines: 152 (45.65%)
    '    - Xml Docs: 84.21%
    ' 
    '   Blank Lines: 28 (8.41%)
    '     File Size: 17.17 KB


    '     Module Ranking
    ' 
    ' 
    '         Enum Strategies
    ' 
    ' 
    ' 
    ' 
    '  
    ' 
    '     Function: DenseRanking, FractionalRanking, ModifiedCompetitionRanking, OrdinalRanking, OrdinalRankingOrder
    '               Ranking, StandardCompetitionRanking
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace Math.Correlations

    ''' <summary>
    ''' A **ranking** is a relationship between a set of items such that, for any two items, 
    ''' the first is either 'ranked higher than', 'ranked lower than' or 'ranked equal to' 
    ''' the second. In mathematics, this is known as a weak order or total preorder of objects. 
    ''' It is not necessarily a total order of objects because two different objects can have 
    ''' the same ranking. The rankings themselves are totally ordered. For example, materials 
    ''' are totally preordered by hardness, while degrees of hardness are totally ordered.
    ''' 
    ''' > https://en.wikipedia.org/wiki/Ranking
    ''' </summary>
    Public Module Ranking

        ''' <summary>
        ''' ###### Strategies for assigning rankings
        ''' 
        ''' It is not always possible to assign rankings uniquely. For example, in a race 
        ''' or competition two (or more) entrants might tie for a place in the ranking. 
        ''' When computing an ordinal measurement, two (or more) of the quantities being 
        ''' ranked might measure equal. In these cases, one of the strategies shown below 
        ''' for assigning the rankings may be adopted. A common shorthand way to distinguish 
        ''' these ranking strategies is by the ranking numbers that would be produced for 
        ''' four items, with the first item ranked ahead of the second and third (which 
        ''' compare equal) which are both ranked ahead of the fourth. 
        ''' </summary>
        Public Enum Strategies As Integer
            StandardCompetition = 1224
            ModifiedCompetition = 1334
            DenseRanking = 1223
            OrdinalRanking = 1234
            FractionalRanking = 1 + 2.5 + 2.5 + 4
        End Enum

        ''' <summary>
        ''' 函数返回与输入的序列中的元素相同顺序的排序的得分
        ''' </summary>
        ''' <typeparam name="C"></typeparam>
        ''' <param name="list"></param>
        ''' <param name="strategy"></param>
        ''' <param name="desc"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Ranking(Of C As IComparable)(list As IEnumerable(Of C),
                                                     Optional strategy As Strategies = Strategies.OrdinalRanking,
                                                     Optional desc As Boolean = False) As Double()

            If list Is Nothing Then
                Return Nothing
            End If

            If strategy = Strategies.OrdinalRanking Then
                Return list.OrdinalRanking(desc)
            ElseIf strategy = Strategies.DenseRanking Then
                Return list.DenseRanking(desc)
            ElseIf strategy = Strategies.FractionalRanking Then
                Return list.FractionalRanking(desc)
            ElseIf strategy = Strategies.StandardCompetition Then
                Return list.StandardCompetitionRanking(desc)
            ElseIf strategy = Strategies.ModifiedCompetition Then
                Return list.ModifiedCompetitionRanking(desc)
            Else
                Throw New NotImplementedException
            End If
        End Function

        ''' <summary>
        ''' ###### Modified competition ranking ("1334" ranking)
        ''' 
        ''' Sometimes, competition ranking is done by leaving the gaps in the ranking numbers before the sets 
        ''' of equal-ranking items (rather than after them as in standard competition ranking).[where?] The 
        ''' number of ranking numbers that are left out in this gap remains one less than the number of items that 
        ''' compared equal. Equivalently, each item's ranking number is equal to the number of items ranked equal 
        ''' to it or above it. This ranking ensures that a competitor only comes second if they score higher than 
        ''' all but one of their opponents, third if they score higher than all but two of their opponents, etc.
        ''' 
        ''' Thus if A ranks ahead of B and C (which compare equal) which are both ranked head of D, then A gets 
        ''' ranking number 1 ("first"), B gets ranking number 3 ("joint third"), C also gets ranking number 3 
        ''' ("joint third") and D gets ranking number 4 ("fourth"). In this case, nobody would get ranking number 
        ''' 2 ("second") and that would be left as a gap.
        ''' </summary>
        ''' <typeparam name="C"></typeparam>
        ''' <param name="list"></param>
        ''' <returns></returns>
        <Extension> Public Function ModifiedCompetitionRanking(Of C As IComparable)(list As IEnumerable(Of C), Optional desc As Boolean = False) As Double()
            Dim array = list _
                .SeqIterator _
                .ToDictionary(Function(x) x,
                              Function(i)
                                  Return i.i
                              End Function)
            Dim asc() = array.Keys _
                .Sort(Function(x) x.value, desc) _
                .ToArray
            Dim ranks#() = New Double(asc.Length - 1) {}
            Dim rank% = 0
            Dim gaps = array.Keys _
                .GroupBy(Function(x) x.value) _
                .ToDictionary(Function(x) x.First.value,
                              Function(g)
                                  Return g.Count
                              End Function)

            ' 使用Nothing的时候，对于数字而言，会是0，则会和0冲突，
            ' 使用最大的值则完全可以避免这个问题了
            Dim previous As C = asc.Last.value

            For i As Integer = 0 To asc.Length - 1
                ' obj -> original_i -> rank
                With asc(i)
                    If .value.CompareTo(previous) = 0 Then
                        ' rank += 0
                    ElseIf gaps.ContainsKey(.value) Then
                        previous = .value
                        rank += gaps(.value)
                    Else
                        rank += 1
                    End If
                End With

                ranks(array(asc(i))) = rank
            Next

            Return ranks
        End Function

        ''' <summary>
        ''' ###### Standard competition ranking ("1224" ranking)
        ''' 
        ''' In competition ranking, items that compare equal receive the same ranking number, and then a gap 
        ''' is left in the ranking numbers. The number of ranking numbers that are left out in this gap is 
        ''' one less than the number of items that compared equal. Equivalently, each item's ranking number 
        ''' is 1 plus the number of items ranked above it. This ranking strategy is frequently adopted for 
        ''' competitions, as it means that if two (or more) competitors tie for a position in the ranking, 
        ''' the position of all those ranked below them is unaffected (i.e., a competitor only comes second if 
        ''' exactly one person scores better than them, third if exactly two people score better than them, 
        ''' fourth if exactly three people score better than them, etc.).
        ''' 
        ''' Thus if A ranks ahead of B and C (which compare equal) which are both ranked ahead of D, then A 
        ''' gets ranking number 1 ("first"), B gets ranking number 2 ("joint second"), C also gets ranking 
        ''' number 2 ("joint second") and D gets ranking number 4 ("fourth").
        ''' </summary>
        ''' <typeparam name="C"></typeparam>
        ''' <param name="list"></param>
        ''' <returns></returns>
        <Extension> Public Function StandardCompetitionRanking(Of C As IComparable)(list As IEnumerable(Of C), Optional desc As Boolean = False) As Double()
            Dim array = list _
                .SeqIterator _
                .ToDictionary(Function(x) x,
                              Function(i)
                                  Return i.i
                              End Function)
            Dim asc() = array.Keys _
                .Sort(Function(x) x.value, desc) _
                .ToArray
            Dim ranks#() = New Double(asc.Length - 1) {}
            Dim rank% = 1
            Dim gap% = 1

            For i As Integer = 0 To asc.Length - 2
                With asc(i)
                    ' obj -> original_i -> rank
                    ranks(array(asc(i))) = rank

                    If .value.CompareTo(asc(i + 1).value) <> 0 Then
                        rank += gap
                        gap = 1
                    Else
                        gap += 1
                    End If
                End With
            Next

            ranks(array(asc.Last)) = rank

            Return ranks
        End Function

        ''' <summary>
        ''' ###### Dense ranking ("1223" ranking)
        ''' 
        ''' In dense ranking, items that compare equal receive the same ranking number, and the next item(s) 
        ''' receive the immediately following ranking number. Equivalently, each item's ranking number is 1 
        ''' plus the number of items ranked above it that are distinct with respect to the ranking order.
        ''' 
        ''' Thus if A ranks ahead of B and C (which compare equal) which are both ranked ahead of D, then A 
        ''' gets ranking number 1 ("first"), B gets ranking number 2 ("joint second"), C also gets ranking 
        ''' number 2 ("joint second") and D gets ranking number 3 ("third").
        ''' </summary>
        ''' <typeparam name="C"></typeparam>
        ''' <param name="list"></param>
        ''' <returns></returns>
        <Extension> Public Function DenseRanking(Of C As IComparable)(list As IEnumerable(Of C), Optional desc As Boolean = False) As Double()
            Dim array = list _
                .SeqIterator _
                .ToDictionary(Function(x) x,
                              Function(i)
                                  Return i.i
                              End Function)
            Dim asc() = array.Keys _
                .Sort(Function(x) x.value, desc) _
                .ToArray
            Dim ranks#() = New Double(asc.Length - 1) {}
            Dim rank% = 1

            For i As Integer = 0 To asc.Length - 2
                With asc(i)
                    ' obj -> original_i -> rank
                    ranks(array(asc(i))) = rank

                    If .value.CompareTo(asc(i + 1).value) <> 0 Then
                        rank += 1
                    End If
                End With
            Next

            ranks(array(asc.Last)) = rank

            Return ranks
        End Function

        ''' <summary>
        ''' ###### Ordinal ranking ("1234" ranking)
        ''' 
        ''' In ordinal ranking, all items receive distinct ordinal numbers, including items that compare equal. 
        ''' The assignment of distinct ordinal numbers to items that compare equal can be done at random, 
        ''' or arbitrarily, but it is generally preferable to use a system that is arbitrary but consistent, 
        ''' as this gives stable results if the ranking is done multiple times. An example of an arbitrary but 
        ''' consistent system would be to incorporate other attributes into the ranking order (such as 
        ''' alphabetical ordering of the competitor's name) to ensure that no two items exactly match.
        ''' 
        ''' With this strategy, if A ranks ahead of B and C (which compare equal) which are both ranked ahead of D, 
        ''' then A gets ranking number 1 ("first") and D gets ranking number 4 ("fourth"), and either B gets 
        ''' ranking number 2 ("second") and C gets ranking number 3 ("third") or C gets ranking number 2 ("second") 
        ''' and B gets ranking number 3 ("third").
        ''' 
        ''' In computer data processing, ordinal ranking is also referred to as "row numbering".
        ''' </summary>
        ''' <typeparam name="C"></typeparam>
        ''' <param name="list"></param>
        ''' <returns></returns>
        <Extension>
        Public Function OrdinalRanking(Of C As IComparable)(list As IEnumerable(Of C), Optional desc As Boolean = False) As Double()
            Return list.OrdinalRankingOrder(desc).Select(Function(x) CDbl(x)).ToArray
        End Function

        ''' <summary>
        ''' ###### Ordinal ranking ("1234" ranking)
        ''' 
        ''' In ordinal ranking, all items receive distinct ordinal numbers, including items that compare equal. 
        ''' The assignment of distinct ordinal numbers to items that compare equal can be done at random, 
        ''' or arbitrarily, but it is generally preferable to use a system that is arbitrary but consistent, 
        ''' as this gives stable results if the ranking is done multiple times. An example of an arbitrary but 
        ''' consistent system would be to incorporate other attributes into the ranking order (such as 
        ''' alphabetical ordering of the competitor's name) to ensure that no two items exactly match.
        ''' 
        ''' With this strategy, if A ranks ahead of B and C (which compare equal) which are both ranked ahead of D, 
        ''' then A gets ranking number 1 ("first") and D gets ranking number 4 ("fourth"), and either B gets 
        ''' ranking number 2 ("second") and C gets ranking number 3 ("third") or C gets ranking number 2 ("second") 
        ''' and B gets ranking number 3 ("third").
        ''' 
        ''' In computer data processing, ordinal ranking is also referred to as "row numbering".
        ''' </summary>
        ''' <typeparam name="C"></typeparam>
        ''' <param name="list"></param>
        ''' <returns></returns>
        <Extension>
        Public Function OrdinalRankingOrder(Of C As IComparable)(list As IEnumerable(Of C), Optional desc As Boolean = False) As Integer()
            Dim array = list _
                .SeqIterator _
                .ToDictionary(Function(x) x,
                              Function(i)
                                  Return i.i
                              End Function)
            Dim asc() = array.Keys _
                .Sort(Function(x) x.value, desc) _
                .ToArray
            Dim ranks%() = New Integer(asc.Length - 1) {}
            Dim rank% = 1

            For i As Integer = 0 To asc.Length - 1
                ' obj -> original_i -> rank
                ranks(array(asc(i))) = rank
                rank += 1
            Next

            Return ranks
        End Function

        ''' <summary>
        ''' ###### Fractional ranking ("1 2.5 2.5 4" ranking)
        ''' 
        ''' Items that compare equal receive the same ranking number, which is the mean 
        ''' of what they would have under ordinal rankings. Equivalently, the ranking 
        ''' number of 1 plus the number of items ranked above it plus half the number 
        ''' of items equal to it. This strategy has the property that the sum of the 
        ''' ranking numbers is the same as under ordinal ranking. For this reason, it 
        ''' is used in computing Borda counts and in statistical tests (see below).
        ''' 
        ''' Thus if A ranks ahead of B and C (which compare equal) which are both ranked 
        ''' ahead of D, then A gets ranking number 1 ("first"), B and C each get ranking 
        ''' number 2.5 (average of "joint second/third") and D gets ranking number 4 
        ''' ("fourth").
        ''' 
        ''' Here is an example: Suppose you have the data set 1.0, 1.0, 2.0, 3.0, 3.0, 4.0, 5.0, 5.0, 5.0.
        ''' The ordinal ranks are 1, 2, 3, 4, 5, 6, 7, 8, 9.
        ''' For v = 1.0, the fractional rank is the average of the ordinal ranks: (1 + 2) / 2 = 1.5. 
        ''' In a similar manner, for v = 5.0, the fractional rank is (7 + 8 + 9) / 3 = 8.0.
        ''' Thus the fractional ranks are: 1.5, 1.5, 3.0, 4.5, 4.5, 6.0, 8.0, 8.0, 8.0
        ''' </summary>
        ''' <typeparam name="C"></typeparam>
        ''' <param name="list"></param>
        ''' <returns></returns>
        <Extension> Public Function FractionalRanking(Of C As IComparable)(list As IEnumerable(Of C), Optional desc As Boolean = False) As Double()
            Dim vector As C() = list.ToArray
            Dim array As SeqValue(Of C)() = vector.SeqIterator.ToArray
            Dim ranks#() = vector.OrdinalRanking(desc)
            Dim equals = array.GroupBy(Function(x) x.value)

            For Each g As IGrouping(Of C, SeqValue(Of C)) In equals
                Dim avgRanks# = Aggregate i
                                In g
                                Into Average(ranks(i))

                For Each i As SeqValue(Of C) In g
                    ranks(i.i) = avgRanks
                Next
            Next

            Return ranks
        End Function
    End Module
End Namespace
