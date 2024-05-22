#Region "Microsoft.VisualBasic::df5b877d873a62b63764377641e3f489, Data_science\Mathematica\Math\GeneticProgramming\evolution\GAConfiguration.vb"

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

    '   Total Lines: 60
    '    Code Lines: 30 (50.00%)
    ' Comment Lines: 14 (23.33%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 16 (26.67%)
    '     File Size: 2.08 KB


    '     Class GAConfiguration
    ' 
    '         Function: createDefaultConfig, validate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.evolution.measure

Namespace evolution

    ''' <summary>
    ''' Configuration for the Genetic Algorithm evolution.
    ''' </summary>
    Public Class GAConfiguration
        Inherits Configuration

        ''' <summary>
        ''' Initial order of generate polynomial. </summary>
        Public initPolyOrder As Integer

        ''' <summary>
        ''' Beginning of the range for parameters of polynomial. </summary>
        Public paramRangeFrom As Double
        ''' <summary>
        ''' End of the range for parameters of polynomial. </summary>
        Public paramRangeTo As Double

        ''' <summary>
        ''' Type of crossover operator. </summary>
        Public crossoverType As GAPolynomialUtils.PolyCrossoverType
        ''' <summary>
        ''' Type of mutation operator. </summary>
        Public mutationType As GAPolynomialUtils.PolyMutationType

        Public Overrides Function validate() As Boolean
            Return MyBase.validate() AndAlso initPolyOrder >= 0 AndAlso paramRangeFrom < paramRangeTo
        End Function

        ''' <returns> configuration with default values set </returns>
        Public Overloads Shared Function createDefaultConfig() As GAConfiguration
            Dim config As GAConfiguration = New GAConfiguration()

            config.objective = ObjectiveFunction.MSE

            config.initPolyOrder = 5

            config.paramRangeFrom = -1.0
            config.paramRangeTo = +1.0

            config.populationSize = 500
            config.selectionSize = 200
            config.tournamentSize = 5

            config.crossoverType = GAPolynomialUtils.PolyCrossoverType.SIMULATED_BINARY_CROSSOVER
            config.mutationType = GAPolynomialUtils.PolyMutationType.GAUSSIAN_POINT_MUTATION
            config.mutationProbability = 0.2

            config.maxEpochs = 100
            config.fitnessThreshold = 0.01

            Return config
        End Function

    End Class

End Namespace
