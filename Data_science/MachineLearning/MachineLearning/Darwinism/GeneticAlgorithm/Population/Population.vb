#Region "Microsoft.VisualBasic::d2e87374b398ae56c69d94e139691d96, Data_science\MachineLearning\MachineLearning\Darwinism\GeneticAlgorithm\Population\Population.vb"

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

    '   Total Lines: 192
    '    Code Lines: 86 (44.79%)
    ' Comment Lines: 82 (42.71%)
    '    - Xml Docs: 74.39%
    ' 
    '   Blank Lines: 24 (12.50%)
    '     File Size: 8.59 KB


    '     Class Population
    ' 
    '         Properties: parallel, Random, Size
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: GetEnumerator, GetParallelCompute, IEnumerable_GetEnumerator, ToString
    ' 
    '         Sub: Add, SortPopulationByFitness, Trim
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
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models

Namespace Darwinism.GAF.Population

    ''' <summary>
    ''' The default genetic algorithm population model.
    ''' </summary>
    ''' <typeparam name="Chr">The chromosome type of the genetic algorithm.</typeparam>
    Public Class Population(Of Chr As {Class, Chromosome(Of Chr)}) : Inherits IPopulation(Of Chr)
        Implements IEnumerable(Of Chr)

        ''' <summary>
        ''' 主要是通过这个比较耗时的计算部分实现并行化来
        ''' 加速整个计算过程
        ''' </summary>
        Protected Friend ReadOnly Pcompute As ParallelComputeFitness(Of Chr)

        ''' <summary>
        ''' 是否使用并行模式在排序之前来计算出fitness
        ''' </summary>
        ''' <returns>A <see cref="Boolean"/> value, the default value is ``True``.</returns>
        Public Property parallel As Boolean = True

        ''' <summary>
        ''' The number of chromosome elements in current population.
        ''' (请注意,这个属性的值是随着<see cref="Add"/>方法的调用而变化的,
        ''' 如果只需要获取得到种群的固定大小,可以使用<see cref="capacitySize"/>
        ''' 属性)
        ''' </summary>
        ''' <returns>The current number of the chromosomes in this population.</returns>
        Public ReadOnly Property Size As Integer
            Get
                Return chromosomes.Count
            End Get
        End Property

        ''' <summary>
        ''' Gets random chromosome
        ''' </summary>
        ''' <param name="rnd">The random number generator which is used for selecting the chromosome.</param>
        ''' <returns>A randomly selected chromosome from the current population.</returns>
        Public ReadOnly Property Random(rnd As Random) As Chr
            Get
                Dim numOfChromosomes As Integer = chromosomes.Count
                ' TODO improve random generator
                ' maybe use pattern strategy ?
                Dim indx As Integer = rnd.Next(numOfChromosomes)

                Return chromosomes(indx)
            End Get
        End Property

        ''' <summary>
        ''' Gets chromosome by index
        ''' </summary>
        ''' <param name="index%">The zero based index of the target chromosome.</param>
        ''' <returns>A chromosome object.</returns>
        Default Public ReadOnly Property Item(index As Integer) As Chr
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return chromosomes(index)
            End Get
        End Property

        ''' <summary>
        ''' 如果<paramref name="parallel"/>参数不是空的，则会启用这个参数的并行计算
        ''' </summary>
        ''' <param name="collection">The chromosome collection of this population.</param>
        ''' <param name="parallel">
        ''' The parallel compute mode: a ``Boolean`` value selects the built-in 
        ''' parallel implementation, a <see cref="ParallelComputeFitness(Of Chr)"/> 
        ''' object uses the external implementation, and ``Nothing`` will use the 
        ''' internal GA_PLinq api by default.
        ''' </param>
        Public Sub New(collection As PopulationCollection(Of Chr), Optional parallel As [Variant](Of ParallelComputeFitness(Of Chr), Boolean) = Nothing)
            Me.Pcompute = GetParallelCompute(parallel)
            Me.chromosomes = collection
        End Sub

        Friend Sub New(collection As PopulationCollection(Of Chr), Pcompute As ParallelComputeFitness(Of Chr))
            Me.Pcompute = Pcompute
            Me.chromosomes = collection
        End Sub

        Private Shared Function GetParallelCompute(parallel As [Variant](Of ParallelComputeFitness(Of Chr), Boolean)) As ParallelComputeFitness(Of Chr)
            If parallel Is Nothing Then
                Call "Parallel computing use internal GA_PLinq api by default, as the parallel parameter is not specific...".debug
                Return New ParallelDataSetCompute(Of Chr)
            End If

            If parallel Like GetType(Boolean) Then
                Dim flag As Boolean = parallel

                Call "Parallel computing use internal GA_PLinq api".info

                If flag Then
                    Return New ParallelPopulationCompute(Of Chr)
                Else
                    Return New ParallelDataSetCompute(Of Chr)
                End If
            Else
                Dim Pcompute = parallel.TryCast(Of ParallelComputeFitness(Of Chr))

                Call $"Parallel computing use external api: {Pcompute.ToString}".info

                Return Pcompute
            End If
        End Function

        ''' <summary>
        ''' 这里是ODEs参数估计的限速步骤
        ''' </summary>
        ''' <param name="comparator">The fitness evaluation function of the current environment.</param>
        Friend Sub SortPopulationByFitness(comparator As FitnessPool(Of Chr))
            Dim fitness = Pcompute.ComputeFitness(comparator, chromosomes) _
                .ToArray _
                .GroupBy(Function(fit) fit.Name) _
                .ToDictionary(Function(fit) fit.Key,
                              Function(group)
                                  ' 因为可能会存在一样的染色体
                                  ' 所以在这里必须要分组之后再构建字典
                                  Return group.OrderByDescending(Function(fit) fit.Value) _
                                      .First _
                                      .Value
                              End Function)

            ' fitness smaller is better
            Call chromosomes.OrderBy(Function(key) fitness(key))
        End Sub

        ''' <summary>
        ''' Add chromosome
        ''' </summary>
        ''' <param name="chromosome">The chromosome object that will be added into this population.</param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub Add(chromosome As Chr)
            Call chromosomes.Add(chromosome)
        End Sub

        ''' <summary>
        ''' shortening population till specific number
        ''' </summary>
        ''' <param name="len">The maximum number of the chromosomes which will be kept.</param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Trim(len As Integer)
            Call chromosomes.Trim(capacitySize:=len)
        End Sub

        ''' <summary>
        ''' Display the brief summary information of this population.
        ''' </summary>
        ''' <returns>
        ''' A string in format like ``A population with capacity N, current size M. //type``
        ''' </returns>
        Public Overrides Function ToString() As String
            Return $"A population with capacity {capacitySize}, current size {Size}. //{GetType(Chr).FullName}"
        End Function

        ''' <summary>
        ''' Iterate through all of the chromosome objects in this population.
        ''' </summary>
        ''' <returns>An enumerator of the chromosome objects.</returns>
        Public Iterator Function GetEnumerator() As IEnumerator(Of Chr) Implements IEnumerable(Of Chr).GetEnumerator
            For i As Integer = 0 To chromosomes.Count - 1
                Yield chromosomes(i)
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
