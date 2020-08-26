Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF
Imports Microsoft.VisualBasic.MachineLearning.StoreProcedure

Namespace NeuralNetwork.DarwinismHybrid

    Public Class Fitness : Implements Fitness(Of NetworkIndividual)

        Dim dataSets As TrainingSample()

        Sub New(trainingSet As Sample())
            Me.dataSets = trainingSet.Select(Function(a) New TrainingSample(a)).ToArray
        End Sub

        Public ReadOnly Property Cacheable As Boolean Implements Fitness(Of NetworkIndividual).Cacheable
            Get
                Return False
            End Get
        End Property

        Public Function Calculate(chromosome As NetworkIndividual, parallel As Boolean) As Double Implements Fitness(Of NetworkIndividual).Calculate
            Dim errors As Double() = ANNTrainer.trainingImpl(
                network:=chromosome.target,
                dataSets:=dataSets,
                parallel:=parallel,
                selective:=True,
                dropoutRate:=0,
                backPropagate:=False
            )

            Return errors.Average
        End Function
    End Class
End Namespace