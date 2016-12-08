#Region "Microsoft.VisualBasic::81e5fb70cc7d0f99edd20cef8970b35f, ..\sciBASIC#\Data_science\Microsoft.VisualBasic.DataMining.Framework\Darwinism\GeneticAlgorithm\GeneticAlgorithm.vb"

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

Imports Microsoft.VisualBasic.DataMining.Darwinism.Models
Imports Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical

Namespace Darwinism.GAF

    Public Class GeneticAlgorithm(Of C As Chromosome(Of C))

        Const ALL_PARENTAL_CHROMOSOMES As Integer = Integer.MaxValue

        ReadOnly _chromosomesComparator As ChromosomesComparator(Of C)
        Friend ReadOnly _fitnessFunc As Fitness(Of C)

        ''' <summary>
        ''' listeners of genetic algorithm iterations (handle callback afterwards)
        ''' </summary>
        ReadOnly iterationListeners As New List(Of IterartionListener(Of C))
        ReadOnly seeds As IRandomSeeds

        Dim _terminate As Boolean = False

        Public Sub New(population As Population(Of C), fitnessFunc As Fitness(Of C), Optional seeds As IRandomSeeds = Nothing)
            Me._Population = population
            Me._fitnessFunc = fitnessFunc
            Me._chromosomesComparator = New ChromosomesComparator(Of C)(Me)
            Me._Population.SortPopulationByFitness(Me, _chromosomesComparator)

            If population.Parallel Then
                Call "Genetic Algorithm running in parallel mode.".Warning
            End If
            If seeds Is Nothing Then
                seeds = Function() New Random(Now.Millisecond)
            End If

            Me.seeds = seeds
        End Sub

        Public Sub Evolve()
            Dim parentPopulationSize As Integer = Population.Size
            Dim newPopulation As New Population(Of C)() With {
                .Parallel = Population.Parallel
            }
            Dim i As Integer = 0

            Do While (i < parentPopulationSize) AndAlso (i < ParentChromosomesSurviveCount)
                newPopulation.Add(Population(i)) ' 旧的原有的种群
                i += 1
            Loop

            ' 新的突变的种群
            ' 这一步并不是限速的部分
            For Each c As C In parentPopulationSize% _
                .Sequence _
                .Select(AddressOf __iterate) _
                .IteratesALL ' 并行化计算每一个突变迭代

                Call newPopulation.Add(c)
            Next

            newPopulation.SortPopulationByFitness(Me, _chromosomesComparator)    ' 通过fitness排序来进行择优
            newPopulation.Trim(parentPopulationSize)                             ' 剪裁掉后面的对象，达到淘汰的效果
            _Population = newPopulation                                          ' 新种群替代旧的种群
        End Sub

        ''' <summary>
        ''' 并行化过程之中的单个迭代
        ''' </summary>
        ''' <param name="i%"></param>
        ''' <returns></returns>
        Private Iterator Function __iterate(i%) As IEnumerable(Of C)
            Dim chromosome As C = Population(i)
            Dim mutated As C = chromosome.Mutate()   ' 突变
            Dim rnd As Random = seeds()
            Dim otherChromosome As C = Population.Random(rnd)  ' 突变体和其他个体随机杂交
            Dim crossovered As IList(Of C) = mutated.Crossover(otherChromosome) ' chromosome.Crossover(otherChromosome)

            ' --------- 新修改的
            otherChromosome = Population.Random(rnd)
            crossovered = crossovered.Join(chromosome.Crossover(otherChromosome))
            ' ---------

            Yield mutated

            For Each c As C In crossovered
                Yield c
            Next
        End Function

        Public Sub Evolve(count As Integer)
            _terminate = False

            For i As Integer = 0 To count - 1
                If _terminate Then
                    Exit For
                End If

                Call Evolve()
                _Iteration = i

                For Each l As IterartionListener(Of C) In iterationListeners
                    Call l.Update(Me)
                Next
            Next
        End Sub

        Public ReadOnly Property Iteration As Integer

        Public Sub Terminate()
            Me._terminate = True
        End Sub

        Public ReadOnly Property Population As Population(Of C)

        Public ReadOnly Property Best As C
            Get
                Return Population(0)
            End Get
        End Property

        Public ReadOnly Property Worst As C
            Get
                Return Population(Population.Size - 1)
            End Get
        End Property

        ''' <summary>
        ''' Number of parental chromosomes, which survive (and move to new
        ''' population)
        ''' </summary>
        ''' <returns></returns>
        Public Property ParentChromosomesSurviveCount As Integer = ALL_PARENTAL_CHROMOSOMES

        Public Sub addIterationListener(listener As IterartionListener(Of C))
            Me.iterationListeners.Add(listener)
        End Sub

        Public Sub removeIterationListener(listener As IterartionListener(Of C))
            iterationListeners.Remove(listener)
        End Sub

        Public Function Fitness(chromosome As C) As Double
            Return _chromosomesComparator.Fitness(chromosome)
        End Function

        ''' <summary>
        ''' Clear the internal cache
        ''' </summary>
        Public Sub Clear()
            _chromosomesComparator.clearCache()
        End Sub
    End Class
End Namespace
