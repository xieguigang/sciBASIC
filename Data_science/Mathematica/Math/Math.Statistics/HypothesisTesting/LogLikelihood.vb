Imports stdNum = System.Math

''' <summary>
''' calculates log likelihood between a source and a reference corpus. for more
''' details on LLH please visit http : //ucrel.lancs.ac.uk/llwizard.html
''' </summary>
Public Module LogLikelihood

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="freq1">frequency of the word in your corpus</param>
    ''' <param name="freq2">frequency of the word in the reference corpus</param>
    ''' <param name="corpus1Size">size of your corpus in words</param>
    ''' <param name="corpus2Size">size of the reference corpus in words</param>
    ''' <returns></returns>
    Public Function Likelihood(freq1#, freq2#, corpus1Size#, corpus2Size#) As Double
        Dim e1p1 As Double = (corpus1Size * (freq1 + freq2))
        Dim e1p2 As Double = (corpus1Size + corpus2Size)
        Dim expectedValue1 As Double = e1p1 / e1p2
        Dim e2p1 As Double = (corpus2Size * (freq1 + freq2))
        Dim e2p2 As Double = (corpus1Size + corpus2Size)
        Dim expectedValue2 = e2p1 / e2p2

        If expectedValue1 = 0 OrElse expectedValue2 = 0 OrElse
            Double.IsNaN(expectedValue1) OrElse
            Double.IsNaN(expectedValue1) OrElse
            Double.IsNaN(stdNum.Log(freq1 / expectedValue1)) OrElse
            Double.IsNaN(stdNum.Log(freq2 / expectedValue2)) OrElse
            Double.IsInfinity(stdNum.Log(freq1 / expectedValue1)) OrElse
            Double.IsInfinity(stdNum.Log(freq2 / expectedValue2)) Then

            Return 0.0
        Else
            Dim llhP1 As Double = (freq1 * stdNum.Log(freq1 / expectedValue1))
            Dim llhP2 As Double = (freq2 * stdNum.Log(freq2 / expectedValue2))

            Return 2 * (llhP1 + llhP2)
        End If
    End Function

End Module
