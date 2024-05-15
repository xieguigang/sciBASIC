#Region "Microsoft.VisualBasic::f5c959d801678d18a1c85e6b1f32c95c, Data_science\MachineLearning\MachineLearning\Darwinism\GeneticAlgorithm\Population\Substitution\EliteReplacement.vb"

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

    '   Total Lines: 52
    '    Code Lines: 26
    ' Comment Lines: 17
    '   Blank Lines: 9
    '     File Size: 2.00 KB


    '     Class EliteReplacement
    ' 
    '         Properties: type
    ' 
    '         Function: newPopulation
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Darwinism.GAF.Population.SubstitutionStrategy

    ''' <summary>
    ''' 种群的精英杂交更替策略
    ''' </summary>
    ''' <typeparam name="Chr"></typeparam>
    Public Class EliteReplacement(Of Chr As {Class, Chromosome(Of Chr)})
        Implements IStrategy(Of Chr)

        ''' <summary>
        ''' top percentage to keeps as elite
        ''' </summary>
        ReadOnly top As Double = 0.65

        Public ReadOnly Property type As Strategies Implements IStrategy(Of Chr).type
            Get
                Return Strategies.EliteCrossbreed
            End Get
        End Property

        ''' <summary>
        ''' 只保留10%的个体,然后这些个体杂交补充到种群的大小
        ''' </summary>
        ''' <param name="newPop"></param>
        ''' <param name="GA"></param>
        ''' <returns></returns>
        Public Function newPopulation(newPop As Population(Of Chr), GA As GeneticAlgorithm(Of Chr)) As Population(Of Chr) Implements IStrategy(Of Chr).newPopulation
            Dim x, y As Chr

            ' 通过fitness排序来进行择优
            ' 只选择最好的前10个染色体
            ' 然后剩余的成员通过这些被保留下来的染色体间的交叉来生成
            Call newPop.SortPopulationByFitness(GA.chromosomesComparator)
            Call newPop.Trim(newPop.capacitySize * top)

            ' 对剩下的精英个体进行杂交,补充种群的成员
            Do While newPop.Size < newPop.capacitySize
                x = newPop.Random(randf.seeds)
                y = newPop.Random(randf.seeds)

                For Each newIndividual As Chr In x.Crossover(y)
                    Call newPop.Add(newIndividual)
                Next
            Loop

            Return newPop
        End Function
    End Class
End Namespace
