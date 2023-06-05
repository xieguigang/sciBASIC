Imports System

Namespace HDBSCAN.Hdbscanstar
    ''' <summary>
    ''' Simple storage class that keeps the outlier score, core distance, and id (index) for a single point.
    ''' OutlierScores are sorted in ascending order by outlier score, with core distances used to break
    ''' outlier score ties, and ids used to break core distance ties.
    ''' </summary>
    Public Class OutlierScore
        Implements IComparable(Of OutlierScore)

        Private ReadOnly _coreDistance As Double

        Public Property Score As Double
        Public Property Id As Integer

        ''' <summary>
        ''' Creates a new OutlierScore for a given point.
        ''' </summary>
        ''' <param name="score">The outlier score of the point</param>
        ''' <param name="coreDistance">The point's core distance</param>
        ''' <param name="id">The id (index) of the point</param>
        Public Sub New(score As Double, coreDistance As Double, id As Integer)
            Me.Score = score
            _coreDistance = coreDistance
            Me.Id = id
        End Sub

        Public Function CompareTo(other As OutlierScore) As Integer Implements IComparable(Of OutlierScore).CompareTo
            If Score > other.Score Then Return 1

            If Score < other.Score Then Return -1

            If _coreDistance > other._coreDistance Then Return 1

            If _coreDistance < other._coreDistance Then Return -1

            Return Id - other.Id
        End Function
    End Class
End Namespace
