Public Class WeightedFit

    ''' <summary>
    ''' FReg: Fisher F statistic for regression
    ''' </summary>
    ''' <returns></returns>
    Public Property FisherF() As Double

    ''' <summary>
    ''' RYSQ: Multiple correlation coefficient
    ''' </summary>
    ''' <returns></returns>
    Public Property CorrelationCoefficient() As Double

    ''' <summary>
    ''' SDV: Standard deviation of errors
    ''' </summary>
    ''' <returns></returns>
    Public Property StandardDeviation() As Double

    ''' <summary>
    ''' Ycalc: Calculated values of Y
    ''' </summary>
    ''' <returns></returns>
    Public Property CalculatedValues() As Double()

    ''' <summary>
    ''' DY: Residual values of Y
    ''' </summary>
    ''' <returns></returns>
    Public Property Residuals() As Double()

    ''' <summary>
    ''' C: Coefficients
    ''' </summary>
    ''' <returns></returns>
    Public Property Coefficients() As Double()

    ''' <summary>
    ''' SEC: Std Error of coefficients
    ''' </summary>
    ''' <returns></returns>
    Public Property CoefficientsStandardError() As Double()

    ''' <summary>
    ''' V: Least squares and var/covar matrix
    ''' </summary>
    ''' <returns></returns>
    Public Property VarianceMatrix() As Double(,)

End Class
