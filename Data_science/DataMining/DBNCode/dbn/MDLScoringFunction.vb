Namespace dbn

    Public Class MDLScoringFunction
        Inherits LLScoringFunction
        Private epsilon As Double = 0.0000000001

        Public Overrides Function evaluate(observations As Observations, transition As Integer, parentNodesPast As IList(Of Integer), parentNodePresent As Integer?, childNode As Integer) As Double

            Dim c As LocalConfiguration = New LocalConfiguration(observations.Attributes, observations.MarkovLag, parentNodesPast, parentNodePresent, childNode)

            Dim score = MyBase.evaluate(observations, transition, parentNodesPast, parentNodePresent, childNode)

            ' regularizer term
            score -= 0.5 * Math.Log(observations.numObservations(transition) + epsilon) * c.NumParameters
            Return score
        End Function

        Public Overrides Function evaluate_2(observations As Observations, transition As Integer, parentNodesPast As IList(Of Integer), parentNodePresent As IList(Of Integer), childNode As Integer) As Double

            Dim c As LocalConfiguration = New LocalConfiguration(observations.Attributes, observations.MarkovLag, parentNodesPast, parentNodePresent, childNode)

            Dim score = MyBase.evaluate_2(observations, transition, parentNodesPast, parentNodePresent, childNode)

            ' regularizer term
            score -= 0.5 * Math.Log(observations.numObservations(transition) + epsilon) * c.NumParameters

            Return score
        End Function

    End Class

End Namespace
