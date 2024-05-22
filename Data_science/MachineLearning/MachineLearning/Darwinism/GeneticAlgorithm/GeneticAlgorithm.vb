#Region "Microsoft.VisualBasic::85bd8e350572383e6eab137c29ed7efd, Data_science\MachineLearning\MachineLearning\Darwinism\GeneticAlgorithm\GeneticAlgorithm.vb"

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

    '   Total Lines: 216
    '    Code Lines: 101 (46.76%)
    ' Comment Lines: 87 (40.28%)
    '    - Xml Docs: 65.52%
    ' 
    '   Blank Lines: 28 (12.96%)
    '     File Size: 9.21 KB


    '     Class GeneticAlgorithm
    ' 
    '         Properties: Best, ParentChromosomesSurviveCount, popStrategy, population, Worst
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: evolIterate, GetFitness, GetRawFitnessModel
    ' 
    '         Sub: Clear, Evolve, UpdateMutationRate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' *****************************************************************************
' Copyright 2012 Yuriy Lagodiuk
' 
' Licensed under the Apache License, Version 2.0 (the "License");
' you may not use this file except in compliance with the License.
' You may obtain a copy of the License at
' 
'   http://www.apache.org/licenses/LICENSE-2.0
' 
' Unless required by applicable law or agreed to in writing, software
' distributed under the License is distributed on an "AS IS" BASIS,
' WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
' See the License for the specific language governing permissions and
' limitations under the License.
' *****************************************************************************

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Population
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Population.SubstitutionStrategy
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models
Imports Microsoft.VisualBasic.Math
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace Darwinism.GAF

    ''' <summary>
    ''' The GA engine core
    ''' </summary>
    ''' <typeparam name="Chr"></typeparam>
    Public Class GeneticAlgorithm(Of Chr As {Class, Chromosome(Of Chr)}) : Inherits Model

        Const ALL_PARENTAL_CHROMOSOMES As Integer = Integer.MaxValue

        Friend ReadOnly chromosomesComparator As FitnessPool(Of Chr)
        Friend ReadOnly seeds As IRandomSeeds
        Friend ReadOnly populationCreator As PopulationCollectionCreator(Of Chr)

        ''' <summary>
        ''' 因为在迭代的过程中，旧的种群会被新的种群所替代
        ''' 所以在这里不可以加readonly修饰
        ''' </summary>
        Public ReadOnly Property population As Population(Of Chr)

        Public ReadOnly Property popStrategy As IStrategy(Of Chr)

        Public ReadOnly Property Best As Chr
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return population(0)
            End Get
        End Property

        Public ReadOnly Property Worst As Chr
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return population(population.Size - 1)
            End Get
        End Property

        ''' <summary>
        ''' Number of parental chromosomes, which survive (and move to new
        ''' population)
        ''' </summary>
        ''' <returns></returns>
        Public Property ParentChromosomesSurviveCount As Integer = ALL_PARENTAL_CHROMOSOMES

        Shared ReadOnly randfSeeds As New [Default](Of IRandomSeeds)(Function() randf.seeds)
        Shared ReadOnly createList As New [Default](Of PopulationCollectionCreator(Of Chr))(Function() New PopulationList(Of Chr))

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="population"></param>
        ''' <param name="fitnessFunc">
        ''' Calculates the fitness of the mutated chromesome in <paramref name="population"/>
        ''' </param>
        ''' <param name="seeds">The random number generator.</param>
        ''' <param name="cacheSize">
        ''' -1 means no cache
        ''' </param>
        ''' <param name="replacementStrategy">Strategy for new population replace the old population.
        ''' </param>
        ''' <param name="createPopulation">By default is create with <see cref="PopulationList(Of Chr)"/></param>
        ''' <remarks>
        ''' Just put the model that implements the <see cref="Fitness(Of Chr)"/>, the
        ''' <see cref="FitnessPool(Of Chr)"/> will be created automatically in this 
        ''' constructor function.
        ''' </remarks>
        Public Sub New(population As Population(Of Chr), fitnessFunc As Fitness(Of Chr),
                       Optional replacementStrategy As Strategies = Strategies.Naive,
                       Optional seeds As IRandomSeeds = Nothing,
                       Optional cacheSize% = 10000,
                       Optional createPopulation As PopulationCollectionCreator(Of Chr) = Nothing)

            Me.population = population
            Me.seeds = seeds Or randfSeeds
            Me.chromosomesComparator = New FitnessPool(Of Chr)(fitnessFunc, capacity:=cacheSize)
            Me.popStrategy = replacementStrategy.GetStrategy(Of Chr)
            Me.populationCreator = createPopulation Or createList

            If population.parallel Then
                Call "Genetic Algorithm running in parallel mode.".Warning
            End If
        End Sub

        Public Function GetRawFitnessModel() As Fitness(Of Chr)
            If TypeOf chromosomesComparator Is FitnessPool(Of Chr) Then
                Return DirectCast(chromosomesComparator, FitnessPool(Of Chr)).evaluateFitness
            Else
                Return chromosomesComparator
            End If
        End Function

        ''' <summary>
        ''' 完成一次种群的迭代进化
        ''' </summary>
        Public Sub Evolve()
            Dim i% = 0
            Dim parentPopulationSize As Integer = population.Size
            Dim newPopulation As New Population(Of Chr)(populationCreator(), population.Pcompute) With {
                .parallel = population.parallel,
                .capacitySize = population.capacitySize
            }

            Do While (i < parentPopulationSize) AndAlso (i < ParentChromosomesSurviveCount)
                ' 旧的原有的种群
                newPopulation.Add(population(i))
                i += 1
            Loop

            ' 新的突变的种群
            ' 这一步并不是限速的部分
            For Each c As Chr In parentPopulationSize% _
                .Sequence _
                .Select(AddressOf evolIterate) _
                .IteratesALL

                ' 并行化计算每一个突变迭代
                ' 将新的突变个体添加进入种群之中
                Call newPopulation.Add(c)
            Next

            ' 下面的两个步骤是机器学习的关键
            ' 通过排序,将错误率最小的种群排在前面
            ' 错误率最大的种群排在后面
            ' 然后对种群进行裁剪,将错误率比较大的种群删除
            ' 从而实现了择优进化, 即程序模型对我们的训练数据集产生了学习

            ' 新种群替代旧的种群
            _population = popStrategy.newPopulation(newPopulation, Me)
        End Sub

        ''' <summary>
        ''' 并行化过程之中的单个迭代
        ''' </summary>
        ''' <param name="i">种群之中的个体的序号,也就是即将发生的目标个体</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 进化发生的契机是个体的突变,这体现在
        '''
        ''' 1. 个体的基因组的变异,可能产生错误率更低的新个体
        ''' 2. 突变体和其他个体随机杂交,可能会产生错误率更低的新个体
        '''
        ''' 在这个函数中,需要完成的就是这两种突变的发生
        ''' </remarks>
        Private Iterator Function evolIterate(i%) As IEnumerable(Of Chr)
            Dim chromosome As Chr = population(i)
            Dim mutated As Chr = chromosome.Mutate()
            Dim rnd As Random = seeds()
            Dim otherChromosome As Chr = population.Random(rnd)
            Dim crossovered As IEnumerable(Of Chr) = mutated.Crossover(otherChromosome)

            otherChromosome = population.Random(rnd)
            crossovered = crossovered.Join(chromosome.Crossover(otherChromosome))

            Yield mutated

            For Each c As Chr In crossovered
                Yield c
            Next
        End Function

        ''' <summary>
        ''' 调用这个函数的代码应该是非并行的
        ''' </summary>
        ''' <param name="chromosome"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetFitness(chromosome As Chr) As Double
            Return chromosomesComparator.Fitness(chromosome, parallel:=True)
        End Function

        ''' <summary>
        ''' 更新种群中的每一个个体的突变变异程度
        ''' </summary>
        ''' <param name="newRate"></param>
        Public Sub UpdateMutationRate(newRate As Double)
            For i As Integer = 0 To population.Size - 1
                population(i).MutationRate = newRate
            Next
        End Sub

        ''' <summary>
        ''' Clear the internal cache
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Clear()
            If TypeOf chromosomesComparator Is FitnessPool(Of Chr) Then
                Call DirectCast(chromosomesComparator, FitnessPool(Of Chr)).Clear()
            End If
        End Sub
    End Class
End Namespace
