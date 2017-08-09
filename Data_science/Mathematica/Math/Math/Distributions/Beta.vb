#Region "Microsoft.VisualBasic::51e9a8fd9982ae9f372d9689129ee5ac, ..\sciBASIC#\Data_science\Mathematica\Math\Math\Distributions\Beta.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Math
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.BasicR
Imports Microsoft.VisualBasic.Math.Distributions.DirichletDistribution
Imports Microsoft.VisualBasic.Math.LinearAlgebra

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
