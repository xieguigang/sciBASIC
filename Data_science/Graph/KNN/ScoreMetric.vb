Namespace KNearNeighbors

    Public MustInherit Class ScoreMetric

        Public Property cutoff As Double

        ''' <summary>
        ''' the score function should produce a positive score value,
        ''' higher score value is better
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public MustOverride Function eval(x As Double(), y As Double()) As Double

        Public Overrides Function ToString() As String
            Return "knn_score_metric();"
        End Function

    End Class
End Namespace