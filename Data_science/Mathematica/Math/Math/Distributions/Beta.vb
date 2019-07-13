#Region "Microsoft.VisualBasic::10c5bb0af65991ecd86709ef21597672, Data_science\Mathematica\Math\Math\Distributions\Beta.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module Beta
    ' 
    '         Function: (+2 Overloads) beta
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Distributions.DirichletDistribution
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace Distributions

    ''' <summary>
    ''' Beta distribution
    ''' </summary>
    Public Module Beta

        ' x ^ (a - 1) * (1 - x) ^ (b - 1) * E ^ C

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

        '''' <summary>
        '''' ###### beta function
        '''' 
        '''' https://en.wikipedia.org/wiki/Beta_function
        '''' </summary>
        '''' <param name="x#"></param>
        '''' <param name="y#"></param>
        '''' <returns></returns>
        'Public Function beta(x#, y#) As Double
        '    Return gamma(x) * gamma(y) / gamma(x + y)
        'End Function

        <Extension>
        Public Function beta(x As IEnumerable(Of Double), alpha#, _beta#) As Vector
            Return New Vector(x.Select(Function(a) beta(a, alpha, _beta)))
        End Function
    End Module
End Namespace
