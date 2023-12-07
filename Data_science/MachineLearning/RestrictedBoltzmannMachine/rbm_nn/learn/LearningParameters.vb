

Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math
Imports Microsoft.VisualBasic.MachineLearning.RestrictedBoltzmannMachine.math.functions

Namespace nn.rbm.learn

    ''' <summary>
    ''' Created by kenny on 5/15/14.
    ''' </summary>
    Public Class LearningParameters

        Private learningRateField As Double = 0.1

        Private logisticsFunctionField As DoubleFunction = New Sigmoid()

        Private epochsField As Integer = 15000

        Private logField As Boolean = True

        Private memoryField As Integer = 1

        Public ReadOnly Property LearningRate As Double
            Get
                Return learningRateField
            End Get
        End Property

        Public Function setLearningRate(learningRate As Double) As LearningParameters
            learningRateField = learningRate
            Return Me
        End Function

        Public ReadOnly Property LogisticsFunction As DoubleFunction
            Get
                Return logisticsFunctionField
            End Get
        End Property

        Public Function setLogisticsFunction(logisticsFunction As DoubleFunction) As LearningParameters
            logisticsFunctionField = logisticsFunction
            Return Me
        End Function

        Public ReadOnly Property Epochs As Integer
            Get
                Return epochsField
            End Get
        End Property

        Public Function setEpochs(epochs As Integer) As LearningParameters
            epochsField = epochs
            Return Me
        End Function


        Public ReadOnly Property Log As Boolean
            Get
                Return logField
            End Get
        End Property

        Public Function setLog(log As Boolean) As LearningParameters
            logField = log
            Return Me
        End Function

        Public ReadOnly Property Memory As Integer
            Get
                Return memoryField
            End Get
        End Property

        Public Function setMemory(memory As Integer) As LearningParameters
            memoryField = memory
            Return Me
        End Function

        Public Overrides Function ToString() As String
            Return "LearningParameters{" & "learningRate=" & learningRateField.ToString() & ", logisticsFunction=" & logisticsFunctionField.ToString() & ", epochs=" & epochsField.ToString() & ", log=" & logField.ToString() & ", memory=" & memoryField.ToString() & "}"c.ToString()
        End Function

    End Class

End Namespace
