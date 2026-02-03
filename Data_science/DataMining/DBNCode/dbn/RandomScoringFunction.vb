Namespace dbn

    Public Class RandomScoringFunction
        Implements ScoringFunction

        Public Sub New()
            ' TODO Auto-generated constructor stub
        End Sub

        Public Overridable Function evaluate(observations As Observations, transition As Integer, parentNodesPast As IList(Of Integer), childNode As Integer) As Double Implements ScoringFunction.evaluate
            Return evaluate(observations, transition, parentNodesPast, Nothing, childNode)
        End Function

        Public Overridable Function evaluate(observations As Observations, transition As Integer, parentNodesPast As IList(Of Integer), parentNodePresent As Integer?, childNode As Integer) As Double Implements ScoringFunction.evaluate
            Dim r As Random = New Random()
            Return -100 + (0 + 100) * r.NextDouble()
        End Function

        Public Overridable Function evaluate_2(observations As Observations, transition As Integer, parentNodesPast As IList(Of Integer), parentNodePresent As IList(Of Integer), childNode As Integer) As Double Implements ScoringFunction.evaluate_2
            Dim r As Random = New Random()
            Return -100 + (0 + 100) * r.NextDouble()
        End Function

        Public Overridable Function evaluate(observations As Observations, parentNodesPast As IList(Of Integer), parentNodePresent As Integer?, childNode As Integer) As Double Implements ScoringFunction.evaluate
            Return evaluate(observations, -1, parentNodesPast, Nothing, childNode)
        End Function

        Public Overridable Function evaluate_2(observations As Observations, parentNodesPast As IList(Of Integer), parentNodePresent As IList(Of Integer), childNode As Integer) As Double Implements ScoringFunction.evaluate_2
            Return evaluate_2(observations, -1, parentNodesPast, parentNodePresent, childNode)
        End Function

        Public Overridable Function evaluate(observations As Observations, parentNodesPast As IList(Of Integer), childNode As Integer) As Double Implements ScoringFunction.evaluate
            Return evaluate(observations, parentNodesPast, Nothing, childNode)
        End Function

    End Class

End Namespace
