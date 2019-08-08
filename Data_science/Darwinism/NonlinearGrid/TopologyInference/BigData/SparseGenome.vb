Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Namespace BigData

    Public Class SparseGenome : Implements Chromosome(Of SparseGenome)

        Friend ReadOnly chromosome As SparseGridSystem

        ''' <summary>
        ''' Number of system variables.
        ''' </summary>
        ReadOnly width As Integer
        ''' <summary>
        ''' 约束变异所产生的值的上限
        ''' </summary>
        ReadOnly truncate As Double
        ReadOnly rangePositive As Boolean

        Public Property MutationRate As Double Implements Chromosome(Of SparseGenome).MutationRate

        Const CrossOverRate As Double = 30

        Public Sub New(chr As SparseGridSystem, mutationRate As Double, truncate As Double, rangePositive As Boolean)
            Me.chromosome = chr
            Me.width = chr.A.Dim
            Me.MutationRate = mutationRate
            Me.truncate = truncate
            Me.rangePositive = rangePositive
        End Sub

        ''' <summary>
        ''' <see cref="SparseGridSystem.Evaluate(Vector)"/>
        ''' </summary>
        ''' <param name="X"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Evaluate(X As Vector) As Double
            Return chromosome.Evaluate(X)
        End Function

        Public Function Crossover(another As SparseGenome) As IEnumerable(Of SparseGenome) Implements Chromosome(Of SparseGenome).Crossover
            Throw New NotImplementedException()
        End Function

        Public Function Mutate() As SparseGenome Implements Chromosome(Of SparseGenome).Mutate
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace