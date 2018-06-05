Public Class htest

    Public Property statistic As Dictionary(Of String, Double)
    Public Property parameters As Dictionary(Of String, Double)
    Public Property pvalue As Double
    Public Property n As Integer
    Public Property b As Double

End Class

''' <summary>
''' mHG definition:
''' 
''' ```
'''   mHG(lambdas) = min over 1 &lt;= n &lt;= N Of HGT (b_n(lambdas); N, B, n)
''' ```
'''   
''' Where ``HGT`` Is the hypergeometric tail:
''' 
''' ```
'''   HGT(b; N, B, n) = Probability(X >= b)
''' ```
'''   
''' And:
''' 
''' ```
'''   b_n = sum over 1 &lt;= i &lt;= n Of lambdas[i]
''' ```
''' </summary>
Public Class mHGstatisticInfo

    ''' <summary>
    ''' the statistic itself
    ''' </summary>
    Public Property mHG As Double
    ''' <summary>
    ''' the index For which it was obtained
    ''' </summary>
    Public Property n As Double
    ''' <summary>
    ''' (Short For b_n) - sum over ``1 &lt;= i &lt;= n`` Of lambdas[i]
    ''' </summary>
    Public Property b As Double

End Class

