Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class Environment : Implements Fitness(Of Genome)

    Dim matrix As (status As Vector, target As Double)()

    Public ReadOnly Property Cacheable As Boolean Implements Fitness(Of Genome).Cacheable
        Get
            Return False
        End Get
    End Property

    Sub New(trainingSet As IEnumerable(Of Sample))
        matrix = trainingSet _
            .Select(Function(sample)
                        Return (sample.status.vector.AsVector, sample.target(Scan0))
                    End Function) _
            .ToArray
    End Sub

    Public Function Calculate(chromosome As Genome) As Double Implements Fitness(Of Genome).Calculate
        Return matrix _
            .Select(Function(sample)
                        Return chromosome.CalculateError(sample.status, sample.target)
                    End Function) _
            .AverageError
    End Function
End Class
