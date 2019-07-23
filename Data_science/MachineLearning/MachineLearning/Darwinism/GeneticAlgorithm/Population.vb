#Region "Microsoft.VisualBasic::3fb93b58e337be0490745193348e1106, Data_science\MachineLearning\MachineLearning\Darwinism\GeneticAlgorithm\Population.vb"

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

'     Delegate Function
' 
' 
'     Class Population
' 
'         Properties: parallel, Random, Size
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: GA_PLinq, GetEnumerator, IEnumerable_GetEnumerator
' 
'         Sub: Add, parallelCacheFitness, (+2 Overloads) SortPopulationByFitness, Trim
' 
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
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models
Imports Microsoft.VisualBasic.Parallel.Linq

Namespace Darwinism.GAF

    Public Delegate Function ParallelComputing(Of chr As {Class, Chromosome(Of chr)})(GA As GeneticAlgorithm(Of chr), source As NamedValue(Of chr)()) As IEnumerable(Of NamedValue(Of Double))

    Public Class Population(Of Chr As {Class, Chromosome(Of Chr)})
        Implements IEnumerable(Of Chr)

        Const DEFAULT_NUMBER_OF_CHROMOSOMES As Integer = 32

        Dim chromosomes As New List(Of Chr)(DEFAULT_NUMBER_OF_CHROMOSOMES)

        ''' <summary>
        ''' 是否使用并行模式在排序之前来计算出fitness
        ''' </summary>
        ''' <returns></returns>
        Public Property parallel As Boolean = True
        ''' <summary>
        ''' 种群的大小
        ''' </summary>
        ''' <returns></returns>
        Public Property initialSize As Integer

        ''' <summary>
        ''' The number of chromosome elements in current population.
        ''' (请注意,这个属性的值是随着<see cref="Add"/>方法的调用而变化的,
        ''' 如果只需要获取得到种群的固定大小,可以使用<see cref="initialSize"/>
        ''' 属性)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Size As Integer
            Get
                Return chromosomes.Count
            End Get
        End Property

        ''' <summary>
        ''' Gets random chromosome
        ''' </summary>
        ''' <returns></returns>
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
        ''' <param name="index%"></param>
        ''' <returns></returns>
        Default Public ReadOnly Property Item(index As Integer) As Chr
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return chromosomes(index)
            End Get
        End Property

        ''' <summary>
        ''' Add chromosome
        ''' </summary>
        ''' <param name="chromosome"></param>
        Public Sub Add(chromosome As Chr)
            Call chromosomes.Add(chromosome)
        End Sub

        Public Sub SortPopulationByFitness(comparator As IComparer(Of Chr))
            ' Call Arrays.Shuffle(chromosomes)
            Call chromosomes.Sort(comparator)
        End Sub

        ''' <summary>
        ''' 使用PLinq进行并行计算
        ''' </summary>
        ''' <param name="GA"></param>
        ''' <param name="source"></param>
        ''' <returns></returns>
        Private Shared Function GA_PLinq(GA As GeneticAlgorithm(Of Chr), source As NamedValue(Of Chr)(), parallelFlag As Boolean) As IEnumerable(Of NamedValue(Of Double))
            Return From x As NamedValue(Of Chr)
                   In source.Populate(parallel:=parallelFlag)
                   Let fit As Double = GA.chromosomesComparator.Calculate(x.Value, parallel:=Not parallelFlag)
                   Select New NamedValue(Of Double) With {
                       .Name = x.Name,
                       .Value = fit
                   }
        End Function

        Friend ReadOnly Pcompute As ParallelComputing(Of Chr)

        ''' <summary>
        ''' 如果<paramref name="parallel"/>参数不是空的，则会启用这个参数的并行计算
        ''' </summary>
        ''' <param name="parallel"></param>
        Public Sub New(Optional parallel As [Variant](Of ParallelComputing(Of Chr), Boolean) = Nothing)
            If Not parallel Is Nothing Then
                If parallel Like GetType(Boolean) Then
                    Dim flag As Boolean = parallel

                    Pcompute = Function(ga, source)
                                   Return GA_PLinq(ga, source, parallelFlag:=flag)
                               End Function
                Else
                    Pcompute = parallel
                End If
            Else
                Pcompute = Function(ga, source)
                               Return GA_PLinq(ga, source, parallelFlag:=True)
                           End Function
            End If
        End Sub

        ''' <summary>
        ''' 这里是ODEs参数估计的限速步骤
        ''' </summary>
        ''' <param name="GA"></param>
        ''' <param name="comparator"></param>
        Friend Sub SortPopulationByFitness(GA As GeneticAlgorithm(Of Chr), comparator As FitnessPool(Of Chr))
            If parallel AndAlso comparator.Cacheable Then
                Call parallelCacheFitness(GA, comparator)
            End If

            'chromosomes = LQuerySchedule _
            '    .LQuery(inputs:=chromosomes,
            '            task:=Function(chr)
            '                      Return (Fitness:=comparator.Fitness(chr, parallel:=False), chr:=chr)
            '                  End Function,
            '            partitionSize:=chromosomes.Count / App.CPUCoreNumbers
            '    ) _
            '    .OrderBy(Function(c) c.Fitness) _
            '    .Select(Function(c) c.chr) _
            '    .AsList
            chromosomes = chromosomes _
                .OrderBy(Function(c) comparator.Fitness(c, parallel:=True)) _
                .AsList
        End Sub

        Private Sub parallelCacheFitness(GA As GeneticAlgorithm(Of Chr), comparator As FitnessPool(Of Chr))
            Dim source As NamedValue(Of Chr)() = chromosomes _
                .Select(Function(x)
                            Return New NamedValue(Of Chr) With {
                                .Name = x.ToString,
                                .Value = x
                            }
                        End Function) _
                .Where(Function(x)
                           Return Not comparator.cache.ContainsKey(x.Name)
                       End Function) _
                .ToArray
            Dim fitness As NamedValue(Of Double)() = Pcompute(GA, source).ToArray

            For Each x As NamedValue(Of Double) In fitness
                If Not comparator.cache.ContainsKey(x.Name) Then
                    Call comparator.cache.Add(x.Name, x.Value)
                End If
            Next
        End Sub

        ''' <summary>
        ''' shortening population till specific number
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Trim(len As Integer)
            chromosomes = chromosomes.SubList(0, len)
        End Sub

        Public Overrides Function ToString() As String
            Return $"A population with capacity {initialSize}, current size {Size}. //{GetType(Chr).FullName}"
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Chr) Implements IEnumerable(Of Chr).GetEnumerator
            For Each x As Chr In chromosomes
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
