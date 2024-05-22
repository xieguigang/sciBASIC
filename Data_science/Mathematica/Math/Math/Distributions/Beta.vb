#Region "Microsoft.VisualBasic::a6c5698f9542f8c8dd3b4d395dbabfd5, Data_science\Mathematica\Math\Math\Distributions\Beta.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 50
    '    Code Lines: 16 (32.00%)
    ' Comment Lines: 28 (56.00%)
    '    - Xml Docs: 53.57%
    ' 
    '   Blank Lines: 6 (12.00%)
    '     File Size: 1.69 KB


    '     Module Beta
    ' 
    '         Function: (+2 Overloads) beta
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Math
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
