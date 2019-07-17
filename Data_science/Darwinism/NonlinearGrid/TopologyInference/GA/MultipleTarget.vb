Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models
Imports Microsoft.VisualBasic.Math.LinearAlgebra

''' <summary>
''' 使用这个模型来对多个目标进行拟合，不过可能会精度较低
''' </summary>
Public Class MultipleTarget : Implements Chromosome(Of MultipleTarget)

    Public Property MutationRate As Double Implements Chromosome(Of MultipleTarget).MutationRate

    ReadOnly genome As Genome

    Sub New(genome As Genome, mutationRate As Double)
        Me.genome = genome
        Me.MutationRate = mutationRate
        Me.genome.MutationRate = mutationRate
    End Sub

    Public Function CalculateError(status As Vector, target As Double()) As Double

    End Function

    Public Iterator Function Crossover(another As MultipleTarget) As IEnumerable(Of MultipleTarget) Implements Chromosome(Of MultipleTarget).Crossover
        For Each genome As Genome In Me.genome.Crossover(another.genome)
            Yield New MultipleTarget(genome, MutationRate)
        Next
    End Function

    Public Function Mutate() As MultipleTarget Implements Chromosome(Of MultipleTarget).Mutate
        Return New MultipleTarget(genome.Mutate, MutationRate)
    End Function

    Public Overrides Function ToString() As String
        Return genome.ToString
    End Function
End Class
