
Imports System.Runtime.CompilerServices

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

        End Function
    End Module
End Namespace