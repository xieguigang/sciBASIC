Public Class WeightedFit

    ''' <summary>
    ''' FReg: Fisher F statistic for regression
    ''' </summary>
    ''' <returns></returns>
    Public Property FisherF() As Double

    ''' <summary>
    ''' RYSQ: Multiple correlation coefficient (R2，相关系数)
    ''' </summary>
    ''' <returns></returns>
    Public Property CorrelationCoefficient() As Double

    ''' <summary>
    ''' SDV: Standard deviation of errors
    ''' </summary>
    ''' <returns></returns>
    Public Property StandardDeviation() As Double

    ''' <summary>
    ''' Ycalc: Calculated values of Y.(根据所拟合的公式所计算出来的预测值)
    ''' </summary>
    ''' <returns></returns>
    Public Property CalculatedValues() As Double()

    ''' <summary>
    ''' DY: Residual values of Y
    ''' </summary>
    ''' <returns></returns>
    Public Property Residuals() As Double()

    ''' <summary>
    ''' C: Coefficients.(拟合出来的多项式系数)
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
