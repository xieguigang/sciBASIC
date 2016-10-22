Imports System.Math
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Mathematical.BasicR
Imports Microsoft.VisualBasic.Mathematical.Distributions.DirichletDistribution

Namespace Distributions

    ''' <summary>
    ''' Beta distribution
    ''' </summary>
    Public Module Beta

        ''' <summary>
        ''' Beta PDF
        ''' </summary>
        ''' <param name="x#"></param>
        ''' <param name="alpha#"></param>
        ''' <param name="_beta#"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' https://github.com/drbenmorgan/CLHEP/blob/master/GenericFunctions/src/BetaDistribution.cc
        ''' 
        ''' 2016-10-22
        ''' beta distribution function test success!
        ''' </remarks>
        Public Function beta(x#, alpha#, _beta#) As Double
            Return Pow(x, alpha - 1) * Pow((1 - x), _beta - 1) *
                Exp(lgamma(alpha + _beta) - lgamma(alpha) - lgamma(_beta))
        End Function

        <Extension>
        Public Function beta(x As IEnumerable(Of Double), alpha#, _beta#) As Vector
            Return New Vector(x.Select(Function(a) beta(a, alpha, _beta)))
        End Function
    End Module
End Namespace