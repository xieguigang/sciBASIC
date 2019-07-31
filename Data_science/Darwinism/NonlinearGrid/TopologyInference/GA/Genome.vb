#Region "Microsoft.VisualBasic::f7b0a9556deb09cec79c25c03762dc25, Data_science\Darwinism\NonlinearGrid\TopologyInference\GA\Genome.vb"

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

    ' Class Genome
    ' 
    '     Properties: MutationRate
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: CalculateError, Crossover, Evaluate, Mutate, (+2 Overloads) ToString
    '               valueMutate
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

''' <summary>
''' 
''' </summary>
''' <remarks>
''' 在系统之中的各个部件之间的突变以及杂交事件应该都是相互独立的
''' </remarks>
Public Class Genome : Implements Chromosome(Of Genome)

    Friend ReadOnly chromosome As GridSystem

    ''' <summary>
    ''' Number of system variables.
    ''' </summary>
    ReadOnly width As Integer
    ''' <summary>
    ''' 约束变异所产生的值的上限
    ''' </summary>
    ReadOnly truncate As Double
    ReadOnly rangePositive As Boolean

    ''' <summary>
    ''' 突变程度
    ''' </summary>
    Public Property MutationRate As Double Implements Chromosome(Of Genome).MutationRate

    Const CrossOverRate As Double = 30

    Sub New(chr As GridSystem, mutationRate As Double, truncate As Double, rangePositive As Boolean)
        Me.chromosome = chr
        Me.width = chr.A.Dim
        Me.MutationRate = mutationRate
        Me.truncate = truncate
        Me.rangePositive = rangePositive
    End Sub

    ''' <summary>
    ''' <see cref="GridSystem.Evaluate(Vector)"/>
    ''' </summary>
    ''' <param name="X"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Evaluate(X As Vector) As Double
        Return chromosome.Evaluate(X)
    End Function

    Public Function CalculateError(status As Vector, target As Double) As Double
        Dim predicts = chromosome.Evaluate(status)

        If rangePositive AndAlso predicts < 0 Then
            Return target
        ElseIf predicts.IsNaNImaginary Then
            Return Double.MaxValue
        Else
            Return Math.Abs(predicts - target)
        End If
    End Function

    Public Iterator Function Crossover(another As Genome) As IEnumerable(Of Genome) Implements Chromosome(Of Genome).Crossover
        Dim a = Me.chromosome.Clone
        Dim b = another.chromosome.Clone

        SyncLock randf.seeds
            If FlipCoin(CrossOverRate) Then
                ' crossover A
                randf.seeds.Crossover(a.A.Array, b.A.Array)
            End If

            If FlipCoin(CrossOverRate) Then
                ' dim(A) is equals to dim(C) and is equals to dim(X)
                Dim i As Integer = randf.NextInteger(upper:=width)
                Dim j As Integer = randf.NextInteger(upper:=width)

                ' If FlipCoin() Then
                ' crossover C
                randf.seeds.Crossover(a.C(i).B.Array, b.C(j).B.Array)
            End If

            If FlipCoin(CrossOverRate) Then
                Dim tmp#
                ' dim(A) is equals to dim(C) and is equals to dim(X)
                Dim i As Integer = randf.NextInteger(upper:=width)
                Dim j As Integer = randf.NextInteger(upper:=width)

                tmp = a.C(i).BC
                a.C(i).BC = b.C(j).BC
                b.C(j).BC = tmp
            End If

            If FlipCoin(CrossOverRate) Then
                Dim tmp#

                tmp = a.AC
                a.AC = b.AC
                b.AC = tmp
            End If

            'If FlipCoin(CrossOverRate) Then
            '    Dim tmp#

            '    tmp = a.Amplify
            '    a.Amplify = b.Amplify
            '    b.Amplify = tmp
            'End If

            'If FlipCoin(CrossOverRate) Then
            '    Dim tmp#

            '    tmp = a.delay
            '    a.delay = b.delay
            '    b.delay = tmp
            'End If
        End SyncLock

        Yield New Genome(a, MutationRate, truncate, rangePositive)
        Yield New Genome(b, MutationRate, truncate, rangePositive)
    End Function

    Private Function valueMutate(x As Double) As Double
        If x = 0R Then
            Return 1
        ElseIf FlipCoin() Then
            x += randf.randf(0, x * MutationRate)
        Else
            x -= randf.randf(0, x * MutationRate)
        End If

        If Math.Abs(x) > truncate Then
            x = Math.Sign(x) * randf.seeds.NextDouble * truncate
        End If

        Return x
    End Function

    Public Function Mutate() As Genome Implements Chromosome(Of Genome).Mutate
        Dim clone As New Genome(Me.chromosome.Clone, MutationRate, truncate, rangePositive)
        Dim chromosome = clone.chromosome
        ' dim(A) is equals to dim(C) and is equals to dim(X)
        Dim i As Integer

        If FlipCoin() Then
            ' mutate one bit in A vector
            ' A only have -1, 0, 1
            chromosome.A.Array.Mutate(randf.seeds, rate:=MutationRate)
            chromosome.A.Truncate(limits:=truncate)
        End If

        If FlipCoin() Then
            chromosome.AC = valueMutate(chromosome.AC)
        End If

        'If FlipCoin() Then
        '    chromosome.Amplify = valueMutate(chromosome.Amplify)
        'End If

        'If FlipCoin() Then
        '    chromosome.delay = valueMutate(chromosome.delay)
        'End If

        For j As Integer = 0 To chromosome.C.Length - 1
            If FlipCoin() Then
                ' mutate one bit in C vector
                chromosome.C(j).B.Array.Mutate(randf.seeds, rate:=MutationRate)
                chromosome.C(j).B.Truncate(limits:=truncate)
            End If
        Next

        'If FlipCoin() Then
        '    chromosome.K = valueMutate(chromosome.K)
        'End If

        'For j As Integer = 0 To chromosome.C.Length - 1
        '    If FlipCoin() Then
        '        ' mutate one bit in C vector
        '        chromosome.C(j).B.Array.Mutate(randf.seeds, rate:=MutationRate)
        '        chromosome.C(j).B.Truncate(limits:=truncate)
        '    End If
        'Next

        If FlipCoin() Then
            i = randf.NextInteger(upper:=width)

            If chromosome.C(i).BC = 0 Then
                ' BC为负数的时候可能会出现0^-c = Inf的问题
                ' 所以还是正实数会比较好
                chromosome.C(i).BC = 0.001
            ElseIf FlipCoin() Then
                chromosome.C(i).BC += randf.randf(0, chromosome.C(i).BC * MutationRate)
            Else
                chromosome.C(i).BC -= randf.randf(0, chromosome.C(i).BC * MutationRate)
            End If

            If Math.Abs(chromosome.C(i).BC) > truncate Then
                chromosome.C(i).BC = Math.Sign(chromosome.C(i).BC) * randf.seeds.NextDouble * truncate
            End If
        End If

        Return clone
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        Return ToString(chromosome)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overloads Shared Function ToString(chromosome As GridSystem) As String
        Return chromosome.A.Length _
            .SeqIterator _
            .Select(Function(i)
                        Dim sign = chromosome.A(i)
                        Dim c = chromosome.C(i).B.Sum + chromosome.C(i).BC
                        Dim S = $"{chromosome.delay} ({chromosome.AC} + {sign} * {c}) * {chromosome.Amplify}"

                        Return S
                    End Function) _
            .ToArray _
            .GetJson _
            .MD5
    End Function
End Class
