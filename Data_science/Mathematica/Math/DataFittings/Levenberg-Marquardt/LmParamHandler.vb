Namespace LevenbergMarquardt
    ''' <summary>
    ''' Created by duy on 18/3/15.
    ''' </summary>
    Public Interface LmParamHandler
        ''' <summary>
        ''' Adjusts (or modifies) values of the Levenberg-Marquardt parameters
        ''' </summary>
        ''' <param name="lmParams"> Numbers which are values of Levenberg-Marquardt parameters </param>
        Sub adjust(lmParams As Double())
    End Interface

End Namespace
