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
    ''' 突变程度
    ''' </summary>
    Public Property MutationRate As Double Implements Chromosome(Of Genome).MutationRate

    Sub New(chr As GridSystem, mutationRate As Double)
        Me.chromosome = chr
        Me.width = chr.A.Dim
        Me.MutationRate = mutationRate
    End Sub

    Public Function CalculateError(status As Vector, target As Double) As Double
        Dim predicts = chromosome.Evaluate(status)

        If predicts.IsNaNImaginary Then
            Return Double.MaxValue
        Else
            Return Math.Abs(predicts - target)
        End If
    End Function

    Public Iterator Function Crossover(another As Genome) As IEnumerable(Of Genome) Implements Chromosome(Of Genome).Crossover
        Dim a = Me.chromosome.Clone
        Dim b = another.chromosome.Clone

        SyncLock randf.seeds
            If FlipCoin() Then
                ' crossover A
                randf.seeds.Crossover(a.A.Array, b.A.Array)
            End If

            If FlipCoin() Then
                ' dim(A) is equals to dim(C) and is equals to dim(X)
                Dim i As Integer = randf.NextInteger(upper:=width)
                Dim j As Integer = randf.NextInteger(upper:=width)

                ' If FlipCoin() Then
                ' crossover C
                randf.seeds.Crossover(a.C(i).B.Array, b.C(j).B.Array)
            End If
            'Else
            '    ' crossover P
            '    randf.seeds.Crossover(a.P(i).W.Array, b.P(j).W.Array)
            'End If
            ' End If
        End SyncLock

        Yield New Genome(a, MutationRate)
        Yield New Genome(b, MutationRate)
    End Function

    Public Function Mutate() As Genome Implements Chromosome(Of Genome).Mutate
        Dim clone As New Genome(Me.chromosome.Clone, MutationRate)
        Dim chromosome = clone.chromosome
        ' dim(A) is equals to dim(C) and is equals to dim(X)
        Dim i As Integer

        If FlipCoin() Then
            ' mutate one bit in A vector
            ' A only have -1, 0, 1
            chromosome.A.Array.Mutate(randf.seeds, rate:=MutationRate)
            ' ElseIf FlipCoin(50) Then
        End If

        If FlipCoin() Then
            If chromosome.AC = 0 Then
                chromosome.AC = 1
            ElseIf FlipCoin() Then
                chromosome.AC += randf.randf(0, chromosome.AC * MutationRate)
            Else
                chromosome.AC -= randf.randf(0, chromosome.AC * MutationRate)
            End If
        End If

        If FlipCoin() Then
            i = randf.NextInteger(upper:=width)
            ' mutate one bit in C vector
            chromosome.C(i).B.Array.Mutate(randf.seeds, rate:=MutationRate)
            ' mutate one bit in P vector
            ' chromosome.P(i).W.Array.Mutate(randf.seeds)
        End If

        If FlipCoin() Then
            i = randf.NextInteger(upper:=width)

            If chromosome.C(i).BC = 0 Then
                chromosome.C(i).BC = 1
            ElseIf FlipCoin() Then
                chromosome.C(i).BC += randf.randf(0, chromosome.C(i).BC * MutationRate)
            Else
                chromosome.C(i).BC -= randf.randf(0, chromosome.C(i).BC * MutationRate)
            End If
        End If

        Return clone
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Overrides Function ToString() As String
        Return width _
            .SeqIterator _
            .Select(Function(i)
                        Dim sign = chromosome.A(i)
                        Dim c = chromosome.C(i).B.Sum + chromosome.C(i).BC
                        ' Dim p = chromosome.P(i).W.Sum

                        Return chromosome.AC + sign * (c) '+ p)
                    End Function) _
            .ToArray _
            .GetJson _
            .MD5
    End Function
End Class
