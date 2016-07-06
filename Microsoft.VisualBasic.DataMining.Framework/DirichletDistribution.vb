''' <summary>
''' Dirichlet distribution
''' 
''' In probability and statistics, the Dirichlet distribution (after Peter Gustav Lejeune Dirichlet), often denoted 
''' {\displaystyle \operatorname {Dir} ({\boldsymbol {\alpha }})} \operatorname {Dir} ({\boldsymbol {\alpha }}), is 
''' a family of continuous multivariate probability distributions parameterized by a vector 
''' {\displaystyle {\boldsymbol {\alpha }}} {\boldsymbol {\alpha }} of positive reals. 
''' It is a multivariate generalization of the beta distribution.[1] Dirichlet distributions are very often used as 
''' prior distributions in Bayesian statistics, and in fact the Dirichlet distribution is the conjugate prior of the 
''' categorical distribution and multinomial distribution.
''' </summary>
Public Class DirichletDistribution

    Public a1 As Single = 1.0F
    Public a2 As Single = 1.0F
    Public a3 As Single = 1.0F

    Public Function Probability(x1 As Single, x2 As Single, x3 As Single) As Single
        Dim logCoef As Single = lgamma(a1 + a2 + a3) - lgamma(a1) - lgamma(a2) - lgamma(a3)
        Dim logValue As Single = (a1 - 1.0F) * Math.Log(x1) + (a2 - 1.0F) * Math.Log(x2) + (a3 - 1.0F) * Math.Log(x3)

        Return Math.Exp(logCoef + logValue)
    End Function

    ''' <summary>
    ''' see http://www.machinedlearnings.com/2011/06/faster-lda.html
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    Public Shared Function lgamma(x As Single) As Single
        Dim logterm As Single = Math.Log(x * (1.0F + x) * (2.0F + x))
        Dim xp3 As Single = 3.0F + x

        Return -2.081061F - x + 0.0833333F / xp3 - logterm + (2.5F + x) * Math.Log(xp3)
    End Function
End Class