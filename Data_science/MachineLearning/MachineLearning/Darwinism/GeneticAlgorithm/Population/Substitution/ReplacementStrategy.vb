#Region "Microsoft.VisualBasic::8422160e74aebb74095bee4b4015c1f5, Data_science\MachineLearning\MachineLearning\Darwinism\GeneticAlgorithm\Population\Substitution\ReplacementStrategy.vb"

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

    '   Total Lines: 31
    '    Code Lines: 23
    ' Comment Lines: 3
    '   Blank Lines: 5
    '     File Size: 1.11 KB


    '     Interface IStrategy
    ' 
    '         Properties: type
    ' 
    '         Function: newPopulation
    ' 
    '     Enum Strategies
    ' 
    '         EliteCrossbreed, Naive
    ' 
    '  
    ' 
    ' 
    ' 
    '     Module Extensions
    ' 
    '         Function: GetStrategy
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models

Namespace Darwinism.GAF.Population.SubstitutionStrategy

    Public Interface IStrategy(Of Chr As {Class, Chromosome(Of Chr)})
        ReadOnly Property type As Strategies
        Function newPopulation(newPop As Population(Of Chr), GA As GeneticAlgorithm(Of Chr)) As Population(Of Chr)
    End Interface

    ''' <summary>
    ''' enums of the population substitution strategies
    ''' </summary>
    Public Enum Strategies
        Naive
        EliteCrossbreed
    End Enum

    <HideModuleName> Public Module Extensions

        <Extension>
        Public Function GetStrategy(Of genome As {Class, Chromosome(Of genome)})(strategy As Strategies) As IStrategy(Of genome)
            Select Case strategy
                Case Strategies.EliteCrossbreed
                    Return New EliteReplacement(Of genome)
                Case Else
                    Return New SimpleReplacement(Of genome)
            End Select
        End Function
    End Module
End Namespace
