#Region "Microsoft.VisualBasic::0120ebd48fec51b027fe10d669d27058, Data_science\Mathematica\Math\GeneticProgramming\evolution\Configuration.vb"

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
    '    Code Lines: 26
    ' Comment Lines: 19
    '   Blank Lines: 15
    '     File Size: 2.25 KB


    '     Class Configuration
    ' 
    '         Function: createDefaultConfig, validate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.Symbolic.GeneticProgramming.evolution.measure

Namespace evolution

    ''' <summary>
    ''' Basic configuration for the evolution.
    ''' </summary>
    Public Class Configuration

        ''' <summary>
        ''' An objective function. </summary>
        Public objective As ObjectiveFunction

        ''' <summary>
        ''' Maximal size of the population. </summary>
        Public populationSize As Integer
        ''' <summary>
        ''' Size of selection to next generation. </summary>
        Public selectionSize As Integer
        ''' <summary>
        ''' Size of tournament selection. </summary>
        Public tournamentSize As Integer

        ''' <summary>
        ''' Probability of mutation. </summary>
        Public mutationProbability As Double

        ''' <summary>
        ''' Maximal number of epochs the evolution will last. </summary>
        Public maxEpochs As Integer
        ''' <summary>
        ''' Goal fitness threshold that evolution should reach. </summary>
        Public fitnessThreshold As Double

        ''' <returns> <tt>true</tt> IFF the configuration is valid </returns>
        Public Overridable Function validate() As Boolean
            Return objective IsNot Nothing AndAlso populationSize > 0 AndAlso selectionSize > 0 AndAlso tournamentSize > 0 AndAlso populationSize >= 2 * selectionSize AndAlso selectionSize Mod 2 = 0 AndAlso populationSize >= tournamentSize AndAlso mutationProbability >= 0.0 AndAlso mutationProbability <= 1.0 AndAlso maxEpochs > 0 AndAlso Not Double.IsNaN(fitnessThreshold)
        End Function

        ''' <returns> configuration with default values set </returns>
        Public Shared Function createDefaultConfig() As Configuration
            Dim config As Configuration = New Configuration()

            config.objective = ObjectiveFunction.MSE

            config.populationSize = 500
            config.selectionSize = 200
            config.tournamentSize = 5

            config.mutationProbability = 0.2

            config.maxEpochs = 100
            config.fitnessThreshold = 0.01

            Return config
        End Function

    End Class

End Namespace
