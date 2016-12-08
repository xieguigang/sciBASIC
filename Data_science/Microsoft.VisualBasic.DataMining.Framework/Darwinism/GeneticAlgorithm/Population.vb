#Region "Microsoft.VisualBasic::bc62dc0962752f8ef7220cf7127db598, ..\sciBASIC#\Data_science\Microsoft.VisualBasic.DataMining.Framework\Darwinism\GeneticAlgorithm\Population.vb"

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

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.DataMining.Darwinism.Models
Imports Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Java
Imports Microsoft.VisualBasic.Parallel.Linq

Namespace Darwinism.GAF

    Public Delegate Function ParallelComputing(Of chr As Chromosome(Of chr))(GA As GeneticAlgorithm(Of chr), source As NamedValue(Of chr)()) As IEnumerable(Of NamedValue(Of Double))

    Public Class Population(Of chr As Chromosome(Of chr))
        Implements IEnumerable(Of chr)

        Const DEFAULT_NUMBER_OF_CHROMOSOMES As Integer = 32

        Dim chromosomes As New List(Of chr)(DEFAULT_NUMBER_OF_CHROMOSOMES)

        ''' <summary>
        ''' 是否使用并行模式在排序之前来计算出fitness
        ''' </summary>
        ''' <returns></returns>
        Public Property Parallel As Boolean = False

        ''' <summary>
        ''' Add chromosome
        ''' </summary>
        ''' <param name="chromosome"></param>
        Public Sub Add(chromosome As chr)
            Call chromosomes.Add(chromosome)
        End Sub

        ''' <summary>
        ''' The number of chromosome elements in the inner list
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
        Public ReadOnly Property Random(rnd As Random) As chr
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
        Default Public ReadOnly Property Item(index%) As chr
            Get
                Return chromosomes(index)
            End Get
        End Property

        Public Sub SortPopulationByFitness(comparator As IComparer(Of chr))
            Call Arrays.Shuffle(chromosomes)
            Call chromosomes.Sort(comparator)
        End Sub

        ''' <summary>
        ''' 使用PLinq进行并行计算
        ''' </summary>
        ''' <param name="GA"></param>
        ''' <param name="source"></param>
        ''' <returns></returns>
        Private Shared Function GA_PLinq(GA As GeneticAlgorithm(Of chr), source As NamedValue(Of chr)()) As IEnumerable(Of NamedValue(Of Double))
            Return From x As NamedValue(Of chr)
                   In source.AsParallel
                   Let fit As Double = GA.Fitness.Calculate(x.Value)
                   Select New NamedValue(Of Double) With {
                       .Name = x.Name,
                       .Value = fit
                   }
        End Function

        Friend ReadOnly Pcompute As ParallelComputing(Of chr) = AddressOf GA_PLinq

        Public Sub New(Optional parallel As ParallelComputing(Of chr) = Nothing)
            If Not parallel Is Nothing Then
                Pcompute = parallel
            End If
        End Sub

        ''' <summary>
        ''' 这里是ODEs参数估计的限速步骤
        ''' </summary>
        ''' <param name="GA"></param>
        ''' <param name="comparator"></param>
        Friend Sub SortPopulationByFitness(GA As GeneticAlgorithm(Of chr), comparator As ChromosomesComparator(Of chr))
            Call Arrays.Shuffle(chromosomes)

            If Parallel Then
                Dim source = chromosomes _
                    .Select(Function(x) New NamedValue(Of chr) With {
                        .Name = x.ToString,
                        .Value = x
                    }) _
                    .Where(Function(x) Not comparator.cache.ContainsKey(x.Name)) _
                    .ToArray

                For Each x As NamedValue(Of Double) In Pcompute(GA, source)
                    If Not comparator.cache.ContainsKey(x.Name) Then
                        Call comparator.cache.Add(x.Name, x.Value)
                    End If
                Next
            End If

            Call chromosomes.Sort(comparator)
        End Sub

        ''' <summary>
        ''' shortening population till specific number
        ''' </summary>
        Public Sub Trim(len As Integer)
            chromosomes = chromosomes.sublist(0, len)
        End Sub

        Public Iterator Function GetEnumerator() As IEnumerator(Of chr) Implements IEnumerable(Of chr).GetEnumerator
            For Each x As chr In chromosomes
                Yield x
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
