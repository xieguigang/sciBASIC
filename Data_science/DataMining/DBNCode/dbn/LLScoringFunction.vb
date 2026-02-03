Namespace dbn

    Public Class LLScoringFunction
        Implements ScoringFunction

        Public Overridable Function evaluate(observations As Observations, transition As Integer, parentNodesPast As IList(Of Integer), childNode As Integer) As Double Implements ScoringFunction.evaluate
            Return evaluate(observations, transition, parentNodesPast, Nothing, childNode)
        End Function

        Public Overridable Function evaluate(observations As Observations, transition As Integer, parentNodesPast As IList(Of Integer), parentNodePresent As Integer?, childNode As Integer) As Double Implements ScoringFunction.evaluate

            Dim c As LocalConfiguration = New LocalConfiguration(observations.Attributes, observations.MarkovLag, parentNodesPast, parentNodePresent, childNode)

            Dim score As Double = 0

            Do

                c.ConsiderChild = False
                Dim Nij = observations.count(c, transition)
                '			System.out.println("Node: " + childNode + " Parents" + parentNodesPast.toString() + " ParentPresent: " +  parentNodePresent + " NIJ: " + Nij);
                c.ConsiderChild = True

                Do

                    Dim Nijk = observations.count(c, transition)
                    '				System.out.println("Configuration: " + c + " NIJK: " + Nijk);
                    If CLng(Math.Round(Nijk * 1000.0R, MidpointRounding.AwayFromZero)) / 1000.0R <> 0 AndAlso Nijk <> Nij Then
                        score += Nijk * (Math.Log(Nijk) - Math.Log(Nij))
                        '					if(Double.isNaN(score)) {
                        '						System.out.println("nijk: " + Nijk + " nij:" + Nij);
                        '					}

                    End If
                Loop While c.nextChild()
            Loop While c.nextParents()

            Return score
        End Function

        Public Overridable Function evaluate_2(observations As Observations, transition As Integer, parentNodesPast As IList(Of Integer), parentNodePresent As IList(Of Integer), childNode As Integer) As Double Implements ScoringFunction.evaluate_2

            Dim c As LocalConfiguration = New LocalConfiguration(observations.Attributes, observations.MarkovLag, parentNodesPast, parentNodePresent, childNode)

            Dim score As Double = 0

            Do
                c.ConsiderChild = False
                Dim Nij = observations.count(c, transition)
                c.ConsiderChild = True
                Do
                    Dim Nijk = observations.count(c, transition)
                    If CLng(Math.Round(Nijk * 1000.0R, MidpointRounding.AwayFromZero)) / 1000.0R <> 0 AndAlso Nijk <> Nij Then
                        score += Nijk * (Math.Log(Nijk) - Math.Log(Nij))
                    End If
                Loop While c.nextChild()
            Loop While c.nextParents()

            Return score
        End Function

        Public Overridable Function evaluate(observations As Observations, parentNodesPast As IList(Of Integer), childNode As Integer) As Double Implements ScoringFunction.evaluate
            Return evaluate(observations, parentNodesPast, Nothing, childNode)
        End Function

        Public Overridable Function evaluate(observations As Observations, parentNodesPast As IList(Of Integer), parentNodePresent As Integer?, childNode As Integer) As Double Implements ScoringFunction.evaluate
            Return evaluate(observations, -1, parentNodesPast, parentNodePresent, childNode)
        End Function

        Public Overridable Function evaluate_2(observations As Observations, parentNodesPast As IList(Of Integer), parentNodePresent As IList(Of Integer), childNode As Integer) As Double Implements ScoringFunction.evaluate_2
            Return evaluate_2(observations, -1, parentNodesPast, parentNodePresent, childNode)
        End Function

    End Class

End Namespace
