Namespace Distributions

    ''' <summary>
    ''' ##### Gaussian function
    ''' 
    ''' https://en.wikipedia.org/wiki/Gaussian_function
    ''' </summary>
    Public Module Gaussian

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="x#"></param>
        ''' <param name="a#">is the height of the curve's peak</param>
        ''' <param name="b#">is the position of the center of the peak</param>
        ''' <param name="c#">(the standard deviation, sometimes called the Gaussian RMS width) 
        ''' controls the width of the "bell"</param>
        ''' <returns></returns>
        Public Function Gaussian(x#, a#, b#, c#) As Double
            Dim p# = ((x - b) ^ 2) / (2 * c ^ 2)
            Dim fx# = a * E ^ (-p)
            Return fx
        End Function


    End Module
End Namespace