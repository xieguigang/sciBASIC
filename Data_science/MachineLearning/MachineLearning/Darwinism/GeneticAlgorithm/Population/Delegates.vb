#Region "Microsoft.VisualBasic::b20c07212994dac592af052b8a324152, sciBASIC#\Data_science\MachineLearning\MachineLearning\Darwinism\GeneticAlgorithm\Population\Delegates.vb"

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

    '   Total Lines: 15
    '    Code Lines: 6
    ' Comment Lines: 6
    '   Blank Lines: 3
    '     File Size: 731 B


    '     Delegate Function
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models

Namespace Darwinism.GAF

    ''' <summary>
    ''' 遗传算法的主要限速步骤是在fitness的计算之上
    ''' </summary>
    ''' <typeparam name="chr"></typeparam>
    ''' <param name="source"></param>
    ''' <returns></returns>
    Public Delegate Function ParallelComputeFitness(Of chr As {Class, Chromosome(Of chr)})(comparator As FitnessPool(Of chr), source As PopulationCollection(Of chr)) As IEnumerable(Of NamedValue(Of Double))
    Public Delegate Function PopulationCollectionCreator(Of Chr As {Class, Chromosome(Of Chr)})() As PopulationCollection(Of Chr)

End Namespace
