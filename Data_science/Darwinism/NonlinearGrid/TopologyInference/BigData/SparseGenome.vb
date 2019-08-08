Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.NonlinearGridTopology
Imports Microsoft.VisualBasic.Math
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

Namespace BigData

    Public Class SparseGenome : Inherits GridGenome(Of SparseGridSystem)
        Implements Chromosome(Of SparseGenome)

        Public Overrides Property MutationRate As Double Implements Chromosome(Of SparseGenome).MutationRate

        Public Sub New(chr As SparseGridSystem, mutationRate As Double, truncate As Double, rangePositive As Boolean)
            Call MyBase.New(chr, mutationRate, truncate, rangePositive)

            Me.MutationRate = mutationRate
        End Sub

        Public Iterator Function Crossover(another As SparseGenome) As IEnumerable(Of SparseGenome) Implements Chromosome(Of SparseGenome).Crossover
            Dim a = Me.chromosome.Clone
            Dim b = another.chromosome.Clone

            SyncLock randf.seeds
                If FlipCoin(CrossOverRate) Then
                    ' crossover A
                    randf.seeds.Crossover(a.A, b.A)
                End If

                If FlipCoin(CrossOverRate) Then
                    ' dim(A) is equals to dim(C) and is equals to dim(X)
                    Dim i As Integer = randf.NextInteger(upper:=width)
                    Dim j As Integer = randf.NextInteger(upper:=width)

                    ' If FlipCoin() Then
                    ' crossover C
                    randf.seeds.Crossover(a.C(i).B, b.C(j).B)
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

            Yield New SparseGenome(a, MutationRate, truncate, rangePositive)
            Yield New SparseGenome(b, MutationRate, truncate, rangePositive)
        End Function

        Public Function Mutate() As SparseGenome Implements Chromosome(Of SparseGenome).Mutate
            Dim clone As New SparseGenome(Me.chromosome.Clone, MutationRate, truncate, rangePositive)
            Dim chromosome = clone.chromosome
            ' dim(A) is equals to dim(C) and is equals to dim(X)
            Dim i As Integer

            If FlipCoin() Then
                ' mutate one bit in A vector
                ' A only have -1, 0, 1
                chromosome.A.Mutate(randf.seeds, rate:=MutationRate)
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
                    chromosome.C(j).B.Mutate(randf.seeds, rate:=MutationRate)
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

        Public Overrides Function ToString() As String
            Return chromosome.ToString
        End Function
    End Class
End Namespace