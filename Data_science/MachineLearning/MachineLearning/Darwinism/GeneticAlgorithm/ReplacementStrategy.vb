#Region "Microsoft.VisualBasic::c8d2b0a83be42a4551173398efa657dd, sciBASIC#\Data_science\MachineLearning\MachineLearning\Darwinism\GeneticAlgorithm\ReplacementStrategy.vb"

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

    '   Total Lines: 105
    '    Code Lines: 59
    ' Comment Lines: 29
    '   Blank Lines: 17
    '     File Size: 4.17 KB


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
    '     Structure SimpleReplacement
    ' 
    '         Properties: type
    ' 
    '         Function: newPopulation
    ' 
    '     Class EliteReplacement
    ' 
    '         Properties: type
    ' 
    '         Function: newPopulation
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models
Imports stdNum = System.Math

Namespace Darwinism.GAF.ReplacementStrategy

    Public Interface IStrategy(Of Chr As {Class, Chromosome(Of Chr)})
        ReadOnly Property type As Strategies
        Function newPopulation(newPop As Population(Of Chr), GA As GeneticAlgorithm(Of Chr)) As Population(Of Chr)
    End Interface

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

    ''' <summary>
    ''' 最简单的种群更替策略
    ''' </summary>
    ''' <typeparam name="Chr"></typeparam>
    Public Structure SimpleReplacement(Of Chr As {Class, Chromosome(Of Chr)})
        Implements IStrategy(Of Chr)

        Public ReadOnly Property type As Strategies Implements IStrategy(Of Chr).type
            Get
                Return Strategies.Naive
            End Get
        End Property

        ''' <summary>
        ''' 下面的两个步骤是机器学习的关键
        ''' 
        ''' 通过排序,将错误率最小的种群排在前面
        ''' 错误率最大的种群排在后面
        ''' 然后对种群进行裁剪,将错误率比较大的种群删除
        ''' 从而实现了择优进化, 即程序模型对我们的训练数据集产生了学习
        ''' </summary>
        ''' <param name="newPop"></param>
        ''' <param name="GA"></param>
        ''' <returns></returns>
        Public Function newPopulation(newPop As Population(Of Chr), GA As GeneticAlgorithm(Of Chr)) As Population(Of Chr) Implements IStrategy(Of Chr).newPopulation
            Call newPop.SortPopulationByFitness(GA.chromosomesComparator) ' 通过fitness排序来进行择优
            Call newPop.Trim(newPop.capacitySize)                         ' 剪裁掉后面的对象，达到淘汰的效果

            Return newPop
        End Function
    End Structure

    ''' <summary>
    ''' 种群的精英杂交更替策略
    ''' </summary>
    ''' <typeparam name="Chr"></typeparam>
    Public Class EliteReplacement(Of Chr As {Class, Chromosome(Of Chr)})
        Implements IStrategy(Of Chr)

        ReadOnly ranf As Random = Math.seeds

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
            Call newPop.Trim(newPop.capacitySize * 0.1)

            ' 对剩下的精英个体进行杂交,补充种群的成员
            Do While newPop.Size < newPop.capacitySize
                x = newPop.Random(ranf)
                y = newPop.Random(ranf)

                For Each newIndividual As Chr In x.Crossover(y)
                    Call newPop.Add(newIndividual)
                Next
            Loop

            Return newPop
        End Function
    End Class
End Namespace
