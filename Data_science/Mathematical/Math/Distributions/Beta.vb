Imports System.Math
Imports Microsoft.VisualBasic.Mathematical.Distributions.DirichletDistribution

Namespace Distributions

    ''' <summary>
    ''' Beta distribution
    ''' </summary>
    Public Module Beta

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="x#"></param>
        ''' <param name="alpha#"></param>
        ''' <param name="_beta#"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' https://github.com/drbenmorgan/CLHEP/blob/master/GenericFunctions/src/BetaDistribution.cc
        ''' not sure
        ''' </remarks>
        Public Function beta(x#, alpha#, _beta#) As Double
            Return Pow(x, alpha - 1) * Pow((1 - x), _beta - 1) *
                Exp(lgamma(alpha + _beta) - lgamma(alpha) - lgamma(_beta))
        End Function
    End Module
End Namespace