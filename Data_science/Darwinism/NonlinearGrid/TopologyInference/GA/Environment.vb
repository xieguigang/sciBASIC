Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.StoreProcedure
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class Environment : Implements Fitness(Of Genome)

    Dim matrix As (status As Vector, target As Double, targetID$)()

    Public ReadOnly Property Cacheable As Boolean Implements Fitness(Of Genome).Cacheable
        Get
            Return False
        End Get
    End Property

    Sub New(trainingSet As IEnumerable(Of Sample))
        matrix = trainingSet _
            .Select(Function(sample)
                        Return (
                            sample.status.vector.AsVector,
                            sample.target(Scan0),
                            sample.target(Scan0).ToString
                        )
                    End Function) _
            .ToArray
    End Sub

    Public Function Calculate(chromosome As Genome) As Double Implements Fitness(Of Genome).Calculate
        ' 理论上是应该使用MAX err来作为fitness的
        ' 但是在最开始的时候,因为整个系统的大部分样本的计算结果误差都是Inf
        ' 所以使用MAX来作为fitness的话,会因为结果都是Inf而导致前期没有办法收敛
        ' 在这里应该是使用平均值来避免这个问题
        Return matrix _
            .Select(Function(sample)
                        Dim err = chromosome.CalculateError(sample.status, sample.target)

                        Return (errors:=err, id:=sample.targetID)
                    End Function) _
            .GroupBy(Function(g) g.id) _
            .Select(Function(g) g.Select(Function(s) s.errors).AverageError) _
            .Average
    End Function
End Class
