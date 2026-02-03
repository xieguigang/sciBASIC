Namespace dbn

    Public Interface ScoringFunction

        Function evaluate(observations As Observations, transition As Integer, parentNodesPast As IList(Of Integer), childNode As Integer) As Double

        Function evaluate(observations As Observations, transition As Integer, parentNodesPast As IList(Of Integer), parentNodePresent As Integer?, childNode As Integer) As Double


        Function evaluate_2(observations As Observations, transition As Integer, parentNodesPast As IList(Of Integer), parentNodePresent As IList(Of Integer), childNode As Integer) As Double




        ''' <summary>
        ''' Calculate score when process is stationary.
        ''' </summary>
        Function evaluate(observations As Observations, parentNodesPast As IList(Of Integer), parentNodePresent As Integer?, childNode As Integer) As Double


        ''' <summary>
        ''' Calculate score when process is stationary.
        ''' </summary>
        Function evaluate_2(observations As Observations, parentNodesPast As IList(Of Integer), parentNodePresent As IList(Of Integer), childNode As Integer) As Double




        ''' <summary>
        ''' Calculate score when process is stationary.
        ''' </summary>
        Function evaluate(observations As Observations, parentNodesPast As IList(Of Integer), childNode As Integer) As Double

    End Interface

End Namespace
