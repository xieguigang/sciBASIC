Namespace ComponentModel.Evaluation

    ''' <summary>
    ''' Class encoding a member of a ranked set of labels.
    ''' </summary>
    Public Class RankPair : Implements IComparable(Of RankPair)

        ''' <summary>
        ''' The score for this pair.
        ''' </summary>
        Public ReadOnly Property Score As Double

        ''' <summary>
        ''' The Label for this pair.
        ''' </summary>
        Public ReadOnly Property Label As Double

        ''' <summary>
        ''' Constructor.
        ''' </summary>
        ''' <param name="score">Score for this pair</param>
        ''' <param name="label">Label associated with the given score</param>
        Public Sub New(score As Double, label As Double)
            _Score = score
            _Label = label
        End Sub

#Region "IComparable<RankPair> Members"

        ''' <summary>
        ''' Compares this pair to another.  It will end up in a sorted list in decending score order.
        ''' </summary>
        ''' <param name="other">The pair to compare to</param>
        ''' <returns>Whether this should come before or after the argument</returns>
        Public Function CompareTo(other As RankPair) As Integer Implements IComparable(Of RankPair).CompareTo
            Return other.Score.CompareTo(Score)
        End Function

#End Region

        ''' <summary>
        ''' Returns a string representation of this pair.
        ''' </summary>
        ''' <returns>A string in the for Score:Label</returns>
        Public Overrides Function ToString() As String
            Return String.Format("{0}:{1}", Score, Label)
        End Function
    End Class

End Namespace