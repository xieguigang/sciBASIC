#Region "Microsoft.VisualBasic::c61f1e9597ae03a1b960298ee1b1c96c, Data_science\Mathematica\Math\GeneticProgramming\evolution\GPConfiguration.vb"

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
    '    Code Lines: 26
    ' Comment Lines: 10
    '   Blank Lines: 14
    '     File Size: 1.65 KB


    '     Class GPConfiguration
    ' 
    '         Function: createDefaultConfig, validate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.evolution.measure

Namespace evolution

    ''' <summary>
    ''' Configuration for the Genetic Programming evolution.
    ''' </summary>
    Public Class GPConfiguration
        Inherits Configuration

        ''' <summary>
        ''' Maximal depth of initially generated trees. </summary>
        Public initTreeDepth As Integer

        ''' <summary>
        ''' Type of crossover operator. </summary>
        Public crossoverType As GPTreeUtils.TreeCrossoverType
        ''' <summary>
        ''' Type of mutation operator. </summary>
        Public mutationType As GPTreeUtils.TreeMutationType

        Public Overrides Function validate() As Boolean
            Return MyBase.validate() AndAlso initTreeDepth >= 0
        End Function

        ''' <returns> configuration with default values set </returns>
        Public Overloads Shared Function createDefaultConfig() As GPConfiguration
            Dim config As GPConfiguration = New GPConfiguration()

            config.objective = ObjectiveFunction.MSE

            config.initTreeDepth = 5

            config.populationSize = 500
            config.selectionSize = 200
            config.tournamentSize = 5

            config.crossoverType = GPTreeUtils.TreeCrossoverType.SUBTREE_CROSSOVER
            config.mutationType = GPTreeUtils.TreeMutationType.POINT_MUTATION
            config.mutationProbability = 0.2

            config.maxEpochs = 100
            config.fitnessThreshold = 0.01

            Return config
        End Function

    End Class

End Namespace
