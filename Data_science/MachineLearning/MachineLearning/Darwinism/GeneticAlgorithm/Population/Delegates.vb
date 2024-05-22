#Region "Microsoft.VisualBasic::aa4b2a20b2c6976283a0a5b218159771, Data_science\MachineLearning\MachineLearning\Darwinism\GeneticAlgorithm\Population\Delegates.vb"

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

    '   Total Lines: 25
    '    Code Lines: 9 (36.00%)
    ' Comment Lines: 8 (32.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (32.00%)
    '     File Size: 779 B


    '     Delegate Function
    ' 
    ' 
    '     Class IPopulation
    ' 
    '         Properties: capacitySize
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models

Namespace Darwinism.GAF.Population

    Public Delegate Function PopulationCollectionCreator(Of Chr As {Class, Chromosome(Of Chr)})() As PopulationCollection(Of Chr)

    Public MustInherit Class IPopulation(Of Chr As {Class, Chromosome(Of Chr)})

        Protected chromosomes As PopulationCollection(Of Chr)

        ''' <summary>
        ''' 种群的容量上限大小
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property capacitySize As Integer

        ''' <summary>
        ''' Add chromosome
        ''' </summary>
        ''' <param name="chromosome"></param>
        Public MustOverride Sub Add(chromosome As Chr)

    End Class

End Namespace
