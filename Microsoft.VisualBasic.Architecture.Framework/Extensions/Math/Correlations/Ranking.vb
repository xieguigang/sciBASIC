
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace Mathematical.Correlations

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

        <Extension>
        Public Function Ranking(Of C As IComparable)(list As IEnumerable(Of C), Optional strategy As Strategies = Strategies.OrdinalRanking) As Double()
            If strategy = Strategies.OrdinalRanking Then
                Return list.OrdinalRanking
            Else
                Throw New NotImplementedException
            End If
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
        <Extension> Public Function OrdinalRanking(Of C As IComparable)(list As IEnumerable(Of C)) As Double()
            Dim array = list _
                .SeqIterator _
                .ToDictionary(Function(x) x,
                              Function(i) i.i)
            Dim desc() = array _
                .Keys _
                .OrderByDescending(Function(x) x.value) _
                .ToArray
            Dim ranks#() = New Double(desc.Length - 1) {}
            Dim rank% = 1

            For i As Integer = 0 To desc.Length - 1
                ' obj -> original_i -> rank
                ranks(array(desc(i))) = rank
                rank += 1
            Next

            Return ranks
        End Function
    End Module
End Namespace